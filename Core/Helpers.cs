using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace DofusBatteriesIncluded.Core;

public static class Helpers
{
    delegate IntPtr FindObjectsOfTypeDelegate(IntPtr type);

    public static T[] FindObjectsOfType<T>() where T: Il2CppObjectBase
    {
        Type type = typeof(T);
        return new Il2CppReferenceArray<T>(
            GetICallUnreliable<FindObjectsOfTypeDelegate>(
                    "UnityEngine.Resources::FindObjectsOfTypeAll",
                    "UnityEngine.ResourcesAPIInternal::FindObjectsOfTypeAll"
                ) // Unity 2020+ updated to this
                .Invoke(Il2CppType.From(type).Pointer)
        );
    }

    public static T FindObjectOfType<T>() where T: Il2CppObjectBase => FindObjectsOfType<T>().FirstOrDefault();

    static readonly Dictionary<string, Delegate> unreliableCache = new();

    /// <summary>
    ///     Get an iCall which may be one of multiple different signatures (ie, the name changed in different Unity versions).
    ///     Each possible signature must have the same Delegate type, it can only vary by name.
    /// </summary>
    public static T GetICallUnreliable<T>(params string[] possibleSignatures) where T: Delegate
    {
        // use the first possible signature as the 'key'.
        string key = possibleSignatures.First();

        if (unreliableCache.TryGetValue(key, out Delegate value))
        {
            return (T)value;
        }

        foreach (string sig in possibleSignatures)
        {
            IntPtr ptr = IL2CPP.il2cpp_resolve_icall(sig);
            if (ptr != IntPtr.Zero)
            {
                T iCall = (T)Marshal.GetDelegateForFunctionPointer(ptr, typeof(T));
                unreliableCache.Add(key, iCall);
                return iCall;
            }
        }

        throw new MissingMethodException($"Could not find any iCall from list of provided signatures starting with '{key}'!");
    }
}
