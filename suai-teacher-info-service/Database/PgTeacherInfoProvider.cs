using NLog;
using Npgsql;
using Suai.Bot.TeacherInfo.Proto;

namespace Suai.TeacherInfo.Service.Database;

record TeacherRecord
{
    public long ID { get; set; }
    public string FirstName { get; set; } = "";
    public string SecondName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string ClassRoom { get; set; } = "";
    public string TeachingDegree { get; set; } = "";
    public string Department { get; set; } = "";
    public string Institute { get; set; } = "";
    public string AcademicDegree { get; set; } = "";
    public string Position { get; set; } = "";

}


public class PgTeacherInfoProvider : ITeacherInfoProvider
{
    const string QUERY_STRING = ""
         + "SELECT * FROM ( "
         + "SELECT DISTINCT * FROM( "
         + "SELECT "
         + "p.id, "
         + "p.last_name, "
         + "p.first_name, "
         + "p.second_name, "
         + "e.email, "
         + "p2.phone_number, "
         + "cr.number as class_room, "
         + "td.name as teaching_degree, "
         + "d2.name as department, "
         + "i.name as institute, "
         + "ad.name as academic_degree, "
         + "p3.name as position "
         + "FROM persons p "
         + "LEFT JOIN emails e on p.id = e.person_id "
         + "LEFT JOIN phones p2 on p.id = p2.person_id "
         + "LEFT JOIN teachers t on p.id = t.person_id "
         + "LEFT JOIN class_rooms cr on t.class_room_id = cr.id "
         + "LEFT JOIN teaching_degrees td on td.id = t.teaching_degree_id "
         + "LEFT JOIN teachers_departments td2 on t.id = td2.teacher_id "
         + "LEFT JOIN departments d2 on td2.department_id = d2.id "
         + "LEFT JOIN institutes i on i.id = d2.institute_id "
         + "LEFT JOIN teachers_academic_degrees tad on t.id = tad.teacher_id "
         + "LEFT JOIN academic_degrees ad on tad.academic_degree_id = ad.id "
         + "LEFT JOIN teachers_positions tp on t.id = tp.teacher_id "
         + "LEFT JOIN positions p3 on tp.position_id = p3.id "
         + "WHERE word_similarity(p.last_name, $1 ) > 0.5 ) as sub) as sub2 "
         + "ORDER BY word_similarity(sub2.last_name, $1 ) DESC";

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
        _logger?.Info("Try to find person with lastname {}", lastName);
        var findedTeachers = new List<TeacherInfoDto>();
        await _semaphore.WaitAsync();
        _logger?.Info("in semaphore");
        var command = new NpgsqlCommand(QUERY_STRING, _conn)
        {
            Parameters =
            {
                new() { Value = lastName}
            }
        };
        using (var reader = await command.ExecuteReaderAsync())
        {
            var teachersRecords = new List<TeacherRecord>();
            while (reader.Read())
            {
                TeacherRecord record = new TeacherRecord()
                {
                    ID = reader.GetInt64(0), // should be first
                    FirstName = reader["first_name"].ToString() ?? "",
                    SecondName = reader["second_name"].ToString() ?? "",
                    LastName = reader["last_name"].ToString() ?? "",
                    Email = reader["email"].ToString() ?? "",
                    Phone = reader["phone_number"].ToString() ?? "",
                    ClassRoom = reader["class_room"].ToString() ?? "",
                    TeachingDegree = reader["teaching_degree"].ToString() ?? "",
                    Department = reader["department"].ToString() ?? "",
                    Institute = reader["institute"].ToString() ?? "",
                    AcademicDegree = reader["academic_degree"].ToString() ?? "",
                    Position = reader["position"].ToString() ?? ""
                };
                teachersRecords.Add(record);
            }
            _logger?.Info("accepted from db");
            var groupedByTeacher = teachersRecords.GroupBy(t => t.ID);
            foreach (var groupedTeacherRecords in groupedByTeacher)
            {
                var findedTeacher = new TeacherInfoDto();
                int i = 0;
                foreach (var teacher in groupedTeacherRecords)
                {
                    if (i == 0)
                    {
                        findedTeacher.FirstName = teacher.FirstName;
                        findedTeacher.SecondName = teacher.SecondName;
                        findedTeacher.LastName = teacher.LastName;
                        findedTeacher.Phone = teacher.Phone;
                        findedTeacher.Email = teacher.Email;
                        findedTeacher.ClassRoom = teacher.ClassRoom;
                        findedTeacher.TeacherDegree = teacher.TeachingDegree;
                    }
                    findedTeacher.Positions.Add(new PositionDto
                    {
                        Position = teacher.Position,
                        Department = teacher.Department,
                        Institute = teacher.Institute,
                    });
                    findedTeacher.AcademicDegrees.Add(teacher.AcademicDegree);
                    ++i;
                }
                findedTeachers.Add(findedTeacher);
            }
        }
        _logger?.Info("Before release");
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
            var teachersRecords = new List<TeacherRecord>();
            while (reader.Read())
            {
                TeacherRecord record = new TeacherRecord()
                {
                    ID = reader.GetInt64(0), // should be first
                    FirstName = reader["first_name"].ToString() ?? "",
                    SecondName = reader["second_name"].ToString() ?? "",
                    LastName = reader["last_name"].ToString() ?? "",
                    Email = reader["email"].ToString() ?? "",
                    Phone = reader["phone_number"].ToString() ?? "",
                    ClassRoom = reader["class_room"].ToString() ?? "",
                    TeachingDegree = reader["teaching_degree"].ToString() ?? "",
                    Department = reader["department"].ToString() ?? "",
                    Institute = reader["institute"].ToString() ?? "",
                    AcademicDegree = reader["academic_degree"].ToString() ?? "",
                    Position = reader["position"].ToString() ?? ""
                };
                teachersRecords.Add(record);
            }
            var groupedByTeacher = teachersRecords.GroupBy(t => t.ID);
            var findedTeacher = new TeacherInfoDto();
            foreach (var teacherRecords in groupedByTeacher)
            {
                int i = 0;
                foreach (var teacher in teacherRecords)
                {
                    if (i == 0)
                    {
                        findedTeacher.FirstName = teacher.FirstName;
                        findedTeacher.SecondName = teacher.SecondName;
                        findedTeacher.LastName = teacher.LastName;
                        findedTeacher.Phone = teacher.Phone;
                        findedTeacher.Email = teacher.Email;
                        findedTeacher.ClassRoom = teacher.ClassRoom;
                        findedTeacher.TeacherDegree = teacher.TeachingDegree;
                    }
                    findedTeacher.Positions.Add(new PositionDto
                    {
                        Position = teacher.Position,
                        Department = teacher.Department,
                        Institute = teacher.Institute,
                    });
                    findedTeacher.AcademicDegrees.Add(teacher.AcademicDegree);

                    ++i;
                }
                findedTeachers.Add(findedTeacher);
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