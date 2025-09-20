using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Models.Interfaces;

namespace TaskManager.Models
{
    public class MemoryManager : IMemoryManager
    {
        public ushort TotalMemory { get; private set; } = 1000;

        private readonly List<MemoryBlock> _blocks;

        public MemoryManager()
        {
            _blocks = new List<MemoryBlock> { new MemoryBlock(0, TotalMemory, true) };
        }

        public MemoryBlock? Allocate(ushort sizeTask, TaskItem owner)
        {
            for(int i = 0; i < _blocks.Count; ++i)
            {
                var block = _blocks[i];
                if (!block.IsFree)
                    continue;

                if (block.Size < sizeTask)
                    continue;

                var (allocated, remaining) = block.Split(sizeTask, owner);

                if (remaining is null)
                {
                    _blocks[i] = allocated;
                }
                else
                {
                    _blocks[i] = allocated;
                    _blocks.Insert(i + 1, remaining);
                }

                OnMemoryChanged(MemoryChangeType.Allocated, allocated);
                return allocated;
            }

            return null;
        }

        public bool Free(MemoryBlock block)
        {
            if(block is null)
                throw new ArgumentNullException(nameof(block));

            int indx = _blocks.IndexOf(block);
            if (indx == -1)
                return false;

            var mb = _blocks[indx];
            if (mb.IsFree)
                return false;

            mb.IsFree = true;

            OnMemoryChanged(MemoryChangeType.Freed, mb);
            return true;
        }

        public IReadOnlyList<MemoryBlock> GetBlocksSnapshot()
        {
            return _blocks.AsReadOnly();
        }

        public void Reset(ushort totalMemory)
        {
            if(totalMemory < 1)
                throw new ArgumentOutOfRangeException(nameof(totalMemory));

            TotalMemory = totalMemory;
            _blocks.Clear();
            _blocks.Add(new MemoryBlock(0, TotalMemory, true));
        }

        #region Event
        public event EventHandler<MemoryChangedEventArgs> MemoryChanged;

        protected virtual void OnMemoryChanged(MemoryChangeType type, MemoryBlock? affectedBlock = null)
        {
            IReadOnlyList<MemoryBlock> snapshot;
            snapshot = GetBlocksSnapshot();

            MemoryChanged?.Invoke(this, new MemoryChangedEventArgs(type, affectedBlock, snapshot));
        }
        #endregion
    }

    public class MemoryChangedEventArgs : EventArgs
    {
        public MemoryChangeType ChangeType { get; }
        public MemoryBlock? AffectedBlock { get; }
        public IReadOnlyList<MemoryBlock> Snapshot { get; }

        public MemoryChangedEventArgs(MemoryChangeType type, MemoryBlock? block, IReadOnlyList<MemoryBlock> snap)
        {
            ChangeType = type;
            AffectedBlock = block;
            Snapshot = snap;
        }
    }

    public enum MemoryChangeType : byte
    {
        Allocated,
        Freed,
        Reset
    }
}
