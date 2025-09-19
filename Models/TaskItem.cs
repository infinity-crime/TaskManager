using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Models
{
    /// <summary>
    /// Представляет модель задачи.
    /// Допускает исключения при невалидных данных:
    /// ArgumentNullException; ArgumentOutOfRangeException.
    /// </summary>
    public class TaskItem : INotifyPropertyChanged
    {
        // основные поля
        public string TaskName { get; } = string.Empty;
        public long SizeBytes { get; }
        public int MaxStartTime { get; }
        public int InitialDuration { get; }

        // вспомогательные поля
        private int? _startTick; // фактическое время начала выполнения
        private int _remainingDuration; // оставщееся время выполнения задачи (в тиках), изменяется при каждом тике
        private object? _allocBlock; // ссылка на занятый блок (nullable)

        // состояние задачи (один список - одно состояние)
        private TaskState _state = TaskState.Waiting;

        public int? StartTick
        {
            get => _startTick;
            set
            {
                _startTick = value;
                OnPropertyChanged(nameof(StartTick));
            }
        }

        public int RemainingDuration
        {
            get => _remainingDuration;
            set
            {
                _remainingDuration = value;
                OnPropertyChanged(nameof(RemainingDuration));
            }
        }

        public TaskState State
        {
            get => _state;
            set
            {
                if(_state == value)
                    return;

                _state = value;
                OnPropertyChanged(nameof(State));
            }
        }

        public object? AllocBlock
        {
            get => _allocBlock;
            set
            {
                if(_allocBlock == value)
                    return;

                _allocBlock = value;
                OnPropertyChanged(nameof(AllocBlock));
            }
        }

        public TaskItem(string name, ushort size, int maxStartTick, int durationTicks)
        {
            if(string.IsNullOrWhiteSpace(name)) 
                throw new ArgumentNullException(nameof(name));

            if(size <= 0) 
                throw new ArgumentOutOfRangeException(nameof(size));

            if (maxStartTick <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxStartTick));

            if (durationTicks <= 0)
                throw new ArgumentOutOfRangeException(nameof(durationTicks));

            TaskName = name;
            SizeBytes = size;
            MaxStartTime = maxStartTick;
            InitialDuration = durationTicks;
            RemainingDuration = durationTicks;
        }

        public bool StartTask(int tick, object? allocBlock = null)
        {
            if (State != TaskState.Waiting || allocBlock is null)
                return false;

            StartTick = tick;
            AllocBlock = allocBlock;
            State = TaskState.Running;

            return true;
        }

        public bool Tick()
        {
            if(State != TaskState.Running)
                return false;

            RemainingDuration = Math.Max(0, RemainingDuration - 1);

            if(RemainingDuration == 0)
            {
                State = TaskState.Finished;
                return true;
            }

            return false;
        }

        public void MarkTimeOut()
        {
            if (State != TaskState.Waiting)
                return;

            State = TaskState.TimeOut;
        }

        public override string ToString() =>
            $"{TaskName}: Size - {SizeBytes} Bytes; Initial duration - {InitialDuration} t; " +
            $"Remaining duration - {RemainingDuration} t";

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }

    public enum TaskState : byte
    {
        Waiting,
        Running,
        TimeOut,
        Finished
    }
}
