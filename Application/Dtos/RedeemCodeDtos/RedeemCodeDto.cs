using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.RedeemCodeDtos
{
    public class RedeemCodeDto
    {
        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;
    }
}
