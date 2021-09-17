using InsercionMasiva.Models;
using System;
using System.Data;
using System.Windows.Forms;

namespace InsercionMasiva.Views
{
    public partial class frmHistorial : Form
    {
        ArtLog artlog;

        public frmHistorial()
        {
            InitializeComponent();
        }

        private void frmHistorial_Load(object sender, EventArgs e)
        {

        }
        private void consultar()
        {
            try
            {
                string statement = "Historial";
                artlog = new ArtLog();
                dgvHistorial.DataSource = artlog.Consultar(statement);
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex); }
        }
        private void btnClose_Click(object sender, EventArgs e)
        {
            consultar();
        }
        private void txtSearchBox_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtSearchBox.Text))
            {
                (dgvHistorial.DataSource as DataTable).DefaultView.RowFilter = string.Empty;
            }
            else
            {
                (dgvHistorial.DataSource as DataTable).DefaultView.RowFilter = string.Format("Almacen LIKE '{0}%' or Articulo like '{0}%' or CONVERT(PuntoOrden, System.String) LIKE '{0}%'", txtSearchBox.Text);
            }
            txtSearchBox.SelectionStart = txtSearchBox.Text.Length + 0;
        }
    }
}
