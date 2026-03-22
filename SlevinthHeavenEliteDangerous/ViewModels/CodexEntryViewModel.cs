using SlevinthHeavenEliteDangerous.Services.Models;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SlevinthHeavenEliteDangerous.ViewModels;

/// <summary>
/// ViewModel for a single codex first-discovery entry.
/// </summary>
public class CodexEntryViewModel : INotifyPropertyChanged
{
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string SubCategory { get; init; } = string.Empty;
    public string Region { get; init; } = string.Empty;
    public string System { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public long? VoucherAmount { get; init; }

    public string TimestampFormatted => Timestamp.ToString("yyyy-MM-dd HH:mm");
    public string VoucherFormatted => VoucherAmount.HasValue ? $"{VoucherAmount.Value:N0} CR" : string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public static CodexEntryViewModel FromModel(CodexEntryModel m) => new()
    {
        Name          = m.Name,
        Category      = m.Category,
        SubCategory   = m.SubCategory,
        Region        = m.Region,
        System        = m.System,
        Timestamp     = m.Timestamp,
        VoucherAmount = m.VoucherAmount
    };
}
