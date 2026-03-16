using System.IO;
using System.Text.Json;

namespace WpfTaskManager;

public class DatabaseService
{
    private readonly string _filePath;

    public DatabaseService()
    {
        var appFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TravelCostCalculator");

        Directory.CreateDirectory(appFolder);
        _filePath = Path.Combine(appFolder, "travels.json");
    }

    public async Task<List<TravelModel>> LoadTravelsAsync()
    {
        if (!File.Exists(_filePath))
        {
            return new List<TravelModel>();
        }

        await using var stream = File.OpenRead(_filePath);
        var records = await JsonSerializer.DeserializeAsync<List<TravelModel>>(stream);
        return records ?? new List<TravelModel>();
    }

    public async Task SaveTravelsAsync(IEnumerable<TravelModel> travels)
    {
        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, travels, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
}
