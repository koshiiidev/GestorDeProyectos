using GestorDeProyectos.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using GestorDeProyectos.Data;
using GestorDeProyectos.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GestorDeProyectos.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public HomeController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                var user = await _userManager.GetUserAsync(User);
                if (await _userManager.IsInRoleAsync(user!, "Admin"))
                {
                    
                    var dashboard = new DashboardViewModel
                    {
                        TotalProjects = _context.Projects.Count(),
                        ProjectsPending = _context.Projects.Count(p => p.Status == "Pendiente"),
                        ProjectsInProgress = _context.Projects.Count(p => p.Status == "En Progreso"),
                        ProjectsCompleted = _context.Projects.Count(p => p.Status == "Completado"),
                        TotalHoursWorked = _context.Projects.Sum(p => p.TotalHours),
                        TotalUsers = (await _userManager.GetUsersInRoleAsync("User")).Count,
                        RecentProjects = await _context.Projects
                            .OrderByDescending(p => p.LastStatusUpdate)
                            .Take(5)
                            .ToListAsync(),
                        RecentUpdates = await _context.StatusUpdates
                            .Include(u => u.Project)
                            .OrderByDescending(u => u.UpdateDate)
                            .Take(10)
                            .ToListAsync()
                    };

                    return View("Dashboard", dashboard);
                }
                return View();
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
