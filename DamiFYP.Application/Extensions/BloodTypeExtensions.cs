using DamiFYP.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DamiFYP.Extensions;

public static class BloodTypeExtensions
{
    public static IEnumerable<BloodType> WhereIf(this IEnumerable<BloodType> source, bool condition,
        Func<IEnumerable<BloodType>, IEnumerable<BloodType>> action)
    {
        return condition ? source : action(source);
    }
}