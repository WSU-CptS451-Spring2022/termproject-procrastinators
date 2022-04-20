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

        public string nestedCommand = "";

        public MainWindow()
        {
            InitializeComponent();
            addState();
            addColumnsToGrid();
            sorted.Items.Add("Name(default)");
            sorted.Items.Add("Highest Rated");
            sorted.Items.Add("Most Number of Tips");
            sorted.Items.Add("Most Checkins");
            sorted.Items.Add("Nearest");
        }
        private string buildConnectionString()
        {
            return "Host = localhost; Username = postgres; Database =yelpdb; password = 12345";
        }

        private void addState()
        {
            using (var connection = new NpgsqlConnection(DBInfo.buildConnectionString()))
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


                using (var connection = new NpgsqlConnection(DBInfo.buildConnectionString()))
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
                using (var connection = new NpgsqlConnection(DBInfo.buildConnectionString()))
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

        private void addColumnsToGrid()
        {
            DataGridTextColumn col1 = new DataGridTextColumn();
            col1.Binding = new Binding("name");
            col1.Header = "Buisness Name";
            col1.Width = 255;
            businessgrid.Columns.Add(col1);

            DataGridTextColumn col2 = new DataGridTextColumn();
            col2.Binding = new Binding("address");
            col2.Header = "Address";
            col2.Width = 60;
            businessgrid.Columns.Add(col2);

            DataGridTextColumn col3 = new DataGridTextColumn();
            col3.Binding = new Binding("stars");
            col3.Header = "Stars";
            col3.Width = 150;
            businessgrid.Columns.Add(col3);

            DataGridTextColumn col4 = new DataGridTextColumn();
            col4.Binding = new Binding("tip_count");
            col4.Header = "Tips";
            col4.Width = 0;
            businessgrid.Columns.Add(col4);
        }

        private void ziplist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            catlist.Items.Clear();
            businessgrid.Items.Clear();
            nestedCommand = "";

            if (ziplist.SelectedIndex > -1)
            {
                using (var connection = new NpgsqlConnection(DBInfo.buildConnectionString()))
                {
                    connection.Open();
                    using (var cmd1 = new NpgsqlCommand())
                    {
                        cmd1.Connection = connection;
                        cmd1.CommandText = "SELECT * FROM business " +
                            "WHERE state = '"+ statelist.SelectedItem.ToString()+"' " +
                            "AND city = '"+ citylist.SelectedItem.ToString()+"' " +
                            "AND zipcode = "+ziplist.SelectedItem.ToString() +" " +
                            "ORDER BY name";
                        try
                        {
                            var reader = cmd1.ExecuteReader();
                            while (reader.Read())
                            {
                                businessgrid.Items.Add(new Business()
                                {
                                    bid = reader.GetString(0),
                                    name = reader.GetString(1),
                                    address = reader.GetString(2),
                                    city = reader.GetString(3),
                                    state = reader.GetString(4),
                                    postal_code = reader.GetInt32(5),
                                    latitude = reader.GetDouble(6),
                                    longitude = reader.GetDouble(7),
                                    stars = reader.GetDouble(8),
                                    checkin_count = reader.GetInt32(9),
                                    tip_count = reader.GetInt32(10),
                                    is_open = reader.GetBoolean(11)
                                });
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

                    connection.Open();
                    using (var cmd2 = new NpgsqlCommand())
                    {
                        cmd2.Connection = connection;
                        cmd2.CommandText = "SELECT distinct category_name FROM categories, business WHERE categories.business_id = business.business_id AND business.state = '" + statelist.SelectedItem.ToString() + "' AND business.city = '" + citylist.SelectedItem.ToString() + "' AND business.zipcode = " + ziplist.SelectedItem.ToString() + " ORDER BY categories.category_name";
                        try
                        {
                            var reader = cmd2.ExecuteReader();
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
        private void catlist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            businessgrid.Items.Clear();
            selectlist.Items.Clear();
            if (catlist.SelectedIndex > -1)
            {
                using (var connection = new NpgsqlConnection(DBInfo.buildConnectionString()))
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        string relation = "business", n = "business";
                        cmd.Connection = connection;
                        if(nestedCommand != "")
                        {
                            relation = nestedCommand;
                            n = "nested";
                        }

                        cmd.CommandText = "SELECT " + n + ".business_id, " + n + ".name, " + n + ".address, " + n + ".city, " + n + ".state, " + n + ".zipcode, " + n +
                            ".latitude, " + n + ".longitude, " + n + ".stars, " + n + ".numCheckins, " + n + ".numTips, " + n + ".is_open, categories.category_name " +
                            "FROM " + relation + ", categories " +
                            "WHERE " + n + ".business_id = categories.business_id " +
                            "AND " + n + ".state = '" + statelist.SelectedItem.ToString() + "' " +
                            "AND " + n + ".city = '" + citylist.SelectedItem.ToString() + "' " +
                            "AND " + n + ".zipcode = " + ziplist.SelectedItem.ToString() + " " +
                            "AND categories.category_name = '" + catlist.SelectedItem.ToString() + "' " +
                            "ORDER BY name";

                        nestedCommand = "(" + cmd.CommandText + ") AS nested";
                        try
                        {
                            var reader = cmd.ExecuteReader();
                            while (reader.Read())
                            {
                                Business b = new Business()
                                {
                                    bid = reader.GetString(0),
                                    name = reader.GetString(1),
                                    address = reader.GetString(2),
                                    city = reader.GetString(3),
                                    state = reader.GetString(4),
                                    postal_code = reader.GetInt32(5),
                                    latitude = reader.GetDouble(6),
                                    longitude = reader.GetDouble(7),
                                    stars = reader.GetDouble(8),
                                    checkin_count = reader.GetInt32(9),
                                    tip_count = reader.GetInt32(10),
                                    is_open = reader.GetBoolean(11)
                                };
                                businessgrid.Items.Add(b);
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
                        for(int i = 0; i < catlist.SelectedItems.Count; i++)
                        {
                            selectlist.Items.Add(catlist.SelectedItems[i]);
                        }
                    }
                }
                
            }
        }

        private void businessgrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectcat.Items.Clear();
            selectatt.Items.Clear();
            if (businessgrid.SelectedIndex >= 0)
            {
                Business test = businessgrid.Items[businessgrid.SelectedIndex] as Business;
                businessname.Text = test.name;
                addy.Text = test.address;
                using (var connection = new NpgsqlConnection(buildConnectionString()))
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = connection;
                        string day = DateTime.Today.DayOfWeek.ToString();
                        cmd.CommandText = "SELECT hrs.close, hrs.open FROM business, hrs WHERE hrs.dayofweek = '" + day + "' AND hrs.business_id = '" + test.bid+"'";
                        try
                        {
                            var reader = cmd.ExecuteReader();
                            while (reader.Read()){
                                opcl.Text = "Today(" + DateTime.Today.DayOfWeek.ToString() + "): " + reader.GetTimeSpan(0) + " - " + reader.GetTimeSpan(1);
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
                using (var connection1 = new NpgsqlConnection(buildConnectionString()))
                {
                    connection1.Open();
                    using (var cmd1 = new NpgsqlCommand())
                    {
                        cmd1.Connection = connection1;
                        cmd1.CommandText = "SELECT category_name FROM categories WHERE business_id = '" + test.bid + "'";
                        try
                        {
                            var reader1 = cmd1.ExecuteReader();
                            while (reader1.Read())
                            {
                                selectcat.Items.Add(reader1.GetString(0));
                            }
                        }
                        catch (NpgsqlException ex)
                        {
                            Console.WriteLine(ex.Message.ToString());
                            System.Windows.MessageBox.Show("SQL Error - " + ex.Message.ToString());
                        }
                        finally
                        {
                            connection1.Close();
                        }
                    }
                }
                using (var connection2 = new NpgsqlConnection(buildConnectionString()))
                {
                    connection2.Open();
                    using (var cmd2 = new NpgsqlCommand())
                    {
                        cmd2.Connection = connection2;
                        cmd2.CommandText = "SELECT attr_name FROM attributes WHERE business_id = '" + test.bid + "' AND val = 'True'";
                        try
                        {
                            var reader2 = cmd2.ExecuteReader();
                            while (reader2.Read())
                            {
                                selectatt.Items.Add(reader2.GetString(0));
                            }
                        }
                        catch (NpgsqlException ex)
                        {
                            Console.WriteLine(ex.Message.ToString());
                            System.Windows.MessageBox.Show("SQL Error - " + ex.Message.ToString());
                        }
                        finally
                        {
                            connection2.Close();
                        }
                    }
                }
                Business B = businessgrid.Items[businessgrid.SelectedIndex] as Business;
                if ((B.bid != null) && (B.bid.ToString().CompareTo("") != 0))
                {
                    BusinessWindow BW = new BusinessWindow(B.bid.ToString());
                    BW.Show();
                }
            }
        }

        private void switchToUserWindow(object sender, RoutedEventArgs e)
        {
            UserWindow UW = new UserWindow();
            UW.Show();
            this.Close();
        }
    }
}

