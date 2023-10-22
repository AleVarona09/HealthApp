using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthApp.Authentication.Models.Dtos.Response
{
    public class AuthResult
    {
        public string Token { get; set; }
        public bool Success { get; set; }
        public List<String> Errors { get; set; }

    }
}
