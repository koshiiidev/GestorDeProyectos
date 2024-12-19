namespace GestorDeProyectos.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalProjects { get; set; }
        public int ProjectsPending { get; set; }
        public int ProjectsInProgress { get; set; }
        public int ProjectsCompleted { get; set; }
        public decimal TotalHoursWorked { get; set; }
        public int TotalUsers { get; set; }
        public List<Project> RecentProjects { get; set; } = new List<Project>();
        public List<StatusUpdate> RecentUpdates { get; set; } = new List<StatusUpdate>();
    }
}