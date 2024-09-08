using System.Threading.Tasks;
using CancellationToken = System.Threading.CancellationToken;

namespace DofusBatteriesIncluded.Core.Stores;

public interface ILocalStorage
{
    Task<string> GetAsync(string key, CancellationToken cancellationToken = default);
    Task SetAsync(string key, string value, CancellationToken cancellationToken = default);
}
