using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace InsercionMasiva.Models
{
    public class ArtLog
    {
        SqlDataAdapter sda;
        DataTable dt;

        //getter and setters
        public string Almacen { get; set; }
        public string Articulo { get; set; }
        public int Minimo { get; set; }
        public int Maximo { get; set; }
        public int PuntoOrden { get; set; }


        //bulkcopy
        public int existeAlmacen(string Almacen)
        {
            int irespuesta;
            using (var connection = sqlConnection.getConnection())
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand("", connection))
                {
                    cmd.CommandText = "select count(Almacen) from ArtAlm where Almacen = '" + Almacen + "'";

                    irespuesta = Int32.Parse(cmd.ExecuteScalar().ToString());

                }
            }
            return irespuesta;
        }
        public void BulkInsert(IEnumerable<ArtLog> detailList)
        {
            try
            {
                //crear Tabla
                var table = new DataTable();
                table.Columns.Add("Almacen", typeof(string));
                table.Columns.Add("Articulo", typeof(string));
                table.Columns.Add("Minimo", typeof(int));
                table.Columns.Add("Maximo", typeof(int));
                table.Columns.Add("PuntoOrden", typeof(int));

                foreach (var itemDetail in detailList)
                {
                    table.Rows.Add(new object[]
                    {
                    itemDetail.Almacen,
                    itemDetail.Articulo,
                    itemDetail.Minimo,
                    itemDetail.Maximo,
                    itemDetail.PuntoOrden
                    });

                }
                using (var connection = sqlConnection.getConnection())
                {
                    //tabla temporal
                    using (SqlCommand cmd = new SqlCommand("", connection))
                    {
                        connection.Open();
                        cmd.CommandText = "CREATE TABLE ##tmpUpdate(Almacen VARCHAR(10), Articulo VARCHAR(15), Minimo INT, Maximo INT, PuntoOrden INT)";
                        cmd.ExecuteNonQuery();

                        //transaction
                        using (SqlTransaction transaction = connection.BeginTransaction())
                        {

                            using (SqlBulkCopy bulkcopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                            {

                                try
                                {
                                    bulkcopy.DestinationTableName = "##tmpUpdate";
                                    bulkcopy.BulkCopyTimeout = 500;
                                    bulkcopy.WriteToServer(table);
                                    transaction.Commit();

                                }
                                catch (Exception ex)
                                {
                                    transaction.Rollback();
                                    connection.Close();
                                    MessageBox.Show("Error: " + ex);

                                }
                            }
                        }
                    }
                    //aqui merengues ejecutar el sp 
                    using (SqlCommand cmd = new SqlCommand("sp_BulkUpdate", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        int ex = cmd.ExecuteNonQuery();
                        if (ex > 0)
                        {
                            MessageBox.Show((ex / 2) + " Registros Actualizados", "BULK UPDATE", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Registros Duplicados", "Excepcion");
            }
        }

        public DataTable Consultar(string statement)
        {
            try
            {
                // STATEMENTTYPE
                using (var connection = sqlConnection.getConnection())
                {
                    sda = new SqlDataAdapter("sp_fil", connection);
                    sda.SelectCommand.CommandType = CommandType.StoredProcedure;
                    sda.SelectCommand.Parameters.AddWithValue("@StatementType", statement);
                    dt = new DataTable();
                    sda.Fill(dt);

                    return dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex, "Conexion");
                return dt;
            }
        }
    }
}


