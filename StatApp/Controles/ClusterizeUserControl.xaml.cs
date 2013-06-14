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
    /// Logique d'interaction pour ClusterizeUserControl.xaml
    /// </summary>
    public partial class ClusterizeUserControl : UserControl
    {
        private OrdModelView m_model;
        public ClusterizeUserControl()
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
        private void myUpdateUI()
        {
            var model = getModel();
            if (model == null)
            {
                return;
            }
            bool b = (model != null);
            this.buttonAdd.IsEnabled = b && (this.srcListBox.SelectedItems.Count > 0);
            this.buttonRemove.IsEnabled = b && (this.destListBox.SelectedItems.Count > 0);
            bool bAdd = false;
            if (model != null)
            {
                bAdd = b;
                var pp = model.CurrentStatDataSet;
                bAdd = bAdd && (pp != null) && pp.IsValid;
            }
        }// myUpdateUI
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var p = getModel();
            if (p == null)
            {
                return;
            }
            p.RefreshLeftVariables();
        }
        private void srcListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            myUpdateUI();
        }
        private void destListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            myUpdateUI();
        }
        private void buttonAdd_Click(object sender, RoutedEventArgs e)
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
            model.AddVariables(oList);
            myUpdateUI();
        }
        private void buttonRemove_Click(object sender, RoutedEventArgs e)
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
            model.RemoveVariables(oList);
            myUpdateUI();
        }
        private void comboboxMatriceMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var model = getModel();
            if (model == null)
            {
                return;
            }
            Object obj = comboboxMatriceMode.SelectedItem;
            MatriceComputeMode mode = MatriceComputeMode.modeNothing;
            if ((obj != null) && (obj is ComboBoxItem))
            {
                ComboBoxItem item = obj as ComboBoxItem;
                Object oo = item.Tag;
                if (oo != null)
                {
                    String s = oo.ToString().Trim().ToUpper();
                    if (s == "AUCUN")
                    {
                        mode = MatriceComputeMode.modeNothing;
                    }
                    else if (s == "NORMALIZE")
                    {
                        mode = MatriceComputeMode.modeNormalize;
                    }
                    else if (s == "PROFIL")
                    {
                        mode = MatriceComputeMode.modeProfil;
                    }
                    else if (s == "RANK")
                    {
                        mode = MatriceComputeMode.modeRank;
                    }
                }
            }// utem
            model.MatriceMode = mode;
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
        }

    }
}
