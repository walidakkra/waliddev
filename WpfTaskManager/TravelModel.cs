using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfTaskManager;

public class TravelModel : INotifyPropertyChanged
{
    private string _employeeName = string.Empty;
    private string _destination = string.Empty;
    private string _fromLocation = string.Empty;
    private string _toLocation = string.Empty;
    private double _distance;
    private double _duration;
    private string _travelType = "Half Day";
    private decimal _cost = 40m;

    public string EmployeeName
    {
        get => _employeeName;
        set => SetField(ref _employeeName, value);
    }

    public string Destination
    {
        get => _destination;
        set => SetField(ref _destination, value);
    }

    public string FromLocation
    {
        get => _fromLocation;
        set => SetField(ref _fromLocation, value);
    }

    public string ToLocation
    {
        get => _toLocation;
        set => SetField(ref _toLocation, value);
    }

    public double Distance
    {
        get => _distance;
        set => SetField(ref _distance, value);
    }

    public double Duration
    {
        get => _duration;
        set => SetField(ref _duration, value);
    }

    public string TravelType
    {
        get => _travelType;
        set
        {
            if (SetField(ref _travelType, value))
            {
                Cost = value == "Full Day" ? 80m : 40m;
            }
        }
    }

    public decimal Cost
    {
        get => _cost;
        set => SetField(ref _cost, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
