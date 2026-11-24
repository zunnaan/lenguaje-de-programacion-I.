using System.Collections.Generic;
using System.Data.SqlClient;
using TiendaApp.Modelo;

namespace TiendaApp.Dato
{
    public class ProductoDAO
    {
        private const string Cols = "id, nombre, categoria, precio, itbis_pct, stock, activo";

        public int Create(Producto p)
        {
            const string sql =
                "INSERT INTO producto (nombre, categoria, precio, itbis_pct, stock, activo) " +
                "VALUES (@nombre, @categoria, @precio, @itbis, @stock, @activo); " +
                "SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (SqlConnection cn = Conexion.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@nombre", p.Nombre);
                cmd.Parameters.AddWithValue("@categoria", p.Categoria);
                cmd.Parameters.Add(Conexion.Dec("@precio", p.Precio));
                cmd.Parameters.Add(Conexion.Dec("@itbis", p.ItbisPct));
                cmd.Parameters.AddWithValue("@stock", p.Stock);
                cmd.Parameters.AddWithValue("@activo", p.Activo);
                cn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        public List<Producto> Read()
        {
            return Consultar("SELECT " + Cols + " FROM producto ORDER BY nombre");
        }

        // Busca por texto en nombre o categoria, o por codigo exacto si el texto es numerico.
        public List<Producto> Buscar(string texto)
        {
            int id;
            if (!int.TryParse(texto, out id)) id = -1;

            return Consultar(
                "SELECT " + Cols + " FROM producto " +
                "WHERE nombre LIKE @t OR categoria LIKE @t OR id = @id ORDER BY nombre",
                new SqlParameter("@t", "%" + texto + "%"),
                new SqlParameter("@id", id));
        }

        // Productos que se pueden vender: activos y con existencia.
        public List<Producto> ParaVenta()
        {
            return Consultar(
                "SELECT " + Cols + " FROM producto WHERE activo = 1 AND stock > 0 ORDER BY nombre");
        }

        public List<string> Categorias()
        {
            List<string> lista = new List<string>();
            using (SqlConnection cn = Conexion.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand("SELECT DISTINCT categoria FROM producto ORDER BY categoria", cn))
            {
                cn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read()) lista.Add(dr.GetString(0));
                }
            }
            return lista;
        }

        public bool Update(Producto p)
        {
            const string sql =
                "UPDATE producto SET nombre = @nombre, categoria = @categoria, precio = @precio, " +
                "itbis_pct = @itbis, stock = @stock, activo = @activo WHERE id = @id";

            using (SqlConnection cn = Conexion.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@nombre", p.Nombre);
                cmd.Parameters.AddWithValue("@categoria", p.Categoria);
                cmd.Parameters.Add(Conexion.Dec("@precio", p.Precio));
                cmd.Parameters.Add(Conexion.Dec("@itbis", p.ItbisPct));
                cmd.Parameters.AddWithValue("@stock", p.Stock);
                cmd.Parameters.AddWithValue("@activo", p.Activo);
                cmd.Parameters.AddWithValue("@id", p.Id);
                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Falla con error 547 si el producto ya tiene ventas (se debe desactivar en su lugar).
        public bool Delete(int id)
        {
            using (SqlConnection cn = Conexion.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand("DELETE FROM producto WHERE id = @id", cn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        private List<Producto> Consultar(string sql, params SqlParameter[] parametros)
        {
            List<Producto> lista = new List<Producto>();

            using (SqlConnection cn = Conexion.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                if (parametros != null) cmd.Parameters.AddRange(parametros);

                cn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new Producto
                        {
                            Id = dr.GetInt32(0),
                            Nombre = dr.GetString(1),
                            Categoria = dr.GetString(2),
                            Precio = dr.GetDecimal(3),
                            ItbisPct = dr.GetDecimal(4),
                            Stock = dr.GetInt32(5),
                            Activo = dr.GetBoolean(6)
                        });
                    }
                }
            }
            return lista;
        }
    }
}
