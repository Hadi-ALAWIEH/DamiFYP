using DamiFYP.Application.Filters;
using DamiFYP.Persistence.Contexts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options =>
    options.Filters
        .Add<ExampleFilter>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration.AddJsonFile("Config/Development/appsettings.Development.json");
var connectionString = builder.Configuration.GetValue<string>("Local:environmentVariables:CONNECTIONSTRINGS__DB");

builder.Services.AddNpgsql<DamiContext>(connectionString);

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