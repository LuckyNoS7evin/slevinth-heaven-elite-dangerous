using SlevinthHeavenEliteDangerous.Data;
using SlevinthHeavenEliteDangerous.Services.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SlevinthHeavenEliteDangerous.DataStorage.Services;

/// <summary>
/// Service for persisting Ranks data to local storage
/// </summary>
public class RankDataService
{
    private readonly DataService<List<RankModel>> _dataService;

    public RankDataService()
    {
        _dataService = new DataService<List<RankModel>>("ranks_data.json");
    }

    public Task SaveDataAsync(List<RankModel> data) => _dataService.SaveDataAsync(data);

    public Task<List<RankModel>?> LoadDataAsync() => _dataService.LoadDataAsync();

    public Task<bool> DataExistsAsync() => _dataService.DataExistsAsync();

    public Task DeleteDataAsync() => _dataService.DeleteDataAsync();
}
