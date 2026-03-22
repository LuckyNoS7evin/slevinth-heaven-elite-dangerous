using SlevinthHeavenEliteDangerous.Services;
using SlevinthHeavenEliteDangerous.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SlevinthHeavenEliteDangerous.Controls;

/// <summary>
/// UserControl for general statistics and information
/// </summary>
public sealed partial class GeneralControl : UserControl
{
    private readonly FSDService _service;
    private readonly RankService _rankService;
    private GeneralViewModel? _viewModel;
    private RanksViewModel? _ranksViewModel;

    public GeneralControl()
    {
        // Get services from DI container
        _service = App.Services.GetRequiredService<FSDService>();
        _rankService = App.Services.GetRequiredService<RankService>();

        this.InitializeComponent();

        // Create ViewModels early so they can receive events
        _viewModel = new GeneralViewModel(DispatcherQueue, _service);
        _ranksViewModel = new RanksViewModel(DispatcherQueue, _rankService);

        // Set DataContext for sub-controls
        FSDTimingCard.DataContext = _viewModel.FSDTiming;
        FSDTargetCard.DataContext = _viewModel.FSDTarget;
        RanksControl.DataContext = _ranksViewModel;

    }
}
