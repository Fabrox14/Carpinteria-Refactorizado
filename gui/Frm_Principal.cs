using Carpinteria_Refactorizado.gui;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Carpinteria_Refactorizado
{
    public partial class Frm_Principal : Form
    {
        public Frm_Principal()
        {
            InitializeComponent();
        }

        private void nuevoPresupuestoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Frm_Alta_Presupuesto frmNuevo = new Frm_Alta_Presupuesto();
            frmNuevo.ShowDialog();
        }

        private void consultaDePresupuestoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Frm_Consultar_Presupuestos frmConsulta = new Frm_Consultar_Presupuestos();
            frmConsulta.ShowDialog();
        }

        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Desea Salir?", "Confirmación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                this.Dispose();
            }
        }

        private void acercaDeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Version Beta de la APP", "Carpinteria");
        }

        private void reporteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Frm_Report().ShowDialog();
        }
    }
}
