using SlevinthHeavenEliteDangerous.Services.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SlevinthHeavenEliteDangerous.ViewModels;

public class SignalCardViewModel : INotifyPropertyChanged
{
    private string _typeLocalised = string.Empty;
    private int _count;

    public string Type_Localised
    {
        get => _typeLocalised;
        set { if (_typeLocalised != value) { _typeLocalised = value; OnPropertyChanged(); } }
    }

    public int Count
    {
        get => _count;
        set { if (_count != value) { _count = value; OnPropertyChanged(); } }
    }

    public SignalCardViewModel(SignalCard model)
    {
        _typeLocalised = model.Type_Localised;
        _count = model.Count;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
