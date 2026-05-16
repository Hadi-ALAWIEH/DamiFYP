using System.Text;
using DamiFYP.Application.Features.BloodType;
using DamiFYP.Application.Filters;
using DamiFYP.Application.Mappers;
using DamiFYP.Persistence.Contexts;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers(options => options.Filters.Add<ExampleFilter>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// builder.Services.AddSwaggerGen(c =>
//     {
//         c.AddSecurityDefinition("token", new OpenApiSecurityScheme
//         {
//             Type = SecuritySchemeType.Http,
//             In = ParameterLocation.Query,
//             Name = HeaderNames.Authorization,
//             Scheme = "Bearer"
//         });
//     }
// );

builder.Configuration.AddJsonFile("Config/Development/appsettings.Development.json");
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<GetAllBloodTypesQuery>());
builder.Services.AddAutoMapper(assemblies: typeof(DamiMapper).Assembly);
var connectionString = builder.Configuration.GetValue<string>("Local:environmentVariables:CONNECTIONSTRINGS__DB");
builder.Services.AddNpgsql<DamiContext>(connectionString);
// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     .AddJwtBearer(options =>
//     {
//         // Minimal JWT setup: validate signature + (optional) issuer/audience.
//         // Configure in appsettings under:
//         // Jwt:Key, Jwt:Issuer, Jwt:Audience
//         var jwtKey = builder.Configuration["Jwt:Key"];
//         if (string.IsNullOrWhiteSpace(jwtKey))
//             throw new InvalidOperationException("Missing configuration value: Jwt:Key");
//
//         options.TokenValidationParameters = new TokenValidationParameters
//         {
//             ValidateIssuerSigningKey = true,
//             IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
//
//             ValidateIssuer = false,
//             ValidateAudience = false,
//
//             // If you want to enforce these, set ValidateIssuer/ValidateAudience to true
//             // and provide Jwt:Issuer and Jwt:Audience.
//             ValidIssuer = builder.Configuration["Jwt:Issuer"],
//             ValidAudience = builder.Configuration["Jwt:Audience"],
//
//             ValidateLifetime = true,
//             ClockSkew = TimeSpan.FromMinutes(1)
//         };
//     });
builder.Services.AddAuthentication().AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidIssuer = builder.Configuration.GetValue<string>("JwtSettings:Issuer"),
            ValidAudience = builder.Configuration.GetValue<string>("JwtSettings:Audience"),
            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("JwtSettings:Key")!)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
        };
    }
);

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // app.UseSwagger();
    // app.UseSwaggerUI();
    app.MapOpenApi();
    app.MapScalarApiReference();
}
app.UseHttpsRedirection();

// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();
app.Run();