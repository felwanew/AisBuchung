using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonSerializer;

namespace AisBuchung_Api.Models
{
    public class BuchungenModel
    {
        public string GetBookings(string eventUid, string queryString)
        {
            var eventId = GetEventId(eventUid);
            var result = GetBookingsByEventAsArray(eventId);

            var jsonData = Json.SerializeObject(new Dictionary<string, string> { { "buchungen", result } });
            return Json.QueryJsonData(jsonData, queryString, -1, false, false, Json.ArrayEntryOrKvpValue.ArrayEntry);
        }

        public string GetBookingsAsArray()
        {
            return GetBookingsAsArray(null);
        }

        public string GetBookingsAsArray(string queryString)
        {
            return Json.QueryJsonData(GetBookingsByEventAsArray(null), queryString, -1, false, false, Json.ArrayEntryOrKvpValue.ArrayEntry);
        }

        public string GetBookingsByEventAsArray(string eventId)
        {
            var command = $"SELECT * FROM Buchungen INNER JOIN Nutzerdaten ON Buchungen.Nutzer=Nutzerdaten.Id";
            if (eventId != null)
            {
                command += $" WHERE Veranstaltung={eventId}";
            }
            
            using (var reader = DatabaseManager.ExecuteReader(command))
            {
                return DatabaseManager.ReadAsJsonArray(GetKeyTableDictionary(), reader);
            }
        }

        public string GetBooking(long bookingId)
        {
            using (var reader = DatabaseManager.ExecuteReader($"SELECT * FROM Nutzerdaten INNER JOIN Buchungen ON Buchungen.Nutzer=Nutzerdaten.Id WHERE Buchungen.Id={bookingId}"))
            {
                return DatabaseManager.ReadFirstAsJsonObject(GetKeyTableDictionary(), reader, null);
            }
        }

        public bool PostBooking(BookingPost bookingPost, string eventUid)
        {
            var eventId = GetEventId(eventUid);
            bookingPost.veranstaltung = eventId;
            if (!CheckIfEventCanBeBooked(Convert.ToInt64(eventId))){
                return false;
            }

            var userId = new NutzerModel().PostUser(bookingPost.ToUserPost());
            var d = bookingPost.ToDictionary();
            d.Add("Nutzer", userId.ToString());
            return DatabaseManager.ExecutePost("Buchungen", d) != -1;
        }

        public string GetEventId(string eventUid)
        {
            eventUid = Json.DeserializeString(eventUid);
            return DatabaseManager.GetId($"SELECT * FROM Veranstaltungen WHERE Uid=\"{eventUid}\"");
        }

        public bool CheckIfEventCanBeBooked(long eventId)
        {
            var dt = CalendarManager.GetDateTime(DateTime.Now);
            var SelectById = $"SELECT * FROM Veranstaltungen WHERE Id={eventId}";
            var FilterByTimeLimit = $"Anmeldefrist>{dt}";
            var FilterByParticipantLimit = $"(Teilnehmerzahl < Teilnehmerlimit OR Teilnehmerlimit < 0)";
            return DatabaseManager.CountResults($"{SelectById} AND {FilterByTimeLimit} AND {FilterByParticipantLimit}") == 1;
        }

        public string GetEventIdOfBooking(long bookingId)
        {
            var booking = GetBooking(bookingId);
            return Json.GetValue(booking, "veranstaltung", false);
        }

        public void ProcessBookings(string eventUid)
        {
            var eventId = GetEventId(eventUid);

            var result = String.Empty;
            using (var reader = DatabaseManager.ExecuteReader($"SELECT * FROM Buchungen WHERE Veranstaltung={eventId}"))
            {
                result = DatabaseManager.ReadAsJsonArray(GetBookingKeyTableDictionary(), reader);
            }

            var arr = Json.DeserializeArray(result);

            foreach(var b in arr)
            {
                var id = Convert.ToInt64(Json.GetKvpValue(b, "id", false));
                ProcessBooking(id);
            }
        }

        public bool ProcessBooking(long bookingId)
        {
            var result = String.Empty;
            using (var reader = DatabaseManager.ExecuteReader($"SELECT * FROM Nutzerdaten INNER JOIN Buchungen ON Buchungen.Nutzer=Nutzerdaten.Id WHERE Buchungen.Id={bookingId}"))
            {
                result = DatabaseManager.ReadFirstAsJsonObject(GetKeyTableDictionary(), reader, null);
            }

            return ProcessBooking(result, bookingId);
        }

        public bool ProcessBooking(string booking, long bookingId)
        {
            if (booking == null)
            {
                return false;
            }

            var d = Json.DeserializeObject(booking);
            if (ProcessBooking(d))
            {
                DeleteBooking(bookingId);
                return true;
            }

            return false;
        }

        private bool ProcessBooking(Dictionary<string, string> booking)
        {
            var d = booking;
            var type = Json.GetKvpValue(d, "buchungstyp", false);

            if (Json.GetKvpValue(d, "verifiziert", false) == "0")
            {
                return false;
            }

            var eventId = Json.GetKvpValue(d, "veranstaltung", false);
            var e = new VeranstaltungenModel().GetEvent(Convert.ToInt64(eventId));
            var deadline = Json.GetKvpValue(e, "anmeldefrist", false);
            if (Convert.ToInt64(deadline) < Convert.ToInt64(CalendarManager.GetDateTime(DateTime.Now)))
            {
                return false;
            }

            if (type == "0")
            {
                return new TeilnehmerModel().AddParticipant(d) != -1;
            }

            if (type == "1")
            {
                return new TeilnehmerModel().DeleteParticipant(d);
            }

            return false;
        }

        public bool DeleteBooking(long bookingId)
        {
            return DatabaseManager.ExecuteDelete("Buchungen", bookingId);
        }

        public void WipeUnnecessaryData()
        {
            var bookings = Json.DeserializeArray(GetBookingsAsArray());
            var bookingEventDictionary = new Dictionary<string, string>();
            var eventIds = new List<string>();
            var outdatedEvents = new List<string>();
            foreach(var booking in bookings)
            {
                var o = Json.DeserializeObject(booking);
                var bookingId = Json.GetKvpValue(o, "id", false);
                var eventId = Json.GetKvpValue(o, "veranstaltung", false);
                bookingEventDictionary.Add(bookingId, eventId);
                if (!eventIds.Contains(eventId))
                {
                    eventIds.Add(eventId);
                }
            }

            var v = new VeranstaltungenModel();
            var timeNow = Convert.ToInt64(CalendarManager.GetDateTime(DateTime.Now));

            foreach (var eventId in eventIds)
            {
                var e = v.GetEvent(Convert.ToInt64(eventId));
                if (e == null)
                {
                    outdatedEvents.Add(eventId);
                    continue;
                }

                var deadline = Convert.ToInt64(Json.GetKvpValue(e, "anmeldefrist", false));
                if (deadline < timeNow)
                {
                    outdatedEvents.Add(eventId);
                }
            }

            DatabaseManager.ExecuteDelete("Buchungen", "Veranstaltung", outdatedEvents.ToArray());
        }

        public Dictionary<string, string> GetKeyTableDictionary()
        {
            return new Dictionary<string, string>
            {
                {"id", "Id" },
                {"nutzerId", "Nutzer" },
                {"buchungstyp", "Buchungstyp" },
                {"vorname", "Vorname" },
                {"nachname", "Nachname" },
                {"email", "Email" },
                {"abteilung", "Abteilung" },
                {"verifiziert", "Verifiziert" },
                {"veranstaltung", "Veranstaltung" },
                {"zeitstempel", "Zeitstempel" },
            };
        }

        public Dictionary<string, string> GetBookingKeyTableDictionary()
        {
            return new Dictionary<string, string>
            {
                {"id", "Id" },
                {"nutzerId", "Nutzer" },
                {"buchungstyp", "Buchungstyp" },
                {"zeitstempel", "Zeitstempel" },
            };
        }
    }

    public class BookingPost : LoginData
    {
        public string veranstaltung { get; set; }
        public int buchungstyp { get; set; }
        public string nachname { get; set; }
        public string vorname { get; set; }
        public string abteilung { get; set; }
        public string email { get; set; }

        public Dictionary<string, string> ToDictionary()
        {
            var result = new Dictionary<string, string> {
                {"Veranstaltung", veranstaltung },
                {"Buchungstyp", buchungstyp.ToString() },
                {"Zeitstempel", CalendarManager.GetDateTime(DateTime.Now) }
            };



            return result;
        }

        public UserPost ToUserPost()
        {
            return new UserPost { abteilung = abteilung, email = email, vorname = vorname, nachname = nachname };
        }
    }
}
