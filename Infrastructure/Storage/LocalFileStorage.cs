using Application.DTO.Requests.Comments.Create;
using Application.DTO.Responses;
using Application.Interfaces;
using Domain.Common;
using Domain.Constants;
using Domain.Enums;
using Microsoft.Extensions.Options;
using SkiaSharp;
using System.Text;

namespace Infrastructure.Storage;

public sealed class LocalFileStorage(IOptions<FileStorageOptions> options) : IFileStorageService
{
    private const int MaxImageSidePx = 8000;
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private readonly string _root = Path.GetFullPath(options.Value.UploadsPath);
    private readonly Dictionary<SKEncodedImageFormat, OutputFormat> Formats = new()
    {
        [SKEncodedImageFormat.Jpeg] = new(SKEncodedImageFormat.Jpeg, 85, "jpg", "image/jpeg"),
        [SKEncodedImageFormat.Png] = new(SKEncodedImageFormat.Png, 100, "png", "image/png"),
        [SKEncodedImageFormat.Gif] = new(SKEncodedImageFormat.Png, 100, "png", "image/png"), //skiasharp does not support gif encoding, so we convert to png
    };

    public async Task<Result<StoredFileResponse>> SaveAsync(FileUploadRequest file, CancellationToken ct)
    {
        await using var content = file.Content;

        if (file.Length is <= 0 || file.Length > AttachmentLimits.UploadMaxBytes)
        {
            return Fail("File.InvalidSize", "File is empty or too large");
        }

        await using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, ct);
        buffer.Position = 0;

        if (buffer.Length > AttachmentLimits.UploadMaxBytes)
        {
            return Fail("File.InvalidSize", "File is too large");
        }    

        Directory.CreateDirectory(_root);

        var isText = Path.GetExtension(file.FileName)
            .Equals(".txt", StringComparison.OrdinalIgnoreCase);

        return isText
            ? await SaveDocumentAsync(buffer.ToArray(), ct)
            : await SaveImageAsync(buffer, ct);
    }

    public void Delete(string storedFileName)
    {
        var path = Path.Combine(_root, Path.GetFileName(storedFileName));
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private async Task<Result<StoredFileResponse>> SaveDocumentAsync(byte[] bytes, CancellationToken ct)
    {
        if (bytes.Length > AttachmentLimits.TextMaxBytes)
        {
            return Fail("File.TextTooLarge", "Text file must be 100 KB or less");
        }

        try
        {
            var text = StrictUtf8.GetString(bytes);
            if (text.Contains('\0')){
                return Fail("File.NotText", "File is not a text file");
            }
        }
        catch (DecoderFallbackException)
        {
            return Fail("File.NotText", "File is not a valid UTF-8 text file");
        }

        var storedName = $"{Guid.CreateVersion7()}.txt";
        await File.WriteAllBytesAsync(Path.Combine(_root, storedName), bytes, ct);

        return Result.Success(new StoredFileResponse(AttachmentType.Document, storedName, "text/plain", bytes.Length));
    }

    private async Task<Result<StoredFileResponse>> SaveImageAsync(MemoryStream buffer, CancellationToken ct)
    {
        using var codec = SKCodec.Create(buffer);
        var isAllowedFormat = codec?.EncodedFormat is not null && Formats.ContainsKey(codec.EncodedFormat);
        if (codec is null || !isAllowedFormat)
        {
            return Fail("File.UnsupportedFormat", "Allowed formats: JPG, GIF, PNG, TXT");
        }
               
        if (codec.Info.Width > MaxImageSidePx || codec.Info.Height > MaxImageSidePx)
        {
            return Fail("File.ImageTooBig", "Image dimensions are too large");
        }

        using var decoded = SKBitmap.Decode(codec);
        if (decoded is null)
        {
            return Fail("File.UnsupportedFormat", "Image could not be read");
        }

        using var oriented = Orient(decoded, codec.EncodedOrigin);

        var widthScale = (float)AttachmentLimits.ImageMaxWidth / oriented.Width;
        var heightScale = (float)AttachmentLimits.ImageMaxHeight / oriented.Height;

        var resizeScale = Math.Min(1f, Math.Min(widthScale, heightScale));

        var resized = oriented;

        if (resizeScale < 1f)
        {
            var newSize = new SKImageInfo(
                Math.Max(1, (int)Math.Round(oriented.Width * resizeScale)),
                Math.Max(1, (int)Math.Round(oriented.Height * resizeScale)));

            resized = oriented.Resize(newSize, SKFilterQuality.High);

            if(resized is null)
            {
                return Fail("File.ImageResizeFailed", "Image could not be resized");
            };
        }
        var format = Formats[codec.EncodedFormat];

        using var data = resized.Encode(format.Format, format.Quality);
        resized.Dispose();

        var storedName = $"{Guid.CreateVersion7()}.{format.Extension}";
        await File.WriteAllBytesAsync(Path.Combine(_root, storedName), data.ToArray(), ct);

        return Result.Success(new StoredFileResponse(
            AttachmentType.Image,
            storedName,
            format.ContentType,
            data.Size));
    }

    private static SKBitmap Orient(SKBitmap src, SKEncodedOrigin origin)
    {
        if (origin is not (SKEncodedOrigin.RightTop or SKEncodedOrigin.BottomRight or SKEncodedOrigin.LeftBottom))
        {
            return src.Copy();
        }    

        var swap = origin != SKEncodedOrigin.BottomRight;
        var result = new SKBitmap(swap ? src.Height : src.Width, swap ? src.Width : src.Height);
        using var canvas = new SKCanvas(result);

        switch (origin)
        {
            case SKEncodedOrigin.RightTop:
                canvas.Translate(result.Width, 0);
                canvas.RotateDegrees(90);
                break;
            case SKEncodedOrigin.BottomRight:
                canvas.Translate(result.Width, result.Height);
                canvas.RotateDegrees(180);
                break;
            case SKEncodedOrigin.LeftBottom:
                canvas.Translate(0, result.Height);
                canvas.RotateDegrees(270);
                break;
        }

        canvas.DrawBitmap(src, 0, 0);
        return result;
    }

    private static Result<StoredFileResponse> Fail(string code, string message) =>
        Result.Failure<StoredFileResponse>(Error.Validation(code, message));
}