using DBI.Hell.Extensions;
using Microsoft.Extensions.Logging;
using AppDomain = Il2CppSystem.AppDomain;
using EventArgs = Il2CppSystem.EventArgs;
using EventHandler = Il2CppSystem.EventHandler;
using Object = Il2CppSystem.Object;

namespace DBI.Hell;

public class HellBackgroundJobs
{
    static readonly ILogger Logger = Hell.Logging.Create<HellBackgroundJobs>();
    readonly CancellationTokenSource _cancellationTokenSource = new();

    internal HellBackgroundJobs()
    {
        AppDomain.CurrentDomain.add_ProcessExit((EventHandler)OnIl2cppDomainExit);
    }

    public void Start(Func<CancellationToken, Task> jobFactory, string name = null)
    {
        Task job = jobFactory(_cancellationTokenSource.Token);
        job.Forget(Logger, name);
    }

    void OnIl2cppDomainExit(Object _, EventArgs __)
    {
        Logger.LogInformation("Game is exiting, stopping all background jobs...");
        _cancellationTokenSource.Cancel();
    }
}
