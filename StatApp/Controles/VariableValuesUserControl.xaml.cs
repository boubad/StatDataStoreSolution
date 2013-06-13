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
using StatData;
using StatApp.ModelView;
namespace StatApp.Controles
{
    /// <summary>
    /// Logique d'interaction pour VariableValuesUserControl.xaml
    /// </summary>
    public partial class VariableValuesUserControl : UserControl
    {
        private bool m_busy = false;
        public VariableValuesUserControl()
        {
            InitializeComponent();
        }
        private void myUpdateUI(bool b)
        {
            var p = getModel();
            bool bx = (p != null);
            if (p != null)
            {
                bx = (p.DataService != null);
                if (bx)
                {
                    var oVar = p.CurrentVariable;
                    bx = (oVar != null) && oVar.IsValid;
                }
            }
            this.buttonRefresh.IsEnabled = b & bx;
            this.buttonAdd.IsEnabled = b && bx;
            this.buttonNextPage.IsEnabled = b && bx && ((p.Skip + p.Taken) < p.TotalValuesCount);
            this.buttonPrevPage.IsEnabled = b && bx && (p.Skip > 0);
            this.buttonRemove.IsEnabled = b && bx && (this.valuesGrid.SelectedItems.Count > 0);
            this.buttonSave.IsEnabled = b && bx;
        }// myUpdateUI
        private MainModelView getModel()
        {
            MainModelView pRet = null;
            Object obj = this.DataContext;
            if ((obj != null) && (obj is MainModelView))
            {
                pRet = obj as MainModelView;
            }
            return pRet;
        }// getModel
        private async void refreshValues()
        {
            var model = getModel();
            if (model == null)
            {
                return;
            }
            var pMan = model.DataService;
            var oVar = model.CurrentVariable;
            if ((pMan != null) && (oVar != null) && oVar.IsValid)
            {
                myUpdateUI(false);
                m_busy = true;
                var yy = await Task.Run<Tuple<int, Exception>>(() =>
                {
                    return pMan.GetVariableValuesCount(oVar);
                });
                if ((yy != null) && (yy.Item2 == null))
                {
                    model.TotalValuesCount = yy.Item1;
                }
                var xx = await Task.Run<Tuple<IEnumerable<ValueDesc>, Exception>>(() =>
                {
                    return pMan.GetVariableValues(oVar, model.Skip, model.Taken);
                });
                if (xx != null)
                {
                    if ((xx.Item1 != null) && (xx.Item2 == null))
                    {
                        model.Values = new ValueDescs(xx.Item1);
                    }
                    else if (xx.Item2 != null)
                    {
                        model.ShowError(xx.Item2);
                    }
                }// xx
                m_busy = false;
                myUpdateUI(true);
            }// pMan
        }// RefreshValues
        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            var model = getModel();
            if (model == null)
            {
                return;
            }
            m_busy = true;
            var oVar = model.CurrentVariable;
            if (oVar != null)
            {
                ValueDescs vv = model.Values;
                int nMax = 0;
                if (vv.Count > 0)
                {
                    nMax = (from x in vv select x.Index).Max();
                    ++nMax;
                }
                int nVarId = oVar.Id;
                ValueDesc v = new ValueDesc();
                v.VariableId = nVarId;
                v.Index = nMax;
                vv.Add(v);
                model.NotifyChange("Values");
            }// oVar
            m_busy = false;
        }
        private async void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            var model = getModel();
            if (model == null)
            {
                return;
            }
            var vv = model.Values;
            var pMan = model.DataService;
            var oVar = model.CurrentVariable;
            if ((vv == null) || (pMan == null) || (oVar == null))
            {
                return;
            }
            if ((vv.Count < 1) || (!oVar.IsValid))
            {
                return;
            }
            List<ValueDesc> vals = new List<ValueDesc>();
            var q = from x in vv where x.IsModified select x;
            foreach (var p in q)
            {
                vals.Add(p);
            }
            if (vals.Count < 1)
            {
                return;
            }
            myUpdateUI(false);
            m_busy = true;
            var yy = await Task.Run<Tuple<bool, Exception>>(() =>
            {
                return pMan.WriteValues(vals);
            });
            if ((yy != null) && (yy.Item2 == null) && yy.Item1)
            {
                myUpdateUI(true);
                m_busy = false;
                this.refreshValues();
            }
            else if ((yy != null) && (yy.Item2 != null))
            {
                model.ShowError(yy.Item2);
                myUpdateUI(true);
            }
            m_busy = false;
        }
        private async void buttonRemove_Click(object sender, RoutedEventArgs e)
        {
            var model = getModel();
            if (model == null)
            {
                return;
            }
            var vv = model.Values;
            var pMan = model.DataService;
            var oVar = model.CurrentVariable;
            if ((vv == null) || (pMan == null) || (oVar == null))
            {
                return;
            }
            if ((vv.Count < 1) || (!oVar.IsValid))
            {
                return;
            }
            List<ValueDesc> vals = new List<ValueDesc>();
            foreach (var v in this.valuesGrid.SelectedItems)
            {
                if ((v != null) && (v is ValueDesc))
                {
                    vals.Add(v as ValueDesc);
                }
            }// v
            if (vals.Count < 1)
            {
                return;
            }
            myUpdateUI(false);
            m_busy = true;
            var yy = await Task.Run<Tuple<bool, Exception>>(() =>
            {
                return pMan.RemoveValues(vals);
            });
            if ((yy != null) && (yy.Item2 == null) && yy.Item1)
            {
                myUpdateUI(true);
                m_busy = false;
                this.refreshValues();
            }
            else if ((yy != null) && (yy.Item2 != null))
            {
                model.ShowError(yy.Item2);
                myUpdateUI(true);
            }
            m_busy = false;
        }
        private void buttonPrevPage_Click(object sender, RoutedEventArgs e)
        {
            var model = getModel();
            if (model == null)
            {
                return;
            }
            int n = model.Skip - model.Taken;
            if (n >= 0)
            {
                model.Skip = n;
                this.refreshValues();
            }
        }
        private void buttonNextPage_Click(object sender, RoutedEventArgs e)
        {
            var model = getModel();
            if (model == null)
            {
                return;
            }
            int n = model.Skip + model.Taken;
            if (n  < model.TotalValuesCount)
            {
                model.Skip = n;
                this.refreshValues();
            }
        }
        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.refreshValues();
        }

        private void valuesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.buttonRemove.IsEnabled = (!m_busy) && (this.valuesGrid.SelectedItems.Count > 0);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            myUpdateUI(true);
            this.buttonRemove.IsEnabled = (this.valuesGrid.SelectedItems.Count > 0);
            this.refreshValues();
        }

        private void listboxVariables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var model = getModel();
            if (model == null)
            {
                return;
            }
            model.Skip = 0;
            this.refreshValues();
        }
    }
}
