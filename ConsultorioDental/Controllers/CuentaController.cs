using ConsultorioDental.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ConsultorioDental.Controllers
{
    public class CuentaController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public CuentaController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        //vista con el boton de login
        public IActionResult Index()
        {
            return View();
        }

        //autenticacion con Google
        public async Task Login()
        {
            await HttpContext.ChallengeAsync(
                GoogleDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("GoogleResponse")
                });
        }

        //google redirige aca despues de autenticar
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync("Identity.External");

            if (result?.Principal == null)
                return RedirectToAction("Index");

            var email = result.Principal.FindFirst(
                System.Security.Claims.ClaimTypes.Email)?.Value;

            var nombre = result.Principal.FindFirst(
                System.Security.Claims.ClaimTypes.Name)?.Value;

            if (email == null)
                return RedirectToAction("Index");

            //busca si ya existe el usuario
            var usuario = await _userManager.FindByEmailAsync(email);

            if (usuario == null)
            {
                //lo crea y le asigna rol Usuario
                usuario = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    NombreCompleto = nombre ?? ""
                };

                await _userManager.CreateAsync(usuario);
                await _userManager.AddToRoleAsync(usuario, "Usuario");
            }

            await _signInManager.SignInAsync(usuario, isPersistent: false);

            return RedirectToAction("Index", "Home");
        }

        //cierra la sesion
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}