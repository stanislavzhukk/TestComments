using Application.DTO.Requests.Captcha;
using Application.DTO.Responses;
using Application.Interfaces;
using Application.Options;
using Domain.Common;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkiaSharp;
using System.Security.Cryptography;

namespace Infrastructure.Services
{
    public class CaptchaService(
        IMemoryCache cache,
        ILogger<CaptchaService> logger, IOptions<CaptchaOptions> options) : ICaptchaService
    {
        private readonly CaptchaOptions _options = options.Value;
        private const string CacheCategoryPrefix = "captcha";
        private static string CacheKey(string id) => $"{CacheCategoryPrefix}:{id}";
        private static readonly char[] AllowedChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

        public Result<CaptchaResponse> GenerateCaptcha()
        {
            try
            {
                var captchaText = GenerateRandomCaptchaText();
                var captchaId = Guid.CreateVersion7();

                cache.Set(
                    CacheKey(captchaId.ToString()),
                    captchaText,
                    TimeSpan.FromMinutes(_options.TtlMinutes)
                );

                var imageBytes = DrawCaptchaImage(captchaText);
                var base64Image = Convert.ToBase64String(imageBytes);

                var response = new CaptchaResponse(
                    Image: $"data:image/png;base64,{base64Image}",
                    Id: captchaId);

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to generate CAPTCHA");

                return Result.Failure<CaptchaResponse>(
                    Error.Failure(
                        "Captcha.GenerationFailed",
                        "Failed to generate captcha."
                    )
                );
            }
        }

        public Result ValidateCaptcha(CheckCaptchaRequest request)
        {
            var key = CacheKey(request.CaptchaId.ToString());

            if (!cache.TryGetValue(key, out string? code))
            {
                return Result.Failure(Error.Validation("Captcha.ValidationError", "CAPTCHA has expired or does not exist."));
            }

            cache.Remove(key);

            return string.Equals(code, request.UserInput.Trim(), StringComparison.OrdinalIgnoreCase)
                ? Result.Success()
                : Result.Failure(Error.Validation("Captcha.ValidationError", "Invalid CAPTCHA."));
        }

        private byte[] DrawCaptchaImage(string captchaText)
        {
            var width = _options.Width;
            var height = _options.Height;

            var imageInfo = new SKImageInfo(
                width,
                height,
                SKColorType.Rgba8888);

            using var bitmap = new SKBitmap(imageInfo);
            using var canvas = new SKCanvas(bitmap);

            canvas.Clear(SKColors.White);

            AddNoise(canvas, width, height, false);

            using var paint = new SKPaint
            {
                IsAntialias = true,
                Color = SKColors.DeepSkyBlue,
            };

            using var typeface = SKTypeface.FromFamilyName(_options.FontFamily, SKFontStyle.Bold);
            using var font = new SKFont(typeface, _options.FontSize);

            using var textBlob = SKTextBlob.Create(captchaText, font);
            var textBounds = textBlob.Bounds;
            var textWidth = textBounds.Width;
            var textHeight = textBounds.Height;

            var textX = (width - textWidth) / 2f - textBounds.Left;
            var textY = (height + textHeight) / 2f - textBounds.Bottom;

            canvas.DrawText(
                captchaText,
                textX,
                textY,
                font,
                paint);

            AddNoise(canvas, width, height, true);

            using var data = bitmap.Encode(
                SKEncodedImageFormat.Png,
                100);

            return data.ToArray();
        }

        private void AddNoise(SKCanvas canvas, int width, int height, bool overlay)
        {
            var curveCount = overlay ? 3 : 4;
            var dotCount = overlay ? 40 : 120;

            using var curvePaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

            for (var i = 0; i < curveCount; i++)
            {
                curvePaint.Color = RandomColor(overlay ? 60 : 110, overlay ? 160 : 200);
                curvePaint.StrokeWidth = overlay
                    ? 1.5f + RandomNumberGenerator.GetInt32(0, 15) / 10f
                    : 1f + RandomNumberGenerator.GetInt32(0, 10) / 10f;

                using var path = new SKPath();
                path.MoveTo(0, RandomNumberGenerator.GetInt32(height));
                path.CubicTo(
                    RandomNumberGenerator.GetInt32(width), RandomNumberGenerator.GetInt32(height),
                    RandomNumberGenerator.GetInt32(width), RandomNumberGenerator.GetInt32(height),
                    width, RandomNumberGenerator.GetInt32(height));

                canvas.DrawPath(path, curvePaint);
            }

            using var dotPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            for (var i = 0; i < dotCount; i++)
            {
                dotPaint.Color = RandomColor(80, 200);
                canvas.DrawCircle(
                    RandomNumberGenerator.GetInt32(width),
                    RandomNumberGenerator.GetInt32(height),
                    0.6f + RandomNumberGenerator.GetInt32(0, 14) / 10f,
                    dotPaint);
            }
        }

        private static SKColor RandomColor(int min, int max)
        {
            return new SKColor(
                (byte)RandomNumberGenerator.GetInt32(min, max),
                (byte)RandomNumberGenerator.GetInt32(min, max),
                (byte)RandomNumberGenerator.GetInt32(min, max));
        }

        private string GenerateRandomCaptchaText()
        {
            var chars = new char[_options.CodeLength];
            for (var i = 0; i < chars.Length; i++)
            {
               chars[i] = AllowedChars[RandomNumberGenerator.GetInt32(AllowedChars.Length)];
            }
            return new string(chars);
        }
    }
}