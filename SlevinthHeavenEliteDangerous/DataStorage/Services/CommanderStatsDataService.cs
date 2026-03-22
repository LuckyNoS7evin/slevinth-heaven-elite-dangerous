using SlevinthHeavenEliteDangerous.Data;
using SlevinthHeavenEliteDangerous.Services.Models;
using System.Threading.Tasks;

namespace SlevinthHeavenEliteDangerous.DataStorage.Services;

public class CommanderStatsDataService
{
    private readonly DataService<CommanderStatsModel> _dataService;

    public CommanderStatsDataService()
    {
        _dataService = new DataService<CommanderStatsModel>("commander_stats_data.json");
    }

    public Task SaveDataAsync(CommanderStatsModel data) => _dataService.SaveDataAsync(data);

    public Task<CommanderStatsModel?> LoadDataAsync() => _dataService.LoadDataAsync();
}
