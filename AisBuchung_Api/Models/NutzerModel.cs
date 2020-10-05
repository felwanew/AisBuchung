using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonSerializer;

namespace AisBuchung_Api.Models
{
    public class NutzerModel
    {


        public long PostUser(UserPost userPost)
        {
            var email = userPost.email;
            if (email == null || DatabaseManager.CountResults($"SELECT * FROM Nutzer WHERE Email={email}") > 0)
            {
                return -1;
            }
            return DatabaseManager.ExecutePost("Nutzerdaten", userPost.ToDictionary());
        }

        public bool PutUser(long id, UserPost userPost)
        {
            return DatabaseManager.ExecutePut("Nutzerdaten", id, userPost.ToDictionary());
        }

    }

    public class UserPost
    {
        public string vorname { get; set; }
        public string nachname { get; set; }
        public string email { get; set; }
        public string abteilung { get; set; }

        public Dictionary<string, string> ToDictionary()
        {
            var result = new Dictionary<string, string>
            {
                {"Vorname", Json.SerializeString(vorname) },
                {"Nachname", Json.SerializeString(nachname) },
                {"Email", Json.SerializeString(email) },
                {"Abteilung", Json.SerializeString(abteilung) }
            };

            return result;
        }
    }
}
