using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using SlevinthHeavenEliteDangerous.Services;
using SlevinthHeavenEliteDangerous.ViewModels;

namespace SlevinthHeavenEliteDangerous.Controls;

/// <summary>
/// Tab control showing combat stats, kills-to-rank estimation, and the kill log.
/// </summary>
public sealed partial class CombatControl : UserControl
{
    private readonly CombatViewModel _viewModel;

    public CombatControl()
    {
        var combatService = App.Services.GetRequiredService<CombatService>();
        var rankService   = App.Services.GetRequiredService<RankService>();
        _viewModel = new CombatViewModel(DispatcherQueue, combatService, rankService);

        this.InitializeComponent();
        this.DataContext = _viewModel;
    }
}
