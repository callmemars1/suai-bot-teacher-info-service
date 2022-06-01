namespace Suai.TeacherInfo.Service.Database;

public record PostgresDatabaseConnectionParams
    (
        string Host,
        string DatabaseName,
        string User,
        string Password
    ) : IDatabaseConnectionParams
{
    public string ConnectionString => "Database=" + DatabaseName + ";Host=" + Host + ";Username=" + User + ";Password=" + Password;
}