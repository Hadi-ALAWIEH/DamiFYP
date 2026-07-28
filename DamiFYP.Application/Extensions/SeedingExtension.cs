using System.Globalization;
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
            optionsAction: options => options
                // UseSeeding is the synchronous counterpart of UseAsyncSeeding below.
                // Tools like `dotnet ef database update` apply migrations
                // synchronously, so without this EF Core throws "no synchronous
                // seed delegate has been provided" and skips seeding entirely.
                .UseSeeding((context, _) =>
                {
                    if (context.Set<DamiUser>().Any())
                        return;

                    var (bob, bib, requests, post) = BuildSeedData();

                    context.AddRange(bob, bib);
                    context.SaveChanges();

                    context.Set<DonationRequest>().AddRange(requests(bob));
                    context.Set<DonationPost>().Add(post(bib));
                    context.SaveChanges();
                })
                .UseAsyncSeeding(async (context, _, cancellationToken) =>
                {
                    if (await context.Set<DamiUser>().AnyAsync(cancellationToken))
                        return;

                    var (bob, bib, requests, post) = BuildSeedData();

                    await context.AddRangeAsync(new object[] { bob, bib }, cancellationToken);
                    await context.SaveChangesAsync(cancellationToken);

                    await context.Set<DonationRequest>().AddRangeAsync(requests(bob), cancellationToken);
                    await context.Set<DonationPost>().AddAsync(post(bib), cancellationToken);
                    await context.SaveChangesAsync(cancellationToken);
                }));
    }

    // Shared so the sync (UseSeeding) and async (UseAsyncSeeding) paths above
    // always insert identical data instead of two copies drifting apart.
    private static (DamiUser bob, DamiUser bib, Func<DamiUser, DonationRequest[]> requests, Func<DamiUser, DonationPost> post)
        BuildSeedData()
    {
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

        // -------------------------
        // Donation Requests
        // -------------------------

        // NOTE: DateTime.Parse("...Z") converts the value to LOCAL time and
        // tags it Kind=Local by default - Npgsql then refuses to write it into
        // a "timestamp with time zone" column ("only UTC is supported"), which
        // is exactly what crashed the previous seeding attempt. RoundtripKind
        // keeps the literal clock value and correctly tags it Kind=Utc instead.
        DonationRequest[] Requests(DamiUser seeker) =>
        [
            new DonationRequest
            {
                DamiUser = seeker,
                Quantity = 2,
                Latitude = 33.8547,
                Longitude = 35.8623,
                Address = "Zahle Hospital, Zahle",
                Urgency = DonationRequestUrgency.Medium,
                NeededByDate = DateTime.Parse("2026-07-24T09:00:00Z", CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind),
                CreatedAt = DateTime.UtcNow
            },

            new DonationRequest
            {
                DamiUser = seeker,
                Quantity = 5,
                Latitude = 33.8886,
                Longitude = 35.4955,
                Address = "Rafik Hariri University Hospital, Beirut",
                Urgency = DonationRequestUrgency.High,
                NeededByDate = DateTime.Parse("2026-07-19T18:00:00Z", CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind),
                CreatedAt = DateTime.UtcNow
            }
        ];

        // -------------------------
        // Donation Posts
        // -------------------------

        DonationPost Post(DamiUser donor) => new()
        {
            DamiUser = donor,
            Quantity = 2
        };

        return (bob, bib, Requests, Post);
    }
}
