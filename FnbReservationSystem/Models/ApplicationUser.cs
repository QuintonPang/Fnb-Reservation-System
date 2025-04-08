using Microsoft.AspNetCore.Identity;

namespace FnbReservationSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}
