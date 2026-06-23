using System.ComponentModel.DataAnnotations;

namespace ConsultorioDental.Models
{
    //editar el perfil de un paciente
    public class PacientePerfilViewModel
    {
        //datos del paciente
        public Paciente Paciente { get; set; } = new();

        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        public string? NuevaPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare("NuevaPassword", ErrorMessage = "Las contraseñas no coinciden.")]
        public string? ConfirmarPassword { get; set; }
    }

    // editar el perfil de un odontologo
    public class OdontologoPerfilViewModel
    {
        //datos del odontologo
        public Odontologo Odontologo { get; set; } = new();

        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        public string? NuevaPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare("NuevaPassword", ErrorMessage = "Las contraseñas no coinciden.")]
        public string? ConfirmarPassword { get; set; }
    }

    //vista para crear o editar turnos
    public class TurnoFormViewModel
    {
        //identificador del turno
        public int Id { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un paciente.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un paciente.")]
        [Display(Name = "Paciente")]
        public int? PacienteId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un odontólogo.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un odontólogo.")]
        [Display(Name = "Odontólogo")]
        public int? OdontologoId { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha")]
        public DateTime? Fecha { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "La hora es obligatoria.")]
        [Display(Name = "Hora")]
        public string Hora { get; set; } = string.Empty;

        [Required(ErrorMessage = "El motivo es obligatorio.")]
        [StringLength(200, ErrorMessage = "El motivo no puede superar los 200 caracteres.")]
        [Display(Name = "Motivo")]
        public string? Motivo { get; set; }

        [Display(Name = "Estado")]
        public string Estado { get; set; } = "Pendiente";
    }

    // agenda por horario
    public class AgendaSlotViewModel
    {
        public DateTime FechaHora { get; set; }
        public bool Disponible { get; set; }
        public List<Turno> Turnos { get; set; } = new();
    }

    //vista completa de la agenda diaria
    public class AgendaViewModel
    {
        public DateTime FechaSeleccionada { get; set; }
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public int? OdontologoId { get; set; }
        public List<Odontologo> Odontologos { get; set; } = new();
        public List<AgendaSlotViewModel> Slots { get; set; } = new();
    }
}