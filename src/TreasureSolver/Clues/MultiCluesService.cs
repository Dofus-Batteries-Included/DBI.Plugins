using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DofusBatteriesIncluded.Core;
using DofusBatteriesIncluded.Core.Maps;
using Microsoft.Extensions.Logging;

namespace DofusBatteriesIncluded.TreasureSolver.Clues;

public class MultiCluesService : ICluesService
{
    readonly ILogger _logger = DBI.Logging.Create<MultiCluesService>();
    readonly List<ICluesService> _cluesServices;

    public MultiCluesService(IEnumerable<ICluesService> cluesServices)
    {
        _cluesServices = cluesServices.ToList();
    }

    public async Task<long?> FindMapOfNextClue(long startMapId, Direction direction, int clueId, int cluesMaxDistance)
    {
        for (int index = 0; index < _cluesServices.Count; index++)
        {
            ICluesService service = _cluesServices[index];
            try
            {
                return await service.FindMapOfNextClue(startMapId, direction, clueId, cluesMaxDistance);
            }
            catch (Exception exn)
            {
                ICluesService next = index + 1 >= _cluesServices.Count ? null : _cluesServices[index + 1];
                if (next == null)
                {
                    _logger.LogError(exn, "Could not find clue using {Service}. This was the last available service, will stop looking.", service);
                }
                else
                {
                    _logger.LogError(exn, "Could not find clue using {Service}, will fall back to next service {NextService}.", service, next);
                }
            }
        }

        return null;
    }

    public async Task RegisterCluesAsync(long mapId, params ClueWithStatus[] clues)
    {
        foreach (ICluesService service in _cluesServices)
        {
            try
            {
                await service.RegisterCluesAsync(mapId, clues);
            }
            catch (Exception exn)
            {
                _logger.LogError(exn, "Could not register clues using {Service}.", service);
            }
        }
    }
}
