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
    public enum Accion
    {
        CREATE,
        READ,
        UPDATE,
        DELETE
    }

    public partial class Frm_Alta_Presupuesto : Form
    {
        private Presupuesto oPresupuesto;
        private GestorPresupuesto gestor;
        private Accion modo;

        private bool banderaUpdate = false;

        public Frm_Alta_Presupuesto(Accion modo, int nro_presupuesto)
        {
            InitializeComponent();

            

            // Crear un nuevo Objeto Presupuesto
            this.modo = modo;
            oPresupuesto = new Presupuesto();
            gestor = new GestorPresupuesto(new DAOFactory());

            // Cargo los datos
           

            if (modo.Equals(Accion.READ))
            {
                txtFecha.Enabled = false;
                txtCliente.Enabled = false;
                txtDescuento.Enabled = false;
                nudCantidad.Enabled = false;
                cboProducto.Enabled = false;
                btnAceptar.Enabled = false;
                btnAgregar.Enabled = false;
                this.Text = "VER PRESUPUESTO";
                this.CargarPresupuesto(nro_presupuesto);
            }

            if (modo.Equals(Accion.UPDATE))
            {
                banderaUpdate = true;
                this.Text = "EDITAR PRESUPUESTO";
                CargarPresupuesto(nro_presupuesto);
            }
        }

        private void Frm_Alta_Presupuesto_Load(object sender, EventArgs e)
        {
            this.cargarCombo();

            if (modo.Equals(Accion.CREATE))
            {
                consultarUltimoPresupuesto();
                // Valores por defecto
                txtFecha.Text = DateTime.Now.ToString("dd/MM/yyyy");
                txtCliente.Text = "CONSUMIDOR FINAL";
                txtDescuento.Text = "0";
                cboProducto.DropDownStyle = ComboBoxStyle.DropDownList;
            }
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
            txtTotal.Text = total.ToString();
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

            if (txtCliente.Text.Trim() == "")
            {
                MessageBox.Show("Debe ingresar un tipo de cliente", "Validaciones", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtCliente.Focus();
                return;
            }
            if (txtDescuento.Text.Trim() == "")
            {
                MessageBox.Show("Debe ingresar el porcetnaje de descuento", "Validaciones", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtCliente.Focus();
                return;
            }

            GuardarPresupuesto();
        }

        private void GuardarPresupuesto()
        {
            oPresupuesto.Cliente = txtCliente.Text;
            oPresupuesto.Descuento = Convert.ToDouble(txtDescuento.Text);
            oPresupuesto.Fecha = Convert.ToDateTime(txtFecha.Text);
            oPresupuesto.Total = Convert.ToDouble(txtTotal.Text);

            if(banderaUpdate)
            {
                if (gestor.ActualizarPresupuesto(oPresupuesto))
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

        private void CargarPresupuesto(int idPresupuesto)
        {
            this.oPresupuesto = gestor.ObtenerPresupuestoPorID(idPresupuesto);
            txtCliente.Text = oPresupuesto.Cliente;
            txtFecha.Text = oPresupuesto.Fecha.ToString("dd/MM/yyyy");
            txtDescuento.Text = oPresupuesto.Descuento.ToString();
            lblNro.Text = "Presupuesto Nro: " + oPresupuesto.PresupuestoNro.ToString();

            dgvDetalles.Rows.Clear();
            foreach (DetallePresupuesto oDetalle in oPresupuesto.Detalles)
            {
                dgvDetalles.Rows.Add(new object[] { "", oDetalle.Producto.Nombre, oDetalle.Producto.Precio, oDetalle.Cantidad, oDetalle.CalcularSubtotal() }); ;
            }
            calcularTotales();
        }
    }
}
