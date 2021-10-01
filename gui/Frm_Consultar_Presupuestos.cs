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
    public partial class Frm_Consultar_Presupuestos : Form
    {
        public Frm_Consultar_Presupuestos()
        {
            InitializeComponent();
        }

        private void btnConsultar_Click(object sender, EventArgs e)
        {
            dgvResultados.Rows.Clear();
            Validar();
        }

        private void Validar()
        {
            if (dtpFechaDesde.Value.Date >= dtpFechaHasta.Value.Date)
            {
                MessageBox.Show("Ingrese Fechas Validas");
                return;
            }

            DateTime fechaDesde = dtpFechaDesde.Value.Date;
            DateTime fechaHasta = dtpFechaHasta.Value.Date;
            // MessageBox.Show(fechaDesde.ToString());
            string cliente = txtCliente.Text.Trim();
            string baja;
            baja = chkBaja.Checked == true ? "S" : "N";

            ConsultarPresupuesto(fechaDesde, fechaHasta, cliente, baja);
        }

        private void ConsultarPresupuesto(DateTime fechaDesde, DateTime fechaHasta, string cliente, string baja)
        {
            SqlConnection cnn = new SqlConnection();

            try
            {
                cnn.ConnectionString = @"Data Source=LAPTOP-8EMNHC7Q;Initial Catalog=carpinteria_db;Integrated Security=True";
                cnn.Open();

                SqlCommand cmd = new SqlCommand("SP_CONSULTAR_PRESUPUESTOS", cnn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@fecha_desde", fechaDesde);
                cmd.Parameters.AddWithValue("@fecha_hasta", fechaHasta);
                cmd.Parameters.AddWithValue("@cliente", cliente);
                cmd.Parameters.AddWithValue("@datos_baja", baja);

                DataTable table = new DataTable();
                table.Load(cmd.ExecuteReader());
                cnn.Close();

                dgvResultados.Rows.Clear();
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    dgvResultados.Rows.Add(new object[]{
                                        table.Rows[i]["presupuesto_nro"],
                                        table.Rows[i]["fecha"],
                                        table.Rows[i]["cliente"],
                                        table.Rows[i]["descuento"],
                                        table.Rows[i]["total"],
                                        table.Rows[i]["fecha_baja"]
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

        private void btnSalir_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Desea Salir?", "Confirmación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                this.Dispose();
            }
        }

        private void dgvResultados_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvResultados.CurrentCell.ColumnIndex == 6)
            {
                // MessageBox.Show(dgvResultados.CurrentRow.Index.ToString());
                // MessageBox.Show(dgvResultados.CurrentRow.Cells["colNro"].Value.ToString());
                // dgvResultados.Rows.Remove(dgvResultados.CurrentRow);

                int idPresupuesto = Int32.Parse(dgvResultados.CurrentRow.Cells["colNro"].Value.ToString());
                ConsultarDetalle(idPresupuesto);
            }
        }

        private void ConsultarDetalle(int idPresupuesto)
        {
            Frm_Alta_Presupuesto detalles = new Frm_Alta_Presupuesto(Accion.READ, idPresupuesto);
            detalles.ShowDialog();
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            int idPresupuesto = Int32.Parse(dgvResultados.CurrentRow.Cells["colNro"].Value.ToString());

            DialogResult result = MessageBox.Show("Esta seguro que desea dar de baja?", "Baja de Presupuesto", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                if (eliminarPresupuesto(idPresupuesto))
                {
                    // MessageBox.Show(idPresupuesto.ToString());
                    dgvResultados.Rows.Remove(dgvResultados.CurrentRow);
                }
            }
        }

        private bool eliminarPresupuesto(int idPresupuesto)
        {
            bool resultado = true;

            SqlConnection cnn = new SqlConnection();
            SqlTransaction trans = null;


            // Realizar una baja logica mediante la fecha_baja SQL SP y agregar transaccion 
            try
            {
                cnn.ConnectionString = @"Data Source=LAPTOP-8EMNHC7Q;Initial Catalog=carpinteria_db;Integrated Security=True";
                cnn.Open();
                trans = cnn.BeginTransaction();

                SqlCommand cmdPre = new SqlCommand("SP_ELIMINAR_PRESUPUESTO", cnn, trans);
                cmdPre.CommandType = CommandType.StoredProcedure;
                cmdPre.Transaction = trans;
                cmdPre.Parameters.AddWithValue("@presupuesto_nro", idPresupuesto);
                cmdPre.ExecuteNonQuery();

                trans.Commit();
            }
            catch (Exception ex)
            {
                trans.Rollback();
                MessageBox.Show(ex.ToString());
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

        private void btnEditar_Click(object sender, EventArgs e)
        {
            int idPresupuesto = Int32.Parse(dgvResultados.CurrentRow.Cells["colNro"].Value.ToString());

            // MessageBox.Show(idPresupuesto.ToString());
            editarPresupuesto(idPresupuesto);
        }

        private void editarPresupuesto(int idPresupuesto)
        {
            // Rellenar el de Alta Presupuesto
            Frm_Alta_Presupuesto frmAlta = new Frm_Alta_Presupuesto(Accion.UPDATE, idPresupuesto);
            frmAlta.ShowDialog();
            Validar();
        }
    }
}
