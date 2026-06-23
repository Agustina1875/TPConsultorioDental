using System.ComponentModel.DataAnnotations;

namespace ConsultorioDental.Models
{
    public class Turno
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha y hora son obligatorias.")]
        [Display(Name = "Fecha y hora")]
        public DateTime FechaHora { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un paciente.")]
        [Display(Name = "Paciente")]
        public int PacienteId { get; set; }

        public Paciente? Paciente { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un odontólogo.")]
        [Display(Name = "Odontólogo")]
        public int OdontologoId { get; set; }

        public Odontologo? Odontologo { get; set; }

        [Required(ErrorMessage = "El motivo es obligatorio.")]
        [StringLength(200, ErrorMessage = "El motivo no puede superar los 200 caracteres.")]
        [Display(Name = "Motivo")]
        public string? Motivo { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio.")]
        [StringLength(30)]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = "Pendiente";

        [Display(Name = "El paciente asistió")]
        public bool? Asistio { get; set; }
    }
}