using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using TiendaApp.Dato;
using TiendaApp.Modelo;

namespace TiendaApp
{
    /// <summary>Reportes: cuadre del dia, por cajero, inventario, facturas credito/contado, top 5 clientes y cuentas por cobrar.</summary>
    public class FrmReportes : Form
    {
        private readonly ReporteDAO dao = new ReporteDAO();
        private readonly UsuarioDAO usuarioDao = new UsuarioDAO();
        private readonly ComboBox cboReporte, cboTipo, cboCajero, cboTurno;
        private readonly DateTimePicker dtpDesde, dtpHasta;
        private readonly Label lblDesde, lblHasta, lblTipo, lblCajero, lblTurno, lblResumen;
        private readonly DataGridView dgv;

        public FrmReportes()
        {
            Text = "Reportes";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(960, 600);
            MinimumSize = new Size(976, 639);
            UI.SoloAdministrador(this);

            cboReporte = new ComboBox { Left = 75, Top = 12, Width = 260, DropDownStyle = ComboBoxStyle.DropDownList };
            cboReporte.Items.AddRange(new object[]
            {
                "Cuadre del día",
                "Cuadre por cajero y turno",
                "Inventario en stock",
                "Facturas (crédito / contado)",
                "Top 5 clientes con más ventas",
                "Cuentas por cobrar"
            });
            cboReporte.SelectedIndex = 0;
            cboReporte.SelectedIndexChanged += delegate { AjustarControles(); };

            lblDesde = UI.Lbl("Desde:", 355, 12);
            dtpDesde = new DateTimePicker { Left = 402, Top = 12, Width = 110, Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            lblHasta = UI.Lbl("Hasta:", 525, 12);
            dtpHasta = new DateTimePicker { Left = 570, Top = 12, Width = 110, Format = DateTimePickerFormat.Short, Value = DateTime.Today };

            lblTipo = UI.Lbl("Tipo:", 695, 12);
            cboTipo = new ComboBox { Left = 735, Top = 12, Width = 110, DropDownStyle = ComboBoxStyle.DropDownList };
            cboTipo.Items.AddRange(new object[] { "(Todas)", "Contado", "Crédito" });
            cboTipo.SelectedIndex = 0;

            lblCajero = UI.Lbl("Cajero:", 12, 46);
            cboCajero = new ComboBox { Left = 75, Top = 46, Width = 260, DropDownStyle = ComboBoxStyle.DropDownList };

            lblTurno = UI.Lbl("Turno:", 355, 46);
            cboTurno = new ComboBox { Left = 402, Top = 46, Width = 110, DropDownStyle = ComboBoxStyle.DropDownList };
            cboTurno.Items.Add("(Todos)");
            cboTurno.Items.AddRange(Turnos.Todos);
            cboTurno.SelectedIndex = 0;

            Button btnGenerar = UI.Btn("Generar reporte", 530, 43, 140, delegate { Generar(); });

            lblResumen = new Label
            {
                Left = 12,
                Top = 565,
                Width = 936,
                Height = 24,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            dgv = UI.Grid(12, 82, 936, 475);

            Controls.AddRange(new Control[]
            {
                UI.Lbl("Reporte:", 12, 12), cboReporte,
                lblDesde, dtpDesde, lblHasta, dtpHasta, lblTipo, cboTipo,
                lblCajero, cboCajero, lblTurno, cboTurno, btnGenerar, dgv, lblResumen
            });

            Load += delegate
            {
                CargarCajeros();
                AjustarControles();
            };
        }

        private void CargarCajeros()
        {
            try
            {
                List<Usuario> lista = usuarioDao.ReadCajeros();
                lista.Insert(0, new Usuario { Id = 0, Nombre = "(Todos los cajeros)" });
                cboCajero.DataSource = lista;
                cboCajero.DisplayMember = "Nombre";
                cboCajero.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                UI.Error("No se pudo cargar la lista de cajeros: " + ex.Message);
            }
        }

        // Muestra solo los filtros que aplican a cada reporte.
        private void AjustarControles()
        {
            int r = cboReporte.SelectedIndex;
            bool usaFechas = r != 2 && r != 5;
            lblDesde.Visible = dtpDesde.Visible = usaFechas;
            lblHasta.Visible = dtpHasta.Visible = usaFechas;
            lblTipo.Visible = cboTipo.Visible = (r == 3);
            lblCajero.Visible = cboCajero.Visible = (r == 1);
            lblTurno.Visible = cboTurno.Visible = (r == 1);
            lblTurno.Visible = cboTurno.Visible = (r == 1);
            dgv.DataSource = null;
            lblResumen.Text = "";
        }

        private void Generar()
        {
            DateTime d = dtpDesde.Value.Date;
            DateTime h = dtpHasta.Value.Date;
            if (h < d)
            {
                UI.Aviso("La fecha \"Hasta\" no puede ser anterior a \"Desde\".");
                return;
            }

            try
            {
                DataTable dt;
                string resumen = "";

                switch (cboReporte.SelectedIndex)
                {
                    case 0:
                        {
                            dt = dao.Cuadre(d, h);
                            resumen = ResumenCuadre(dt);
                            AgregarTotal(dt, "Tipo", "Facturas", "Total bruto", "ITBIS", "Total");
                            break;
                        }
                    case 1:
                        {
                            Usuario c = cboCajero.SelectedItem as Usuario;
                            int? uid = (c != null && c.Id != 0) ? c.Id : (int?)null;
                            string turno = cboTurno.SelectedIndex > 0 ? (string)cboTurno.SelectedItem : null;
                            dt = dao.PorCajero(d, h, uid, turno);
                            AgregarTotal(dt, "Cajero", "Facturas", "Contado", "Crédito", "Total");
                            break;
                        }
                    case 2:
                        {
                            dt = dao.Inventario();
                            object valor = dt.Rows.Count > 0 ? dt.Compute("SUM([Valor en stock])", "") : 0m;
                            resumen = "Valor total del inventario: " + UI.Dinero(Convert.ToDecimal(valor == DBNull.Value ? 0m : valor));
                            AgregarTotal(dt, "Producto", "Stock", "Valor en stock");
                            break;
                        }
                    case 3:
                        {
                            string tipo = null;
                            if (cboTipo.SelectedIndex == 1) tipo = "Contado";
                            else if (cboTipo.SelectedIndex == 2) tipo = "Credito";
                            dt = dao.Facturas(d, h, tipo);
                            AgregarTotal(dt, "No.", "Bruto", "ITBIS", "Total");
                            break;
                        }
                    case 4:
                        {
                            dt = dao.TopClientes(d, h);
                            break;
                        }
                    default:
                        {
                            dt = dao.CuentasPorCobrar();
                            AgregarTotal(dt, "Factura", "Monto", "Saldo");
                            break;
                        }
                }

                dgv.DataSource = dt;
                FormatearColumnas();
                lblResumen.Text = dt.Rows.Count == 0 ? "Sin resultados para los filtros seleccionados." : resumen;
            }
            catch (Exception ex)
            {
                UI.Error("No se pudo generar el reporte: " + ex.Message);
            }
        }

        private static string ResumenCuadre(DataTable dt)
        {
            decimal contado = 0, credito = 0;
            foreach (DataRow r in dt.Rows)
            {
                string tipo = Convert.ToString(r["Tipo"]);
                decimal total = Convert.ToDecimal(r["Total"]);
                if (tipo == "Contado") contado = total;
                else if (tipo == "Credito") credito = total;
            }
            return "Efectivo esperado en caja (contado): " + UI.Dinero(contado) +
                   "     A crédito (por cobrar): " + UI.Dinero(credito) +
                   "     Total vendido: " + UI.Dinero(contado + credito);
        }

        // Agrega una fila TOTAL sumando las columnas indicadas.
        private static void AgregarTotal(DataTable dt, string columnaEtiqueta, params string[] columnasSuma)
        {
            if (dt.Rows.Count == 0) return;
            DataRow fila = dt.NewRow();
            fila[columnaEtiqueta] = "TOTAL";
            foreach (string c in columnasSuma)
                fila[c] = dt.Compute("SUM([" + c + "])", "");
            dt.Rows.Add(fila);
        }

        private void FormatearColumnas()
        {
            foreach (DataGridViewColumn col in dgv.Columns)
            {
                if (col.ValueType == typeof(decimal))
                {
                    col.DefaultCellStyle.Format = "N2";
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                else if (col.ValueType == typeof(DateTime))
                {
                    col.DefaultCellStyle.Format = "dd/MM/yyyy hh:mm tt";
                }
            }

            // Resalta la fila TOTAL (ultima fila, si existe)
            if (dgv.Rows.Count > 0)
            {
                DataGridViewRow ultima = dgv.Rows[dgv.Rows.Count - 1];
                foreach (DataGridViewCell celda in ultima.Cells)
                {
                    if (Convert.ToString(celda.Value) == "TOTAL")
                    {
                        ultima.DefaultCellStyle.Font = new Font(dgv.Font, FontStyle.Bold);
                        ultima.DefaultCellStyle.BackColor = Color.WhiteSmoke;
                        break;
                    }
                }
            }
        }
    }
}
