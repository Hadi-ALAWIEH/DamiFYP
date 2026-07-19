using DamiFYP.Application.Helpers;
using DamiFYP.Domain.Models;
using DamiFYP.Persistence.Contexts;
using MediatR;

public class CreateDonationPostCommand : IRequest
{
    public BloodTypeName BloodTypeName { get; set; }
    public int? Quantity { get; set; }
}

public class CreateDonationPostCommandHandler : IRequestHandler<CreateDonationPostCommand>
{
    private readonly DamiContext _context;
    private readonly ICurrentUserProfileService _currentUserProfileService;

    public CreateDonationPostCommandHandler(DamiContext context,  ICurrentUserProfileService currentUserProfileService)
    {
        _context = context;
        _currentUserProfileService = currentUserProfileService;
    }

    public async Task Handle(CreateDonationPostCommand request, CancellationToken cancellationToken)
    {

        var post = new DonationPost()
        {
            BloodTypeName = request.BloodTypeName,
            Quantity = request.Quantity,
            DamiUserId = (await _currentUserProfileService.GetCurrentAsync(cancellationToken)).UserId
        };

        _context.DonationPosts.Add(post);
        await _context.SaveChangesAsync(cancellationToken);
    }
}