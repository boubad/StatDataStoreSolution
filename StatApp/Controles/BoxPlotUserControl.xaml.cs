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
using System.Windows.Navigation;
using System.Windows.Shapes;
using StatApp.ModelView;
using StatData;

namespace StatApp.Controles
{
    /// <summary>
    /// Logique d'interaction pour BoxPlotUserControl.xaml
    /// </summary>
    public partial class BoxPlotUserControl : UserControl
    {
        public BoxPlotUserControl()
        {
            InitializeComponent();
        }
        protected MainModelView getModel()
        {
            Object obj = this.DataContext;
            if ((obj != null) && (obj is MainModelView))
            {
                return obj as MainModelView;
            }
            return null;
        }// getModel
        private void myUpdateUI()
        {
            bool b = (getModel() != null);
            this.buttonAdd.IsEnabled = b && (this.srcListBox.SelectedItems.Count > 0);
            this.buttonRemove.IsEnabled = b && (this.destListBox.SelectedItems.Count > 0);
        }// myUpdateUI
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var p = getModel();
            if (p != null)
            {
                p.NotifyChange("BoxPlotModel");
            }
            myUpdateUI();
        }
        private void srcListBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            myUpdateUI();
        }

        private void destListBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            myUpdateUI();
        }

        private void buttonAdd_Click_1(object sender, RoutedEventArgs e)
        {
            var model = getModel();
            if (model == null)
            {
                return;
            }
            var oList = new List<VariableDesc>();
            foreach (var o in this.srcListBox.SelectedItems)
            {
                if ((o != null) && (o is VariableDesc))
                {
                    oList.Add(o as VariableDesc);
                }
            }
            model.AddNumVariables(oList);
            myUpdateUI();
        }

        private void buttonRemove_Click_1(object sender, RoutedEventArgs e)
        {
            var model = getModel();
            if (model == null)
            {
                return;
            }
            var oList = new List<VariableDesc>();
            foreach (var o in this.destListBox.SelectedItems)
            {
                if ((o != null) && (o is VariableDesc))
                {
                    oList.Add(o as VariableDesc);
                }
            }
            model.RemoveNumVariables(oList);
            myUpdateUI();
        }
    }
}
