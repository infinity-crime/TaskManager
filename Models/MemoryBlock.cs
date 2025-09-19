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
    /// Представляет собой единицу блока памяти (виртуальную).
    /// Упорядочивается в списке по Offset.
    /// </summary>
    public class MemoryBlock : INotifyPropertyChanged
    {
        public Guid Id { get; set; } // идентификатор блока 

        // основные поля
        private ushort _offset;
        private ushort _size;
        private bool _isFree;
        private TaskItem? _owner;

        // вспомогательные поля
        public string? OwnerName => Owner?.TaskName;
        public string BlockStatus => IsFree ? "Free" : $"Using ({OwnerName ?? "unknown"})";

        public ushort Offset // смещение от начала
        {
            get => _offset;
            set
            {
                if (_offset == value)
                    return;

                _offset = value;
                OnPropertyChanged(nameof(Offset));
            }
        }

        public ushort Size // размер блока
        {
            get => _size;
            set
            {
                if(_size == value)
                    return;

                _size = value;
                OnPropertyChanged(nameof(Size));
            }
        }

        public bool IsFree // флаг доступности блока для задачи
        {
            get => _isFree;
            set
            {
                if (_isFree == value)
                    return;
                _isFree = value;

                if(_isFree)
                    Owner = null;

                OnPropertyChanged(nameof(BlockStatus));
                OnPropertyChanged(nameof(IsFree));
            }
        }

        public TaskItem? Owner // ссылка на владельца
        {
            get => _owner;
            set
            {
                if(_owner == value)
                    return;

                _owner = value;
                OnPropertyChanged(nameof(Owner));
                OnPropertyChanged(nameof(OwnerName));
                OnPropertyChanged(nameof(BlockStatus));
            }
        }

        public MemoryBlock(ushort offset, ushort size, bool isFree = true)
        {
            if(offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if(size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            Id = Guid.NewGuid();
            Offset = offset;
            Size = size;
            IsFree = isFree;
        }

        /// <summary>
        /// Разбивает текущий свободный блок на две части: выделяемую (requestedSize) и оставшуюся.
        /// Возвращает кортеж: (allocatedBlock, remainingBlock).
        /// Если requestedSize == Size, allocatedBlock — этот же блок (IsFree станет false), remainingBlock = null.
        /// Бросает исключение, если блок занят или requestedSize invalid.
        /// </summary>
        public (MemoryBlock allocatedBlock, MemoryBlock? remainingBlock) Split(ushort requestedSize, TaskItem owner)
        {
            if (!IsFree)
                throw new InvalidOperationException("Нельзя занять блок, когда он не свободен!");

            if(requestedSize < 0 || requestedSize > Size)
                throw new ArgumentOutOfRangeException(nameof(requestedSize));

            if(requestedSize == Size)
            {
                IsFree = false;
                Owner = owner;

                return (this, null);
            }

            // создадим выделенный блок в начале текущего блока
            var allocatedBlock = new MemoryBlock(Offset, requestedSize, false)
            {
                Owner = owner
            };

            // создадим оставшийся блок после выделенного блока
            var remainingBlock = new MemoryBlock((ushort)(Offset + requestedSize), (ushort)(Size - requestedSize), true);

            return (allocatedBlock, remainingBlock);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
