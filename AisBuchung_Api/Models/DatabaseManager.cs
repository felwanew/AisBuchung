using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Data.Sqlite;
using JsonSerializer;

namespace AisBuchung_Api.Models
{
    public static class DatabaseManager
    {

        public static void CreateNewDatabase(bool overwrite)
        {
            if (File.Exists(Path))
            {
                if (overwrite)
                {
                    File.Delete(Path);
                }
                else
                {
                    return;
                }
            }

            File.WriteAllText(Path, "");

            var createCommands = new string[]
            {
                $"CREATE TABLE Admins (Id INTEGER PRIMARY KEY)",
                $"CREATE TABLE Buchungen (Id INTEGER PRIMARY KEY AUTOINCREMENT, Veranstaltung TEXT NOT NULL, Nutzer INTEGER NOT NULL, Buchungstyp INTEGER NOT NULL, Verifiziert INTEGER NOT NULL, Verarbeitung INTEGER NOT NULL, Zeitstempel INTEGER NOT NULL)",
                $"CREATE TABLE Kalender (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL)",
                $"CREATE TABLE Kalenderberechtigte (Id INTEGER PRIMARY KEY AUTOINCREMENT, Kalender INTEGER NOT NULL, Veranstalter INTEGER NOT NULL)",
                $"CREATE TABLE Nutzerdaten (Id INTEGER PRIMARY KEY AUTOINCREMENT, Nachname TEXT NOT NULL, Vorname TEXT NOT NULL, Email TEXT NOT NULL, Abteilung INTEGER NOT NULL)",
                $"CREATE TABLE Teilnehmer (Id INTEGER PRIMARY KEY AUTOINCREMENT, Veranstaltung TEXT NOT NULL, Nutzer INTEGER NOT NULL)",
                $"CREATE TABLE Veranstalter (Id INTEGER PRIMARY KEY AUTOINCREMENT, Passwort TEXT NOT NULL)",
                $"CREATE TABLE Veranstaltung (Id TEXT PRIMARY KEY, Teilnahmelimit INTEGER NOT NULL, Anmeldefrist INTEGER NOT NULL, Anmeldetyp INTEGER NOT NULL, Abmeldetyp INTEGER NOT NULL)",
            };

            ExecuteNonQuery(createCommands);
        }

        public static SqliteDataReader ExecuteReader(string command)
        {
            var result = new List<string>();
            if (!File.Exists(Path))
            {
                CreateNewDatabase(false);
            }

            var connectionBuilder = new SqliteConnectionStringBuilder { DataSource = Path };
            var connection = new SqliteConnection(connectionBuilder.ConnectionString);
            connection.Open();
            var c = connection.CreateCommand();
            c.CommandText = command;
            var r = c.ExecuteReader();
            //r.Read();
            return r;
        }

        public static void ExecuteNonQuery(string[] commands)
        {
            if (!File.Exists(Path))
            {
                CreateNewDatabase(false);
            }

            var connectionBuilder = new SqliteConnectionStringBuilder { DataSource = Path };
            var connection = new SqliteConnection(connectionBuilder.ConnectionString);
            connection.Open();
            var command = connection.CreateCommand();
            foreach (var c in commands)
            {
                command.CommandText = c;
                command.ExecuteNonQuery();
            }
        }

        public static int CountResults(string command)
        {
            var r = ExecuteReader(command);
            var result = 0;
            using (r)
            {
                while (r.Read())
                {
                    result += 1;
                }
            }

            return result;
        }

        public static long ExecutePost(string table, Dictionary<string, string> keyValuePairs)
        {
            if (!File.Exists(Path))
            {
                CreateNewDatabase(false);
            }

            var columns = String.Join(", ", keyValuePairs.Keys);
            var v = new List<string>();
            foreach(var kvp in keyValuePairs)
            {
                if (kvp.Value != null)
                {
                    v.Add($"@{kvp.Key}");
                }
                else
                {
                    v.Add("NULL");
                }
            }

            var values = String.Join(", ", v);
            

            var command = $"INSERT INTO {table} ({columns}) VALUES ({values})";

            var connectionBuilder = new SqliteConnectionStringBuilder { DataSource = Path };
            var connection = new SqliteConnection(connectionBuilder.ConnectionString);
            connection.Open();
            var c = connection.CreateCommand();
            c.CommandText = command;

            foreach(var kvp in keyValuePairs)
            {
                if (kvp.Value == null)
                {
                    /*
                    var p = new SqliteParameter($"@{kvp.Key}", GetMax(table, kvp.Key) + 1);
                    p.SqliteType = SqliteType.Integer;
                    c.Parameters.Add(p);
                    */
                    c.Parameters.Add(new SqliteParameter($"@{kvp.Key}", DBNull.Value));
                }
                else
                {
                    var p = new SqliteParameter($"@{kvp.Key}", $"{kvp.Value}");
                    switch (Json.CheckValueType(kvp.Value))
                    {
                        case Json.ValueType.Number: p.SqliteType = SqliteType.Integer; break;
                        case Json.ValueType.String: p.SqliteType = SqliteType.Text; p.Value = Json.DeserializeString(kvp.Value); break;
                        case Json.ValueType.Invalid: p.SqliteType = SqliteType.Text; break;
                    }

                    c.Parameters.Add(p);
                }
                
            }

            var success = c.ExecuteNonQuery();

            if (success == 1)
            {
                c.CommandText = @"SELECT last_insert_rowid()";
                return (long)c.ExecuteScalar();
            }

            return -1;
        }

        public static bool ExecutePut(string table, long id, Dictionary<string, string> keyValuePairs)
        {
            if (!File.Exists(Path))
            {
                CreateNewDatabase(false);
            }

            var v = new List<string>();
            foreach (var kvp in keyValuePairs)
            {
                if (kvp.Value != null)
                {
                    v.Add($"{kvp.Key} = {kvp.Value}");
                }
            }

            var values = String.Join(", ", v);


            var command = $"UPDATE {table} SET {values} WHERE Id = {id}";

            var connectionBuilder = new SqliteConnectionStringBuilder { DataSource = Path };
            var connection = new SqliteConnection(connectionBuilder.ConnectionString);
            connection.Open();
            var c = connection.CreateCommand();
            c.CommandText = command;

            var success = c.ExecuteNonQuery();

            if (success == 1)
            {
                return true;
            }

            return false;
        }


        public static long GetMax(string table, string column)
        {
            var connectionBuilder = new SqliteConnectionStringBuilder { DataSource = Path };
            var connection = new SqliteConnection(connectionBuilder.ConnectionString);
            connection.Open();
            var c = connection.CreateCommand();
            c.CommandText = $"SELECT MAX({column}) FROM {table}";
            var r = c.ExecuteReader();
            if (r.Read())
            {
                if (!r.IsDBNull(0))
                {
                    var l = r.GetInt64(0);
                    connection.Close();
                    return l;
                }
                else
                {
                    connection.Close();
                    return 0;
                }
            }
            else
            {
                connection.Close();
                return 0;
            }
        }

        public static string ReadAsJsonArray(Dictionary<string, string> keyTableDictionary, SqliteDataReader reader)
        {
            var result = new List<string>();
            using (reader)
                while (reader.Read())
                {
                    var d = new Dictionary<string, string>();
                    foreach(var kvp in keyTableDictionary)
                    {
                        var v = reader[kvp.Value];
                        d[kvp.Key] = Convert.ToString(v);
                    }
                    result.Add(Json.SerializeObject(d));
                }

            return Json.SerializeArray(result.ToArray());
        }

        public static string ReadFirstAsJsonObject(Dictionary<string, string> keyTableDictionary, SqliteDataReader reader, string objectKey)
        {
            using (reader)
                if (reader.Read())
                {
                    var d = new Dictionary<string, string>();
                    foreach (var kvp in keyTableDictionary)
                    {
                        var v = reader[kvp.Value];
                        d[kvp.Key] = Convert.ToString(v);
                    }

                    var result = Json.SerializeObject(d);
                    if (objectKey != null)
                    {
                        result = Json.SerializeObject(new Dictionary<string, string> { { objectKey, result } });
                    }

                    return result;
                }

            return null;
        }

        public const string Path = "AisBuchungen.db";

        public struct Table
        {
            public const string Buchungen = "Buchungen";
            public const string Nutzerdaten = "Nutzerdaten";
            public const string Teilnehmer = "Teilnehmer";
            public const string Veranstalter = "Veranstalter";
            public const string Veranstaltungen = "Veranstaltungen";
        }
    }
}
