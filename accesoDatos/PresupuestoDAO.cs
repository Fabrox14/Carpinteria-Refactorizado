using Carpinteria_Refactorizado.dominio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carpinteria_Refactorizado.accesoDatos
{
    class PresupuestoDAO : IPresupuestoDAO
    {
        // La interfaz asociada me obliga a implementar estos metodos
        public int ObtenerProximoNroPresupuesto()
        {
            return HelperDAO.ObtenerInstancia().ProximoID("SP_PROXIMO_ID", "@next");
        }

        public DataTable ListarProductos()
        {
            return HelperDAO.ObtenerInstancia().ConsultaSQL("SP_CONSULTAR_PRODUCTOS");
        }

        public bool Crear(Presupuesto oPresupuesto)
        {
            bool resultado = true;
            /*
            Dictionary<string, object> parametros = new Dictionary<string, object>();
            parametros.Add("@presupuesto_nro", 9);
            parametros.Add("@detalle_nro", 3);
            parametros.Add("@id_producto", 1);
            parametros.Add("@cantidad", 5);
            HelperDAO.ObtenerInstancia().EjecutarSQL("SP_INSERTAR_DETALLE", parametros);
            */

            
            SqlConnection cnn = new SqlConnection();
            SqlTransaction trans = null;

            try
            {
                cnn.ConnectionString = @"Data Source=LAPTOP-8EMNHC7Q;Initial Catalog=carpinteria_db;Integrated Security=True";
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
    }
}
