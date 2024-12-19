namespace GestorDeProyectos.Models.ViewModels
{
    public class ProjectAssignmentViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public List<UserAssignmentViewModel> AvailableUsers { get; set; } = new List<UserAssignmentViewModel>();
    }

    public class UserAssignmentViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string JobRole { get; set; } = string.Empty;
        public bool IsAssigned { get; set; }
    }
}
