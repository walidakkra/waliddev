using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using WpfTaskManager.Models;
using WpfTaskManager.Services;

namespace WpfTaskManager.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly JsonStorageService _storageService = new();
    private string _newTaskTitle = string.Empty;
    private string _newTaskNotes = string.Empty;
    private TaskItem? _selectedTask;

    public ObservableCollection<TaskItem> Tasks { get; } = new();

    public string NewTaskTitle
    {
        get => _newTaskTitle;
        set
        {
            _newTaskTitle = value;
            OnPropertyChanged();
            AddTaskCommandInternal.RaiseCanExecuteChanged();
        }
    }

    public string NewTaskNotes
    {
        get => _newTaskNotes;
        set
        {
            _newTaskNotes = value;
            OnPropertyChanged();
        }
    }

    public TaskItem? SelectedTask
    {
        get => _selectedTask;
        set
        {
            _selectedTask = value;
            OnPropertyChanged();
            ToggleTaskCommandInternal.RaiseCanExecuteChanged();
            DeleteTaskCommandInternal.RaiseCanExecuteChanged();
        }
    }

    public int TotalCount => Tasks.Count;
    public int CompletedCount => Tasks.Count(t => t.IsCompleted);
    public int PendingCount => Tasks.Count(t => !t.IsCompleted);

    private RelayCommand AddTaskCommandInternal { get; }
    private RelayCommand ToggleTaskCommandInternal { get; }
    private RelayCommand DeleteTaskCommandInternal { get; }

    public ICommand AddTaskCommand => AddTaskCommandInternal;
    public ICommand ToggleTaskCommand => ToggleTaskCommandInternal;
    public ICommand DeleteTaskCommand => DeleteTaskCommandInternal;
    public ICommand ClearCompletedCommand { get; }
    public ICommand RefreshCommand { get; }

    public MainViewModel()
    {
        AddTaskCommandInternal = new RelayCommand(_ => AddTask(), _ => !string.IsNullOrWhiteSpace(NewTaskTitle));
        ToggleTaskCommandInternal = new RelayCommand(_ => ToggleSelectedTask(), _ => SelectedTask is not null);
        DeleteTaskCommandInternal = new RelayCommand(_ => DeleteSelectedTask(), _ => SelectedTask is not null);
        ClearCompletedCommand = new RelayCommand(_ => ClearCompleted());
        RefreshCommand = new RelayCommand(async _ => await LoadAsync());

        Tasks.CollectionChanged += async (_, _) =>
        {
            RecalculateCounters();
            await _storageService.SaveAsync(Tasks);
        };
    }

    public async Task InitializeAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var loadedTasks = await _storageService.LoadAsync();
        Tasks.Clear();
        foreach (var task in loadedTasks.OrderByDescending(t => t.CreatedAt))
        {
            Tasks.Add(task);
        }

        RecalculateCounters();
    }

    private async void AddTask()
    {
        var task = new TaskItem
        {
            Title = NewTaskTitle.Trim(),
            Notes = NewTaskNotes.Trim()
        };

        Tasks.Insert(0, task);

        NewTaskTitle = string.Empty;
        NewTaskNotes = string.Empty;
        RecalculateCounters();
        await _storageService.SaveAsync(Tasks);
    }

    private async void ToggleSelectedTask()
    {
        if (SelectedTask is null)
        {
            return;
        }

        SelectedTask.IsCompleted = !SelectedTask.IsCompleted;
        SelectedTask.CompletedAt = SelectedTask.IsCompleted ? DateTime.Now : null;
        OnPropertyChanged(nameof(Tasks));
        RecalculateCounters();
        await _storageService.SaveAsync(Tasks);
    }

    private async void DeleteSelectedTask()
    {
        if (SelectedTask is null)
        {
            return;
        }

        var current = SelectedTask;
        Tasks.Remove(current);
        SelectedTask = null;
        RecalculateCounters();
        await _storageService.SaveAsync(Tasks);
    }

    private async void ClearCompleted()
    {
        var completed = Tasks.Where(t => t.IsCompleted).ToList();
        foreach (var item in completed)
        {
            Tasks.Remove(item);
        }

        SelectedTask = null;
        RecalculateCounters();
        await _storageService.SaveAsync(Tasks);
        MessageBox.Show("تم حذف جميع المهام المكتملة.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void RecalculateCounters()
    {
        OnPropertyChanged(nameof(TotalCount));
        OnPropertyChanged(nameof(CompletedCount));
        OnPropertyChanged(nameof(PendingCount));
    }
}
