using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using TaskManager.Common.Commands;
using TaskManager.Models;

namespace TaskManager.ViewModels
{
    public class MainViewModel : BaseViewModel, IDisposable
    {
        private readonly SimulationManager _sim;
        private readonly DispatcherTimer? _timer;

        public ObservableCollection<TaskItem> QueueTasks { get; } = new ObservableCollection<TaskItem>();
        public ObservableCollection<TaskItem> RunningTasks { get; } = new ObservableCollection<TaskItem>();
        public ObservableCollection<TaskItem> FinishedTasks { get; } = new ObservableCollection<TaskItem>();
        public ObservableCollection<TaskItem> TimedOutTasks { get; } = new ObservableCollection<TaskItem>();

        private string _newTaskName = string.Empty;
        public string NewTaskName
        {
            get => _newTaskName;
            set { _newTaskName = value; OnPropertyChanged(); }
        }

        private string _newTaskSize = "100";
        public string NewTaskSize
        {
            get => _newTaskSize;
            set { _newTaskSize = value; OnPropertyChanged(); }
        }

        private string _newTaskMaxStart = "5";
        public string NewTaskMaxStart
        {
            get => _newTaskMaxStart;
            set { _newTaskMaxStart = value; OnPropertyChanged(); }
        }

        private string _newTaskDuration = "5";
        public string NewTaskDuration
        {
            get => _newTaskDuration;
            set { _newTaskDuration = value; OnPropertyChanged(); }
        }

        public ICommand EnQueueCommand { get; }

        public int CurrentTick => _sim.CurrentTick;

        public MainViewModel(SimulationManager sim)
        {
            _sim = sim ?? throw new ArgumentNullException(nameof(sim));

            _sim.TaskStarted += OnTaskStarted;
            _sim.TaskFinished += OnTaskFinished;
            _sim.TaskTimedOut += OnTaskTimedOut;
            _sim.TickAdvanced += OnTickAdvanced;

            _timer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal, (s, e) => _sim.Step(),
                Application.Current.Dispatcher);

            EnQueueCommand = new RelayCommand(_ =>
            {
                AddTask();
            }, _ => true);
        }

        #region Event Helpers

        private void OnTaskStarted(object sender, TaskEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                QueueRemove(e.Task);
                RunningTasks.Add(e.Task);
                OnPropertyChanged(nameof(CurrentTick));
            });
        }

        private void OnTaskFinished(object sender, TaskEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                RunningRemove(e.Task);
                FinishedTasks.Add(e.Task);
                OnPropertyChanged(nameof(CurrentTick));
            });
        }

        private void OnTaskTimedOut(object sender, TaskEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                QueueRemove(e.Task);
                TimedOutTasks.Add(e.Task);
                OnPropertyChanged(nameof(CurrentTick));
            });
        }

        private void OnTickAdvanced(object sender, TickEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                OnPropertyChanged(nameof(CurrentTick));
            });
        }

        #endregion


        #region Helpers

        private void QueueRemove(TaskItem t)
        {
            if(QueueTasks.Contains(t))
                QueueTasks.Remove(t);
        }

        private void RunningRemove(TaskItem t)
        {
            if(RunningTasks.Contains(t))
                RunningTasks.Remove(t);
        }

        private void ResetCollectionsFromSnapshots()
        {
            QueueTasks.Clear();
            RunningTasks.Clear();
            TimedOutTasks.Clear();
            FinishedTasks.Clear();

            foreach(var t in _sim.GetQueueSnapshot()) QueueTasks.Add(t);
            foreach(var t in _sim.GetRunningSnapshot()) RunningTasks.Add(t);
            foreach(var t in _sim.GetFinishedSnapshot()) FinishedTasks.Add(t);
            foreach(var t in _sim.GetTimedOutSnapshot()) TimedOutTasks.Add(t);

            OnPropertyChanged(nameof(CurrentTick));
        }

        #endregion

        private void AddTask()
        {
            if (string.IsNullOrWhiteSpace(NewTaskName))
            {
                MessageBox.Show("Введите имя задачи.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ushort.TryParse(NewTaskSize, out var size) || size < 1)
            {
                MessageBox.Show("Неверное значение размера (целое положительное число).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(NewTaskMaxStart, out var maxStart) || maxStart < 0)
            {
                MessageBox.Show("Неверное значение MaxStartTime (целое неотрицательное).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(NewTaskDuration, out var duration) || duration <= 0)
            {
                MessageBox.Show("Неверное значение длительности (целое положительное).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var task = new TaskItem(NewTaskName, size, maxStart, duration);

                if(_sim.EnQueue(task))
                {
                    QueueTasks.Add(task);
                }
                else
                {
                    TimedOutTasks.Add(task);
                }
               
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void Dispose()
        {
            _timer?.Stop();

            _sim.TaskStarted -= OnTaskStarted;
            _sim.TaskFinished -= OnTaskFinished;
            _sim.TaskTimedOut -= OnTaskTimedOut;
            _sim.TickAdvanced -= OnTickAdvanced;
        }
    }
}
