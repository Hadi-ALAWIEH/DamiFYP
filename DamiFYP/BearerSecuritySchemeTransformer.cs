using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi;

// This class is only to add Bearer Token field for Scalar
namespace DamiFYP;

internal sealed class BearerSecuritySchemeTransformer(
    IAuthenticationSchemeProvider authenticationSchemeProvider,
    IConfiguration configuration
) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var schemes = await authenticationSchemeProvider.GetAllSchemesAsync();

        if (schemes.All(s => s.Name != "Bearer"))
            return;

        document.Components ??= new OpenApiComponents();

        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        var bearerSchemeId = "Bearer";
        var oauthSchemeId = "OAuth2";

        document.Components.SecuritySchemes[bearerSchemeId] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT Authorization"
        };

        var oauthSection = configuration.GetSection("OAuth2");
        var oauthScopes = oauthSection.GetSection("Scopes").Get<Dictionary<string, string>>()
                          ?? new Dictionary<string, string>
                          {
                              ["openid"] = "OpenID",
                              ["profile"] = "Profile"
                          };

        document.Components.SecuritySchemes[oauthSchemeId] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri(oauthSection.GetValue<string>("AuthorizationUrl")),
                    TokenUrl = new Uri(oauthSection.GetValue<string>("TokenUrl")),
                    Scopes = oauthScopes
                }
            }
        };

        document.Security ??= new List<OpenApiSecurityRequirement>();

        document.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(bearerSchemeId)] = new List<string>(),
            [new OpenApiSecuritySchemeReference(oauthSchemeId)] = new List<string>()
        });
    }
}