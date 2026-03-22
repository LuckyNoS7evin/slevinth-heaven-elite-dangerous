using SlevinthHeavenEliteDangerous.Services;
using SlevinthHeavenEliteDangerous.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SlevinthHeavenEliteDangerous.Controls;

/// <summary>
/// UserControl for managing visited systems
/// </summary>
public sealed partial class VisitedSystemsControl : UserControl
{
    private readonly VisitedSystemsService _service;
    private VisitedSystemsViewModel? _viewModel;
    private CurrentSystemViewModel? _currentSystemViewModel;

    public VisitedSystemsControl()
    {
        // Get services from DI container
        _service = App.Services.GetRequiredService<VisitedSystemsService>();

        this.InitializeComponent();

        // Create ViewModels early so they can receive events
        _viewModel = new VisitedSystemsViewModel(DispatcherQueue, _service);
        _currentSystemViewModel = new CurrentSystemViewModel(DispatcherQueue, _service);

        // Set DataContext for child controls
        VisitedSystemsList.DataContext = _viewModel;
        CurrentSystemControl.DataContext = _currentSystemViewModel;

    }
}

