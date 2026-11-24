using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using TiendaApp.Dato;
using TiendaApp.Modelo;

namespace TiendaApp
{
    public class FrmLogin : Form
    {
        private readonly TextBox txtUsuario;
        private readonly TextBox txtClave;

        public FrmLogin()
        {
            Text = "colmado doña petronila (no fio)";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(340, 195);

            Label titulo = new Label
            {
                Text = "Iniciar sesión",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                Left = 20,
                Top = 15,
                AutoSize = true
            };

            txtUsuario = UI.Txt(110, 60, 200, 30);
            txtClave = UI.Txt(110, 95, 200, 100);
            txtClave.UseSystemPasswordChar = true;

            Button btnIngresar = UI.Btn("Ingresar", 110, 140, 95, btnIngresar_Click);
            Button btnSalir = UI.Btn("Salir", 215, 140, 95, null);
            btnSalir.DialogResult = DialogResult.Cancel;

            AcceptButton = btnIngresar;
            CancelButton = btnSalir;

            Controls.AddRange(new Control[]
            {
                titulo,
                UI.Lbl("Usuario:", 20, 60), txtUsuario,
                UI.Lbl("Contraseña:", 20, 95), txtClave,
                btnIngresar, btnSalir
            });
        }

        private void btnIngresar_Click(object sender, EventArgs e)
        {
            string usuario = txtUsuario.Text.Trim();
            string clave = txtClave.Text;

            if (usuario == "" || clave == "")
            {
                UI.Aviso("Escribe tu usuario y contraseña.");
                return;
            }

            try
            {
                Usuario u = new UsuarioDAO().Login(usuario, clave);
                if (u == null)
                {
                    UI.Aviso("Usuario o contraseña incorrectos, o el usuario está inactivo.");
                    txtClave.Clear();
                    txtClave.Focus();
                    return;
                }

                bool validarTurno = ConfigurationManager.AppSettings["ValidarTurnoPorHora"] == "true";
                if (u.Rol == Roles.Cajero && validarTurno && !Turnos.EnHorario(u.Turno, DateTime.Now))
                {
                    UI.Aviso("Tu turno (" + u.Turno + ") no está activo en este momento.\n" + Turnos.Horario(u.Turno));
                    return;
                }

                Sesion.Actual = u;
                DialogResult = DialogResult.OK;
            }
            catch (SqlException ex)
            {
                UI.Error("No se pudo conectar con la base de datos.\n" +
                         "Revisa el Server en App.config y que tienda_db exista (ejecuta tienda_db.sql).\n\nDetalle: " + ex.Message);
            }
            catch (Exception ex)
            {
                UI.Error("Error al iniciar sesión: " + ex.Message);
            }
        }
    }
}
