using ConsultorioDental.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ConsultorioDental.Data;
using Microsoft.EntityFrameworkCore;

namespace ConsultorioDental.Controllers
{
    public class CuentaController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public CuentaController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        //muestra la vista de login
       
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }


        //procesa el login con email y contraseña
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var result = await _signInManager.PasswordSignInAsync(vm.Email, vm.Password, vm.Recordarme, false);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos.");
                return View(vm);
            }

            var usuario = await _userManager.FindByEmailAsync(vm.Email);
            if (usuario == null)
                return RedirectToAction("Index", "Home");

           
            //crea el perfil si falta antes de redirigir
            await CrearPerfilFaltanteSegunRolAsync(usuario, usuario.NombreCompleto);
            return await RedirigirSegunRolAsync(usuario);
        }

        
        //muestra la vista de registro
      
        [AllowAnonymous]
        public IActionResult Registro()
        {
            return View(new RegisterViewModel
            {
                FechaNacimiento = DateTime.Today.AddYears(-18)
            });
        }

        //crea la cuenta nueva y el perfil de paciente
      
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(RegisterViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            if (await _userManager.FindByEmailAsync(vm.Email) != null)
            {
                ModelState.AddModelError(nameof(vm.Email), "Ya existe una cuenta con ese email.");
                return View(vm);
            }

            if (await _context.Pacientes.AnyAsync(p => p.Email == vm.Email))
            {
                ModelState.AddModelError(nameof(vm.Email), "Ya existe un paciente con ese email.");
                return View(vm);
            }

            var usuario = new ApplicationUser
            {
                UserName = vm.Email,
                Email = vm.Email,
                NombreCompleto = $"{vm.Nombre} {vm.Apellido}".Trim(),
                EmailConfirmed = true
            };

            var creado = await _userManager.CreateAsync(usuario, vm.Password);
            if (!creado.Succeeded)
            {
                foreach (var error in creado.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(vm);
            }

            await _userManager.AddToRoleAsync(usuario, "Paciente");

            _context.Pacientes.Add(new Paciente
            {
                Nombre = vm.Nombre,
                Apellido = vm.Apellido,
                DNI = vm.DNI,
                Telefono = vm.Telefono,
                FechaNacimiento = vm.FechaNacimiento,
                Email = vm.Email
            });

            await _context.SaveChangesAsync();
            await _signInManager.SignInAsync(usuario, isPersistent: false);
            return RedirectToAction("Panel", "Paciente");
        }



        //inicia el login con google
       
        
        [AllowAnonymous]
        public IActionResult LoginWithGoogle()
        {
            var redirectUrl = Url.Action(nameof(GoogleResponse), "Cuenta");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(
                GoogleDefaults.AuthenticationScheme, redirectUrl);
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

       
        //recibe la respuesta de google y completa el inicio de sesion
        [AllowAnonymous]
       
        public async Task<IActionResult> GoogleResponse()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return RedirectToAction(nameof(Login));

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var nombre = info.Principal.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrWhiteSpace(email))
            {
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                return RedirectToAction(nameof(Login));
            }

            var usuario = await _userManager.FindByEmailAsync(email);


            if (usuario == null)
            {
                usuario = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    NombreCompleto = nombre ?? "",
                    EmailConfirmed = true
                };

                var creado = await _userManager.CreateAsync(usuario);
                if (!creado.Succeeded)
                {
                    foreach (var error in creado.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                    await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                    return View("Login", new LoginViewModel());
                }

                await _userManager.AddToRoleAsync(usuario, "Paciente");
                await CrearPacienteSiNoExisteAsync(email, nombre);
            }
            else
            {
                await CrearPerfilFaltanteSegunRolAsync(usuario, nombre);
            }

            await _signInManager.SignInAsync(usuario, isPersistent: false);
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            return await RedirigirSegunRolAsync(usuario);
        }

        //crea un paciente minimo si no existe
        private async Task CrearPacienteSiNoExisteAsync(string email, string? nombreCompleto)
        {
            var existe = await _context.Pacientes.AnyAsync(p => p.Email == email);
            if (existe) return;

            var (nombre, apellido) = SepararNombre(nombreCompleto);
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

        //crea los perfiles faltantes segun los roles del usuario
        private async Task CrearPerfilFaltanteSegunRolAsync(ApplicationUser usuario, string? nombreCompleto)
        {
            var roles = await _userManager.GetRolesAsync(usuario);

            if (roles.Contains("Paciente"))
                await CrearPacienteSiNoExisteAsync(usuario.Email!, nombreCompleto ?? usuario.NombreCompleto);

            if (roles.Contains("Odontologo"))
                await CrearOdontologoSiNoExisteAsync(usuario.Email!, nombreCompleto ?? usuario.NombreCompleto);
        }

        //crea un odontologo minimo si no existe
        private async Task CrearOdontologoSiNoExisteAsync(string email, string? nombreCompleto)
        {
            var existe = await _context.Odontologos.AnyAsync(o => o.Email == email);
            if (existe) return;

            var (nombre, apellido) = SepararNombre(nombreCompleto);
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

        //redirige segun el rol principal del usuario
        private async Task<IActionResult> RedirigirSegunRolAsync(ApplicationUser usuario)
        {
            var roles = await _userManager.GetRolesAsync(usuario);

            if (roles.Contains("Administrador"))
                return RedirectToAction("Index", "Admin");

            if (roles.Contains("Odontologo"))
                return RedirectToAction("MisTurnos", "Turnos");

            if (roles.Contains("Paciente"))
                return RedirectToAction("Panel", "Paciente");

            return RedirectToAction("Index", "Home");
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

      

        //cierra la sesion actual

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}