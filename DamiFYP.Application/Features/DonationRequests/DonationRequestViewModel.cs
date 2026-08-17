using DamiFYP.Domain.Models;

namespace DamiFYP.Application.Features.DonationRequests;

public class DonationRequestViewModel
{
    public long Id { get; set; }
    public long DamiUserId { get; set; }
    public string? BloodTypeName { get; set; }
    public int? Quantity { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string Address { get; set; }
    public DonationRequestUrgency Urgency { get; set; }
    public DonationRequestStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? NeededByDate { get; set; }
    public int?    FeedbackRating  { get; set; }
    public string? FeedbackComment { get; set; }
    public bool    HasFeedback     { get; set; }
}


public class DonationPostViewModel
{
    public long DonationPostId { get; set; }
    public long DonorUserId { get; set; }
    public string  DonorName { get; set; }
    public string DonorAddress { get; set; } = "";
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? BloodTypeName { get; set; }
    public int? Quantity { get; set; }
    public DonationPostStatus Status { get; set; } = DonationPostStatus.Active;

    // True when this specific donor has already been confirmed as a match for
    // the donation request this candidate list was fetched for.
    public bool IsMatched { get; set; }
    public string? DonorProfilePictureUrl { get; set; }
    public BadgeTier? DonorBadgeTier { get; set; }
    // Specific feedback received for this completed donation (donor's My Posts view)
    public int?    ReceivedFeedbackRating  { get; set; }
    public string? ReceivedFeedbackComment { get; set; }
    // Aggregate across all completed donations by this donor (seeker's candidate cards)
    public double? AverageRating { get; set; }
    public int     ReviewCount   { get; set; }
}

public class DonationRequestMatchCandidatesViewModel
{
    public DonationRequestViewModel DonationRequest { get; set; } = null!;
    public List<DonationPostViewModel> Candidates { get; set; } = new();
}
