using Coravel;
using MySql.Data.MySqlClient;
using Roham.Services.Helpers;
using Roham.Services.Models;
using Roham.Services.Services;
using SqlKata.Compilers;
using SqlKata.Execution;
using Stimulsoft.Base;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file
EnvReader.Load(".env");

// Add services to the container.
builder.Services.AddScoped(provider =>
{
    var db_host = Environment.GetEnvironmentVariable("DB_HOST");
    var db_port = Environment.GetEnvironmentVariable("DB_PORT");
    var db_user = Environment.GetEnvironmentVariable("DB_USER");
    var db_password = Environment.GetEnvironmentVariable("DB_PASSWORD");
    var db_databasename = Environment.GetEnvironmentVariable("DB_DATABASENAME");

    var connection_string = $"Host={db_host};Port={db_port};User={db_user};Password={db_password};Database={db_databasename};";

    Console.WriteLine(connection_string);

    return new QueryFactory(
    new MySqlConnection(connection_string),
    new MySqlCompiler())
    {
        // Log the compiled query to the console
        Logger = compiled =>
        {
            if (builder.Environment.IsDevelopment()) Console.WriteLine(compiled.ToString());
        }
    };
});

builder.Services.AddScheduler();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<LocksService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

StiLicense.LoadFromString("6vJhGtLLLz2GNviWmUTrhSqnOItdDwjBylQzQcAOiHkgpgFGkUl79uxVs8X+uspx6K+tqdtOB5G1S6PFPRrlVNvMUiSiNYl724EZbrUAWwAYHlGLRbvxMviMExTh2l9xZJ2xc4K1z3ZVudRpQpuDdFq+fe0wKXSKlB6okl0hUd2ikQHfyzsAN8fJltqvGRa5LI8BFkA/f7tffwK6jzW5xYYhHxQpU3hy4fmKo/BSg6yKAoUq3yMZTG6tWeKnWcI6ftCDxEHd30EjMISNn1LCdLN0/4YmedTjM7x+0dMiI2Qif/yI+y8gmdbostOE8S2ZjrpKsgxVv2AAZPdzHEkzYSzx81RHDzZBhKRZc5mwWAmXsWBFRQol9PdSQ8BZYLqvJ4Jzrcrext+t1ZD7HE1RZPLPAqErO9eo+7Zn9Cvu5O73+b9dxhE2sRyAv9Tl1lV2WqMezWRsO55Q3LntawkPq0HvBkd9f8uVuq9zk7VKegetCDLb0wszBAs1mjWzN+ACVHiPVKIk94/QlCkj31dWCg8YTrT5btsKcLibxog7pv1+2e4yocZKWsposmcJbgG0");

app.MapControllers();

//app.Services.UseScheduler(s =>
//{
//    s.Schedule(() => {
//        var locks_service = app
//        .Services
//        .CreateScope()
//        .ServiceProvider
//        .GetService(typeof(LocksService)) as LocksService;

//#pragma warning disable CS8602 // Dereference of a possibly null reference.
//        locks_service.CheckLock();
//#pragma warning restore CS8602 // Dereference of a possibly null reference.

//        Console.WriteLine("Lock Checked.");
//    })
//    .EveryMinute();
//});

var base_port = Environment.GetEnvironmentVariable("BASE_PORT");
var base_host = Environment.GetEnvironmentVariable("BASE_HOST");
base_host = "127.0.0.1";
base_host = $"http://{base_host}:{base_port}/";

await app.RunAsync(base_host);
