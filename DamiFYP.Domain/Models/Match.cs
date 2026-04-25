namespace DamiFYP.Domain.Models;

public class Match
{
    public long Id { get; set; }
    public long DonationRequestId { get; set; }
    public long DonationPostId { get; set; }

    public DonationRequest DonationRequest { get; set; } = null!;
    public DonationPost DonationPost { get; set; } = null!;
    public Conversation Conversation { get; set; } = null!;

    public string? Status { get; set; }
    public double? DistanceKm { get; set; }
    public DateTime CreatedAt { get; set; }
}