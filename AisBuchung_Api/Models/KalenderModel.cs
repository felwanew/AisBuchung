using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonSerializer;

namespace AisBuchung_Api.Models
{
    public class KalenderModel
    {
        public long PostCalendar(CalendarPost calendarPost)
        {
            var name = calendarPost.name;
            var c = CalendarManager.AddNewCalendar(name);
            if (c != null)
            {
                var id = DatabaseManager.ExecutePost("Kalender", calendarPost.ToDictionary());
                DatabaseManager.ExecutePost("Kalenderberechtigte", calendarPost.ToAuthorizationDictionary(id));
                return id;
            }
            else
            {
                return -1;
            }

            
        }



    }

    public class CalendarPost
    {
        public long veranstalter { get; set; }
        public string name { get; set; }

        public Dictionary<string, string> ToDictionary()
        {
            var result = new Dictionary<string, string>
            {
                {"Name", Json.SerializeString(name) },
            };

            return result;
        }

        public Dictionary<string, string> ToAuthorizationDictionary(long calendarId)
        {
            var result = new Dictionary<string, string>
            {
                {"Calendar", calendarId.ToString() },
                {"Veranstalter", veranstalter.ToString() },
            };

            return result;
        }
    }
}
