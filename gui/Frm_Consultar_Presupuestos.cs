using Carpinteria_Refactorizado.accesoDatos;
using Carpinteria_Refactorizado.dominio;
using Carpinteria_Refactorizado.servicios;
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
        private Presupuesto oPresupuesto;
        private GestorPresupuesto gestor;

        public Frm_Consultar_Presupuestos()
        {
            InitializeComponent();

            oPresupuesto = new Presupuesto();
            gestor = new GestorPresupuesto(new DAOFactory());
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


            List<Parametro> filtros = new List<Parametro>();

            Parametro fecha_desde = new Parametro();
            fecha_desde.Nombre = "@fecha_desde";
            fecha_desde.Valor = fechaDesde;
            filtros.Add(fecha_desde);
            filtros.Add(new Parametro("@fecha_hasta", fechaHasta));

            filtros.Add(new Parametro("@cliente", cliente));

            filtros.Add(new Parametro("@datos_baja", baja));

            ConsultarPresupuesto(filtros);
        }

        private void ConsultarPresupuesto(List<Parametro> filtros)
        {
            List<Presupuesto> lst = gestor.ConsultarPresupuestos(filtros);

            dgvResultados.Rows.Clear();
            foreach (Presupuesto oPresupuesto in lst)
            {
                dgvResultados.Rows.Add(new object[]{
                                        oPresupuesto.PresupuestoNro,
                                        oPresupuesto.Fecha.ToString("dd/MM/yyyy"),
                                        oPresupuesto.Cliente,
                                        oPresupuesto.Descuento,
                                        oPresupuesto.Total,
                                        oPresupuesto.GetFechaBajaFormato()
                 }); ;
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
                    // dgvResultados.Rows.Remove(dgvResultados.CurrentRow);
                    MessageBox.Show("Presupuesto eliminado!", "Confirmación", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.btnConsultar_Click(null, null);
                } else
                {
                    MessageBox.Show("Error al intentar borrar el presupuesto!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private bool eliminarPresupuesto(int idPresupuesto)
        {
            bool resultado = gestor.RegistrarBajaPresupuesto(idPresupuesto);

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
