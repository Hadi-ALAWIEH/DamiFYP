using System.Security.Authentication;
using DamiFYP.Application.Features.Authentication;
using DamiFYP.Application.Helpers;
using MediatR;

public class LoginUserRequestHandler : IRequestHandler<LoginUserRequest, LoginUserRequestViewModel>
{
    private readonly ITokenService _tokenService;
    private readonly IDamiAuthService _damiAuthService;

    public LoginUserRequestHandler(ITokenService tokenService, IDamiAuthService damiAuthService)
    {
        _tokenService = tokenService;
        _damiAuthService = damiAuthService;
    }

    public async Task<LoginUserRequestViewModel> Handle(LoginUserRequest request, CancellationToken cancellationToken)
    {
        // todo: logic for authenticating the user
        var authedUser = await _damiAuthService.AuthUserAsync(request.UserId, request.Email);

        // generate token for user
        if (authedUser is not null)
            return new LoginUserRequestViewModel()
                { Token = _tokenService.GenerateToken(authedUser.Id, authedUser.Email, authedUser.Role.ToString()) };

        throw new InvalidCredentialException("You have entered invalid credentials");
    }
}