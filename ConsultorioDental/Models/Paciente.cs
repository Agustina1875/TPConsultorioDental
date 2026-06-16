using System.ComponentModel.DataAnnotations;

namespace ConsultorioDental.Models
{
    public class Paciente
    {
        public int Id { get; set; }

        //(valido desde la base de datos por eso el requiered, asi no es necesario validar en el controlador)
        [Display(Name = "Nombre")]
        [Required(ErrorMessage = "Debe ingresar un nombre.")]
        public string Nombre { get; set; } = "";

        [Display(Name = "Apellido")]
        [Required(ErrorMessage = "Debe ingresar un apellido.")]
        public string Apellido { get; set; } = "";

        [Display(Name = "DNI")]
        [Required(ErrorMessage = "Debe ingresar un DNI.")]
        public string DNI { get; set; } = "";

        [Display(Name = "Teléfono")]
        [Required(ErrorMessage = "Debe ingresar un teléfono.")]
        public string Telefono { get; set; } = "";

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Debe ingresar un email.")]
        [EmailAddress(ErrorMessage = "Debe ingresar un email válido.")]
        public string Email { get; set; } = "";

        [Display(Name = "Fecha de nacimiento")]
        [Required(ErrorMessage = "Debe ingresar una fecha de nacimiento.")]
        public DateTime? FechaNacimiento { get; set; }

        public string NombreCompleto => $"{Nombre} {Apellido}".Trim();
    }
}