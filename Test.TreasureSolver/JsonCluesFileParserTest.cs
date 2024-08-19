using DofusBatteriesIncluded.TreasureSolver.Clues.Serialization;
using DofusBatteriesIncluded.TreasureSolver.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.TreasureSolver;

[TestClass]
public class JsonCluesFileParserTest
{
    [TestMethod]
    public async Task ShouldParseJsonCluesFile()
    {
        await using FileStream stream = File.OpenRead("TestFiles/clues.json");
        JsonCluesFileParser parser = new();
        Dictionary<Position, int[]>? result = await parser.ParseAsync(stream);

        result.Should()
            .BeEquivalentTo(
                new Dictionary<Position, int[]>
                {
                    { new Position(-61, -50), [113, 187, 197] },
                    { new Position(-7, 29), [177, 56, 35] }
                }
            );
    }
}
