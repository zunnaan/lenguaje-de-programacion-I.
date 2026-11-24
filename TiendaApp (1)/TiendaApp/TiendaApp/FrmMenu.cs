using System;
using System.Drawing;
using System.Windows.Forms;
using TiendaApp.Modelo;

namespace TiendaApp
{
    public class FrmMenu : Form
    {
        private int y = 85;

        /// <summary>true si el usuario pulso "Cerrar sesión" (vuelve al login); false si cerro la ventana.</summary>
        public bool CerrarSesion { get; private set; }

        public FrmMenu()
        {
            Usuario u = Sesion.Actual;

            Text = "Tienda - Menú principal";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            Controls.Add(new Label
            {
                Text = "Bienvenido, " + u.Nombre,
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                Left = 20,
                Top = 15,
                AutoSize = true
            });
            Controls.Add(new Label
            {
                Text = u.Rol + (string.IsNullOrEmpty(u.Turno) ? "" : "  -  Turno " + u.Turno),
                Left = 22,
                Top = 48,
                AutoSize = true
            });

            // Control de permisos: el Administrador ve todos los modulos;
            // el Cajero solo tiene la facturacion de su turno.
            AgregarBoton("Nueva venta (facturar)", delegate { Abrir(new FrmFacturacion()); });

            if (Sesion.EsAdministrador)
            {
                AgregarBoton("Usuarios y cajeros (turnos)", delegate { Abrir(new FrmUsuarios()); });
                AgregarBoton("Productos / inventario", delegate { Abrir(new FrmProductos()); });
                AgregarBoton("Clientes", delegate { Abrir(new Form1()); });
                AgregarBoton("Reportes", delegate { Abrir(new FrmReportes()); });
            }

            y += 10;
            AgregarBoton("Cerrar sesión", delegate { CerrarSesion = true; Close(); });

            ClientSize = new Size(340, y + 15);
        }

        private void AgregarBoton(string texto, Action accion)
        {
            Button b = UI.Btn(texto, 40, y, 260, delegate { accion(); });
            b.Height = 38;
            Controls.Add(b);
            y += 48;
        }

        private void Abrir(Form f)
        {
            using (f)
            {
                f.ShowDialog(this);
            }
        }
    }
}
