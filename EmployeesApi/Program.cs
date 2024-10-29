
using AutoMapper.EquivalencyExpression;
using Employees.DataAccess;
using Employees.DataAccess.Interface;
using Employees.Service;
using Employees.Service.Interface;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("TaurexDB");

var connection = new NpgsqlConnection(connectionString);
using var cmd = connection.CreateCommand();
cmd.CommandText = @"CREATE TABLE IF NOT EXISTS EMPLOYEES(
                EmployeeId INT generated always as identity PRIMARY KEY,
                FullName VARCHAR(100) NOT NULL,
                Title VARCHAR(100) NOT NULL,
                ManagerEmployeeId INT NULL REFERENCES EMPLOYEES(EmployeeId)
            )";
await connection.OpenAsync();
using var reader = await cmd.ExecuteReaderAsync();
await connection.CloseAsync();

builder.Services.AddScoped((provider) => new NpgsqlConnection(connectionString));
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>(); 
builder.Services.AddScoped<IEmployeeService, EmployeeService>();

builder.Services.AddAutoMapper(cfg => { cfg.AddCollectionMappers(); }, AppDomain.CurrentDomain.GetAssemblies());



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
