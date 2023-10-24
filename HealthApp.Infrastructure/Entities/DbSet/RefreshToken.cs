using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthApp.Infrastructure.Entities.DbSet
{
    public class RefreshToken:BaseEntity
    {
        public string UserId{ get; set; }
        public string Token { get; set; }
        public string JwtId { get; set; } //the jti when token is created
        public bool IsUsed { get; set; }
        public bool IsRevoked { get; set; }
        public bool IsExpired { get; set; }
        public DateTime ExpiryDate { get; set; }

        [ForeignKey(nameof(UserId))]
        public IdentityUser User { get; set; }
    }
}
