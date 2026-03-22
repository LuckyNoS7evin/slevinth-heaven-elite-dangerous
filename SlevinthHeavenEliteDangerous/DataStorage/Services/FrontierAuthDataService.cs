using SlevinthHeavenEliteDangerous.Data;
using SlevinthHeavenEliteDangerous.Services.Models;
using System.Threading.Tasks;

namespace SlevinthHeavenEliteDangerous.DataStorage.Services;

public class FrontierAuthDataService
{
    private readonly DataService<FrontierTokens> _dataService;

    public FrontierAuthDataService()
    {
        _dataService = new DataService<FrontierTokens>("frontier_auth_data.json");
    }

    public Task SaveDataAsync(FrontierTokens data) => _dataService.SaveDataAsync(data);

    public Task<FrontierTokens?> LoadDataAsync() => _dataService.LoadDataAsync();
}
