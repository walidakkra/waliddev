using System.IO;
using System.Text.Json;
using WpfTaskManager.Models;

namespace WpfTaskManager.Services;

public class JsonStorageService
{
    private readonly string _dataFilePath;

    public JsonStorageService()
    {
        var appFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WpfTaskManager");
        Directory.CreateDirectory(appFolder);
        _dataFilePath = Path.Combine(appFolder, "transport-data.json");
    }

    public async Task<AppData> LoadAsync()
    {
        if (!File.Exists(_dataFilePath))
        {
            return new AppData();
        }

        await using var stream = File.OpenRead(_dataFilePath);
        var appData = await JsonSerializer.DeserializeAsync<AppData>(stream);
        return appData ?? new AppData();
    }

    public async Task SaveAsync(AppData appData)
    {
        await using var stream = File.Create(_dataFilePath);
        await JsonSerializer.SerializeAsync(stream, appData, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
}
