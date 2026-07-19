using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DamiFYP.Application.Features.DonationRequests;

public interface IMatchService
{
    Task<DonationRequestMatchCandidatesViewModel> GetCandidatesAsync(long donationRequestId, CancellationToken cancellationToken);
    Task ConfirmMatch(long donationRequestId, long donationPostId, CancellationToken cancellationToken);
}

