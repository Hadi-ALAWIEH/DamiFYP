namespace DamiFYP.Application.Features.DonationRequests;

public class DonationRequestViewModel
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string? BloodTypeName { get; set; }
    public int? Quantity { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? UrgencyLevel { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? NeededByDate { get; set; }
}

public class DonationRequestMatchCandidatesViewModel
{
    public DonationRequestViewModel DonationRequest { get; set; } = null!;
    public List<DonationPostMatchCandidateViewModel> Candidates { get; set; } = new();
}

public class DonationPostMatchCandidateViewModel
{
    public long DonationPostId { get; set; }
    public long DonorUserId { get; set; }
    public string? BloodTypeName { get; set; }
    public int? Quantity { get; set; }
}

