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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Npgsql;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public class Buffer
        {
            public string name { get; set; }

            public string state { get; set; }

            public string zip { get; set; }
        }
        
        public MainWindow()
        {
            InitializeComponent();
            addState();
        }
        private string buildConnectionString()
        {
            return "Host = localhost; Username = postgres; Database =yelpdb; password = 12345";
        }

        private void addState()
        {
            using (var connection = new NpgsqlConnection(buildConnectionString()))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = connection;
                    cmd.CommandText = "SELECT distinct state FROM business ORDER BY state"; ;
                    try
                    {
                        var reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            statelist.Items.Add(reader.GetString(0));
                        }
                    }
                    catch (NpgsqlException ex)
                    {
                        Console.WriteLine(ex.Message.ToString());
                        System.Windows.MessageBox.Show("SQL Error - " + ex.Message.ToString());
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        private void statelist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            citylist.Items.Clear();
            if (statelist.SelectedIndex > -1)
            {


                using (var connection = new NpgsqlConnection(buildConnectionString()))
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = connection;
                        cmd.CommandText = "SELECT distinct city FROM business WHERE state = '" + statelist.SelectedItem.ToString() + "' ORDER BY city";
                        try
                        {
                            var reader = cmd.ExecuteReader();
                            while (reader.Read())
                            {
                                citylist.Items.Add(reader.GetString(0));
                            }
                        }
                        catch (NpgsqlException ex)
                        {
                            Console.WriteLine(ex.Message.ToString());
                            System.Windows.MessageBox.Show("SQL Error - " + ex.Message.ToString());
                        }
                        finally
                        {
                            connection.Close();
                        }
                    }
                }
            }
        }

        private void citylist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ziplist.Items.Clear();
            if (citylist.SelectedIndex > -1)
            {
                using (var connection = new NpgsqlConnection(buildConnectionString()))
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = connection;
                        cmd.CommandText = "SELECT distinct zipcode FROM business WHERE city = '" + citylist.SelectedItem.ToString() + "' ORDER BY zipcode";
                        try
                        {
                            var reader = cmd.ExecuteReader();
                            while (reader.Read())
                            {
                                ziplist.Items.Add(reader.GetInt32(0));
                            }
                        }
                        catch (NpgsqlException ex)
                        {
                            Console.WriteLine(ex.Message.ToString());
                            System.Windows.MessageBox.Show("SQL Error - " + ex.Message.ToString());
                        }
                        finally
                        {
                            connection.Close();
                        }
                    }
                }
            }
        }

        private void ziplist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            catlist.Items.Clear();
            if (ziplist.SelectedIndex > -1)
            {
                using (var connection = new NpgsqlConnection(buildConnectionString()))
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = connection;
                        cmd.CommandText = "SELECT distinct category_name FROM categories, business WHERE categories.business_id = business.business_id AND business.state = '"+ statelist.SelectedItem.ToString()+"' AND business.city = '"+ citylist.SelectedItem.ToString()+"' AND business.zipcode = "+ziplist.SelectedItem.ToString() +" ORDER BY categories.category_name";
                        try
                        {
                            var reader = cmd.ExecuteReader();
                            while (reader.Read())
                            {
                                catlist.Items.Add(reader.GetString(0));
                            }
                        }
                        catch (NpgsqlException ex)
                        {
                            Console.WriteLine(ex.Message.ToString());
                            System.Windows.MessageBox.Show("SQL Error - " + ex.Message.ToString());
                        }
                        finally
                        {
                            connection.Close();
                        }
                    }
                }
            }
        }
    }
}
