using System.Threading;
using System.Threading.Tasks;

namespace DamiFYP.Application.Helpers;

public interface ICurrentUserProfileService
{
    Task<UserProfile?> GetCurrentAsync(CancellationToken cancellationToken = default);
    Task<UserProfile?> GetByUserIdAsync(string keycloakUserId, string email, CancellationToken cancellationToken = default);
    Task InvalidateAsync(string keycloakUserId);
}

