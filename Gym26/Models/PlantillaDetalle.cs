using System.ComponentModel.DataAnnotations.Schema;

public class PlantillaDetalle
{
    public int Id { get; set; }
    public int PlantillaId { get; set; }
    public int EjercicioId { get; set; }
    public int Series { get; set; }
    public string Repeticiones { get; set; }

    [NotMapped]
    public string? NombreEjercicio { get; set; }

    [NotMapped]
    public string? UrlGif { get; set; } // <--- Agregá esto
}