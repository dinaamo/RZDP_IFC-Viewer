using System.Windows;
using IFC_Table_View.IFC.ModelItem;

namespace RZDP_IFC_Viewer.View.Windows
{
    /// <summary>
    /// Логика взаимодействия для EditorWindow.xaml
    /// </summary>
    public partial class EditorWindow : Window
    {
        private static EditorWindow instance;

        public static void CreateWindowSearch(ModelItemIFCObject ModelObject)
        {
            if (instance == null)
            {
                instance = new EditorWindow(ModelObject);
                instance.Show();
            }
            else
            {
                return;
            }
        }

        private EditorWindow(ModelItemIFCObject ModelObject)
        {
            this.ModelObject = ModelObject;



            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            instance = null;
        }

        public ModelItemIFCObject ModelObject { get; }

        private void Window_Closed(object sender, EventArgs e)
        {
            ModelObject.OnPropertyChanged("CollectionPropertySet");
        }
    }
}