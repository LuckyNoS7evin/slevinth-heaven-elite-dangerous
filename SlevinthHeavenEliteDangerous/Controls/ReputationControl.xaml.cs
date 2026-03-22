using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using SlevinthHeavenEliteDangerous.Services;
using SlevinthHeavenEliteDangerous.ViewModels;

namespace SlevinthHeavenEliteDangerous.Controls;

public sealed partial class ReputationControl : UserControl
{
    private readonly ReputationViewModel _viewModel;

    public ReputationControl()
    {
        var service = App.Services.GetRequiredService<ReputationService>();
        _viewModel = new ReputationViewModel(DispatcherQueue, service);

        this.InitializeComponent();

        this.DataContext = _viewModel;
    }
}
