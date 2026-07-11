using System.Linq;
using System.Threading.Tasks;
using DamiFYP.Domain.Models;
using DamiFYP.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace DamiFYP.Application.Helpers;

public class ManualAuthService(DamiContext context) : IDamiAuthService
{
    public async Task<User?> AuthUserAsync(long userId, string email)
    {
        var users = await context.Users.ToListAsync();
        return users.Any(user => user.Id == userId && user.Email == email)
            ? users.First(user => user.Id == userId)
            : null;
    }
}