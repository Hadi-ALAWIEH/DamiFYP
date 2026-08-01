using DamiFYP.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DamiFYP.Persistence.Contexts;

public class DamiContext : DbContext
{
    public DamiContext(DbContextOptions<DamiContext> options) : base(options)
    {
    }

    public DbSet<DamiUser> DamiUsers => Set<DamiUser>();
    public DbSet<DonationRequest> DonationRequests => Set<DonationRequest>();
    public DbSet<DonationPost> DonationPosts => Set<DonationPost>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ConversationParticipant> ConversationParticipants => Set<ConversationParticipant>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<BloodType> BloodTypes => Set<BloodType>();
    public DbSet<BotMessage> BotMessages => Set<BotMessage>();

    // public DbSet<Organization> Organizations => Set<Organization>();
    // public DbSet<BloodInventory> BloodInventories => Set<BloodInventory>();
    // public DbSet<Donation> Donations => Set<Donation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DamiUser>(entity =>
        {
            entity.ToTable("DamiUser");
            entity.HasKey(x => x.Id);

            entity.HasOne(user => user.BloodType)
                .WithOne(bloodType => bloodType.DamiUser)
                .HasForeignKey<BloodType>(bloodType => bloodType.DamiUserId);

            entity.Property(x => x.Name).IsRequired();
            entity.Property(x => x.Email).IsRequired();
            entity.Property(x => x.PasswordHash).IsRequired();
            entity.Property(x => x.Role).IsRequired();
            entity.Property(x => x.IsAvailable).HasDefaultValue(false);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<DonationRequest>(entity =>
        {
            entity.ToTable("DonationRequest");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.BloodTypeName).IsRequired();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(x => x.DamiUser)
                .WithMany(x => x.DonationRequests)
                .HasForeignKey(x => x.DamiUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Match>(entity =>
        {
            entity.ToTable("Match");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(x => x.DonationRequest)
                .WithMany(x => x.Matches)
                .HasForeignKey(x => x.DonationRequestId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(x => x.DonationPost)
                .WithMany(x => x.Matches)
                .HasForeignKey(x => x.DonationPostId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(match => match.Conversation)
                .WithOne(conversation => conversation.Match)
                .HasForeignKey<Conversation>(conversation => conversation.MatchId);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable("Message");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Content).IsRequired();
            entity.Property(x => x.SentAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.IsRead).HasDefaultValue(false);

            entity.HasOne(x => x.ConversationParticipant)
                .WithMany(x => x.Messages)
                .HasForeignKey(x => x.ConversationParticipantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BotMessage>(entity =>
        {
            entity.ToTable("BotMessage");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Content).IsRequired();
            entity.Property(x => x.Role).IsRequired();
            entity.Property(x => x.SentAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(x => x.DamiUser)
                .WithMany(x => x.BotMessages)
                .HasForeignKey(x => x.DamiUserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.DamiUserId, x.SentAt });
        });

        // modelBuilder.Entity<BloodType>(entity =>
        // {
        //     entity.ToTable("BloodType");
        //     entity.HasKey(bloodType => bloodType.Id);
        //     entity.HasOne<User>()
        //         .WithOne(user => user.BloodType);
        // });

        // modelBuilder.Entity<Organization>(entity =>
        // {
        //     entity.ToTable("Organizations");
        //     entity.HasKey(x => x.Id);
        //
        //     entity.Property(x => x.Name).IsRequired();
        // });

        // modelBuilder.Entity<BloodInventory>(entity =>
        // {
        //     entity.ToTable("BloodInventory");
        //     entity.HasKey(x => x.Id);
        //
        //     entity.Property(x => x.BloodType).IsRequired();
        //     entity.Property(x => x.UnitsAvailable).IsRequired();
        //     entity.Property(x => x.LastUpdated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        //
        //     entity.HasOne(x => x.Organization)
        //         .WithMany(x => x.BloodInventories)
        //         .HasForeignKey(x => x.OrganizationId)
        //         .OnDelete(DeleteBehavior.NoAction);
        // });

        // modelBuilder.Entity<Donation>(entity =>
        // {
        //     entity.ToTable("Donations");
        //     entity.HasKey(x => x.Id);
        //
        //     entity.HasOne(x => x.Donor)
        //         .WithMany(x => x.Donations)
        //         .HasForeignKey(x => x.DonorId)
        //         .OnDelete(DeleteBehavior.NoAction);
        //
        //     entity.HasOne(x => x.Request)
        //         .WithMany(x => x.Donations)
        //         .HasForeignKey(x => x.RequestId)
        //         .OnDelete(DeleteBehavior.NoAction);
        // });
    }
}