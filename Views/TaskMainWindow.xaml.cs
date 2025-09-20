using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TaskManager.Models;
using TaskManager.ViewModels;

namespace TaskManager.Views
{
    public partial class TaskMainWindow : Window
    {
        private MainViewModel _vm;

        public TaskMainWindow()
        {
            InitializeComponent();

            var memoryManager = new MemoryManager();
            var sim = new SimulationManager(memoryManager);
            _vm = new MainViewModel(sim);
            DataContext = _vm;

            this.Closed += (_, __) => _vm.Dispose();
        }
    }
}
