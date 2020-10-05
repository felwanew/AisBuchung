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
            if (new VeranstalterModel().GetOrganizer(calendarPost.veranstalter) == null)
            {
                return -1;
            }
            if (DatabaseManager.GetId($"SELECT * FROM Kalender WHERE Name={Json.SerializeString(calendarPost.name)}") != null)
            {
                return -1;
            }

            var id = DatabaseManager.ExecutePost("Kalender", calendarPost.ToDictionary());
            DatabaseManager.ExecutePost("Kalenderberechtigte", calendarPost.ToAuthorizationDictionary(id));
            return id;
        }

        public bool PutCalendar(CalendarPost calendarPost, long calendarId)
        {
            if (DatabaseManager.GetId($"SELECT * FROM Kalender WHERE Name={Json.SerializeString(calendarPost.name)}") != null)
            {
                return false;
            }

            return DatabaseManager.ExecutePut("Kalender", calendarId, calendarPost.ToDictionary());
        }

        public string GetCalendarOrganizers(long calendarId)
        {
            var reader = DatabaseManager.ExecuteReader($"SELECT * FROM Kalenderberechtigte WHERE Kalender={calendarId}");
            var ids = DatabaseManager.ReadAsJsonArray(new Dictionary<string, string> { { "id", "Veranstalter" } }, reader);
            var array = Json.DeserializeArray(ids);
            var idList = new List<long>();
            foreach(var a in array)
            {
                idList.Add(Convert.ToInt64(Json.GetKvpValue(a, "id", false)));
            }
            var result = new NutzerModel().GetUsers(idList.ToArray());
            return Json.SerializeObject(new Dictionary<string, string> { { "veranstalter", result } });
        }

        public string GetCalendars(long[] ids)
        {
            return DatabaseManager.ExecuteGet("Kalender", ids, GetCalendarKeyTableDictionary());
        }

        public bool PostCalendarOrganizer(long calendarId, CalendarPost calendarPost)
        {
            var organizerId = calendarPost.veranstalter;
            if (DatabaseManager.CountResults($"SELECT * FROM Veranstalter WHERE Id={organizerId} AND Autorisiert=1") != 1)
            {
                return false;
            }

            if (DatabaseManager.CountResults($"SELECT * FROM Kalenderberechtigte WHERE Veranstalter={organizerId} AND Kalender={calendarId}") == 1)
            {
                return false;
            }

            DatabaseManager.ExecutePost("Kalenderberechtigte", calendarPost.ToAuthorizationDictionary(calendarId));
            return true;
        }

        public bool DeleteCalendar(long calendarId)
        {
            if (!DatabaseManager.ExecuteDelete("Kalender", calendarId))
            {
                return false;
            }
            else
            {
                DatabaseManager.ExecuteNonQuery($"DELETE FROM Veranstaltungen WHERE Kalender = {calendarId}");
                return CalendarManager.DeleteCalendar(calendarId);
            }
        }

        public long GetCalendarId(string name)
        {
            if (name == null)
            {
                return -1;
            }
            var command = $"SELECT * FROM Kalender WHERE Name={Json.SerializeString(name)}";

            var r = DatabaseManager.ExecuteReader(command);
            var result = DatabaseManager.ReadFirstAsJsonObject(GetCalendarKeyTableDictionary(), r, null);
            if (result == null) {
                return -1;
            }
            return Convert.ToInt64(Json.GetValue(result, "id", false));
        }

        public string GetCalendar(string name)
        {
            var command = "SELECT * FROM Kalender";
            if(name != null)
            {
                command += $" WHERE Name={Json.SerializeString(name)}";
            }

            var r = DatabaseManager.ExecuteReader(command);
            if (name != null)
            {
                return DatabaseManager.ReadFirstAsJsonObject(GetCalendarKeyTableDictionary(), r, null);
            }
            else
            {
                var result = DatabaseManager.ReadAsJsonArray(GetCalendarKeyTableDictionary(), r);
                return Json.SerializeObject(new Dictionary<string, string> { { "kalender", result } });
            }
        }

        public string GetCalendar(long id)
        {
            var r = DatabaseManager.ExecuteReader($"SELECT * FROM Kalender WHERE Id={id}");
            return DatabaseManager.ReadFirstAsJsonObject(GetCalendarKeyTableDictionary(), r, null);
        }

        public string GetCalendars()
        {
            return GetCalendar(null);
        }

        public bool DeleteCalendarOrganizer(long calendarId, long organizerId)
        {
            var c = DatabaseManager.CountResults($"SELECT * FROM Kalenderberechtigte WHERE Kalender={calendarId}");
            if (c <= 1)
            {
                return false;
            }
            else
            {
                var id = DatabaseManager.GetId($"SELECT * FROM Kalenderberechtigte WHERE Kalender={calendarId} AND Veranstalter={organizerId}");
                if (id == null)
                {
                    return false;
                }
                else
                {
                    return DatabaseManager.ExecuteDelete("Kalenderberechtigte", Convert.ToInt64(id));
                }
            }
        }

        public Dictionary<string, string> GetCalendarKeyTableDictionary()
        {
            return new Dictionary<string, string>
            {
                {"id", "Id" },
                {"name", "Name" },
            };
        }
    }

    public class CalendarPost : LoginData
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
                {"Kalender", calendarId.ToString() },
                {"Veranstalter", veranstalter.ToString() },
            };

            return result;
        }
    }
}
