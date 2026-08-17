namespace DamiFYP.Domain.Models;

public class DonorFeedback
{
    public long    Id                { get; set; }
    public long    DonationRequestId { get; set; }
    public int     Rating            { get; set; }  // 1–5
    public string? Comment           { get; set; }
    public DateTime CreatedAt        { get; set; }

    public DonationRequest DonationRequest { get; set; } = null!;
}
