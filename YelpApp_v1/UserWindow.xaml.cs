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
    /// Interaction logic for UserWindow.xaml
    /// </summary>
    public partial class UserWindow : Window
    {
        User selectedUser;

        public UserWindow()
        {
            InitializeComponent();
            addColumnsToUsrIdGrid();
            addColumnsToFriendsGrid();
            addColumnsToLatestTips();

            selectedUser = null;
        }

        private class LatestPost
        {
            public string user_name { get; set; }
            public string business { get; set; }
            public string city { get; set; }
            public string text { get; set; }
            public DateTime? date { get; set; }
        }
        

        private void addColumnsToUsrIdGrid()
        {
            DataGridTextColumn col1 = new DataGridTextColumn();
            col1.Binding = new Binding("usr_id");
            col1.Header = "User ID";
            col1.Width = usridgrid.Width;
            usridgrid.Columns.Add(col1);
        }

        
        

        private void updateUsridgrid(string name)
        {
            using (var connection = new NpgsqlConnection(DBInfo.buildConnectionString()))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = connection;
                    cmd.CommandText = $"SELECT * FROM Usr WHERE name = '{name}';";

                    try
                    {
                        // load users who have the same name into the datagrid
                        usridgrid.Items.Clear();

                        var reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            usridgrid.Items.Add(new User()
                            {
                                usr_id = reader["usr_id"].ToString(),
                                name = reader["name"].ToString(),
                                average_stars = reader["average_stars"] as double?,
                                fans = reader["fans"] as int?,
                                cool = reader["cool"] as int?,
                                funny = reader["funny"] as int?,
                                tipcount = reader["tipCount"] as int?,
                                totallikes = reader["totalLikes"] as int?,
                                useful = reader["useful"] as int?,
                                latitude = reader["latitude"] as double?,
                                longitude = reader["longitude"] as double?,
                                yelping_since = reader["yelping_since"] as DateTime?
                            });
                        }
                    }
                    catch (NpgsqlException ex)
                    {
                        Console.WriteLine(ex.Message);
                        System.Windows.MessageBox.Show($"SQL Error - {ex.Message}");
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }
        
        private void enterName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                updateUsridgrid(currentusrentry.Text);
            }
        }

        private void usridgrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (usridgrid.SelectedItem != null)
            { 
                selectedUser = (User)usridgrid.SelectedItem;

                // update the user info box
                name.Content = selectedUser.name.ToString();
                stars.Content = selectedUser.average_stars.ToString();
                fans.Content = selectedUser.fans.ToString();
                yelpingsince.Content = selectedUser.yelping_since.ToString();

                funny.Content = selectedUser.funny.ToString();
                cool.Content = selectedUser.cool.ToString();
                useful.Content = selectedUser.useful.ToString();
                
                tipcount.Content = selectedUser.tipcount.ToString();
                totaltiplikes.Content = selectedUser.totallikes.ToString();

                latentry.Text = selectedUser.latitude.ToString();
                longentry.Text = selectedUser.longitude.ToString();

                // update the friends grid
                updateFriendsGrid();
                updateLatestTipsGrid();
            }
        }

        

        private void editlocation(object sender, RoutedEventArgs e)
        {
            if (selectedUser != null)
            {
                latentry.IsEnabled = true;
                longentry.IsEnabled = true;
            }
        }

        private void updatelocation(object sender, RoutedEventArgs e)
        {
            if (selectedUser != null)
            {
                using (var connection = new NpgsqlConnection(DBInfo.buildConnectionString()))
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = connection;
                        if (!String.IsNullOrEmpty(longentry.Text)) // if a long was entered
                        {
                            cmd.CommandText = "UPDATE Usr SET longitude = " + Convert.ToDouble(longentry.Text) + " WHERE usr_id = '" + selectedUser.usr_id + "';";
                            selectedUser.longitude = Convert.ToDouble(longentry.Text);
                        }
                        else // if nothing was entered, update to NULL
                        {
                            cmd.CommandText = "UPDATE Usr SET longitude = NULL WHERE usr_id = '" + selectedUser.usr_id + "';";
                            selectedUser.longitude = null;
                        }

                        if (!String.IsNullOrEmpty(latentry.Text)) // if a lat was entered
                        {
                            cmd.CommandText += "UPDATE Usr SET latitude = " + Convert.ToDouble(latentry.Text) + " WHERE usr_id = '" + selectedUser.usr_id + "';";
                            selectedUser.latitude = Convert.ToDouble(latentry.Text);
                        }
                        else // if nothing was entered, update to NULL
                        {
                            cmd.CommandText += "UPDATE Usr SET latitude = NULL WHERE usr_id = '" + selectedUser.usr_id + "';";
                            selectedUser.latitude = null;
                        }
                        
                        // Execute update statements
                        try
                        {
                            cmd.ExecuteNonQuery();
                        }
                        catch (NpgsqlException ex)
                        {
                            Console.WriteLine(ex.Message);
                            System.Windows.MessageBox.Show($"SQL Error - {ex.Message}");
                        }
                        finally
                        {
                            connection.Close();
                        }
                    }
                }

                latentry.IsEnabled = false;
                longentry.IsEnabled = false;
            }
        }

        /// <summary>
        /// Friends Grid Functions
        /// </summary>
        private void addColumnsToFriendsGrid()
        {
            DataGridTextColumn name_col = new DataGridTextColumn();
            name_col.Binding = new Binding("name");
            name_col.Header = "Name";
            name_col.Width = friendsgrid.Width / 3;
            friendsgrid.Columns.Add(name_col);

            DataGridTextColumn totalLikes_col = new DataGridTextColumn();
            totalLikes_col.Binding = new Binding("totallikes");
            totalLikes_col.Header = "Total Likes";
            totalLikes_col.Width = friendsgrid.Width / 6;
            friendsgrid.Columns.Add(totalLikes_col);

            DataGridTextColumn avgStars_col = new DataGridTextColumn();
            avgStars_col.Binding = new Binding("average_stars");
            avgStars_col.Header = "Average Stars";
            avgStars_col.Width = friendsgrid.Width / 6;
            friendsgrid.Columns.Add(avgStars_col);

            DataGridTextColumn yelpingSince_col = new DataGridTextColumn();
            yelpingSince_col.Binding = new Binding("yelping_since");
            yelpingSince_col.Header = "Yelping Since";
            yelpingSince_col.Width = friendsgrid.Width / 3;
            friendsgrid.Columns.Add(yelpingSince_col);
        }
        
        private void updateFriendsGrid()
        {
            friendsgrid.Items.Clear();

            using (var connection = new NpgsqlConnection(DBInfo.buildConnectionString()))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = connection;
                    cmd.CommandText = $"SELECT * FROM Usr WHERE usr_id IN (SELECT friend_for FROM Friend WHERE friend_of = '{selectedUser.usr_id}');";
                    try
                    {
                        var reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            friendsgrid.Items.Add(new User()
                            {
                                usr_id = reader["usr_id"].ToString(),
                                name = reader["name"].ToString(),
                                average_stars = reader["average_stars"] as double?,
                                fans = reader["fans"] as int?,
                                cool = reader["cool"] as int?,
                                funny = reader["funny"] as int?,
                                tipcount = reader["tipCount"] as int?,
                                totallikes = reader["totalLikes"] as int?,
                                useful = reader["useful"] as int?,
                                latitude = reader["latitude"] as double?,
                                longitude = reader["longitude"] as double?,
                                yelping_since = reader["yelping_since"] as DateTime?
                            });
                        }
                    }
                    catch (NpgsqlException ex)
                    {
                        Console.WriteLine(ex.Message);
                        System.Windows.MessageBox.Show($"SQL Error - {ex.Message}");
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        private void friendsgrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            updateSelectedUserInfo((User)friendsgrid.SelectedItem);
            currentusrentry.Text = selectedUser.name;
            updateUsridgrid(currentusrentry.Text);
            updateFriendsGrid();
        }

        // Latest Tips Grid
        private void addColumnsToLatestTips()
        {
            
            DataGridTextColumn name_col = new DataGridTextColumn();
            name_col.Binding = new Binding("user_name");
            name_col.Header = "Name";
            name_col.Width = latesttipsgrid.Width / 6;
            latesttipsgrid.Columns.Add(name_col);

            DataGridTextColumn business_name_col = new DataGridTextColumn();
            business_name_col.Binding = new Binding("business");
            business_name_col.Header = "Business";
            business_name_col.Width = latesttipsgrid.Width / 6;
            latesttipsgrid.Columns.Add(business_name_col);

            DataGridTextColumn city_col = new DataGridTextColumn();
            city_col.Binding = new Binding("city");
            city_col.Header = "City";
            city_col.Width = latesttipsgrid.Width / 6;
            latesttipsgrid.Columns.Add(city_col);

            DataGridTextColumn text_col = new DataGridTextColumn();
            text_col.Binding = new Binding("text");
            text_col.Header = "Text";
            text_col.Width = latesttipsgrid.Width / 3;
            latesttipsgrid.Columns.Add(text_col);

            DataGridTextColumn date_col = new DataGridTextColumn();
            date_col.Binding = new Binding("date");
            date_col.Header = "Date";
            date_col.Width = latesttipsgrid.Width / 6;
            latesttipsgrid.Columns.Add(date_col);
        }

        private void updateLatestTipsGrid()
        {
            latesttipsgrid.Items.Clear();
            using (var connection = new NpgsqlConnection(DBInfo.buildConnectionString()))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = connection;
                    try
                    {
                        NpgsqlDataReader reader = null;
                        foreach (User friend in friendsgrid.Items)
                        {
                            cmd.CommandText = "SELECT name, city, tipText, tipDate " +
                                              "FROM Business, Tip " +
                                             $"WHERE usr_id = '{friend.usr_id}' " +
                                              "AND Tip.business_id = Business.business_id AND tipDate IS NOT NULL " +
                                              "AND tipDate IN (SELECT MAX(tipDate) " +
                                              "FROM Tip as T2 WHERE T2.usr_id = Tip.usr_id GROUP BY usr_id);";
                            reader = cmd.ExecuteReader();
                            if (reader.Read())
                            {
                                latesttipsgrid.Items.Add(new LatestPost()
                                {
                                    user_name = friend.name,
                                    business = reader["name"].ToString(),
                                    city = reader["city"].ToString(),
                                    text = reader["tipText"].ToString(),
                                    date = reader["tipDate"] as DateTime?
                                });
                            }
                            reader.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        System.Windows.MessageBox.Show($"SQL Error - {ex.Message}");
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        // General Functions
        private void updateSelectedUserInfo(User newSelection)
        {
            if (newSelection != null)
            {
                selectedUser = newSelection;

                // update the user info box
                name.Content = selectedUser.name.ToString();
                stars.Content = selectedUser.average_stars.ToString();
                fans.Content = selectedUser.fans.ToString();
                yelpingsince.Content = selectedUser.yelping_since.ToString();

                funny.Content = selectedUser.funny.ToString();
                cool.Content = selectedUser.cool.ToString();
                useful.Content = selectedUser.useful.ToString();

                tipcount.Content = selectedUser.tipcount.ToString();
                totaltiplikes.Content = selectedUser.totallikes.ToString();

                latentry.Text = selectedUser.latitude.ToString();
                longentry.Text = selectedUser.longitude.ToString();

                // update the friends grid
                updateFriendsGrid();

                // update the latest tips grid
                updateLatestTipsGrid();
            }
        }

   
    }
}
