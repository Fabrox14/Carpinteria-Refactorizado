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
    public partial class Frm_Alta_Presupuesto : Form
    {
        private Presupuesto oPresupuesto;
        private GestorPresupuesto gestor;
        private bool banderaUpdate = false;
        public Frm_Alta_Presupuesto()
        {
            InitializeComponent();

            // Valores por defecto
            txtFecha.Text = DateTime.Now.ToString("dd/MM/yyyy");
            txtCliente.Text = "CONSUMIDOR FINAL";
            txtDescuento.Text = "0";
            cboProducto.DropDownStyle = ComboBoxStyle.DropDownList;

            // Crear un nuevo Objeto Presupuesto
            oPresupuesto = new Presupuesto();
            gestor = new GestorPresupuesto(new DAOFactory());

            // Cargo los datos
            consultarUltimoPresupuesto();
            cargarCombo();
        }

        private void cargarCombo()
        {
            DataTable tabla = gestor.ObtenerProductos();

            // tabla.Rows[0]; // cada fila que tenga va a ser un DataRow

            cboProducto.DataSource = tabla;
            cboProducto.DisplayMember = tabla.Columns[1].ColumnName; // n_producto
            cboProducto.ValueMember = tabla.Columns[0].ColumnName; // id_producto 
        }

        private void consultarUltimoPresupuesto()
        {
            lblNro.Text = "Presupuesto Nro: " + gestor.ProximoPresupuesto();
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            if (ExisteProductoEnGrilla(cboProducto.Text))
            {
                MessageBox.Show("Producto ya agregado como detalle", "Validacion", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            DialogResult result = MessageBox.Show("Desea Agregar?", "Confirmación", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                DetallePresupuesto item = new DetallePresupuesto();
                item.Cantidad = (int)nudCantidad.Value;

                // tengo el objeto asociado al objeto actual,
                // se corresponde los items a las filas de la DataTable Completa del SP que le dimos
                // item o producto seleccionado
                DataRowView oDataRow = (DataRowView)cboProducto.SelectedItem;

                // oDataRow[0].ToString();  me da la columna 0 - accedo al id como cadena de la fila
                // seleccionada (combo seleccionado)
                // si coloco MessageBox.Show(oDataRow[1].ToString()); me devuelve nombre de producto

                // Lo que hace es traer del SP de consultar el producto la respuesta de eso producto,
                // por lo tanto trae del producto su id, descripcion, precio, etc.
                // desde el SP_CONSULTAR_PRODUCTOS cargado en el ComboBox

                // Producto:
                Producto oProducto = new Producto();
                oProducto.IdProducto = Int32.Parse(oDataRow[0].ToString());
                oProducto.Nombre = oDataRow[1].ToString();
                oProducto.Precio = Double.Parse(oDataRow[2].ToString());
                item.Producto = oProducto;

                oPresupuesto.AgregarDetalle(item);

                // te voy a dar un array de objetos
                dgvDetalles.Rows.Add(new object[] { "", oProducto.Nombre, oProducto.Precio, item.Cantidad, item.CalcularSubtotal() });

                calcularTotales();
            }
        }

        private void calcularTotales()
        {
            double subTotal = oPresupuesto.CalcularTotal();
            double desc = (Double.Parse(txtDescuento.Text) * subTotal) / 100;
            double total = subTotal - desc;
            lblSubtotal.Text = "Subtotal: $" + subTotal.ToString();
            lblDescuento.Text = "Descuento: $" + desc.ToString();
            lblTotal.Text = "Total: $" + total.ToString();
        }

        private bool ExisteProductoEnGrilla(string producto)
        {
            foreach (DataGridViewRow fila in dgvDetalles.Rows)
            {
                string col = fila.Cells["producto"].Value.ToString();
                if (col.Equals(producto))
                {
                    return true;
                }
            }
            return false;
        }

        private void dgvDetalles_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvDetalles.CurrentCell.ColumnIndex == 5)
            {
                oPresupuesto.QuitarDetalle(dgvDetalles.CurrentRow.Index);
                dgvDetalles.Rows.Remove(dgvDetalles.CurrentRow);
                calcularTotales();
            }
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            if (txtCliente.Text == "")
            {
                MessageBox.Show("Debe ingresar un cliente!", "Control",
                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (dgvDetalles.Rows.Count == 0)
            {
                MessageBox.Show("Debe ingresar al menos detalle!",
                "Control", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            GuardarPresupuesto();
        }

        private void GuardarPresupuesto()
        {
            oPresupuesto.Cliente = txtCliente.Text;
            oPresupuesto.Descuento = Convert.ToDouble(txtDescuento.Text);
            oPresupuesto.Fecha = Convert.ToDateTime(txtFecha.Text);
            oPresupuesto.Total = Convert.ToDouble(.Text);

            if(banderaUpdate)
            {
                if (oPresupuesto.Actualizar())
                {
                    MessageBox.Show("Presupuesto Actualizado", "Informe", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Dispose();
                }
                else
                {
                    MessageBox.Show("ERROR. No se pudo registrar el presupuesto", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            } else
            {
                if (gestor.ConfirmarPresupuesto(oPresupuesto))
                {
                    MessageBox.Show("Presupuesto registrado", "Informe", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Dispose();
                }
                else
                {
                    MessageBox.Show("ERROR. No se pudo registrar el presupuesto", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Desea Salir?", "Confirmación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                this.Dispose();
            }
        }

        // -------------------------------- UPDATE -------------------------------------
        internal void CargarDatos(int idPresupuesto)
        {
            banderaUpdate = true;
            SqlConnection cnn = new SqlConnection();

            try
            {
                cnn.ConnectionString = @"Data Source=LAPTOP-8EMNHC7Q;Initial Catalog=carpinteria_db;Integrated Security=True";
                cnn.Open();

                oPresupuesto.PresupuestoNro = idPresupuesto;
                cargarPresupuesto(cnn, idPresupuesto);
                cargarDetalles(cnn, idPresupuesto);
                calcularTotales();
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

        private void cargarDetalles(SqlConnection cnn, int idPresupuesto)
        {
            SqlCommand cmd = new SqlCommand("SP_CARGAR_DETALLES", cnn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@presupuesto_nro", idPresupuesto);

            SqlDataReader reader = cmd.ExecuteReader();

            int presupuesto_nro;
            int detalle_nro;
            int id_producto;
            string producto;
            decimal precio;
            int cantidad;

            DetallePresupuesto item = new DetallePresupuesto();
            Producto oProducto = new Producto();



            while (reader.Read())
            {
                presupuesto_nro = reader.GetInt32(0);
                detalle_nro = reader.GetInt32(1);
                id_producto = reader.GetInt32(2);
                producto = reader.GetString(3);
                precio = reader.GetDecimal(4);
                cantidad = reader.GetInt32(5);

                item.Cantidad = cantidad;
                oProducto.IdProducto = id_producto;
                oProducto.Nombre = producto;
                oProducto.Precio = Double.Parse(precio.ToString());
                item.Producto = oProducto;
                oPresupuesto.AgregarDetalle(item);

                dgvDetalles.Rows.Add(new object[] { "", oProducto.Nombre, oProducto.Precio, item.Cantidad, item.CalcularSubtotal() });

            }
            reader.Close();
        }

        private void cargarPresupuesto(SqlConnection cnn, int idPresupuesto)
        {
            SqlCommand cmd = new SqlCommand("SP_CARGAR_PRESUPUESTOS", cnn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@presupuesto_nro", idPresupuesto);

            SqlDataReader reader = cmd.ExecuteReader();

            int presupuesto_nro;
            DateTime fecha;
            string cliente;
            decimal descuento;

            while (reader.Read())
            {
                presupuesto_nro = reader.GetInt32(0);
                fecha = reader.GetDateTime(1);
                cliente = reader.GetString(2);
                descuento = reader.GetDecimal(3);

                lblNro.Text += presupuesto_nro;
                txtFecha.Text = fecha.ToShortDateString();
                txtCliente.Text = cliente;
                txtDescuento.Text = descuento.ToString();
            }
            reader.Close();
        }
    }
}
