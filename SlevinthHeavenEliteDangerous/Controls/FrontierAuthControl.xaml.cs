using SlevinthHeavenEliteDangerous.Services;
using SlevinthHeavenEliteDangerous.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace SlevinthHeavenEliteDangerous.Controls;

public sealed partial class FrontierAuthControl : UserControl
{
    private readonly FrontierAuthViewModel _viewModel;

    public FrontierAuthControl()
    {
        var service = App.Services.GetRequiredService<FrontierAuthService>();
        _viewModel = new FrontierAuthViewModel(DispatcherQueue, service);

        this.InitializeComponent();
        this.DataContext = _viewModel;
    }

    private async void LoginButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await _viewModel.LoginAsync();
    }

    private void LogoutButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        _viewModel.Logout();
    }
}
