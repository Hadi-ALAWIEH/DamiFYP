using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DamiFYP.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DamiFYP.Extensions;

public static class BloodTypeExtensions
{
    // public static IQueryable WhereIf(
    //     this IQueryable<BloodType> source,
    //     bool condition,
    //     Expression<Func<BloodType, bool>> action)
    // {
    //     // return condition ? source : action(source);
    //     return condition ? source.Where(action) : source;
    // }

    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> source,
        bool condition,
        Expression<Func<T, bool>> predicate)
    {
        return condition ? source.Where(predicate) : source;
    }

    public static IEnumerable<T> WhereIf<T>(
        this IEnumerable<T> source,
        bool condition,
        Func<T, bool> predicate)
    {
        return condition ? source.Where(predicate) : source;
    }
}