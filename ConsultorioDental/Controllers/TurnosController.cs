using System.Globalization;
using ConsultorioDental.Data;
using ConsultorioDental.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ConsultorioDental.Controllers
{
    public class TurnosController : Controller
    {
        //horario de inicio de atencion
        private static readonly TimeSpan HoraInicio = TimeSpan.FromHours(8);

        //horario de fin de atencion
        private static readonly TimeSpan HoraFin = TimeSpan.FromHours(18);

        //duracion de cada bloque de turno
        private static readonly TimeSpan Intervalo = TimeSpan.FromMinutes(30);

        //estado para los turnos
        private static readonly string[] EstadosValidos = { "Pendiente", "Confirmado", "Completado", "Cancelado", "Inasistencia" };

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TurnosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        //lista todos los turnos con filtro opcional por odontologo
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Index(int? odontologoId = null)
        {
            var odontologos = await _context.Odontologos
                .OrderBy(o => o.Apellido)
                .ThenBy(o => o.Nombre)
                .ToListAsync();

            ViewBag.Odontologos = new SelectList(odontologos, "Id", "NombreCompleto", odontologoId);

            var query = _context.Turnos
                .Include(t => t.Paciente)
                .Include(t => t.Odontologo)
                .OrderBy(t => t.FechaHora)
                .AsQueryable();

            if (odontologoId.HasValue && odontologoId.Value > 0)
            {
                query = query.Where(t => t.OdontologoId == odontologoId.Value);
            }

            var turnos = await query.ToListAsync();

            return View(turnos);
        }

        //muestra la agenda diaria organizada por horarios
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Agenda(DateTime? fechaSeleccionada = null, int? odontologoId = null)
        {
            var fecha = (fechaSeleccionada ?? DateTime.Today).Date;
            var inicioDia = fecha;
            var finDia = fecha.AddDays(1);

            var odontologos = await _context.Odontologos
                .OrderBy(o => o.Apellido)
                .ThenBy(o => o.Nombre)
                .ToListAsync();

            var query = _context.Turnos
                .Include(t => t.Paciente)
                .Include(t => t.Odontologo)
                .Where(t => t.FechaHora >= inicioDia && t.FechaHora < finDia && t.Estado != "Cancelado");

            if (odontologoId.HasValue && odontologoId.Value > 0)
            {
                query = query.Where(t => t.OdontologoId == odontologoId.Value);
            }

            var turnosDia = await query
                .OrderBy(t => t.FechaHora)
                .ToListAsync();

            var slots = new List<AgendaSlotViewModel>();

            for (var hora = HoraInicio; hora < HoraFin; hora += Intervalo)
            {
                var fechaHora = fecha.Add(hora);
                var turnosSlot = turnosDia
                    .Where(t => t.FechaHora == fechaHora)
                    .OrderBy(t => t.Odontologo!.Apellido)
                    .ThenBy(t => t.Odontologo!.Nombre)
                    .ToList();

                slots.Add(new AgendaSlotViewModel
                {
                    FechaHora = fechaHora,
                    Disponible = !turnosSlot.Any(),
                    Turnos = turnosSlot
                });
            }

            var vm = new AgendaViewModel
            {
                FechaSeleccionada = fecha,
                FechaDesde = inicioDia,
                FechaHasta = finDia.AddSeconds(-1),
                OdontologoId = odontologoId,
                Odontologos = odontologos,
                Slots = slots
            };

            return View(vm);
        }

        //muestra los turnos asignados al odontologo logueado
        [Authorize(Roles = "Odontologo")]
        public async Task<IActionResult> MisTurnos()
        {
            var user = await _userManager.GetUserAsync(User);
            var odontologo = await _context.Odontologos
                .FirstOrDefaultAsync(o => o.Email == user!.Email);

            if (odontologo == null)
            {
                ViewBag.Error = "Tu cuenta no esta vinculada a ningun odontologo. Contacta al administrador.";
                return View(new List<Turno>());
            }

            var turnos = await _context.Turnos
                .Include(t => t.Paciente)
                .Where(t => t.OdontologoId == odontologo.Id)
                .OrderBy(t => t.FechaHora)
                .ToListAsync();

            ViewBag.OdontologoNombre = odontologo.NombreCompleto;
            return View(turnos);
        }

        //marca asistencia o inasistencia de un turno confirmado
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Odontologo")]
        public async Task<IActionResult> MarcarAsistencia(int id, bool asistio)
        {
            var turno = await _context.Turnos.FindAsync(id);
            if (turno == null) return NotFound();

            if (turno.Estado != "Confirmado")
                return RedirectToAction(nameof(MisTurnos));

            turno.Asistio = asistio;
            turno.Estado = asistio ? "Completado" : "Inasistencia";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MisTurnos));
        }

        //permite al paciente confirmar un turno pendiente
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Paciente")]
        public async Task<IActionResult> Confirmar(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var paciente = await _context.Pacientes.FirstOrDefaultAsync(p => p.Email == user!.Email);
            var turno = await _context.Turnos.FindAsync(id);

            if (turno == null || paciente == null || turno.PacienteId != paciente.Id)
                return RedirectToAction(nameof(MisTurnosPaciente));

            if (turno.Estado == "Pendiente")
            {
                turno.Estado = "Confirmado";
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(MisTurnosPaciente));
        }

        //permite al paciente cancelar un turno propio
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Paciente")]
        public async Task<IActionResult> Cancelar(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var paciente = await _context.Pacientes.FirstOrDefaultAsync(p => p.Email == user!.Email);
            var turno = await _context.Turnos.FindAsync(id);

            if (turno == null || paciente == null || turno.PacienteId != paciente.Id)
                return RedirectToAction(nameof(MisTurnosPaciente));

            if (turno.Estado == "Pendiente" || turno.Estado == "Confirmado")
            {
                turno.Estado = "Cancelado";
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(MisTurnosPaciente));
        }

        //muestra los turnos del paciente autenticado
        [Authorize(Roles = "Paciente")]
        public async Task<IActionResult> MisTurnosPaciente()
        {
            var user = await _userManager.GetUserAsync(User);
            var paciente = await _context.Pacientes
                .FirstOrDefaultAsync(p => p.Email == user!.Email);

            if (paciente == null)
            {
                ViewBag.Error = "Tu cuenta no esta vinculada a ningun paciente. Contacta al administrador.";
                return View("~/Views/Turnos/MisTurnosPaciente.cshtml", new List<Turno>());
            }

            var turnos = await _context.Turnos
                .Include(t => t.Odontologo)
                .Where(t => t.PacienteId == paciente.Id)
                .OrderBy(t => t.FechaHora)
                .ToListAsync();

            ViewBag.PacienteNombre = paciente.NombreCompleto;
            return View("~/Views/Turnos/MisTurnosPaciente.cshtml", turnos);
        }

        //muestra el formulario para crear un turno
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Crear()
        {
            await CargarFormulariosAsync();
            return View(new TurnoFormViewModel
            {
                Fecha = DateTime.Today,
                Hora = "08:00",
                Estado = "Pendiente"
            });
        }

        //crea un nuevo turno validando disponibilidad
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Crear(TurnoFormViewModel vm)
        {
            ModelState.Remove(nameof(vm.Estado));

            vm.Estado = string.IsNullOrWhiteSpace(vm.Estado) ? "Pendiente" : vm.Estado.Trim();

            if (!TryConstruirFechaHora(vm.Fecha, vm.Hora, out var fechaHora))
            {
                await CargarFormulariosAsync(vm.PacienteId, vm.OdontologoId, vm.Hora);
                return View(vm);
            }

            var turno = new Turno
            {
                PacienteId = vm.PacienteId ?? 0,
                OdontologoId = vm.OdontologoId ?? 0,
                FechaHora = fechaHora,
                Estado = vm.Estado,
                Motivo = vm.Motivo
            };

            if (!await ValidarTurnoAsync(turno))
            {
                await CargarFormulariosAsync(vm.PacienteId, vm.OdontologoId, vm.Hora);
                return View(vm);
            }

            _context.Add(turno);
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Turno creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        //muestra el formulario para editar un turno
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Editar(int id)
        {
            var turno = await _context.Turnos.FindAsync(id);
            if (turno == null) return NotFound();

            await CargarFormulariosAsync(turno.PacienteId, turno.OdontologoId, turno.FechaHora.ToString("HH:mm"));
            return View(new TurnoFormViewModel
            {
                Id = turno.Id,
                PacienteId = turno.PacienteId,
                OdontologoId = turno.OdontologoId,
                Fecha = turno.FechaHora.Date,
                Hora = turno.FechaHora.ToString("HH:mm"),
                Estado = turno.Estado,
                Motivo = turno.Motivo
            });
        }

        //guarda los cambios realizados sobre un turno
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Editar(int id, TurnoFormViewModel vm)
        {
            if (id != vm.Id) return NotFound();

            ModelState.Remove(nameof(vm.Estado));
            vm.Estado = string.IsNullOrWhiteSpace(vm.Estado) ? "Pendiente" : vm.Estado.Trim();

            if (!TryConstruirFechaHora(vm.Fecha, vm.Hora, out var fechaHora))
            {
                await CargarFormulariosAsync(vm.PacienteId, vm.OdontologoId, vm.Hora);
                return View(vm);
            }

            var turnoActual = await _context.Turnos.FindAsync(id);
            if (turnoActual == null) return NotFound();

            var turnoValidacion = new Turno
            {
                Id = id,
                PacienteId = vm.PacienteId ?? 0,
                OdontologoId = vm.OdontologoId ?? 0,
                FechaHora = fechaHora,
                Estado = vm.Estado,
                Motivo = vm.Motivo
            };

            if (!await ValidarTurnoAsync(turnoValidacion, turnoValidacion.Id, validarFechaPasada: false))
            {
                await CargarFormulariosAsync(vm.PacienteId, vm.OdontologoId, vm.Hora);
                return View(vm);
            }

            turnoActual.PacienteId = vm.PacienteId ?? 0;
            turnoActual.OdontologoId = vm.OdontologoId ?? 0;
            turnoActual.FechaHora = fechaHora;
            turnoActual.Estado = vm.Estado;
            turnoActual.Motivo = vm.Motivo;

            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Turno actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        //muestra la confirmacion para eliminar un turno
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var turno = await _context.Turnos
                .Include(t => t.Paciente)
                .Include(t => t.Odontologo)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (turno == null) return NotFound();
            return View(turno);
        }

        //elimina definitivamente un turno
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var turno = await _context.Turnos.FindAsync(id);
            if (turno != null)
            {
                _context.Turnos.Remove(turno);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Turno eliminado correctamente.";
            }

            return RedirectToAction(nameof(Index));
        }

        //carga pacientes, odontologos y horarios para los formularios
        private async Task CargarFormulariosAsync(int? pacienteId = null, int? odontologoId = null, string? horaSeleccionada = null)
        {
            var pacientes = await _context.Pacientes
                .OrderBy(p => p.Apellido)
                .ThenBy(p => p.Nombre)
                .Select(p => new { p.Id, Nombre = p.Apellido + ", " + p.Nombre })
                .ToListAsync();

            var odontologos = await _context.Odontologos
                .OrderBy(o => o.Apellido)
                .ThenBy(o => o.Nombre)
                .Select(o => new { o.Id, Nombre = o.Apellido + ", " + o.Nombre + " — " + o.Especialidad })
                .ToListAsync();

            ViewBag.Pacientes = new SelectList(pacientes, "Id", "Nombre", pacienteId);
            ViewBag.Odontologos = new SelectList(odontologos, "Id", "Nombre", odontologoId);
            ViewBag.Horas = new SelectList(GenerarHorasTurno(), horaSeleccionada);
        }

        //genera los horarios disponibles segun la configuracion

        private static List<string> GenerarHorasTurno()
        {
            var horas = new List<string>();

            for (var hora = HoraInicio; hora < HoraFin; hora += Intervalo)
            {
                horas.Add(hora.ToString(@"hh\:mm"));
            }

            return horas;
        }

        //convierte fecha y hora seleccionadas en un datetime valido

        private bool TryConstruirFechaHora(DateTime? fecha, string? horaTexto, out DateTime fechaHora)
        {
            fechaHora = default;

            if (fecha == null || fecha == default)
            {
                ModelState.AddModelError("Fecha", "La fecha es obligatoria.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(horaTexto) ||
                !TimeSpan.TryParseExact(horaTexto, @"hh\:mm", CultureInfo.InvariantCulture, out var hora))
            {
                ModelState.AddModelError("Hora", "Seleccioná una hora válida.");
                return false;
            }

            fechaHora = fecha.Value.Date.Add(hora);
            return true;
        }

        //valida conflictos, horarios y reglas de negocio del turno

        private async Task<bool> ValidarTurnoAsync(Turno turno, int? turnoIdExcluir = null, bool validarFechaPasada = true)
        {
            var valido = true;

            if (turno.PacienteId <= 0)
            {
                ModelState.AddModelError(nameof(turno.PacienteId), "Debe seleccionar un paciente.");
                valido = false;
            }

            if (turno.OdontologoId <= 0)
            {
                ModelState.AddModelError(nameof(turno.OdontologoId), "Debe seleccionar un odontólogo.");
                valido = false;
            }

            if (string.IsNullOrWhiteSpace(turno.Estado) || !EstadosValidos.Contains(turno.Estado))
            {
                ModelState.AddModelError(nameof(turno.Estado), "El estado seleccionado no es válido.");
                valido = false;
            }

            if (turno.FechaHora == default)
            {
                ModelState.AddModelError("Fecha", "La fecha y hora son obligatorias.");
                ModelState.AddModelError("Hora", "La fecha y hora son obligatorias.");
                valido = false;
            }
            else
            {
                if (validarFechaPasada && turno.FechaHora < DateTime.Now)
                {
                    ModelState.AddModelError("Fecha", "No se puede crear un turno en una fecha pasada.");
                    valido = false;
                }

                var hora = turno.FechaHora.TimeOfDay;
                if (hora < HoraInicio || hora >= HoraFin)
                {
                    ModelState.AddModelError("Hora", "El horario debe estar entre las 08:00 y las 18:00.");
                    valido = false;
                }

                if (turno.FechaHora.Minute % 30 != 0 || turno.FechaHora.Second != 0)
                {
                    ModelState.AddModelError("Hora", "Los turnos deben programarse en bloques de 30 minutos.");
                    valido = false;
                }
            }

            if (turno.PacienteId > 0)
            {
                var conflictoPaciente = await _context.Turnos.AnyAsync(t =>
                    t.Id != (turnoIdExcluir ?? 0) &&
                    t.PacienteId == turno.PacienteId &&
                    t.FechaHora == turno.FechaHora &&
                    t.Estado != "Cancelado");

                if (conflictoPaciente)
                {
                    ModelState.AddModelError("Hora", "El paciente ya tiene un turno en ese horario.");
                    valido = false;
                }
            }

            if (turno.OdontologoId > 0)
            {
                var conflictoOdontologo = await _context.Turnos.AnyAsync(t =>
                    t.Id != (turnoIdExcluir ?? 0) &&
                    t.OdontologoId == turno.OdontologoId &&
                    t.FechaHora == turno.FechaHora &&
                    t.Estado != "Cancelado");

                if (conflictoOdontologo)
                {
                    ModelState.AddModelError("Hora", "El odontólogo ya tiene un turno en ese horario.");
                    valido = false;
                }
            }

            return valido && ModelState.IsValid;

        }
    }


}