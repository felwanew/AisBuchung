using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace AisBuchung_Api.Models
{
    public class DatenModel
    {

        public void WipeUnnecessaryData()
        {
            new BuchungenModel().WipeUnnecessaryData();
            new EmailverifizierungenModel().WipeUnnecessaryData();
            new TeilnehmerModel().WipeUnnecessaryData();
            new NutzerModel().WipeUnnecessaryData();
        }

        public void ClearData()
        {
            SaveData();

            if (File.Exists(CalendarManager.Path))
            {
                File.Delete(CalendarManager.Path);
            }

            if (File.Exists(DatabaseManager.Path))
            {
                File.Delete(DatabaseManager.Path);
            }
        }

        public void SaveData()
        {
            if (!Directory.Exists("Archiv"))
            {
                Directory.CreateDirectory("Archiv");
            }

            var directory = $"Archiv\\{CalendarManager.GetDateTime(DateTime.Now)}";

            Directory.CreateDirectory(directory);
            if (File.Exists(CalendarManager.Path))
            {
                File.Copy(CalendarManager.Path, $"{directory}\\{CalendarManager.Path}");
            }

            if (File.Exists(DatabaseManager.Path))
            {
                File.Copy(DatabaseManager.Path, $"{directory}\\{DatabaseManager.Path}");
            }
        }
    }
}
