using System;
using System.Configuration;
using System.IO;
using System.Text;
using TiendaApp.Modelo;

namespace TiendaApp
{
    /// <summary>Genera el archivo .txt de la factura (impresion rapida, tecla F5).</summary>
    internal static class FacturaArchivo
    {
        private const int Ancho = 70;

        // Carpeta: App.config "CarpetaFacturas" o, si esta vacia, Documentos\TiendaApp\Facturas.
        public static string Carpeta()
        {
            string c = ConfigurationManager.AppSettings["CarpetaFacturas"];
            if (string.IsNullOrWhiteSpace(c))
                c = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TiendaApp", "Facturas");
            return c;
        }

        /// <summary>Crea Factura_00001.txt y devuelve su ruta completa.</summary>
        public static string GuardarTxt(Factura f)
        {
            string carpeta = Carpeta();
            Directory.CreateDirectory(carpeta);
            string ruta = Path.Combine(carpeta, "Factura_" + f.NumeroFormateado + ".txt");
            File.WriteAllText(ruta, Construir(f), new UTF8Encoding(true));
            return ruta;
        }

        public static string Construir(Factura f)
        {
            string tienda = ConfigurationManager.AppSettings["NombreTienda"] ?? "TIENDA";
            bool credito = f.Tipo == TipoFactura.Credito;
            string linea = new string('=', Ancho);
            string raya = new string('-', Ancho);
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(linea);
            sb.AppendLine(Centrar(tienda));
            sb.AppendLine(Centrar("FACTURA"));
            sb.AppendLine(linea);
            sb.AppendLine(DosColumnas("Factura No.: " + f.NumeroFormateado, "Tipo: " + (credito ? "Crédito" : "Contado")));
            sb.AppendLine(DosColumnas("Fecha: " + f.Fecha.ToString("dd/MM/yyyy"), "Cajero: " + f.CajeroNombre));
            sb.AppendLine(DosColumnas("Hora : " + f.Fecha.ToString("hh:mm:ss tt"),
                string.IsNullOrEmpty(f.Turno) ? "" : "Turno: " + f.Turno));
            sb.AppendLine("Cliente: " + f.ClienteNombre);
            sb.AppendLine(raya);
            sb.AppendLine(Fila("ID", "Producto", "Cant.", "Valor", "ITBIS", "Total"));
            sb.AppendLine(raya);

            foreach (FacturaDetalle d in f.Detalles)
            {
                sb.AppendLine(Fila(d.ProductoId.ToString(), d.NombreProducto, d.Cantidad.ToString(),
                    UI.Dinero(d.Valor), UI.Dinero(d.Itbis), UI.Dinero(d.Total)));
            }

            sb.AppendLine(raya);
            sb.AppendLine(Total("Total bruto:", f.TotalBruto));
            sb.AppendLine(Total("Total ITBIS:", f.TotalItbis));
            sb.AppendLine(Total("TOTAL A PAGAR:", f.TotalPagar));

            if (credito)
            {
                sb.AppendLine();
                sb.AppendLine("Venta a crédito - registrada en cuentas por cobrar.");
            }
            else
            {
                sb.AppendLine(Total("Monto pagado:", f.MontoPagado));
                sb.AppendLine(Total("Devuelta:", f.Devuelta));
            }

            sb.AppendLine(linea);
            sb.AppendLine(Centrar("¡Gracias por su compra!"));
            return sb.ToString();
        }

        private static string Centrar(string texto)
        {
            int izq = Math.Max(0, (Ancho - texto.Length) / 2);
            return new string(' ', izq) + texto;
        }

        private static string DosColumnas(string izq, string der)
        {
            int espacios = Math.Max(1, Ancho - izq.Length - der.Length);
            return izq + new string(' ', espacios) + der;
        }

        // ID(5) Producto(22) Cant(5) Valor(11) ITBIS(10) Total(11), separados por un espacio.
        private static string Fila(string id, string producto, string cant, string valor, string itbis, string total)
        {
            if (producto.Length > 22) producto = producto.Substring(0, 22);
            return id.PadRight(5) + " " + producto.PadRight(22) + " " + cant.PadLeft(5) + " " +
                   valor.PadLeft(11) + " " + itbis.PadLeft(10) + " " + total.PadLeft(11);
        }

        private static string Total(string etiqueta, decimal valor)
        {
            string v = UI.Dinero(valor);
            return etiqueta.PadLeft(Ancho - 16) + v.PadLeft(16);
        }
    }
}
