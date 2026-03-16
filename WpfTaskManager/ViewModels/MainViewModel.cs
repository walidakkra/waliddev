using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using WpfTaskManager.Models;
using WpfTaskManager.Services;

namespace WpfTaskManager.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly JsonStorageService _storageService = new();
    private readonly ICollectionView _filteredRecordsView;

    private string _newEmployeeName = string.Empty;
    private string _newNotes = string.Empty;
    private bool _isNewRecordHalfDay;
    private DateTime _newTravelDate = DateTime.Today;
    private string _searchText = string.Empty;
    private decimal _fullDayCost = 80m;
    private decimal _halfDayCost = 40m;
    private TransportRecord? _selectedRecord;

    public ObservableCollection<TransportRecord> Records { get; } = new();

    public ICollectionView FilteredRecords => _filteredRecordsView;

    public string NewEmployeeName
    {
        get => _newEmployeeName;
        set
        {
            _newEmployeeName = value;
            OnPropertyChanged();
            AddRecordCommandInternal.RaiseCanExecuteChanged();
        }
    }

    public string NewNotes
    {
        get => _newNotes;
        set
        {
            _newNotes = value;
            OnPropertyChanged();
        }
    }

    public bool IsNewRecordHalfDay
    {
        get => _isNewRecordHalfDay;
        set
        {
            _isNewRecordHalfDay = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(NewRecordCostPreview));
        }
    }

    public DateTime NewTravelDate
    {
        get => _newTravelDate;
        set
        {
            _newTravelDate = value;
            OnPropertyChanged();
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            OnPropertyChanged();
            _filteredRecordsView.Refresh();
        }
    }

    public decimal FullDayCost
    {
        get => _fullDayCost;
        set
        {
            if (value < 0)
            {
                return;
            }

            _fullDayCost = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(NewRecordCostPreview));
        }
    }

    public decimal HalfDayCost
    {
        get => _halfDayCost;
        set
        {
            if (value < 0)
            {
                return;
            }

            _halfDayCost = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(NewRecordCostPreview));
        }
    }

    public decimal NewRecordCostPreview => IsNewRecordHalfDay ? HalfDayCost : FullDayCost;

    public TransportRecord? SelectedRecord
    {
        get => _selectedRecord;
        set
        {
            _selectedRecord = value;
            OnPropertyChanged();
            DeleteRecordCommandInternal.RaiseCanExecuteChanged();
        }
    }

    public decimal TotalCost => Records.Sum(r => r.Cost);
    public decimal FullDayTotal => Records.Where(r => !r.IsHalfDay).Sum(r => r.Cost);
    public decimal HalfDayTotal => Records.Where(r => r.IsHalfDay).Sum(r => r.Cost);
    public int TotalCount => Records.Count;

    private RelayCommand AddRecordCommandInternal { get; }
    private RelayCommand DeleteRecordCommandInternal { get; }

    public ICommand AddRecordCommand => AddRecordCommandInternal;
    public ICommand DeleteRecordCommand => DeleteRecordCommandInternal;
    public ICommand SaveSettingsCommand { get; }
    public ICommand ClearSearchCommand { get; }
    public ICommand RefreshCommand { get; }

    public MainViewModel()
    {
        _filteredRecordsView = CollectionViewSource.GetDefaultView(Records);
        _filteredRecordsView.Filter = FilterByName;

        AddRecordCommandInternal = new RelayCommand(_ => AddRecord(), _ => !string.IsNullOrWhiteSpace(NewEmployeeName));
        DeleteRecordCommandInternal = new RelayCommand(_ => DeleteSelectedRecord(), _ => SelectedRecord is not null);
        SaveSettingsCommand = new RelayCommand(async _ => await SaveSettingsAsync());
        ClearSearchCommand = new RelayCommand(_ => SearchText = string.Empty);
        RefreshCommand = new RelayCommand(async _ => await LoadAsync());

        Records.CollectionChanged += async (_, _) =>
        {
            RecalculateTotals();
            await SaveAllAsync();
        };
    }

    public async Task InitializeAsync()
    {
        await LoadAsync();
    }

    private bool FilterByName(object recordObj)
    {
        if (recordObj is not TransportRecord record)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return true;
        }

        return record.EmployeeName.Contains(SearchText, StringComparison.CurrentCultureIgnoreCase);
    }

    private async Task LoadAsync()
    {
        var appData = await _storageService.LoadAsync();

        FullDayCost = appData.Settings.FullDayCost <= 0 ? 80m : appData.Settings.FullDayCost;
        HalfDayCost = appData.Settings.HalfDayCost <= 0 ? 40m : appData.Settings.HalfDayCost;

        Records.Clear();
        foreach (var record in appData.Records.OrderByDescending(r => r.TravelDate).ThenByDescending(r => r.CreatedAt))
        {
            Records.Add(record);
        }

        _filteredRecordsView.Refresh();
        RecalculateTotals();
    }

    private async void AddRecord()
    {
        var record = new TransportRecord
        {
            EmployeeName = NewEmployeeName.Trim(),
            Notes = NewNotes.Trim(),
            TravelDate = NewTravelDate.Date,
            IsHalfDay = IsNewRecordHalfDay,
            Cost = IsNewRecordHalfDay ? HalfDayCost : FullDayCost
        };

        Records.Insert(0, record);

        NewEmployeeName = string.Empty;
        NewNotes = string.Empty;
        NewTravelDate = DateTime.Today;
        IsNewRecordHalfDay = false;

        RecalculateTotals();
        await SaveAllAsync();
    }

    private async void DeleteSelectedRecord()
    {
        if (SelectedRecord is null)
        {
            return;
        }

        var current = SelectedRecord;
        Records.Remove(current);
        SelectedRecord = null;

        RecalculateTotals();
        await SaveAllAsync();
    }

    private async Task SaveSettingsAsync()
    {
        if (FullDayCost <= 0 || HalfDayCost <= 0)
        {
            MessageBox.Show("يجب أن تكون الكلفة أكبر من صفر.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        await SaveAllAsync();
        OnPropertyChanged(nameof(NewRecordCostPreview));
        MessageBox.Show("تم حفظ الإعدادات بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async Task SaveAllAsync()
    {
        await _storageService.SaveAsync(new AppData
        {
            Settings = new AppSettings
            {
                FullDayCost = FullDayCost,
                HalfDayCost = HalfDayCost
            },
            Records = Records.ToList()
        });
    }

    private void RecalculateTotals()
    {
        OnPropertyChanged(nameof(TotalCost));
        OnPropertyChanged(nameof(FullDayTotal));
        OnPropertyChanged(nameof(HalfDayTotal));
        OnPropertyChanged(nameof(TotalCount));
    }
}
