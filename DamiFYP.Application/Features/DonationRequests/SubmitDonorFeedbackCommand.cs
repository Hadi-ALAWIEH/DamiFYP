using System.Threading;
using System.Threading.Tasks;
using MediatR;
using DamiFYP.Domain.Models;
using DamiFYP.Application.Helpers;
using DamiFYP.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace DamiFYP.Application.Features.DonationRequests;

public class SubmitDonorFeedbackCommand : IRequest<Unit>
{
    public long    DonationRequestId { get; set; }
    public int     Rating            { get; set; }
    public string? Comment           { get; set; }
}

public class SubmitDonorFeedbackCommandHandler : IRequestHandler<SubmitDonorFeedbackCommand, Unit>
{
    private readonly DamiContext _context;
    private readonly ICurrentUserProfileService _currentUserProfileService;

    public SubmitDonorFeedbackCommandHandler(DamiContext context, ICurrentUserProfileService currentUserProfileService)
    {
        _context = context;
        _currentUserProfileService = currentUserProfileService;
    }

    public async Task<Unit> Handle(SubmitDonorFeedbackCommand request, CancellationToken cancellationToken)
    {
        var currentUser = await _currentUserProfileService.GetCurrentAsync(cancellationToken);

        var donationRequest = await _context.DonationRequests
            .Include(r => r.DonorFeedback)
            .FirstOrDefaultAsync(r => r.Id == request.DonationRequestId, cancellationToken);

        if (donationRequest == null)
            return Unit.Value;

        if (donationRequest.DamiUserId != currentUser!.UserId)
            throw new UnauthorizedAccessException();

        if (donationRequest.Status != DonationRequestStatus.Completed)
            throw new InvalidOperationException("Feedback can only be submitted for completed requests.");

        if (donationRequest.DonorFeedback != null)
            throw new InvalidOperationException("Feedback has already been submitted for this request.");

        _context.DonorFeedbacks.Add(new DonorFeedback
        {
            DonationRequestId = request.DonationRequestId,
            Rating            = request.Rating,
            Comment           = request.Comment,
        });

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
