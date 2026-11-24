using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace TiendaApp
{
    /// <summary>Venta al contado: muestra cantidad a pagar, pide el monto pagado y calcula la devuelta.</summary>
    public class FrmPago : Form
    {
        private readonly decimal total;
        private readonly TextBox txtMonto;
        private readonly Label lblDevuelta;

        public decimal MontoPagado { get; private set; }
        public decimal Devuelta { get; private set; }

        public FrmPago(decimal totalAPagar)
        {
            total = totalAPagar;

            Text = "Cobro al contado";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(380, 220);

            Font grande = new Font("Segoe UI", 14f, FontStyle.Bold);

            Label lblTotal = new Label
            {
                Text = UI.Dinero(total),
                Font = grande,
                Left = 190,
                Top = 15,
                Width = 170,
                Height = 30,
                TextAlign = ContentAlignment.MiddleRight
            };

            txtMonto = new TextBox
            {
                Left = 190,
                Top = 66,
                Width = 170,
                Font = grande,
                TextAlign = HorizontalAlignment.Right,
                Text = UI.Dinero(total)
            };
            txtMonto.TextChanged += delegate { Recalcular(); };

            lblDevuelta = new Label
            {
                Font = grande,
                ForeColor = Color.DarkGreen,
                Left = 190,
                Top = 115,
                Width = 170,
                Height = 30,
                TextAlign = ContentAlignment.MiddleRight,
                Text = UI.Dinero(0)
            };

            Button btnAceptar = UI.Btn("Aceptar", 90, 170, 100, btnAceptar_Click);
            Button btnCancelar = UI.Btn("Cancelar", 200, 170, 100, null);
            btnCancelar.DialogResult = DialogResult.Cancel;
            AcceptButton = btnAceptar;
            CancelButton = btnCancelar;

            Controls.AddRange(new Control[]
            {
                UI.Lbl("Cantidad a pagar:", 20, 22), lblTotal,
                UI.Lbl("Monto pagado:", 20, 74), txtMonto,
                UI.Lbl("Devuelta:", 20, 123), lblDevuelta,
                btnAceptar, btnCancelar
            });

            Shown += delegate { txtMonto.Focus(); txtMonto.SelectAll(); };
        }

        private bool LeerMonto(out decimal monto)
        {
            return decimal.TryParse(txtMonto.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out monto);
        }

        private void Recalcular()
        {
            decimal m;
            if (LeerMonto(out m) && m >= total)
                lblDevuelta.Text = UI.Dinero(m - total);
            else
                lblDevuelta.Text = UI.Dinero(0);
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            decimal m;
            if (!LeerMonto(out m))
            {
                UI.Aviso("Escribe un monto válido.");
                return;
            }
            if (m < total)
            {
                UI.Aviso("El monto pagado es menor que la cantidad a pagar (" + UI.Dinero(total) + ").");
                return;
            }

            MontoPagado = m;
            Devuelta = m - total;
            DialogResult = DialogResult.OK;
        }
    }
}
