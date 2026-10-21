using System;
using System.Linq;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using CRUDTablas.Models;

namespace CRUDTablas
{
    public partial class ftmProductos : Form
    {
        private TiendaContext _context = new TiendaContext();
        private int _productoIdSeleccionado = 0;

        public ftmProductos()
        {
            InitializeComponent();
        }

        private void ftmProductos_Load(object sender, EventArgs e)
        {
            CargarCategorias();
            CargarProductos();
        }



        // --- MÉTODOS DE SOPORTE ---
        private void CargarCategorias()
        {
            try
            {
                // 1. Asegura que la base de datos y las tablas existan
                _context.Database.EnsureCreated();

                // 2. Inserta datos por defecto si la tabla está vacía
                if (!_context.Categorias.Any())
                {
                    _context.Categorias.AddRange(
                        new Categoria { Nombre = "Electrónica" },
                        new Categoria { Nombre = "Ropa" }
                    );
                    _context.SaveChanges();
                }

                // 3. Consulta limpia aislando solo Id y Nombre (Evita el conflicto de EF Core)
                var listaCategorias = _context.Categorias
                    .Select(c => new { c.Id, c.Nombre })
                    .ToList();

                // 4. Asignamos los valores al ComboBox
                cmbCategorias.DataSource = null;
                cmbCategorias.DataSource = listaCategorias;
                cmbCategorias.DisplayMember = "Nombre";
                cmbCategorias.ValueMember = "Id";

                // 5. Seleccionamos el primer elemento por defecto
                if (cmbCategorias.Items.Count > 0)
                {
                    cmbCategorias.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar categorías: " + ex.Message);
            }
        }




        private void CargarProductos()
        {
            // READ: Carga productos incluyendo los datos de la tabla relacionada Categorias
            var productos = _context.Productos
                .Include(p => p.Categoria)
                .Select(p => new
                {
                    p.Id,
                    Producto = p.Nombre,
                    p.Precio,
                    Categoria = p.Categoria.Nombre,
                    p.CategoriaId
                }).ToList();

            dgvProductos.DataSource = productos;

            // Ocultar columna de ID de categoría
            if (dgvProductos.Columns["CategoriaId"] != null)
                dgvProductos.Columns["CategoriaId"].Visible = false;
        }

        private void LimpiarFormulario()
        {
            txtNombre.Clear();
            txtPrecio.Clear();
            if (cmbCategorias.Items.Count > 0)
                cmbCategorias.SelectedIndex = 0;
            _productoIdSeleccionado = 0;
            btnGuardar.Text = "Guardar";
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtNombre.Text) || !decimal.TryParse(txtPrecio.Text, out decimal precio))
            {
                MessageBox.Show("Por favor, rellene los campos correctamente.");
                return;
            }

            // Validación de seguridad para que no explote si no hay categoría seleccionada
            if (cmbCategorias.SelectedValue == null)
            {
                MessageBox.Show("Por favor, seleccione una categoría válida.");
                return;
            }

            int categoriaId = Convert.ToInt32(cmbCategorias.SelectedValue);

            if (_productoIdSeleccionado == 0)
            {
                // CREATE
                var nuevoProducto = new Producto
                {
                    Nombre = txtNombre.Text,
                    Precio = precio,
                    CategoriaId = categoriaId
                };
                _context.Productos.Add(nuevoProducto);
            }
            else
            {
                // UPDATE
                var productoExistente = _context.Productos.Find(_productoIdSeleccionado);
                if (productoExistente != null)
                {
                    productoExistente.Nombre = txtNombre.Text;
                    productoExistente.Precio = precio;
                    productoExistente.CategoriaId = categoriaId;
                }
            }

            _context.SaveChanges();
            CargarProductos();
            LimpiarFormulario();
            MessageBox.Show("¡Operación realizada con éxito!");
        }

        private void dgvProductos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvProductos.Rows[e.RowIndex];
                _productoIdSeleccionado = Convert.ToInt32(row.Cells["Id"].Value);
                txtNombre.Text = row.Cells["Producto"].Value.ToString();
                txtPrecio.Text = row.Cells["Precio"].Value.ToString();
                cmbCategorias.SelectedValue = Convert.ToInt32(row.Cells["CategoriaId"].Value);
                btnGuardar.Text = "Actualizar";
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (_productoIdSeleccionado == 0)
            {
                MessageBox.Show("Seleccione un producto de la lista para eliminar.");
                return;
            }

            var confirm = MessageBox.Show("¿Está seguro de eliminar este producto?", "Confirmar", MessageBoxButtons.YesNo);
            if (confirm == DialogResult.Yes)
            {
                var producto = _context.Productos.Find(_productoIdSeleccionado);
                if (producto != null)
                {
                    _context.Productos.Remove(producto);
                    _context.SaveChanges();
                    CargarProductos();
                    LimpiarFormulario();
                    MessageBox.Show("Producto eliminado.");
                }
            }
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        // --- MÉTODOS PUENTE PARA EL DISEÑADOR (DENTRO DE LA CLASE) ---
        private void btnGuardar_Click_1(object sender, EventArgs e) => btnGuardar_Click(sender, e);
        private void btnEditar_Click(object sender, EventArgs e) { }
        private void btnEliminar_Click_1(object sender, EventArgs e) => btnEliminar_Click(sender, e);
        private void btnLimpiar_Click_1(object sender, EventArgs e) => btnLimpiar_Click(sender, e);
    }
}