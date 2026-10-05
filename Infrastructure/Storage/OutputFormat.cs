using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Storage
{
    internal sealed record OutputFormat(
        SKEncodedImageFormat Format,
        int Quality,
        string Extension,
        string ContentType);
}
