using DamiFYP.Domain.Models;
using DamiFYP.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DamiFYP.Domain.Models;
using DamiFYP.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public static class SeedingExtension
{
    public static IServiceCollection UseNpgSqlWithSeeding(
        this IServiceCollection services,
        string connectionString)
    {
        return services.AddNpgsql<DamiContext>(
            connectionString,
            optionsAction: options => options.UseAsyncSeeding(async (context, _, cancellationToken) =>
            {
                if (await context.Set<DamiUser>().AnyAsync(cancellationToken))
                    return;

                // -------------------------
                // Users
                // -------------------------

                var bob = new DamiUser
                {
                    Name = "Bob",
                    Email = "bob@hotmail.com",
                    PasswordHash = "EXTERNAL_AUTH",
                    KeyCloakId = "<BOB_KEYCLOAK_ID>",
                    Role = BusinessRole.Seeker,
                    IsAvailable = true,
                    Latitude = 33.8938,
                    Longitude = 35.5018,
                    CreatedAt = DateTime.UtcNow,
                    LastActiveAt = DateTime.UtcNow,

                    BloodType = new BloodType
                    {
                        BloodTypeName = BloodTypeName.APositive
                    }
                };

                var bib = new DamiUser
                {
                    Name = "Bib",
                    Email = "bib@hotmail.com",
                    PasswordHash = "EXTERNAL_AUTH",
                    KeyCloakId = "<BIB_KEYCLOAK_ID>",
                    Role = BusinessRole.Donor,
                    IsAvailable = true,
                    Latitude = 33.828,
                    Longitude = 35.5318,
                    CreatedAt = DateTime.UtcNow,
                    LastActiveAt = DateTime.UtcNow,

                    BloodType = new BloodType
                    {
                        BloodTypeName = BloodTypeName.APositive
                    }
                };

                await context.AddRangeAsync(bob, bib, cancellationToken);
                var rows = await context.SaveChangesAsync(cancellationToken);

                // -------------------------
                // Donation Requests
                // -------------------------

                await context.Set<DonationRequest>().AddRangeAsync(
                    new DonationRequest
                    {
                        DamiUser = bob,
                        Quantity = 2,
                        Latitude = 33.8547,
                        Longitude = 35.8623,
                        Address = "Zahle Hospital, Zahle",
                        Urgency = DonationRequestUrgency.Medium,
                        NeededByDate = DateTime.Parse("2026-07-24T09:00:00Z"),
                        CreatedAt = DateTime.UtcNow
                    },

                    new DonationRequest
                    {
                        DamiUser = bob,
                        Quantity = 5,
                        Latitude = 33.8886,
                        Longitude = 35.4955,
                        Address = "Rafik Hariri University Hospital, Beirut",
                        Urgency = DonationRequestUrgency.High,
                        NeededByDate = DateTime.Parse("2026-07-19T18:00:00Z"),
                        CreatedAt = DateTime.UtcNow
                    });

                // -------------------------
                // Donation Posts
                // -------------------------

                await context.Set<DonationPost>().AddAsync(
                    new DonationPost
                    {
                        DamiUser = bib,
                        Quantity = 2
                    },
                    cancellationToken);

                await context.SaveChangesAsync(cancellationToken);
            }));
    }
}
