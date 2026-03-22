using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SlevinthHeavenEliteDangerous.Data;

/// <summary>
/// Generic service for persisting data to local storage using System.IO.
/// </summary>
public partial class DataService<T> : IDisposable where T : class
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        WriteIndented = true
    };

    public DataService(string fileName)
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "SlevinthHeavenEliteDangerous");

        Directory.CreateDirectory(folder);
        _filePath = Path.Combine(folder, fileName);
    }

    public async Task SaveDataAsync(T data)
    {
        await _lock.WaitAsync();
        try
        {
            var json = JsonSerializer.Serialize(data, _serializerOptions);
            await File.WriteAllTextAsync(_filePath, json);
            System.Diagnostics.Debug.WriteLine($"{typeof(T).Name} data saved to {_filePath}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save {typeof(T).Name} data: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<T?> LoadDataAsync()
    {
        await _lock.WaitAsync();
        try
        {
            if (!File.Exists(_filePath))
            {
                System.Diagnostics.Debug.WriteLine($"No existing {typeof(T).Name} data found at {_filePath}");
                return null;
            }

            var json = await File.ReadAllTextAsync(_filePath);
            var data = JsonSerializer.Deserialize<T>(json);
            System.Diagnostics.Debug.WriteLine($"{typeof(T).Name} data loaded from {_filePath}");
            return data;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load {typeof(T).Name} data: {ex.Message}");
            return null;
        }
        finally
        {
            _lock.Release();
        }
    }

    public Task<bool> DataExistsAsync() => Task.FromResult(File.Exists(_filePath));

    public async Task DeleteDataAsync()
    {
        await _lock.WaitAsync();
        try
        {
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
                System.Diagnostics.Debug.WriteLine($"{typeof(T).Name} data deleted from {_filePath}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to delete {typeof(T).Name} data: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    public void Dispose()
    {
        _lock?.Dispose();
        GC.SuppressFinalize(this);
    }
}
