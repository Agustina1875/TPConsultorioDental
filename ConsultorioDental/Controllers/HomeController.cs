using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ConsultorioDental.Models;

namespace ConsultorioDental.Controllers
{
    public class HomeController : Controller
    {
        //muestra la pagina principal
        public IActionResult Index()
        {
            return View();
        }

       
        //redirige al panel correspondiente segun el rol
        [Authorize]
        public IActionResult Dashboard()
        {
            if (User.IsInRole("Administrador"))
                return RedirectToAction("Index", "Admin");

            if (User.IsInRole("Odontologo"))
                return RedirectToAction("MisTurnos", "Turnos");

            if (User.IsInRole("Paciente"))
                return RedirectToAction("Panel", "Paciente");

            return RedirectToAction("Index", "Home");
        }

       
      
    }
}