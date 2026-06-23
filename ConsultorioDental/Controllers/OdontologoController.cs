using ConsultorioDental.Data;
using ConsultorioDental.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConsultorioDental.Controllers
{
    public class OdontologosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OdontologosController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        //lista todos los odontologos para administracion
        [Authorize(Roles = "Administrador")]
        
        public async Task<IActionResult> Index()
        {
            var odontologos = await _context.Odontologos
                .OrderBy(o => o.Apellido)
                .ThenBy(o => o.Nombre)
                .ToListAsync();

            return View("~/Views/Odontologos/Index.cshtml", odontologos);
        }



        //muestra el formulario para editar un odontologo

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Editar(int id)
       
        {
            var odontologo = await _context.Odontologos.FindAsync(id);
            if (odontologo == null)
                return NotFound();

            return View("~/Views/Odontologos/Editar.cshtml", odontologo);
        }



        //guarda los cambios realizados sobre un odontologo

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Editar(int id, Odontologo odontologo)
        {
            if (id != odontologo.Id)
                return NotFound();

            ModelState.Remove("Turnos");

            var odontologoActual = await _context.Odontologos.FindAsync(id);
            if (odontologoActual == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                return View("~/Views/Odontologos/Editar.cshtml", odontologo);
            }

            odontologoActual.Nombre = odontologo.Nombre;
            odontologoActual.Apellido = odontologo.Apellido;
            odontologoActual.Especialidad = odontologo.Especialidad;
            odontologoActual.Telefono = odontologo.Telefono;
            odontologoActual.Email = odontologo.Email;

            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Odontólogo actualizado.";
            return RedirectToAction(nameof(Index));
        }




        //muestra el formulario para editar el perfil propio

        [Authorize(Roles = "Odontologo")]
        public async Task<IActionResult> EditarPerfil()
        {
            var odontologo = await ObtenerOdontologoActualAsync();

            if (odontologo == null)
                return RedirectToAction("MisTurnos", "Turnos");

            return View("~/Views/Odontologos/EditarPerfil.cshtml", new OdontologoPerfilViewModel
            {
                Odontologo = odontologo
            });
        }



        //guarda los cambios del perfil del odontologo

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Odontologo")]
        public async Task<IActionResult> EditarPerfil(OdontologoPerfilViewModel vm)
        {
            var odontologoActual = await ObtenerOdontologoActualAsync();
            var user = await _userManager.GetUserAsync(User);

            if (odontologoActual == null || user == null)
                return RedirectToAction("MisTurnos", "Turnos");

            ModelState.Remove("Odontologo.Turnos");

            if (!ModelState.IsValid)
            {
                return View("~/Views/Odontologos/EditarPerfil.cshtml", vm);
            }

            odontologoActual.Nombre = vm.Odontologo.Nombre;
            odontologoActual.Apellido = vm.Odontologo.Apellido;
            odontologoActual.Especialidad = vm.Odontologo.Especialidad;
            odontologoActual.Telefono = vm.Odontologo.Telefono;
            odontologoActual.Email = vm.Odontologo.Email;

            var identidadOk = await ActualizarUsuarioIdentityAsync(
                user,
                vm.Odontologo.Email,
                $"{vm.Odontologo.Nombre} {vm.Odontologo.Apellido}".Trim(),
                vm.NuevaPassword);

            if (!identidadOk)
                return View("~/Views/Odontologos/EditarPerfil.cshtml", vm);

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Perfil actualizado correctamente.";
            return RedirectToAction("MisTurnos", "Turnos");
        }



        //muestra la confirmacion para eliminar un odontologo
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var odontologo = await _context.Odontologos.FindAsync(id);
            if (odontologo == null)
                return NotFound();

            return View("~/Views/Odontologos/Eliminar.cshtml", odontologo);
        }


        //elimina el odontologo y solo borra la cuenta si era exclusivamente de odontologo
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var odontologo = await _context.Odontologos.FindAsync(id);
            if (odontologo == null)
                return RedirectToAction(nameof(Index));

            var tieneTurnos = await _context.Turnos.AnyAsync(t => t.OdontologoId == id);
            if (tieneTurnos)
            {
                TempData["Error"] = "No se puede eliminar el odontólogo porque tiene turnos asociados. Primero eliminá o reasigná sus turnos.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByEmailAsync(odontologo.Email);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Odontologo"))
                {
                    if (roles.Count == 1)
                    {
                        await _userManager.DeleteAsync(user);
                    }
                    else
                    {
                        await _userManager.RemoveFromRoleAsync(user, "Odontologo");
                    }
                }
            }

            _context.Odontologos.Remove(odontologo);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Odontólogo eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }



        //obtiene el odontologo asociado al usuario autenticado
        private async Task<Odontologo?> ObtenerOdontologoActualAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || string.IsNullOrWhiteSpace(user.Email))
                return null;

            return await _context.Odontologos
                .FirstOrDefaultAsync(o => o.Email == user.Email);
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