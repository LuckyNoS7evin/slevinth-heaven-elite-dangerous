using SlevinthHeavenEliteDangerous.Data;
using SlevinthHeavenEliteDangerous.Services.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SlevinthHeavenEliteDangerous.DataStorage.Services;

/// <summary>
/// Service for persisting VisitedSystems data to local storage
/// </summary>
public class VisitedSystemsDataService
{
    private readonly DataService<List<VisitedSystemCard>> _dataService;

    public VisitedSystemsDataService()
    {
        _dataService = new DataService<List<VisitedSystemCard>>("visited_systems_data.json");
    }

    public Task SaveDataAsync(List<VisitedSystemCard> data) => _dataService.SaveDataAsync(data);

    public Task<List<VisitedSystemCard>?> LoadDataAsync() => _dataService.LoadDataAsync();

    public Task<bool> DataExistsAsync() => _dataService.DataExistsAsync();

    public Task DeleteDataAsync() => _dataService.DeleteDataAsync();
}
