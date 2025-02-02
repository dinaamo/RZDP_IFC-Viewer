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
using RZDP_IFC_Viewer.IFC.ModelItem;

namespace RZDP_IFC_Viewer.View.Windows.EditorWindows
{
    /// <summary>
    /// Логика взаимодействия для FileEditorWindow.xaml
    /// </summary>
    public partial class FileEditorWindow : Window
    {
        public FileEditorWindow()
        {
            InitializeComponent();
        }

        public FileEditorWindow(ModelItemIFCFile modelItemIFCFile)
        {
            InitializeComponent();
            DataContext = modelItemIFCFile;
        }
    }
}
