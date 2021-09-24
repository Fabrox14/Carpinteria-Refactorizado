
namespace Carpinteria_Refactorizado.gui
{
    partial class Frm_Reporte
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.rvReporte = new Microsoft.Reporting.WinForms.ReportViewer();
            this.SuspendLayout();
            // 
            // rvReporte
            // 
            this.rvReporte.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rvReporte.LocalReport.ReportEmbeddedResource = "Carpinteria_Refactorizado.gui.Reportes.rptProductos.rdlc";
            this.rvReporte.Location = new System.Drawing.Point(0, 0);
            this.rvReporte.Name = "rvReporte";
            this.rvReporte.ServerReport.BearerToken = null;
            this.rvReporte.Size = new System.Drawing.Size(800, 450);
            this.rvReporte.TabIndex = 0;
            // 
            // Frm_Reporte
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.rvReporte);
            this.Name = "Frm_Reporte";
            this.Text = "Reporte de Productos";
            this.Load += new System.EventHandler(this.Frm_Reporte_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Reporting.WinForms.ReportViewer rvReporte;
    }
}