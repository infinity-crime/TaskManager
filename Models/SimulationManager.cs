using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Models.Interfaces;

namespace TaskManager.Models
{
    public class SimulationManager
    {
        public int CurrentTick { get; private set; }

        private readonly IList<TaskItem> _queue = new List<TaskItem>();
        private readonly IList<TaskItem> _running = new List<TaskItem>();
        private readonly IList<TaskItem> _timeout = new List<TaskItem>();
        private readonly IList<TaskItem> _finished = new List<TaskItem>();

        private readonly IMemoryManager _memoryManager;

        private TimeSpan _tickInterval = TimeSpan.FromSeconds(1);

        public SimulationManager(IMemoryManager memoryManager)
        {
            _memoryManager = memoryManager;
        }

        public IReadOnlyList<TaskItem> GetQueueSnapshot() => _queue.ToArray();
        public IReadOnlyList<TaskItem> GetRunningSnapshot() => _running.ToArray();
        public IReadOnlyList<TaskItem> GetTimedOutSnapshot() => _timeout.ToArray();
        public IReadOnlyList<TaskItem> GetFinishedSnapshot() => _finished.ToArray();

        public bool EnQueue(TaskItem task)
        {
            if(task is null || task.MaxStartTime <= CurrentTick)
                return false;

            task.State = TaskState.Waiting;
            _queue.Add(task);
            return true;
        }

        public void Step()
        {
            CurrentTick++;

            foreach(var task in _running.ToList())
            {
                var finished = task.Tick();
                if(finished)
                {
                    if(task.AllocBlock is MemoryBlock md)
                    {
                        _memoryManager.Free(md);
                        task.AllocBlock = null;
                    }

                    _running.Remove(task);
                    _finished.Add(task);

                    task.State = TaskState.Finished;

                    OnTaskFinished(task);
                }
            }

            foreach(var task in _queue.ToList())
            {
                if(CurrentTick > task.MaxStartTime)
                {
                    _queue.Remove(task);
                    _timeout.Add(task);

                    task.State = TaskState.TimeOut;

                    OnTaskTimedOut(task);
                    continue;
                }

                var block = _memoryManager.Allocate((ushort)task.SizeBytes, task);
                if(block is not null)
                {
                    task.StartTick = CurrentTick;
                    task.AllocBlock = block;

                    _queue.Remove(task);

                    task.State = TaskState.Running;

                    _running.Add(task);

                    OnTaskStarted(task);
                }
            }

            OnTickAdvanced(CurrentTick);
        }

        public event EventHandler<TickEventArgs>? TickAdvanced;
        public event EventHandler<TaskEventArgs>? TaskStarted;
        public event EventHandler<TaskEventArgs>? TaskFinished;
        public event EventHandler<TaskEventArgs>? TaskTimedOut;

        protected virtual void OnTickAdvanced(int tick)
        {
            var handler = TickAdvanced;
            handler?.Invoke(this, new TickEventArgs(tick));
        }

        protected virtual void OnTaskStarted(TaskItem task)
        {
            var handler = TaskStarted;
            handler?.Invoke(this, new TaskEventArgs(task));
        }

        protected virtual void OnTaskFinished(TaskItem task)
        {
            var handler = TaskFinished;
            handler?.Invoke(this, new TaskEventArgs(task));
        }

        protected virtual void OnTaskTimedOut(TaskItem task)
        {
            var handler = TaskTimedOut;
            handler?.Invoke(this, new TaskEventArgs(task));
        }
    }

    public class TickEventArgs : EventArgs
    {
        public int Tick { get; }
        public TickEventArgs(int tick) => Tick = tick;
    }

    public class TaskEventArgs : EventArgs
    {
        public TaskItem Task { get; }
        public TaskEventArgs(TaskItem task) => Task = task;
    }
}
