using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hangfire;
using DamiFYP.Application.Features.DonationRequests;

namespace DamiFYP.Services;

public class HangfireBackgroundJobDispatcher : IBackgroundJobDispatcher
{
    private readonly IBackgroundJobClient _client;

    public HangfireBackgroundJobDispatcher(IBackgroundJobClient client)
    {
        _client = client;
    }

    public void Enqueue<T>(Expression<Func<T, Task>> method)
    {
        _client.Enqueue(method);
    }
}

