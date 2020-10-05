using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonSerializer;

namespace AisBuchung_Api.Models
{
    public class VeranstalterModel
    {
        public string GetOrganizers(string queryString)
        {
            var result = String.Empty;
            using (var reader = DatabaseManager.ExecuteReader("SELECT * FROM Veranstalter INNER JOIN Nutzerdaten ON Veranstalter.Id=Nutzerdaten.Id"))
            {
                result = DatabaseManager.ReadAsJsonArray(GetOrganizerKeyTableDictionary(), reader);
            }

            var jsonData = Json.SerializeObject(new Dictionary<string, string> { { "veranstalter", result } });
            return Json.QueryJsonData(jsonData, queryString, -1, false, false, Json.ArrayEntryOrKvpValue.ArrayEntry);
        }

        public string GetOrganizer(long id)
        {
            var command = $"SELECT * FROM Veranstalter INNER JOIN Nutzerdaten ON Veranstalter.Id=Nutzerdaten.Id WHERE Veranstalter.Id={id}";
            var r = DatabaseManager.ExecuteReader(command);
            return DatabaseManager.ReadFirstAsJsonObject(GetOrganizerKeyTableDictionary(), r, "veranstalter");
        }

        public long PostOrganizer(OrganizerPost organizerPost)
        {
            var id = new NutzerModel().PostUser(organizerPost.ToUserPost());
            if (id == -1)
            {
                return -1;
            }

            var result = DatabaseManager.ExecutePost("Veranstalter", organizerPost.ToDictionary());

            if (DatabaseManager.CountResults("SELECT * FROM Admins") == 0)
            {
                new AdminsModel().PostAdmin(result);
            }

            return result;
        }

        public bool PutOrganizer(long id, OrganizerPost organizerPost)
        {
            var user = GetOrganizer(id);
            if (user == null)
            {
                return false;
            }

            return DatabaseManager.ExecutePut("Veranstalter", id, organizerPost.ToDictionary());
        }

        public Dictionary<string, string> GetOrganizerKeyTableDictionary()
        {
            return new Dictionary<string, string>
            {
                {"id", "Id" },
                {"vorname", "Vorname" },
                {"nachname", "Nachname" },
                {"email", "Email" },
                {"abteilung", "Abteilung" },
            };
        }


    }

    public class OrganizerPost{
        public string passwort { get; set; }
        public string vorname { get; set; }
        public string nachname { get; set; }
        public string email { get; set; }
        public string abteilung { get; set; }

        public Dictionary<string, string> ToDictionary()
        {
            var result = new Dictionary<string, string>
            {
                {"Passwort", Json.SerializeString(passwort) }
            };

            return result;
        }

        public UserPost ToUserPost()
        {
            return new UserPost { abteilung = abteilung, email = email, vorname = vorname, nachname = nachname };
        }
    }
}
