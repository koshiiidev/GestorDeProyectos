using System.ComponentModel.DataAnnotations;

namespace GestorDeProyectos.Models
{
    public class StatusUpdate
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        [Required]
        public string Status { get; set; } = string.Empty;

        [Required]
        public string UpdatedBy { get; set; } = string.Empty;

        public DateTime UpdateDate { get; set; }

        public decimal HoursWorked { get; set; }

        public string? Comments { get; set; }
    }
}
