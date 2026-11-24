using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace TiendaApp.Modelo
{
    public static class TipoFactura
    {
        public const string Contado = "Contado";
        public const string Credito = "Credito";
    }

    public class FacturaDetalle
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal Valor { get; set; }      // precio unitario
        public decimal Itbis { get; set; }      // ITBIS de la linea
        public decimal Total { get; set; }      // cantidad * valor + itbis

        [Browsable(false)]
        public decimal ItbisPct { get; set; }

        [Browsable(false)]
        public decimal Subtotal
        {
            get { return Valor * Cantidad; }
        }

        public void Calcular()
        {
            Itbis = Math.Round(Valor * Cantidad * ItbisPct / 100m, 2);
            Total = Valor * Cantidad + Itbis;
        }
    }

    public class Factura
    {
        public int Id { get; set; }
        public int Numero { get; set; }
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; }
        public int UsuarioId { get; set; }
        public string CajeroNombre { get; set; }
        public string Turno { get; set; }
        public int? PersonaId { get; set; }
        public string ClienteNombre { get; set; }
        public decimal TotalBruto { get; set; }
        public decimal TotalItbis { get; set; }
        public decimal TotalPagar { get; set; }
        public decimal MontoPagado { get; set; }
        public decimal Devuelta { get; set; }
        public List<FacturaDetalle> Detalles { get; set; }

        public Factura()
        {
            Detalles = new List<FacturaDetalle>();
        }

        // Numero de 5 digitos: 00001, 00002, ...
        public string NumeroFormateado
        {
            get { return Numero.ToString("D5"); }
        }
    }
}
