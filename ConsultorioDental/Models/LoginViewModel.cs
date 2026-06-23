using System.ComponentModel.DataAnnotations;

namespace ConsultorioDental.Models
{
    //modelo para iniciar sesion
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingresá un email válido.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;


        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]

        public string Password { get; set; } = string.Empty;


        [Display(Name = "Recordarme")]
        public bool Recordarme { get; set; }
    }
}