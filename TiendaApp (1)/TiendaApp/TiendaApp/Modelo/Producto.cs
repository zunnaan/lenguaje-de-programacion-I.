using System.ComponentModel;

namespace TiendaApp.Modelo
{
    public class Producto
    {
        public int Id { get; set; }                 // ID / codigo
        public string Nombre { get; set; }
        public string Categoria { get; set; }
        public decimal Precio { get; set; }
        public decimal ItbisPct { get; set; }
        public int Stock { get; set; }
        public bool Activo { get; set; }

        [Browsable(false)]
        public string Descripcion
        {
            get { return Id + " - " + Nombre + "  (stock: " + Stock + ")"; }
        }
    }
}
