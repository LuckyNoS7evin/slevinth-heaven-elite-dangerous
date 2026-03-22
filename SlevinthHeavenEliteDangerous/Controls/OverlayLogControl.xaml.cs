using SlevinthHeavenEliteDangerous.Services;
using SlevinthHeavenEliteDangerous.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace SlevinthHeavenEliteDangerous.Controls;

/// <summary>
/// Overlay panel showing a live log of valuable body scans and exobio discoveries.
/// </summary>
public sealed partial class OverlayLogControl : UserControl
{
    private readonly OverlayLogViewModel _viewModel;

    public OverlayLogControl()
    {
        var overlayLogService = App.Services.GetRequiredService<OverlayLogService>();

        _viewModel = new OverlayLogViewModel(DispatcherQueue, overlayLogService);

        InitializeComponent();

        LogItems.ItemsSource = _viewModel.Entries;

        // Dispose when the window closes, not on visual tree removal
        App.MainWindow.Closed += (_, _) => _viewModel.Dispose();
    }
}
