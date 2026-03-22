using SlevinthHeavenEliteDangerous.Data;
using SlevinthHeavenEliteDangerous.Services.Models;
using System.Threading.Tasks;

namespace SlevinthHeavenEliteDangerous.DataStorage.Services;

/// <summary>
/// Persists codex discovery data to local storage.
/// </summary>
public class CodexDataService
{
    private readonly DataService<CodexStateModel> _dataService;

    public CodexDataService()
    {
        _dataService = new DataService<CodexStateModel>("codex_data.json");
    }

    public Task SaveDataAsync(CodexStateModel data) => _dataService.SaveDataAsync(data);

    public Task<CodexStateModel?> LoadDataAsync() => _dataService.LoadDataAsync();

    public Task<bool> DataExistsAsync() => _dataService.DataExistsAsync();

    public Task DeleteDataAsync() => _dataService.DeleteDataAsync();
}
