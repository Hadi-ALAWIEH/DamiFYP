namespace DamiFYP.Domain.Models;

public enum BadgeTier
{
    Newcomer    = 0,
    Helper      = 1,
    Contributor = 2,
    Guardian    = 3,
    Hero        = 4,
}

public enum DonationPostStatus
{
    Active    = 0,
    Completed = 1,
}

public class DamiBadge
{
    public long      Id             { get; set; }
    public long      DamiUserId     { get; set; }
    public BadgeTier Tier           { get; set; } = BadgeTier.Newcomer;
    public int       DonationPoints { get; set; } = 0;
    public DateTime? LastDonationAt { get; set; }

    public DamiUser DamiUser { get; set; } = null!;
}
