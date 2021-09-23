using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Carpinteria_Refactorizado.gui
{
    public partial class Frm_Consultar_Detalles : Form
    {
        public Frm_Consultar_Detalles()
        {
            InitializeComponent();
        }

        public void nroPresupuesto(int nroPresupuesto)
        {
            lblPresupuestoNro.Text += nroPresupuesto.ToString();
            ConsultarDetalle(nroPresupuesto);
        }

        private void ConsultarDetalle(int nroPresupuesto)
        {
            SqlConnection cnn = new SqlConnection();

            try
            {
                cnn.ConnectionString = @"Data Source=LAPTOP-8EMNHC7Q;Initial Catalog=carpinteria_db;Integrated Security=True";
                cnn.Open();

                SqlCommand cmd = new SqlCommand("SP_CONSULTAR_DETALLES", cnn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@presupuesto_nro", nroPresupuesto);

                DataTable table = new DataTable();

                table.Load(cmd.ExecuteReader());
                cnn.Close();

                dgvResultados.Rows.Clear();
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    dgvResultados.Rows.Add(new object[]{
                                        table.Rows[i]["presupuesto_nro"],
                                        table.Rows[i]["detalle_nro"],
                                        table.Rows[i]["n_producto"],
                                        table.Rows[i]["precio"],
                                        table.Rows[i]["cantidad"]
                 });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los Presupuestos");
            }
            finally
            {
                if (cnn != null && cnn.State == ConnectionState.Open)
                {
                    cnn.Close();
                }
            }
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Desea Salir?", "Confirmación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                this.Dispose();
            }
        }
    }
}
