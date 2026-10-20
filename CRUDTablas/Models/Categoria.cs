using System.Collections.Generic;
namespace SuProyecto.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;

        // Relación: Una categoría tiene muchos productos
        public virtual ICollection<Producto> Productos { get; set; } = new List<Productos>();
    }
}

