using System;
using System.Threading;
using System.Threading.Tasks;
using DamiFYP.Application.Helpers;
using MediatR;
using DamiFYP.Persistence.Contexts;
using DamiFYP.Domain.Models;

namespace DamiFYP.Application.Features.DonationRequests;

public class CreateDonationRequestCommand : IRequest<DonationRequestMatchCandidatesViewModel>
{
	// public long UserId { get; set; }
	public string? BloodTypeName { get; set; }
	public int? Quantity { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public DonationRequestUrgency Urgency { get; set; }
	public DateTime? NeededByDate { get; set; }
	public string Address { get; set; }
}

public class CreateDonationRequestCommandHandler : IRequestHandler<CreateDonationRequestCommand, DonationRequestMatchCandidatesViewModel>
{
	private readonly DamiContext _context;
	private readonly IMatchService _matchService;
	private readonly ICurrentUserProfileService _profileService;

	public CreateDonationRequestCommandHandler(DamiContext context, IMatchService matchService, ICurrentUserProfileService profileService)
	{
		_context = context;
		_matchService = matchService;
		_profileService = profileService;
	}

	public async Task<DonationRequestMatchCandidatesViewModel> Handle(CreateDonationRequestCommand request, CancellationToken cancellationToken)
	{
		var entity = new DonationRequest
		{
			DamiUserId = (await _profileService.GetCurrentAsync(cancellationToken)).UserId,
			BloodTypeName = !string.IsNullOrWhiteSpace(request.BloodTypeName) && Enum.TryParse<BloodTypeName>(request.BloodTypeName, out var bt)
				? bt
				: default,
			Quantity = request.Quantity,
			Latitude = request.Latitude,
			Longitude = request.Longitude,
			Urgency = request.Urgency,
			NeededByDate = DateTime.SpecifyKind(request.NeededByDate ?? new DateTime(), DateTimeKind.Utc),
			Address = request.Address
		};

		_context.DonationRequests.Add(entity);
		await _context.SaveChangesAsync(cancellationToken);

		var candidates = await _matchService.GetCandidatesAsync(entity.Id, cancellationToken);

		return new DonationRequestMatchCandidatesViewModel
		{
			DonationRequest = new DonationRequestViewModel
			{
				Id = entity.Id,
				DamiUserId = entity.DamiUserId,
				BloodTypeName = entity.BloodTypeName.ToString(),
				Quantity = entity.Quantity,
				Latitude = entity.Latitude,
				Longitude = entity.Longitude,
				Urgency = entity.Urgency,
				Status = entity.Status,
				CreatedAt = entity.CreatedAt,
				NeededByDate = entity.NeededByDate,
				Address = entity.Address
			},
			Candidates = candidates.Candidates
		};
	}
}
