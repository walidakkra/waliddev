namespace WpfTaskManager.Models;

public class TransportRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime TravelDate { get; set; } = DateTime.Today;
    public bool IsHalfDay { get; set; }
    public decimal Cost { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public string DayTypeLabel => IsHalfDay ? "نصف يوم" : "يوم كامل";
}
