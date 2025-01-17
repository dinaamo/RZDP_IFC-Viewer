using System.Windows;
using IFC_Table_View.IFC.ModelItem;
using IFC_Table_View.ViewModels;

namespace IFC_Viewer.View.Windows
{
    /// <summary>
    /// Логика взаимодействия для SelectReferenceObjectWindow.xaml
    /// </summary>
    public partial class SelectReferenceObjectWindow : Window
    {
        public SelectReferenceObjectWindow(List<ModelItemIFCObject> collectionObject, List<BaseModelReferenceIFC> collectionModelReference, Action<List<ModelItemIFCObject>, List<BaseModelReferenceIFC>> createNewModelReference)
        {
            DataContext = new SelectReferenceObjectWindowViewModel(collectionObject, collectionModelReference, createNewModelReference);
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}