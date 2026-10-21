using Microsoft.EntityFrameworkCore;
using System;
using System.IO;



namespace CRUDTablas.Models
{
    public class TiendaContext : DbContext
    {
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Producto> Productos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    @"Server=(LocalDB)\MSSQLLocalDB;Database=CRUDTablasProductosDB;Integrated Security=True;TrustServerCertificate=True");
            }
        }
    }
}

