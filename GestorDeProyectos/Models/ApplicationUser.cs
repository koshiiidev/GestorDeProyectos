using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace GestorDeProyectos.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [Display(Name = "Nombre")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Apellido")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Rol de trabajo")]
        public string JobRole { get; set; } = string.Empty;

        public virtual ICollection<Project> OwnedProjects { get; set; } = new List<Project>();
        public virtual ICollection<ProjectUser> AssignedProjects { get; set; } = new List<ProjectUser>();
    }

    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string User = "User";
    }


}
