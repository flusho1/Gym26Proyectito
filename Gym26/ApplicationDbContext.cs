using Microsoft.EntityFrameworkCore;
using Gym26.Models; // Asegúrate de que este namespace coincida con el que tienen tus archivos

namespace Gym26.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Ejercicio> Ejercicios { get; set; }
        public DbSet<PlanRutina> PlanesRutina { get; set; }
        public DbSet<Plantilla> Plantillas { get; set; }
        public DbSet<PlantillaDetalle> PlantillaDetalles { get; set; }
        public DbSet<RecordPersonal> RecordsPersonales { get; set; }
        public DbSet<Rutina> Rutinas { get; set; }
        public DbSet<RutinaDetalle> RutinaDetalles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
    }
}