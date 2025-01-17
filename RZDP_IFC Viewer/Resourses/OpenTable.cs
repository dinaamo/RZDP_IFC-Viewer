//using System.Windows.Controls;
//using System.Windows.Input;
//using IFC_Table_View.IFC.ModelItem;
//using IFC_Table_View.View.Windows;

//namespace IFC_Table_View.Resourses
//{
//    public partial class OpenTable
//    {
//        public void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
//        {
//            if (e.ClickCount >= 2)
//            {
//                if (sender is TextBlock textBlock)
//                {
//                    if (textBlock.DataContext is ModelItemIFCTable table)
//                    {
//                        new WindowTable(table.IFCTable).ShowDialog();
//                    }
//                }
//            }
//        }
//    }
//}