using System;
using System.Net.Http;
using System.Threading.Tasks;
using DofusBatteriesIncluded.Core;
using Microsoft.Extensions.Logging;
using TreasureSolver.Clients;
using Direction = DofusBatteriesIncluded.Core.Maps.Direction;

namespace DofusBatteriesIncluded.TreasureSolver.Clues;

public class RemoteCluesService : ICluesService
{
    readonly ILogger _logger = DBI.Logging.Create<RemoteCluesService>();

    public async Task<long?> FindMapOfNextClue(long startMapId, Direction direction, int clueId, int cluesMaxDistance)
    {
        TreasureSolverClient treasureSolverClient = new(new HttpClient { Timeout = TimeSpan.FromSeconds(10) });
        global::TreasureSolver.Clients.Direction mappedDirection = direction switch
        {
            Direction.Top => global::TreasureSolver.Clients.Direction.North,
            Direction.Right => global::TreasureSolver.Clients.Direction.West,
            Direction.Left => global::TreasureSolver.Clients.Direction.East,
            Direction.Bottom => global::TreasureSolver.Clients.Direction.South,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };

        FindNextMapResponse result = await treasureSolverClient.FindNextMapAsync(startMapId, mappedDirection, clueId);
        return result?.MapId;
    }

    public Task RegisterCluesAsync(long mapId, params ClueWithStatus[] clues)
    {
        CluesClient cluesClient = new(new HttpClient { Timeout = TimeSpan.FromSeconds(10) });
        _logger.LogInformation("NOT IMPLEMENTED: register clues");
        return Task.CompletedTask;
    }
}
