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
            addColumnsToFriendGrid();
            loadBusinessDetails();
            loadFriendTips();
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
        private void addColumnsToFriendGrid()
        {
            DataGridTextColumn col1 = new DataGridTextColumn();
            col1.Binding = new Binding("tipDate");
            col1.Header = "Date";
            col1.Width = 255;
            friendtipgrid.Columns.Add(col1);

            DataGridTextColumn col2 = new DataGridTextColumn();
            col2.Binding = new Binding("tipText");
            col2.Header = "Text";
            col2.Width = 60;
            friendtipgrid.Columns.Add(col2);

            DataGridTextColumn col3 = new DataGridTextColumn();
            col3.Binding = new Binding("likes");
            col3.Header = "Likes";
            col3.Width = 150;
            friendtipgrid.Columns.Add(col3);
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

        private void loadFriendTips()
        {
            friendtipgrid.Items.Clear();
            if (UserWindow.selectedUser != null)
            {
                using (var connection = new NpgsqlConnection(DBInfo.buildConnectionString()))
                {
                    connection.Open();
                    List<User> userList = new List<User>();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = connection;                       
                        cmd.CommandText = $"SELECT * FROM Usr WHERE usr_id IN (SELECT friend_for FROM Friend WHERE friend_of = '{UserWindow.selectedUser.usr_id}');";
                        using (var reader = cmd.ExecuteReader())
                        {
                            try
                            {
                                while (reader.Read())
                                {
                                    User newUser = new User()
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
                                    };
                                    userList.Add(newUser);
                                }
                            }
                            catch (NpgsqlException ex)
                            {
                                Console.WriteLine(ex.Message);
                                System.Windows.MessageBox.Show($"SQL Error - {ex.Message}");
                            }
                        }
                    }
                    foreach (User friend in userList)
                    {
                        using (var cmd = new NpgsqlCommand("SELECT * " +
                                                "FROM Tip " +
                                                $"WHERE usr_id = '{friend.usr_id}' " +                                                
                                                $"AND business_id = '{bid}';", connection))
                        {

                            using (var reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    friendtipgrid.Items.Add(new Tip()
                                    {
                                        tipDate = reader["tipDate"] as DateTime?,
                                        tipText = reader["tipText"] as string,
                                        likes = reader["likes"] as int?,
                                        uid = reader["usr_id"] as string,
                                        bid = reader["business_id"] as string
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }



        private void addtipbutton_Click(object sender, RoutedEventArgs e)
        {
            if (tipentry.Text != "")
            {
                using (var connection = new NpgsqlConnection(DBInfo.buildConnectionString()))
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = connection;
                        cmd.CommandText = $"INSERT INTO tip (tipDate, tipText, likes, usr_id, business_id) VALUES ( '{DateTime.Now}', '{tipentry.Text}', 0, '{UserWindow.selectedUser.usr_id}', '{bid}')";
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
                tipentry.Clear();
            }
            loadBusinessDetails();
        }

        private void tiplikebutton_Click(object sender, RoutedEventArgs e)
        {
            if (tipgrid.SelectedItems.Count != 0)
            {
                Tip T = tipgrid.Items[tipgrid.SelectedIndex] as Tip;
                using (var connection = new NpgsqlConnection(DBInfo.buildConnectionString()))
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = connection;
                        cmd.CommandText = $"UPDATE Tip SET likes = likes + 1 WHERE business_id = '{T.bid}' AND usr_id = '{T.uid}' AND tipDate = '{T.tipDate}'";
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

        private void friendtiplikebutton_Click(object sender, RoutedEventArgs e)
        {
            if(tipgrid.SelectedItems.Count != 0)
            { 
                Tip T = friendtipgrid.Items[friendtipgrid.SelectedIndex] as Tip;
                using (var connection = new NpgsqlConnection(DBInfo.buildConnectionString()))
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = connection;
                        cmd.CommandText = $"UPDATE Tip SET likes = likes + 1 WHERE business_id = '{T.bid}' AND usr_id = '{T.uid}' AND tipDate = {T.tipDate}";
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
            loadFriendTips();
        }
    }
}
