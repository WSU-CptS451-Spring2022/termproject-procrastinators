using System;
using System.Data;
using System.Collections.Generic;
using System.Diagnostics;
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

        // List of businesses to insert
        Dictionary<string, Business> _bus = new Dictionary<string, Business>();
        int numPriceBoxesSelected = 0;

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
            //addColumnsToGrid();
            sorted.Items.Add("Name(default)");
            sorted.Items.Add("Highest Rated");
            sorted.Items.Add("Most Number of Tips");
            sorted.Items.Add("Most Checkins");
            sorted.Items.Add("Nearest");
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

        private void ziplist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            catlist.Items.Clear();
            businessgrid.Items.Clear();
            _bus.Clear();
            List<string> categories = new List<string>();
            nestedCommand = "";

            if (ziplist.SelectedIndex > -1)
            {
                using (var connection = new NpgsqlConnection(DBInfo.buildConnectionString()))
                {
                    using (var cmd1 = new NpgsqlCommand("SELECT * FROM Business " +
                                                       $"WHERE state = '{statelist.SelectedItem.ToString()}' AND " +
                                                             $"city = '{citylist.SelectedItem.ToString()}' AND " +
                                                             $"zipcode = '{ziplist.SelectedItem.ToString()}' " +
                                                       $"ORDER BY name", connection))
                    {
                        connection.Open();
                        using (NpgsqlDataReader reader = cmd1.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Business newBus = new Business();

                                newBus.bid = reader["business_id"] as string;
                                newBus.name = reader["name"] as string;
                                newBus.address = reader["address"] as string;
                                newBus.city = reader["city"] as string;
                                newBus.state = reader["state"] as string;
                                newBus.postal_code = reader["zipcode"] as int?;
                                newBus.latitude = reader["latitude"] as double?;
                                newBus.longitude = reader["longitude"] as double?;
                                newBus.stars = reader["stars"] as double?;
                                newBus.checkin_count = reader["numcheckins"] as int?;
                                newBus.tip_count = reader["numtips"] as int?;
                                newBus.is_open = reader["is_open"] as bool?;
                                newBus.is_visible = true;
                                
                                _bus.Add(newBus.bid, newBus);
                            }
                        }
                    }

                    using (var cmd2 = new NpgsqlCommand("SELECT distinct category_name as c_name, business.business_id as bid " +
                                                        "FROM categories, business " +
                                                        $"WHERE categories.business_id = business.business_id AND " +
                                                              $"business.state = '{statelist.SelectedItem.ToString()}' AND " +
                                                              $"business.city = '{citylist.SelectedItem.ToString()}' AND " +
                                                              $"business.zipcode = '{ziplist.SelectedItem.ToString()}' " +
                                                        $"ORDER BY categories.category_name", connection))
                    {
                        using (NpgsqlDataReader reader = cmd2.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                categories.Add(reader["c_name"] as string);
                            }
                        }
                    }

                    User u = UserWindow.selectedUser;
                    foreach (Business bus in _bus.Values)
                    {
                        if (u != null)
                        {
                            if (u.latitude != null && u.longitude != null)
                            {
                                // This way of finding the distance was found here : https://stackoverflow.com/questions/61135374/postgresql-calculate-distance-between-two-points-without-using-postgis
                                using (var cmd = new NpgsqlCommand($"SELECT SQRT(POW(69.1 * (B.latitude - {u.latitude}), 2) " +
                                                                             $"+ POW(69.1 * ({u.longitude} - B.longitude) * COS(B.latitude / 57.3), 2)) as distance " +
                                                                   "FROM Business AS B " +
                                                                   $"WHERE B.business_id = '{bus.bid}';", connection))
                                {
                                    using (var reader = cmd.ExecuteReader())
                                    {
                                        if (reader.Read())
                                        {
                                            bus.distance = Math.Round((double)(reader["distance"] as double?), 2);
                                        }
                                    }
                                }

                            }
                        }
                        using (var cmd = new NpgsqlCommand("SELECT * " +
                                                           "FROM Hrs " +
                                                          $"WHERE business_id = '{bus.bid}';", connection))
                        {
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                    bus.insert_hours(reader["dayofweek"] as string, reader["open"] as TimeSpan?, reader["close"] as TimeSpan?);
                            }
                        }
                    }

                    

                    categories = categories.Distinct().ToList();
                    foreach (string c in categories)
                    {
                        catlist.Items.Add(c);
                    }

                    query_filters();
                }
                numbusinesses.Content = businessgrid.Items.Count;
            }
        }

        private void businessgrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectcat.Items.Clear();
            selectatt.Items.Clear();
            if (businessgrid.SelectedIndex >= 0)
            {
                Business test = businessgrid.SelectedItem as Business;
                businessname.Text = test.name;
                
                addy.Text = test.address;
                (TimeSpan?, TimeSpan?) hrs = test.get_hrs_of_day(DateTime.Today.DayOfWeek.ToString());
                if (hrs.Item1 != null && hrs.Item2 != null)
                {
                    opcl.Text = $"Today ( {DateTime.Today.DayOfWeek} ) : {hrs.Item1.ToString()} {hrs.Item2.ToString()}";
                }
                else
                    opcl.Text = $"Today ( {DateTime.Today.DayOfWeek} ) : Closed";


                using (var connection1 = new NpgsqlConnection(DBInfo.buildConnectionString()))
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
                using (var connection2 = new NpgsqlConnection(DBInfo.buildConnectionString()))
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
            }
        }
        
        private void tipsbutton_Click(object sender, EventArgs e)
        {
            if (businessgrid.SelectedIndex >= 0)
            {
                Business B = businessgrid.Items[businessgrid.SelectedIndex] as Business;
                if ((B.bid != null) && (B.bid.ToString().CompareTo("") != 0))
                {
                    BusinessWindow BW = new BusinessWindow(B.bid.ToString());
                    BW.Show();
                }
            }
        }

        private void checkinsbutton_Click(object sender, EventArgs e)
        {
            if (businessgrid.SelectedIndex >= 0)
            {
                Business B = businessgrid.Items[businessgrid.SelectedIndex] as Business;
                if ((B.bid != null) && (B.bid.ToString().CompareTo("") != 0))
                {
                    CheckInWindow BW = new CheckInWindow(B.bid.ToString());
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

        /// <summary>
        /// Called whenever a filter box is checked. Creates a query string and updates the business table
        /// with all qualifying businesses
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void filter_changed(object sender, RoutedEventArgs e)
        {
            query_filters();
        }

        /// <summary>
        /// This entire function is an abomination. I'm sorry in advance to whoever has to read this.
        /// </summary>
        private void query_filters()
        {
            businessgrid.Items.Clear();
            string query = "SELECT DISTINCT * FROM Business WHERE ";
            bool filter_added = false;
            string at = atcheck_query();
            if (at != "")
            {
                query += at + " AND ";
                filter_added = true;
            }

            string meal = meal_filter_query();
            if (meal != "")
            {
                query += meal + " AND ";
                filter_added = true;
            }

            string price = price_filter_query();
            if (price != "")
            {
                query += price + " AND ";
                filter_added = true;
            }

            // Filter categories
            for (int i = 0; i < selectlist.Items.Count; ++i)
            {
                query += $"business_id IN (SELECT business_id FROM Categories WHERE category_name = '{selectlist.Items[i]}') AND ";
                filter_added = true;
            }

            // This removes either the trailing AND or, in the case of no filters being added, removes the WHERE (not removing the where will
            //                                                                                                    cause a crash when adding ORDER BY)
            if (filter_added == true)
                query = query.Remove(query.Length - 4);
            else
                query = query.Remove(query.Length - 6);

            // add order by
            if (sorted.SelectedItem.ToString() == "Highest Rated")
                query += " ORDER BY stars DESC";
            else if (sorted.SelectedItem.ToString() == "Most Number of Tips")
                query += " ORDER BY numTips DESC";
            else if (sorted.SelectedItem.ToString() == "Most Checkins")
                query += " ORDER BY numCheckins DESC";
            else if (sorted.SelectedItem.ToString() == "Nearest") // I'm sorry
            {
                User u = UserWindow.selectedUser;
                if (u != null)
                {
                    if (u.latitude != null && u.longitude != null)
                    {
                        int i = query.IndexOf("*");
                        query = query.Insert(i + 1, $", SQRT(POW(69.1 * (Business.latitude - {u.latitude}), 2) + POW(69.1 * ({u.longitude} - Business.longitude) * COS(Business.latitude / 57.3), 2)) as distance ");
                        query += " ORDER BY distance";
                    }
                }
            }
            else
                query += " ORDER BY name ASC";

            using (var connection = new NpgsqlConnection(DBInfo.buildConnectionString()))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (_bus.ContainsKey(reader["business_id"] as string))
                            {
                                businessgrid.Items.Add(_bus[reader["business_id"] as string]);
                            }
                        }
                    }
                }
                connection.Close();
            }
            numbusinesses.Content = businessgrid.Items.Count;
        }

        /// <summary>
        /// Creates a query string for all selected attribute filter boxes
        /// </summary>
        /// <returns>A query string</returns>
        private string atcheck_query()
        {
            string query = "";
            foreach (CheckBox cb in atcheck.Children)
            {
                if (cb.IsChecked == true)
                {
                    if (cb.Name == "WiFi")
                        query += $"business_id IN (SELECT business_id FROM Attributes WHERE attr_name = 'WiFi' AND val = 'free') AND ";
                    else
                        query += $"business_id IN (SELECT business_id FROM Attributes WHERE attr_name = '{cb.Name}') AND ";
                }
            }
            if (query != "")
            {
                query = query.Remove(query.Length - 4); // remove extra AND from end of string
            }
            return query;
        }

        /// <summary>
        /// Creates a query string for all selected meal filter boxes
        /// </summary>
        /// <returns>A query string</returns>
        private string meal_filter_query()
        {
            string query = "";

            foreach (CheckBox cb in mealcheck.Children)
            {
                if (cb.IsChecked == true)
                    query += $"business_id IN (SELECT business_id FROM Attributes WHERE attr_name = 'GoodForMeal_{cb.Name}') AND ";
            }

            if (query != "")
            {
                query = query.Remove(query.Length - 4); // remove extra AND at end of string
            }


            return query;
        }

        /// <summary>
        /// Creates a query string for all selected price filter boxes
        /// </summary>
        /// <returns>A query string</returns>
        private string price_filter_query()
        {
            string query = "";
            foreach (CheckBox cb in innerprice.Children)
            {
                if (cb.IsChecked == true)
                {
                    if (query == "")
                        query = " business_id IN (SELECT business_id FROM Attributes WHERE ";
                    query += $" attr_name = 'RestaurantsPriceRange2' AND val = '{cb.Content.ToString().Length}' OR ";
                }
            }

            if (query != "")
            {
                query = query.Remove(query.Length - 3);
                query += ")";
            }

            return query;
        }

        private void sorted_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            query_filters();
        }

        private void addbutton_Click(object sender, RoutedEventArgs e)
        {
            if (catlist.SelectedItem != null)
            {
                foreach (string s in selectlist.Items )
                {
                    if (s == catlist.SelectedItem.ToString())
                        return;
                }
                selectlist.Items.Add(catlist.SelectedItem);
                query_filters();
            }
        }

        private void removebutton_Click(object sender, RoutedEventArgs e)
        {
            if (selectlist.SelectedItem != null)
            {
                selectlist.Items.Remove(selectlist.SelectedItem);
                query_filters();
            }
        }
    }
}

