using NLog;

namespace Suai.TeacherInfo.Service;

public class NLogFactory : ILoggerFactory
{
    public ILogger GetLogger(string className)
    {
        return LogManager.GetLogger(className);
    }
}
