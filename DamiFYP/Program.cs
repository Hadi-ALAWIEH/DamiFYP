using System.Security.Claims;
using System.Text;
using DamiFYP;
using DamiFYP.Application.Features.BloodType;
using DamiFYP.Application.Filters;
using DamiFYP.Application.Helpers;
using DamiFYP.Application.Mappers;
using DamiFYP.ExceptionHandlers.cs;
using DamiFYP.Persistence.Contexts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers(options => options.Filters.Add<ExampleFilter>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Configuration.AddJsonFile("Config/Development/appsettings.Development.json");
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<GetAllBloodTypesQuery>());
builder.Services.AddAutoMapper(assemblies: typeof(DamiMapper).Assembly);
var connectionString = builder.Configuration.GetValue<string>("Local:environmentVariables:CONNECTIONSTRINGS__DB");
builder.Services.AddNpgsql<DamiContext>(connectionString);
builder.Services.AddOpenApi("v1", options => { options.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); });
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddAuthorization();
builder.Services.AddAuthentication().AddJwtBearer();

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((options, jwtOptions) =>
    {
        var jwt = jwtOptions.Value;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddScoped<ITokenService, TokenGeneratorService>();
builder.Services.AddScoped<IDamiAuthService, ManualAuthService>();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DamiGlobalExceptionHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
        {
            options.WithTitle("DamiFYP API")
                .WithTheme(ScalarTheme.BluePlanet)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                .AddPreferredSecuritySchemes("Bearer")
                .AddHttpAuthentication("Bearer", auth => { auth.Token = ""; })
                .EnablePersistentAuthentication();
        }
    );
    app.UseExceptionHandler("/errors");
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();