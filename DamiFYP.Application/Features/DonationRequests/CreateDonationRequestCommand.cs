using MediatR;
using DamiFYP.Persistence.Contexts;
using DamiFYP.Domain.Models;

namespace DamiFYP.Application.Features.DonationRequests;

public class CreateDonationRequestCommand : IRequest<DonationRequestMatchCandidatesViewModel>
{
	public long UserId { get; set; }
	public string? BloodTypeName { get; set; }
	public int? Quantity { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public string? UrgencyLevel { get; set; }
	public DateTime? NeededByDate { get; set; }
}

public class CreateDonationRequestCommandHandler : IRequestHandler<CreateDonationRequestCommand, DonationRequestMatchCandidatesViewModel>
{
	private readonly DamiContext _context;
	private readonly IMatchService _matchService;

	public CreateDonationRequestCommandHandler(DamiContext context, IMatchService matchService)
	{
		_context = context;
		_matchService = matchService;
	}

	public async Task<DonationRequestMatchCandidatesViewModel> Handle(CreateDonationRequestCommand request, CancellationToken cancellationToken)
	{
		var entity = new DonationRequest
		{
			UserId = request.UserId,
			BloodTypeName = !string.IsNullOrWhiteSpace(request.BloodTypeName) && Enum.TryParse<BloodTypeName>(request.BloodTypeName, out var bt)
				? bt
				: default,
			Quantity = request.Quantity,
			Latitude = request.Latitude,
			Longitude = request.Longitude,
			UrgencyLevel = request.UrgencyLevel,
			NeededByDate = DateTime.SpecifyKind(request.NeededByDate ?? new DateTime(), DateTimeKind.Utc)
		};

		_context.DonationRequests.Add(entity);
		await _context.SaveChangesAsync(cancellationToken);

		var candidates = await _matchService.GetCandidates(entity.Id, cancellationToken);

		return new DonationRequestMatchCandidatesViewModel
		{
			DonationRequest = new DonationRequestViewModel
			{
				Id = entity.Id,
				UserId = entity.UserId,
				BloodTypeName = entity.BloodTypeName.ToString(),
				Quantity = entity.Quantity,
				Latitude = entity.Latitude,
				Longitude = entity.Longitude,
				UrgencyLevel = entity.UrgencyLevel,
				Status = entity.Status,
				CreatedAt = entity.CreatedAt,
				NeededByDate = entity.NeededByDate
			},
			Candidates = candidates
		};
	}
}
