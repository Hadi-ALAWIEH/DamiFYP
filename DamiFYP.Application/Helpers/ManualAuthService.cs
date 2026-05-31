using DamiFYP.Application.Helpers;
using DamiFYP.Domain.Models;
using DamiFYP.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

public class ManualAuthService : IDamiAuthService
{
    private readonly DamiContext _context;

    public ManualAuthService(DamiContext context)
    {
        _context = context;
    }

    public async Task<User?> AuthUserAsync(long userId, string email)
    {
        var users = await _context.Users.ToListAsync();
        return users.Any(user => user.Id == userId && user.Email == email)
            ? users.First(user => user.Id == userId)
            : null;
    }
}