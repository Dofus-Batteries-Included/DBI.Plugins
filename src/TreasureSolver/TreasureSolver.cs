using DofusBatteriesIncluded.Core;
using DofusBatteriesIncluded.TreasureSolver.Clues;
using Microsoft.Extensions.Logging;

namespace DofusBatteriesIncluded.TreasureSolver;

static class TreasureSolver
{
    static readonly ILogger Log = DBI.Logging.Create(typeof(TreasureSolver));

    static ICluesService _cluesService;

    public static ICluesService GetCluesService() => _cluesService ??= new MultiCluesService([new RemoteCluesService(), LocalCluesService.Create()]);

    public static bool TryGetCluesService(out ICluesService cluesService)
    {
        cluesService = _cluesService;
        return cluesService != null;
    }
}
