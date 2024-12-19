using System.ComponentModel.DataAnnotations;

namespace GestorDeProyectos.Models
{
    public class ProjectLog
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        [Required]
        public string Action { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
