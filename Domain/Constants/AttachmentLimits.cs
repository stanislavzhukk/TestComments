using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Constants
{
    public static class AttachmentLimits
    {
        public const int ImageMaxWidth = 320;
        public const int ImageMaxHeight = 240;
        public const int TextMaxBytes = 100 * 1024;
        public const int UploadMaxBytes = 5 * 1024 * 1024;
    }
}
