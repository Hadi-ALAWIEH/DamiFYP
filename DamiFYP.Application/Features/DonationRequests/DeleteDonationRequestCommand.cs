using System.Threading;
using System.Threading.Tasks;
using MediatR;
using DamiFYP.Persistence.Contexts;
using DamiFYP.Application.Helpers;
using DamiFYP.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DamiFYP.Application.Features.DonationRequests;

public class DeleteDonationRequestCommand : IRequest<Unit>
{
    public long Id { get; set; }
}

public class DeleteDonationRequestCommandHandler : IRequestHandler<DeleteDonationRequestCommand, Unit>
{
    private readonly DamiContext _context;
    private readonly ICurrentUserProfileService _currentUserProfileService;

    public DeleteDonationRequestCommandHandler(DamiContext context, ICurrentUserProfileService currentUserProfileService)
    {
        _context = context;
        _currentUserProfileService = currentUserProfileService;
    }

    public async Task<Unit> Handle(DeleteDonationRequestCommand request, CancellationToken cancellationToken)
    {
        var currentUser = await _currentUserProfileService.GetCurrentAsync(cancellationToken);
        var entity = await _context.DonationRequests
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null) return Unit.Value;
        if (entity.DamiUserId != currentUser!.UserId) throw new UnauthorizedAccessException();
        if (entity.Status != DonationRequestStatus.Pending)
            throw new InvalidOperationException("Only pending requests can be deleted.");

        _context.DonationRequests.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
