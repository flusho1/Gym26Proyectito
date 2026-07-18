namespace Gym26.Models
{
    // En Models/Plantilla.cs
    public class Plantilla
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int? UsuarioId { get; set; }
        public string TipoPlantilla => UsuarioId == null ? "Global" : "Personal";
    }

}