using System.Security.Claims;
using System.Text;
using System.Text.Json;
using DamiFYP;
using DamiFYP.Application.Features.BloodType;
using DamiFYP.Application.Authorization;
using DamiFYP.Domain.Models;
using DamiFYP.Application.Filters;
using DamiFYP.Application.Helpers;
using DamiFYP.Application.Mappers;
using DamiFYP.Application.Features.DonationRequests;
using DamiFYP.ExceptionHandlers.cs;
using DamiFYP.Infrastructure.BloodAvailability;
using DamiFYP.Infrastructure.Email;
using DamiFYP.Persistence.Contexts;
using DamiFYP.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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

// todo: create an extension method for this
// builder.Services.AddNpgsql<DamiContext>(connectionString);

builder.Services.UseNpgSqlWithSeeding(connectionString);


builder.Services.AddOpenApi("v1", options => { options.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); });
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<KeycloakOptions>(builder.Configuration.GetSection("Keycloak"));
builder.Services.Configure<BloodAvailabilityServiceOptions>(
    builder.Configuration.GetSection("BloodAvailabilityService"));
builder.Services.AddScoped<IEmailService>(_ =>
{
    var emailOptions = new SmtpEmailOptions();
    builder.Configuration.GetSection("Email").Bind(emailOptions);
    return new SmtpEmailService(emailOptions);
});
builder.Services.AddHttpClient<IBloodAvailabilityServiceClient, BloodAvailabilityServiceClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<BloodAvailabilityServiceOptions>>().Value;
    if (string.IsNullOrWhiteSpace(options.BaseUrl))
    {
        throw new InvalidOperationException(
            "BloodAvailabilityService:BaseUrl is not configured.");
    }

    client.BaseAddress = new Uri(options.BaseUrl);
});
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddLogging();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthorizationPolicies.CanAccessConversations, policy =>
        policy.Requirements.Add(new BusinessRoleRequirement(BusinessRole.Donor, BusinessRole.Seeker,
            BusinessRole.ManageAccount)));

    options.AddPolicy(AuthorizationPolicies.CanManageDonationRequests, policy =>
        policy.Requirements.Add(new BusinessRoleRequirement(BusinessRole.Seeker, BusinessRole.ManageAccount)));
    // policy.Requirements.Add(new BusinessRoleRequirement(BusinessRole.Donor, BusinessRole.Seeker, BusinessRole.ManageAccount)));

    options.AddPolicy(AuthorizationPolicies.CanManageBloodTypes, policy =>
        policy.Requirements.Add(new BusinessRoleRequirement(BusinessRole.Donor, BusinessRole.Seeker,
            BusinessRole.ManageAccount)));

    options.AddPolicy(AuthorizationPolicies.CanViewAvailableDonationRequests, policy =>
        policy.Requirements.Add(new BusinessRoleRequirement(BusinessRole.Donor)));

    options.AddPolicy(AuthorizationPolicies.CanManageDonationPosts, policy =>
        policy.Requirements.Add(new BusinessRoleRequirement(BusinessRole.Donor, BusinessRole.DonorAndSeeker)));

    options.AddPolicy(AuthorizationPolicies.CanViewBloodAvailabilityPredictions, policy =>
        policy.Requirements.Add(new BusinessRoleRequirement(BusinessRole.Seeker, BusinessRole.ManageAccount)));
});

builder.Services.AddAuthentication().AddJwtBearer();

// todo: keep for if you would like to use manual token issuing
// builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
//     .Configure<IOptions<JwtOptions>>((options, jwtOptions) =>
//     {
//         var jwt = jwtOptions.Value;
//         options.TokenValidationParameters = new TokenValidationParameters()
//         {
//             ValidIssuer = jwt.Issuer,
//             ValidAudience = jwt.Audience,
//             IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
//             ValidateIssuer = true,
//             ValidateAudience = true,
//             ValidateLifetime = true,
//             ValidateIssuerSigningKey = true,
//             RoleClaimType = ClaimTypes.Role
//         };
//     });

// builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
//     .Configure<IOptions<JwtOptions>>((options, jwtOptions) =>
//     {
//         var jwt = jwtOptions.Value;
//         options.TokenValidationParameters = new TokenValidationParameters()
//         {
//             ValidIssuer = jwt.Issuer,
//             ValidAudience = jwt.Audience,
//             IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
//             ValidateIssuer = true,
//             ValidateAudience = true,
//             ValidateLifetime = true,
//             ValidateIssuerSigningKey = true,
//             RoleClaimType = ClaimTypes.Role
//         };
//     });

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<KeycloakOptions>>((options, keycloakOptions) =>
    {
        var kc = keycloakOptions.Value;

        if (!string.IsNullOrWhiteSpace(kc.Audience))
        {
            options.Audience = kc.Audience;
        }

        if (!string.IsNullOrWhiteSpace(kc.Authority))
        {
            options.Authority = kc.Authority;
        }

        // if (!string.IsNullOrWhiteSpace(kc.ClientId))
        // {
        //     options.Audience = kc.ClientId;
        // }

        if (!string.IsNullOrWhiteSpace(kc.MetadataAddress))
        {
            options.MetadataAddress = kc.MetadataAddress;
        }

        options.RequireHttpsMetadata = kc.RequireHttpsMetadata;

        options.TokenValidationParameters ??= new TokenValidationParameters();
        options.TokenValidationParameters.ValidateIssuer = true;
        options.TokenValidationParameters.ValidateAudience = true;
        options.TokenValidationParameters.RoleClaimType = ClaimTypes.Role;

        options.Events = new JwtBearerEvents
        {
            // Browsers can't set an Authorization header on a WebSocket upgrade
            // request, so the SignalR JS client instead sends the token as an
            // "access_token" query string parameter (via accessTokenFactory).
            // Bridge it into the normal bearer flow, but only for the chat hub's
            // path, so query-string tokens aren't accepted on regular API routes.
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/chat"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                if (context.Principal?.Identity is not ClaimsIdentity identity)
                {
                    return Task.CompletedTask;
                }

                var realmAccess = context.Principal.FindFirst("realm_access")?.Value;
                if (!string.IsNullOrWhiteSpace(realmAccess))
                {
                    using var realmDoc = JsonDocument.Parse(realmAccess);
                    if (realmDoc.RootElement.TryGetProperty("roles", out var roles))
                    {
                        foreach (var role in roles.EnumerateArray())
                        {
                            var roleName = role.GetString();
                            if (!string.IsNullOrWhiteSpace(roleName))
                            {
                                identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                            }
                        }
                    }
                }

                var resourceAccess = context.Principal.FindFirst("resource_access")?.Value;
                if (!string.IsNullOrWhiteSpace(resourceAccess) && !string.IsNullOrWhiteSpace(kc.Audience))
                {
                    using var resourceDoc = JsonDocument.Parse(resourceAccess);
                    if (resourceDoc.RootElement.TryGetProperty(kc.Audience, out var client) &&
                        client.TryGetProperty("roles", out var roles))
                    {
                        foreach (var role in roles.EnumerateArray())
                        {
                            var roleName = role.GetString();
                            if (!string.IsNullOrWhiteSpace(roleName))
                            {
                                identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                            }
                        }
                    }
                }

                // Parse roles from damifyp-client resource_access claim
                if (!string.IsNullOrWhiteSpace(resourceAccess) && !string.IsNullOrWhiteSpace(kc.ClientId))
                {
                    using var resourceDoc = JsonDocument.Parse(resourceAccess);
                    if (resourceDoc.RootElement.TryGetProperty(kc.ClientId, out var client) &&
                        client.TryGetProperty("roles", out var roles))
                    {
                        foreach (var role in roles.EnumerateArray())
                        {
                            var roleName = role.GetString();
                            if (!string.IsNullOrWhiteSpace(roleName))
                            {
                                identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                            }
                        }
                    }
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddScoped<ITokenService, TokenGeneratorService>();
builder.Services.AddScoped<IDamiAuthService, ManualAuthService>();
builder.Services.AddScoped<ICurrentUserProfileService, CurrentUserProfileService>();
builder.Services.AddScoped<CurrentUserProfileMiddleware>();
builder.Services.AddScoped<IAuthorizationHandler, BusinessRoleHandler>();
// register match service used by Hangfire jobs
builder.Services.AddScoped<IMatchService, MatchService>();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DamiGlobalExceptionHandler>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(
            "ReactClient",
            builder =>
                builder
                    .WithOrigins("http://localhost:5173")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    // .AllowCredentials()
        );
});
builder.Services.AddSignalR();

var app = builder.Build();

var oauth2Section = builder.Configuration.GetSection("OAuth2");
var oauth2Scopes = oauth2Section.GetSection("Scopes").Get<Dictionary<string, string>>()
                   ?? new Dictionary<string, string>
                   {
                       ["openid"] = "OpenID",
                       ["profile"] = "Profile"
                   };

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
        {
            options
                .WithTitle("DamiFYP API")
                .WithTheme(ScalarTheme.Mars)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                .AddPreferredSecuritySchemes("OAuth2")
                .AddOAuth2Flows("OAuth2", flows =>
                {
                    flows.AuthorizationCode = new AuthorizationCodeFlow()
                    {
                        AuthorizationUrl = oauth2Section.GetValue<string>("AuthorizationUrl"),
                        TokenUrl = oauth2Section.GetValue<string>("TokenUrl"),
                        RedirectUri = oauth2Section.GetValue<string>("RedirectUri"),
                        SelectedScopes = oauth2Scopes.Keys.ToList(),
                        ClientId = oauth2Section.GetValue<string>("ClientId"),
                        ClientSecret = oauth2Section.GetValue<string>("ClientSecret"),
                        Pkce = Pkce.Sha256,
                        CredentialsLocation = CredentialsLocation.Body
                    };
                })
                // .AddPreferredSecuritySchemes("Bearer")
                .AddHttpAuthentication("Bearer", auth => { auth.Token = ""; })
                .EnablePersistentAuthentication();
        }
    );
    app.UseExceptionHandler("/errors");
}

app.UseHttpsRedirection();

app.UseCors("ReactClient");
app.UseAuthentication();
app.UseMiddleware<CurrentUserProfileMiddleware>();
app.UseAuthorization();

// Mapped after UseAuthentication/UseAuthorization so DamiHub's [Authorize] is
// actually enforced on the connection handshake - it was previously mapped
// before those middlewares were registered, which left it unprotected.
app.MapHub<DamiHub>("/hubs/chat");
app.MapControllers();
app.Run();