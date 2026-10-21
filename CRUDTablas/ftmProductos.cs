using System;
using System.Linq;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using CRUDTablas.Models;

namespace CRUDTablas
{
    public partial class ftmProductos : Form
    {
        private readonly TiendaContext _context = new TiendaContext();
        private int _productoIdSeleccionado = 0;

        public ftmProductos()
        {
            InitializeComponent();
            CargarCategorias();
            CargarProductos();
        }

        private void ftmProductos_Load(object sender, EventArgs e)
        {
            // Ya se cargan los productos en el constructor.

        }

        // Carga las categorías en el ComboBox.
        private void CargarCategorias()
        {
            try
            {
                _context.Database.EnsureCreated();

                if (!_context.Categorias.Any())
                {
                    _context.Categorias.AddRange(
                        new Categoria { Nombre = "Electrónica" },
                        new Categoria { Nombre = "Ropa" }
                    );

                    _context.SaveChanges();
                }

                var listaCategorias = _context.Categorias
                    .OrderBy(c => c.Nombre)
                    .ToList();

                cmbCategorias.DataSource = null;
                cmbCategorias.DisplayMember = nameof(Categoria.Nombre);
                cmbCategorias.ValueMember = nameof(Categoria.Id);
                cmbCategorias.DataSource = listaCategorias;

                
                    cmbCategorias.SelectedIndex = -1;
                }
            
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar categorías: {ex.GetBaseException().Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // Carga los productos en el DataGridView.
        private void CargarProductos()
        {
            var productos = _context.Productos
                .Select(p => new
                {
                    p.Id,
                    Producto = p.Nombre,
                    p.Precio,
                    Categoria = p.Categoria.Nombre,
                    p.CategoriaId
                })
                .ToList();

            dgvProductos.DataSource = productos;

            if (dgvProductos.Columns["CategoriaId"] != null)
            {
                dgvProductos.Columns["CategoriaId"].Visible = false;
            }
        }

        private void LimpiarFormulario()
        {
            txtNombre.Clear();
            txtPrecio.Clear();

         
                cmbCategorias.SelectedIndex = -1;
           

            _productoIdSeleccionado = 0;
            btnGuardar.Text = "Guardar";
        }

        // Copia al formulario los datos de la fila seleccionada.
        private void SeleccionarProducto(DataGridViewRow fila)
        {
            if (fila == null ||
                fila.IsNewRow ||
                fila.Cells["Id"].Value == null)
            {
                MessageBox.Show("Seleccione un producto válido.");
                return;
            }

            _productoIdSeleccionado = Convert.ToInt32(fila.Cells["Id"].Value);

            txtNombre.Text =
                Convert.ToString(fila.Cells["Producto"].Value) ?? string.Empty;

            txtPrecio.Text =
                Convert.ToString(fila.Cells["Precio"].Value) ?? string.Empty;

            object valorCategoriaId = fila.Cells["CategoriaId"].Value;

            if (valorCategoriaId != null && valorCategoriaId != DBNull.Value)
            {
                cmbCategorias.SelectedValue = Convert.ToInt32(valorCategoriaId);
            }

            btnGuardar.Text = "Actualizar";
        }

        // CREATE / UPDATE
        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Introduzca el nombre del producto.");
                txtNombre.Focus();
                return;
            }

            if (!decimal.TryParse(txtPrecio.Text, out decimal precio))
            {
                MessageBox.Show("Introduzca un precio válido.");
                txtPrecio.Focus();
                return;
            }

            if (cmbCategorias.SelectedItem is not Categoria categoriaSeleccionada)
            {
                MessageBox.Show("Seleccione una categoría válida.");
                return;
            }

            int categoriaId = categoriaSeleccionada.Id;

            if (!_context.Categorias.Any(c => c.Id == categoriaId))
            {
                MessageBox.Show("La categoría seleccionada no existe en la base de datos.");
                CargarCategorias();
                return;
            }

            if (_productoIdSeleccionado == 0)
            {
                var nuevoProducto = new Producto
                {
                    Nombre = txtNombre.Text.Trim(),
                    Precio = precio,
                    CategoriaId = categoriaId
                };

                _context.Productos.Add(nuevoProducto);
            }
            else
            {
                var productoExistente =
                    _context.Productos.Find(_productoIdSeleccionado);

                if (productoExistente == null)
                {
                    MessageBox.Show("No se encontró el producto que intenta actualizar.");
                    LimpiarFormulario();
                    CargarProductos();
                    return;
                }

                productoExistente.Nombre = txtNombre.Text.Trim();
                productoExistente.Precio = precio;
                productoExistente.CategoriaId = categoriaId;
            }

            try
            {
                _context.SaveChanges();
                CargarProductos();
                LimpiarFormulario();

                MessageBox.Show("Operación realizada con éxito.");
            }
            catch (DbUpdateException ex)
            {
                MessageBox.Show(
                    $"No se pudo guardar el producto: {ex.GetBaseException().Message}",
                    "Error de base de datos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // Al hacer clic en una fila, carga sus datos en el formulario.
        private void dgvProductos_CellClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                SeleccionarProducto(dgvProductos.Rows[e.RowIndex]);
            }
        }

        // El botón Editar también carga la fila actual en el formulario.
        private void btnEditar_Click(object sender, EventArgs e)
        {
            if (dgvProductos.CurrentRow == null)
            {
                MessageBox.Show("Seleccione un producto de la tabla.");
                return;
            }

            SeleccionarProducto(dgvProductos.CurrentRow);
        }

        // DELETE
        private void btnEliminar_Click(object sender, EventArgs e)
        {
            int productoId = _productoIdSeleccionado;

            // Si CellClick no estableció el ID, intenta obtenerlo de la fila actual.
            if (productoId == 0 &&
                dgvProductos.CurrentRow != null &&
                dgvProductos.CurrentRow.Cells["Id"].Value != null)
            {
                productoId = Convert.ToInt32(
                    dgvProductos.CurrentRow.Cells["Id"].Value);
            }

            if (productoId == 0)
            {
                MessageBox.Show("Seleccione un producto de la tabla para eliminar.");
                return;
            }

            var confirmacion = MessageBox.Show(
                "¿Está seguro de eliminar este producto?",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmacion != DialogResult.Yes)
            {
                return;
            }

            try
            {
                var producto = _context.Productos.Find(productoId);

                if (producto == null)
                {
                    MessageBox.Show("No se encontró el producto.");
                    CargarProductos();
                    LimpiarFormulario();
                    return;
                }

                _context.Productos.Remove(producto);
                _context.SaveChanges();

                CargarProductos();
                LimpiarFormulario();

                MessageBox.Show("Producto eliminado.");
            }
            catch (DbUpdateException ex)
            {
                MessageBox.Show(
                    $"No se pudo eliminar el producto: {ex.GetBaseException().Message}",
                    "Error de base de datos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        // Puentes del diseñador: conserva estos métodos si el diseñador los usa.
        private void btnGuardar_Click_1(object sender, EventArgs e)
        {
            btnGuardar_Click(sender, e);
        }

        private void btnEliminar_Click_1(object sender, EventArgs e)
        {
            btnEliminar_Click(sender, e);
        }

        private void btnLimpiar_Click_1(object sender, EventArgs e)
        {
            btnLimpiar_Click(sender, e);
        }
    }
}


