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

    public override Task<TeacherInfoReply> GetTeacherInfo(TeacherInfoRequest request, ServerCallContext context)
    {
        var findedTeachers = _teacherInfoProvider.GetTeacherInfoAsync(request.LastName).Result;
        var reply = new TeacherInfoReply();
        reply.Teachers.AddRange(findedTeachers);
        return Task.FromResult(reply);
    }
}
