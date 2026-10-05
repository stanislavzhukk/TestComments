using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTO.Responses
{
    public sealed record AttachmentResponse(AttachmentType Type, string Url, string FileName);
}
