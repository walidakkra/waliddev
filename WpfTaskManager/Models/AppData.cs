namespace WpfTaskManager.Models;

public class AppData
{
    public List<TransportRecord> Records { get; set; } = new();
    public AppSettings Settings { get; set; } = new();
}
