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
    public partial class Frm_Reporte : Form
    {
        public Frm_Reporte()
        {
            InitializeComponent();
        }

        private void Frm_Reporte_Load(object sender, EventArgs e)
        {
            SqlConnection cnn = new SqlConnection(@"Data Source=LAPTOP-8EMNHC7Q;Initial Catalog=carpinteria_db;Integrated Security=True");

            DataTable table = new DataTable();
            cnn.Open();

            SqlCommand cmd = new SqlCommand("SP_REPORTE_PRODUCTOS", cnn);
            cmd.CommandType = CommandType.StoredProcedure;

            table.Load(cmd.ExecuteReader());
            cnn.Close();

            rvReporte.LocalReport.DataSources.Add(new Microsoft.Reporting.WinForms.ReportDataSource("DataSet1", table));

            this.rvReporte.RefreshReport();
        }
    }
}
