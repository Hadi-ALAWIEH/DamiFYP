namespace DamiFYP.Application.Helpers;

public sealed class KeycloakOptions
{
    public string Authority { get; init; }
    public string Audience { get; set; }
    public string Realm { get; init; }
    public string ClientId { get; init; }
    public string ClientSecret { get; init; }
    public string MetadataAddress { get; init; }
    public string[] Scopes { get; init; }
    public bool RequireHttpsMetadata { get; init; }
}

