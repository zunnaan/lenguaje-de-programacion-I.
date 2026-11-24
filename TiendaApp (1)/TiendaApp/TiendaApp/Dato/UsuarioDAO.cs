using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using TiendaApp.Modelo;

namespace TiendaApp.Dato
{
    public class UsuarioDAO
    {
        private const string Cols = "id, usuario, nombre, rol, turno, activo";

        // LOGIN: devuelve el usuario o null si las credenciales no coinciden / esta inactivo.
        public Usuario Login(string usuario, string clave)
        {
            const string sql =
                "SELECT " + Cols + " FROM usuario " +
                "WHERE usuario = @usuario AND clave_hash = HASHBYTES('SHA2_256', @clave) AND activo = 1";

            using (SqlConnection cn = Conexion.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add(Conexion.Texto("@usuario", usuario, 30));
                cmd.Parameters.Add(Conexion.Texto("@clave", clave, 100));
                cn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    return dr.Read() ? Mapear(dr) : null;
                }
            }
        }

        // CREATE: el administrador crea cuentas (Administrador o Cajero con turno).
        public int Create(Usuario u, string clave)
        {
            const string sql =
                "INSERT INTO usuario (usuario, clave_hash, nombre, rol, turno, activo) " +
                "VALUES (@usuario, HASHBYTES('SHA2_256', @clave), @nombre, @rol, @turno, @activo); " +
                "SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (SqlConnection cn = Conexion.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add(Conexion.Texto("@usuario", u.NombreUsuario, 30));
                cmd.Parameters.Add(Conexion.Texto("@clave", clave, 100));
                cmd.Parameters.AddWithValue("@nombre", u.Nombre);
                cmd.Parameters.Add(Conexion.Texto("@rol", u.Rol, 15));
                cmd.Parameters.Add(Conexion.Texto("@turno", u.Turno, 10));
                cmd.Parameters.AddWithValue("@activo", u.Activo);
                cn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        // READ: todas las cuentas.
        public List<Usuario> ReadTodos()
        {
            return Listar("SELECT " + Cols + " FROM usuario ORDER BY rol, nombre");
        }

        // READ: solo cajeros (para filtros de reportes).
        public List<Usuario> ReadCajeros()
        {
            return Listar("SELECT " + Cols + " FROM usuario WHERE rol = 'Cajero' ORDER BY nombre");
        }

        // UPDATE: el rol no se cambia. Si claveNueva viene vacia, se conserva la contrasena actual.
        public bool Update(Usuario u, string claveNueva)
        {
            bool cambiaClave = !string.IsNullOrEmpty(claveNueva);
            string sql =
                "UPDATE usuario SET usuario = @usuario, nombre = @nombre, turno = @turno, activo = @activo" +
                (cambiaClave ? ", clave_hash = HASHBYTES('SHA2_256', @clave)" : "") +
                " WHERE id = @id";

            using (SqlConnection cn = Conexion.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add(Conexion.Texto("@usuario", u.NombreUsuario, 30));
                cmd.Parameters.AddWithValue("@nombre", u.Nombre);
                cmd.Parameters.Add(Conexion.Texto("@turno", u.Turno, 10));
                cmd.Parameters.AddWithValue("@activo", u.Activo);
                cmd.Parameters.AddWithValue("@id", u.Id);
                if (cambiaClave) cmd.Parameters.Add(Conexion.Texto("@clave", claveNueva, 100));
                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        private List<Usuario> Listar(string sql)
        {
            List<Usuario> lista = new List<Usuario>();
            using (SqlConnection cn = Conexion.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read()) lista.Add(Mapear(dr));
                }
            }
            return lista;
        }

        private static Usuario Mapear(SqlDataReader dr)
        {
            return new Usuario
            {
                Id = dr.GetInt32(0),
                NombreUsuario = dr.GetString(1),
                Nombre = dr.GetString(2),
                Rol = dr.GetString(3),
                Turno = dr.IsDBNull(4) ? null : dr.GetString(4),
                Activo = dr.GetBoolean(5)
            };
        }
    }
}
