using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Responses
{
    public class RefreshResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public required UserDataResponse User { get; set; }
    }
}
