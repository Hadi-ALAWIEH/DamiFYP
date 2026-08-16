using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using DamiFYP.Persistence.Contexts;
using DamiFYP.Application.Helpers;
using Microsoft.EntityFrameworkCore;

namespace DamiFYP.Application.Features.DonationPosts;

public class DeleteDonationPostCommand : IRequest<Unit>
{
    public long Id { get; set; }
}

public class DeleteDonationPostCommandHandler : IRequestHandler<DeleteDonationPostCommand, Unit>
{
    private readonly DamiContext _context;
    private readonly ICurrentUserProfileService _currentUserProfileService;

    public DeleteDonationPostCommandHandler(DamiContext context, ICurrentUserProfileService currentUserProfileService)
    {
        _context = context;
        _currentUserProfileService = currentUserProfileService;
    }

    public async Task<Unit> Handle(DeleteDonationPostCommand request, CancellationToken cancellationToken)
    {
        var currentUser = await _currentUserProfileService.GetCurrentAsync(cancellationToken);

        var entity = await _context.DonationPosts
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (entity == null) return Unit.Value;
        if (entity.DamiUserId != currentUser!.UserId) throw new UnauthorizedAccessException();

        var hasMatch = await _context.Matches
            .AnyAsync(m => m.DonationPostId == request.Id, cancellationToken);

        if (hasMatch)
            throw new InvalidOperationException(
                "This post cannot be deleted because it has been matched with a request.");

        _context.DonationPosts.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
