using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.DiscountCodeDtos
{
    public class DiscountCodeCreateDto
    {
        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [Range(1, 100)]
        public decimal DiscountPercentage { get; set; }

        [Required]
        public DateTime ValidFrom { get; set; }

        [Required]
        public DateTime ValidUntil { get; set; }

        public int? MaxUses { get; set; }
    }
}
