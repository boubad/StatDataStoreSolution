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
    /// Logique d'interaction pour CorrelationUserControl.xaml
    /// </summary>
    public partial class CorrelationUserControl : UserControl
    {
        private bool m_busy = false;
        public CorrelationUserControl()
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
        private void listboxXVar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m_busy)
            {
                return;
            }
            m_busy = true;
            var model = getModel();
            if (model != null)
            {
                Object obj = listboxXVar.SelectedItem;
                VariableDesc oVar = null;
                if ((obj != null) && (obj is VariableDesc))
                {
                    oVar = obj as VariableDesc;
                }
                if (oVar != null)
                {
                    var xVar = model.CurrentYVariable;
                    if ((xVar != null) && xVar.Equals(oVar))
                    {
                        oVar = null;
                    }
                }// oVar
                model.CurrentXVariable = oVar;
            }
            m_busy = false;
        }
        private void listboxYVar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m_busy)
            {
                return;
            }
            m_busy = true;
            var model = getModel();
            if (model != null)
            {
                Object obj = listboxYVar.SelectedItem;
                VariableDesc oVar = null;
                if ((obj != null) && (obj is VariableDesc))
                {
                    oVar = obj as VariableDesc;
                }
                if (oVar != null)
                {
                    var xVar = model.CurrentXVariable;
                    if ((xVar != null) && xVar.Equals(oVar))
                    {
                        oVar = null;
                    }
                }// oVar
                model.CurrentYVariable = oVar;
            }
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
                    this.comboboxCateg.IsEnabled = false;
                    this.comboboxCateg.SelectedItem = null;
                }
                model.CorrelationPlotModel = null;
            }// model
            m_busy = false;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (m_busy)
            {
                return;
            }
            m_busy = true;
            this.comboboxCateg.IsEnabled = false;
            this.checkboxCateg.IsChecked = false;
            var model = getModel();
            if (model != null)
            {
                this.checkboxPoints.IsChecked = model.HasPoints;
                this.checkboxImages.IsChecked = model.HasImages;
                this.checkboxLabels.IsChecked = model.HasLabels;
                var x = model.CurrentCategVariable;
                if ((x != null) && x.IsValid)
                {
                    this.comboboxCateg.IsEnabled = true;
                    this.checkboxCateg.IsChecked = true;
                }
                model.NotifyChange("CorrelationPlotModel");
            }
            m_busy = false;
        }

        private void checkboxPoints_Click(object sender, RoutedEventArgs e)
        {
            var model = getModel();
            if (model != null)
            {
                var v = this.checkboxPoints.IsChecked;
                if ((v != null) && v.HasValue)
                {
                    model.HasPoints = v.Value;
                }
                else
                {
                    model.HasPoints = false;
                }
            }// model
        }

        private void checkboxLabels_Click(object sender, RoutedEventArgs e)
        {
            var model = getModel();
            if (model != null)
            {
                var v = this.checkboxLabels.IsChecked;
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

        private void checkboxImages_Click(object sender, RoutedEventArgs e)
        {
            var model = getModel();
            if (model != null)
            {
                var v = this.checkboxImages.IsChecked;
                if ((v != null) && v.HasValue)
                {
                    model.HasImages = v.Value;
                }
                else
                {
                    model.HasImages = false;
                }
            }// model
        }
    }
}
