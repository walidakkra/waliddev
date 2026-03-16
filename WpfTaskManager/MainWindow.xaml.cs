using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfTaskManager;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<TravelModel> _travels = new();
    private readonly DatabaseService _databaseService = new();

    public MainWindow()
    {
        InitializeComponent();
        TravelsDataGrid.ItemsSource = _travels;
        Loaded += MainWindow_OnLoaded;
    }

    private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        var storedTravels = await _databaseService.LoadTravelsAsync();
        foreach (var travel in storedTravels)
        {
            _travels.Add(travel);
        }

        UpdateCostPreview();
        UpdateTotals();
    }

    private async void AddTravelButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!ValidateInputs(out var distance, out var duration))
        {
            return;
        }

        var travel = new TravelModel
        {
            EmployeeName = EmployeeNameTextBox.Text.Trim(),
            Destination = DestinationTextBox.Text.Trim(),
            FromLocation = FromTextBox.Text.Trim(),
            ToLocation = ToTextBox.Text.Trim(),
            Distance = distance,
            Duration = duration,
            TravelType = GetSelectedTravelType()
        };

        _travels.Add(travel);
        await _databaseService.SaveTravelsAsync(_travels);

        UpdateTotals();
        ClearInputFields();
    }

    private async void DeleteTravelButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (TravelsDataGrid.SelectedItem is not TravelModel selectedTravel)
        {
            MessageBox.Show("Please select a travel record to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        _travels.Remove(selectedTravel);
        await _databaseService.SaveTravelsAsync(_travels);
        UpdateTotals();
    }

    private void ClearFieldsButton_OnClick(object sender, RoutedEventArgs e)
    {
        ClearInputFields();
    }

    private void CalculateTotalButton_OnClick(object sender, RoutedEventArgs e)
    {
        UpdateTotals();
    }

    private void TravelTypeComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateCostPreview();
    }

    private bool ValidateInputs(out double distance, out double duration)
    {
        distance = 0;
        duration = 0;

        if (string.IsNullOrWhiteSpace(EmployeeNameTextBox.Text) ||
            string.IsNullOrWhiteSpace(DestinationTextBox.Text) ||
            string.IsNullOrWhiteSpace(FromTextBox.Text) ||
            string.IsNullOrWhiteSpace(ToTextBox.Text))
        {
            MessageBox.Show("Please fill in all required text fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (!double.TryParse(DistanceTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out distance) || distance <= 0)
        {
            MessageBox.Show("Please enter a valid distance greater than 0.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (!double.TryParse(DurationTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out duration) || duration <= 0)
        {
            MessageBox.Show("Please enter a valid duration greater than 0.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        return true;
    }

    private string GetSelectedTravelType()
    {
        return (TravelTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Half Day";
    }

    private void UpdateCostPreview()
    {
        var travelType = GetSelectedTravelType();
        var cost = travelType == "Full Day" ? 80m : 40m;
        CostTextBox.Text = cost.ToString("N2", CultureInfo.InvariantCulture);
    }

    private void UpdateTotals()
    {
        var grandTotal = _travels.Sum(t => t.Cost);
        GrandTotalTextBlock.Text = $"{grandTotal:N2} MAD";

        var employeeName = EmployeeNameTextBox.Text.Trim();
        var employeeTotal = string.IsNullOrWhiteSpace(employeeName)
            ? 0m
            : _travels.Where(t => string.Equals(t.EmployeeName, employeeName, StringComparison.OrdinalIgnoreCase))
                .Sum(t => t.Cost);

        EmployeeTotalTextBlock.Text = $"{employeeTotal:N2} MAD";
    }

    private void ClearInputFields()
    {
        EmployeeNameTextBox.Clear();
        DestinationTextBox.Clear();
        FromTextBox.Clear();
        ToTextBox.Clear();
        DistanceTextBox.Clear();
        DurationTextBox.Clear();
        TravelTypeComboBox.SelectedIndex = 0;
        UpdateCostPreview();
    }
}
