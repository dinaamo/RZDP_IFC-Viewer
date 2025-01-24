using System.Windows;
using RZDP_IFC_Viewer.IFC.ModelItem;
using RZDP_IFC_Viewer.ViewModels;
using RZDP_IFC_Viewer.View.Windows;

namespace IFC_Viewer.View.Windows
{
    /// <summary>
    /// Логика взаимодействия для SelectReferenceObjectWindow.xaml
    /// </summary>
    public partial class SelectReferenceObjectWindow : Window
    {
        private static SelectReferenceObjectWindow instance;

        public static void CreateSelectReferenceObjectWindow(List<ModelItemIFCObject> collectionObject, List<BaseModelReferenceIFC> collectionModelReference, Action<List<ModelItemIFCObject>, List<BaseModelReferenceIFC>> createNewModelReference)
        {
            if (instance == null)
            {
                instance = new SelectReferenceObjectWindow(collectionObject, collectionModelReference, createNewModelReference);
                instance.Show();
            }
            else
            {
                return;
            }
        }

        //List<ModelItemIFCObject> collectionObject;
        private SelectReferenceObjectWindow(List<ModelItemIFCObject> collectionObject, List<BaseModelReferenceIFC> collectionModelReference, Action<List<ModelItemIFCObject>, List<BaseModelReferenceIFC>> createNewModelReference)
        {
            //this.collectionObject = collectionObject;
            DataContext = new SelectReferenceObjectWindowViewModel(collectionObject, collectionModelReference, createNewModelReference);
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //foreach (var item in collectionObject)
            //{
            //    item.OnPropertyChanged("CollectionPropertySet");
            //}

            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            instance = null;
        }
    }
}