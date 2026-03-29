using SlevinthHeavenEliteDangerous.Data;
using SlevinthHeavenEliteDangerous.Services.Models;
using System.Threading.Tasks;

namespace SlevinthHeavenEliteDangerous.DataStorage.Services;

/// <summary>
/// Service for persisting combat log data to local storage
/// </summary>
public class CombatDataService
{
    private readonly DataService<CombatStateModel> _dataService;

    public CombatDataService()
    {
        _dataService = new DataService<CombatStateModel>("combat_data.json");
    }

    public Task SaveDataAsync(CombatStateModel data) => _dataService.SaveDataAsync(data);

    public Task<CombatStateModel?> LoadDataAsync() => _dataService.LoadDataAsync();

    public Task<bool> DataExistsAsync() => _dataService.DataExistsAsync();

    public Task DeleteDataAsync() => _dataService.DeleteDataAsync();
}
