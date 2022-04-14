using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Npgsql;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for BusinessWindow.xaml
    /// </summary>
    public partial class BusinessWindow : Window
    {
        public string bid = "";
        public BusinessWindow(string bid)
        {
            InitializeComponent();
            this.bid = String.Copy(bid);
            addColumnsToGrid();
            loadBusinessDetails();
        }

        

        private void addColumnsToGrid()
        {
            DataGridTextColumn col1 = new DataGridTextColumn();
            col1.Binding = new Binding("tipDate");
            col1.Header = "Date";
            col1.Width = 255;
            tipgrid.Columns.Add(col1);

            DataGridTextColumn col2 = new DataGridTextColumn();
            col2.Binding = new Binding("tipText");
            col2.Header = "Text";
            col2.Width = 60;
            tipgrid.Columns.Add(col2);

            DataGridTextColumn col3 = new DataGridTextColumn();
            col3.Binding = new Binding("likes");
            col3.Header = "Likes";
            col3.Width = 150;
            tipgrid.Columns.Add(col3);
        }

        private void loadBusinessDetails()
        {
            tipgrid.Items.Clear();
            using (var connection = new NpgsqlConnection(DBInfo.buildConnectionString()))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = connection;
                    cmd.CommandText = "SELECT DISTINCT * FROM tip WHERE tip.business_id = '" + this.bid + "' ORDER BY tipDate";
                    try
                    {
                        var reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            tipgrid.Items.Add(new Tip()
                            {
                                tipDate = reader.GetDateTime(0),
                                tipText = reader.GetString(1),
                                likes = reader.GetInt32(2),
                                uid = reader.GetString(3),
                                bid = reader.GetString(4),
                            });
                        }
                    }
                    catch (NpgsqlException ex)
                    {
                        Console.WriteLine(ex.Message.ToString());
                        System.Windows.MessageBox.Show($"SQL Error - {ex.Message.ToString()}");
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        private void addTip_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                using (var connection = new NpgsqlConnection(DBInfo.buildConnectionString()))
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = connection;
                        cmd.CommandText = "INSERT INTO tip (tipDate, tipText, likes, usr_id, business_id) VALUES ( '2011-12-26 01:46:17', '" + tipentry.Text + "', 0, 'jRyO2V1pA4CdVVqCIOPc1Q', '" + bid + "')";
                        try
                        {
                            cmd.ExecuteNonQuery();
                        }
                        catch (NpgsqlException ex)
                        {
                            Console.WriteLine(ex.Message.ToString());
                            System.Windows.MessageBox.Show($"SQL Error - {ex.Message.ToString()}");
                        }
                        finally
                        {
                            connection.Close();
                        }
                    }
                }
            }
            loadBusinessDetails();
        }
    }
}
