using System;
using System.Windows.Forms;
using TiendaApp.Modelo;

namespace TiendaApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Login -> menu segun rol -> (cerrar sesion vuelve al login)
            while (true)
            {
                Sesion.Actual = null;

                using (FrmLogin login = new FrmLogin())
                {
                    if (login.ShowDialog() != DialogResult.OK) break;
                }

                using (FrmMenu menu = new FrmMenu())
                {
                    menu.ShowDialog();
                    if (!menu.CerrarSesion) break;
                }
            }
        }
    }
}
