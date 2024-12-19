using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GestorDeProyectos.Data;
using GestorDeProyectos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using GestorDeProyectos.Models.ViewModels;

namespace GestorDeProyectos.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProjectsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            var isAdmin = await _userManager.IsInRoleAsync(currentUser, UserRoles.Admin);
            IQueryable<Project> projectsQuery;

            if (isAdmin)
            {
                projectsQuery = _context.Projects.Include(p => p.User);
            }
            else
            {
                projectsQuery = _context.Projects
                    .Include(p => p.ProjectUsers)
                    .Where(p => p.ProjectUsers.Any(pu => pu.UserId == currentUser.Id));
            }

            var projects = await projectsQuery.ToListAsync();
            return View(projects);
        }

        // GET: Projects/Assign/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Assign(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.ProjectUsers)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            var allUsers = await _userManager.GetUsersInRoleAsync(UserRoles.User);

            var viewModel = new ProjectAssignmentViewModel
            {
                ProjectId = project.Id,
                ProjectName = project.Name ?? string.Empty,
                AvailableUsers = allUsers.Select(u => new UserAssignmentViewModel
                {
                    UserId = u.Id ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    FirstName = u.FirstName ?? string.Empty,
                    LastName = u.LastName ?? string.Empty,
                    JobRole = u.JobRole ?? string.Empty,
                    IsAssigned = project.ProjectUsers?.Any(pu => pu.UserId == u.Id) ?? false
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: Projects/Assign/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(int id, List<string>? selectedUsers)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectUsers)
                    .ThenInclude(pu => pu.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            try
            {
                var currentAssignments = project.ProjectUsers.ToList();

                if (selectedUsers != null)
                {
                    
                    foreach (var assignment in currentAssignments)
                    {
                        if (!selectedUsers.Contains(assignment.UserId))
                        {
                            assignment.IsActive = false;
                            var userEmail = assignment.User?.Email;
                            if (!string.IsNullOrEmpty(userEmail))
                            {
                                await AddToProjectLog(project.Id, $"Usuario desasignado: {userEmail}", "Asignación");
                            }
                        }
                    }

                    
                    foreach (var userId in selectedUsers)
                    {
                        var existingAssignment = currentAssignments
                            .FirstOrDefault(a => a.UserId == userId);

                        if (existingAssignment == null)
                        {
                            var user = await _userManager.FindByIdAsync(userId);
                            if (user != null)
                            {
                                project.ProjectUsers.Add(new ProjectUser
                                {
                                    ProjectId = project.Id,
                                    UserId = userId,
                                    AssignedDate = DateTime.Now,
                                    IsActive = true
                                });

                                if (!string.IsNullOrEmpty(user.Email))
                                {
                                    await AddToProjectLog(project.Id, $"Usuario asignado: {user.Email}", "Asignación");
                                }
                            }
                        }
                        else if (!existingAssignment.IsActive)
                        {
                            existingAssignment.IsActive = true;
                            var userEmail = existingAssignment.User?.Email;
                            if (!string.IsNullOrEmpty(userEmail))
                            {
                                await AddToProjectLog(project.Id, $"Usuario reasignado: {userEmail}", "Asignación");
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Usuarios asignados exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ocurrió un error al asignar los usuarios.";
                
            }

            return RedirectToAction(nameof(Index));
        }


        private async Task AddToProjectLog(int projectId, string description, string action)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            _context.ProjectLogs.Add(new ProjectLog
            {
                ProjectId = projectId,
                Action = action,
                Description = description,
                CreatedBy = currentUser?.UserName ?? "Sistema",
                CreatedAt = DateTime.Now
            });
        }

        // GET: Projects/UpdateStatus/5
        [Authorize(Roles = "User")]
        public async Task<IActionResult> UpdateStatus(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound("Usuario no encontrado");
            }

            var project = await _context.Projects
                .Include(p => p.ProjectUsers)
                .FirstOrDefaultAsync(p => p.Id == id &&
                    p.ProjectUsers.Any(pu => pu.UserId == currentUser.Id));

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/UpdateStatus/5
        [HttpPost]
        [Authorize(Roles = "User")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status, decimal hoursWorked, string? comments)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            var statusUpdate = new StatusUpdate
            {
                ProjectId = id,
                Status = status,
                UpdatedBy = currentUser.UserName ?? currentUser.Email ?? "Usuario",
                UpdateDate = DateTime.Now,
                HoursWorked = hoursWorked,
                Comments = comments ?? string.Empty
            };

            project.Status = status;
            project.LastStatusUpdate = DateTime.Now;
            project.LastUpdateBy = currentUser.UserName ?? currentUser.Email;
            project.TotalHours += hoursWorked;

            _context.StatusUpdates.Add(statusUpdate);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Estado actualizado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Projects/Report/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Report(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.ProjectUsers)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (project == null || project.Status != "Completado")
            {
                return NotFound();
            }

            // Obtener las actualizaciones y mapearlas al tipo correcto
            var statusUpdates = await _context.StatusUpdates
                .Where(su => su.ProjectId == id)
                .OrderByDescending(su => su.UpdateDate)
                .Select(su => new StatusUpdateInfo
                {
                    Id = su.Id,
                    Status = su.Status,
                    UpdatedBy = su.UpdatedBy,
                    UpdateDate = su.UpdateDate,
                    HoursWorked = su.HoursWorked,
                    Comments = su.Comments
                })
                .ToListAsync();

            var report = new ProjectReport
            {
                ProjectId = project.Id,
                ProjectName = project.Name,
                TotalHours = project.TotalHours,
                CompletionDate = project.LastStatusUpdate ?? project.EndDate,
                StatusUpdates = statusUpdates
            };

            return View(report);
        }



        // GET: Projects/Create
        public IActionResult Create()
        {
            
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,StartDate,EndDate,Status,UserId")] Project project)
        {
            if (project.EndDate <= project.StartDate)
            {
                ModelState.AddModelError("EndDate", "La fecha de fin debe ser posterior a la fecha de inicio");
            }

            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return NotFound("Usuario no encontrado");
                }

                project.UserId = currentUser.Id;
                project.Status = "Pendiente"; 
                _context.Add(project);
                await _context.SaveChangesAsync();

                await LogProjectAction(project.Id, "Crear", "Proyecto creado");
                TempData["Success"] = "Proyecto creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            return View(project);
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, UserRoles.Admin);

            var project = await _context.Projects
                .Include(p => p.User)
                .Include(p => p.ProjectUsers)
                .FirstOrDefaultAsync(p => p.Id == id &&
                    (isAdmin || p.ProjectUsers.Any(pu => pu.UserId == currentUser.Id)));

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,StartDate,EndDate,Status,UserId")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var existingProject = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existingProject == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Mantener el UserId original
                    project.UserId = existingProject.UserId;

                    _context.Entry(existingProject).CurrentValues.SetValues(project);
                    await _context.SaveChangesAsync();

                    // Registrar la acción
                    await LogProjectAction(project.Id, "Editar", "Proyecto actualizado");
                    TempData["Success"] = "Proyecto actualizado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(project);
        }


        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, UserRoles.Admin);

            var project = await _context.Projects
                .Include(p => p.User)
                .Include(p => p.ProjectUsers)
                    .ThenInclude(pu => pu.User)
                .FirstOrDefaultAsync(m => m.Id == id &&
                    (isAdmin || m.ProjectUsers.Any(pu => pu.UserId == currentUser.Id)));

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, UserRoles.Admin);

            var project = await _context.Projects
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id && (isAdmin || m.UserId == currentUser.Id));

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Projects == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Projects' is null.");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }

            var isAdmin = await _userManager.IsInRoleAsync(currentUser, UserRoles.Admin);
            var project = await _context.Projects
                .Include(p => p.ProjectUsers)
                .FirstOrDefaultAsync(p => p.Id == id && (isAdmin || p.UserId == currentUser.Id));

            if (project == null)
            {
                return NotFound();
            }

            try
            {
                
                var logs = await _context.ProjectLogs.Where(l => l.ProjectId == id).ToListAsync();
                _context.ProjectLogs.RemoveRange(logs);

                var statusUpdates = await _context.StatusUpdates.Where(s => s.ProjectId == id).ToListAsync();
                _context.StatusUpdates.RemoveRange(statusUpdates);

                
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Proyecto eliminado exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar el proyecto.";
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }


        private async Task LogProjectAction(int projectId, string action, string description)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            _context.ProjectLogs.Add(new ProjectLog
            {
                ProjectId = projectId,
                Action = action,
                Description = description,
                CreatedBy = currentUser?.UserName ?? "Sistema",
                CreatedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();
        }

        // GET: Projects/ViewLog/5
        public async Task<IActionResult> ViewLog(int id)
        {
            var logs = await _context.ProjectLogs
                .Where(l => l.ProjectId == id)
                .Include(l => l.Project)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            if (!logs.Any())
            {
                TempData["Info"] = "No hay registros de actividad para este proyecto.";
            }

            return View(logs);
        }
    }
}
