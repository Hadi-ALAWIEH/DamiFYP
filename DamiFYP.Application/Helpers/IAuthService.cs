using System.Threading.Tasks;
using DamiFYP.Domain.Models;

namespace DamiFYP.Application.Helpers;

public interface IDamiAuthService
{
    public Task<User?> AuthUserAsync(long userId, string email);
}