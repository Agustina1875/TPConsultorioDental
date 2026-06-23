using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsultorioDental.Models
{
    public class Odontologo
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(50, ErrorMessage = "El apellido no puede superar los 50 caracteres.")]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; } = string.Empty;

        [NotMapped]
        public string NombreCompleto => $"{Nombre} {Apellido}".Trim();

        [Required(ErrorMessage = "La especialidad es obligatoria.")]
        [StringLength(80, ErrorMessage = "La especialidad no puede superar los 80 caracteres.")]
        [Display(Name = "Especialidad")]
        public string Especialidad { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [RegularExpression(@"^[0-9+\-\s()]{6,20}$", ErrorMessage = "Ingresá un número de teléfono válido.")]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingresá una dirección de email válida.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        public ICollection<Turno> Turnos { get; set; } = new List<Turno>();
    }
}