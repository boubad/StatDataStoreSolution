using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
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
    /// Logique d'interaction pour CategorryUtilityUserControl.xaml
    /// </summary>
    public partial class CategorryUtilityUserControl : UserControl
    {
        private OrdModelView m_model;
        private CancellationTokenSource m_cts = null;
        public CategorryUtilityUserControl()
        {
            InitializeComponent();
            this.progressbar.Visibility = Visibility.Hidden;
            this.buttonCancel.IsEnabled = false;
        }
        private void myUpdateUI()
        {
            var p = getModel();
            this.buttonCompute.IsEnabled = (p != null) && (!p.IsBusy) &&  (p.CurrentVariables.Count > 0);
            this.buttonAdd.IsEnabled = (p != null) && (!p.IsBusy) && (p.CategClusterSet != null) && p.CategClusterSet.IsValid;
        }// myUpdateUI
        private OrdModelView getModel()
        {
            OrdModelView pRet = null;
            Object obj = this.DataContext;
            if ((obj != null) && (obj is OrdModelView))
            {
                pRet = obj as OrdModelView;
            }
            return pRet;
        }// getModel
        private void buttonCompute_Click(object sender, RoutedEventArgs e)
        {
            var model = getModel();
            if (model == null)
            {
                return;
            }
            this.buttonAdd.IsEnabled = false;
            this.buttonCompute.IsEnabled = false;
            this.progressbar.Visibility = Visibility.Visible;
            this.progressbar.Maximum = 100;
            this.progressbar.Value = 0;
            m_cts = new CancellationTokenSource();
            this.buttonCancel.IsEnabled = true;
            var progress = new Progress<Tuple<int, CategClusterSet>>(v => {
                if (v != null)
                {
                    int f = v.Item1;
                    CategClusterSet oSet = v.Item2;
                    progressbar.Value = f;
                    if (oSet != null)
                    {
                        model.CategClusterSet = oSet;
                    }
                }// v
            });
            model.IsBusy = true;
            model.UpdateClusters(model.ClassesCount, model.IterationsCount, m_cts.Token, progress);
            m_cts = null;
            this.progressbar.Visibility = Visibility.Hidden;
            this.buttonCancel.IsEnabled = false;
            this.buttonCompute.IsEnabled = true;
            this.buttonAdd.IsEnabled = true;
            model.IsBusy = false;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            if ((m_cts != null) && (!m_cts.IsCancellationRequested))
            {
                m_cts.Cancel();
                this.buttonCancel.IsEnabled = false;
            }
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
            myUpdateUI();
        }

        void m_model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            String name = e.PropertyName;
            if ((name == "IsBusy") || (name == "CurrentVariables") || (name == "CategClusterSet"))
            {
                myUpdateUI();
            }
        }

        private async void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            var model = getModel();
            if (model == null)
            {
                return;
            }
            var oSet = model.CurrentStatDataSet;
            if (oSet == null)
            {
                return;
            }
            var pMan = model.DataService;
            if ((!oSet.IsValid) || (pMan == null))
            {
                return;
            }
            VariableDesc vv = new VariableDesc();
            vv.DataSetId = oSet.Id;
            vv.Name = "CategUtility";
            vv.DataType = "short";
            vv.IsCategVar = true;
            vv.Description = "Category Utility Classification";
            this.buttonAdd.IsEnabled = false;
            this.buttonCompute.IsEnabled = false;
            model.IsBusy = true;
            var yy = await Task.Run<Tuple<bool, Exception>>(() => {
                return pMan.RemoveVariable(vv);
            });
            if ((yy != null) && (yy.Item2 != null))
            {
                this.buttonAdd.IsEnabled = true;
                this.buttonCompute.IsEnabled = true;
                model.ShowError(yy.Item2);
                model.IsBusy = false;
                return;
            }
            var xx = await Task.Run<Tuple<VariableDesc, Exception>>(() => {
                return pMan.MaintainsVariable(vv);
            });
            if ((xx != null) && (xx.Item1 != null) && (xx.Item2 == null))
            {
                VariableDesc oVar = xx.Item1;
                int nVarId = oVar.Id;
                List<ValueDesc> vals = new List<ValueDesc>();
                foreach (var ind in model.Individus)
                {
                    int ival = ind.UtilityClusterIndex;
                    int index = ind.IndivIndex;
                    if ((ival >= 0) && (index >= 0))
                    {
                        ValueDesc v = new ValueDesc();
                        v.VariableId = nVarId;
                        v.Index = index;
                        v.DataStringValue = Convert.ToString(ival);
                        v.IsModified = true;
                        vals.Add(v);
                    }
                }// ind
                if (vals.Count > 0)
                {
                    var zz = await Task.Run<Tuple<bool, Exception>>(() => {
                        return pMan.WriteValues(vals);
                    });
                    if ((zz != null) && (zz.Item2 != null))
                    {
                        model.ShowError(zz.Item2);
                    }
                }// vals
            }
            else if ((xx != null) && (xx.Item2 != null))
            {
                model.ShowError(xx.Item2);
            }
            this.buttonAdd.IsEnabled = true;
            this.buttonCompute.IsEnabled = true;
            model.IsBusy = false;
            myUpdateUI();
        }
    }
}
