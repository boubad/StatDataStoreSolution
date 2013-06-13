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
    /// Logique d'interaction pour CurrentDataSetUserControl.xaml
    /// </summary>
    public partial class CurrentDataSetUserControl : UserControl
    {
        private static readonly String[] TAB_NAMES = new String[] {"IsBusy", "InitialData", "VariablesInfos",
        "CorrelationsDisplay","CurrentStatDataSet","CurrentVariable"};
        private MainModelView m_model;
        public CurrentDataSetUserControl()
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
            var p = getModel();
            bool bOk = (p != null) && (!p.IsBusy);
            this.controlCorrelations.IsEnabled = bOk && (p.CorrelationsDisplay.Count > 1);
            this.controlEdit.IsEnabled = bOk && (p.CurrentVariable != null) && (p.CurrentStatDataSet != null) && p.CurrentStatDataSet.IsValid;
            this.controlInitialData.IsEnabled = bOk && (p.InitialData.Count > 1);
            this.controlStatDataSet.IsEnabled = bOk && (p.DataService != null);
            this.controlStats.IsEnabled = bOk && (p.VariablesInfos.Count > 1);
            this.assocControl.IsEnabled = bOk && (p.AllIndividus.Count > 0);
        }// myUpdateUI
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            myUpdateUI();
        }
    }
}
