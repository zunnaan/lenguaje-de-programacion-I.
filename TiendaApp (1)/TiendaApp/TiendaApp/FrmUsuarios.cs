using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using TiendaApp.Dato;
using TiendaApp.Modelo;

namespace TiendaApp
{
    /// <summary>Administracion de cuentas: el administrador crea administradores y cajeros (con turno).</summary>
    public class FrmUsuarios : Form
    {
        private readonly UsuarioDAO dao = new UsuarioDAO();
        private readonly TextBox txtUsuario, txtNombre, txtClave;
        private readonly ComboBox cboRol, cboTurno;
        private readonly Label lblTurno;
        private readonly CheckBox chkActivo;
        private readonly DataGridView dgv;
        private int idSel = 0;

        public FrmUsuarios()
        {
            Text = "Usuarios y cajeros";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(820, 520);
            MinimumSize = new Size(836, 559);
            UI.SoloAdministrador(this);

            txtUsuario = UI.Txt(100, 15, 170, 30);
            txtNombre = UI.Txt(440, 15, 340, 100);
            txtClave = UI.Txt(100, 48, 170, 100);
            txtClave.UseSystemPasswordChar = true;

            cboRol = new ComboBox { Left = 440, Top = 48, Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
            cboRol.Items.AddRange(new object[] { Roles.Cajero, Roles.Administrador });
            cboRol.SelectedIndex = 0;
            cboRol.SelectedIndexChanged += delegate { ActualizarTurnoHabilitado(); };

            lblTurno = UI.Lbl("Turno:", 590, 48);
            cboTurno = new ComboBox { Left = 640, Top = 48, Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            cboTurno.Items.AddRange(Turnos.Todos);
            cboTurno.SelectedIndex = 0;

            chkActivo = new CheckBox { Text = "Activo", Left = 400, Top = 93, AutoSize = true, Checked = true };

            Label nota = UI.Lbl("Al actualizar, deja la contraseña vacía para conservar la actual. El rol no se puede cambiar.", 15, 130);
            nota.ForeColor = SystemColors.GrayText;

            dgv = UI.Grid(12, 160, 796, 348);
            dgv.CellDoubleClick += dgv_CellDoubleClick;

            Controls.AddRange(new Control[]
            {
                UI.Lbl("Usuario:", 15, 15), txtUsuario,
                UI.Lbl("Nombre:", 370, 15), txtNombre,
                UI.Lbl("Contraseña:", 15, 48), txtClave,
                UI.Lbl("Rol:", 370, 48), cboRol, lblTurno, cboTurno,
                UI.Btn("Guardar", 15, 90, 110, btnGuardar_Click),
                UI.Btn("Actualizar", 135, 90, 110, btnActualizar_Click),
                UI.Btn("Limpiar", 255, 90, 110, btnLimpiar_Click),
                chkActivo, nota, dgv
            });

            Load += delegate { CargarGrid(); };
        }

        private void ActualizarTurnoHabilitado()
        {
            bool esCajero = (string)cboRol.SelectedItem == Roles.Cajero;
            cboTurno.Enabled = esCajero;
            lblTurno.Enabled = esCajero;
        }

        private void CargarGrid()
        {
            try
            {
                dgv.DataSource = dao.ReadTodos();
                Encabezado("Id", "ID");
                Encabezado("NombreUsuario", "Usuario");
            }
            catch (Exception ex)
            {
                UI.Error("No se pudieron cargar los usuarios: " + ex.Message);
            }
        }

        private void Encabezado(string columna, string texto)
        {
            if (dgv.Columns[columna] != null) dgv.Columns[columna].HeaderText = texto;
        }

        private void Limpiar()
        {
            idSel = 0;
            txtUsuario.Clear();
            txtNombre.Clear();
            txtClave.Clear();
            cboRol.Enabled = true;
            cboRol.SelectedIndex = 0;
            cboTurno.SelectedIndex = 0;
            chkActivo.Checked = true;
            ActualizarTurnoHabilitado();
            dgv.ClearSelection();
            txtUsuario.Focus();
        }

        private Usuario Leer()
        {
            string usuario = txtUsuario.Text.Trim();
            string nombre = txtNombre.Text.Trim();
            if (usuario == "" || nombre == "")
            {
                UI.Aviso("Usuario y nombre son obligatorios.");
                return null;
            }
            string rol = (string)cboRol.SelectedItem;
            return new Usuario
            {
                Id = idSel,
                NombreUsuario = usuario,
                Nombre = nombre,
                Rol = rol,
                Turno = rol == Roles.Cajero ? (string)cboTurno.SelectedItem : null,
                Activo = chkActivo.Checked
            };
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (idSel != 0)
            {
                UI.Info("Hay un usuario cargado. Usa Actualizar, o pulsa Limpiar para crear uno nuevo.");
                return;
            }
            Usuario u = Leer();
            if (u == null) return;
            if (txtClave.Text.Length < 4)
            {
                UI.Aviso("La contraseña debe tener al menos 4 caracteres.");
                return;
            }

            try
            {
                dao.Create(u, txtClave.Text);
                UI.Info("Cuenta creada: " + u.Nombre + " (" + u.Rol +
                        (u.Turno == null ? "" : ", turno " + u.Turno) + ").");
                Limpiar();
                CargarGrid();
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                    UI.Aviso("Ya existe un usuario con ese nombre de usuario.");
                else
                    UI.Error("No se pudo guardar: " + ex.Message);
            }
            catch (Exception ex)
            {
                UI.Error("No se pudo guardar: " + ex.Message);
            }
        }

        private void btnActualizar_Click(object sender, EventArgs e)
        {
            if (idSel == 0)
            {
                UI.Info("Haz doble clic en un usuario de la tabla para editarlo.");
                return;
            }
            Usuario u = Leer();
            if (u == null) return;
            if (txtClave.Text != "" && txtClave.Text.Length < 4)
            {
                UI.Aviso("La nueva contraseña debe tener al menos 4 caracteres.");
                return;
            }
            if (Sesion.Actual != null && idSel == Sesion.Actual.Id && !u.Activo)
            {
                UI.Aviso("No puedes desactivar tu propia cuenta mientras tienes la sesión abierta.");
                return;
            }

            try
            {
                if (dao.Update(u, txtClave.Text))
                {
                    UI.Info("Cuenta actualizada.");
                    Limpiar();
                    CargarGrid();
                }
                else
                {
                    UI.Aviso("No se encontró el usuario.");
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                    UI.Aviso("Ya existe un usuario con ese nombre de usuario.");
                else
                    UI.Error("No se pudo actualizar: " + ex.Message);
            }
            catch (Exception ex)
            {
                UI.Error("No se pudo actualizar: " + ex.Message);
            }
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            Limpiar();
        }

        private void dgv_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            Usuario u = dgv.Rows[e.RowIndex].DataBoundItem as Usuario;
            if (u == null) return;

            idSel = u.Id;
            txtUsuario.Text = u.NombreUsuario;
            txtNombre.Text = u.Nombre;
            txtClave.Clear();
            cboRol.SelectedItem = u.Rol;
            cboRol.Enabled = false;
            if (u.Turno != null) cboTurno.SelectedItem = u.Turno;
            chkActivo.Checked = u.Activo;
            ActualizarTurnoHabilitado();
        }
    }
}
