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
///////////////////////////////
using StatApp.ModelView;
using StatApp;
using StatData;
/////////////////////////////////////
namespace StatApp.Controles
{
    /// <summary>
    /// Logique d'interaction pour MainMenuUserControl.xaml
    /// </summary>
    public partial class MainMenuUserControl : UserControl
    {
        private static readonly String[] TAB_NAMES = new String[] {"IsBusy","StatDataSets","CurrentStatDataSet","AllNumVariables",
        "AllComputeVariables","AllIndividus","Variables"};
        private MainModelView m_model;
        public MainMenuUserControl()
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
        private void myUpdateUI()
        {
            var model = getModel();
            bool bData = (model != null) && (model.DataService != null);
            bool bEigen = bData;
            bool bGraph = bData;
            bool bOrd = bData;
            bool bPhoto = bData;
            bool bSets = bData;
            //
            if (model != null)
            {
                var p = model.CurrentStatDataSet;
                bool b = (p != null) && (p.IsValid);
                bEigen = b && (model.AllNumVariables.Count > 1) && (model.AllIndividus.Count > 1);
                bGraph = b && (model.AllNumVariables.Count > 0) && (model.AllIndividus.Count > 1);
                bOrd = b && (model.AllComputeVariables.Count > 0) && (model.AllIndividus.Count > 1);
            }// model
            //
            this.buttonDataSets.IsEnabled = bSets;
            this.buttonEigen.IsEnabled = bEigen;
            this.buttonGraphs.IsEnabled = bGraph;
            this.buttonClassification.IsEnabled = bOrd;
            this.buttonPhotos.IsEnabled = bPhoto;
            this.comboboxDataSets.IsEnabled = bSets && (model.StatDataSets.Count > 0);
        }// myUpdateUI
        private void navigate(String s)
        {
            var model = getModel();
            if (model != null)
            {
                model.NavigateToPage(s);
            }
        }// navigate
        private void buttonDataSets_Click(object sender, RoutedEventArgs e)
        {
            navigate(PageLocator.DATASETS_PAGE);
        }
        private void buttonGraphs_Click(object sender, RoutedEventArgs e)
        {
            navigate(PageLocator.GRAPH_PAGE);
        }

        private void buttonPhotos_Click(object sender, RoutedEventArgs e)
        {
            navigate(PageLocator.PHOTOS_PAGE);
        }

        private void buttonClassification_Click(object sender, RoutedEventArgs e)
        {
            navigate(PageLocator.CLASSIFICATION_PAGE);
        }

        private void buttonEigen_Click(object sender, RoutedEventArgs e)
        {
            navigate(PageLocator.EIGEN_PAGE);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            myUpdateUI();
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
        }// import
    }
}
