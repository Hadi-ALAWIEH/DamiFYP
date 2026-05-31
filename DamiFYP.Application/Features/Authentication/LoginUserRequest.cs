using MediatR;

namespace DamiFYP.Application.Features.Authentication;

public class LoginUserRequest : IRequest<LoginUserRequestViewModel>
{
    public long UserId { get; set; }
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
}