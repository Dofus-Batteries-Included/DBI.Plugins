using System;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppSystem.Collections;
using Task = System.Threading.Tasks.Task;

namespace DofusBatteriesIncluded.Plugins.Core.Coroutines;

public static class CoroutineExtensions
{
    public static IEnumerator WaitForCompletion(Task task) => WaitUntil(() => task.IsCompleted);
    public static IEnumerator WaitUntil(Func<bool> condition) => WaitUntilManaged(condition).WrapToIl2Cpp();

    static System.Collections.IEnumerator WaitUntilManaged(Func<bool> condition)
    {
        while (!condition())
        {
            yield return null;
        }
    }
}
