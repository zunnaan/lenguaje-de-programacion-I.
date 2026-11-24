using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using TiendaApp.Dato;
using TiendaApp.Modelo;

namespace TiendaApp
{
    /// <summary>Administracion de productos e inventario: codigo, nombre, categoria, precio, ITBIS y stock.</summary>
    public class FrmProductos : Form
    {
        private readonly ProductoDAO dao = new ProductoDAO();
        private readonly TextBox txtCodigo, txtNombre, txtBuscar;
        private readonly ComboBox cboCategoria;
        private readonly NumericUpDown numPrecio, numItbis, numStock;
        private readonly CheckBox chkActivo;
        private readonly DataGridView dgv;
        private int idSel = 0;

        public FrmProductos()
        {
            Text = "Productos / inventario";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(860, 560);
            MinimumSize = new Size(876, 599);
            UI.SoloAdministrador(this);

            txtCodigo = UI.Txt(70, 15, 60);
            txtCodigo.ReadOnly = true;
            txtNombre = UI.Txt(200, 15, 230, 100);
            cboCategoria = new ComboBox { Left = 520, Top = 15, Width = 170, DropDownStyle = ComboBoxStyle.DropDown, MaxLength = 50 };

            numPrecio = new NumericUpDown { Left = 70, Top = 48, Width = 110, DecimalPlaces = 2, Maximum = 99999999, ThousandsSeparator = true };
            numItbis = new NumericUpDown { Left = 265, Top = 48, Width = 70, DecimalPlaces = 2, Maximum = 100, Value = 18 };
            numStock = new NumericUpDown { Left = 400, Top = 48, Width = 90, Maximum = 10000000 };
            chkActivo = new CheckBox { Text = "Activo", Left = 520, Top = 51, AutoSize = true, Checked = true };
            Label nota = UI.Lbl("ITBIS %: 18 gravados, 0 exentos", 610, 48);
            nota.ForeColor = SystemColors.GrayText;

            txtBuscar = UI.Txt(165, 128, 200, 100);

            dgv = UI.Grid(12, 165, 836, 383);
            dgv.CellDoubleClick += dgv_CellDoubleClick;

            Controls.AddRange(new Control[]
            {
                UI.Lbl("Código:", 15, 15), txtCodigo,
                UI.Lbl("Nombre:", 140, 15), txtNombre,
                UI.Lbl("Categoría:", 450, 15), cboCategoria,
                UI.Lbl("Precio:", 15, 48), numPrecio,
                UI.Lbl("ITBIS %:", 205, 48), numItbis,
                UI.Lbl("Stock:", 350, 48), numStock, chkActivo, nota,
                UI.Btn("Guardar", 15, 85, 110, btnGuardar_Click),
                UI.Btn("Actualizar", 135, 85, 110, btnActualizar_Click),
                UI.Btn("Eliminar", 255, 85, 110, btnEliminar_Click),
                UI.Btn("Limpiar", 375, 85, 110, btnLimpiar_Click),
                UI.Lbl("Buscar (código, nombre o categoría):", 15, 128), txtBuscar,
                UI.Btn("Buscar", 375, 125, 90, btnBuscar_Click),
                UI.Btn("Ver todos", 475, 125, 90, btnTodos_Click),
                dgv
            });

            Load += delegate { CargarCategorias(); CargarGrid(null); };
        }

        private void CargarCategorias()
        {
            try
            {
                string actual = cboCategoria.Text;
                cboCategoria.Items.Clear();
                List<string> cats = dao.Categorias();
                if (cats.Count == 0) cats.Add("General");
                cboCategoria.Items.AddRange(cats.ToArray());
                cboCategoria.Text = actual != "" ? actual : cats[0];
            }
            catch (Exception ex)
            {
                UI.Error("No se pudieron cargar las categorías: " + ex.Message);
            }
        }

        private void CargarGrid(List<Producto> datos)
        {
            try
            {
                dgv.DataSource = datos ?? dao.Read();
                Encabezado("Id", "Código");
                Encabezado("Categoria", "Categoría");
                Encabezado("ItbisPct", "ITBIS %");
                if (dgv.Columns["Precio"] != null) dgv.Columns["Precio"].DefaultCellStyle.Format = "N2";
            }
            catch (Exception ex)
            {
                UI.Error("No se pudieron cargar los productos: " + ex.Message);
            }
        }

        private void Encabezado(string columna, string texto)
        {
            if (dgv.Columns[columna] != null) dgv.Columns[columna].HeaderText = texto;
        }

        private void Limpiar()
        {
            idSel = 0;
            txtCodigo.Clear();
            txtNombre.Clear();
            numPrecio.Value = 0;
            numItbis.Value = 18;
            numStock.Value = 0;
            chkActivo.Checked = true;
            dgv.ClearSelection();
            txtNombre.Focus();
        }

        private Producto Leer()
        {
            string nombre = txtNombre.Text.Trim();
            string categoria = cboCategoria.Text.Trim();
            if (nombre == "")
            {
                UI.Aviso("El nombre del producto es obligatorio.");
                return null;
            }
            if (categoria == "")
            {
                UI.Aviso("La categoría es obligatoria (escribe una nueva o elige una de la lista).");
                return null;
            }
            if (numPrecio.Value <= 0)
            {
                UI.Aviso("El precio debe ser mayor que cero.");
                return null;
            }
            return new Producto
            {
                Id = idSel,
                Nombre = nombre,
                Categoria = categoria,
                Precio = numPrecio.Value,
                ItbisPct = numItbis.Value,
                Stock = (int)numStock.Value,
                Activo = chkActivo.Checked
            };
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (idSel != 0)
            {
                UI.Info("Hay un producto cargado. Usa Actualizar, o pulsa Limpiar para crear uno nuevo.");
                return;
            }
            Producto p = Leer();
            if (p == null) return;

            try
            {
                int id = dao.Create(p);
                UI.Info("Producto guardado con código " + id + ".");
                Limpiar();
                CargarCategorias();
                CargarGrid(null);
            }
            catch (Exception ex)
            {
                UI.Error("No se pudo guardar: " + ex.Message);
            }
        }

        private void btnActualizar_Click(object sender, EventArgs e)
        {
            if (idSel == 0)
            {
                UI.Info("Haz doble clic en un producto de la tabla para editarlo.");
                return;
            }
            Producto p = Leer();
            if (p == null) return;

            try
            {
                if (dao.Update(p))
                {
                    UI.Info("Producto actualizado.");
                    Limpiar();
                    CargarCategorias();
                    CargarGrid(null);
                }
                else
                {
                    UI.Aviso("No se encontró el producto.");
                }
            }
            catch (Exception ex)
            {
                UI.Error("No se pudo actualizar: " + ex.Message);
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (idSel == 0)
            {
                UI.Info("Haz doble clic en un producto de la tabla para eliminarlo.");
                return;
            }
            if (!UI.Confirmar("¿Eliminar el producto \"" + txtNombre.Text + "\"?")) return;

            try
            {
                dao.Delete(idSel);
                UI.Info("Producto eliminado.");
                Limpiar();
                CargarCategorias();
                CargarGrid(null);
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547)
                    UI.Aviso("Este producto ya tiene ventas y no se puede eliminar.\n" +
                             "Desmarca \"Activo\" y pulsa Actualizar para retirarlo de la venta.");
                else
                    UI.Error("No se pudo eliminar: " + ex.Message);
            }
            catch (Exception ex)
            {
                UI.Error("No se pudo eliminar: " + ex.Message);
            }
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            Limpiar();
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            string t = txtBuscar.Text.Trim();
            if (t == "")
            {
                UI.Aviso("Escribe un código, nombre o categoría a buscar.");
                return;
            }
            try
            {
                List<Producto> res = dao.Buscar(t);
                if (res.Count == 0) UI.Info("No hay productos que coincidan con \"" + t + "\".");
                CargarGrid(res);
            }
            catch (Exception ex)
            {
                UI.Error("Error en la búsqueda: " + ex.Message);
            }
        }

        private void btnTodos_Click(object sender, EventArgs e)
        {
            txtBuscar.Clear();
            CargarGrid(null);
        }

        private void dgv_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            Producto p = dgv.Rows[e.RowIndex].DataBoundItem as Producto;
            if (p == null) return;

            idSel = p.Id;
            txtCodigo.Text = p.Id.ToString();
            txtNombre.Text = p.Nombre;
            cboCategoria.Text = p.Categoria;
            numPrecio.Value = p.Precio;
            numItbis.Value = p.ItbisPct;
            numStock.Value = p.Stock;
            chkActivo.Checked = p.Activo;
        }
    }
}
