using System.Threading;
using System.Threading.Tasks;
using MediatR;
using DamiFYP.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace DamiFYP.Application.Features.DonationRequests;

public class DeleteDonationRequestCommand : IRequest<Unit>
{
    public int Id { get; set; }
}

public class DeleteDonationRequestCommandHandler : IRequestHandler<DeleteDonationRequestCommand, Unit>
{
    private readonly DamiContext _context;

    public DeleteDonationRequestCommandHandler(DamiContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteDonationRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.DonationRequests.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (entity != null)
        {
            _context.DonationRequests.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}

