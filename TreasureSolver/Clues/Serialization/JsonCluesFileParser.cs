using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DofusBatteriesIncluded.TreasureSolver.Models;

namespace DofusBatteriesIncluded.TreasureSolver.Clues.Serialization;

public class JsonCluesFileParser
{
    public async Task<Dictionary<Position, int[]>> ParseAsync(Stream stream)
    {
        MapClues[] clues = await JsonSerializer.DeserializeAsync<MapClues[]>(stream, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        return clues.ToDictionary(c => new Position(c.X, c.Y), c => c.Clues);
    }

    class MapClues
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int[] Clues { get; set; }
    }
}
