using ClosedXML.Excel;
using InsercionMasiva.Models;
using InsercionMasiva.Views;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
namespace InsercionMasiva
{
    public partial class Form1 : Form
    {
        Excel excel;
        ArtLog artlog;
        DataTable dt;
        frmHistorial historial;

        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.AllowUserToAddRows = false;
        }
        // crea lista con los valores del grid
        private void button1_Click(object sender, EventArgs e)
        {
            bulkupdate();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            excel = new Excel();
            dataGridView1.DataSource = excel.LeerExcel();

            validate();
        }
        private void txtSearchBox_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtSearchBox.Text))
            {
                (dataGridView1.DataSource as DataTable).DefaultView.RowFilter = string.Empty;
            }
            else
            {
                (dataGridView1.DataSource as DataTable).DefaultView.RowFilter = string.Format("Almacen LIKE '{0}%' or Articulo like '{0}%' or CONVERT(PuntoOrden, System.String) LIKE '{0}%'", txtSearchBox.Text);
            }
            txtSearchBox.SelectionStart = txtSearchBox.Text.Length + 0;
        }

        //consulta y despliega en el gridview
        private void consult()
        {
            artlog = new ArtLog();
            dataGridView1.DataSource = artlog.Consultar("");
        }
        private void ExportarExcel()
        {
            try
            {
                SaveFileDialog fichero = new SaveFileDialog();
                fichero.Filter = "Excel (.xlsx)|.xlsx";
                if (fichero.ShowDialog() == DialogResult.OK)
                {
                    artlog = new ArtLog();
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        wb.Worksheets.Add(artlog.Consultar(""), "Alumnos");
                        wb.SaveAs(fichero.FileName);
                        MessageBox.Show("Archivo Generado", "Exportar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }

        }

        private void bulkupdate()
        {
            try
            {
                List<ArtLog> condicion = new List<ArtLog>();

                List<ArtLog> objecdetailist = new List<ArtLog>();
                foreach (DataGridViewRow dgvRow in dataGridView1.Rows)
                {

                    var detail = new ArtLog()
                    {
                        Almacen = Convert.ToString(dgvRow.Cells["Almacen"].Value),
                        Articulo = Convert.ToString(dgvRow.Cells["Articulo"].Value),
                        Minimo = Convert.ToInt32(dgvRow.Cells["Minimo"].Value),
                        Maximo = Convert.ToInt32(dgvRow.Cells["Maximo"].Value),
                        PuntoOrden = Convert.ToInt32(dgvRow.Cells["PuntoOrden"].Value)
                    };
                    if (Convert.ToInt32(dgvRow.Cells["Minimo"].Value.ToString()) < Convert.ToInt32(dgvRow.Cells["Maximo"].Value.ToString()) && Convert.ToInt32(dgvRow.Cells["PuntoOrden"].Value.ToString()) <= 2)
                    {
                        objecdetailist.Add(detail);
                    }
                    else if (Convert.ToInt32(dgvRow.Cells["Minimo"].Value.ToString()) > Convert.ToInt32(dgvRow.Cells["Maximo"].Value.ToString()))
                    {
                        condicion.Add(detail);
                        dgvRow.Cells["Minimo"].Style.BackColor = Color.Red;
                        dgvRow.Cells["Maximo"].Style.BackColor = Color.Red;

                    }
                    else if (Convert.ToInt32(dgvRow.Cells["PuntoOrden"].Value.ToString()) > 2)
                    {
                        condicion.Add(detail);
                        dgvRow.Cells["PuntoOrden"].Style.BackColor = Color.Red;
                    }
                }
                if (condicion.Count > 0)
                {
                    var message = "Se Encontro " + condicion.Count.ToString();

                    if (condicion.Count > 1)
                    {
                        message += " Errores  \n\n";
                    }
                    else
                    {
                        message += " Error  \n\n";
                    }
                    MessageBox.Show(message, "Errores", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                if (condicion.Count == 0)
                {
                    ArtLog detailmodel = new ArtLog();
                    detailmodel.BulkInsert(objecdetailist);
                    consult();
                }
            }
            catch (Exception ex)
            { MessageBox.Show("Error: " + ex, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
        private void validate()
        {
            artlog = new ArtLog();
            foreach (DataGridViewRow dgvRow in dataGridView1.Rows)
            {
                string almacen;
                almacen = dgvRow.Cells["Almacen"].Value.ToString();
                int iexiste = artlog.existeAlmacen(almacen);
                if (iexiste == 0)
                {
                    dgvRow.Cells["Observaciones"].Value = null;
                    dgvRow.DefaultCellStyle.BackColor = Color.Red;
                    dgvRow.Cells["Observaciones"].Value += "Resgistro Inexistente";
                }


                if (Convert.ToInt32(dgvRow.Cells["Minimo"].Value.ToString()) > Convert.ToInt32(dgvRow.Cells["Maximo"].Value.ToString()))
                {

                    dgvRow.Cells["Minimo"].Style.BackColor = Color.Red;
                    dgvRow.Cells["Maximo"].Style.BackColor = Color.Red;
                    dgvRow.Cells["Observaciones"].Value = null;
                    dgvRow.Cells["Observaciones"].Value += "Minimo debe ser Menor del Maximo";

                }
                if (Convert.ToInt32(dgvRow.Cells["PuntoOrden"].Value.ToString()) > 2)
                {
                    dgvRow.Cells["PuntoOrden"].Style.BackColor = Color.Red;
                    dgvRow.Cells["Observaciones"].Value += " |PuntoOrden debe ser [0, 1 o 2]";
                }
                button1.Enabled = true;
            }

        }
        private void dataGridView1_RowHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            dt = dataGridView1.DataSource as DataTable;
            DataRow row = dt.NewRow();
            dt.Rows.Add(row);
            dataGridView1.CurrentCell = dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[0];
        }
        private void txtSearchBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetterOrDigit(e.KeyChar) && e.KeyChar != '-' && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }
        private void button5_Click(object sender, EventArgs e)
        {
            ExportarExcel();
        }
        private void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            e.Control.KeyPress -= new KeyPressEventHandler(Column1_KeyPress);
            if (dataGridView1.CurrentCell.ColumnIndex == 5)
            {
                TextBox tb = e.Control as TextBox;
                if (tb != null)
                {
                    tb.KeyPress += new KeyPressEventHandler(Column1_KeyPress);
                }

            }
            if (dataGridView1.CurrentCell.ColumnIndex == 3 || dataGridView1.CurrentCell.ColumnIndex == 4)
            {
                TextBox tb = e.Control as TextBox;
                if (tb != null)
                {
                    tb.KeyPress += new KeyPressEventHandler(Column2_KeyPress);
                }
            }
        }
        private void Column1_KeyPress(object sender, KeyPressEventArgs e)
        {
            int iKey;

            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
            else
            {
                iKey = Convert.ToInt32(char.GetNumericValue(e.KeyChar));
                if (iKey > 2 || iKey < 0 && e.KeyChar != '\b')
                {
                    e.Handled = true;
                }
            }
        }
        private void Column2_KeyPress(object sender, KeyPressEventArgs e)
        {

            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0 && e.ColumnIndex >= 3 && e.ColumnIndex <= 5)
                {
                    validate();
                    string strCadena = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();

                    if (strCadena != "" && strCadena != null)
                    {
                        int val = Int32.Parse(strCadena);
                        if (dataGridView1.CurrentCell.ColumnIndex == 5 && val <= 2 && val >= 0)
                        {
                            dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = Color.FromArgb(46, 51, 73);
                            dataGridView1.Rows[e.RowIndex].Cells[0].Value = null;
                            validate();
                        }
                        else
                        {
                            dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = Color.Red;

                        }
                        if (dataGridView1.CurrentCell.ColumnIndex == 3 || dataGridView1.CurrentCell.ColumnIndex == 4)
                        {
                            int valCol2, valCol3;
                            valCol2 = Int32.Parse(dataGridView1.Rows[e.RowIndex].Cells[3].Value.ToString());
                            valCol3 = Int32.Parse(dataGridView1.Rows[e.RowIndex].Cells[4].Value.ToString());
                            if (valCol2 < valCol3)
                            {
                                dataGridView1.Rows[e.RowIndex].Cells[3].Style.BackColor = Color.FromArgb(46, 51, 73);
                                dataGridView1.Rows[e.RowIndex].Cells[4].Style.BackColor = Color.FromArgb(46, 51, 73);
                                dataGridView1.Rows[e.RowIndex].Cells[0].Value = null;
                                validate();
                            }
                            else
                            {
                                dataGridView1.Rows[e.RowIndex].Cells[3].Style.BackColor = Color.Red;
                                dataGridView1.Rows[e.RowIndex].Cells[4].Style.BackColor = Color.Red;

                            }

                        }
                    }
                    else
                    {
                        dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = Color.Red;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex);
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            historial = new frmHistorial();
            historial.Show();
        }
    }
}