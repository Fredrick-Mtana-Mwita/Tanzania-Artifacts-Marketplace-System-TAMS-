using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Models
{
    [Table("AspNetUsers")]
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? ProfileImage { get; set; }
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public bool IsVerified { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public Roles Role { get; internal set; }
    }
}
