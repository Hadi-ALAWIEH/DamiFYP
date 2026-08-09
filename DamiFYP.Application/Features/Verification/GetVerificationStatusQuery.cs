using System.Threading;
using System.Threading.Tasks;
using DamiFYP.Application.Helpers;
using DamiFYP.Domain.Models;
using DamiFYP.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DamiFYP.Application.Features.Verification;

public class GetVerificationStatusQuery : IRequest<VerificationStatusViewModel>
{
}

public class VerificationStatusViewModel
{
    public VerificationStatus Status { get; set; }

    // Total attempts logged so far (VerificationAttempt rows) - lets the
    // frontend show "attempt 2 of N" / decide when to stop offering retry
    // once a max-attempts policy is added later.
    public int AttemptCount { get; set; }
}

public class GetVerificationStatusQueryHandler : IRequestHandler<GetVerificationStatusQuery, VerificationStatusViewModel>
{
    private readonly ICurrentUserProfileService _profileService;
    private readonly DamiContext _context;

    public GetVerificationStatusQueryHandler(ICurrentUserProfileService profileService, DamiContext context)
    {
        _profileService = profileService;
        _context = context;
    }

    public async Task<VerificationStatusViewModel> Handle(GetVerificationStatusQuery request, CancellationToken cancellationToken)
    {
        var profile = await _profileService.GetCurrentAsync(cancellationToken);
        if (profile is null)
        {
            throw new InvalidOperationException("Current user is required to check verification status.");
        }

        var attemptCount = await _context.VerificationAttempts
            .AsNoTracking()
            .CountAsync(x => x.DamiUserId == profile.UserId, cancellationToken);

        return new VerificationStatusViewModel
        {
            Status = profile.VerificationStatus,
            AttemptCount = attemptCount
        };
    }
}
