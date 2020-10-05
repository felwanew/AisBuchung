using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonSerializer;

namespace AisBuchung_Api.Models
{
    public class AuthModel
    {
        public bool CheckIfCalendarPermissions(LoginData loginData, long calendarId)
        {
            if (CheckIfAllPermissions(loginData))
            {
                return true;
            }

            var organizerId = GetLoggedInOrganizer(loginData);
            return CheckIfCalendarPermissions(organizerId, calendarId);
        }

        public bool CheckIfCalendarPermissions(long organizerId, long calendarId)
        {
            if (CheckIfAllPermissions(organizerId))
            {
                return true;
            }

            return CheckIfAdminPermissions(organizerId) || DatabaseManager.CountResults($"SELECT * FROM Kalenderberechtigte WHERE Kalender={calendarId} AND Veranstalter={organizerId}") == 1;
        }

        public bool CheckIfOrganizerPermissions(long organizerId)
        {
            if (CheckIfAllPermissions(organizerId))
            {
                return true;
            }

            return organizerId > 0;
        }

        public bool CheckIfOrganizerPermissions(LoginData loginData)
        {
            if (CheckIfAllPermissions(loginData))
            {
                return true;
            }

            var organizerId = GetLoggedInOrganizer(loginData);
            return CheckIfOrganizerPermissions(organizerId);
        }

        public bool CheckIfOrganizerPermissions(LoginData loginData, long organizerId)
        {
            if (CheckIfAllPermissions(loginData))
            {
                return true;
            }

            var loginId = GetLoggedInOrganizer(loginData);
            return loginId == organizerId || CheckIfAdminPermissions(loginId);
        }

        public bool CheckIfAdminPermissions(LoginData loginData)
        {
            if (CheckIfAllPermissions(loginData))
            {
                return true;
            }

            var organizerId = GetLoggedInOrganizer(loginData);
            return CheckIfAdminPermissions(organizerId);
        }

        public bool CheckIfAdminPermissions(long organizerId)
        {
            if (CheckIfAllPermissions(organizerId))
            {
                return true;
            }

            return DatabaseManager.CountResults($"SELECT * FROM Admins WHERE Id={organizerId}") == 1;
        }

        public bool CheckIfAllPermissions(LoginData loginData)
        {
            return CheckIfDebugPermissionsTakePriority() && CheckIfDebugPermissions(loginData);
        }

        public bool CheckIfAllPermissions(long organizerId)
        {
            return CheckIfDebugPermissionsTakePriority() && CheckIfDebugPermissions(organizerId);
        }

        public bool CheckIfDebugPermissionsTakePriority()
        {
            return false;
        }

        public bool CheckIfDebugPermissions(LoginData loginData)
        {
            return true;
        }

        public bool CheckIfDebugPermissions(long organizerId)
        {
            return true;
        }



        public long GetLoggedInOrganizer(LoginData loginData)
        {
            var email = Json.SerializeString(loginData.ml);
            var passwort = Json.SerializeString(loginData.pw);
            var result = DatabaseManager.GetId($"SELECT * FROM Veranstalter INNER JOIN Nutzerdaten ON Veranstalter.Id=Nutzerdaten.Id " +
                $"WHERE Email={email} AND Passwort={passwort} AND Verifiziert=1 AND Autorisiert=1");

            if (result != null)
            {
                return Convert.ToInt64(result);
            }
            else
            {
                return -1;
            }
        }

        public string GetLoggedInOrganizerData(LoginData loginData)
        {
            var id = GetLoggedInOrganizer(loginData);
            if (id == -1)
            {
                return null;
            }
            else
            {
                return new VeranstalterModel().GetOrganizer(id);
            }
            
        }

    }

    public abstract class LoginData
    {
        public string ml { get; set; }
        public string pw { get; set; }
    }

    public class LoginPost : LoginData
    {

    }
}
