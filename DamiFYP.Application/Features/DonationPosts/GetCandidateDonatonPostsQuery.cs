using DamiFYP.Application.Features.DonationRequests;
using MediatR;

public class GetCandidateDonationPostsQuery : IRequest<DonationRequestMatchCandidatesViewModel>
{
    public long DonationRequestId { get; set; }
}


public class GetCandidateDonationPostsQueryHandler : IRequestHandler<GetCandidateDonationPostsQuery, DonationRequestMatchCandidatesViewModel>
{
    private readonly IMatchService _matchService;

    public GetCandidateDonationPostsQueryHandler(IMatchService matchService)
    {
        _matchService = matchService;
    }

    public async Task<DonationRequestMatchCandidatesViewModel> Handle(GetCandidateDonationPostsQuery request,
        CancellationToken cancellationToken)
        =>  await _matchService.GetCandidatesAsync(request.DonationRequestId, cancellationToken);

}