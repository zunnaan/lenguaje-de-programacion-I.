using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Windows.Forms;
using TiendaApp.Dato;
using TiendaApp.Modelo;

namespace TiendaApp
{
    public partial class Form1 : Form
    {
        private readonly PersonaDAO dao = new PersonaDAO();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Modulo de clientes: solo Administrador (Sesion.Actual es null si se ejecuta aislado).
            if (Sesion.Actual != null && !Sesion.EsAdministrador)
            {
                MessageBox.Show("Acceso restringido: solo el Administrador puede usar este módulo.",
                    "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
                return;
            }
            CargarGrid();
        }

        // ---------- Utilidades ----------

        private void CargarGrid(List<Persona> datos = null)
        {
            try
            {
                dgvPersonas.DataSource = datos ?? dao.Read();
            }
            catch (SqlException ex)
            {
                MessageBox.Show(
                    "No se pudo conectar con la base de datos.\n" +
                    "Revisa el Server en App.config y que tienda_db exista.\n\nDetalle: " + ex.Message,
                    "Error de base de datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los datos: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LimpiarCampos()
        {
            txtId.Clear();
            txtNombre.Clear();
            txtApellido.Clear();
            txtCorreo.Clear();
            txtNombre.Focus();
            dgvPersonas.ClearSelection();
        }

        private static bool CorreoValido(string correo)
        {
            try
            {
                return new MailAddress(correo).Address == correo;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        // Valida los campos y devuelve la persona armada (o null si hay error).
        private Persona LeerFormulario()
        {
            string nombre = txtNombre.Text.Trim();
            string apellido = txtApellido.Text.Trim();
            string correo = txtCorreo.Text.Trim();

            if (nombre == "" || apellido == "" || correo == "")
            {
                MessageBox.Show("Nombre, apellido y correo son obligatorios.",
                    "Datos incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }
            if (!CorreoValido(correo))
            {
                MessageBox.Show("El correo no tiene un formato valido.",
                    "Correo invalido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCorreo.Focus();
                return null;
            }

            int id;
            int.TryParse(txtId.Text, out id);
            return new Persona { Id = id, Nombre = nombre, Apellido = apellido, Correo = correo };
        }

        // ---------- CRUD ----------

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            Persona p = LeerFormulario();
            if (p == null) return;

            if (p.Id != 0)
            {
                MessageBox.Show("Hay un registro cargado. Usa Actualizar, o pulsa Limpiar para crear uno nuevo.",
                    "Informacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                int nuevoId = dao.Create(p);
                MessageBox.Show("Persona guardada con ID " + nuevoId + ".",
                    "Exito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarCampos();
                CargarGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo guardar: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnActualizar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtId.Text))
            {
                MessageBox.Show("Haz doble clic en una fila de la tabla para cargarla y poder actualizarla.",
                    "Sin seleccion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Persona p = LeerFormulario();
            if (p == null) return;

            try
            {
                if (dao.Update(p))
                {
                    MessageBox.Show("Registro actualizado.",
                        "Exito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LimpiarCampos();
                    CargarGrid();
                }
                else
                {
                    MessageBox.Show("No se encontro el registro (quiza fue eliminado).",
                        "Sin cambios", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    CargarGrid();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo actualizar: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            int id;
            if (!int.TryParse(txtId.Text, out id))
            {
                MessageBox.Show("Haz doble clic en una fila de la tabla para cargarla y poder eliminarla.",
                    "Sin seleccion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult r = MessageBox.Show(
                "¿Seguro que deseas eliminar a " + txtNombre.Text + " " + txtApellido.Text + "?",
                "Confirmar eliminacion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r != DialogResult.Yes) return;

            try
            {
                if (dao.Delete(id))
                {
                    MessageBox.Show("Registro eliminado.",
                        "Exito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("No se encontro el registro.",
                        "Sin cambios", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                LimpiarCampos();
                CargarGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo eliminar: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarCampos();
            txtBuscarId.Clear();
            txtBuscarNombre.Clear();
            CargarGrid();
        }

        // ---------- Busquedas ----------

        private void btnBuscarId_Click(object sender, EventArgs e)
        {
            int id;
            if (!int.TryParse(txtBuscarId.Text.Trim(), out id))
            {
                MessageBox.Show("Escribe un ID numerico valido.",
                    "ID invalido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBuscarId.Focus();
                return;
            }

            try
            {
                List<Persona> res = dao.BuscarPorId(id);
                if (res.Count == 0)
                    MessageBox.Show("No existe ninguna persona con ID " + id + ".",
                        "Sin resultados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarGrid(res);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en la busqueda: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBuscarNombre_Click(object sender, EventArgs e)
        {
            string texto = txtBuscarNombre.Text.Trim();
            if (texto == "")
            {
                MessageBox.Show("Escribe parte del nombre a buscar.",
                    "Texto vacio", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBuscarNombre.Focus();
                return;
            }

            try
            {
                List<Persona> res = dao.BuscarPorNombre(texto);
                if (res.Count == 0)
                    MessageBox.Show("No hay nombres que coincidan con \"" + texto + "\".",
                        "Sin resultados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarGrid(res);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en la busqueda: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnMostrarTodos_Click(object sender, EventArgs e)
        {
            txtBuscarId.Clear();
            txtBuscarNombre.Clear();
            CargarGrid();
        }

        // ---------- DataGridView ----------

        private void dgvPersonas_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return; // clic en el encabezado

            Persona p = dgvPersonas.Rows[e.RowIndex].DataBoundItem as Persona;
            if (p == null) return;

            txtId.Text = p.Id.ToString();
            txtNombre.Text = p.Nombre;
            txtApellido.Text = p.Apellido;
            txtCorreo.Text = p.Correo;
        }
    }
}
