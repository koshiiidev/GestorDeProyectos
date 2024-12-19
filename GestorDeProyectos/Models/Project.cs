using System.ComponentModel.DataAnnotations;

namespace GestorDeProyectos.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del proyecto es obligatorio")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
        [Display(Name = "Fecha de Inicio")]
        [DataType(DataType.Date)]
        [FutureOrPresentDate(ErrorMessage = "La fecha de inicio no puede ser anterior a hoy")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria")]
        [Display(Name = "Fecha de Fin")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        [ProjectEndDate(ErrorMessage = "La fecha de fin debe ser posterior a la fecha de inicio")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio")]
        [ValidateProjectStatus(ErrorMessage = "Estado no válido")]
        public string Status { get; set; } = "Pendiente";


        [Range(0, double.MaxValue, ErrorMessage = "Las horas totales no pueden ser negativas")]
        public decimal TotalHours { get; set; }
        public DateTime? LastStatusUpdate { get; set; }
        public string? LastUpdateBy { get; set; }

        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        public virtual ICollection<ProjectUser> ProjectUsers { get; set; } = new List<ProjectUser>();
    }

    
    public class FutureOrPresentDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime date)
            {
                if (date.Date < DateTime.Today)
                {
                    return new ValidationResult(ErrorMessage);
                }
            }
            return ValidationResult.Success;
        }
    }

    
    public class ProjectEndDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var project = (Project)validationContext.ObjectInstance;

            if (project.EndDate <= project.StartDate)
            {
                return new ValidationResult("La fecha de fin debe ser posterior a la fecha de inicio");
            }

            return ValidationResult.Success;
        }
    }

    
    public class ValidateProjectStatusAttribute : ValidationAttribute
    {
        private readonly string[] validStatuses = { "Pendiente", "En Progreso", "Completado" };

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string status)
            {
                if (!validStatuses.Contains(status))
                {
                    return new ValidationResult($"Estado no válido. Estados permitidos: {string.Join(", ", validStatuses)}");
                }

                var project = (Project)validationContext.ObjectInstance;

                
                if (status == "Completado" && project.TotalHours == 0)
                {
                    return new ValidationResult("No se puede marcar como completado un proyecto sin horas registradas");
                }
            }
            return ValidationResult.Success;
        }
    }

    
}
