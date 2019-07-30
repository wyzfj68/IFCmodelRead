using Microsoft.Win32;
using System;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xbim.Common;
using Xbim.Common.Step21;
using Xbim.Ifc;
using Xbim.IO;
using Xbim.IO.Xml.BsConf;
using Xbim.ModelGeometry.Scene;
using Xbim.Presentation;
using Xbim.Presentation.FederatedModel;

namespace WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public IfcStore ModelStore { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            IfcStore.ModelProviderFactory.UseHeuristicModelProvider();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "IFC Files|*.ifc;*.ifczip;*.ifcxml|Xbim Files|*.xbim";
            dlg.FileOk += (s, args) =>{LoadIFCFile(dlg.FileName);};                   //主要的加载单个文件的核心代码
            dlg.ShowDialog(this);
        }

        private  void LoadIFCFile(string dlgFileName)
        {
            var currentIfcStore = DrawingControl.Model as IfcStore;
            currentIfcStore?.Dispose();
            DrawingControl.Model = null;
            var model = IfcStore.Open(dlgFileName);
            if (model.GeometryStore.IsEmpty)
            {
                // Create the geometry using the XBIM GeometryEngine
                try
                {
                    var context = new Xbim3DModelContext(model);
                    context.CreateContext();
                    // TODO: SaveAs(xbimFile); // so we don't re-process every time
                }
                catch (Exception geomEx)
                {
                    //Logger.LogError(0, geomEx, "Failed to create geometry for {filename}", dlgFileName);
                    MessageBox.Show(geomEx.ToString());
                }
            }
            DrawingControl.Model = model;
        }
        
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var files = OpenFiles();
            IfcStore store= FederationFromDialogbox(files);
            ModelStore = store;
            MessageBox.Show("数据加载完毕");
            this.DrawingControl.Model = store;     //赋值
            //DrawingControl.ReloadModel(DrawingControl3D.ModelRefreshOptions.None);
        }
        private string[] OpenFiles()
        {
            var corefilters = new[] {  "Ifc File (*.ifc)|*.ifc" ,  "Zipped File (*.zip)|*.zip" };

            // Filter files by extension 
            var dlg = new OpenFileDialog
            {
                Title = "选择模型文件",
                Filter = string.Join("|", corefilters),
                CheckFileExists = true,
                Multiselect = true           //允许用户选择多个文件
            };

            var result = dlg.ShowDialog(this);
            if (result == true)
            {
                if (dlg.FileNames != null)
                {
                    // either file name or file names                
                    return dlg.FileNames;
                }

                if (!string.IsNullOrEmpty(dlg.FileName))
                {
                    return new[] { dlg.FileName };
                }
            }
            return new string[0];
        }     //打开一个openfiledialog，并获取选择的文件名

        public static IfcStore FederationFromDialogbox(string[] files)
        {
            IfcStore fedModel = null;
            if (files == null || files.Length == 0)
            {
                return fedModel;
            }
            else
            {
                //use the first filename it's extension to decide which action should happen
                var s = System.IO.Path.GetExtension(files[0]);
                //if (s == null) return ifcStore;
                var firstExtension = s.ToLower();
                if (firstExtension == ".ifc")
                {
                    // create temp file as a placeholder for the temporory xbim file                   
                    fedModel = IfcStore.Create(null, XbimSchemaVersion.Ifc2X3, XbimStoreType.InMemoryModel);
                    using (var txn = fedModel.BeginTransaction())
                    {
                        var project = fedModel.Instances.New<Xbim.Ifc2x3.Kernel.IfcProject>();
                        project.Name = "默认项目名称";
                        project.Initialize(ProjectUnits.SIUnitsUK);
                        txn.Commit();
                    }
                    var informUser = true;
                    for (var i = 0; i < files.Length; i++)
                    {
                        var fileName = files[i];
                        var temporaryReference = new XbimReferencedModelViewModel
                        {
                            Name = fileName,
                            OrganisationName = "机构 " + i,
                            OrganisationRole = "未定义"
                        };

                        var buildRes = false;
                        Exception exception = null;
                        try
                        {
                            buildRes = temporaryReference.TryBuildAndAddTo(fedModel);  //验证所有数据并创建模型。
                        }
                        catch (Exception ex)
                        {
                            //usually an EsentDatabaseSharingViolationException, user needs to close db first
                            exception = ex;
                        }

                        if (buildRes || !informUser) continue;
                        var msg = exception == null ? "" : "\r\nMessage: " + exception.Message;
                        var res = MessageBox.Show(fileName + " couldn't be opened." + msg + "\r\nShow this message again?",
                            "Failed to open a file", MessageBoxButton.YesNoCancel, MessageBoxImage.Error);
                        if (res == MessageBoxResult.No)
                        {
                            informUser = false;
                        }
                        else if (res == MessageBoxResult.Cancel)
                        {
                            fedModel = null;
                        }
                        //return fedModel;
                    }
                }
            }
            return fedModel;
        }
    }



    public class IfcstoreClass
    {
        public IfcStore ModelStore { get; set; }
        private IfcStore IfcStoreMethod()
        {
            string[] files = new string[2] { "C:\\Users\\lenovo-wanchi\\Desktop\\rvt17.ifc", "C:\\Users\\lenovo-wanchi\\Desktop\\site.ifc" };
            IfcStore store = MainWindow.FederationFromDialogbox(files);
            return store;
        }
    }



}
