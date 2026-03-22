using SlevinthHeavenEliteDangerous.Data;
using SlevinthHeavenEliteDangerous.Services.Models;
using System.Threading.Tasks;

namespace SlevinthHeavenEliteDangerous.DataStorage.Services;

/// <summary>
/// Persists reputation data to local storage.
/// </summary>
public class ReputationDataService
{
    private readonly DataService<ReputationModel> _dataService;

    public ReputationDataService()
    {
        _dataService = new DataService<ReputationModel>("reputation_data.json");
    }

    public Task SaveDataAsync(ReputationModel data) => _dataService.SaveDataAsync(data);

    public Task<ReputationModel?> LoadDataAsync() => _dataService.LoadDataAsync();

    public Task<bool> DataExistsAsync() => _dataService.DataExistsAsync();

    public Task DeleteDataAsync() => _dataService.DeleteDataAsync();
}
