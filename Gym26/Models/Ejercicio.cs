namespace Gym26.Models
{
    public class Ejercicio
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string GrupoMuscular { get; set; }
        public string? UrlGif { get; set; } // Propiedad nueva
    }
}