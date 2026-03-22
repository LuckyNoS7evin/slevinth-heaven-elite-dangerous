using SlevinthHeavenEliteDangerous.Data;
using SlevinthHeavenEliteDangerous.Services.Models;
using System.Threading.Tasks;

namespace SlevinthHeavenEliteDangerous.DataStorage.Services;

/// <summary>
/// Service for persisting GeneralControl data to local storage
/// </summary>
public class GeneralControlDataService
{
    private readonly DataService<FSDTimingModel> _dataService;

    public GeneralControlDataService()
    {
        _dataService = new DataService<FSDTimingModel>("general_control_data.json");
    }

    public Task SaveDataAsync(FSDTimingModel data) => _dataService.SaveDataAsync(data);

    public Task<FSDTimingModel?> LoadDataAsync() => _dataService.LoadDataAsync();

    public Task<bool> DataExistsAsync() => _dataService.DataExistsAsync();

    public Task DeleteDataAsync() => _dataService.DeleteDataAsync();
}
