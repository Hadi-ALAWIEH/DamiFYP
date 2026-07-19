using DamiFYP.Domain.Models;
using DamiFYP.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public static class SeedingExtension
{
    public static IServiceCollection UseNpgSqlWithSeeding(this IServiceCollection services, string connectionString)
    {
        return services.AddNpgsql<DamiContext>(
            connectionString,
            optionsAction: options => options.UseAsyncSeeding(async (context, _, cancellationToken) =>
                {
                    var userProbe = await context.Set<DamiUser>().FirstOrDefaultAsync(user => user.Email == "doe@example.com", cancellationToken);
                    if (userProbe == null)
                    {
                        // add users
                        context.Set<DamiUser>().Add(
                            new DamiUser()
                            {
                                Name ="hadiz",
                                Role = BusinessRole.Seeker,
                                PasswordHash = "EXTERNAL_AUTH",
                                BloodType = new BloodType() { BloodTypeName = BloodTypeName.AbPositive },
                                Email = "hadiz@gmail.com",
                                IsAvailable = true,
                                LastActiveAt = DateTime.UtcNow,
                                CreatedAt = DateTime.UtcNow,
                                Latitude = 22,
                                Longitude = 23,
                                KeyCloakId = "23e8fa64-6830-4465-9667-3cbbef454aef"
                            });
                    }
                    await context.SaveChangesAsync(cancellationToken);
                }
            )
        );
    }
}