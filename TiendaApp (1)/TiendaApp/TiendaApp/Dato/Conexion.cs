using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace TiendaApp.Dato
{
    /// <summary>
    /// Centraliza la creacion de conexiones a SQL Server usando App.config
    /// y algunos helpers para parametros.
    /// </summary>
    public static class Conexion
    {
        public static SqlConnection ObtenerConexion()
        {
            string cadena = ConfigurationManager.ConnectionStrings["TiendaDb"].ConnectionString;
            return new SqlConnection(cadena);
        }

        // Parametro DECIMAL(12,2) explicito (evita redondeos).
        public static SqlParameter Dec(string nombre, decimal valor)
        {
            return new SqlParameter(nombre, SqlDbType.Decimal) { Precision = 12, Scale = 2, Value = valor };
        }

        // Parametro INT que puede ser NULL.
        public static SqlParameter IntNulo(string nombre, int? valor)
        {
            return new SqlParameter(nombre, SqlDbType.Int)
            {
                Value = valor.HasValue ? (object)valor.Value : DBNull.Value
            };
        }

        // Parametro VARCHAR (mismo tipo que usa HASHBYTES al crear las contrasenas).
        public static SqlParameter Texto(string nombre, string valor, int largo)
        {
            return new SqlParameter(nombre, SqlDbType.VarChar, largo) { Value = (object)valor ?? DBNull.Value };
        }
    }
}
