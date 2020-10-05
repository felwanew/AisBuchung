﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonSerializer;

namespace AisBuchung_Api.Models
{
    public class AdminsModel
    {
        public string GetAdmins(string queryString)
        {
            var result = String.Empty;
            using (var reader = DatabaseManager.ExecuteReader("SELECT * FROM Admins INNER JOIN Nutzerdaten ON Admins.Id=Nutzerdaten.Id"))
            {
                result = DatabaseManager.ReadAsJsonArray(GetAdminKeyTableDictionary(), reader);
            }

            var jsonData = Json.SerializeObject(new Dictionary<string, string> { { "admins", result } });
            return Json.QueryJsonData(jsonData, queryString, -1, false, false, Json.ArrayEntryOrKvpValue.ArrayEntry);
        }

        public string GetAdmin(long id)
        {
            var command = $"SELECT * FROM Admins INNER JOIN Nutzerdaten ON Admins.Id=Nutzerdaten.Id WHERE Admins.Id={id}";
            var r = DatabaseManager.ExecuteReader(command);
            return DatabaseManager.ReadFirstAsJsonObject(GetAdminKeyTableDictionary(), r, null);
        }
        public bool PostAdmin(long organizerId)
        {
            var organizer = new VeranstalterModel().GetOrganizer(organizerId);

            if (organizer != null && GetAdmin(organizerId) == null)
            {
                if (Json.GetKvpValue(organizer, "verifiziert", false) == "0" || Json.GetKvpValue(organizer, "autorisiert", false) == "0")
                {
                    return false;
                }

                var adminPost = new Dictionary<string, string> { { "id", organizerId.ToString() } };
                return DatabaseManager.ExecutePost("Admins", adminPost) == organizerId;
            }
            else
            {
                return false;
            }
            
        }

        public bool DeleteAdmin(long adminId)
        {
            return DatabaseManager.ExecuteDelete("Admins", adminId);
        }

        public Dictionary<string, string> GetAdminKeyTableDictionary()
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

    public class AdminsPost : LoginData
    {
        public long veranstalter { get; set; }
    }
}
