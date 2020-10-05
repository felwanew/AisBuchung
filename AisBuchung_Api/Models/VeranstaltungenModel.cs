using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonSerializer;

namespace AisBuchung_Api.Models
{
    public class VeranstaltungenModel
    {
        public string GetEvents(long organizerId, string queryString)
        {
            var result = String.Empty;
            var command = "SELECT * FROM Veranstaltungen";
            using (var reader = DatabaseManager.ExecuteReader(command))
            {
                result = DatabaseManager.ReadAsJsonArray(GetEventKeyTableDictionary(), reader);
            }

            var jsonData = Json.SerializeObject(new Dictionary<string, string> { { "veranstalter", result } });
            return Json.QueryJsonData(jsonData, queryString, -1, false, false, Json.ArrayEntryOrKvpValue.ArrayEntry);
        }

        public string PostEvent(long organizerId, string calendar, EventPost eventPost)
        {
            var uid = CalendarManager.AddEvent(organizerId, eventPost, calendar);
            return uid;
        }

        public bool PutEvent(long organizerId, string uid, EventPost eventPost)
        {
            var result = CalendarManager.EditEvent(organizerId, eventPost, uid);
            return result;
        }

        public Dictionary<string, string> GetEventKeyTableDictionary()
        {
            return new Dictionary<string, string>
            {
                {"id", "Id" },
                {"teilnahmelimit", "Teilnahmelimit" },
                {"anmeldefrist", "Anmeldefrist" },
                {"anmeldetyp", "Anmeldetyp" },
                {"abmeldetyp", "Abmeldetyp" },
            };
        }


    }

    public class EventPost{
        public long veranstalter { get; set; }
        public string name { get; set; }
        public string beschreibung { get; set; }
        public string datum { get; set; }
        public string startzeit { get; set; }
        public string endzeit { get; set; }
        public string ort { get; set; }
        public string teilnahmelimit { get; set; }
        public string anmeldefrist { get; set; }
        public int anmeldetyp { get; set; }
        public int abmeldetyp { get; set; }

        public Dictionary<string, string> ToDictionary()
        {
            var result = new Dictionary<string, string>
            {
                {"Teilnahmelimit", teilnahmelimit.ToString() },
                {"Anmeldetyp", anmeldetyp.ToString() },
                {"Abmeldetyp", anmeldetyp.ToString() },
            };
            if (anmeldefrist == null)
            {
                if (startzeit != null)
                {
                    anmeldefrist = startzeit;
                    result["Anmeldefrist"] = anmeldefrist;
                }
            }
            else
            {
                result["Anmeldefrist"] = anmeldefrist;
            }

            return result;
        }
    }
}
