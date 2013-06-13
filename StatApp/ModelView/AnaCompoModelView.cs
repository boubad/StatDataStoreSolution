using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
/////////////////////////////////////
using System.Windows.Navigation;
using System.IO;
//////////////////////////////
using MathNet.Numerics.Statistics;
using MathNet.Numerics.Distributions;
/////////////////////////
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
//////////////////////////////////
using StatData;
using StatApp.Controles;
////////////////////////////////////
namespace StatApp.ModelView
{
   
    public class EigenData
    {
        public EigenData()
        {
        }
        public DisplayItemsArray EigenValues { get; set; }
        public DisplayItemsArray EigenVectors { get; set; }
        public DisplayItemsArray EigenVariables { get; set; }
        public DisplayItemsArray EigenIndivs { get; set; }
    }// EigenData
    public class AnaCompoModelView : StatModelViewBase
    {
        #region static variables
        private static readonly String TYPE_EIGENVALS = "Valeurs propres";
        private static readonly String TYPE_VARIABLES = "Variables";
        private static readonly String TYPE_INDIVS = "Individus";
        private static readonly String TYPE_COMBINED = "Combiné";
        private static readonly String DEFAULT_PREFIX = "FACT";
        #endregion // static variable
        #region Instance variables
        private DisplayItemsArray m_initialdata;
        private MainModelView m_main;
        private bool m_busy = false;
        private PlotModel m_plotmodel;
        private ObservableCollection<String> m_types;
        private String m_currenttype = TYPE_EIGENVALS;
        private ObservableCollection<String> m_axes;
        private String m_prefix = DEFAULT_PREFIX;
        private String m_xaxe;
        private String m_yaxe;
        private bool m_xbusy = false;
        private bool m_haslabels = false;
        private bool m_haspoints = true;
        private bool m_hasimages = false;
        private Dictionary<int, String> m_categdict;
        private VariableDescs m_currentvariables;
        private VariableDescs m_leftvariables;
        private VariableDesc m_currentcategvariable;
        private Anacompo m_anacompo;
        private IndivDatas m_indivs;
        private int m_nfact = -1;
        private bool m_robust = false;
        private DisplayItemsArray m_values;
        private DisplayItemsArray m_vectors;
        private DisplayItemsArray m_eigvariables;
        private DisplayItemsArray m_eigindivs;
        #endregion // Instance Variables
        #region constructors
        public AnaCompoModelView(MainModelView pMain)
            : base(pMain.ServiceLocator, pMain.PageLocator, pMain.NavigationService)
        {
            m_main = pMain;
            m_main.PropertyChanged += m_main_PropertyChanged;
        }
        void m_main_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (m_busy)
            {
                return;
            }
            m_busy = true;
            String name = e.PropertyName;
            if (name == "IsBusy")
            {
                NotifyPropertyChanged("IsBusy");
                return;
            }
            if (name == "Variables")
            {
                NotifyPropertyChanged("AllVariables");
                this.InitialData = null;
            }
            if (name == "CurrentStatDataSet")
            {
                m_main.RefreshVariables();
            }
            else if (name == "AllNumVariables")
            {
                NotifyPropertyChanged("AllNumVariables");
                this.m_currentvariables = null;
                this.LeftVariables = m_main.AllNumVariables;
                NotifyPropertyChanged("CurrentVariables");
                this.RefreshVariablesValues();
            }
            else if (name == "AllCategVariables")
            {
                this.m_currentcategvariable = null;
                NotifyPropertyChanged("AllCategVariables");
                NotifyPropertyChanged("CurrentcategVariable");
                if (this.CurrentType == TYPE_INDIVS)
                {
                    this.UpdatePlot();
                }
            }
            m_busy = false;
        }
        #endregion // constructors
        #region Properties
        public bool IsBusy
        {
            get
            {
                return (m_main.IsBusy) || m_busy;
            }
            set
            {
                if (value != m_busy)
                {
                    m_busy = value;
                    NotifyPropertyChanged("IsBusy");
                }
            }
        }// IsBusy
        public Dictionary<int, OxyImage> ImagesDisctionary
        {
            get
            {
                return m_main.ImagesDictionary;
            }
            set
            {
            }
        }// ImagesDictionary
        public DisplayItemsArray InitialData
        {
            get
            {
                if (m_initialdata == null)
                {
                    m_initialdata = new DisplayItemsArray();
                }
                return m_initialdata;
            }
            set
            {
                if (value != m_initialdata)
                {
                    m_initialdata = value;
                    NotifyPropertyChanged("InitialData");
                }
            }
        }// InitialData
        public VariableDescs AllVariables
        {
            get
            {
                return m_main.Variables;
            }
            set
            {
                NotifyPropertyChanged("AllVariables");
            }
        }// AllVariables
        public DisplayItemsArray EigenVectors
        {
            get
            {
                if (m_vectors == null)
                {
                    m_vectors = new DisplayItemsArray();
                }
                return m_vectors;
            }
            set
            {
                if (value != m_vectors)
                {
                    m_vectors = value;
                    NotifyPropertyChanged("EigenVectors");
                }
            }
        }// EigenVectors
        public DisplayItemsArray EigenValues
        {
            get
            {
                if (m_values == null)
                {
                    m_values = new DisplayItemsArray();
                }
                return m_values;
            }
            set
            {
                if (value != m_values)
                {
                    m_values = value;
                    NotifyPropertyChanged("EigenValues");
                }
            }
        }// EigenValues
        public DisplayItemsArray EigenVariables
        {
            get
            {
                if (m_eigvariables == null)
                {
                    m_eigvariables = new DisplayItemsArray();
                }
                return m_eigvariables;
            }
            set
            {
                if (value != m_eigvariables)
                {
                    m_eigvariables = value;
                    NotifyPropertyChanged("EigenVariables");
                }
            }
        }// EigenVariables
        public DisplayItemsArray EigenIndivs
        {
            get
            {
                if (m_eigindivs == null)
                {
                    m_eigindivs = new DisplayItemsArray();
                }
                return m_eigindivs;
            }
            set
            {
                if (value != m_eigindivs)
                {
                    m_eigindivs = value;
                    NotifyPropertyChanged("EigenIndivs");
                }
            }
        }// EigenIndivs
        public bool IsRobust
        {
            get
            {
                return m_robust;
            }
            set
            {
                if (m_robust != value)
                {
                    m_robust = value;
                    NotifyPropertyChanged("IsRobust");
                    RefreshVariablesValues();
                }
            }
        }// IsRobust
        public int FactorCount
        {
            get
            {
                if (this.TotalFactorCount < 1)
                {
                    return -1;
                }
                if ((m_nfact >= 0) && (m_nfact <= this.TotalFactorCount))
                {
                    return m_nfact;
                }
                m_nfact = 3;
                if (m_nfact >= this.TotalFactorCount)
                {
                    m_nfact = this.TotalFactorCount;
                }
                return m_nfact;
            }
            set
            {
                if ((value != m_nfact) && (value >= 0) && (value <= this.TotalFactorCount))
                {
                    m_nfact = value;
                    NotifyPropertyChanged("FactorCount");
                }
            }
        }// FactorCount
        public int TotalFactorCount
        {
            get
            {
                return ((m_anacompo != null) && (m_anacompo.IsValid)) ? m_anacompo.EigenValues.Length : -1;
            }
            set
            {
                NotifyPropertyChanged("TotalFactorCount");
            }
        }// TotalFactorCount
        public StatDataSet CurrentStatDataSet
        {
            get
            {
                return m_main.CurrentStatDataSet;
            }
        }// CurrentStatDataSet
        public IndivDatas Individus
        {
            get
            {
                if (m_indivs == null)
                {
                    m_indivs = new IndivDatas();
                }
                return m_indivs;
            }
            set
            {
                if (value != m_indivs)
                {
                    m_indivs = value;
                    NotifyPropertyChanged("Individus");
                }
            }
        }// Individus
        public VariableDescs AllNumVariables
        {
            get
            {
                return m_main.AllNumVariables;
            }
            set
            {
                NotifyPropertyChanged("AllNumVariables");
            }
        }// AllNumVariables
        public VariableDescs CurrentVariables
        {
            get
            {
                if (m_currentvariables == null)
                {
                    m_currentvariables = new VariableDescs();
                }
                return m_currentvariables;
            }
            set
            {
                if (value != m_currentvariables)
                {
                    m_currentvariables = value;
                    NotifyPropertyChanged("CurrentVariables");
                }
            }
        }// CurrentVariables
        public VariableDescs LeftVariables
        {
            get
            {
                if ((m_leftvariables != null) && (m_leftvariables.Count > 0))
                {
                    return m_leftvariables;
                }
                m_leftvariables = null;
                var col = m_main.AllNumVariables;
                List<VariableDesc> oList = new List<VariableDesc>();
                oList.AddRange(col);
                m_leftvariables = new VariableDescs(oList);
                return m_leftvariables;
            }
            set
            {
                if (value != m_leftvariables)
                {
                    m_leftvariables = value;
                    if ((m_leftvariables != null) && (m_leftvariables.Count > 1))
                    {
                        var oList = m_leftvariables.ToList();
                        oList.Sort();
                        m_leftvariables = new VariableDescs(oList);
                    }
                    NotifyPropertyChanged("LeftVariables");
                }
            }
        }// LeftVariables
        public VariableDescs AllCategVariables
        {
            get
            {
                return m_main.AllCategVariables;
            }
            set
            {
                NotifyPropertyChanged("AllCategVariables");
            }
        }// CategVariables
        public VariableDesc CurrentCategVariable
        {
            get
            {
                if (m_currentcategvariable == null)
                {
                    m_currentcategvariable = null;
                }
                return m_currentcategvariable;
            }
            set
            {
                if (m_currentcategvariable != value)
                {
                    m_currentcategvariable = value;
                    NotifyPropertyChanged("CurrentCategVariable");
                    this.refreshCategories();
                }
            }
        }// CurrentCategVariable
        public Dictionary<int, String> CategDictionary
        {
            get
            {
                if (m_categdict == null)
                {
                    m_categdict = new Dictionary<int, string>();
                }
                return m_categdict;
            }
            set
            {
                if (value != m_categdict)
                {
                    m_categdict = value;
                    NotifyPropertyChanged("CategDictionary");
                }
            }
        }// CategDictionary
        public bool HasLabels
        {
            get
            {
                return m_haslabels;
            }
            set
            {
                if (value != m_haslabels)
                {
                    m_haslabels = value;
                    NotifyPropertyChanged("HasLabels");
                    String sType = this.CurrentType;
                    if ((sType == TYPE_INDIVS) || (sType == TYPE_COMBINED))
                    {
                        this.UpdatePlot();
                    }
                }// change
            }
        }// HasLabels
        public bool HasImages
        {
            get
            {
                return m_hasimages;
            }
            set
            {
                if (value != m_hasimages)
                {
                    m_hasimages = value;
                    NotifyPropertyChanged("HasImages");
                    String sType = this.CurrentType;
                    if ((sType == TYPE_INDIVS) || (sType == TYPE_COMBINED))
                    {
                        this.UpdatePlot();
                    }
                }// change
            }
        }// HasLabels
        public bool HasPoints
        {
            get
            {
                return m_haspoints;
            }
            set
            {
                if (value != m_haspoints)
                {
                    m_haspoints = value;
                    NotifyPropertyChanged("HasPoints");
                    String sType = this.CurrentType;
                    if ((sType == TYPE_INDIVS) || (sType == TYPE_COMBINED))
                    {
                        this.UpdatePlot();
                    }
                }// change
            }
        }// HasPoints
        public String FactorPrefix
        {
            get
            {
                return String.IsNullOrEmpty(m_prefix) ? DEFAULT_PREFIX : m_prefix.Trim();
            }
            set
            {
                String old = String.IsNullOrEmpty(m_prefix) ? DEFAULT_PREFIX : m_prefix.Trim();
                String s = String.IsNullOrEmpty(value) ? String.Empty : value.Trim();
                if (s != old)
                {
                    m_prefix = s;
                    NotifyPropertyChanged("Prefix");
                    this.FactorAxes = null;
                    this.RefreshVariablesValues();
                }
            }
        }// CurrentType
        public ObservableCollection<String> FactorAxes
        {
            get
            {
                if ((m_axes != null) && (m_axes.Count > 0))
                {
                    return m_axes;
                }
                m_axes = new ObservableCollection<string>();
                var p = this.Anacompo;
                if ((p != null) && p.IsValid)
                {
                    int n = p.Cols;
                    String prefix = this.FactorPrefix;
                    for (int i = 0; i < n; ++i)
                    {
                        String s = String.Format("{0}{1}", prefix, i + 1);
                        m_axes.Add(s);
                    }// i
                }
                return (m_axes == null) ? new ObservableCollection<String>() : m_axes;
            }
            set
            {
                if (value != m_axes)
                {
                    m_axes = value;
                    NotifyPropertyChanged("FactorAxes");
                    this.XAxe = null;
                    this.YAxe = null;
                    this.UpdatePlot();
                }
            }
        }// PlotTypes
        public String XAxe
        {
            get
            {
                if (!String.IsNullOrEmpty(m_xaxe))
                {
                    return m_xaxe;
                }
                m_xaxe = null;
                var p = this.Anacompo;
                if ((p != null) && p.IsValid)
                {
                    m_xaxe = String.Format("{0}{1}", this.FactorPrefix, 1);
                }
                return m_xaxe;
            }
            set
            {
                String old = String.IsNullOrEmpty(m_xaxe) ? String.Empty : m_xaxe.Trim();
                String s = String.IsNullOrEmpty(value) ? String.Empty : value.Trim();
                if (s != old)
                {
                    m_xaxe = s;
                    NotifyPropertyChanged("XAxe");
                    if (!m_xbusy)
                    {
                        this.UpdatePlot();
                    }
                }
            }
        }// XAxe
        public String YAxe
        {
            get
            {
                if (!String.IsNullOrEmpty(m_yaxe))
                {
                    return m_yaxe;
                }
                m_yaxe = null;
                var p = this.Anacompo;
                if ((p != null) && p.IsValid)
                {
                    int n = p.EigenValues.Length;
                    String old = this.XAxe;
                    for (int i = 0; i < n; ++i)
                    {
                        String s = String.Format("{0}{1}", this.FactorPrefix, i+1);
                        if ((!String.IsNullOrEmpty(old)) && (old == s))
                        {
                            continue;
                        }
                        else
                        {
                            m_yaxe = s;
                            break;
                        }
                    }
                }
                return m_yaxe;
            }
            set
            {
                String old = String.IsNullOrEmpty(m_yaxe) ? String.Empty : m_yaxe.Trim();
                String s = String.IsNullOrEmpty(value) ? String.Empty : value.Trim();
                if (s != old)
                {
                    m_yaxe = s;
                    NotifyPropertyChanged("YAxe");
                    if (!m_xbusy)
                    {
                        this.UpdatePlot();
                    }
                }
            }
        }// YAxe
        public ObservableCollection<String> PlotTypes
        {
            get
            {
                if ((m_types != null) && (m_types.Count > 0))
                {
                    return m_types;
                }
                m_types = new ObservableCollection<string>(new String[] { TYPE_EIGENVALS, TYPE_VARIABLES, TYPE_INDIVS,TYPE_COMBINED });
                return m_types;
            }
            set { }
        }// PlotTypes
        public String CurrentType
        {
            get
            {
                return String.IsNullOrEmpty(m_currenttype) ? TYPE_EIGENVALS : m_currenttype.Trim();
            }
            set
            {
                String old = String.IsNullOrEmpty(m_currenttype) ? TYPE_EIGENVALS : m_currenttype.Trim();
                String s = String.IsNullOrEmpty(value) ? String.Empty : value.Trim();
                if (s.ToLower() != old.ToLower())
                {
                    m_currenttype = s;
                    NotifyPropertyChanged("CurrentType");
                    this.UpdatePlot();
                }
            }
        }// CurrentType
        public Anacompo Anacompo
        {
            get
            {
                return m_anacompo;
            }
            set
            {
                if (value != m_anacompo)
                {
                    m_anacompo = value;
                    NotifyPropertyChanged("Anacompo");
                    this.FactorAxes = null;
                }
            }
        }// Anacompo
        public PlotModel PlotModel
        {
            get
            {
                return m_plotmodel;
            }
            set
            {
                if (value != m_plotmodel)
                {
                    m_plotmodel = value;
                    NotifyPropertyChanged("PlotModel");
                }
            }
        }// PlotModel
        #endregion // Properties
        #region Methods
        public void AddVariables(IEnumerable<VariableDesc> col)
        {
            if (col == null)
            {
                return;
            }
            var old = this.CurrentVariables.ToList();
            var oLeft = this.LeftVariables;
            bool bAdd = false;
            foreach (var v in col)
            {
                if (v != null)
                {
                    old.Add(v);
                    if (oLeft.Contains(v))
                    {
                        oLeft.Remove(v);
                    }
                    bAdd = true;
                }
            }// v
            if (old.Count > 1)
            {
                old.Sort();
            }
            this.CurrentVariables = new VariableDescs(old);
            NotifyPropertyChanged("LeftVariables");
            if (bAdd)
            {
                this.RefreshVariablesValues();
            }
        }//AddVariables
        public void RemoveVariables(IEnumerable<VariableDesc> col)
        {
            if (col == null)
            {
                return;
            }
            var src = col.ToList();
            var dest = this.CurrentVariables.ToList();
            var oLeft = this.LeftVariables;
            bool bDel = false;
            foreach (var v in src)
            {
                if ((v != null) && dest.Contains(v))
                {
                    dest.Remove(v);
                    if (!oLeft.Contains(v))
                    {
                        oLeft.Add(v);
                    }
                    bDel = true;
                }
            }// v
            if (dest.Count > 1)
            {
                dest.Sort();
            }
            this.CurrentVariables = new VariableDescs(dest);
            NotifyPropertyChanged("LeftVariables");
            if (bDel)
            {
                this.RefreshVariablesValues();
            }
        }// RemoveVariables
        public async void AddToDataSet()
        {
            var pAna = this.Anacompo;
            if (pAna == null)
            {
                return;
            }
            if (!pAna.IsValid)
            {
                return;
            }
            var oSet = this.CurrentStatDataSet;
            if (oSet == null)
            {
                return;
            }
            if (!oSet.IsValid)
            {
                return;
            }
            var inds = pAna.RecodedInds;
            if (inds == null)
            {
                return;
            }
            var oIndexes = pAna.Indexes;
            if (oIndexes == null)
            {
                return;
            }
            int nMax = this.FactorCount;
            int nv = this.FactorCount;
            if (nv < 1)
            {
                nv = 3;
            }
            if (nv > nMax)
            {
                nv = nMax;
            }
            int nr = inds.GetLength(0);
            var pMan = this.DataService;
            if (pMan == null)
            {
                return;
            }
            this.IsBusy = true;
            try
            {
                List<VariableDesc> oVars = new List<VariableDesc>();
                String prefix = this.FactorPrefix;
                for (int i = 0; i < nv; ++i)
                {
                    VariableDesc oVar = new VariableDesc();
                    oVar.DataSetId = oSet.Id;
                    oVar.Name = String.Format("{0}{1}", prefix, i + 1);
                    oVar.IsCategVar = false;
                    oVar.DataType = "float";
                    oVar.Description = String.Format("Composante principale {0}", i + 1);
                    oVar.IsModified = true;
                    List<ValueDesc> vals = new List<ValueDesc>();
                    for (int j = 0; j < nr; ++j)
                    {
                        ValueDesc v = new ValueDesc();
                        v.Index = oIndexes[j];
                        v.DataStringValue = Convert.ToString(inds[j, i]);
                        v.IsModified = true;
                        vals.Add(v);
                    }// j
                    oVar.Values = new ValueDescs(vals);
                    oVars.Add(oVar);
                }// i
                var xx = await Task.Run<Tuple<bool, Exception>>(() =>
                {
                    return pMan.MaintainsVariableAndValues(oVars);
                });
                if ((xx != null) && (xx.Item2 != null))
                {
                    this.ShowError(xx.Item2);
                }
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
            }
            this.IsBusy = false;
        }// addDataSet
        public void RefreshVariables()
        {
            m_main.RefreshVariables();
        }
        public async void UpdatePlot()
        {
            this.PlotModel = null;
            this.PlotModel = await createPlotAsync();
        }// UpdatePlot
        #endregion // Methods
        #region helpers
        private Task<PlotModel> createPlotAsync()
        {
            return Task.Run<PlotModel>(() =>
            {
                PlotModel model = null;
                try
                {
                    var pAna = this.Anacompo;
                    if (pAna == null)
                    {
                        return null;
                    }
                    if (!pAna.IsValid)
                    {
                        return null;
                    }
                    String sType = this.CurrentType;
                    int nType = -1;
                    if (sType == TYPE_EIGENVALS)
                    {
                        return createEigenValuesPlotModel(pAna);
                    }
                    else if (sType == TYPE_VARIABLES)
                    {
                        nType = 1;
                    }
                    else if (sType == TYPE_INDIVS)
                    {
                        nType = 2;
                    }
                    else if (sType == TYPE_COMBINED)
                    {
                        nType = 3;
                    }
                    else
                    {
                        return null;
                    }
                    var tt = getAxesIndexes();
                    int iFactorX = tt.Item1;
                    int iFactorY = tt.Item2;
                    if ((iFactorX < 0) || (iFactorY < 0))
                    {
                        return null;
                    }
                    if (iFactorX == iFactorY)
                    {
                        return null;
                    }
                    if (nType == 1)
                    {
                        model = createVariablesPlotModel(pAna, iFactorX, iFactorY);
                    }
                    else if (nType == 2)
                    {
                        model = createIndivsPlotModel(pAna, iFactorX, iFactorY);
                    }
                    else if (nType == 3)
                    {
                        model = createCombinedPlotModel(pAna, iFactorX, iFactorY);
                    }
                }// try
                catch (Exception /*ex */)
                {
                    model = null;
                }
                return model;
            });
        }// createPlot
        private PlotModel createIndivsPlotModel(Anacompo pAna, int iFactorX, int iFactorY)
        {
            PlotModel model = null;
            var data = pAna.RecodedInds;
            if (data == null)
            {
                return null;
            }
            bool bLabel = this.HasLabels;
            bool bImage = this.HasImages;
            bool bPoints = this.HasPoints;
            if ((!bLabel) && (!bImage) && (!bPoints))
            {
                return null;
            }
            int nr = data.GetLength(0);
            int nv = data.GetLength(1);
            if ((nv <= iFactorX) || (nv <= iFactorX) || (nr < 1))
            {
                return null;
            }
            var inds = this.Individus.ToArray();
            nr = (inds.Length < nr) ? inds.Length : nr;
            if (nr < 1)
            {
                return null;
            }
            double[] xx = new double[nr];
            double[] yy = new double[nr];
            for (int i = 0; i < nr; ++i)
            {
                xx[i] = data[i, iFactorX];
                yy[i] = data[i, iFactorY];
            }// i
            Dictionary<String, ScatterSeries> oSeries = new Dictionary<string, ScatterSeries>();
            Dictionary<int, String> dict = this.CategDictionary;
            model = new PlotModel("Individus", String.Format("{0} / {1}", this.YAxe, this.XAxe));
            model.LegendPlacement = LegendPlacement.Outside;
            model.Axes.Add(new LinearAxis(AxisPosition.Bottom, xx.Min(), xx.Max(), this.XAxe) { PositionAtZeroCrossing = true });
            model.Axes.Add(new LinearAxis(AxisPosition.Left, yy.Min(), yy.Max(), this.YAxe) { PositionAtZeroCrossing = true });
            ScatterSeries oTrash = null;
            double xwidth = 3.0 * (xx.Max() - xx.Min()) / nr;
            for (int i = 0; i < nr; ++i)
            {
                var ind = inds[i];
                int index = ind.IndivIndex;
                double x = data[i, iFactorX];
                double y = data[i, iFactorY];

                if (dict.ContainsKey(index))
                {
                    String scateg = dict[index];
                    if (!oSeries.ContainsKey(scateg))
                    {
                        oSeries[scateg] = new ScatterSeries() { MarkerType = MarkerType.Circle };
                    }
                    var color = oSeries[scateg].MarkerFill;
                    if (bImage && (this.ImagesDisctionary != null) && this.ImagesDisctionary.ContainsKey(index))
                    {
                        model.Annotations.Add(new ImageAnnotation { 
                            ImageSource = this.ImagesDisctionary[index],
                            X= new PlotLength(x,PlotLengthUnit.Data),
                            Y= new PlotLength(y,PlotLengthUnit.Data),
                            HorizontalAlignment=HorizontalAlignment.Center,
                            VerticalAlignment=VerticalAlignment.Middle,
                            Width = new PlotLength(xwidth,PlotLengthUnit.Data)
                        });
                    }
                    if (bLabel)
                    {
                        String name = ind.Name;
                        if (!String.IsNullOrEmpty(name))
                        {
                            model.Annotations.Add(new TextAnnotation
                            {
                                Position = new DataPoint(x, y),
                                Text = name,
                                TextColor = color,
                                FontWeight = FontWeights.Bold,Tag = ind
                            });
                        }
                    }
                    if (bPoints)
                    {
                        oSeries[scateg].Points.Add(new ScatterPoint(x, y) { Tag = ind });
                    }
                }
                else
                {
                    if (oTrash == null)
                    {
                        oTrash = new ScatterSeries();
                    }
                    if (bImage && (this.ImagesDisctionary != null) && this.ImagesDisctionary.ContainsKey(index))
                    {
                        model.Annotations.Add(new ImageAnnotation
                        {
                            ImageSource = this.ImagesDisctionary[index],
                            X = new PlotLength(x, PlotLengthUnit.Data),
                            Y = new PlotLength(y, PlotLengthUnit.Data),
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Middle,
                            Width = new PlotLength(xwidth, PlotLengthUnit.Data)
                        });
                    }
                    if (bLabel)
                    {
                        String name = ind.IdString;
                        if (!String.IsNullOrEmpty(name))
                        {
                            model.Annotations.Add(new TextAnnotation { Position = new DataPoint(x, y), Text = name, 
                                FontWeight = FontWeights.Bold , Tag=ind});
                        }
                    }
                    if (bPoints)
                    {
                        oTrash.Points.Add(new ScatterPoint(x, y) { Tag = ind });
                    }
                }
            }// inds
            foreach (var sx in oSeries.Values)
            {
                model.Series.Add(sx);
            }
            if (oTrash != null)
            {
                model.Series.Add(oTrash);
            }
            return model;
        }//createIndivsPlotModel
        private PlotModel createCombinedPlotModel(Anacompo pAna, int iFactorX, int iFactorY)
        {
            PlotModel model = null;
            var data = pAna.RecodedInds;
            if (data == null)
            {
                return null;
            }
            int nr = data.GetLength(0);
            int nv = data.GetLength(1);
            if ((nv <= iFactorX) || (nv <= iFactorX) || (nr < 1))
            {
                return null;
            }
            var inds = this.Individus.ToArray();
            nr = (inds.Length < nr) ? inds.Length : nr;
            if (nr < 1)
            {
                return null;
            }
            double[] xx = new double[nr];
            double[] yy = new double[nr];
            for (int i = 0; i < nr; ++i)
            {
                xx[i] = data[i, iFactorX];
                yy[i] = data[i, iFactorY];
            }// i
            Dictionary<String, ScatterSeries> oSeries = new Dictionary<string, ScatterSeries>();
            Dictionary<int, String> dict = this.CategDictionary;
            model = new PlotModel("Individus", String.Format("{0} / {1}", this.YAxe, this.XAxe));
            model.LegendPlacement = LegendPlacement.Outside;
            model.Axes.Add(new LinearAxis(AxisPosition.Bottom, xx.Min(), xx.Max(), this.XAxe) { PositionAtZeroCrossing = true });
            model.Axes.Add(new LinearAxis(AxisPosition.Left, yy.Min(), yy.Max(), this.YAxe) { PositionAtZeroCrossing = true });
            var vardata = pAna.RecodedVars;
            var names = this.CurrentVariables.ToArray();
            for (int i = 0; i < nv; ++i)
            {
                double x = vardata[i, iFactorX];
                double y = vardata[i, iFactorY];
                String sname = names[i].Name;
                model.Annotations.Add(new ArrowAnnotation { StartPoint = new DataPoint(0, 0), EndPoint = new DataPoint(x, y), LineStyle=LineStyle.DashDotDot });
                model.Annotations.Add(new TextAnnotation { Position = new DataPoint(x, y), Text = sname, HorizontalAlignment=HorizontalAlignment.Center,
                VerticalAlignment=VerticalAlignment.Middle});
            }// i
            bool bLabel = this.HasLabels;
            ScatterSeries oTrash = null;
            for (int i = 0; i < nr; ++i)
            {
                var ind = inds[i];
                int index = ind.IndivIndex;
                double x = data[i, iFactorX];
                double y = data[i, iFactorY];

                if (dict.ContainsKey(index))
                {
                    String scateg = dict[index];
                    if (!oSeries.ContainsKey(scateg))
                    {
                        oSeries[scateg] = new ScatterSeries() { MarkerType = MarkerType.Circle };
                    }
                    var color = oSeries[scateg].MarkerFill;
                    if (bLabel)
                    {
                        String name = ind.Name;
                        if (!String.IsNullOrEmpty(name))
                        {
                            model.Annotations.Add(new TextAnnotation
                            {
                                Position = new DataPoint(x, y),
                                Text = name,
                                TextColor = color,
                                FontWeight = FontWeights.Bold,
                                Tag = ind,HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Middle
                            });
                        }
                    }
                    else
                    {
                        oSeries[scateg].Points.Add(new ScatterPoint(x, y) { Tag = ind });
                    }
                }
                else
                {
                    if (oTrash == null)
                    {
                        oTrash = new ScatterSeries();
                    }
                    if (bLabel)
                    {
                        String name = ind.IdString;
                        if (!String.IsNullOrEmpty(name))
                        {
                            model.Annotations.Add(new TextAnnotation
                            {
                                Position = new DataPoint(x, y),
                                Text = name,
                                FontWeight = FontWeights.Bold,
                                Tag = ind,HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Middle
                            });
                        }
                    }
                    else
                    {
                        oTrash.Points.Add(new ScatterPoint(x, y) { Tag = ind });
                    }
                }
            }// inds
            foreach (var sx in oSeries.Values)
            {
                model.Series.Add(sx);
            }
            if (oTrash != null)
            {
                model.Series.Add(oTrash);
            }
            return model;
        }//createCombinedPlotModel
        private PlotModel createVariablesPlotModel(Anacompo pAna, int iFactorX, int iFactorY)
        {
            PlotModel model = null;
            var data = pAna.RecodedVars;
            if (data == null)
            {
                return null;
            }
            int nr = data.GetLength(0);
            int nv = data.GetLength(1);
            if ((nv < iFactorX) || (nv < iFactorY) || (nr < 1))
            {
                return null;
            }
            model = new PlotModel("Variables", String.Format("{0} / {1}", this.YAxe, this.XAxe));
            double[] yy = new double[nr];
            double[] xx = new double[nr];
            String[] names = new String[nr];
            var col = this.CurrentVariables.ToArray();
            for (int i = 0; i < nr; ++i)
            {
                yy[i] = data[i, iFactorY];
                xx[i] = data[i, iFactorX];
                names[i] = (i < col.Length) ? col[i].Name : "";
            }// i
            model.Axes.Add(new LinearAxis(AxisPosition.Bottom, -1.0, 1.0, this.XAxe) { PositionAtZeroCrossing = true });
            model.Axes.Add(new LinearAxis(AxisPosition.Left, -1.0, 1.0, this.YAxe) { PositionAtZeroCrossing = true });
            for (int i = 0; i < nr; ++i)
            {
                double x = xx[i];
                double y = yy[i];
                String sname = names[i];
                model.Annotations.Add(new ArrowAnnotation { StartPoint = new DataPoint(0, 0), EndPoint = new DataPoint(x, y) });
                model.Annotations.Add(new TextAnnotation { Position = new DataPoint(x, y), Text = sname });
            }// i
            return model;
        }// reateVariablesPlotModel
        private PlotModel createEigenValuesPlotModel(Anacompo pAna)
        {
            PlotModel model = null;
            var data = pAna.EigenValues;
            if (data == null)
            {
                return null;
            }
            if (data.Length < 1)
            {
                return null;
            }
            int n = data.Length;
            double[] dd = new double[n];
            for (int i = 0; i < n; ++i)
            {
                double x = data[i];
                if (x < 0.0)
                {
                    x = 0;
                }
                dd[i] = x;
            }// i
            double somme = dd.Sum();
            if (somme <= 0.0)
            {
                return null;
            }
            model = new PlotModel("Valeurs propres");
            var ps = new PieSeries();
            for (int i = 0; i < n; ++i)
            {
                double x = (dd[i] / somme) * 100.0 + 0.5;
                if (x > 100.0)
                {
                    x = 100.0;
                }
                ps.Slices.Add(new PieSlice(String.Format("FACT{0}", i + 1), x) { IsExploded = true });
            }
            ps.InnerDiameter = 0;
            ps.ExplodedDistance = 0.0;
            ps.Stroke = OxyColors.White;
            ps.StrokeThickness = 2.0;
            ps.InsideLabelPosition = 0.8;
            ps.AngleSpan = 360;
            ps.StartAngle = 0;
            ps.FontWeight = FontWeights.Bold;
            model.Series.Add(ps);
            return model;
        }//createEigenValuesPlotMode
        private Tuple<int, int> getAxesIndexes()
        {
            int n1 = -1;
            int n2 = -1;
            String prefix = this.FactorPrefix;
            String s1 = this.XAxe;
            String s2 = this.YAxe;
            if ((!String.IsNullOrEmpty(prefix)) && (!String.IsNullOrEmpty(s1)) && (!String.IsNullOrEmpty(s2)))
            {
                int l = prefix.Length;
                if (s1.Length > l)
                {
                    String s = s1.Substring(l);
                    int.TryParse(s, out n1);
                }
                if (s2.Length > l)
                {
                    String s = s2.Substring(l);
                    int.TryParse(s, out n2);
                }
                if ((n1 > 0) && (n2 > 0))
                {
                    --n1;
                    --n2;
                }
                else
                {
                    n1 = -1;
                    n2 = -1;
                }
            }
            return new Tuple<int, int>(n1, n2);
        }// getAxesIndexes
        private Dictionary<int, String> createCategDict()
        {
            Dictionary<int, String> dict = null;
            VariableDesc oCateg = this.CurrentCategVariable;
            if ((oCateg != null) && oCateg.IsValid)
            {
                dict = m_main.ComputeCategDict(oCateg);
                if ((dict != null) && (dict.Count < 2))
                {
                    dict = null;
                }
            }
            if (dict == null)
            {
                var col = this.Individus;
                String s = DEFAULT_SERIE_NAME;
                dict = new Dictionary<int, string>();
                foreach (var p in col)
                {
                    int index = p.IndivIndex;
                    if (index >= 0)
                    {
                        dict[index] = s;
                    }
                }// p
            }
            return dict;
        }// createCategDict
        private async void refreshCategories()
        {
            var dict = await Task.Run<Dictionary<int, String>>(() =>
            {
                return createCategDict();
            });
            this.CategDictionary = dict;
            this.UpdatePlot();
        }// refreshCategories
        private Task<Dictionary<VariableDesc, ValueDescs>> performRefreshDataAsync()
        {
            var col = this.CurrentVariables;
            return Task.Run<Dictionary<VariableDesc, ValueDescs>>(() =>
            {
                return this.GetCommonValues(col);
            });
        }// perform RefreshDataAsync
        public async void RefreshVariablesValues()
        {
            this.IsBusy = true;
            var inds = m_main.AllIndividus;
            var vars = this.CurrentVariables;
            MatriceComputeMode mode = (this.IsRobust) ? MatriceComputeMode.modeRank : MatriceComputeMode.modeNothing;
            int nClasses = 5;
            var xx = await Task.Run<IEnumerable<IndivData>>(() =>
            {
                return GetIndivsData(inds, vars, mode, nClasses);
            });
            if (xx != null)
            {
                this.Individus = new IndivDatas(xx);
            }
            var xy = await Task.Run<Tuple<IndivDatas, Anacompo,EigenData >>(() =>
            {
                return refreshData();
            });
            DisplayItemsArray v1 = null;
            DisplayItemsArray v2 = null;
            DisplayItemsArray v3 = null;
            DisplayItemsArray v4 = null;
            var xf = xy.Item3;
            if (xf != null)
            {
                v1 = xf.EigenValues;
                v2 = xf.EigenVectors;
                v3 = xf.EigenVariables;
                v4 = xf.EigenIndivs;
            }
            this.Anacompo = xy.Item2;
            this.EigenValues = v1;
            this.EigenVectors = v2;
            this.EigenVariables = v3;
            this.EigenIndivs = v4;
            this.m_axes = null;
            this.m_xaxe = null;
            this.m_yaxe = null;
            NotifyPropertyChanged("FactorAxes");
            NotifyPropertyChanged("XAxe");
            NotifyPropertyChanged("YAxe");
            NotifyPropertyChanged("TotalFactorCount");
            NotifyPropertyChanged("FactorCount");
            this.UpdatePlot();
            this.InitialData = await CreateDataDisplayAsync(this.Individus, this.AllVariables, this.CurrentVariables);
            this.IsBusy = false;
        }// RefreshVariablesValues
        private Tuple<IndivDatas, Anacompo,EigenData> refreshData()
        {
            IndivDatas dx = this.Individus;
            Anacompo cp = null;
            EigenData eigen = null;
            try
            {
                var oAr = this.Individus.ToArray();
                int nr = oAr.Length;
                if (nr < 2)
                {
                    return new Tuple<IndivDatas, Anacompo, EigenData>(dx, cp, eigen);
                }
                int nv = oAr[0].DoubleData.Length;
                if (nv < 2)
                {
                    return new Tuple<IndivDatas, Anacompo, EigenData>(dx, cp, eigen);
                }
                double[,] alldata = new double[nr, nv];
                List<int> zinds = new List<int>();
                for (int i = 0; i < nr; ++i)
                {
                    var ind = oAr[i];
                    zinds.Add(ind.IndivIndex);
                    double[] dd = ind.DoubleData;
                    for (int j = 0; j < nv; ++j)
                    {
                        alldata[i, j] = dd[j];
                    }// j
                }// i
                Exception err = null;
                cp = new Anacompo();
                if (!cp.ComputeEigen(alldata, out err))
                {
                    cp = null;
                }
                if (cp != null)
                {
                    eigen = computeData(cp);
                }
                cp.Indexes = zinds.ToArray();
            }
            catch (Exception /*ex */)
            {
                dx = null;
                cp = null;
            }
            return new Tuple<IndivDatas, Anacompo,EigenData>(dx, cp,eigen);
        }// RefreshVariablesValues
        private EigenData computeData(Anacompo p)
        {
            String prefix = this.FactorPrefix;
            EigenData oRet = new EigenData();
            var vals = p.EigenValues;
            int nv = vals.Length;
            double sum = vals.Sum();
            if (sum <= 0.0)
            {
                return null;
            }
            DisplayItemsArray vv = new DisplayItemsArray();
            DisplayItems header = new DisplayItems();
            header.Add(new DisplayItem("Num", true));
            header.Add(new DisplayItem("Facteur", true));
            header.Add(new DisplayItem("Valeur", true));
            header.Add(new DisplayItem("Taux", true));
            header.Add(new DisplayItem("Cummul", true));
            vv.Add(header);
            double s = 0.0;
            for (int i = 0; i < nv; ++i)
            {
                DisplayItems line = new DisplayItems();
                line.Add(new DisplayItem((int)(i + 1)));
                String sname = String.Format("{0}{1}", prefix, i + 1);
                line.Add(new DisplayItem(sname));
                double x = vals[i];
                line.Add(new DisplayItem(x));
                line.Add(new DisplayItem(x / sum));
                s += x;
                line.Add(new DisplayItem(s / sum));
                vv.Add(line);
            }// i
            oRet.EigenValues = vv;
            //
            DisplayItemsArray vecs = new DisplayItemsArray();
            DisplayItems hh = new DisplayItems();
            hh.Add(new DisplayItem("Variable", true));
            var vx = p.EigenVectors;
            nv = vx.GetLength(1);
            int nr = vx.GetLength(0);
            var xnames = this.CurrentVariables.ToArray();
            for (int i = 0; i < nv; ++i)
            {
                String sname = String.Format("{0}{1}", prefix, i + 1);
                hh.Add(new DisplayItem(sname, true));
            }// i
            vecs.Add(hh);
            for (int i = 0; i < nr; ++i)
            {
                DisplayItems line = new DisplayItems();
                String name = xnames[i].Name;
                line.Add(new DisplayItem(name));
                for (int j = 0; j < nv; ++j)
                {
                    double x = vx[i, j];
                    line.Add(new DisplayItem(x));
                }// j
                vecs.Add(line);
            }// i
            oRet.EigenVectors = vecs;
            //
            DisplayItemsArray vecsv = new DisplayItemsArray();
            DisplayItems hhv = new DisplayItems();
            hhv.Add(new DisplayItem("Variable", true));
            var vxv = p.RecodedVars;
            nv = vxv.GetLength(1);
            nr = vxv.GetLength(0);
            for (int i = 0; i < nv; ++i)
            {
                String sname = String.Format("{0}{1}", prefix, i + 1);
                hhv.Add(new DisplayItem(sname, true));
            }// i
            vecsv.Add(hhv);
            for (int i = 0; i < nr; ++i)
            {
                DisplayItems line = new DisplayItems();
                String name = xnames[i].Name;
                line.Add(new DisplayItem(name));
                for (int j = 0; j < nv; ++j)
                {
                    double x = vxv[i, j];
                    line.Add(new DisplayItem(x));
                }// j
                vecsv.Add(line);
            }// i
            oRet.EigenVariables = vecsv;
            //
            DisplayItemsArray vecsx = new DisplayItemsArray();
            DisplayItems hhx = new DisplayItems();
            hhx.Add(new DisplayItem("Num", true));
            hhx.Add(new DisplayItem("Index", true));
            hhx.Add(new DisplayItem("Nom", true));
            hhx.Add(new DisplayItem("Photo", true));
            var vxx = p.RecodedInds;
            nv = vxx.GetLength(1);
            nr = vxx.GetLength(0);
            for (int i = 0; i < nv; ++i)
            {
                String sname = String.Format("{0}{1}", prefix, i + 1);
                hhx.Add(new DisplayItem(sname, true));
            }// i
            vecsx.Add(hhx);
            var inds = this.Individus.ToArray();
            for (int i = 0; i < nr; ++i)
            {
                DisplayItems line = new DisplayItems();
                line.Add(new DisplayItem(i+1));
                var ind = inds[i];
                line.Add(new DisplayItem(ind.IndivIndex));
                line.Add(new DisplayItem(ind.IdString));
                line.Add(new DisplayItem(ind.Name));
                if ((ind.PhotoData != null) && (ind.PhotoData.Length > 1))
                {
                    line.Add(new DisplayItem(ind.PhotoData));
                }
                else
                {
                    line.Add(new DisplayItem());
                }
                for (int j = 0; j < nv; ++j)
                {
                    double x = vxx[i, j];
                    line.Add(new DisplayItem(x));
                }// j
                vecsx.Add(line);
            }// i
            oRet.EigenIndivs = vecsx;
            return oRet;
        }// computeData
        private async void refreshIndivs()
        {
            var inds = m_main.AllIndividus;
            var vars = this.CurrentVariables;
            MatriceComputeMode mode = (this.IsRobust) ? MatriceComputeMode.modeRank : MatriceComputeMode.modeNothing;
            int nClasses = 5;
            var xx = await Task.Run<IEnumerable<IndivData>>(() =>
            {
                return GetIndivsData(inds, vars, mode, nClasses);
            });
            if (xx != null)
            {
                this.Individus = new IndivDatas(xx);
            }
        }// refreshIndivs
        #endregion // helpers
    }// AnaCompoModelView
}
