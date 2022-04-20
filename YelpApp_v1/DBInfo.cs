using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class User
    {
        public string usr_id { get; set; }
        public string name { get; set; }
        public double? average_stars { get; set; }
        public int? fans { get; set; }
        public int? cool { get; set; }
        public int? funny { get; set; }
        public int? tipcount { get; set; }
        public int? totallikes { get; set; }
        public int? useful { get; set; }
        public double? latitude { get; set; }
        public double? longitude { get; set; }
        public DateTime? yelping_since { get; set; }
    }

    public class Business
    {
        public Business()
        {
            attributes = new Dictionary<string, string>();
            hrs = new Dictionary<string, (TimeSpan?, TimeSpan?)>();
            categories = new List<string>();
        }

        public string bid { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public int? postal_code { get; set; }
        public double? latitude { get; set; }
        public double? longitude { get; set; }
        public double? stars { get; set; }
        public int? checkin_count { get; set; }
        public int? tip_count { get; set; }
        public bool? is_open { get; set; }
        public bool? is_visible { get; set; }
        
        // dictonary of attributes. 
        // key      : category
        // value    : attribute value. Most are true/false, but since some aren't, this needs to be a string :(
        private Dictionary<string, string> attributes { get; set; }

        // dictionary of business hours. 
        // key : day of the week
        // value : tuple(open time, close time)
        private Dictionary<string, (TimeSpan?, TimeSpan?)> hrs { get; set; }

        // list of categories applying to this business
        private List<string> categories { get; set; }


        public void insert_attribtue(string att, string val)
        {
            attributes.Add(att, val);
        }

        public void insert_category(string cat)
        {
            categories.Add(cat);
        }

        public void insert_hours(string day_of_week, TimeSpan? open, TimeSpan? close)
        {
            hrs.Add(day_of_week, (open, close));
        }

        public string get_attribute_val(string attr_name)
        {
            if (attributes.ContainsKey(attr_name))
                return attributes[attr_name];
            else return null;
        }

        public (TimeSpan?, TimeSpan?) get_hrs_of_day(string day)
        {
            if (hrs.ContainsKey(day))
                return hrs[day];
            else return (null, null);
        }
    }


    public class Tip
    {
        public DateTime? tipDate { get; set; }
        public string tipText { get; set; }
        public int? likes { get; set; }
        public string uid { get; set; }
        public string bid { get; set; }
    }

    public class DBInfo
    {
        private static string host = "localhost";
        private static string username = "postgres";
        private static string password = "123";
        private static string database = "yelpdb";

        public static string buildConnectionString()
        {
            return $"Host={host};Username={username};Database={database};Password={password}";
        }
    }

}
