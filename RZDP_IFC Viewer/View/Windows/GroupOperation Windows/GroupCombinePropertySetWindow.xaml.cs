﻿using System;
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
using RZDP_IFC_Viewer.ViewModels;

namespace RZDP_IFC_Viewer.View.Windows.GroupOperation_Windows
{

    public partial class GroupCombinePropertySetWindow : Window
    {
        public GroupCombinePropertySetWindow(IEnumerable<ModelItemIFCObject> FilteredSearchItems)
        {
            InitializeComponent();
            DataContext = new GroupCombinePropertySetViewModel(FilteredSearchItems);
        }
    }
}
