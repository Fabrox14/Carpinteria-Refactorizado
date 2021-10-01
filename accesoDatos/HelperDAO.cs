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
        }
    }
}
