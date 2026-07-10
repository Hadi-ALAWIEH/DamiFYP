using MediatR;

namespace DamiFYP.Application.Features.DonationRequests;

public class ConfirmDonationRequestMatchCommand : IRequest
{
    public long DonationRequestId { get; set; }
    public long DonationPostId { get; set; }
}

public class ConfirmDonationRequestMatchCommandHandler : IRequestHandler<ConfirmDonationRequestMatchCommand>
{
    private readonly IMatchService _matchService;

    public ConfirmDonationRequestMatchCommandHandler(IMatchService matchService)
    {
        _matchService = matchService;
    }

    public async Task Handle(ConfirmDonationRequestMatchCommand request, CancellationToken cancellationToken)
    {
        await _matchService.ConfirmMatch(request.DonationRequestId, request.DonationPostId, cancellationToken);
    }
}
