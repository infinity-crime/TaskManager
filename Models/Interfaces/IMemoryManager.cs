using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Models.Interfaces
{
    public interface IMemoryManager
    {
        MemoryBlock? Allocate(ushort sizeTask, TaskItem owner);
        bool Free(MemoryBlock block);
        IReadOnlyList<MemoryBlock> GetBlocksSnapshot();
        void Reset(ushort totalMemory);
    }
}
