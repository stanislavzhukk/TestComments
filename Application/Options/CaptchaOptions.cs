using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Options
{
    public sealed class CaptchaOptions
    {
        public const string SectionName = "Captcha";

        public int CodeLength { get; init; } = 5;
        public int TtlMinutes { get; init; } = 5;
        public int Width { get; init; } = 200;
        public int Height { get; init; } = 60;
        public string FontFamily { get; init; } = "DejaVu Sans";
        public float FontSize { get; init; } = 32;
    }
}
