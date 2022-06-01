using NLog;

namespace Suai.TeacherInfo.Service;

public interface ILoggerFactory
{
    ILogger GetLogger(string className);
}
