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
    /// Logique d'interaction pour EigenUserControl.xaml
    /// </summary>
    public partial class EigenUserControl : UserControl
    {
        private static readonly String[] TAB_NAMES = new String[] {"IsBusy","Anacompo",
            "InitialData","EigenValues","EigenVectors",
            "EigenVariables","EigenIndivs","AllNumVariables"};
        private AnaCompoModelView m_model;
        public EigenUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.comboboxCateg.IsEnabled = false;
            this.checkboxCateg.IsChecked = false;
            var model = getModel();
            if (model != null)
            {
                this.checkboxPoints.IsChecked = model.HasPoints;
                this.checkboxImages.IsChecked = model.HasImages;
                this.checkboxLabels.IsChecked = model.HasLabels;
                model.NotifyChange("AllNumVariables");
                var x = model.CurrentCategVariable;
                if ((x != null) && x.IsValid)
                {
                    this.comboboxCateg.IsEnabled = true;
                    this.checkboxCateg.IsChecked = true;
                }
            }// model
            myUpdateUI();
        }
        private AnaCompoModelView getModel()
        {
            AnaCompoModelView model = null;
            Object obj = this.DataContext;
            if ((obj != null) && (obj is AnaCompoModelView))
            {
                model = obj as AnaCompoModelView;
            }
            return model;
        }// getModel
        private void myUpdateUI()
        {
            var model = getModel();
            bool b = (model != null) && (!model.IsBusy);
            this.buttonAdd.IsEnabled = b && (this.srcListBox.SelectedItems.Count > 0);
            this.buttonRemove.IsEnabled = b && (this.destListBox.SelectedItems.Count > 0);
            bool bAdd = b && model.CurrentStatDataSet.IsValid && (model.Anacompo != null) && model.Anacompo.IsValid;
            this.buttonAddDataSet.IsEnabled = bAdd;
            bool bOk = b && (model.Anacompo != null) && model.Anacompo.IsValid;
            this.controlGraphiques.IsEnabled = bOk;
            this.controlIndivs.IsEnabled = b && (model.EigenIndivs.Count > 1);
            this.controlInitialData.IsEnabled = b && (model.InitialData.Count > 1);
            this.controlValues.IsEnabled = b && (model.EigenValues.Count > 1);
            this.controlVariables.IsEnabled = b && (model.EigenVariables.Count > 1);
            this.controlVectors.IsEnabled = b && (model.EigenVectors.Count > 1);
            this.checkboxPoints.IsEnabled = bOk;
            this.checkboxLabels.IsEnabled = bOk;
            this.checkboxImages.IsEnabled = bOk;
            this.comboboxX.IsEnabled = bOk;
            this.comboboxY.IsEnabled = bOk;
            this.comboboxGraphiques.IsEnabled = bOk;
            this.textboxFactorCount.IsEnabled = bOk;
        }// myUpdateUI
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
        private void checkboxCateg_Click(object sender, RoutedEventArgs e)
        {
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
            }// model
            myUpdateUI();
        }
        private  void buttonAddDataSet_Click(object sender, RoutedEventArgs e)
        {
            var model = getModel();
            if (model == null)
            {
                return;
            }
            model.AddToDataSet();
            myUpdateUI();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var model = getModel();
            m_model = model;
            this.comboboxCateg.IsEnabled = false;
            this.checkboxCateg.IsChecked = false;
            if (model != null)
            {
                m_model.PropertyChanged +=m_model_PropertyChanged;
                var x = model.CurrentCategVariable;
                if ((x != null) && x.IsValid)
                {
                    this.comboboxCateg.IsEnabled = true;
                    this.checkboxCateg.IsChecked = true;
                }
            }// model
            myUpdateUI();
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

        private void checkboxRobust_Click(object sender, RoutedEventArgs e)
        {
            var model = getModel();
            if (model != null)
            {
                var v = this.checkboxRobust.IsChecked;
                if ((v != null) && v.HasValue)
                {
                    model.IsRobust = v.Value;
                }
                else
                {
                    model.IsRobust = false;
                }
            }// model
            myUpdateUI();
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
            myUpdateUI();
        }
    }
}
