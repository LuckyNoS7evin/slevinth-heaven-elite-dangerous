using SlevinthHeavenEliteDangerous.Services;
using SlevinthHeavenEliteDangerous.Services.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SlevinthHeavenEliteDangerous.ViewModels;

public class BodyCardViewModel : INotifyPropertyChanged
{
    private string _bodyName = string.Empty;
    private int _bodyID;
    private int? _planetParentID;
    private int? _starParentID;
    private bool _wasDiscovered;
    private bool _wasMapped;
    private bool _wasFootfalled;
    private string _planetClass = string.Empty;
    private bool _landable;
    private bool _mapped;
    private string _terraformState = string.Empty;
    private double _distanceFromArrivalLS;
    private string? _signalsFormattedCache;

    public ObservableCollection<SignalCardViewModel> Signals { get; } = [];
    public ObservableCollection<BodyCardViewModel> Children { get; } = [];

    public string BodyName
    {
        get => _bodyName;
        set { if (_bodyName != value) { _bodyName = value; OnPropertyChanged(); } }
    }

    public int BodyID
    {
        get => _bodyID;
        set { if (_bodyID != value) { _bodyID = value; OnPropertyChanged(); } }
    }

    public int? PlanetParentID
    {
        get => _planetParentID;
        set { if (_planetParentID != value) { _planetParentID = value; OnPropertyChanged(); } }
    }

    public int? StarParentID
    {
        get => _starParentID;
        set { if (_starParentID != value) { _starParentID = value; OnPropertyChanged(); } }
    }

    public bool WasDiscovered
    {
        get => _wasDiscovered;
        set { if (_wasDiscovered != value) { _wasDiscovered = value; OnPropertyChanged(); OnPropertyChanged(nameof(DiscoveryStatus)); } }
    }

    public bool WasMapped
    {
        get => _wasMapped;
        set { if (_wasMapped != value) { _wasMapped = value; OnPropertyChanged(); OnPropertyChanged(nameof(DiscoveryStatus)); } }
    }

    public bool WasFootfalled
    {
        get => _wasFootfalled;
        set { if (_wasFootfalled != value) { _wasFootfalled = value; OnPropertyChanged(); OnPropertyChanged(nameof(DiscoveryStatus)); } }
    }

    public string PlanetClass
    {
        get => _planetClass;
        set { if (_planetClass != value) { _planetClass = value; OnPropertyChanged(); } }
    }

    public bool Landable
    {
        get => _landable;
        set { if (_landable != value) { _landable = value; OnPropertyChanged(); OnPropertyChanged(nameof(DiscoveryStatus)); } }
    }

    public bool Mapped
    {
        get => _mapped;
        set { if (_mapped != value) { _mapped = value; OnPropertyChanged(); } }
    }

    public string TerraformState
    {
        get => _terraformState;
        set { if (_terraformState != value) { _terraformState = value; OnPropertyChanged(); } }
    }

    public double DistanceFromArrivalLS
    {
        get => _distanceFromArrivalLS;
        set { if (_distanceFromArrivalLS != value) { _distanceFromArrivalLS = value; OnPropertyChanged(); OnPropertyChanged(nameof(DistanceFromArrivalFormatted)); } }
    }

    public bool HasChildren => Children.Count > 0;
    public bool IsTopLevel => !PlanetParentID.HasValue && !StarParentID.HasValue;
    public bool HasSignals => Signals.Count > 0;
    public int BiologicalSignalCount => BodyValueHelper.GetBiologicalSignalCount(Signals);
    public bool HasBiologicalSignals => BiologicalSignalCount > 0;
    public string BiologicalSignalsBadgeText => $"BIOLOGICAL ({BiologicalSignalCount})";
    public string DistanceFromArrivalFormatted => $"{DistanceFromArrivalLS:F0} Ls";

    public string DiscoveryStatus
    {
        get
        {
            var parts = new List<string>();
            if (!WasDiscovered) parts.Add("First Discovery");
            if (!WasMapped) parts.Add("First Mapped");
            if (Landable && !WasFootfalled) parts.Add("First Footfall");
            return parts.Count > 0 ? string.Join(", ", parts) : "Previously Discovered";
        }
    }

    public string SignalsFormatted
    {
        get
        {
            _signalsFormattedCache ??= Signals.Count == 0
                ? "None"
                : string.Join(", ", Signals.Select(s => $"{s.Type_Localised} ({s.Count})"));
            return _signalsFormattedCache;
        }
    }

    public BodyCardViewModel(BodyCard model)
    {
        _bodyName = model.BodyName;
        _bodyID = model.BodyID;
        _planetParentID = model.PlanetParentID;
        _starParentID = model.StarParentID;
        _wasDiscovered = model.WasDiscovered;
        _wasMapped = model.WasMapped;
        _wasFootfalled = model.WasFootfalled;
        _planetClass = model.PlanetClass;
        _landable = model.Landable;
        _mapped = model.Mapped;
        _terraformState = model.TerraformState;
        _distanceFromArrivalLS = model.DistanceFromArrivalLS;
        foreach (var s in model.Signals)
            Signals.Add(new SignalCardViewModel(s));
    }

    public void ApplyScanData(BodyScanData data)
    {
        BodyName = data.BodyName ?? BodyName;
        WasDiscovered = data.WasDiscovered;
        WasMapped = data.WasMapped;
        WasFootfalled = data.WasFootfalled;
        Landable = data.Landable;
        TerraformState = data.TerraformState ?? string.Empty;
        PlanetClass = data.PlanetClass ?? string.Empty;
        DistanceFromArrivalLS = data.DistanceFromArrivalLS;
        PlanetParentID = data.PlanetParentID;
        StarParentID = data.StarParentID;
    }

    public void ApplySignalData(List<SignalCard> signals)
    {
        Signals.Clear();
        foreach (var s in signals)
            Signals.Add(new SignalCardViewModel(s));
        NotifySignalsChanged();
    }

    public void MarkMapped() => Mapped = true;

    public void NotifySignalsChanged()
    {
        _signalsFormattedCache = null;
        OnPropertyChanged(nameof(SignalsFormatted));
        OnPropertyChanged(nameof(HasSignals));
        OnPropertyChanged(nameof(BiologicalSignalCount));
        OnPropertyChanged(nameof(HasBiologicalSignals));
        OnPropertyChanged(nameof(BiologicalSignalsBadgeText));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
