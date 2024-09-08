using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace DofusBatteriesIncluded.Plugins.Core.Maps.PathFinding;

public class GamePath : IReadOnlyList<GamePathStep>
{
    readonly IReadOnlyList<GamePathStep> _steps;

    public GamePath(long fromMap, long toMap, IReadOnlyList<GamePathStep> steps)
    {
        FromMap = fromMap;
        ToMap = toMap;
        _steps = steps;
    }

    public long FromMap { get; }
    public long ToMap { get; }
    public int Count => _steps.Count;
    public GamePathStep this[int index] => _steps[index];


    public IEnumerator<GamePathStep> GetEnumerator() => _steps.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString()
    {
        if (Count == 0)
        {
            return "(empty path)";
        }

        StringBuilder stringBuilder = new();

        foreach (GamePathStep step in _steps)
        {
            switch (step)
            {
                case ChangeMapStep changeMapStep:
                    stringBuilder.Append(changeMapStep.Direction.ToArrow());
                    break;
            }
        }

        return stringBuilder.ToString();
    }

    public static GamePath Empty(long map) => new(map, map, []);
}
