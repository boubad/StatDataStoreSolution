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
    /// Logique d'interaction pour ClassificationUserControl.xaml
    /// </summary>
    public partial class ClassificationUserControl : UserControl
    {
        private static readonly String[] TAB_NAMES = new String[] {"IsBusy","OrdDisplayData",
            "ClustersData","CurrentVariables","Individus","ImagesDictionary",
            "CategClusterSet","KMeansClusterSet","HierarClusterSet"
        };
        private OrdModelView m_model;
        public ClassificationUserControl()
        {
            InitializeComponent();
        }
        private void myUpdateUI()
        {
            var p = getModel();
            if (p == null)
            {
                return;
            }
            bool b = (p != null) && (p.CurrentVariables.Count > 0) && (p.Individus != null) && (p.Individus.Count > 0);
            this.controlUtility.IsEnabled = b;
            this.controlKMeans.IsEnabled = b;
            this.controlHierar.IsEnabled = b;
         //   this.controlMatrices.IsEnabled = b;
            this.controlClusters.IsEnabled = b && (p.ClustersData != null) &&  (p.ClustersData.Count > 1);
            this.controlData.IsEnabled = b && (p.OrdDisplayData.Count > 1);
            bool bb = ((p.CategClusterSet != null) && p.CategClusterSet.IsValid) || 
                ((p.KMeansClusterSet != null) && p.KMeansClusterSet.IsValid) ||
                ((p.HierarClusterSet != null) && p.HierarClusterSet.IsValid);
            bb = b && bb;
            this.checkboxPoints.IsEnabled = bb;
            this.checkboxLabels.IsEnabled = bb;
            this.checkboxImages.IsEnabled = bb && (p.ImagesDictionary != null) &&  (p.ImagesDictionary.Count > 0);
            this.controlArrange.IsEnabled = bb && (p.SortedIndivsData != null) && (p.SortedIndivsData.Count > 0);
        }// myUpdateUI
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
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var model = getModel();
            if (model != null)
            {
                this.checkboxImages.IsChecked = model.HasImages;
                this.checkboxLabels.IsChecked = model.HasLabels;
                this.checkboxPoints.IsChecked = model.HasPoints;
            }
            myUpdateUI();
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

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            m_model = getModel();
            if (m_model != null)
            {
                m_model.PropertyChanged += m_model_PropertyChanged;
            }
        }

        void m_model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            String name = e.PropertyName;
            foreach (var s in TAB_NAMES)
            {
                if (s == name)
                {
                    myUpdateUI();
                }
            }
        }
    }
}
