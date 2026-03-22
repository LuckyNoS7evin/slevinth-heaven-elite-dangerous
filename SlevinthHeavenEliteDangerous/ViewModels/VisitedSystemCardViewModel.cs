using SlevinthHeavenEliteDangerous.Services.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SlevinthHeavenEliteDangerous.ViewModels;

public class VisitedSystemCardViewModel : INotifyPropertyChanged
{
    private readonly VisitedSystemCard _model;
    private readonly Dictionary<int, BodyCardViewModel> _bodyVMs = [];
    private int _bodiesCount;
    private bool _hasBodies;

    public ObservableCollection<BodyCardViewModel> Bodies { get; } = [];

    // Pass-through data properties
    public long SystemAddress => _model.SystemAddress;
    public string StarSystem => _model.StarSystem;
    public double[]? StarPos => _model.StarPos;
    public double DistanceFromSol => _model.DistanceFromSol;
    public DateTime FirstVisitTimestamp => _model.FirstVisitTimestamp;
    public DateTime LastVisitTimestamp => _model.LastVisitTimestamp;

    public int BodiesCount
    {
        get => _bodiesCount;
        private set { if (_bodiesCount != value) { _bodiesCount = value; OnPropertyChanged(); } }
    }

    public bool HasBodies
    {
        get => _hasBodies;
        private set { if (_hasBodies != value) { _hasBodies = value; OnPropertyChanged(); } }
    }

    // Formatted properties
    public string StarPosFormatted
    {
        get
        {
            if (StarPos == null || StarPos.Length < 3) return "N/A";
            return $"[{StarPos[0]:F2}, {StarPos[1]:F2}, {StarPos[2]:F2}]";
        }
    }

    public string DistanceFromSolFormatted => $"{DistanceFromSol:F2} Ly";
    public string FirstVisitFormatted => FirstVisitTimestamp.ToString("yyyy-MM-dd HH:mm:ss");
    public string LastVisitFormatted => LastVisitTimestamp.ToString("yyyy-MM-dd HH:mm:ss");

    public VisitedSystemCardViewModel(VisitedSystemCard model)
    {
        _model = model;
        Bodies.CollectionChanged += (_, _) =>
        {
            BodiesCount = Bodies.Count;
            HasBodies = Bodies.Count > 0;
        };
        foreach (var body in model.GetAllBodiesFlat())
            _bodyVMs[body.BodyID] = new BodyCardViewModel(body);
    }

    public BodyCardViewModel? GetBodyVM(int bodyID)
        => _bodyVMs.TryGetValue(bodyID, out var vm) ? vm : null;

    public void RegisterBodyVM(BodyCardViewModel vm)
        => _bodyVMs[vm.BodyID] = vm;

    public void AddBodyToUI(BodyCardViewModel bodyVM)
    {
        if (bodyVM.PlanetParentID.HasValue)
        {
            var parent = GetBodyVM(bodyVM.PlanetParentID.Value);
            if (parent != null) { parent.Children.Add(bodyVM); return; }
        }
        else if (bodyVM.StarParentID.HasValue)
        {
            var parent = GetBodyVM(bodyVM.StarParentID.Value);
            if (parent != null) { parent.Children.Add(bodyVM); return; }
        }
        Bodies.Add(bodyVM);
    }

    public void OrganizeHierarchy()
    {
        Bodies.Clear();
        foreach (var body in _bodyVMs.Values)
            body.Children.Clear();

        foreach (var body in _bodyVMs.Values.OrderBy(b => b.BodyID))
        {
            if (body.PlanetParentID.HasValue)
            {
                var parent = GetBodyVM(body.PlanetParentID.Value);
                if (parent != null) { parent.Children.Add(body); continue; }
            }
            else if (body.StarParentID.HasValue)
            {
                var parent = GetBodyVM(body.StarParentID.Value);
                if (parent != null) { parent.Children.Add(body); continue; }
            }
            Bodies.Add(body);
        }
    }

    /// <summary>
    /// Update the underlying model's LastVisitTimestamp and fire property change notifications.
    /// Called from UI thread after a revisit.
    /// </summary>
    public void UpdateLastVisit(DateTime timestamp)
    {
        _model.LastVisitTimestamp = timestamp;
        OnPropertyChanged(nameof(LastVisitTimestamp));
        OnPropertyChanged(nameof(LastVisitFormatted));
    }

    /// <summary>
    /// Fire property change notifications for LastVisit without updating the model.
    /// Used when the model was already updated silently (e.g. off UI thread).
    /// </summary>
    public void NotifyLastVisitChanged()
    {
        OnPropertyChanged(nameof(LastVisitTimestamp));
        OnPropertyChanged(nameof(LastVisitFormatted));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
