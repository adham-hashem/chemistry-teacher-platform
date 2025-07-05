using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.DiscountCodeDtos
{
    public class DiscountCodeDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal DiscountPercentage { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidUntil { get; set; }
        public int? MaxUses { get; set; }
        public int Uses { get; set; }
        public bool IsActive { get; set; }
        public string TeacherId { get; set; }
    }
}
