using System.ComponentModel;

namespace TiendaApp.Modelo
{
    /// <summary>
    /// Entidad que mapea la tabla dbo.persona (en el sistema de facturacion se usa como cliente).
    /// </summary>
    public class Persona
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Correo { get; set; }

        [Browsable(false)]
        public string NombreCompleto
        {
            get { return Nombre + " " + Apellido; }
        }
    }
}
