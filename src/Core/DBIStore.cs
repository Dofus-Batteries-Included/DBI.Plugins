using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DofusBatteriesIncluded.Core.Stores;
using Il2CppSystem;
using Microsoft.Extensions.Logging;

namespace DofusBatteriesIncluded.Core;

// ReSharper disable once InconsistentNaming
public class DBIStore
{
    static readonly ILogger Log = DBI.Logging.Create<DBIStore>();
    static readonly string BasePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DBI", "Stores");
    readonly Dictionary<string, ILocalStorage> _stores = [];

    public async Task<string> GetValueAsync(string name, string key)
    {
        ILocalStorage store = GetStore(name);
        if (store == null)
        {
            return null;
        }

        return await store.GetAsync(key);
    }

    public async Task SetValueAsync(string name, string key, string value)
    {
        ILocalStorage store = GetStore(name);
        if (store == null)
        {
            string path = GetStorePath(name);
            Log.LogInformation("Created new store {Name} at {Path}.", name, path);
            store = CreateStore(name, path);
        }

        await store.SetAsync(key, value);
    }

    ILocalStorage CreateStore(string name, string path)
    {
        ILocalStorage store = new JsonFileLocalStorage(path);
        _stores[name] = store;

        return store;
    }

    ILocalStorage GetStore(string name)
    {
        if (_stores.TryGetValue(name, out ILocalStorage store))
        {
            return store;
        }

        string path = GetStorePath(name);
        if (File.Exists(path))
        {
            Log.LogInformation("Found store {Name} at {Path}.", name, path);
            return CreateStore(name, path);
        }

        return null;
    }

    static string GetStorePath(string name) => Path.Join(BasePath, $"{name}.bin");
}
