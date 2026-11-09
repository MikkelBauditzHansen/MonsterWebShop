using Microsoft.Data.SqlClient;
using MonsterWebShop.Models;
using System.Data;

namespace MonsterWebShop.Repo
{
    public class LogRepoDB : ILogRepo
    {
        private readonly string _connectionString;

        private const string SelectColumns = @"
            LogID,
            IPAddress,
            WhatTime,
            WhatHappened,
            SessionID,
            IncidentType";

        public LogRepoDB(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Log CreateLog(Log log)
        {
            if (string.IsNullOrWhiteSpace(log.IPAddress))
                throw new ArgumentException("IP-adresse mangler.");

            if (log.IPAddress.Length > 45)
                throw new ArgumentException("IP-adressen er for lang.");

            using SqlConnection connection =
                new SqlConnection(_connectionString);

            connection.Open();

            string sql = @"
                INSERT INTO Logging
                (
                    IPAddress,
                    WhatHappened,
                    SessionID,
                    IncidentType
                )
                OUTPUT INSERTED.LogID, INSERTED.WhatTime
                VALUES
                (
                    @IPAddress,
                    @WhatHappened,
                    @SessionID,
                    @IncidentType
                )";

            using SqlCommand command =
                new SqlCommand(sql, connection);

            command.Parameters.Add(
                "@IPAddress", SqlDbType.VarChar, 45
            ).Value = log.IPAddress;

            command.Parameters.Add(
                "@WhatHappened", SqlDbType.NVarChar, 255
            ).Value = (object?)log.WhatHappened ?? DBNull.Value;

            command.Parameters.Add(
                "@SessionID", SqlDbType.NVarChar, 255
            ).Value = (object?)log.SessionID ?? DBNull.Value;

            command.Parameters.Add(
                "@IncidentType", SqlDbType.NVarChar, 255
            ).Value = (object?)log.IncidentType ?? DBNull.Value;

            using SqlDataReader reader =
                command.ExecuteReader();

            if (!reader.Read())
                throw new InvalidOperationException(
                    "Loggen kunne ikke oprettes.");

            log.LogID = reader.GetInt32(0);
            log.WhatTime = reader.GetDateTime(1);

            return log;
        }

        public Log? GetLogById(int logId)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            connection.Open();

            string sql = $@"
                SELECT {SelectColumns}
                FROM Logging
                WHERE LogID = @LogID";

            using SqlCommand command =
                new SqlCommand(sql, connection);

            command.Parameters.Add(
                "@LogID", SqlDbType.Int
            ).Value = logId;

            using SqlDataReader reader =
                command.ExecuteReader();

            if (!reader.Read())
                return null;

            return MapLog(reader);
        }

        public List<Log> GetAllLogs(int maxRows = 100)
        {
            if (maxRows <= 0)
                throw new ArgumentException("maxRows skal være større end 0.");

            using SqlConnection connection =
                new SqlConnection(_connectionString);

            connection.Open();

            string sql = $@"
                SELECT TOP (@MaxRows) {SelectColumns}
                FROM Logging
                ORDER BY WhatTime DESC, LogID DESC";

            using SqlCommand command =
                new SqlCommand(sql, connection);

            command.Parameters.Add(
                "@MaxRows", SqlDbType.Int
            ).Value = maxRows;

            return ReadLogs(command);
        }

        public List<Log> GetLogsByIPAddress(string ipAddress)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            connection.Open();

            string sql = $@"
                SELECT {SelectColumns}
                FROM Logging
                WHERE IPAddress = @IPAddress
                ORDER BY WhatTime DESC, LogID DESC";

            using SqlCommand command =
                new SqlCommand(sql, connection);

            command.Parameters.Add(
                "@IPAddress", SqlDbType.VarChar, 45
            ).Value = ipAddress;

            return ReadLogs(command);
        }

        public List<Log> GetLogsBySessionId(string sessionId)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            connection.Open();

            string sql = $@"
                SELECT {SelectColumns}
                FROM Logging
                WHERE SessionID = @SessionID
                ORDER BY WhatTime DESC, LogID DESC";

            using SqlCommand command =
                new SqlCommand(sql, connection);

            command.Parameters.Add(
                "@SessionID", SqlDbType.NVarChar, 255
            ).Value = sessionId;

            return ReadLogs(command);
        }

        public List<Log> GetLogsByIncidentType(string incidentType)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            connection.Open();

            string sql = $@"
                SELECT {SelectColumns}
                FROM Logging
                WHERE IncidentType = @IncidentType
                ORDER BY WhatTime DESC, LogID DESC";

            using SqlCommand command =
                new SqlCommand(sql, connection);

            command.Parameters.Add(
                "@IncidentType", SqlDbType.NVarChar, 255
            ).Value = incidentType;

            return ReadLogs(command);
        }

        private static List<Log> ReadLogs(SqlCommand command)
        {
            List<Log> logs = new();

            using SqlDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                logs.Add(MapLog(reader));
            }

            return logs;
        }

        private static Log MapLog(SqlDataReader reader)
        {
            return new Log
            {
                LogID = reader.GetInt32(0),
                IPAddress = reader.GetString(1),
                WhatTime = reader.GetDateTime(2),
                WhatHappened = reader.IsDBNull(3)
                    ? null : reader.GetString(3),
                SessionID = reader.IsDBNull(4)
                    ? null : reader.GetString(4),
                IncidentType = reader.IsDBNull(5)
                    ? null : reader.GetString(5)
            };
        }
    }
}