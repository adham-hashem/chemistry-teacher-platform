using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public bool IsRevoked { get; set; }
        public ApplicationUser User { get; set; }
    }
}
