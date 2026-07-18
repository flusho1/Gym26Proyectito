namespace Gym26.Models
{
    public class RutinaDetalle
    {
        public int Id { get; set; }
        public int RutinaId { get; set; }
        public int EjercicioId { get; set; }
        public int Series { get; set; }
        public int Repeticiones { get; set; }
        public decimal Peso { get; set; }

        // Propiedad de navegación para mostrar el nombre en el Grid
        public string? NombreEjercicio { get; set; }
    }
}