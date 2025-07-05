using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class DiscountCode
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [Range(0, 100)]
        public decimal DiscountPercentage { get; set; }

        public DateTime ValidFrom { get; set; }

        public DateTime ValidUntil { get; set; }

        public int? MaxUses { get; set; }

        public int Uses { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public string TeacherId { get; set; }

        public ApplicationUser Teacher { get; set; }
    }
}
