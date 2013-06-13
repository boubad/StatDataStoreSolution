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
using System.Threading;
using System.IO;
////////////////////////////////////////
using Microsoft.Research.Science.Data;
using sds = Microsoft.Research.Science.Data;
using Microsoft.Research.Science.Data.Imperative;
///////////////////////////////////
using StatData;
using StatApp.ModelView;
///////////////////////////////////////
namespace StatApp.Controles
{
    /// <summary>
    /// Logique d'interaction pour StatDataSetUserControl.xaml
    /// </summary>
    public partial class StatDataSetUserControl : UserControl
    {
        private static readonly String[] TAB_NAMES = new String[] { "IsBusy", "CurrentStatDataSet", "Variables", "CurrentVariable" };
        private MainModelView m_model;
        private CancellationTokenSource m_cts;
        private bool m_busy;
        public StatDataSetUserControl()
        {
            InitializeComponent();
            this.progressbar.Visibility = Visibility.Hidden;
            this.buttonCancel.IsEnabled = false;
        }
        private static double myconvert(double x)
        {
            double xx = 10000.0 * x;
            if (x < 0.0)
            {
                xx -= 0.5;
            }
            else
            {
                xx += 0.5;
            }
            int nx = (int)xx;
            return (double)(nx / 10000.0);
        }// myconvert
        public bool IsBusy
        {
            get
            {
                return m_busy;
            }
            set
            {
                if (value != m_busy)
                {
                    m_busy = value;
                }
            }
        }// IsBusy
        private void myUpdateUI(bool b)
        {
            var p = getModel();
            if (p == null)
            {
                return;
            }
            bool bb = (p != null) && (p.DataService != null) && (!p.IsBusy);
            this.buttonRefresh.IsEnabled = b && bb && p.CurrentStatDataSet.IsValid;
            this.buttonRemove.IsEnabled = b && bb && p.CurrentStatDataSet.IsValid;
            this.buttonSave.IsEnabled = b && bb && p.CurrentStatDataSet.IsWriteable;
            this.buttonRemoveVariable.IsEnabled = b && bb && p.CurrentVariable.IsValid;
            this.buttonSaveVariable.IsEnabled = b && bb && p.CurrentVariable.IsWriteable;
            this.buttonCancel.IsEnabled = bb && (!b);
            this.buttonExport.IsEnabled = b && bb && p.CurrentStatDataSet.IsValid;
            this.buttonImport.IsEnabled = b && bb;
            this.panelVariables.IsEnabled = b && bb && (p.Variables.Count > 0);
            this.currentVariablePanel.IsEnabled = b && bb && (p.CurrentVariable != null);
            if ((p.CurrentVariable != null) && p.CurrentVariable.IsValid && p.CurrentVariable.IsNumVar && (!p.CurrentVariable.IsInfoVar))
            {
                this.gridNumInfo.Visibility = Visibility.Visible;
            }
            else
            {
                this.gridNumInfo.Visibility = Visibility.Hidden;
            }
            if ((p.CurrentVariable != null) && p.CurrentVariable.IsCategVar && (!p.CurrentVariable.IsInfoVar))
            {
                this.listboxCategs.Visibility = Visibility.Visible;
            }
            else
            {
                this.listboxCategs.Visibility = Visibility.Hidden;
            }
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
        private StatDataSet getDataSet()
        {
            StatDataSet oSet = null;
            var p = getModel();
            if (p != null)
            {
                oSet = p.CurrentStatDataSet;
            }
            return oSet;
        }// getDataSet
        private async void refreshCurrentDataSet()
        {
            if (this.IsBusy)
            {
                return;
            }
            var model = getModel();
            if (model == null)
            {
                return;
            }
            var pMan = model.DataService;
            var oSet = model.CurrentStatDataSet;
            if ((pMan != null) && (oSet != null) && oSet.IsValid)
            {
                this.IsBusy = true;
                myUpdateUI(false);
                var xx = await Task.Run<Tuple<StatDataSet, Exception>>(() =>
                {
                    return pMan.GetDataSet(oSet);
                });
                if (xx != null)
                {
                    if ((xx.Item1 != null) && (xx.Item2 == null))
                    {
                        StatDataSet xSet = xx.Item1;
                        oSet.Id = xSet.Id;
                        oSet.Name = xSet.Name;
                        oSet.LastIndex = xSet.LastIndex;
                        oSet.Description = xSet.Description;
                        oSet.Variables = null;
                        model.CurrentStatDataSet = null;
                        model.CurrentStatDataSet = oSet;
                    }
                    else if (xx.Item2 != null)
                    {
                        model.ShowError(xx.Item2);
                    }
                }// xx
                myUpdateUI(true);
                this.IsBusy = false;
            }// pMan

        }// RefreshCurrentDataSet
        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            refreshCurrentDataSet();
        }
        private async void saveCurrentDataSet()
        {
            if (this.IsBusy)
            {
                return;
            }
            var model = getModel();
            if (model == null)
            {
                return;
            }

            var pMan = model.DataService;
            var oSet = model.CurrentStatDataSet;
            if ((pMan != null) && (oSet != null) && oSet.IsWriteable)
            {
                List<VariableDesc> oList = new List<VariableDesc>();
                var p = oSet.Variables;
                if (p != null)
                {
                    oList = p.ToList();
                }
                this.IsBusy = true;
                myUpdateUI(false);
                var xx = await Task.Run<Tuple<StatDataSet, Exception>>(() =>
                {
                    return pMan.MaintainsDataSet(oSet);
                });
                if (xx != null)
                {
                    if ((xx.Item1 != null) && (xx.Item2 == null))
                    {
                        StatDataSet xSet = xx.Item1;
                        oSet.Id = xSet.Id;
                        oSet.Name = xSet.Name;
                        oSet.LastIndex = xSet.LastIndex;
                        oSet.Description = xSet.Description;
                        foreach (var oVar in oList)
                        {
                            if ((oVar != null) && oVar.IsWriteable)
                            {
                                var yy = await Task.Run<Tuple<VariableDesc, Exception>>(() =>
                                {
                                    return pMan.MaintainsVariable(oVar);
                                });
                            }// oVar
                        }// oVar
                        oSet.Variables = null;
                    }
                    else if (xx.Item2 != null)
                    {
                        model.ShowError(xx.Item2);
                    }
                }// xx
                myUpdateUI(true);
                this.IsBusy = false;
            }// pMan

        }// RefreshCurrentDataSet
        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            this.saveCurrentDataSet();
        }
        private async void removeCurrentDataSet()
        {
            if (this.IsBusy)
            {
                return;
            }
            var model = getModel();
            if (model == null)
            {
                return;
            }
            var r = MessageBox.Show("Voulez-vous vraiment supprimer cet ensemble de données?", "StatApp", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (r != MessageBoxResult.Yes)
            {
                return;
            }
            var pMan = model.DataService;
            var oSet = model.CurrentStatDataSet;
            if ((pMan != null) && (oSet != null) && oSet.IsValid)
            {
                this.IsBusy = true;
                myUpdateUI(false);
                List<VariableDesc> oList = new List<VariableDesc>();
                var p = oSet.Variables;
                if (p != null)
                {
                    oList = p.ToList();
                }
                var xx = await Task.Run<Tuple<bool, Exception>>(() =>
                {
                    return pMan.RemoveDataSet(oSet);
                });
                if (xx != null)
                {
                    if ((xx.Item1) && (xx.Item2 == null))
                    {
                        model.StatDataSets = null;
                        model.RefreshDataSets();
                    }
                    else if (xx.Item2 != null)
                    {
                        model.ShowError(xx.Item2);
                    }
                }// xx
                myUpdateUI(true);
                this.IsBusy = false;
            }// pMan
        }// removeCurrentDataSet
        private void buttonRemove_Click(object sender, RoutedEventArgs e)
        {
            this.removeCurrentDataSet();
        }
        private void myCheckUI()
        {
            var p = getDataSet();
            this.buttonCancel.IsEnabled = false;
            if ((p != null) && p.IsValid)
            {
                myUpdateUI(true);
            }
            else
            {
                myUpdateUI(false);
                this.buttonCancel.IsEnabled = false;
            }
        }// myCheckUI
        private void buttonSaveVariable_Click(object sender, RoutedEventArgs e)
        {
            var model = getModel();
            if (model == null)
            {
                return;
            }
            StatDataSet oSet = model.CurrentStatDataSet;
            VariableDesc oVar = model.CurrentVariable;
            var pMan = model.DataService;
            if ((oSet != null) && (oVar != null) && (pMan != null))
            {
                oSet.MaintainsVariable(oVar, pMan);
            }
        }
        private async void buttonRemoveVariable_Click(object sender, RoutedEventArgs e)
        {
            if (this.IsBusy)
            {
                return;
            }
            var model = getModel();
            if (model == null)
            {
                return;
            }
            VariableDesc oVar = model.CurrentVariable;
            var pMan = model.DataService;
            if ((pMan != null) && (oVar != null) && oVar.IsWriteable)
            {
                this.IsBusy = true;
                myUpdateUI(false);
                var xx = await Task.Run<Tuple<bool, Exception>>(() =>
                {
                    return pMan.RemoveVariable(oVar);
                });
                this.IsBusy = false;
                myUpdateUI(true);
                if ((xx != null) && (xx.Item1) && (xx.Item2 == null))
                {
                    this.refreshCurrentDataSet();
                }
                else if ((xx != null) && (xx.Item2 != null))
                {
                    model.ShowError(xx.Item2);
                }
            }// ok
        }
        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            if ((m_cts != null) && (!m_cts.IsCancellationRequested))
            {
                m_cts.Cancel();
            }
        }
        private Task<Tuple<StatDataSet, Exception>> readDataSet(String filename,
            sds.DataSet input, CancellationToken cancellationToken, IProgress<int> progress)
        {
            return Task.Run<Tuple<StatDataSet, Exception>>(() =>
            {
                StatDataSet oSet = null;
                Exception err = null;
                try
                {
                    FileInfo info = new FileInfo(filename);
                    oSet = new StatDataSet();
                    oSet.Name = info.Name;
                    var col = input.Variables;
                    int nTotal = col.Count;
                    int nCur = 0;
                    foreach (var v in col)
                    {
                        if (progress != null)
                        {
                            progress.Report(nCur++);
                        }
                        if (v.Dimensions.Count != 1)
                        {
                            continue;
                        }
                        VariableDesc vv = new VariableDesc();
                        ValueDescs vals = new ValueDescs();
                        vv.Name = v.Name;
                        Type t = v.TypeOfData;
                        if (t == typeof(String))
                        {
                            vv.DataType = "string";
                            vv.IsCategVar = true;
                            String[] data = input.GetData<String[]>(v.ID);
                            if ((data != null) && (data.Length > 0))
                            {
                                for (int i = 0; i < data.Length; ++i)
                                {
                                    ValueDesc vx = new ValueDesc();
                                    vx.Index = i;
                                    vx.DataStringValue = data[i];
                                    vals.Add(vx);
                                }// i
                            }// data
                        }
                        else if (t == typeof(double))
                        {
                            vv.DataType = "double";
                            vv.IsCategVar = false;
                            double[] data = input.GetData<double[]>(v.ID);
                            if ((data != null) && (data.Length > 0))
                            {
                                for (int i = 0; i < data.Length; ++i)
                                {
                                    ValueDesc vx = new ValueDesc();
                                    vx.Index = i;
                                    String s = Convert.ToString(myconvert(data[i]));
                                    vx.DataStringValue = s;
                                    vals.Add(vx);
                                }// i
                            }// data
                        }
                        else if (t == typeof(float))
                        {
                            vv.DataType = "float";
                            vv.IsCategVar = false;
                            float[] data = input.GetData<float[]>(v.ID);
                            if ((data != null) && (data.Length > 0))
                            {
                                for (int i = 0; i < data.Length; ++i)
                                {
                                    ValueDesc vx = new ValueDesc();
                                    vx.Index = i;
                                    String s = Convert.ToString(myconvert(data[i]));
                                    vx.DataStringValue = s;
                                    vals.Add(vx);
                                }// i
                            }// data
                        }
                        else if (t == typeof(int))
                        {
                            vv.DataType = "int";
                            vv.IsCategVar = false;
                            int[] data = input.GetData<int[]>(v.ID);
                            if ((data != null) && (data.Length > 0))
                            {
                                for (int i = 0; i < data.Length; ++i)
                                {
                                    ValueDesc vx = new ValueDesc();
                                    vx.Index = i;
                                    String s = Convert.ToString(data[i]);
                                    vx.DataStringValue = s;
                                    vals.Add(vx);
                                }// i
                            }// data
                        }
                        else if (t == typeof(short))
                        {
                            vv.DataType = "short";
                            vv.IsCategVar = false;
                            short[] data = input.GetData<short[]>(v.ID);
                            if ((data != null) && (data.Length > 0))
                            {
                                for (int i = 0; i < data.Length; ++i)
                                {
                                    ValueDesc vx = new ValueDesc();
                                    vx.Index = i;
                                    String s = Convert.ToString(data[i]);
                                    vx.DataStringValue = s;
                                    vals.Add(vx);
                                }// i
                            }// data
                        }
                        else if (t == typeof(bool))
                        {
                            vv.DataType = "bool";
                            vv.IsCategVar = true;
                            bool[] data = input.GetData<bool[]>(v.ID);
                            if ((data != null) && (data.Length > 0))
                            {
                                for (int i = 0; i < data.Length; ++i)
                                {
                                    ValueDesc vx = new ValueDesc();
                                    vx.Index = i;
                                    String s = Convert.ToString(data[i]);
                                    vx.DataStringValue = s;
                                    vals.Add(vx);
                                }// i
                            }// data
                        }
                        vv.Values = vals;
                        oSet.Variables.Add(vv);
                    }// v
                }
                catch (Exception ex)
                {
                    err = ex;
                }
                return new Tuple<StatDataSet, Exception>(oSet, err);
            }, cancellationToken);
        }// readDataSet
        private async void buttonImport_Click(object sender, RoutedEventArgs e)
        {
            if (this.IsBusy)
            {
                return;
            }
            var pModel = getModel();
            if (pModel == null)
            {
                return;
            }
            var pMan = pModel.DataService;
            if (pMan == null)
            {
                return;
            }
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".tsv";
            dlg.Filter = "Tab Separated values (*.tsv)|*.tsv|Comma Separated values (*.csv)|*.csv|NetCDF files (*.nc)|*.nc|Tous les fichiers (*.*)|*.*";
            dlg.Multiselect = false;
            //
            Nullable<bool> result = dlg.ShowDialog();
            if ((result != null) && result.HasValue && (result.Value == true))
            {
                try
                {
                    this.IsBusy = true;
                    myUpdateUI(false);
                    this.progressbar.Visibility = Visibility.Visible;
                    this.buttonCancel.IsEnabled = true;
                    var progress = new Progress<int>((int ival) =>
                    {
                        progressbar.Value = ival;
                    });
                    m_cts = new CancellationTokenSource();
                    String filename = dlg.FileName;
                    var uri = sds.DataSetUri.Create(filename);
                    if (!uri.ContainsParameter("openMode"))
                    {
                        uri.OpenMode = sds.ResourceOpenMode.ReadOnly;
                    }
                    var input = sds.DataSet.Open(uri);
                    this.progressbar.Maximum = input.Variables.Count;

                    var yy = await readDataSet(filename, input, m_cts.Token, progress);
                    if ((yy.Item1 != null) && (yy.Item2 == null) && (!m_cts.IsCancellationRequested))
                    {
                        StatDataSet oSet = yy.Item1;
                        this.progressbar.Maximum = 100;
                        this.progressbar.Value = 0;
                        var xx = await Task.Run<Tuple<bool, Exception>>(() =>
                        {
                            return pMan.ReplaceDataSet(oSet, m_cts.Token, progress);
                        }, m_cts.Token);
                        m_cts = null;
                        this.progressbar.Visibility = Visibility.Hidden;
                        myUpdateUI(true);
                        this.IsBusy = false;
                        this.buttonCancel.IsEnabled = false;
                        if ((xx.Item1) && (xx.Item2 == null))
                        {
                            pModel.StatDataSets = null;
                            pModel.RefreshDataSets();
                        }
                        else if (xx.Item2 != null)
                        {
                            pModel.ShowError(xx.Item2);
                        }
                    }
                    else if (yy.Item2 != null)
                    {
                        m_cts = null;
                        this.progressbar.Visibility = Visibility.Hidden;
                        myUpdateUI(true);
                        this.IsBusy = false;
                        pModel.ShowError(yy.Item2);
                    }
                }// try
                catch (Exception ex)
                {
                    m_cts = null;
                    this.progressbar.Visibility = Visibility.Hidden;
                    myUpdateUI(true);
                    this.IsBusy = false;
                    pModel.ShowError(ex);
                }
            }// result
        }
        private async void buttonExport_Click(object sender, RoutedEventArgs e)
        {
            if (this.IsBusy)
            {
                return;
            }
            var pModel = getModel();
            if (pModel == null)
            {
                return;
            }
            StatDataSet oSet = pModel.CurrentStatDataSet;
            if (oSet == null)
            {
                return;
            }
            var pMan = pModel.DataService;
            if (pMan == null)
            {
                return;
            }
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".tsv";
            dlg.Filter = "Tab Separated values (*.tsv)|*.tsv";
            //
            Nullable<bool> result = dlg.ShowDialog();
            if ((result != null) && result.HasValue && (result.Value == true))
            {
                try
                {
                    this.IsBusy = true;
                    myUpdateUI(false);
                    this.progressbar.Visibility = Visibility.Visible;
                    this.buttonCancel.IsEnabled = true;
                    var progress = new Progress<int>((int ival) =>
                    {
                        progressbar.Value = ival;
                    });
                    m_cts = new CancellationTokenSource();
                    String filename = dlg.FileName;
                    this.progressbar.Maximum = 100;
                    var xx = await exportTSV(pMan, oSet, filename, m_cts.Token, progress);
                    m_cts = null;
                    this.progressbar.Visibility = Visibility.Hidden;
                    myUpdateUI(true);
                    this.IsBusy = false;
                    if (xx.Item2 != null)
                    {
                        pModel.ShowError(xx.Item2);
                    }
                }// try
                catch (Exception ex)
                {
                    m_cts = null;
                    this.progressbar.Visibility = Visibility.Hidden;
                    myUpdateUI(true);
                    this.IsBusy = false;
                    pModel.ShowError(ex);
                }
            }// result
        }
        private Task<Tuple<bool, Exception>> exportTSV(IStoreDataManager pMan, StatDataSet oSet, String filename,
            CancellationToken cancellationToken, IProgress<int> progress)
        {
            return Task.Run<Tuple<bool, Exception>>(() =>
            {
                bool bRet = false;
                Exception err = null;
                try
                {
                    var xx = pMan.GetDataSetIndexes(oSet);
                    if (xx == null)
                    {
                        return new Tuple<bool, Exception>(bRet, err);
                    }
                    var indexes = xx.Item1;
                    if (indexes == null)
                    {
                        return new Tuple<bool, Exception>(bRet, err);
                    }
                    int nMaxIndex = indexes.Max() + 1;
                    int nm1 = nMaxIndex - 1;
                    double dTotal = (double)nMaxIndex;
                    StringBuilder sb = new StringBuilder();
                    int nCur = 0;
                    var varsCol = oSet.Variables.ToArray();
                    int nVars = varsCol.Length;
                    for (int i = 0; i < nVars; ++i)
                    {
                        if (i > 0)
                        {
                            sb.Append("\t");
                        }
                        sb.Append(varsCol[i].Name);
                    }// i
                    sb.Append("\n");
                    if (progress != null)
                    {
                        int f = (int)((nCur++ / dTotal) * 100.0 + 0.5);
                        progress.Report(f);
                    }
                    for (int irow = 0; irow < nMaxIndex; ++irow)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }
                        String[] vals = new String[nVars];
                        for (int i = 0; i < nVars; ++i)
                        {
                            vals[i] = "N/A";
                        }// i
                        if (indexes.Contains(irow))
                        {
                            var yy = pMan.GetDataSetIndivValues(oSet, irow);
                            if ((yy != null) && (yy.Item1 != null) && (yy.Item2 == null))
                            {
                                var xvals = yy.Item1;
                                for (int j = 0; j < nVars; ++j)
                                {
                                    int ind = varsCol[j].Id;
                                    var qq = from x in xvals where x.VariableId == ind select x;
                                    if (qq.Count() > 0)
                                    {
                                        var z = StatHelpers.ConvertValue(qq.First().DataStringValue);
                                        if (!String.IsNullOrEmpty(z))
                                        {
                                            vals[j] = z;
                                        }
                                    }// ok
                                }// j
                            }// ok
                        }
                        for (int i = 0; i < nVars; ++i)
                        {
                            if (i > 0)
                            {
                                sb.Append("\t");
                            }
                            sb.Append(vals[i]);
                        }// i
                        if (irow < nm1)
                        {
                            sb.Append("\n");
                        }
                        if (progress != null)
                        {
                            int f = (int)((nCur++ / dTotal) * 100.0 + 0.5);
                            progress.Report(f);
                        }
                    }// irow
                    String s = sb.ToString();
                    String ss = s.Replace(",", ".");
                    FileStream fs = new FileStream(filename, FileMode.OpenOrCreate);
                    StreamWriter writer = new StreamWriter(fs);
                    writer.Write(ss);
                    writer.Close();
                    bRet = true;
                }// try
                catch (Exception ex)
                {
                    err = ex;
                }
                return new Tuple<bool, Exception>(bRet, err);
            }, cancellationToken);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            myUpdateUI(true);
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
                    myUpdateUI(true);
                }
            }
        }// ekportTSV
    }
}
