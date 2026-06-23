using ConsultorioDental.Data;
using ConsultorioDental.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConsultorioDental.Controllers
{
    public class PacienteController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PacienteController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        //muestra el panel principal del paciente
        [Authorize(Roles = "Paciente")]
        public async Task<IActionResult> Panel()
        {
            var paciente = await ObtenerPacienteActualAsync(true);
            if (paciente == null)
                return RedirectToAction("Index", "Home");

            return View("~/Views/Pacientes/Panel.cshtml", paciente);
        }

        //muestra el formulario para editar el perfil propio
        [Authorize(Roles = "Paciente")]
        public async Task<IActionResult> EditarPerfil()
        {
            var paciente = await ObtenerPacienteActualAsync();
            if (paciente == null)
                return RedirectToAction(nameof(Panel));

            return View("~/Views/Pacientes/EditarPerfil.cshtml", new PacientePerfilViewModel
            {
                Paciente = paciente
            });
        }

        //guarda los cambios del perfil del paciente
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Paciente")]
        public async Task<IActionResult> EditarPerfil(PacientePerfilViewModel vm)
        {
            var pacienteActual = await ObtenerPacienteActualAsync();
            var user = await _userManager.GetUserAsync(User);

            if (pacienteActual == null || user == null)
                return RedirectToAction(nameof(Panel));

            ModelState.Remove("Paciente.Turnos");

            if (!ModelState.IsValid)
            {
                return View("~/Views/Pacientes/EditarPerfil.cshtml", vm);
            }

            pacienteActual.Nombre = vm.Paciente.Nombre;
            pacienteActual.Apellido = vm.Paciente.Apellido;
            pacienteActual.DNI = vm.Paciente.DNI;
            pacienteActual.Telefono = vm.Paciente.Telefono;
            pacienteActual.FechaNacimiento = vm.Paciente.FechaNacimiento;
            pacienteActual.Email = vm.Paciente.Email;

            var identidadOk = await ActualizarUsuarioIdentityAsync(
                user,
                vm.Paciente.Email,
                $"{vm.Paciente.Nombre} {vm.Paciente.Apellido}".Trim(),
                vm.NuevaPassword);

            if (!identidadOk)
                return View("~/Views/Pacientes/EditarPerfil.cshtml", vm);

            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Perfil actualizado correctamente.";
            return RedirectToAction(nameof(Panel));
        }

        //lista todos los pacientes para administracion
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Index()
        {
            var pacientes = await _context.Pacientes
                .OrderBy(p => p.Apellido)
                .ThenBy(p => p.Nombre)
                .ToListAsync();

            return View("~/Views/Pacientes/Index.cshtml", pacientes);
        }

        //muestra el perfil completo del paciente con sus turnos

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Perfil(int id)
        {
            var paciente = await _context.Pacientes
                .Include(p => p.Turnos)
                .ThenInclude(t => t.Odontologo)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (paciente == null)
                return NotFound();

            return View("~/Views/Pacientes/Perfil.cshtml", paciente);
        }

        //muestra el formulario para editar un paciente

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Editar(int id)
        {
            var paciente = await _context.Pacientes.FindAsync(id);
            if (paciente == null)
                return NotFound();

            return View("~/Views/Pacientes/Editar.cshtml", paciente);
        }

        //guarda los cambios realizados sobre un paciente

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Editar(int id, Paciente paciente)
        {

            if (id != paciente.Id)
                return NotFound();

            ModelState.Remove("Turnos");

            var pacienteActual = await _context.Pacientes.FindAsync(id);
            if (pacienteActual == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                return View("~/Views/Pacientes/Editar.cshtml", paciente);
            }

            pacienteActual.Nombre = paciente.Nombre;
            pacienteActual.Apellido = paciente.Apellido;
            pacienteActual.DNI = paciente.DNI;
            pacienteActual.Telefono = paciente.Telefono;
            pacienteActual.FechaNacimiento = paciente.FechaNacimiento;
            pacienteActual.Email = paciente.Email;



            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Paciente actualizado.";
            return RedirectToAction(nameof(Index));
        }


        //muestra la confirmacion para eliminar un paciente
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var paciente = await _context.Pacientes.FindAsync(id);
            if (paciente == null)
                return NotFound();

            return View("~/Views/Pacientes/Eliminar.cshtml", paciente);
        }

        //elimina el paciente y solo borra la cuenta si era exclusivamente de paciente
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var paciente = await _context.Pacientes.FindAsync(id);
            if (paciente == null)
                return RedirectToAction(nameof(Index));


            var tieneTurnos = await _context.Turnos.AnyAsync(t => t.PacienteId == id);
            if (tieneTurnos)
            {
                TempData["Error"] = "No se puede eliminar el paciente porque tiene turnos asociados. Primero eliminá o reasigná sus turnos.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByEmailAsync(paciente.Email);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Paciente"))
                {
                    if (roles.Count == 1)
                    {
                        await _userManager.DeleteAsync(user);
                    }
                    else
                    {
                        await _userManager.RemoveFromRoleAsync(user, "Paciente");
                    }
                }
            }

            _context.Pacientes.Remove(paciente);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Paciente eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        //obtiene el paciente asociado al usuario autenticado
        private async Task<Paciente?> ObtenerPacienteActualAsync(bool incluirTurnos = false)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || string.IsNullOrWhiteSpace(user.Email))
                return null;

           
            var query = _context.Pacientes.AsQueryable();

            if (incluirTurnos)
            {
                query = query
                    .Include(p => p.Turnos)
                    .ThenInclude(t => t.Odontologo);
            }

            return await query.FirstOrDefaultAsync(p => p.Email == user.Email);
        }

        //actualiza email, nombre y contraseña en identity
        private async Task<bool> ActualizarUsuarioIdentityAsync(
            ApplicationUser user,
            string nuevoEmail,
            string nombreCompleto,
            string? nuevaPassword)
        {
           
            if (!string.Equals(user.Email, nuevoEmail, StringComparison.OrdinalIgnoreCase))
            {
                user.Email = nuevoEmail;
                user.UserName = nuevoEmail;
            }

            user.NombreCompleto = nombreCompleto;

            var actualizar = await _userManager.UpdateAsync(user);
            if (!actualizar.Succeeded)
            {
                foreach (var error in actualizar.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return false;
            }

          
            
            if (!string.IsNullOrWhiteSpace(nuevaPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resultado = await _userManager.ResetPasswordAsync(user, token, nuevaPassword);

                if (!resultado.Succeeded)
                {
                    foreach (var error in resultado.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);

                   
                    return false;
                }
            }


            return true;
        
        }

    }


}