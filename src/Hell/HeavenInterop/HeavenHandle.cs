using System.Diagnostics;

namespace DBI.Hell.HeavenInterop;

class HeavenHandle
{
    public HeavenHandle(Process process)
    {
        Process = process;
    }

    public Process Process { get; }
}
