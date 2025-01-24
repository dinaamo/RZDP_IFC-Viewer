using System.Windows;
using RZDP_IFC_Viewer.ViewModels;

namespace RZDP_IFC_Viewer.View.Windows
{
    /// <summary>
    /// Логика взаимодействия для AddDocumentWindow.xaml
    /// </summary>
    public partial class AddDocumentWindow : Window
    {
        public AddDocumentWindow(string path, Action<ModelDocument> CreateNewIFCDocumentInformation)
        {
            InitializeComponent();
            AddDocumentWindowViewModel addDocumentWindowViewModel = (AddDocumentWindowViewModel)DataContext;
            addDocumentWindowViewModel.IFCPath = path;
            addDocumentWindowViewModel.CreateNewIFCDocumentInformation = CreateNewIFCDocumentInformation;
        }

        public string IFCPath { get; set; }
    }
}