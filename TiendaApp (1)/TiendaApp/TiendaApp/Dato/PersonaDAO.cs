using System.Collections.Generic;
using System.Data.SqlClient;
using TiendaApp.Modelo;

namespace TiendaApp.Dato
{
    /// <summary>
    /// Acceso a datos de la tabla persona (CRUD + busquedas).
    /// Todas las consultas son parametrizadas. Las excepciones se propagan
    /// para que el formulario las muestre con MessageBox.
    /// </summary>
    public class PersonaDAO
    {
        // CREATE: inserta y devuelve el id generado.
        public int Create(Persona p)
        {
            const string sql =
                "INSERT INTO persona (nombre, apellido, correo) " +
                "VALUES (@nombre, @apellido, @correo); " +
                "SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (SqlConnection cn = Conexion.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@nombre", p.Nombre);
                cmd.Parameters.AddWithValue("@apellido", p.Apellido);
                cmd.Parameters.AddWithValue("@correo", p.Correo);
                cn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        // READ: lista todos los registros.
        public List<Persona> Read()
        {
            return Consultar("SELECT id, nombre, apellido, correo FROM persona ORDER BY id", null, null);
        }

        // UPDATE: devuelve true si se modifico una fila.
        public bool Update(Persona p)
        {
            const string sql =
                "UPDATE persona SET nombre = @nombre, apellido = @apellido, correo = @correo " +
                "WHERE id = @id";

            using (SqlConnection cn = Conexion.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@nombre", p.Nombre);
                cmd.Parameters.AddWithValue("@apellido", p.Apellido);
                cmd.Parameters.AddWithValue("@correo", p.Correo);
                cmd.Parameters.AddWithValue("@id", p.Id);
                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // DELETE: elimina por id; devuelve true si se borro una fila.
        public bool Delete(int id)
        {
            const string sql = "DELETE FROM persona WHERE id = @id";

            using (SqlConnection cn = Conexion.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // BUSQUEDA por ID numerico (0 o 1 resultado).
        public List<Persona> BuscarPorId(int id)
        {
            return Consultar(
                "SELECT id, nombre, apellido, correo FROM persona WHERE id = @id",
                "@id", id);
        }

        // BUSQUEDA por coincidencia de texto en el nombre.
        public List<Persona> BuscarPorNombre(string texto)
        {
            return Consultar(
                "SELECT id, nombre, apellido, correo FROM persona " +
                "WHERE nombre LIKE @texto ORDER BY nombre",
                "@texto", "%" + texto + "%");
        }

        // Ejecuta un SELECT con 0 o 1 parametro y mapea a List<Persona>.
        private List<Persona> Consultar(string sql, string paramNombre, object paramValor)
        {
            List<Persona> lista = new List<Persona>();

            using (SqlConnection cn = Conexion.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                if (paramNombre != null)
                    cmd.Parameters.AddWithValue(paramNombre, paramValor);

                cn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new Persona
                        {
                            Id = dr.GetInt32(0),
                            Nombre = dr.GetString(1),
                            Apellido = dr.GetString(2),
                            Correo = dr.GetString(3)
                        });
                    }
                }
            }
            return lista;
        }
    }
}
