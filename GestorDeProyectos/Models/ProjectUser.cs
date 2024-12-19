using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestorDeProyectos.Models
{
    public class ProjectUser
    {
        [Required]
        public int ProjectId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public DateTime AssignedDate { get; set; }
        public bool IsActive { get; set; } = true;

        [ForeignKey(nameof(ProjectId))]
        public virtual Project Project { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; } = null!;
    }
}