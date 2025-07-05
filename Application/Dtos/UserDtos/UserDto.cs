using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.UserDtos
{
    public class UserDto
    {
        public string Id { get; set; }
        
        [Required]
        public string Email { get; set; }
        
        [Required]
        public string FirstName { get; set; }
        
        [Required]
        public string LastName { get; set; }
            
        [Required]
        [Range(1, 3)]
        public string Grade { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
