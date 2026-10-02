using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Requests.Auth
{
    public class LogoutRequest
    {
        public required string RefreshToken { get; set; } = string.Empty;
    }
}
