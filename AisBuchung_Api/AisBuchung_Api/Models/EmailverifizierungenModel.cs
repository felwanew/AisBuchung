using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonSerializer;

namespace AisBuchung_Api.Models
{
    public class EmailverifizierungenModel
    {
        public string GetAllCodes()
        {
            var reader = DatabaseManager.ExecuteReader("SELECT * FROM Emailverifizierungen");
            return DatabaseManager.ReadAsJsonArray(GetKeyTableDictionary(), reader, "emailverifizierungen");
        }

        public string AddNewCode(long userId, double hours)
        {
            var code = GenerateUniqueCode();
            var dt = DateTime.Now.AddHours(hours);
            var dateTime = CalendarManager.GetDateTime(dt);
            var dict = new Dictionary<string, string> {
                {"Nutzer", userId.ToString() },
                {"Zeitfrist", dateTime },
                {"Code", code },
            };

            if (DatabaseManager.ExecutePost("Emailverifizierungen", dict) > 0)
            {
                return code;
            }
            else
            {
                return null;
            }
        }

        public void WipeUnnecessaryData()
        {
            var dateTime = CalendarManager.GetDateTime(DateTime.Now);
            DatabaseManager.ExecuteNonQuery($"DELETE FROM Emailverifizierungen WHERE Zeitfrist<={dateTime}");
        }

        public string GenerateUniqueCode()
        {
            string result = null;
            do
            {
                result = Guid.NewGuid().ToString();
            }
            while (DatabaseManager.CountResults($"SELECT * FROM Emailverifizierungen WHERE Code=\"{result}\"") > 0);

            return result;
        }

        public bool ProcessVerification(string code)
        {
            var dateTime = CalendarManager.GetDateTime(DateTime.Now);
            var reader = DatabaseManager.ExecuteReader($"SELECT * FROM Emailverifizierungen WHERE Code=\"{code}\" AND Zeitfrist>={dateTime}");
            var r = DatabaseManager.ReadFirstAsJsonObject(new Dictionary<string, string> { { "nutzer", "Nutzer" } }, reader, null);
            var id = Convert.ToInt64(Json.GetKvpValue(r, "nutzer", false));
            if (new NutzerModel().VerifyUser(id) > 0)
            {
                DeleteVerificationCode(GetVerificationCodeId(code));
                return true;
            }
            else
            {
                return false;
            }
        }

        public long GetVerificationCodeId(string code)
        {
            var id = DatabaseManager.GetId($"SELECT * FROM Emailverifizierungen WHERE Code=\"{code}\"");
            if (id != null)
            {
                return Convert.ToInt64(id);
            }
            else
            {
                return -1;
            }
        }

        public bool DeleteVerificationCode(long id)
        {
            return DatabaseManager.ExecuteDelete("Emailverifizierungen", id);
        }

        public Dictionary<string, string> GetKeyTableDictionary()
        {
            return new Dictionary<string, string>
            {
                {"id", "Id" },
                {"code", "Code" },
                {"nutzer", "Nutzer" },
                {"zeitfrist", "Zeitfrist" },
            };
        }
    }
}
