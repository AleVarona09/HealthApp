using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthApp.Infrastructure.Dtos.Request
{
    public class ProfileUpdateDto
    {
        public string Country { get; set; }
        public string Sex { get; set; }
        public string MobilePhone { get; set; }
        public string Address { get; set; }
    }
}
