using System.Windows;
using IFC_Table_View.ViewModels;

namespace IFC_Table_View.View.Windows
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