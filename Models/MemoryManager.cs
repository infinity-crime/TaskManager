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
        public MemoryBlock? Allocate(ushort sizeTask, TaskItem owner)
        {
            throw new NotImplementedException();
        }

        public void Free(MemoryBlock block)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<MemoryBlock> GetBlocksSnapshot()
        {
            throw new NotImplementedException();
        }

        public void Reset(ushort totalMemory)
        {
            throw new NotImplementedException();
        }
    }
}
