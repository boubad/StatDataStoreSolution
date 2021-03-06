﻿using System;
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
    /// Logique d'interaction pour HierarchicalUserControl.xaml
    /// </summary>
    public partial class HierarchicalUserControl : UserControl
    {
        private CancellationTokenSource m_cts = null;
        private OrdModelView m_model;
        public HierarchicalUserControl()
        {
            InitializeComponent();
            this.buttonCancel.IsEnabled = false;
        }
        private void myUpdateUI()
        {
            var p = getModel();
            if (p == null)
            {
                return;
            }
            this.buttonCompute.IsEnabled = (p != null) && (!p.IsBusy) && (p.CurrentVariables.Count > 0);
            this.buttonAdd.IsEnabled = (p != null) && (!p.IsBusy) && (p.HierarClusterSet != null) && p.HierarClusterSet.IsValid;
            this.comboboxLinks.IsEnabled = (p != null) && (!p.IsBusy) && (p.CurrentVariables.Count > 0);
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
            m_cts = new CancellationTokenSource();
            this.buttonCancel.IsEnabled = true;
            model.IsBusy = true;
            model.UpdateClusters(model.ClassesCount, model.IterationsCount, m_cts.Token, null);
            m_cts = null;
            this.buttonCancel.IsEnabled = false;
            this.buttonCompute.IsEnabled = true;
            this.buttonAdd.IsEnabled = true;
            model.IsBusy = false;
            myUpdateUI();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            if ((m_cts != null) && (!m_cts.IsCancellationRequested))
            {
                m_cts.Cancel();
                this.buttonCancel.IsEnabled = false;
            }
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
            if ((name == "IsBusy") || (name == "CurrentVariables") || (name == "HierarClusterSet"))
            {
                myUpdateUI();
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            myUpdateUI();
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
            vv.Name = "Hierar";
            vv.DataType = "short";
            vv.IsCategVar = true;
            vv.Description = "Hierarchical Classification";
            this.buttonAdd.IsEnabled = false;
            this.buttonCompute.IsEnabled = false;
            model.IsBusy = true;
            var yy = await Task.Run<Tuple<bool, Exception>>(() =>
            {
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
            var xx = await Task.Run<Tuple<VariableDesc, Exception>>(() =>
            {
                return pMan.MaintainsVariable(vv);
            });
            if ((xx != null) && (xx.Item1 != null) && (xx.Item2 == null))
            {
                VariableDesc oVar = xx.Item1;
                int nVarId = oVar.Id;
                List<ValueDesc> vals = new List<ValueDesc>();
                foreach (var ind in model.Individus)
                {
                    int ival = ind.HierarClusterIndex;
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
                    var zz = await Task.Run<Tuple<bool, Exception>>(() =>
                    {
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
