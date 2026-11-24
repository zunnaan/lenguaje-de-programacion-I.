using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using TiendaApp.Modelo;

namespace TiendaApp
{
    /// <summary>Muestra la vista previa de impresion de la factura (desde ahi se imprime).</summary>
    internal static class FacturaImpresion
    {
        public static void VistaPrevia(Factura f, IWin32Window owner)
        {
            try
            {
                using (FacturaDocumento doc = new FacturaDocumento(f))
                using (PrintPreviewDialog dlg = new PrintPreviewDialog())
                {
                    dlg.Document = doc;
                    dlg.Text = "Factura " + f.NumeroFormateado + " - usa el icono de impresora para imprimir";
                    dlg.WindowState = FormWindowState.Maximized;
                    dlg.ShowDialog(owner);
                }
            }
            catch (InvalidPrinterException)
            {
                UI.Aviso("Windows no tiene ninguna impresora instalada. La factura " + f.NumeroFormateado +
                         " ya quedó guardada; instala una impresora (puede ser \"Microsoft Print to PDF\") para imprimirla.");
            }
        }
    }

    internal class FacturaDocumento : PrintDocument
    {
        private readonly Factura f;
        private int indice;
        private int pagina;

        public FacturaDocumento(Factura factura)
        {
            f = factura;
            DocumentName = "Factura " + f.NumeroFormateado;
        }

        protected override void OnBeginPrint(PrintEventArgs e)
        {
            indice = 0;
            pagina = 0;
            base.OnBeginPrint(e);
        }

        protected override void OnPrintPage(PrintPageEventArgs e)
        {
            base.OnPrintPage(e);

            Graphics g = e.Graphics;
            Rectangle m = e.MarginBounds;
            float y = m.Top;
            string tienda = ConfigurationManager.AppSettings["NombreTienda"] ?? "TIENDA";
            bool credito = f.Tipo == TipoFactura.Credito;

            using (Font fTitulo = new Font("Arial", 18f, FontStyle.Bold))
            using (Font fSub = new Font("Arial", 12f, FontStyle.Bold))
            using (Font fNorm = new Font("Arial", 10f))
            using (Font fNeg = new Font("Arial", 10f, FontStyle.Bold))
            using (Font fTotal = new Font("Arial", 12f, FontStyle.Bold))
            using (StringFormat centro = new StringFormat { Alignment = StringAlignment.Center })
            using (StringFormat der = new StringFormat { Alignment = StringAlignment.Far })
            using (StringFormat izq = new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap })
            {
                // ----- Encabezado -----
                if (pagina == 0)
                {
                    g.DrawString(tienda, fTitulo, Brushes.Black, new RectangleF(m.Left, y, m.Width, 34), centro);
                    y += 34;
                    g.DrawString("FACTURA", fSub, Brushes.Black, new RectangleF(m.Left, y, m.Width, 26), centro);
                    y += 36;

                    g.DrawString("Factura No.: " + f.NumeroFormateado, fNeg, Brushes.Black, m.Left, y);
                    g.DrawString("Tipo: " + (credito ? "Crédito" : "Contado"), fNeg, Brushes.Black,
                        new RectangleF(m.Left, y, m.Width, 20), der);
                    y += 22;

                    g.DrawString("Fecha: " + f.Fecha.ToString("dd/MM/yyyy"), fNorm, Brushes.Black, m.Left, y);
                    g.DrawString("Cajero: " + f.CajeroNombre, fNorm, Brushes.Black,
                        new RectangleF(m.Left, y, m.Width, 20), der);
                    y += 20;

                    g.DrawString("Hora: " + f.Fecha.ToString("hh:mm:ss tt"), fNorm, Brushes.Black, m.Left, y);
                    if (!string.IsNullOrEmpty(f.Turno))
                        g.DrawString("Turno: " + f.Turno, fNorm, Brushes.Black,
                            new RectangleF(m.Left, y, m.Width, 20), der);
                    y += 20;

                    g.DrawString("Cliente: " + f.ClienteNombre, fNorm, Brushes.Black, m.Left, y);
                    y += 30;
                }
                else
                {
                    g.DrawString("Factura No. " + f.NumeroFormateado + " (continuación)", fNeg, Brushes.Black, m.Left, y);
                    y += 30;
                }

                // ----- Columnas -----
                float[] fracciones = { 0.08f, 0.36f, 0.10f, 0.15f, 0.14f, 0.17f };
                string[] titulos = { "ID", "Producto", "Cant.", "Valor", "ITBIS", "Total" };
                float[] xs = new float[6];
                float[] ws = new float[6];
                float acc = m.Left;
                for (int i = 0; i < 6; i++)
                {
                    xs[i] = acc;
                    ws[i] = m.Width * fracciones[i] - 4;
                    acc += m.Width * fracciones[i];
                }

                for (int i = 0; i < 6; i++)
                {
                    StringFormat fmt = (i <= 1) ? izq : der;
                    g.DrawString(titulos[i], fNeg, Brushes.Black, new RectangleF(xs[i], y, ws[i], 20), fmt);
                }
                y += 22;
                g.DrawLine(Pens.Black, m.Left, y, m.Right, y);
                y += 4;

                // ----- Lineas -----
                float limite = m.Bottom - 160;   // se reserva espacio para los totales
                while (indice < f.Detalles.Count && y + 20 <= limite)
                {
                    FacturaDetalle d = f.Detalles[indice];
                    string[] celdas =
                    {
                        d.ProductoId.ToString(),
                        d.NombreProducto,
                        d.Cantidad.ToString(),
                        UI.Dinero(d.Valor),
                        UI.Dinero(d.Itbis),
                        UI.Dinero(d.Total)
                    };
                    for (int i = 0; i < 6; i++)
                    {
                        StringFormat fmt = (i <= 1) ? izq : der;
                        g.DrawString(celdas[i], fNorm, Brushes.Black, new RectangleF(xs[i], y, ws[i], 20), fmt);
                    }
                    y += 20;
                    indice++;
                }

                if (indice < f.Detalles.Count)
                {
                    e.HasMorePages = true;
                    pagina++;
                    return;
                }

                // ----- Totales -----
                y += 4;
                g.DrawLine(Pens.Black, m.Left, y, m.Right, y);
                y += 8;

                float xEtiqueta = m.Left + m.Width * 0.45f;
                float wEtiqueta = m.Width * 0.30f;
                float xValor = m.Left + m.Width * 0.75f;
                float wValor = m.Width * 0.25f - 4;

                y = FilaTotal(g, fNorm, der, "Total bruto:", f.TotalBruto, xEtiqueta, wEtiqueta, xValor, wValor, y);
                y = FilaTotal(g, fNorm, der, "Total ITBIS:", f.TotalItbis, xEtiqueta, wEtiqueta, xValor, wValor, y);
                y = FilaTotal(g, fTotal, der, "TOTAL A PAGAR:", f.TotalPagar, xEtiqueta, wEtiqueta, xValor, wValor, y + 2);

                if (!credito)
                {
                    y += 4;
                    y = FilaTotal(g, fNorm, der, "Monto pagado:", f.MontoPagado, xEtiqueta, wEtiqueta, xValor, wValor, y);
                    y = FilaTotal(g, fNorm, der, "Devuelta:", f.Devuelta, xEtiqueta, wEtiqueta, xValor, wValor, y);
                }
                else
                {
                    y += 8;
                    g.DrawString("Venta a crédito - registrada en cuentas por cobrar.", fNeg, Brushes.Black,
                        new RectangleF(m.Left, y, m.Width, 20), der);
                    y += 22;
                }

                y += 14;
                g.DrawString("¡Gracias por su compra!", fNeg, Brushes.Black, new RectangleF(m.Left, y, m.Width, 22), centro);
            }

            e.HasMorePages = false;
        }

        private static float FilaTotal(Graphics g, Font fuente, StringFormat der, string etiqueta, decimal valor,
            float xEtiqueta, float wEtiqueta, float xValor, float wValor, float y)
        {
            g.DrawString(etiqueta, fuente, Brushes.Black, new RectangleF(xEtiqueta, y, wEtiqueta, 22), der);
            g.DrawString(UI.Dinero(valor), fuente, Brushes.Black, new RectangleF(xValor, y, wValor, 22), der);
            return y + 22;
        }
    }
}
