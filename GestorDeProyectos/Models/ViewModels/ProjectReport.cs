namespace GestorDeProyectos.Models.ViewModels
{
    public class ProjectReport
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public decimal TotalHours { get; set; }
        public DateTime CompletionDate { get; set; }
        public List<StatusUpdateInfo> StatusUpdates { get; set; } = new List<StatusUpdateInfo>();
    }

    public class StatusUpdateInfo  
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTime UpdateDate { get; set; }
        public decimal HoursWorked { get; set; }
        public string? Comments { get; set; }
    }
}