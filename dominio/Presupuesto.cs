using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carpinteria_Refactorizado.dominio
{
    class Presupuesto
    {
        public int PresupuestoNro { get; set; }
        public DateTime Fecha { get; set; }
        public string Cliente { get; set; }
        public double Total { get; set; }
        public double Descuento { get; set; }
        public DateTime FechaBaja { get; set; }
        public List<DetallePresupuesto> Detalles { get; }

        public Presupuesto()
        {
            // Generar la relacion 1 a muchos
            Detalles = new List<DetallePresupuesto>();
        }

        // Funcion para agregar los detalles del mismo presupuesto a una lista
        // pq un presupuesto tiene muchos detalles
        public void AgregarDetalle(DetallePresupuesto detalle)
        {
            Detalles.Add(detalle);
        }

        public void QuitarDetalle(int nro)
        {
            Detalles.RemoveAt(nro);
        }

        public double CalcularTotal()
        {
            double total = 0;

            foreach (DetallePresupuesto item in Detalles)
            {
                total += item.CalcularSubtotal();
            }

            return total;
        }

        //public bool Confirmar()
        //{
        //  paso al DAO
        //}

        public bool Actualizar()
        {
            bool resultado = true;
            SqlConnection cnn = new SqlConnection();
            SqlTransaction trans = null;

            try
            {
                Total = calcularTotalDesc(this.CalcularTotal(), this.Descuento);

                cnn.ConnectionString = @"Data Source=LAPTOP-8EMNHC7Q;Initial Catalog=carpinteria_db;Integrated Security=True";
                cnn.Open();
                trans = cnn.BeginTransaction();

                SqlCommand cmd = new SqlCommand("SP_UPDATE_PRESUPUESTOS", cnn, trans);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@nro_presupuesto", this.PresupuestoNro);
                cmd.Parameters.AddWithValue("@cliente", this.Cliente);
                cmd.Parameters.AddWithValue("@dto", this.Descuento);
                cmd.Parameters.AddWithValue("@total", this.Total);
                cmd.ExecuteNonQuery();

                SqlCommand cmdElimnar = new SqlCommand("SP_ELIMINAR_DETALLE_PRESUPUESTO", cnn, trans);
                cmdElimnar.CommandType = CommandType.StoredProcedure;
                cmdElimnar.Parameters.AddWithValue("@presupuesto_nro", this.PresupuestoNro);
                cmdElimnar.ExecuteNonQuery();


                int cDetalles = 1; // es el ID que forma de la PK doble entre ID_PRESUPUESTO E ID_DETALLE
                foreach (DetallePresupuesto det in Detalles)
                {
                    // SqlCommand cmdDet = new SqlCommand("SP_UPDATE_DETALLES_PRESUPUESTO", cnn);
                    SqlCommand cmdDet = new SqlCommand("[SP_INSERTAR_DETALLE]", cnn);
                    cmdDet.CommandType = CommandType.StoredProcedure;
                    cmdDet.Transaction = trans;
                    cmdDet.Parameters.AddWithValue("@presupuesto_nro", this.PresupuestoNro);
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

        private double calcularTotalDesc(double total, double descuento)
        {
            return total - ((descuento * total) / 100);
        }
    }
}
