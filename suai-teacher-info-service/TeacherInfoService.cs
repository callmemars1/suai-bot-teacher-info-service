using Grpc.Core;
using Suai.Bot.TeacherInfo.Proto;
using Suai.TeacherInfo.Service.Database;
using NLog;

namespace Suai.TeacherInfo.Service;

public class GrpcTeacherInfoService : TeacherInfoProvider.TeacherInfoProviderBase
{
    private readonly ITeacherInfoProvider _teacherInfoProvider;
    private readonly ILogger? _logger;

    public GrpcTeacherInfoService(ITeacherInfoProvider teacherInfoProvider, ILoggerFactory? loggerFactory)
    {
        _teacherInfoProvider = teacherInfoProvider;
        _logger = loggerFactory?.GetLogger(GetType().Name);
    }

    public async override Task<TeacherInfoReply> GetTeacherInfo(TeacherInfoRequest request, ServerCallContext context)
    {
        _logger?.Warn("Accepted");
        var findedTeachers = await _teacherInfoProvider.GetTeacherInfoAsync(request.LastName);
        var reply = new TeacherInfoReply();
        reply.Teachers.AddRange(findedTeachers);
        _logger.Info("returning");
        return reply;
    }
}
