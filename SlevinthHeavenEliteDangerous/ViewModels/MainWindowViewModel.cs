using SlevinthHeavenEliteDangerous.Services;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SlevinthHeavenEliteDangerous.ViewModels;

/// <summary>
/// ViewModel for MainWindow - manages UI state based on journal events
/// </summary>
public sealed class MainWindowViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly JournalEventService _journalEventService;
    private readonly DispatcherQueue _dispatcherQueue;
    private bool _isInitializing = false;
    private string _initializationMessage = "Initializing...";
    private string _initializationProgress = string.Empty;
    private bool _showProgress = false;
    private bool _showLoginPrompt = false;

    public ObservableCollection<string> UnknownEvents { get; } = [];

    public bool IsInitializing
    {
        get => _isInitializing;
        set
        {
            if (_isInitializing != value)
            {
                _isInitializing = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsInitializingVisibility));
            }
        }
    }

    public Visibility IsInitializingVisibility => IsInitializing ? Visibility.Visible : Visibility.Collapsed;

    public string InitializationMessage
    {
        get => _initializationMessage;
        set
        {
            if (_initializationMessage != value)
            {
                _initializationMessage = value;
                OnPropertyChanged();
            }
        }
    }

    public string InitializationProgress
    {
        get => _initializationProgress;
        set
        {
            if (_initializationProgress != value)
            {
                _initializationProgress = value;
                OnPropertyChanged();
            }
        }
    }

    public bool ShowProgress
    {
        get => _showProgress;
        set
        {
            if (_showProgress != value)
            {
                _showProgress = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowProgressVisibility));
            }
        }
    }

    public Visibility ShowProgressVisibility => ShowProgress ? Visibility.Visible : Visibility.Collapsed;

    public bool ShowLoginPrompt
    {
        get => _showLoginPrompt;
        set
        {
            if (_showLoginPrompt != value)
            {
                _showLoginPrompt = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowLoginPromptVisibility));
            }
        }
    }

    public Visibility ShowLoginPromptVisibility => ShowLoginPrompt ? Visibility.Visible : Visibility.Collapsed;

    private bool _showUpdateBanner = false;
    private string _updateMessage = string.Empty;

    public bool ShowUpdateBanner
    {
        get => _showUpdateBanner;
        set
        {
            if (_showUpdateBanner != value)
            {
                _showUpdateBanner = value;
                OnPropertyChanged();
            }
        }
    }

    public string UpdateMessage
    {
        get => _updateMessage;
        set
        {
            if (_updateMessage != value)
            {
                _updateMessage = value;
                OnPropertyChanged();
            }
        }
    }

    public void ShowUpdateAvailable(string latestVersion, string? releaseNotesUrl)
    {
        var message = $"Version {latestVersion} is available.";
        if (!string.IsNullOrWhiteSpace(releaseNotesUrl))
            message += $" Release notes: {releaseNotesUrl}";

        _dispatcherQueue.TryEnqueue(() =>
        {
            UpdateMessage = message;
            ShowUpdateBanner = true;
        });
    }

    private bool _showVersionBlock = false;
    private string _versionBlockMessage = string.Empty;
    private string _versionDownloadUrl = string.Empty;

    public bool ShowVersionBlock
    {
        get => _showVersionBlock;
        set
        {
            if (_showVersionBlock != value)
            {
                _showVersionBlock = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowVersionBlockVisibility));
            }
        }
    }

    public Visibility ShowVersionBlockVisibility => ShowVersionBlock ? Visibility.Visible : Visibility.Collapsed;

    public string VersionBlockMessage
    {
        get => _versionBlockMessage;
        set
        {
            if (_versionBlockMessage != value)
            {
                _versionBlockMessage = value;
                OnPropertyChanged();
            }
        }
    }

    public string VersionDownloadUrl
    {
        get => _versionDownloadUrl;
        set
        {
            if (_versionDownloadUrl != value)
            {
                _versionDownloadUrl = value;
                OnPropertyChanged();
            }
        }
    }

    public void BlockForUpdate(string latestVersion, string? downloadUrl)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            VersionBlockMessage = $"Version {latestVersion} is required. Please download the latest version to continue.";
            VersionDownloadUrl = downloadUrl ?? string.Empty;
            ShowVersionBlock = true;
        });
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public MainWindowViewModel(
        DispatcherQueue dispatcherQueue,
        JournalEventService journalEventService)
    {
        _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));
        _journalEventService = journalEventService ?? throw new ArgumentNullException(nameof(journalEventService));

        // Subscribe to journal service events
        _journalEventService.UnknownEventReceived += OnUnknownEventReceived;
        _journalEventService.ErrorOccurred += OnErrorOccurred;
    }

    public void SetLoginPrompt(bool show)
    {
        _dispatcherQueue.TryEnqueue(() => ShowLoginPrompt = show);
    }

    public void SetInitializing(bool isInitializing, string message = "Initializing...", string progress = "")
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            IsInitializing = isInitializing;
            InitializationMessage = message;
            InitializationProgress = progress;
            ShowProgress = !string.IsNullOrEmpty(progress);
        });
    }

    public void Dispose()
    {
        _journalEventService.UnknownEventReceived -= OnUnknownEventReceived;
        _journalEventService.ErrorOccurred -= OnErrorOccurred;
    }

    private void OnUnknownEventReceived(object? sender, UnknownEventReceivedEventArgs e)
    {
        // Update UI on the dispatcher thread
        _dispatcherQueue.TryEnqueue(() =>
        {
            UnknownEvents.Add(e.Message);
        });
    }

    private void OnErrorOccurred(object? sender, ErrorOccurredEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"Error in {e.Context}: {e.Exception.Message}");

        // Optionally add errors to a collection for UI display
        // _dispatcherQueue.TryEnqueue(() =>
        // {
        //     Errors.Add($"{e.Context}: {e.Exception.Message}");
        // });
    }
}
