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

    // True when this specific donor has already been confirmed as a match for
    // the donation request this candidate list was fetched for. Lets the UI
    // show "Matched" and disable the button for that one donor, even after a
    // page reload, instead of re-deriving it from in-memory state that's lost
    // on refresh.
    public bool IsMatched { get; set; }
    public string? DonorProfilePictureUrl { get; set; }
}

public class DonationRequestMatchCandidatesViewModel
{
    public DonationRequestViewModel DonationRequest { get; set; } = null!;
    public List<DonationPostViewModel> Candidates { get; set; } = new();
}
