using MonsterWebShop.Models;

namespace MonsterWebShop.Repo
{
    public interface ILogRepo
    {
        Log CreateLog(Log log);

        Log? GetLogById(int logId);

        List<Log> GetAllLogs(int maxRows = 100);

        List<Log> GetLogsByIPAddress(string ipAddress);

        List<Log> GetLogsBySessionId(string sessionId);

        List<Log> GetLogsByIncidentType(string incidentType);
    }
}