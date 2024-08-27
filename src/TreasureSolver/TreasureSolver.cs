using System.Threading.Tasks;
using DofusBatteriesIncluded.Core;
using DofusBatteriesIncluded.TreasureSolver.Clues;
using DofusBatteriesIncluded.TreasureSolver.Database;
using Microsoft.Extensions.Logging;

namespace DofusBatteriesIncluded.TreasureSolver;

static class TreasureSolver
{
    static readonly ILogger Log = DBI.Logging.Create(typeof(TreasureSolver));

    static ICluesService _cluesService;

    public static async Task<ICluesService> GetCluesService()
    {
        _cluesService = await DatabaseCluesService.CreateAsync();
        return _cluesService;
    }
}
