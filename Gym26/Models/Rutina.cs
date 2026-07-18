namespace Gym26.Models
{
    public class Rutina
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int EjercicioId { get; set; }
        public int Series { get; set; }
        public string Repeticiones { get; set; } = string.Empty;
        public decimal PesoKG { get; set; }
        public DateTime FechaRegistro { get; set; }

        // Propiedad extendida para el nombre (no se guarda en la tabla Rutinas)
        public string? NombreEjercicio { get; set; }
        public string FechaFormateada => FechaRegistro.ToString("dd/MM");
    }
}