using SlevinthHeavenEliteDangerous.Services;
using SlevinthHeavenEliteDangerous.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SlevinthHeavenEliteDangerous.Controls;

/// <summary>
/// UserControl for managing ExoBio discoveries
/// </summary>
public sealed partial class ExoBioControl : UserControl
{
    private readonly ExoBioService _service;
    private ExoBioViewModel? _viewModel;
    private ExoBioSalesHistoryViewModel? _salesViewModel;

    public ExoBioControl()
    {
        // Get service from DI container
        _service = App.Services.GetRequiredService<ExoBioService>();

        this.InitializeComponent();

        // Create ViewModels early so they can receive events
        _viewModel = new ExoBioViewModel(DispatcherQueue, _service);
        _salesViewModel = new ExoBioSalesHistoryViewModel(DispatcherQueue, _service);

        // Set DataContext for child controls
        ExoBioInfoCard.DataContext = _viewModel;
        ExoBioCardList.DataContext = _viewModel;
        ExoBioSalesHistory.DataContext = _salesViewModel;

    }
}
