using System.ComponentModel.DataAnnotations.Schema;

namespace Gym26.Models
{
    public class PlantillaDetalle
    {
        public int Id { get; set; }
        public int PlantillaId { get; set; }
        public int EjercicioId { get; set; }
        public int Series { get; set; }
        public string Repeticiones { get; set; }

        // Propiedad auxiliar para la UI
        [NotMapped]
        public string? NombreEjercicio { get; set; }
    }
}