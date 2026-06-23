using Microsoft.AspNetCore.Identity;

namespace ConsultorioDental.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? NombreCompleto { get; set; }
    }
}
