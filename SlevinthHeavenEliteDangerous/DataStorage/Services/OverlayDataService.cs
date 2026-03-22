using SlevinthHeavenEliteDangerous.Data;
using SlevinthHeavenEliteDangerous.Services.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SlevinthHeavenEliteDangerous.DataStorage.Services;

internal class OverlayDataService
{
    private readonly DataService<List<OverlayLogEntryRecord>> _dataService;

    public OverlayDataService()
    {
        _dataService = new DataService<List<OverlayLogEntryRecord>>("overlay_log_data.json");
    }

    public Task SaveDataAsync(List<OverlayLogEntryRecord> data) => _dataService.SaveDataAsync(data);

    public Task<List<OverlayLogEntryRecord>?> LoadDataAsync() => _dataService.LoadDataAsync();

    public Task<bool> DataExistsAsync() => _dataService.DataExistsAsync();

    public Task DeleteDataAsync() => _dataService.DeleteDataAsync();
}
