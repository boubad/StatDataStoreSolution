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
namespace StatApp.Controles
{
    /// <summary>
    /// Logique d'interaction pour CombinedOrdUserControl.xaml
    /// </summary>
    public partial class CombinedOrdUserControl : UserControl
    {
        public CombinedOrdUserControl()
        {
            InitializeComponent();
        }
        private OrdModelView getModel()
        {
            OrdModelView model = null;
            Object obj = this.DataContext;
            if ((obj != null) && (obj is OrdModelView))
            {
                model = obj as OrdModelView;
            }
            return model;
        }// getModel
        private void checkboxLabels_Click(object sender, RoutedEventArgs e)
        {
            var model = getModel();
            if (model != null)
            {
                var v = checkboxLabels.IsChecked;
                if ((v != null) && v.HasValue)
                {
                    model.HasLabels = v.Value;
                }
                else
                {
                    model.HasLabels = false;
                }
            }// model
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var model = getModel();
            if (model != null)
            {
                checkboxLabels.IsChecked = model.HasLabels;
            }// model
        }
    }
}
