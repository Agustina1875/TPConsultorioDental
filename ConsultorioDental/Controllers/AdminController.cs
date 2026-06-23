using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ConsultorioDental.Models;
using ConsultorioDental.Data;
using Microsoft.EntityFrameworkCore;

namespace ConsultorioDental.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        //muestra el resumen general del panel admin
        public async Task<IActionResult> Index()
        {
            ViewBag.TotalPacientes = await _context.Pacientes.CountAsync();
            ViewBag.TotalOdontologos = await _context.Odontologos.CountAsync();
            ViewBag.TotalTurnos = await _context.Turnos.CountAsync();
            return View();
        }

        //lista los usuarios con sus roles para administrarlos
        public async Task<IActionResult> Usuarios()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRoles = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles.Add(new UserRoleViewModel
                {
                    UserId = user.Id,
                    Email = user.Email ?? "",
                    NombreCompleto = user.NombreCompleto ?? "",
                    Roles = roles.ToList()
                });
            }

            ViewBag.Roles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
            return View(userRoles);
        }

        //asigna un rol al usuario y sincroniza su perfil de dominio
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarRol(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                var quitarRoles = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!quitarRoles.Succeeded)
                {
                    TempData["Error"] = string.Join(" | ", quitarRoles.Errors.Select(e => e.Description));
                    return RedirectToAction(nameof(Usuarios));
                }
            }

            if (!string.IsNullOrWhiteSpace(role) && role != "Sin rol")
            {
                var agregarRol = await _userManager.AddToRoleAsync(user, role);
                if (!agregarRol.Succeeded)
                {
                    TempData["Error"] = string.Join(" | ", agregarRol.Errors.Select(e => e.Description));
                    return RedirectToAction(nameof(Usuarios));
                }

                await SincronizarPerfilesSegunRolAsync(user, role);
            }

            TempData["Mensaje"] = "Rol actualizado correctamente.";
            return RedirectToAction(nameof(Usuarios));
        }

        //si cambia el rol desde admin, crea el perfil minimo y elimina el del otro rol para no dejar datos cruzados
        private async Task SincronizarPerfilesSegunRolAsync(ApplicationUser user, string role)
        {
            if (role == "Paciente")
            {
                await EliminarOdontologoPorEmailAsync(user.Email);
                await CrearPacienteSiNoExisteAsync(user);
            }
            else if (role == "Odontologo")
            {
                await EliminarPacientePorEmailAsync(user.Email);
                await CrearOdontologoSiNoExisteAsync(user);
            }
        }

        private async Task CrearPacienteSiNoExisteAsync(ApplicationUser user)
        {
            var email = user.Email ?? "";
            if (string.IsNullOrWhiteSpace(email))
                return;

            var existePaciente = await _context.Pacientes.AnyAsync(p => p.Email == email);
            if (existePaciente)
                return;

            var (nombre, apellido) = SepararNombre(user.NombreCompleto);
            _context.Pacientes.Add(new Paciente
            {
                Nombre = nombre,
                Apellido = apellido,
                Email = email,
                DNI = "00000000",
                Telefono = "0000000000",
                FechaNacimiento = new DateTime(2000, 1, 1)
            });
            await _context.SaveChangesAsync();
        }

        private async Task CrearOdontologoSiNoExisteAsync(ApplicationUser user)
        {
            var email = user.Email ?? "";
            if (string.IsNullOrWhiteSpace(email))
                return;

            var existeOdontologo = await _context.Odontologos.AnyAsync(o => o.Email == email);
            if (existeOdontologo)
                return;

            var (nombre, apellido) = SepararNombre(user.NombreCompleto);
            _context.Odontologos.Add(new Odontologo
            {
                Nombre = nombre,
                Apellido = apellido,
                Email = email,
                Telefono = "0000000000",
                Especialidad = "Sin definir"
            });
            await _context.SaveChangesAsync();
        }

        private async Task EliminarPacientePorEmailAsync(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return;

            var paciente = await _context.Pacientes.FirstOrDefaultAsync(p => p.Email == email);
            if (paciente == null)
                return;

            var tieneTurnos = await _context.Turnos.AnyAsync(t => t.PacienteId == paciente.Id);
            if (tieneTurnos)
                return;

            _context.Pacientes.Remove(paciente);
            await _context.SaveChangesAsync();
        }

        private async Task EliminarOdontologoPorEmailAsync(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return;

            var odontologo = await _context.Odontologos.FirstOrDefaultAsync(o => o.Email == email);
            if (odontologo == null)
                return;

            var tieneTurnos = await _context.Turnos.AnyAsync(t => t.OdontologoId == odontologo.Id);
            if (tieneTurnos)
                return;

            _context.Odontologos.Remove(odontologo);
            await _context.SaveChangesAsync();
        }

        //separa nombre y apellido desde el nombre completo
        private static (string nombre, string apellido) SepararNombre(string? nombreCompleto)
        {
            if (string.IsNullOrWhiteSpace(nombreCompleto))
                return ("Usuario", "Nuevo");

            var partes = nombreCompleto.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (partes.Length == 1)
                return (partes[0], "-");

            return (partes[0], string.Join(" ", partes.Skip(1)));
        }

        //elimina al usuario del sistema y limpia sus perfiles asociados
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarUsuario(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                if (!string.IsNullOrWhiteSpace(user.Email))
                {
                    var paciente = await _context.Pacientes.FirstOrDefaultAsync(p => p.Email == user.Email);
                    if (paciente != null)
                    {
                        var turnosPaciente = await _context.Turnos.AnyAsync(t => t.PacienteId == paciente.Id);
                        if (turnosPaciente)
                        {
                            TempData["Error"] = "No se puede eliminar el usuario porque tiene un perfil de paciente con turnos asociados.";
                            return RedirectToAction(nameof(Usuarios));
                        }

                        _context.Pacientes.Remove(paciente);
                    }

                    var odontologo = await _context.Odontologos.FirstOrDefaultAsync(o => o.Email == user.Email);
                    if (odontologo != null)
                    {
                        var turnosOdontologo = await _context.Turnos.AnyAsync(t => t.OdontologoId == odontologo.Id);
                        if (turnosOdontologo)
                        {
                            TempData["Error"] = "No se puede eliminar el usuario porque tiene un perfil de odontólogo con turnos asociados.";
                            return RedirectToAction(nameof(Usuarios));
                        }

                        _context.Odontologos.Remove(odontologo);
                    }

                    await _context.SaveChangesAsync();
                }

                await _userManager.DeleteAsync(user);
            }

            TempData["Mensaje"] = "Usuario eliminado correctamente.";
            return RedirectToAction(nameof(Usuarios));
        }
    }

    public class UserRoleViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? NombreCompleto { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}