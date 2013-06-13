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
    /// Logique d'interaction pour DistribUserControl.xaml
    /// </summary>
    public partial class DistribUserControl : UserControl
    {
        private bool m_busy = false;
        public DistribUserControl()
        {
            InitializeComponent();
        }
        private MainModelView getModel()
        {
            MainModelView model = null;
            Object obj = this.DataContext;
            if ((obj != null) && (obj is MainModelView))
            {
                model = obj as MainModelView;
            }
            return model;
        }// getModel
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (m_busy)
            {
                return;
            }
            m_busy = true;
            var model = getModel();
            this.comboboxCateg.IsEnabled = false;
            this.checkboxCateg.IsChecked = false;
            if (model != null)
            {
                var x = model.CurrentCategVariable;
                if ((x != null) && x.IsValid)
                {
                    this.comboboxCateg.IsEnabled = true;
                    this.checkboxCateg.IsChecked = true;
                }
                model.NotifyChanges(new String[] {"AllNumVariables","AllCategVariables","CurrentVariable","CurrentCategVariable",
                    "HistogPlotModel","NormalModel","CategBoxPlotModel" });
            }// model
            m_busy = false;
        }
        private void checkboxCateg_Click(object sender, RoutedEventArgs e)
        {
            if (m_busy)
            {
                return;
            }
            m_busy = true;
            var model = getModel();
            if (model != null)
            {
                var v = checkboxCateg.IsChecked;
                if ((v != null) && v.HasValue && v.Value)
                {
                    this.comboboxCateg.IsEnabled = true;
                }
                else
                {
                    model.CurrentCategVariable = null;
                    this.comboboxCateg.SelectedItem = null;
                    this.comboboxCateg.IsEnabled = false;
                }
                model.CategBoxPlotModel = null;
            }// model
            m_busy = false;
        }
    }
}
