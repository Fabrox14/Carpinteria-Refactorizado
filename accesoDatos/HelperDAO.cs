using Carpinteria_Refactorizado.dominio;
using Carpinteria_Refactorizado.servicios;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carpinteria_Refactorizado.accesoDatos
{
    class HelperDAO
    {
        private static HelperDAO instancia;
        private string cadenaConexion;
        private HelperDAO()
        {
            // cadenaConexion = @"Data Source=LAPTOP-8EMNHC7Q;Initial Catalog=carpinteria_db;Integrated Security=True"; SIN USAR RECURSOS
            cadenaConexion = Properties.Resources.stringConexion;
        }

        public static HelperDAO ObtenerInstancia()
        {
            if(instancia == null)
            {
                instancia = new HelperDAO();
            }
            return instancia;
        }

        public DataTable ConsultaSQL(string nombreSP)
        {
            SqlConnection cnn = new SqlConnection();
            SqlCommand cmd = new SqlCommand();
            DataTable tabla = new DataTable();

            try
            {
                cnn.ConnectionString = cadenaConexion;
                cnn.Open();

                // Command Productos
                cmd.Connection = cnn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = nombreSP;

                tabla.Load(cmd.ExecuteReader());

                return tabla;
            } catch (SqlException ex)
            {
                throw (ex);
            } finally
            {
                if(cnn.State == ConnectionState.Open)
                {
                    cnn.Close();
                }
            }

        }

        public int ProximoID(string nombreSP, string nombreParametro)
        {
            SqlConnection cnn = new SqlConnection();
            SqlCommand cmd = new SqlCommand();
            SqlParameter param = new SqlParameter();

            try
            {
                cnn.ConnectionString = cadenaConexion;
                cnn.Open();

                // Command proximo ID
                cmd.Connection = cnn;

                // Command Type para el Tipo de COmando que quiero ejecutar
                // cmd.CommandText = CommandType.Text;  ejecutamos sql como texto plano
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = nombreSP;

                param.ParameterName = nombreParametro;
                param.SqlDbType = SqlDbType.Int;
                param.Direction = ParameterDirection.Output;

                cmd.Parameters.Add(param);
                cmd.ExecuteReader(); // no estoy esperando que el SP me devuelva un SELECT

                return (int)param.Value;
            } catch (SqlException ex)
            {
                throw (ex);
            } finally
            {
                if (cnn.State == ConnectionState.Open)
                {
                    cnn.Close();
                }
            }
        }

        public int EjecutarSQL(string nombreSP, Dictionary<string, object> parametros)
        {
            SqlConnection cnn = new SqlConnection();
            SqlCommand cmd = new SqlCommand();
            SqlTransaction trans = null;
            try
            {
                cnn.ConnectionString = cadenaConexion;
                cnn.Open();

                // Command proximo ID
                cmd.Connection = cnn;
                trans = cnn.BeginTransaction();

                // Command Type para el Tipo de COmando que quiero ejecutar
                // cmd.CommandText = CommandType.Text;  ejecutamos sql como texto plano
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = nombreSP;

                cmd.Transaction = trans;

                foreach (KeyValuePair<string, object> item in parametros)
                {
                    cmd.Parameters.AddWithValue(item.Key, item.Value);
                }

                int filasAfectadas = cmd.ExecuteNonQuery();
                trans.Commit();
                return filasAfectadas;
            }
            catch (SqlException ex)
            {
                trans.Rollback();
                throw (ex);
            }
            finally
            {
                if (cnn.State == ConnectionState.Open)
                {
                    cnn.Close();
                }
            }
        } // Botta

        public bool Save(Presupuesto oPresupuesto)
        {
            SqlConnection cnn = new SqlConnection();
            SqlTransaction trans = null;
            bool resultado = true;

            try
            {
                cnn.ConnectionString = cadenaConexion;
                cnn.Open();
                trans = cnn.BeginTransaction();

                SqlCommand cmd = new SqlCommand("SP_INSERTAR_MAESTRO", cnn, trans);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@cliente", oPresupuesto.Cliente);
                cmd.Parameters.AddWithValue("@dto", oPresupuesto.Descuento);
                cmd.Parameters.AddWithValue("@total", oPresupuesto.Total);

                SqlParameter param = new SqlParameter("@presupuesto_nro", SqlDbType.Int);
                param.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(param);
                cmd.ExecuteNonQuery();
                int presupuestoNro = Convert.ToInt32(param.Value);
                int cDetalles = 1; // es el ID que forma de la PK doble entre ID_PRESUPUESTO E ID_DETALLE
                int filasAfectadas = 0;


                foreach (DetallePresupuesto det in oPresupuesto.Detalles)
                {
                    SqlCommand cmdDet = new SqlCommand("SP_INSERTAR_DETALLE", cnn);
                    cmdDet.CommandType = CommandType.StoredProcedure;
                    cmdDet.Transaction = trans;
                    cmdDet.Parameters.AddWithValue("@presupuesto_nro", presupuestoNro);
                    cmdDet.Parameters.AddWithValue("@detalle", cDetalles);
                    cmdDet.Parameters.AddWithValue("@id_producto", det.Producto.IdProducto);
                    cmdDet.Parameters.AddWithValue("@cantidad", det.Cantidad);
                    cmdDet.ExecuteNonQuery();

                    cDetalles++;
                }


                trans.Commit();
            }
            catch (Exception ex)
            {
                trans.Rollback();
                resultado = false;
            }
            finally
            {
                if (cnn != null && cnn.State == ConnectionState.Open)
                {
                    cnn.Close();
                }
            }


            return resultado;
        }

        public Presupuesto GetById(int id)
        {
            Presupuesto oPresupuesto = new Presupuesto();
            SqlConnection cnn = new SqlConnection();
            cnn.ConnectionString = cadenaConexion;
            cnn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cnn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_CONSULTAR_PRESUPUESTO_POR_ID";
            cmd.Parameters.AddWithValue("@nro", id);
            SqlDataReader reader = cmd.ExecuteReader();
            bool esPrimerRegistro = true;

            while (reader.Read())
            {
                if (esPrimerRegistro)
                {
                    //solo para el primer resultado recuperamos los datos del MAESTRO:
                    oPresupuesto.PresupuestoNro = Convert.ToInt32(reader["presupuesto_nro"].ToString());
                    oPresupuesto.Cliente = reader["cliente"].ToString();
                    oPresupuesto.Fecha = Convert.ToDateTime(reader["fecha"].ToString());
                    oPresupuesto.Descuento = Convert.ToDouble(reader["descuento"].ToString());
                    oPresupuesto.PresupuestoNro = Convert.ToInt32(reader["presupuesto_nro"].ToString());
                    oPresupuesto.Total = Convert.ToDouble(reader["total"].ToString());
                    esPrimerRegistro = false;
                }

                DetallePresupuesto oDetalle = new DetallePresupuesto();
                Producto oProducto = new Producto();
                oProducto.IdProducto = Convert.ToInt32(reader["id_producto"].ToString());
                oProducto.Nombre = reader["n_producto"].ToString();
                oProducto.Precio = Convert.ToDouble(reader["precio"].ToString());
                oProducto.Activo = reader["activo"].ToString().Equals("S");
                oDetalle.Producto = oProducto;
                oDetalle.Cantidad = Convert.ToInt32(reader["cantidad"].ToString());
                esPrimerRegistro = false;
                oPresupuesto.AgregarDetalle(oDetalle);
            }
            return oPresupuesto;
        }

        public bool Delete(int idPresupuesto)
        {
            SqlConnection cnn = new SqlConnection();
            cnn.ConnectionString = cadenaConexion;
            SqlTransaction t = null;
            int affected = 0;
            try
            {
                cnn.Open();
                t = cnn.BeginTransaction();
                SqlCommand cmd = new SqlCommand("SP_REGISTRAR_BAJA_PRESUPUESTOS", cnn, t);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@presupuesto_nro", idPresupuesto);
                affected = cmd.ExecuteNonQuery();
                t.Commit();

            }
            catch (SqlException)
            {
                t.Rollback();
            }
            finally
            {
                if (cnn != null && cnn.State == ConnectionState.Open)
                    cnn.Close();
            }

            return affected == 1;
        }

        public List<Presupuesto> GetByFilters(List<Parametro> criterios)
        {
            List<Presupuesto> lst = new List<Presupuesto>();
            DataTable table = new DataTable();
            SqlConnection cnn = new SqlConnection();
            cnn.ConnectionString = cadenaConexion;
            try
            {
                cnn.Open();

                SqlCommand cmd = new SqlCommand("SP_CONSULTAR_PRESUPUESTOS", cnn);
                cmd.CommandType = CommandType.StoredProcedure;
                foreach (Parametro p in criterios)
                {
                    cmd.Parameters.AddWithValue(p.Nombre, p.Valor);
                }

                table.Load(cmd.ExecuteReader());
                //mappear los registros como objetos del dominio:

                foreach (DataRow row in table.Rows)
                {
                    //Por cada registro creamos un objeto del dominio
                    Presupuesto oPresupuesto = new Presupuesto();
                    oPresupuesto.Cliente = row["cliente"].ToString();
                    oPresupuesto.Fecha = Convert.ToDateTime(row["fecha"].ToString());
                    oPresupuesto.Descuento = Convert.ToDouble(row["descuento"].ToString());
                    oPresupuesto.PresupuestoNro = Convert.ToInt32(row["presupuesto_nro"].ToString());
                    oPresupuesto.Total = Convert.ToDouble(row["total"].ToString());
                    //validar que fecha_baja no es null:
                    if (!row["fecha_baja"].Equals(DBNull.Value))
                        oPresupuesto.FechaBaja = Convert.ToDateTime(row["fecha_baja"].ToString());

                    lst.Add(oPresupuesto);
                }

                cnn.Close();
            }
            catch (SqlException)
            {
                lst = null;
            }
            return lst;
        }

        public bool Update(Presupuesto oPresupuesto)
        {
            bool resultado = true;
            SqlConnection cnn = new SqlConnection();
            SqlTransaction trans = null;

            try
            {
                oPresupuesto.Total = oPresupuesto.calcularTotalDesc(oPresupuesto.CalcularTotal(), oPresupuesto.Descuento);

                cnn.ConnectionString = cadenaConexion;
                cnn.Open();
                trans = cnn.BeginTransaction();

                SqlCommand cmd = new SqlCommand("SP_UPDATE_PRESUPUESTOS", cnn, trans);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@nro_presupuesto", oPresupuesto.PresupuestoNro);
                cmd.Parameters.AddWithValue("@cliente", oPresupuesto.Cliente);
                cmd.Parameters.AddWithValue("@dto", oPresupuesto.Descuento);
                cmd.Parameters.AddWithValue("@total", oPresupuesto.Total);
                cmd.ExecuteNonQuery();

                SqlCommand cmdElimnar = new SqlCommand("SP_ELIMINAR_DETALLE_PRESUPUESTO", cnn, trans);
                cmdElimnar.CommandType = CommandType.StoredProcedure;
                cmdElimnar.Parameters.AddWithValue("@presupuesto_nro", oPresupuesto.PresupuestoNro);
                cmdElimnar.ExecuteNonQuery();


                int cDetalles = 1; // es el ID que forma de la PK doble entre ID_PRESUPUESTO E ID_DETALLE
                foreach (DetallePresupuesto det in oPresupuesto.Detalles)
                {
                    // SqlCommand cmdDet = new SqlCommand("SP_UPDATE_DETALLES_PRESUPUESTO", cnn);
                    SqlCommand cmdDet = new SqlCommand("[SP_INSERTAR_DETALLE]", cnn);
                    cmdDet.CommandType = CommandType.StoredProcedure;
                    cmdDet.Transaction = trans;
                    cmdDet.Parameters.AddWithValue("@presupuesto_nro", oPresupuesto.PresupuestoNro);
                    cmdDet.Parameters.AddWithValue("@detalle", cDetalles);
                    cmdDet.Parameters.AddWithValue("@id_producto", det.Producto.IdProducto);
                    cmdDet.Parameters.AddWithValue("@cantidad", det.Cantidad);
                    cmdDet.ExecuteNonQuery();
                    cDetalles++;
                }

                trans.Commit();
            }
            catch (Exception ex)
            {
                trans.Rollback();
                resultado = false;
            }
            finally
            {
                if (cnn != null && cnn.State == ConnectionState.Open)
                {
                    cnn.Close();
                }
            }

            return resultado;
        }
    }
}
