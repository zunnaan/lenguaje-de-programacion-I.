using System;
using System.Data;
using System.Data.SqlClient;

namespace TiendaApp.Dato
{
    /// <summary>Consultas de reportes. Todas devuelven DataTable para mostrarlas en un DataGridView.</summary>
    public class ReporteDAO
    {
        private const string Rango = "f.fecha >= @desde AND f.fecha < DATEADD(DAY, 1, @hasta)";

        private static SqlParameter Fecha(string nombre, DateTime valor)
        {
            return new SqlParameter(nombre, SqlDbType.Date) { Value = valor.Date };
        }

        private DataTable Llenar(string sql, params SqlParameter[] parametros)
        {
            DataTable dt = new DataTable();
            using (SqlConnection cn = Conexion.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddRange(parametros);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }
            return dt;
        }

        // Cuadre del dia (o rango): totales por tipo de venta.
        public DataTable Cuadre(DateTime desde, DateTime hasta)
        {
            string sql =
                "SELECT f.tipo AS [Tipo], COUNT(*) AS [Facturas], " +
                "SUM(f.total_bruto) AS [Total bruto], SUM(f.total_itbis) AS [ITBIS], SUM(f.total_pagar) AS [Total] " +
                "FROM factura f WHERE " + Rango + " GROUP BY f.tipo ORDER BY f.tipo";
            return Llenar(sql, Fecha("@desde", desde), Fecha("@hasta", hasta));
        }

        // Cuadre por cajero y turno (uno o todos; un turno o todos).
        public DataTable PorCajero(DateTime desde, DateTime hasta, int? usuarioId, string turno)
        {
            string sql =
                "SELECT u.nombre AS [Cajero], ISNULL(f.turno, '-') AS [Turno], COUNT(f.id) AS [Facturas], " +
                "SUM(CASE WHEN f.tipo = 'Contado' THEN f.total_pagar ELSE 0 END) AS [Contado], " +
                "SUM(CASE WHEN f.tipo = 'Credito' THEN f.total_pagar ELSE 0 END) AS [Crédito], " +
                "SUM(f.total_pagar) AS [Total] " +
                "FROM factura f INNER JOIN usuario u ON u.id = f.usuario_id " +
                "WHERE " + Rango + " AND (@u IS NULL OR f.usuario_id = @u) AND (@turno IS NULL OR f.turno = @turno) " +
                "GROUP BY u.id, u.nombre, f.turno " +
                "ORDER BY CASE f.turno WHEN 'Mañana' THEN 1 WHEN 'Tarde' THEN 2 WHEN 'Noche' THEN 3 ELSE 4 END, u.nombre";
            return Llenar(sql,
                Fecha("@desde", desde), Fecha("@hasta", hasta),
                Conexion.IntNulo("@u", usuarioId), Conexion.Texto("@turno", turno, 10));
        }

        // Inventario actual.
        public DataTable Inventario()
        {
            const string sql =
                "SELECT id AS [Código], nombre AS [Producto], categoria AS [Categoría], precio AS [Precio], " +
                "itbis_pct AS [ITBIS %], stock AS [Stock], precio * stock AS [Valor en stock], " +
                "CASE WHEN activo = 1 THEN 'Sí' ELSE 'No' END AS [Activo] " +
                "FROM producto ORDER BY nombre";
            return Llenar(sql);
        }

        // Facturas; tipo = null (todas), "Contado" o "Credito".
        public DataTable Facturas(DateTime desde, DateTime hasta, string tipo)
        {
            string sql =
                "SELECT RIGHT('00000' + CAST(f.numero AS VARCHAR(5)), 5) AS [No.], f.fecha AS [Fecha], " +
                "f.tipo AS [Tipo], u.nombre AS [Cajero], " +
                "ISNULL(p.nombre + ' ' + p.apellido, 'Consumidor final') AS [Cliente], " +
                "f.total_bruto AS [Bruto], f.total_itbis AS [ITBIS], f.total_pagar AS [Total] " +
                "FROM factura f INNER JOIN usuario u ON u.id = f.usuario_id " +
                "LEFT JOIN persona p ON p.id = f.persona_id " +
                "WHERE " + Rango + " AND (@tipo IS NULL OR f.tipo = @tipo) ORDER BY f.numero";
            return Llenar(sql, Fecha("@desde", desde), Fecha("@hasta", hasta), Conexion.Texto("@tipo", tipo, 7));
        }

        // Los 5 clientes con mas ventas (por monto).
        public DataTable TopClientes(DateTime desde, DateTime hasta)
        {
            string sql =
                "SELECT TOP 5 p.id AS [ID], p.nombre + ' ' + p.apellido AS [Cliente], " +
                "COUNT(f.id) AS [Facturas], SUM(f.total_pagar) AS [Total comprado] " +
                "FROM factura f INNER JOIN persona p ON p.id = f.persona_id " +
                "WHERE " + Rango + " " +
                "GROUP BY p.id, p.nombre, p.apellido ORDER BY SUM(f.total_pagar) DESC";
            return Llenar(sql, Fecha("@desde", desde), Fecha("@hasta", hasta));
        }

        // Cuentas por cobrar (ventas a credito).
        public DataTable CuentasPorCobrar()
        {
            const string sql =
                "SELECT RIGHT('00000' + CAST(f.numero AS VARCHAR(5)), 5) AS [Factura], c.fecha AS [Fecha], " +
                "p.nombre + ' ' + p.apellido AS [Cliente], c.monto AS [Monto], c.saldo AS [Saldo], c.estado AS [Estado] " +
                "FROM cuenta_por_cobrar c INNER JOIN factura f ON f.id = c.factura_id " +
                "INNER JOIN persona p ON p.id = c.persona_id ORDER BY c.fecha DESC";
            return Llenar(sql);
        }
    }
}
