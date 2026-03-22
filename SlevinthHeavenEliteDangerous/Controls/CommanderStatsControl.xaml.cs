using SlevinthHeavenEliteDangerous.Services;
using SlevinthHeavenEliteDangerous.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace SlevinthHeavenEliteDangerous.Controls;

public sealed partial class CommanderStatsControl : UserControl
{
    private readonly CommanderStatsViewModel _viewModel;

    public CommanderStatsControl()
    {
        var service = App.Services.GetRequiredService<CommanderStatsService>();
        _viewModel = new CommanderStatsViewModel(DispatcherQueue, service);

        this.InitializeComponent();
        this.DataContext = _viewModel;
    }
}
