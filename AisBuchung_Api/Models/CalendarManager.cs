using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical;
using Ical.Net.Serialization;
using Ical.Net.DataTypes;
using System.IO;
using JsonSerializer;

namespace AisBuchung_Api.Models
{
    public static class CalendarManager
    {
        public static string AddEvent(long organizerId, EventPost eventPost, string calendar)
        {
            var cc = GetCalendarCollection();
            var c = GetCalendar(calendar, cc);
            var result = AddEvent(organizerId, eventPost, c);
            if (result != null)
            {
                SaveCalendars(cc);
                return result;
            }
            else
            {
                return null;
            }
        }

        public static string AddEvent(long organizerId, EventPost eventPost, Calendar calendar)
        {
            var e = new CalendarEvent();
            if (EditEvent(organizerId, eventPost, e))
            {
                calendar.Events.Add(e);
                return e.Uid;
            }

            return null;
        }

        public static bool EditEvent(long organizerId, EventPost eventPost, string uid)
        {
            var cc = GetCalendarCollection();
            var e = GetEvent(uid, cc);
            if (e == null)
            {
                return false;
            }

            if (EditEvent(organizerId, eventPost, e))
            {
                SaveCalendars(cc);
                return true;
            }

            return false;
        }

        public static bool EditEvent(long organizerId, EventPost eventPost, CalendarEvent calenderEvent)
        {
            var e = calenderEvent;
            var ep = eventPost;

            e.Organizer.CommonName = organizerId.ToString();
            
            e.Summary = ep.name;
            e.Description = ep.beschreibung;
            e.DtStart.Value = GetDateTime(eventPost.datum, eventPost.startzeit);
            e.DtEnd.Value = GetDateTime(eventPost.datum, eventPost.endzeit);
            e.Location = ep.ort;
            e.DtStamp.Value = DateTime.Now;

            return true;
        }

        public static Calendar AddNewCalendar(string calendarName)
        {
            var cc = GetCalendarCollection();
            var result = AddNewCalendar(calendarName, cc);
            if (result != null)
            {
                SaveCalendars(cc);
                return result;
            }
            else
            {
                return null;
            }
            
        }

        public static Calendar AddNewCalendar(string calendarName, CalendarCollection calendars)
        {
            var c = GetCalendar(calendarName, calendars);
            if (c != null)
            {
                return null;
            }

            var result = new Calendar();
            result.Name = calendarName;
            calendars.Add(result);
            return result;
        }

        public static CalendarCollection GetCalendarCollection()
        {
            if (File.Exists(Path))
            {
                return CalendarCollection.Load(File.ReadAllText(Path));
            }
            else
            {
                return CreateCalendarCollection();
            }
            
        }

        public static CalendarCollection CreateCalendarCollection()
        {
            return new CalendarCollection();
        }

        public static string ReadEventsAsJsonArray(Calendar calendar)
        {
            var result = new List<string>();
            foreach (var e in calendar.Events)
            {
                result.Add(ReadEventAsJsonObject(e));
            }

            return Json.SerializeArray(result.ToArray());
        }

        public static string ReadEventAsJsonObject(Calendar calendar, string uid)
        {
            foreach (var e in calendar.Events)
            {
                if (uid == null || e.Uid == uid)
                {
                    return ReadEventAsJsonObject(e);
                }
            }

            return null;
        }

        public static string ReadEventAsJsonObject(CalendarCollection calendars, string uid)
        {
            foreach(var calendar in calendars)
            {
                var result = ReadEventAsJsonObject(calendar, uid);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public static string ReadEventAsJsonObject(CalendarEvent calendarEvent)
        {
            var e = calendarEvent;
            var o = new Dictionary<string, string>();
            Json.AddKeyValuePair(o, "id", e.Uid, true);
            Json.AddKeyValuePair(o, "name", e.Summary, true);
            Json.AddKeyValuePair(o, "beschreibung", e.Description, true);
            Json.AddKeyValuePair(o, "ort", e.Location, true);
            Json.AddKeyValuePair(o, "datum", GetDate(e.DtStart.Value), true);
            Json.AddKeyValuePair(o, "startzeit", GetDayTime(e.DtStart.Value), true);
            Json.AddKeyValuePair(o, "endzeit", GetDayTime(e.DtEnd.Value), true);
            Json.AddKeyValuePair(o, "jahr", e.DtStart.Year.ToString(), true);
            Json.AddKeyValuePair(o, "monat", e.DtStart.Month.ToString(), true);
            Json.AddKeyValuePair(o, "tag", e.DtStart.Day.ToString(), true);
            return Json.SerializeObject(o);
        }

        public static string GetDayTime(CalDateTime dateTime)
        {
            return $"{dateTime.Hour.ToString().PadLeft(2)}{dateTime.Minute.ToString().PadLeft(2)}";
        }

        public static string GetDate(CalDateTime dateTime)
        {
            return $"{dateTime.Year.ToString().PadLeft(4)}{dateTime.Month.ToString().PadLeft(2)}{dateTime.Day.ToString().PadLeft(2)}";
        }

        public static DateTime GetDateTime(string date, string time)
        {
            var Y = Convert.ToInt32(date.Substring(0, 4));
            var M = Convert.ToInt32(date.Substring(4, 2));
            var D = Convert.ToInt32(date.Substring(6, 2));
            var h = Convert.ToInt32(time.Substring(0, 2));
            var m = Convert.ToInt32(time.Substring(2, 2));
            return new DateTime(Y, M, D, h, m, 0);
        }

        public static CalendarEvent GetEvent(string uid, CalendarCollection calendars)
        {
            foreach(var c in calendars)
            {
                var e = GetEvent(uid, c);
                if (e != null)
                {
                    return e;
                }
            }

            return null;
        }

        public static CalendarEvent GetEvent(string uid, Calendar calendar)
        {
            foreach (var e in calendar.Events)
            {
                if (uid == null || e.Uid == uid)
                {
                    return e;
                }
            }

            return null;
        }

        public static Calendar GetCalendar(string calendarName)
        {
            var c = GetCalendarCollection();

            return GetCalendar(calendarName, c);
        }

        public static Calendar GetCalendar(string calendarName, CalendarCollection calendars)
        {
            foreach(var calendar in calendars)
            {
                if (calendar.Name == calendarName)
                {
                    return calendar;
                }
            }

            var result = AddNewCalendar(calendarName, calendars);
            return result;
        }

        public static bool SaveCalendars(CalendarCollection collection)
        {
            var t = new CalendarSerializer().SerializeToString(collection);
            File.WriteAllText(Path, t);
            return true;
        }

        public const string Path = "calendar.ics";
    }
}
