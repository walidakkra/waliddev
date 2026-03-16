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
        _dataFilePath = Path.Combine(appFolder, "tasks.json");
    }

    public async Task<List<TaskItem>> LoadAsync()
    {
        if (!File.Exists(_dataFilePath))
        {
            return new List<TaskItem>();
        }

        await using var stream = File.OpenRead(_dataFilePath);
        var tasks = await JsonSerializer.DeserializeAsync<List<TaskItem>>(stream);
        return tasks ?? new List<TaskItem>();
    }

    public async Task SaveAsync(IEnumerable<TaskItem> tasks)
    {
        await using var stream = File.Create(_dataFilePath);
        await JsonSerializer.SerializeAsync(stream, tasks, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
}
