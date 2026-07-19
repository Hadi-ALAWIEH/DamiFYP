using System.Threading.Tasks;
using DamiFYP.Domain.Models;

namespace DamiFYP.Application.Helpers;

public interface IDamiAuthService
{
    public Task<DamiUser?> AuthUserAsync(long userId, string email);
}