namespace CRUDTablas.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public decimal Precio { get; set; }
        public int CategoriaId { get; set; }

        // Propiedad de navegación (debe llamarse Categoria en singular)
        public virtual Categoria Categoria { get; set; } = null!;
    }
}