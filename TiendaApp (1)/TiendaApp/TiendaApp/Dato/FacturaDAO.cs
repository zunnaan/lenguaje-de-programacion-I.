using System;
using System.Data.SqlClient;
using TiendaApp.Modelo;

namespace TiendaApp.Dato
{
    public class FacturaDAO
    {
        // Numero que tendra la proxima factura (solo informativo; el real se asigna al guardar).
        public int ProximoNumero()
        {
            using (SqlConnection cn = Conexion.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand("SELECT ultimo + 1 FROM secuencia WHERE nombre = 'factura'", cn))
            {
                cn.Open();
                object r = cmd.ExecuteScalar();
                return r == null ? 1 : Convert.ToInt32(r);
            }
        }

        /// <summary>
        /// Guarda factura + detalle, descuenta el stock y, si es a credito, registra la cuenta por cobrar.
        /// Todo en una sola transaccion: si algo falla no se guarda nada ni se consume numero de factura.
        /// </summary>
        public void Guardar(Factura f)
        {
            using (SqlConnection cn = Conexion.ObtenerConexion())
            {
                cn.Open();
                using (SqlTransaction tx = cn.BeginTransaction())
                {
                    try
                    {
                        // 1) Numero de factura (5 digitos, sin saltos)
                        int numero;
                        using (SqlCommand cmd = new SqlCommand(
                            "UPDATE secuencia SET ultimo = ultimo + 1 OUTPUT inserted.ultimo WHERE nombre = 'factura'",
                            cn, tx))
                        {
                            numero = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                        if (numero > 99999)
                            throw new InvalidOperationException("Se alcanzo el maximo de facturas de 5 digitos (99999).");

                        // 2) Cabecera
                        const string sqlFactura =
                            "INSERT INTO factura (numero, fecha, tipo, usuario_id, turno, persona_id, " +
                            "total_bruto, total_itbis, total_pagar, monto_pagado, devuelta) " +
                            "OUTPUT inserted.id " +
                            "VALUES (@numero, @fecha, @tipo, @usuario, @turno, @persona, " +
                            "@bruto, @itbis, @total, @pagado, @devuelta)";

                        int facturaId;
                        using (SqlCommand cmd = new SqlCommand(sqlFactura, cn, tx))
                        {
                            cmd.Parameters.AddWithValue("@numero", numero);
                            cmd.Parameters.AddWithValue("@fecha", f.Fecha);
                            cmd.Parameters.AddWithValue("@tipo", f.Tipo);
                            cmd.Parameters.AddWithValue("@usuario", f.UsuarioId);
                            cmd.Parameters.AddWithValue("@turno", (object)f.Turno ?? DBNull.Value);
                            cmd.Parameters.Add(Conexion.IntNulo("@persona", f.PersonaId));
                            cmd.Parameters.Add(Conexion.Dec("@bruto", f.TotalBruto));
                            cmd.Parameters.Add(Conexion.Dec("@itbis", f.TotalItbis));
                            cmd.Parameters.Add(Conexion.Dec("@total", f.TotalPagar));
                            cmd.Parameters.Add(Conexion.Dec("@pagado", f.MontoPagado));
                            cmd.Parameters.Add(Conexion.Dec("@devuelta", f.Devuelta));
                            facturaId = (int)cmd.ExecuteScalar();
                        }

                        // 3) Detalle + descuento de stock
                        const string sqlDetalle =
                            "INSERT INTO factura_detalle (factura_id, producto_id, nombre_producto, cantidad, valor, itbis, total) " +
                            "VALUES (@fid, @pid, @nombre, @cant, @valor, @itbis, @total)";
                        const string sqlStock =
                            "UPDATE producto SET stock = stock - @cant WHERE id = @pid AND activo = 1 AND stock >= @cant";

                        foreach (FacturaDetalle d in f.Detalles)
                        {
                            using (SqlCommand cmd = new SqlCommand(sqlDetalle, cn, tx))
                            {
                                cmd.Parameters.AddWithValue("@fid", facturaId);
                                cmd.Parameters.AddWithValue("@pid", d.ProductoId);
                                cmd.Parameters.AddWithValue("@nombre", d.NombreProducto);
                                cmd.Parameters.AddWithValue("@cant", d.Cantidad);
                                cmd.Parameters.Add(Conexion.Dec("@valor", d.Valor));
                                cmd.Parameters.Add(Conexion.Dec("@itbis", d.Itbis));
                                cmd.Parameters.Add(Conexion.Dec("@total", d.Total));
                                cmd.ExecuteNonQuery();
                            }

                            using (SqlCommand cmd = new SqlCommand(sqlStock, cn, tx))
                            {
                                cmd.Parameters.AddWithValue("@cant", d.Cantidad);
                                cmd.Parameters.AddWithValue("@pid", d.ProductoId);
                                if (cmd.ExecuteNonQuery() == 0)
                                    throw new InvalidOperationException(
                                        "Stock insuficiente para \"" + d.NombreProducto + "\". Actualiza la lista de productos.");
                            }
                        }

                        // 4) Cuenta por cobrar (solo credito)
                        if (f.Tipo == TipoFactura.Credito)
                        {
                            const string sqlCxc =
                                "INSERT INTO cuenta_por_cobrar (factura_id, persona_id, fecha, monto, saldo, estado) " +
                                "VALUES (@fid, @pid, @fecha, @monto, @monto, 'Pendiente')";
                            using (SqlCommand cmd = new SqlCommand(sqlCxc, cn, tx))
                            {
                                cmd.Parameters.AddWithValue("@fid", facturaId);
                                cmd.Parameters.AddWithValue("@pid", f.PersonaId.Value);
                                cmd.Parameters.AddWithValue("@fecha", f.Fecha);
                                cmd.Parameters.Add(Conexion.Dec("@monto", f.TotalPagar));
                                cmd.ExecuteNonQuery();
                            }
                        }

                        tx.Commit();
                        f.Id = facturaId;
                        f.Numero = numero;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
