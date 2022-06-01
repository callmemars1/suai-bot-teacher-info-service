using Grpc.Core;
using NLog;
using Suai.Bot.TeacherInfo.Proto;
using Suai.TeacherInfo.Service;
using Suai.TeacherInfo.Service.Database;

LogManager.LoadConfiguration(@"Logging/nlog.config");
var logger = LogManager.GetCurrentClassLogger();

logger.Info("started");

var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? throw new ArgumentException("NO DB_HOST ENV");
var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? throw new ArgumentException("NO DB_NAME ENV");
var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? throw new ArgumentException("NO DB_USER ENV");
var dbPass = Environment.GetEnvironmentVariable("DB_PASS") ?? throw new ArgumentException("NO DB_PASS ENV");
var grpcPort = Convert.ToInt32(Environment.GetEnvironmentVariable("GRPC_PORT") ?? throw new ArgumentException("NO GRPC_PORT ENV"));

var dbReader = new PgTeacherInfoProvider(new PostgresDatabaseConnectionParams(
    Host: dbHost,
    DatabaseName: dbName,
    User: dbUser,
    Password: dbPass
    ), new NLogFactory());

Server server = new()
{
    Services = { TeacherInfoProvider.BindService(new GrpcTeacherInfoService(dbReader, new NLogFactory())) },
    Ports = { new ServerPort("0.0.0.0", grpcPort, ServerCredentials.Insecure) }
};

server.Start();

logger.Info("Server listening on {}", grpcPort);

while (true) { }