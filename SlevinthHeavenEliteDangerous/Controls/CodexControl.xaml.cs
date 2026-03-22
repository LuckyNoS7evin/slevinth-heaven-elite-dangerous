using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using SlevinthHeavenEliteDangerous.Services;
using SlevinthHeavenEliteDangerous.ViewModels;

namespace SlevinthHeavenEliteDangerous.Controls;

public sealed partial class CodexControl : UserControl
{
    private readonly CodexViewModel _viewModel;

    public CodexControl()
    {
        var service = App.Services.GetRequiredService<CodexService>();
        _viewModel = new CodexViewModel(DispatcherQueue, service);

        this.InitializeComponent();

        this.DataContext = _viewModel;
    }
}
