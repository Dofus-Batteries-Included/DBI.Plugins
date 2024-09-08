using System;
using DofusBatteriesIncluded.Plugins.Core;
using DofusBatteriesIncluded.Plugins.TreasureSolver.Clues;
using Microsoft.Extensions.Logging;
using EventArgs = System.EventArgs;
using EventHandler = System.EventHandler;

namespace DofusBatteriesIncluded.Plugins.TreasureSolver;

static class TreasureSolver
{
    static readonly ILogger Log = DBI.Logging.Create(typeof(TreasureSolver));

    static Solver _solver;
    static RemoteCluesService _remoteCluesService;
    static LocalCluesService _localCluesService;
    static ICluesService _cluesService;
    public static event EventHandler CluesServiceChanged;

    public static ICluesService TryGetCluesService() => _cluesService;

    public static ICluesService GetCluesService()
    {
        if (_cluesService == null)
        {
            _cluesService = _solver switch
            {
                Solver.Remote => new MultiCluesService([GetRemoteCluesService(), GetLocalCluesService()]),
                Solver.Local => GetLocalCluesService(),
                _ => throw new ArgumentOutOfRangeException(nameof(_solver), _solver, null)
            };
        }

        return _cluesService;
    }

    public static void SetSolver(Solver solver)
    {
        _solver = solver;
        _cluesService = null;
        CluesServiceChanged?.Invoke(null, EventArgs.Empty);
    }

    static RemoteCluesService GetRemoteCluesService() => _remoteCluesService ??= new RemoteCluesService();
    static LocalCluesService GetLocalCluesService() => _localCluesService ??= LocalCluesService.Create();

    public enum Solver
    {
        Remote,
        Local
    }
}
