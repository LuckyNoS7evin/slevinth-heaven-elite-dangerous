using SlevinthHeavenEliteDangerous.Data;
using SlevinthHeavenEliteDangerous.Services.Models;
using System.Threading.Tasks;

namespace SlevinthHeavenEliteDangerous.DataStorage.Services;

/// <summary>
/// Service for persisting ExoBio data to local storage
/// </summary>
public class ExoBioDataService
{
    private readonly DataService<ExoBioStateModel> _dataService;

    public ExoBioDataService()
    {
        _dataService = new DataService<ExoBioStateModel>("exobio_data.json");
    }

    public Task SaveDataAsync(ExoBioStateModel data) => _dataService.SaveDataAsync(data);

    public Task<ExoBioStateModel?> LoadDataAsync() => _dataService.LoadDataAsync();

    public Task<bool> DataExistsAsync() => _dataService.DataExistsAsync();

    public Task DeleteDataAsync() => _dataService.DeleteDataAsync();
}
