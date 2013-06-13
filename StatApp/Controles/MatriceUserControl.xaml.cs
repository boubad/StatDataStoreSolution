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
using System.Collections.ObjectModel;
using StatData;
namespace StatApp.Controles
{
    public class MatSortItem : IComparable
    {
        private int m_index;
        private double m_val;
        public MatSortItem()
        {
        }
        public MatSortItem(int index, double val)
        {
            m_index = index;
            m_val = val;
        }
        public int Index
        {
            get { return m_index; }
        }
        public double Value
        {
            get { return m_val; }
        }
        public override bool Equals(object obj)
        {
            bool bRet = false;
            if ((obj != null) && (obj is MatSortItem))
            {
                MatSortItem p = obj as MatSortItem;
                bRet = (this.Value == p.Value);
            }
            return bRet;
        }
        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }
        #region IComparable Members
        public int CompareTo(object obj)
        {
            int nRet = -1;
            if ((obj != null) && (obj is MatSortItem))
            {
                MatSortItem p = obj as MatSortItem;
                if (this.Value > p.Value)
                {
                    nRet = 1;
                }
                else if (this.Value == p.Value)
                {
                    nRet = 0;
                }
            }
            return nRet;
        }
        #endregion
    }// class MatSortItem 
    /// <summary>
    /// Logique d'interaction pour MatriceUserControl.xaml
    /// </summary>
    public partial class MatriceUserControl : UserControl
    {
        //
        #region static variables
        private static String[] TAB_MODES_STRINGS = { "PROFIL", "NORMALIZE", "NOTHING" };
        private static MatriceComputeMode[] TAB_MODES_ENUMS = { MatriceComputeMode.modeProfil,
                                                              MatriceComputeMode.modeNormalize,
                                                              MatriceComputeMode.modeNothing};
        #endregion // static variables
        #region static helpers
        private static void onOrdModelViewChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            MatriceUserControl p = obj as MatriceUserControl;
            if (!p.IsBusy)
            {
                p.IsBusy = true;
                p.myChangeOrdViewModel();
                p.IsBusy = false;
            }
        }// onIndexChanged
        private static void onIndexChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            MatriceUserControl p = obj as MatriceUserControl;
            if (!p.IsBusy)
            {
                p.IsBusy = true;
                p.myChangeIndexes();
                p.IsBusy = false;
            }
        }// onIndexChanged
        private static void onModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            MatriceUserControl p = obj as MatriceUserControl;
            if (p.IsBusy)
            {
                p.IsBusy = true;
                p.myChangeMode();
                p.IsBusy = false;
            }
        }// onModeChanged
        private static void onSummaryChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            MatriceUserControl p = obj as MatriceUserControl;
            if (!p.IsBusy)
            {
                p.IsBusy = true;
                p.myChangeSumary();
                p.IsBusy = false;
            }
        }// onSummaryChanged
        private static void onSummaryDataChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            MatriceUserControl p = obj as MatriceUserControl;
            if (!p.IsBusy)
            {
                p.IsBusy = true;
                p.myChangeSumaryData();
                p.IsBusy = false;
            }
        }// onSummaryDataChanged
        private static void onInitialDataChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            MatriceUserControl p = obj as MatriceUserControl;
            if (!p.IsBusy)
            {
                p.IsBusy = true;
                p.myChangeInitialData();
                p.IsBusy = false;
            }
        }// onInitialDataChanged
        private static void onClassesChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            MatriceUserControl p = obj as MatriceUserControl;
            if (!p.IsBusy)
            {
                p.IsBusy = true;
                p.myChangeClasses();
                p.IsBusy = false;
            }
        }// onClassesChanged
        private static void onDisplayChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            MatriceUserControl p = obj as MatriceUserControl;
            if (!p.IsBusy)
            {
                p.IsBusy = true;
                p.myChangeDisplay();
                p.IsBusy = false;
            }
        }// onDisplayChanged
        #endregion // static helpers
        #region MatriceDisplayMode Property
        public static readonly DependencyProperty MatriceDisplayModeProperty =
            DependencyProperty.Register("MatriceDisplayMode",
            typeof(MatDisplayMode),
            typeof(MatriceUserControl),
            new UIPropertyMetadata(MatDisplayMode.modeHistog,
                new PropertyChangedCallback(onDisplayChanged)));
        public MatDisplayMode MatriceDisplayMode
        {
            get { return (MatDisplayMode)GetValue(MatriceDisplayModeProperty); }
            set { SetValue(MatriceDisplayModeProperty, value); }
        }//MatriceDisplayMode
        #endregion
        #region MatriceScaleMode Property
        public static readonly DependencyProperty MatriceScaleModeProperty =
            DependencyProperty.Register("MatriceScaleMode",
            typeof(MatScaleMode),
            typeof(MatriceUserControl),
            new UIPropertyMetadata(MatScaleMode.scaleOwn,
                new PropertyChangedCallback(onClassesChanged)));
        public MatScaleMode MatriceScaleMode
        {
            get { return (MatScaleMode)GetValue(MatriceScaleModeProperty); }
            set { SetValue(MatriceScaleModeProperty, value); }
        }//MatriceScaleMode
        #endregion
        #region Dependency Properties
        #region MatItemHeight Property
        public static readonly DependencyProperty MatItemHeightProperty =
            DependencyProperty.Register("MatItemHeight",
            typeof(int),
            typeof(MatriceUserControl),
            new UIPropertyMetadata((int)40));
        public int MatItemHeight
        {
            get { return (int)GetValue(MatItemHeightProperty); }
            set { SetValue(MatItemHeightProperty, value); }
        }//MatItemHeight
        #endregion
        #region MatItemWidth Property
        public static readonly DependencyProperty MatItemWidthProperty =
            DependencyProperty.Register("MatItemWidth",
            typeof(int),
            typeof(MatriceUserControl),
            new UIPropertyMetadata((int)40));
        public int MatItemWidth
        {
            get { return (int)GetValue(MatItemWidthProperty); }
            set { SetValue(MatItemWidthProperty, value); }
        }//MatItemWidth
        #endregion
        #region VarDelta Property
        public static readonly DependencyProperty VarDeltaProperty =
            DependencyProperty.Register("VarDelta",
            typeof(int),
            typeof(MatriceUserControl),
            new UIPropertyMetadata((int)1));
        public int VarDelta
        {
            get { return (int)GetValue(VarDeltaProperty); }
            set { SetValue(VarDeltaProperty, value); }
        }//VarDelta
        #endregion
        #region IndDelta Property
        public static readonly DependencyProperty IndDeltaProperty =
            DependencyProperty.Register("IndDelta",
            typeof(int),
            typeof(MatriceUserControl),
            new UIPropertyMetadata((int)2));
        public int IndDelta
        {
            get { return (int)GetValue(IndDeltaProperty); }
            set { SetValue(IndDeltaProperty, value); }
        }//IndDelta
        #endregion
        //
        #region ClassesCount Property
        public static readonly DependencyProperty ClassesCountProperty =
            DependencyProperty.Register("ClassesCount",
            typeof(int),
            typeof(MatriceUserControl),
            new UIPropertyMetadata((int)5,
                new PropertyChangedCallback(onClassesChanged)));
        public int ClassesCount
        {
            get { return (int)GetValue(ClassesCountProperty); }
            set
            {
                int nc = value;
                if (nc < 1)
                {
                    nc = 5;
                }
                if ((nc % 2) == 0)
                {
                    ++nc;
                }
                SetValue(ClassesCountProperty, nc);
            }
        }//ClassesCount
        #endregion
        #region Classes Property
        public static readonly DependencyProperty ClassesProperty =
            DependencyProperty.Register("Classes",
            typeof(int[,]),
            typeof(MatriceUserControl),
            new UIPropertyMetadata(new int[0, 0]));
        public int[,] Classes
        {
            get
            {
                int[,] oRet = (int[,])GetValue(ClassesProperty);
                if (oRet == null)
                {
                    oRet = new int[0, 0];
                }
                return oRet;
            }
            set { SetValue(ClassesProperty, value); }
        }//ClassesCount
        #endregion
        //
        #region SummaryVarNames Property
        public static readonly DependencyProperty SummaryVarNamesProperty =
            DependencyProperty.Register("SummaryVarNames",
            typeof(IEnumerable<String>),
            typeof(MatriceUserControl),
            new UIPropertyMetadata(new List<String>()));
        public IEnumerable<String> SummaryVarNames
        {
            get
            {
                IEnumerable<String> oRet = (IEnumerable<String>)GetValue(SummaryVarNamesProperty);
                if (oRet == null)
                {
                    oRet = new List<string>();
                }
                return oRet;
            }
            set { SetValue(SummaryVarNamesProperty, value); }
        }//SummaryVarNames
        #endregion
        #region SummaryData Property
        public static readonly DependencyProperty SummaryDataProperty =
            DependencyProperty.Register("SummaryData",
            typeof(Dictionary<String, double[]>),
            typeof(MatriceUserControl),
            new UIPropertyMetadata(new Dictionary<String, double[]>(),
                new PropertyChangedCallback(onSummaryDataChanged)));
        public Dictionary<String, double[]> SummaryData
        {
            get
            {
                Dictionary<String, double[]> oRet = (Dictionary<String, double[]>)GetValue(SummaryDataProperty);
                if (oRet == null)
                {
                    oRet = new Dictionary<String, double[]>();
                }
                return oRet;
            }
            set { SetValue(SummaryDataProperty, value); }
        }//SummaryData
        #endregion
        #region MatriceMode Property
        public static readonly DependencyProperty MatriceModeProperty =
            DependencyProperty.Register("MatriceMode",
            typeof(MatriceComputeMode),
            typeof(MatriceUserControl),
            new UIPropertyMetadata(MatriceComputeMode.modeNothing,
                new PropertyChangedCallback(onModeChanged)));
        public MatriceComputeMode MatriceMode
        {
            get { return (MatriceComputeMode)GetValue(MatriceModeProperty); }
            set { SetValue(MatriceModeProperty, value); }
        }//MatriceMode
        #endregion
        #region HasSummary Property
        public static readonly DependencyProperty HasSummaryProperty =
            DependencyProperty.Register("HasSummary",
            typeof(bool),
            typeof(MatriceUserControl),
            new UIPropertyMetadata(false,
                new PropertyChangedCallback(onSummaryChanged)));
        public bool HasSummary
        {
            get { return (bool)GetValue(HasSummaryProperty); }
            set { SetValue(HasSummaryProperty, value); }
        }//HasSummary
        #endregion
        #region HasOwnScale Property
        public static readonly DependencyProperty HasOwnScaleProperty =
            DependencyProperty.Register("HasOwnScale",
            typeof(bool),
            typeof(MatriceUserControl),
            new UIPropertyMetadata(false,
                new PropertyChangedCallback(onClassesChanged)));
        public bool HasOwnScale
        {
            get { return (bool)GetValue(HasOwnScaleProperty); }
            set { SetValue(HasOwnScaleProperty, value); }
        }//HasOwnScale
        #endregion
        #region ShowIndivs Property
        public static readonly DependencyProperty ShowIndivsProperty =
            DependencyProperty.Register("ShowIndivs",
            typeof(bool),
            typeof(MatriceUserControl),
            new UIPropertyMetadata(false,
                new PropertyChangedCallback(onIndexChanged)));
        public bool ShowIndivs
        {
            get { return (bool)GetValue(ShowIndivsProperty); }
            set { SetValue(ShowIndivsProperty, value); }
        }//ShowIndivs
        #endregion
        #region Summary Property
        public static readonly DependencyProperty SummaryProperty =
            DependencyProperty.Register("Summary",
            typeof(double[]),
            typeof(MatriceUserControl),
            new UIPropertyMetadata(new double[0],
                new PropertyChangedCallback(onSummaryChanged)));
        public double[] Summary
        {
            get { return (double[])GetValue(SummaryProperty); }
            set { SetValue(SummaryProperty, value); }
        }//Summary
        #endregion
        #region InitialData Property
        public static readonly DependencyProperty InitialDataProperty =
            DependencyProperty.Register("InitialData",
            typeof(double[,]),
            typeof(MatriceUserControl),
            new UIPropertyMetadata(new double[0, 0],
                new PropertyChangedCallback(onInitialDataChanged)));
        public double[,] InitialData
        {
            get
            {
                double[,] oRet = (double[,])GetValue(InitialDataProperty);
                if (oRet == null)
                {
                    oRet = new double[0, 0];
                }
                return oRet;
            }
            set { SetValue(InitialDataProperty, value); }
        }//InitialData
        #endregion
        #region VarNames Property
        public static readonly DependencyProperty VarNamesProperty =
            DependencyProperty.Register("VarNames",
            typeof(IEnumerable<String>),
            typeof(MatriceUserControl),
            new UIPropertyMetadata(new List<String>()));
        public IEnumerable<String> VarNames
        {
            get
            {
                IEnumerable<String> oRet = (IEnumerable<String>)GetValue(VarNamesProperty);
                if (oRet == null)
                {
                    oRet = new List<String>();
                }
                double[,] data = this.InitialData;
                int n = oRet.Count();
                int nx = data.GetLength(1);
                if (n < nx)
                {
                    List<String> oList = new List<String>();
                    for (int i = 0; i < nx; ++i)
                    {
                        oList.Add(String.Format("V{0}", i + 1));
                    }// i
                    oRet = oList;
                }// n
                return oRet;
            }
            set { SetValue(VarNamesProperty, value); }
        }//VarNames
        #endregion
        #region IndNames Property
        public static readonly DependencyProperty IndNamesProperty =
            DependencyProperty.Register("IndNames",
            typeof(IEnumerable<String>),
            typeof(MatriceUserControl),
            new UIPropertyMetadata(new List<String>()));
        public IEnumerable<String> IndNames
        {
            get
            {
                IEnumerable<String> oRet = (IEnumerable<String>)GetValue(IndNamesProperty);
                if (oRet == null)
                {
                    oRet = new List<String>();
                }
                double[,] data = this.InitialData;
                int n = oRet.Count();
                int nx = data.GetLength(0);
                if (n < nx)
                {
                    List<String> oList = new List<String>();
                    for (int i = 0; i < nx; ++i)
                    {
                        oList.Add(String.Format("I{0}", i + 1));
                    }// i
                    oRet = oList;
                }// n
                return oRet;
            }
            set { SetValue(IndNamesProperty, value); }
        }//IndNames
        #endregion
        //
        #region ColIndex Property
        public static readonly DependencyProperty ColIndexProperty =
            DependencyProperty.Register("ColIndex",
            typeof(int[]),
            typeof(MatriceUserControl),
            new UIPropertyMetadata(new int[0],
                new PropertyChangedCallback(onIndexChanged)));
        public int[] ColIndex
        {
            get { return (int[])GetValue(ColIndexProperty); }
            set
            {
                this.m_colcells = null;
                SetValue(ColIndexProperty, value);
            }
        }//ColIndex
        #endregion
        #region RowIndex Property
        public static readonly DependencyProperty RowIndexProperty =
            DependencyProperty.Register("RowIndex",
            typeof(int[]),
            typeof(MatriceUserControl),
            new UIPropertyMetadata(new int[0],
                new PropertyChangedCallback(onIndexChanged)));
        public int[] RowIndex
        {
            get { return (int[])GetValue(RowIndexProperty); }
            set
            {
                this.m_rowcells = null;
                SetValue(RowIndexProperty, value);
            }
        }//RowIndex
        #endregion
        #region OrdModelView Property
        public static readonly DependencyProperty OrdModelViewProperty =
             DependencyProperty.Register("OrdModelView",
             typeof(OrdModelView),
             typeof(MatriceUserControl),
             new UIPropertyMetadata((OrdModelView)null,
                 new PropertyChangedCallback(onOrdModelViewChanged)));
        public OrdModelView OrdModelView
        {
            get { return (OrdModelView)GetValue(OrdModelViewProperty); }
            set
            {
                SetValue(OrdModelViewProperty, value);
            }
        }//OrdModelView
        #endregion // StatIndiv
        #endregion // Dependencey Properties
        #region Instance Variables
        private OrdModelView m_ordmodelview;
        private IndivDatas m_indivs;
        private bool m_busy = false;
        private List<MatCellItems> m_colcells;
        private List<MatCellItems> m_rowcells;
        private CancellationTokenSource m_cts;
        private MatCellItem[] m_varcells;
        private MatCellItem[] m_indivcells;
        private MatCellItem[,] m_datacells;
        private MatCellItem[] m_refclasses;
        private MatCellItem m_defaultitem;
        #endregion
        public MatriceUserControl()
        {
            m_busy = true;
            m_ordmodelview = null;
            m_indivs = null;
            m_colcells = null;
            m_rowcells = null;
            m_refclasses = null;
            m_cts = null;
            m_varcells = null;
            m_indivcells = null;
            m_datacells = null;
            m_defaultitem = null;
            InitializeComponent();
            m_busy = false;
        }
        #region Properties
        public bool IsBusy
        {
            get
            {
                return m_busy;
            }
            set
            {
                if (m_busy != value)
                {
                    m_busy = value;
                }
            }
        }// isBusy
        #endregion // Properties
        #region instance helpers
        private void mySetSummary()
        {
            Dictionary<String, double[]> dict = null;
            String[] sumnames = null;
            OrdModelView model = this.OrdModelView;
            if (model != null)
            {
                var oVars = model.AllNumVariables.ToArray();
                var inds = model.Individus.ToArray();
                int nv = oVars.Length;
                int nr = inds.Length;
                dict = new Dictionary<string, double[]>();
                sumnames = new String[nv];
                for (int i = 0; i < nv; ++i)
                {
                    var oVar = oVars[i];
                    String name = oVar.Name;
                    double[] dd = new double[nr];
                    sumnames[i] = name;
                    int nVarId = oVar.Id;
                    for (int j = 0; j < nr; ++j)
                    {
                        var ind = inds[j];
                        var q = from x in ind.Values where x.VariableId == nVarId select x;
                        if (q.Count() > 0)
                        {
                            var vv = q.First();
                            dd[j] = vv.DoubleValue;
                        }
                    }// j
                    dict[name] = dd;
                }// i
            }// model
            this.SummaryVarNames = sumnames;
            this.SummaryData = dict;
        }// mySetSummary
        private void myChangeModelData()
        {
            try
            {
                int nv = 0;
                int nr = 0;
                double[,] doubledata = null;
                int[,] intdata = null;
                string[] colnames = null;
                string[] rownames = null;
                OrdModelView model = this.OrdModelView;
                m_indivs = null;
               
                if (model != null)
                {
                    m_indivs = model.Individus;
                    var oVars = model.CurrentVariables.ToArray();
                    nv = oVars.Length;
                    nr = m_indivs.Count;
                    doubledata = new double[nr, nv];
                    intdata = new int[nr, nv];
                    var oInds = m_indivs.ToArray();
                    rownames = new String[nr];
                    colnames = new String[nv];
                    for (int i = 0; i < nv; ++i)
                    {
                        var v = oVars[i];
                        colnames[i] = v.Name;
                    }// i
                    for (int i = 0; i < nr; ++i)
                    {
                        var v = oInds[i];
                        rownames[i] = v.Name;
                        double[] dd = v.DoubleData;
                        int nx = (dd.Length < nv) ? dd.Length : nv;
                        for (int j = 0; j < nx; ++j)
                        {
                            doubledata[i, j] = dd[j];
                        }// j
                        int[] zz = v.IntData;
                        nx = (zz.Length < nv) ? zz.Length : nv;
                        for (int j = 0; j < nx; ++j)
                        {
                            intdata[i, j] = zz[j];
                        }
                    }// i
                }// model
                this.InitialData = doubledata;
                this.Classes = intdata;
                this.VarNames = colnames;
                this.IndNames = rownames;
                mySetSummary();
                myChangeData();
            }
            catch (Exception /*ex */)
            {
            }
        }// myChangeModelData
        private void myChangeOrdViewModel()
        {
            m_ordmodelview = this.OrdModelView;
            if (m_ordmodelview != null)
            {
                m_ordmodelview.PropertyChanged +=m_ordmodelview_PropertyChanged;
            }
            myChangeModelData();
        }
        void m_ordmodelview_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (m_busy)
            {
                return;
            }
            m_busy = true;
            String name = e.PropertyName;
            if (name == "Individus")
            {
                myChangeModelData();
            }
            else if (name == "AllNumVariables")
            {
                mySetSummary();
            }
            m_busy = false;
        }
        private void myChangeData()
        {
            m_colcells = null;
            m_rowcells = null;
            m_refclasses = null;
            double[,] data = this.InitialData;
            int width = this.MatItemWidth;
            int height = this.MatItemHeight;
            String[] varnames = this.VarNames.ToArray();
            String[] indnames = this.IndNames.ToArray();
            int nr = data.GetLength(0);
            int nv = data.GetLength(1);
            int[] colindex = new int[nv];
            m_varcells = new MatCellItem[nv];
            m_datacells = new MatCellItem[nr, nv];
            for (int i = 0; i < nv; ++i)
            {
                colindex[i] = i;
                MatCellItem t = new MatCellItem();
                t.Text = varnames[i];
                t.TotalHeight = height;
                t.TotalWidth = width;
                m_varcells[i] = t;
                for (int j = 0; j < nr; ++j)
                {
                    MatCellItem tt = new MatCellItem();
                    tt.TotalHeight = height;
                    tt.TotalWidth = width;
                    tt.CellType = MatItemType.typeValInf;
                    m_datacells[j, i] = tt;
                }// j
            }// i
            this.ColIndex = colindex;
            int[] rowindex = new int[nr];
            m_indivcells = new MatCellItem[nr];
            m_refclasses = new MatCellItem[nr];
            for (int i = 0; i < nr; ++i)
            {
                rowindex[i] = i;
                MatCellItem t = new MatCellItem();
                t.Text = indnames[i];
                t.TotalHeight = height;
                t.TotalWidth = width;
                m_indivcells[i] = t;
                MatCellItem tt = new MatCellItem();
                tt.CellType = MatItemType.typeSummary;
                tt.TotalHeight = height;
                tt.TotalWidth = width;
                m_refclasses[i] = tt;
            }// i
            this.RowIndex = rowindex;
            this.myChangeClasses();
        }// myChangeData
        private List<MatCellItems> getVariablesCells()
        {
            List<MatCellItems> cells = new List<MatCellItems>();
            if ((m_varcells == null) || (m_indivcells == null) || (m_datacells == null))
            {
                return cells;
            }
            var ff = createVariableLine();
            if (ff != null)
            {
                cells.Add(ff);
            }
            int[] colindex = this.ColIndex;
            int[] rowindex = this.RowIndex;
            double[,] xdata = this.InitialData;
            int nr = xdata.GetLength(0);
            int nv = xdata.GetLength(1);
            if ((nr > 0) && (nv > 0) && (colindex.Length >= nv) && (rowindex.Length >= nr))
            {
                MatDisplayMode mode = this.MatriceDisplayMode;
                int width = this.MatItemWidth / 2;
                if (width < 10)
                {
                    width = 10;
                }
                int height = this.MatItemHeight;
                MatCellItems cur = new MatCellItems();
                if (m_defaultitem == null)
                {
                    MatCellItem tt = new MatCellItem();
                    tt.TotalHeight = height;
                    tt.TotalWidth = this.MatItemWidth;
                    m_defaultitem = tt;
                }
                cur.Add(m_defaultitem);
                for (int i = 0; i < nr; ++i)
                {
                    int irow = rowindex[i];
                    MatCellItem item = m_indivcells[irow];
                    item.CellType = MatItemType.typeVar;
                    item.TotalWidth = width;
                    cur.Add(item);
                }// i
                cells.Add(cur);
                //
                for (int i = 0; i < nv; ++i)
                {
                    MatCellItems line = new MatCellItems();
                    //
                    int icol = colindex[i];
                    MatCellItem tx = m_varcells[icol];
                    tx.CellType = MatItemType.typeInd;
                    tx.TotalWidth = this.MatItemWidth;
                    line.Add(tx);
                    //
                    for (int j = 0; j < nr; ++j)
                    {
                        int irow = rowindex[j];
                        MatCellItem item = m_datacells[irow, icol];
                        item.DisplayMode = mode;
                        item.TotalWidth = width;
                        item.DisplayMode = mode;
                        item.TotalWidth = width;
                        line.Add(item);
                    }// j
                    //
                    cells.Add(line);
                }// i
            }// nr && nv
            return cells;
        }// getVariablesCells
        private MatCellItems createIndivsLine()
        {
            if (m_defaultitem == null)
            {
                MatCellItem tt = new MatCellItem();
                tt.TotalHeight = this.MatItemHeight;
                tt.TotalWidth = this.MatItemWidth;
                m_defaultitem = tt;
            }
            double[,] xdata = this.InitialData;
            int nv = xdata.GetLength(1);
            if (nv < 1)
            {
                return null;
            }
            var line = new MatCellItems();
            MatCellItem tx = m_defaultitem;
            line.Add(tx);
            //
            for (int j = 0; j < nv; ++j)
            {
                line.Add(tx);
            }// j
            if (this.HasSummary && (m_refclasses != null))
            {
                line.Add(tx);
            }
            //
            return line;
        }// createIndivLine
        private MatCellItems createVariableLine()
        {
            if (m_defaultitem == null)
            {
                MatCellItem tt = new MatCellItem();
                tt.TotalHeight = this.MatItemHeight;
                tt.TotalWidth = this.MatItemWidth;
                m_defaultitem = tt;
            }
            double[,] xdata = this.InitialData;
            int nr = xdata.GetLength(0);
            if (nr < 1)
            {
                return null;
            }
            var line = new MatCellItems();
            MatCellItem tx = m_defaultitem;
            line.Add(tx);
            //
            for (int j = 0; j < nr; ++j)
            {
                line.Add(tx);
            }// j
            //
            return line;
        }// createVariableLine
        private List<MatCellItems> getIndivsCells()
        {
            List<MatCellItems> cells = new List<MatCellItems>();
            if ((m_varcells == null) || (m_indivcells == null) || (m_datacells == null))
            {
                return cells;
            }
            var ff = createIndivsLine();
            if (ff != null)
            {
                cells.Add(ff);
            }
            if (m_defaultitem == null)
            {
                MatCellItem tt = new MatCellItem();
                tt.TotalHeight = this.MatItemHeight;
                tt.TotalWidth = this.MatItemWidth;
                m_defaultitem = tt;
            }
            int[] colindex = this.ColIndex;
            int[] rowindex = this.RowIndex;
            double[,] xdata = this.InitialData;
            int nr = xdata.GetLength(0);
            int nv = xdata.GetLength(1);
            if ((nr > 0) && (nv > 0) && (colindex.Length >= nv) && (rowindex.Length >= nr))
            {
                int width = this.MatItemWidth;
                if (width < 10)
                {
                    width = 10;
                }
                MatDisplayMode mode = this.MatriceDisplayMode;
                MatCellItems cur = new MatCellItems();
                cur.Add(m_defaultitem);
                for (int i = 0; i < nv; ++i)
                {
                    int icol = colindex[i];
                    MatCellItem item = m_varcells[icol];
                    item.CellType = MatItemType.typeVar;
                    item.TotalWidth = width;
                    cur.Add(item);
                }// i
                cells.Add(cur);
                //
                for (int i = 0; i < nr; ++i)
                {
                    MatCellItems line = new MatCellItems();
                    //
                    int irow = rowindex[i];
                    MatCellItem tx = m_indivcells[irow];
                    tx.CellType = MatItemType.typeInd;
                    tx.TotalWidth = width;
                    line.Add(tx);
                    //
                    for (int j = 0; j < nv; ++j)
                    {
                        int icol = colindex[j];
                        MatCellItem item = m_datacells[irow, icol];
                        item.DisplayMode = mode;
                        item.TotalWidth = width;
                        line.Add(item);
                    }// j
                    if (this.HasSummary && (m_refclasses != null))
                    {
                        line.Add(m_defaultitem);
                        MatCellItem item = m_refclasses[irow];
                        item.TotalWidth = width;
                        line.Add(item);
                    }
                    //
                    cells.Add(line);
                }// i
            }// nr && nv
            return cells;
        }// getIndivCells
        private void myChangeIndexes()
        {
            int[] colindex = this.ColIndex;
            int[] rowindex = this.RowIndex;
            if ((colindex == null) || (rowindex == null))
            {
                return;
            }
            List<MatCellItems> oList = null;
            if (this.ShowIndivs)
            {

                if (m_rowcells == null)
                {
                    m_rowcells = getIndivsCells();
                }
                oList = m_rowcells;
            }
            else
            {
                if (m_colcells == null)
                {
                    m_colcells = getVariablesCells();
                }
                oList = m_colcells;
            }
            if (this.myLst != null)
            {
                this.myLst.ItemsSource = null;
                this.myLst.ItemsSource = oList;
            }
        }// myChangeIndexes
        private void myChangeMode()
        {
            this.myChangeClasses();
        }// myChangeMode
        private void myChangeSumary()
        {
            int[] nRef = null;
            double[,] data = this.InitialData;
            int nr = data.GetLength(0);
            if (this.HasSummary)
            {
                double[] vRef = null;
                vRef = this.Summary;
                if (vRef != null)
                {
                    if (vRef.Length < nr)
                    {
                        vRef = null;
                    }
                }
                if (vRef != null)
                {
                    double vMin = vRef.Min();
                    double vMax = vRef.Max();
                    if (vMin < vMax)
                    {
                        double mean = vRef.Average();
                        double[] fLimits = new double[this.ClassesCount + 1];
                        if (GenRout.ComputeClassesLimits(this.ClassesCount, vMin, vMax, mean, fLimits))
                        {
                            nRef = new int[nr];
                            GenRout.ComputeClasses(vRef, fLimits, nRef);
                        }
                    }// vMin
                }// vRef
                if ((nRef != null) && (m_refclasses != null))
                {
                    for (int i = 0; i < nr; ++i)
                    {
                        m_refclasses[i].MaxValue = this.ClassesCount;
                        m_refclasses[i].CurrentValue = nRef[i];
                    }// i
                }// nRef
            }// HasSumary
            //
            myChangeIndexes();
        }// myChangeSumary
        private void myChangeSumaryData()
        {
            Dictionary<String, double[]> dict = this.SummaryData;
            if (dict == null)
            {
                dict = new Dictionary<string, double[]>();
            }
            ObservableCollection<String> col = new ObservableCollection<string>(dict.Keys);
            this.SummaryVarNames = col;
            if (this.comboBoxSummary != null)
            {
                this.comboBoxSummary.ItemsSource = col;
            }
        }// myChangeSumaryData
        private void myChangeInitialData()
        {
            myChangeData();
        }// myChangeInitialData
        private void myChangeClasses()
        {
            double[,] data = this.InitialData;
            if (data == null)
            {
                data = new double[0, 0];
            }
            int nr = data.GetLength(0);
            int nv = data.GetLength(1);
            if ((nr > 0) && (nv > 0))
            {
                int[,] classes = new int[nr, nv];
                bool bCommon = (this.MatriceScaleMode == MatScaleMode.scaleCommon) ? true : false;
                int nc = this.ClassesCount;
                GenRout.ComputeClasses(data, nc, classes, bCommon);
                if (m_refclasses != null)
                {
                    for (int i = 0; i < nr; ++i)
                    {
                        m_refclasses[i].MaxValue = nc;
                    }
                }
                if (m_datacells != null)
                {
                    for (int i = 0; i < nv; ++i)
                    {
                        for (int j = 0; j < nr; ++j)
                        {
                            m_datacells[j, i].MaxValue = nc;
                            m_datacells[j, i].CurrentValue = classes[j, i];
                        }// j
                    }// i
                }// m_dataCelles
                this.Classes = classes;
            }
            m_rowcells = null;
            m_colcells = null;
            this.myChangeIndexes();
        }// myChangeClasses
        private void myChangeDisplay()
        {
            this.myChangeIndexes();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            OrdModelView p = null;
            Object obj = this.DataContext;
            if ((obj != null) && (obj is OrdModelView))
            {
                p = obj as OrdModelView;
            }
            this.OrdModelView = p;
        }// myChangeDisplay
        #endregion // instance helpers
        private void comboBoxMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MatriceComputeMode mode = MatriceComputeMode.modeNothing;
            Object obj = comboBoxMode.SelectedItem;
            if ((obj != null) && (obj is ComboBoxItem))
            {
                ComboBoxItem item = obj as ComboBoxItem;
                Object o = item.Tag;
                if (o != null)
                {
                    String s = o.ToString().Trim().ToUpper();
                    int n = TAB_MODES_STRINGS.Length;
                    for (int i = 0; i < n; ++i)
                    {
                        if (s == TAB_MODES_STRINGS[i])
                        {
                            mode = TAB_MODES_ENUMS[i];
                            break;
                        }
                    }// i
                }// o
            }// obj
            this.MatriceMode = mode;
            this.myChangeData();
        }
        private void comboBoxSummary_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            double[] data = null;
            Object obj = comboBoxSummary.SelectedItem;
            if (obj != null)
            {
                String s = obj.ToString();
                Dictionary<String, double[]> dict = this.SummaryData;
                if (dict == null)
                {
                    dict = new Dictionary<string, double[]>();
                }
                if (dict.ContainsKey(s))
                {
                    data = dict[s];
                }
            }
            bool bOk = false;
            if (data != null)
            {
                this.HasSummary = true;
                this.Summary = data;
                bOk = true;
            }
            else
            {
                this.HasSummary = false;
                this.Summary = new double[0];
            }
            if (this.buttonSort != null)
            {
                this.buttonSort.IsEnabled = bOk;
            }
            m_rowcells = null;
            m_colcells = null;
            myChangeClasses();
        }
        private void checkboxIllustration_Click(object sender, RoutedEventArgs e)
        {
            bool b = ((this.checkboxIllustration.IsChecked != null) && this.checkboxIllustration.IsChecked.HasValue) ?
                this.checkboxIllustration.IsChecked.Value : false;
            if (this.ShowIndivs)
            {
                this.HasSummary = b;
            }
            m_rowcells = null;
            m_colcells = null;
            myChangeClasses();

        }
        private void buttonStop_Click(object sender, RoutedEventArgs e)
        {
            if ((m_cts != null) && (!m_cts.IsCancellationRequested))
            {
                m_cts.Cancel();
                this.buttonStop.IsEnabled = false;
            }
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.comboBoxMode != null)
            {
                MatriceComputeMode mode = this.MatriceMode;
                bool bDone = false;
                foreach (Object obj in comboBoxMode.Items)
                {
                    if (bDone)
                    {
                        break;
                    }
                    if (obj is ComboBoxItem)
                    {
                        ComboBoxItem item = obj as ComboBoxItem;
                        Object o = item.Tag;
                        if (o != null)
                        {
                            String s = o.ToString().Trim().ToUpper();
                            int n = TAB_MODES_STRINGS.Length;
                            for (int i = 0; i < n; ++i)
                            {
                                if ((s == TAB_MODES_STRINGS[i]) && (mode == TAB_MODES_ENUMS[i]))
                                {
                                    this.comboBoxMode.SelectedItem = item;
                                    bDone = true;
                                    break;
                                }
                            }// i
                        }// o
                    }// item
                }// obj
            }//
            if (this.checkboxIllustration != null)
            {
                this.checkboxIllustration.IsChecked = new Nullable<bool>(this.HasSummary);
            }
            if (this.buttonSort != null)
            {
                this.buttonSort.IsEnabled = (this.comboBoxSummary != null) && (this.comboBoxSummary.SelectedItem != null);
            }
            if (this.buttonStop != null)
            {
                this.buttonStop.IsEnabled = false;
            }
            if (this.buttonArrange != null)
            {
                this.buttonArrange.IsEnabled = (this.ColIndex != null) && (this.ColIndex.Length > 0) &&
                    (this.RowIndex != null) && (this.RowIndex.Length > 0);
            }
            this.myChangeIndexes();
        }
        private void checkboxShowIndivs_Click(object sender, RoutedEventArgs e)
        {
            Nullable<bool> bb = checkboxShowIndivs.IsChecked;
            bool b = ((bb != null) && bb.HasValue) ? bb.Value : false;
            if (b)
            {
                m_rowcells = null;
            }
            else
            {
                m_colcells = null;
            }
            this.m_busy = true;
            this.ShowIndivs = b;
            this.m_busy = false;
            myChangeIndexes();
        }
        private void checkboxCommScale_Click(object sender, RoutedEventArgs e)
        {
            Nullable<bool> bb = checkboxCommScale.IsChecked;
            bool b = ((bb != null) && bb.HasValue) ? bb.Value : false;
            this.m_busy = true;
            this.HasOwnScale = !b;
            this.m_busy = false;
            myChangeClasses();
        }
        private void checkboxGrayscale_Click(object sender, RoutedEventArgs e)
        {
            Nullable<bool> bb = checkboxGrayscale.IsChecked;
            bool b = ((bb != null) && bb.HasValue) ? bb.Value : false;
            MatDisplayMode m = (b) ? MatDisplayMode.modeGrayscale : MatDisplayMode.modeHistog;
            this.MatriceDisplayMode = m;
            this.myChangeMode();
        }
        private void buttonSort_Click(object sender, RoutedEventArgs e)
        {
            this.buttonArrange.IsEnabled = false;
            this.buttonSort.IsEnabled = false;
            this.buttonStop.IsEnabled = false;
            double[] data = null;
            Object obj = comboBoxSummary.SelectedItem;
            if (obj != null)
            {
                String s = obj.ToString();
                Dictionary<String, double[]> dict = this.SummaryData;
                if (dict == null)
                {
                    dict = new Dictionary<string, double[]>();
                }
                if (dict.ContainsKey(s))
                {
                    data = dict[s];
                }
            }
            int[] indexes = this.RowIndex;
            if ((data != null) && (indexes != null))
            {
                int n = data.Length;
                if (indexes.Length >= n)
                {
                    this.m_busy = true;
                    List<MatSortItem> oList = new List<MatSortItem>();
                    for (int i = 0; i < n; ++i)
                    {
                        oList.Add(new MatSortItem(i, data[i]));
                    }// i
                    oList.Sort();
                    for (int i = 0; i < n; ++i)
                    {
                        MatSortItem item = oList.ElementAt(i);
                        this.RowIndex[i] = item.Index;
                    }// i
                    this.m_busy = false;
                }// n
            }// data
            this.m_busy = true;
            m_colcells = null;
            m_rowcells = null;
            this.m_busy = false;
            myChangeIndexes();
            this.buttonArrange.IsEnabled = true;
            this.buttonSort.IsEnabled = true;
            this.buttonStop.IsEnabled = false;
        }
        private  async void buttonArrange_Click(object sender, RoutedEventArgs e)
        {
            var model = this.OrdModelView;
            if (model == null)
            {
                return;
            }
            this.buttonArrange.IsEnabled = false;
            this.buttonSort.IsEnabled = false;
            try
            {
                m_cts = new CancellationTokenSource();
                this.buttonStop.IsEnabled = true;
                var xx = await ArrangeSet.ArrangeIndexAsync(model, model.IterationsCount, m_cts.Token);
                if (xx != null)
                {
                    int[] colindex = xx.Item2;
                    int[] rowindex = xx.Item1;
                    this.ColIndex = ColIndex;
                    this.RowIndex = rowindex;
                }
            }
            finally
            {
                this.buttonSort.IsEnabled = true;
                this.buttonArrange.IsEnabled = true;
                this.buttonStop.IsEnabled = false;
                m_cts = null;
            }

        }
    }// class MatriceUserControl
}
