using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Xbim.Ifc;

namespace WPF
{
    
    /// <summary>
    /// DatatemplateDemo.xaml 的交互逻辑
    /// </summary>
    public partial class DatatemplateDemo : Window
    {
        private readonly MainViewModel _viewModel = new MainViewModel();
        public DatatemplateDemo()
        {
            InitializeComponent();
            string[] files = new string[2] { @"C:\Users\lenovo-wanchi\Desktop\rvt17.ifc", @"C:\Users\lenovo-wanchi\Desktop\site.ifc" };
            IfcStore store = MainWindow. FederationFromDialogbox(files);
            _viewModel.ModelStore = store;
            this.DataContext = _viewModel;
            usercontrol.DataContext = _viewModel;
        }
    }
}
