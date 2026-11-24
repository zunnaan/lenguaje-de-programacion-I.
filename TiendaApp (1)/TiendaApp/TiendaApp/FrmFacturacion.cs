using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using TiendaApp.Dato;
using TiendaApp.Modelo;

namespace TiendaApp
{
    /// <summary>Pantalla de ventas del cajero: arma la factura, cobra (contado) o registra la cuenta por cobrar (credito) e imprime.</summary>
    public class FrmFacturacion : Form
    {
        private readonly FacturaDAO facturaDao = new FacturaDAO();
        private readonly ProductoDAO productoDao = new ProductoDAO();
        private readonly PersonaDAO personaDao = new PersonaDAO();
        private readonly BindingList<FacturaDetalle> lineas = new BindingList<FacturaDetalle>();

        private readonly Label lblNumero, lblFechaHora, lblCajero, lblBruto, lblItbis, lblTotal;
        private readonly RadioButton rdoContado, rdoCredito;
        private readonly ComboBox cboCliente, cboProducto;
        private readonly TextBox txtCodigo;
        private readonly NumericUpDown numCantidad;
        private readonly DataGridView dgv;
        private readonly Timer reloj = new Timer();

        private decimal bruto, itbis, total;

        public FrmFacturacion()
        {
            Text = "Nueva venta";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(980, 620);
            MinimumSize = new Size(996, 659);

            Usuario u = Sesion.Actual;

            // ----- Encabezado -----
            lblNumero = new Label { Left = 12, Top = 10, AutoSize = true, Font = new Font("Segoe UI", 14f, FontStyle.Bold) };
            lblFechaHora = new Label { Left = 560, Top = 14, Width = 408, Height = 22, TextAlign = ContentAlignment.MiddleRight, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            lblCajero = new Label
            {
                Left = 14,
                Top = 42,
                AutoSize = true,
                Text = "Cajero: " + u.Nombre + (string.IsNullOrEmpty(u.Turno) ? "" : "   |   Turno: " + u.Turno)
            };

            // ----- Tipo de venta y cliente -----
            GroupBox gbVenta = new GroupBox { Text = "Venta", Left = 12, Top = 68, Width = 956, Height = 58, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            rdoContado = new RadioButton { Text = "Contado", Left = 15, Top = 23, AutoSize = true, Checked = true };
            rdoCredito = new RadioButton { Text = "Crédito", Left = 100, Top = 23, AutoSize = true };
            cboCliente = new ComboBox { Left = 290, Top = 21, Width = 320, DropDownStyle = ComboBoxStyle.DropDownList };
            Label lblCliente = UI.Lbl("Cliente:", 230, 21);
            Label lblNotaCliente = UI.Lbl("(obligatorio si es a crédito)", 625, 21);
            lblNotaCliente.ForeColor = SystemColors.GrayText;
            gbVenta.Controls.AddRange(new Control[] { rdoContado, rdoCredito, lblCliente, cboCliente, lblNotaCliente });

            // ----- Producto -----
            GroupBox gbProducto = new GroupBox { Text = "Producto", Left = 12, Top = 132, Width = 956, Height = 66, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            txtCodigo = UI.Txt(65, 25, 70);
            txtCodigo.KeyDown += txtCodigo_KeyDown;
            cboProducto = new ComboBox { Left = 225, Top = 25, Width = 420, DropDownStyle = ComboBoxStyle.DropDownList };
            numCantidad = new NumericUpDown { Left = 700, Top = 25, Width = 70, Minimum = 1, Maximum = 9999, Value = 1 };
            numCantidad.KeyDown += numCantidad_KeyDown;
            Button btnAgregar = UI.Btn("Agregar", 785, 22, 90, delegate { Agregar(); });
            gbProducto.Controls.AddRange(new Control[]
            {
                UI.Lbl("Código:", 15, 25), txtCodigo,
                UI.Lbl("Producto:", 150, 25), cboProducto,
                UI.Lbl("Cant.:", 655, 25), numCantidad,
                btnAgregar
            });

            // ----- Detalle -----
            dgv = UI.Grid(12, 206, 956, 300);   // el DataSource se asigna en Load (ConfigurarGrid)

            // ----- Totales y botones -----
            Button btnQuitar = UI.Btn("Quitar línea seleccionada", 12, 514, 170, delegate { QuitarLinea(); });
            Button btnNueva = UI.Btn("Cancelar venta", 190, 514, 120, delegate { if (lineas.Count == 0 || UI.Confirmar("¿Cancelar esta venta?")) NuevaVenta(); });
            btnQuitar.Anchor = btnNueva.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            Button btnFacturar = UI.Btn("FACTURAR  (F5)", 12, 556, 298, delegate { Facturar(); });
            btnFacturar.Height = 46;
            btnFacturar.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            btnFacturar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            Button btnCerrar = UI.Btn("Cerrar", 320, 567, 80, delegate { Close(); });
            btnCerrar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            lblBruto = MontoLabel(512);
            lblItbis = MontoLabel(538);
            lblTotal = MontoLabel(566);
            lblTotal.Font = new Font("Segoe UI", 16f, FontStyle.Bold);
            lblTotal.Height = 34;

            Label cBruto = CaptionLabel("Total bruto:", 514);
            Label cItbis = CaptionLabel("Total ITBIS:", 540);
            Label cTotal = CaptionLabel("Total a pagar:", 572);
            cTotal.Font = new Font("Segoe UI", 11f, FontStyle.Bold);

            Controls.AddRange(new Control[]
            {
                lblNumero, lblFechaHora, lblCajero, gbVenta, gbProducto, dgv,
                btnQuitar, btnNueva, btnFacturar, btnCerrar,
                cBruto, lblBruto, cItbis, lblItbis, cTotal, lblTotal
            });

            reloj.Interval = 1000;
            reloj.Tick += delegate { lblFechaHora.Text = DateTime.Now.ToString("dd/MM/yyyy   hh:mm:ss tt"); };

            // F5 = impresion rapida: finaliza la venta y genera el archivo .txt de la factura.
            KeyPreview = true;
            KeyDown += delegate (object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.F5)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    Facturar();
                }
            };

            Load += FrmFacturacion_Load;
        }

        private Label MontoLabel(int top)
        {
            return new Label
            {
                Left = 800,
                Top = top,
                Width = 168,
                Height = 22,
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Text = UI.Dinero(0)
            };
        }

        private Label CaptionLabel(string texto, int top)
        {
            return new Label
            {
                Text = texto,
                Left = 640,
                Top = top,
                Width = 150,
                Height = 22,
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
        }

        private void Encabezado(string columna, string texto)
        {
            if (dgv.Columns[columna] != null) dgv.Columns[columna].HeaderText = texto;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            reloj.Stop();
            reloj.Dispose();
            base.OnFormClosed(e);
        }

        // ---------- Carga inicial ----------

        private void ConfigurarGrid()
        {
            dgv.DataSource = lineas;
            Encabezado("ProductoId", "ID");
            Encabezado("NombreProducto", "Producto");
            Encabezado("Cantidad", "Cant.");
            Encabezado("Valor", "Valor");
            Encabezado("Itbis", "ITBIS");
            Encabezado("Total", "Total");
            foreach (string col in new[] { "Valor", "Itbis", "Total" })
            {
                if (dgv.Columns[col] == null) continue;
                dgv.Columns[col].DefaultCellStyle.Format = "N2";
                dgv.Columns[col].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }

        private void FrmFacturacion_Load(object sender, EventArgs e)
        {
            ConfigurarGrid();
            try
            {
                CargarClientes();
                CargarProductos();
                ActualizarNumero();
                lblFechaHora.Text = DateTime.Now.ToString("dd/MM/yyyy   hh:mm:ss tt");
                reloj.Start();
                txtCodigo.Focus();
            }
            catch (Exception ex)
            {
                UI.Error("No se pudieron cargar los datos de la venta: " + ex.Message);
            }
        }

        private void CargarClientes()
        {
            List<Persona> clientes = personaDao.Read();
            clientes.Insert(0, new Persona { Id = 0, Nombre = "Consumidor", Apellido = "final" });
            cboCliente.DataSource = clientes;
            cboCliente.DisplayMember = "NombreCompleto";
            cboCliente.SelectedIndex = 0;
        }

        private void CargarProductos()
        {
            cboProducto.DataSource = productoDao.ParaVenta();
            cboProducto.DisplayMember = "Descripcion";
            cboProducto.SelectedIndex = -1;
        }

        private void ActualizarNumero()
        {
            lblNumero.Text = "Factura No. " + facturaDao.ProximoNumero().ToString("D5");
        }

        // ---------- Captura de producto ----------

        private void txtCodigo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;

            int id;
            if (!int.TryParse(txtCodigo.Text.Trim(), out id))
            {
                UI.Aviso("Escribe el ID numérico del producto o elígelo en la lista.");
                return;
            }

            List<Producto> lista = cboProducto.DataSource as List<Producto>;
            Producto p = lista == null ? null : lista.Find(x => x.Id == id);
            if (p == null)
            {
                UI.Aviso("No existe un producto disponible con el ID " + id + " (¿inactivo o sin stock?).");
                return;
            }

            cboProducto.SelectedItem = p;
            numCantidad.Focus();
            numCantidad.Select(0, numCantidad.Text.Length);
        }

        private void numCantidad_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            Agregar();
        }

        private void Agregar()
        {
            Producto p = cboProducto.SelectedItem as Producto;
            if (p == null)
            {
                UI.Aviso("Selecciona un producto.");
                return;
            }

            int cantidad = (int)numCantidad.Value;
            FacturaDetalle linea = null;
            foreach (FacturaDetalle d in lineas)
            {
                if (d.ProductoId == p.Id) { linea = d; break; }
            }

            int yaEnVenta = linea == null ? 0 : linea.Cantidad;
            if (yaEnVenta + cantidad > p.Stock)
            {
                UI.Aviso("Stock insuficiente. Disponible: " + p.Stock +
                         (yaEnVenta > 0 ? " (ya llevas " + yaEnVenta + " en esta venta)" : "") + ".");
                return;
            }

            if (linea == null)
            {
                linea = new FacturaDetalle
                {
                    ProductoId = p.Id,
                    NombreProducto = p.Nombre,
                    Cantidad = cantidad,
                    Valor = p.Precio,
                    ItbisPct = p.ItbisPct
                };
                linea.Calcular();
                lineas.Add(linea);
            }
            else
            {
                linea.Cantidad += cantidad;
                linea.Calcular();
                lineas.ResetBindings();
            }

            numCantidad.Value = 1;
            txtCodigo.Clear();
            cboProducto.SelectedIndex = -1;
            RecalcularTotales();
            txtCodigo.Focus();
        }

        private void QuitarLinea()
        {
            if (dgv.CurrentRow == null || dgv.CurrentRow.Index < 0 || dgv.CurrentRow.Index >= lineas.Count)
            {
                UI.Info("Selecciona la línea que quieres quitar.");
                return;
            }
            lineas.RemoveAt(dgv.CurrentRow.Index);
            RecalcularTotales();
        }

        private void RecalcularTotales()
        {
            bruto = 0; itbis = 0; total = 0;
            foreach (FacturaDetalle d in lineas)
            {
                bruto += d.Subtotal;
                itbis += d.Itbis;
                total += d.Total;
            }
            lblBruto.Text = UI.Dinero(bruto);
            lblItbis.Text = UI.Dinero(itbis);
            lblTotal.Text = UI.Dinero(total);
        }

        private void NuevaVenta()
        {
            lineas.Clear();
            rdoContado.Checked = true;
            if (cboCliente.Items.Count > 0) cboCliente.SelectedIndex = 0;
            numCantidad.Value = 1;
            txtCodigo.Clear();
            RecalcularTotales();
            try
            {
                CargarProductos();   // refresca el stock
                ActualizarNumero();
            }
            catch (Exception ex)
            {
                UI.Error("No se pudo refrescar la lista de productos: " + ex.Message);
            }
            txtCodigo.Focus();
        }

        // ---------- Facturar ----------

        private void Facturar()
        {
            if (lineas.Count == 0)
            {
                UI.Aviso("Agrega al menos un producto a la factura.");
                return;
            }

            bool credito = rdoCredito.Checked;
            Persona cli = cboCliente.SelectedItem as Persona;
            int? personaId = (cli != null && cli.Id != 0) ? cli.Id : (int?)null;

            if (credito && personaId == null)
            {
                UI.Aviso("Las ventas a crédito requieren un cliente. Selecciónalo en la lista.");
                return;
            }

            decimal pagado = 0, devuelta = 0;
            if (credito)
            {
                if (!UI.Confirmar("Cantidad a pagar: " + UI.Dinero(total) + "\n\n" +
                                  "Venta a crédito para " + cli.NombreCompleto + ".\n" +
                                  "Se registrará en cuentas por cobrar. ¿Continuar?"))
                    return;
            }
            else
            {
                using (FrmPago pago = new FrmPago(total))
                {
                    if (pago.ShowDialog(this) != DialogResult.OK) return;
                    pagado = pago.MontoPagado;
                    devuelta = pago.Devuelta;
                }
            }

            Usuario u = Sesion.Actual;
            Factura f = new Factura
            {
                Fecha = DateTime.Now,
                Tipo = credito ? TipoFactura.Credito : TipoFactura.Contado,
                UsuarioId = u.Id,
                CajeroNombre = u.Nombre,
                Turno = u.Turno,
                PersonaId = personaId,
                ClienteNombre = personaId.HasValue ? cli.NombreCompleto : "Consumidor final",
                TotalBruto = bruto,
                TotalItbis = itbis,
                TotalPagar = total,
                MontoPagado = pagado,
                Devuelta = devuelta,
                Detalles = new List<FacturaDetalle>(lineas)
            };

            try
            {
                facturaDao.Guardar(f);
            }
            catch (SqlException ex)
            {
                UI.Error("No se pudo guardar la factura (no se descontó stock ni se consumió número):\n" + ex.Message);
                return;
            }
            catch (Exception ex)
            {
                UI.Error("No se pudo guardar la factura:\n" + ex.Message);
                return;
            }

            // Impresion rapida: .txt de la factura (carpeta configurable en App.config).
            try
            {
                string ruta = FacturaArchivo.GuardarTxt(f);
                lblCajero.Text = "Factura " + f.NumeroFormateado + " guardada en: " + ruta;
            }
            catch (Exception ex)
            {
                UI.Aviso("La factura " + f.NumeroFormateado + " se guardó en la base de datos, " +
                         "pero no se pudo crear el archivo .txt:\n" + ex.Message);
            }

            if (System.Configuration.ConfigurationManager.AppSettings["VistaPreviaAlFacturar"] != "false")
                FacturaImpresion.VistaPrevia(f, this);

            NuevaVenta();
        }
    }
}
