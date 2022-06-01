using NLog;
using Npgsql;
using Suai.Bot.TeacherInfo.Proto;

namespace Suai.TeacherInfo.Service.Database;
public class PgTeacherInfoProvider : ITeacherInfoProvider
{
    const string QUERY_STRING = "SELECT * FROM persons p\r\nWHERE word_similarity(p.last_name, $1) > 0.5\r\nORDER BY word_similarity(p.last_name, $1) DESC\r\nLIMIT 5;";

    private readonly ILogger? _logger;

    public PgTeacherInfoProvider(IDatabaseConnectionParams connectionParams, ILoggerFactory? loggerFactory = null)
    {
        _logger = loggerFactory?.GetLogger(GetType().Name);
        _conn = new NpgsqlConnection(connectionParams.ConnectionString);
        try
        {
            _conn.Open();
            _logger?.Info("Succesfuly connected to a database");
        }
        catch (Exception ex)
        {
            _logger?.Error("Database connection parameters are wrong: " + ex.GetType().Name);
            throw;
        }
    }
    public async Task<IEnumerable<TeacherInfoDto>> GetTeacherInfoAsync(string lastName)
    {
        var findedTeachers = new List<TeacherInfoDto>();
        await _semaphore.WaitAsync();
        var command = new NpgsqlCommand(QUERY_STRING, _conn)
        {
            Parameters =
            {
                new() { Value = lastName}
            }
        };
        using (var reader = await command.ExecuteReaderAsync())
        {
            while (reader.Read())
            {
                var firstNameFinded = reader["first_name"].ToString();
                var secondNameFinded = reader["second_name"].ToString();
                var lastNameFinded = reader["last_name"].ToString();
                _logger?.Info("Finded person {} {} {}", firstNameFinded, secondNameFinded, lastNameFinded);
                findedTeachers.Add(new()
                {
                    FirstName = firstNameFinded,
                    SecondName = secondNameFinded,
                    LastName = lastNameFinded,
                });
            }
        }
        _semaphore.Release();
        return findedTeachers;
    }

    public IEnumerable<TeacherInfoDto> GetTeacherInfo(string lastName)
    {
        var findedTeachers = new List<TeacherInfoDto>();
        _semaphore.Wait();
        var command = new NpgsqlCommand(QUERY_STRING, _conn)
        {
            Parameters =
            {
                new() { Value = lastName}
            }
        };
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                var firstNameFinded = reader["first_name"].ToString();
                var secondNameFinded = reader["second_name"].ToString();
                var lastNameFinded = reader["last_name"].ToString();
                _logger?.Info("Finded person {} {} {}", firstNameFinded, secondNameFinded, lastNameFinded);
                findedTeachers.Add(new()
                {
                    FirstName = firstNameFinded,
                    SecondName = secondNameFinded,
                    LastName = lastNameFinded,
                });
            }
        }
        _semaphore.Release();
        return findedTeachers;
    }

    ~PgTeacherInfoProvider()
    {
        _conn.Close();
    }

    private readonly NpgsqlConnection _conn;
    private readonly SemaphoreSlim _semaphore = new(1);

}