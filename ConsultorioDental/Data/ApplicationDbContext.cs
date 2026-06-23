using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ConsultorioDental.Models;

namespace ConsultorioDental.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        //configura el contexto de base de datos
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        //tabla de pacientes
        public DbSet<Paciente> Pacientes { get; set; }

        //tabla de odontologos
        public DbSet<Odontologo> Odontologos { get; set; }

        //tabla de turnos
        public DbSet<Turno> Turnos { get; set; }

        //define tablas y relaciones entre entidades
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Paciente>().ToTable("Pacientes");
            modelBuilder.Entity<Odontologo>().ToTable("Odontologos");
            modelBuilder.Entity<Turno>().ToTable("Turnos");

            modelBuilder.Entity<Turno>()
                .HasOne(t => t.Paciente)
                .WithMany(p => p.Turnos)
                .HasForeignKey(t => t.PacienteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Turno>()
                .HasOne(t => t.Odontologo)
                .WithMany(o => o.Turnos)
                .HasForeignKey(t => t.OdontologoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}