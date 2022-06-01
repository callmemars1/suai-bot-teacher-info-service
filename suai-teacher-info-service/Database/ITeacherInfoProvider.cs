using Suai.Bot.TeacherInfo.Proto;

namespace Suai.TeacherInfo.Service.Database;

public interface ITeacherInfoProvider
{
    public Task<IEnumerable<TeacherInfoDto>> GetTeacherInfoAsync(string lastName);

    public IEnumerable<TeacherInfoDto> GetTeacherInfo(string lastName);
}
