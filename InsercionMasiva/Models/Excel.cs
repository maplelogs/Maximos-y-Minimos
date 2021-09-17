using ClosedXML.Excel;
using System;
using System.Data;
using System.Windows.Forms;

namespace InsercionMasiva.Models
{
    class Excel
    {
        public DataTable LeerExcel()
        {
            DataTable dt = new DataTable();
            DataRow dr;
            try
            {
                OpenFileDialog fichero = new OpenFileDialog
                {
                    Filter = "Excel | .xls;*.xlsx;*.csv;",
                    Title = "Seleccionar Archivo..."
                };
                if (fichero.ShowDialog() == DialogResult.OK)
                {

                    using (XLWorkbook wb = new XLWorkbook(fichero.FileName))
                    {

                        var nonEmptyDataRows = wb.Worksheet(1).RowsUsed();
                        var nonEmptyDataColumns = wb.Worksheet(1).ColumnsUsed();

                        foreach (var dataRow in nonEmptyDataRows)
                        {
                            dr = dt.NewRow();
                            foreach (var dataColumn in nonEmptyDataColumns)
                            {
                                if (dataRow.RowNumber() == 1)
                                {
                                    dt.Columns.Add(dataRow.Cell(dataColumn.ColumnNumber()).Value.ToString(), Type.GetType("System.String"));
                                }
                                else
                                {
                                    dr[dataColumn.ColumnNumber() - 1] = dataRow.Cell(dataColumn.ColumnNumber()).Value.ToString();
                                }
                            }
                            if (dr[0].ToString() != "" && dr[0].ToString() != null)
                            {
                                dt.Rows.Add(dr);
                            }
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dt;

        }
    }
}
