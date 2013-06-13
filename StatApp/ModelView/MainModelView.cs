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
    public class MainModelView : StatModelViewBase
    {
        #region Instance variables
        private Dictionary<int, OxyImage> m_imagesdict;
        private PhotosAssocModelView m_photosassoc;
        private DisplayItemsArray m_infos;
        private DisplayItemsArray m_initialdata;
        private DisplayItemsArray m_corrdisplay;
        private String m_factorprefix = "FACT";
        private AnaCompoModelView m_anacompomodel;
        private OrdModelView m_ordview;
        private PhotosModelView m_photomodelview;
        private bool m_busy = false;
        private StatDataSets m_statdatasets;
        private StatDataSet m_currentdataset;
        private VariableDesc m_currentvariable;
        private VariableDesc m_currentX;
        private VariableDesc m_currentY;
        private ValueDescs m_values;
        private int m_skip = 0;
        private int m_taken = 20;
        private int m_count = 0;
        private VariableDescs m_categvars;
        private VariableDescs m_numvars;
        private VariableDescs m_leftcategvars;
        private VariableDescs m_leftnumvars;
        private VariableDesc m_currentcateg;
        //
        private PlotModel m_boxplotmodel;
        private PlotModel m_normalmodel;
        private PlotModel m_histogmodel;
        private PlotModel m_boxplotcateg;
        private PlotModel m_correlmodel;
        //
        private CorrelData m_correldata;
        private bool m_haslabels = false;
        private bool m_haspoints = true;
        private bool m_hasimages = false;
        //
        #endregion // Instance variables
        #region Constructors
        public MainModelView(IServiceLocator servicelocator, IPageLocator pageLocator, NavigationService pNav)
            : base(servicelocator, pageLocator, pNav)
        {
        }
        #endregion // constructors
        #region Properties
        public bool IsBusy
        {
            get
            {
                bool bRet = m_busy;
                if (this.CurrentStatDataSet != null)
                {
                    bRet = bRet || this.CurrentStatDataSet.IsBusy;
                }
                return bRet;
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
        public Dictionary<int, OxyImage> ImagesDictionary
        {
            get
            {
                if ((m_imagesdict != null) && (m_imagesdict.Count > 0))
                {
                    return m_imagesdict;
                }
                var oSet = this.CurrentStatDataSet;
                if ((oSet != null) && oSet.IsValid)
                {
                    m_imagesdict = new Dictionary<int, OxyImage>();
                    var col = oSet.Individus;
                    foreach (var ind in col)
                    {
                        int index = ind.IndivIndex;
                        var data = ind.PhotoData;
                        if ((data != null) && (data.Length > 1))
                        {
                            OxyImage image = null;
                            try
                            {
                                image = new OxyImage(data);
                            }
                            catch (Exception /* ex */) { }
                            if (image != null)
                            {
                                m_imagesdict[index] = image;
                            }
                        }
                    }// ind
                }// oSet
                return (m_imagesdict == null) ? new Dictionary<int, OxyImage>() : m_imagesdict;
            }
            set
            {
                if (value != m_imagesdict)
                {
                    m_imagesdict = value;
                    NotifyPropertyChanged("ImagesDictionary");
                }
            }
        }// ImagesDictionary
        public PhotosAssocModelView PhotosAssocModelView
        {
            get
            {
                if (m_photosassoc == null)
                {
                    m_photosassoc = new PhotosAssocModelView(this);
                }
                return m_photosassoc;
            }
            set { }
        }// PhotosAssocModelView 
        public DisplayItemsArray VariablesInfos
        {
            get
            {
                if (m_infos == null)
                {
                    m_infos = new DisplayItemsArray();
                }
                return m_infos;
            }
            set
            {
                if (value != m_infos)
                {
                    m_infos = value;
                    NotifyPropertyChanged("VariablesInfos");
                }
            }
        }// VariablesInfos
        public DisplayItemsArray CorrelationsDisplay
        {
            get
            {
                if (m_corrdisplay == null)
                {
                    m_corrdisplay = new DisplayItemsArray();
                }
                return m_corrdisplay;
            }
            set
            {
                if (value != m_corrdisplay)
                {
                    m_corrdisplay = value;
                    NotifyPropertyChanged("CorrelationsDisplay");
                }
            }
        }// CorrelationsDisplay
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
        public AnaCompoModelView AnaCompoViewModel
        {
            get
            {
                if (m_anacompomodel == null)
                {
                    m_anacompomodel = new AnaCompoModelView(this);
                }
                return m_anacompomodel;
            }
            set { }
        }
        public String FactorPrefix
        {
            get
            {
                String s = String.IsNullOrEmpty(m_factorprefix) ? null : m_factorprefix.Trim();
                return String.IsNullOrEmpty(s) ? "FACT" : s;
            }
            set
            {
                String s = String.IsNullOrEmpty(m_factorprefix) ? null : m_factorprefix.Trim();
                String old = String.IsNullOrEmpty(s) ? "FACT" : s;
                String cur = String.IsNullOrEmpty(value) ? "FACT" : value.Trim();
                if (old.ToLower() == cur.ToLower())
                {
                    m_factorprefix = cur;
                    NotifyPropertyChanged("FactorPrefix");
                }
            }
        }// FactorPrefix
        public IndivDescs AllIndividus
        {
            get
            {
                if ((this.CurrentStatDataSet != null) && this.CurrentStatDataSet.IsValid)
                {
                    return this.CurrentStatDataSet.Individus;
                }
                return new IndivDescs();
            }
            set
            {
               NotifyPropertyChanged("AllIndividus");
            }
        }// AllIndividus
        public OrdModelView OrdModelView
        {
            get
            {
                if (m_ordview == null)
                {
                    m_ordview = new OrdModelView(this);
                }
                return m_ordview;
            }
            set { }
        }// OrdModelView
        public PhotosModelView PhotosModelView
        {
            get
            {
                if (m_photomodelview == null)
                {
                    m_photomodelview = new PhotosModelView(this.ServiceLocator, this.PageLocator, this.NavigationService);
                }
                return m_photomodelview;
            }
            set { }
        }// PhotosModelView
        public CorrelData CorrelData
        {
            get
            {
                if (m_correldata == null)
                {
                    m_correldata = new CorrelData();
                }
                return m_correldata;
            }
            set
            {
                if (value != m_correldata)
                {
                    m_correldata = value;
                    NotifyPropertyChanged("CorrelData");
                }
            }
        }// CorrelData
        public String XVarName
        {
            get
            {
                return this.CurrentXVariable.Name;
            }
            set
            {
                NotifyPropertyChanged("XVarName");
            }
        }
        public String YVarName
        {
            get
            {
                return this.CurrentYVariable.Name;
            }
            set
            {
                NotifyPropertyChanged("YVarName");
            }
        }
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
                    this.RefreshCorrelationPlot();
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
                    this.RefreshCorrelationPlot();
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
                    this.RefreshCorrelationPlot();
                }// change
            }
        }// HasPoints
        public VariableDesc CurrentXVariable
        {
            get
            {
                if (m_currentX == null)
                {
                    var cols = this.AllNumVariables;
                    if (cols.Count > 0)
                    {
                        m_currentX = cols.First();
                        this.NotifyPropertyChanged("XVarName");
                    }
                }
                if (m_currentX == null)
                {
                    m_currentX = new VariableDesc();
                }
                return m_currentX;
            }
            set
            {
                if (value != m_currentX)
                {
                    m_currentX = value;
                    this.NotifyPropertyChanged("CurrentXVariable");
                    this.NotifyPropertyChanged("XVarName");
                    this.CorrelationPlotModel = null;
                    this.RefreshCorrelationPlot();
                }
            }
        }// CurrentXVariable
        public VariableDesc CurrentYVariable
        {
            get
            {
                if (m_currentY == null)
                {
                    var cols = this.AllNumVariables;
                    var vx = this.CurrentXVariable;
                    if (cols.Count > 0)
                    {
                        foreach (var v in cols)
                        {
                            if ((vx != null) && (vx.Id == v.Id))
                            {
                                continue;
                            }
                            else
                            {
                                m_currentY = v;
                                this.NotifyPropertyChanged("YVarName");
                                break;
                            }
                        }// v
                    }
                }
                if (m_currentY == null)
                {
                    m_currentY = new VariableDesc();
                }
                return m_currentY;
            }
            set
            {
                if (value != m_currentX)
                {
                    m_currentY = value;
                    this.NotifyPropertyChanged("CurrentYVariable");
                    this.NotifyPropertyChanged("YVarName");
                    this.CorrelationPlotModel = null;
                    this.RefreshCorrelationPlot();
                }
            }
        }// CurrentXVariable
        public PlotModel CorrelationPlotModel
        {
            get
            {
                return m_correlmodel;
            }
            set
            {
                if (value != m_correlmodel)
                {
                    m_correlmodel = value;
                    NotifyPropertyChanged("CorrelationPlotModel");
                }
            }
        }// CorrelationPlotModel
        public PlotModel BoxPlotModel
        {
            get
            {
                return m_boxplotmodel;
            }
            set
            {
                if (value != m_boxplotmodel)
                {
                    m_boxplotmodel = value;
                    NotifyPropertyChanged("BoxPlotModel");
                }
            }
        }// BoxPlotModel
        public PlotModel NormalModel
        {
            get
            {
                return m_normalmodel;
            }
            set
            {
                if (value != m_normalmodel)
                {
                    m_normalmodel = value;
                    NotifyPropertyChanged("NormalModel");
                }
            }
        }// NormalModel
        public PlotModel HistogPlotModel
        {
            get
            {
                return m_histogmodel;
            }
            set
            {
                if (value != m_histogmodel)
                {
                    m_histogmodel = value;
                    NotifyPropertyChanged("HistogPlotModel");
                }
            }
        }// HistogPlotModel
        public PlotModel CategBoxPlotModel
        {
            get
            {
                return m_boxplotcateg;
            }
            set
            {
                if (value != m_boxplotcateg)
                {
                    m_boxplotcateg = value;
                    NotifyPropertyChanged("CategBoxPlotModel");
                }
            }
        }// CategBoxPlotModel
        public VariableDescs AllCategVariables
        {
            get
            {
                VariableDescs oRet = new VariableDescs();
                var col = this.Variables;
                var q = from x in col where x.IsCategVar orderby x.Name ascending select x;
                foreach (var p in q)
                {
                    if ((!p.IsIdVar) && (!p.IsNameVar) && (!p.IsInfoVar) && (!p.IsImageVar))
                    {
                        oRet.Add(p);
                    }
                }
                return oRet;
            }
            set
            {
                NotifyPropertyChanged("AllCategVariables");
            }
        }// AllCategVariables
        public VariableDescs AllNumVariables
        {
            get
            {
                VariableDescs oRet = new VariableDescs();
                var col = this.Variables;
                var q = from x in col where x.IsNumVar orderby x.Name ascending select x;
                foreach (var p in q)
                {
                    if ((!p.IsIdVar) && (!p.IsNameVar) && (!p.IsInfoVar) && (!p.IsImageVar))
                    {
                        oRet.Add(p);
                    }
                }
                return oRet;
            }
            set
            {
                NotifyPropertyChanged("AllNumVariables");
            }
        }// AllNumVariables
        public VariableDescs AllComputeVariables
        {
            get
            {
                VariableDescs oRet = new VariableDescs();
                var col = this.Variables;
                var q = from x in col orderby x.Name ascending select x;
                foreach (var p in q)
                {
                    if ((!p.IsIdVar) && (!p.IsNameVar) && (!p.IsInfoVar) && (!p.IsImageVar))
                    {
                        oRet.Add(p);
                    }
                }
                return oRet;
            }
            set
            {
                NotifyPropertyChanged("AllComputeVariables");
            }
        }// AllComputeVariables
        public VariableDescs LeftCategVariables
        {
            get
            {
                if (m_leftcategvars == null)
                {
                    List<VariableDesc> oList = new List<VariableDesc>();
                    var col = this.Variables;
                    if (col != null)
                    {

                        foreach (var v in col)
                        {
                            if ((v != null) && v.IsCategVar)
                            {
                                if ((!v.IsIdVar) && (!v.IsNameVar))
                                {
                                    oList.Add(v);
                                }
                            }
                        }// v
                        if (oList.Count > 1)
                        {
                            oList.Sort();
                        }
                        m_leftcategvars = new VariableDescs(oList);
                    }
                }
                return m_leftcategvars;
            }
            set
            {
                if (value != m_leftcategvars)
                {
                    m_leftcategvars = value;
                    NotifyPropertyChanged("LeftCategVariables");
                }
            }
        }// LeftCategVariables
        public VariableDescs LeftNumVariables
        {
            get
            {
                if (m_leftnumvars == null)
                {
                    List<VariableDesc> oList = new List<VariableDesc>();
                    var col = this.Variables;
                    if (col != null)
                    {

                        foreach (var v in col)
                        {
                            if ((v != null) && (!v.IsCategVar))
                            {
                                if ((!v.IsIdVar) && (!v.IsNameVar))
                                {
                                    oList.Add(v);
                                }
                            }
                        }// v
                        if (oList.Count > 1)
                        {
                            oList.Sort();
                        }
                    }
                    m_leftnumvars = new VariableDescs(oList);
                }
                return m_leftnumvars;
            }
            set
            {
                if (value != m_leftnumvars)
                {
                    m_leftnumvars = value;
                    NotifyPropertyChanged("LeftNumVariables");
                }
            }
        }// LeftNumVariables
        public VariableDesc CurrentCategVariable
        {
            get
            {
                if (m_currentcateg == null)
                {
                    m_currentcateg = new VariableDesc();
                }
                return m_currentcateg;
            }
            set
            {
                if (value != m_currentcateg)
                {
                    m_currentcateg = value;
                    NotifyPropertyChanged("CurrentCategVariable");
                    this.NormalModel = null;
                    this.CategBoxPlotModel = null;
                    this.CorrelationPlotModel = null;
                }
            }
        }// CurrentCategVariable
        public VariableDescs CategVariables
        {
            get
            {
                if (m_categvars == null)
                {
                    m_categvars = new VariableDescs();
                }
                return m_categvars;
            }
            set
            {
                if (value != m_categvars)
                {
                    m_categvars = value;
                    NotifyPropertyChanged("CategVariables");
                    this.CurrentCategVariable = null;
                }
            }
        }// CategVariables
        public VariableDescs NumVariables
        {
            get
            {
                if (m_numvars == null)
                {
                    m_numvars = new VariableDescs();
                }
                return m_numvars;
            }
            set
            {
                if (value != m_numvars)
                {
                    m_numvars = value;
                    NotifyPropertyChanged("NumVariables");
                }
            }
        }// NumVariables
        public int TotalValuesCount
        {
            get
            {
                return m_count;
            }
            set
            {
                if (value != m_count)
                {
                    m_count = value;
                    NotifyPropertyChanged("TotalValuesCount");
                }
            }
        }//TotalValusCount
        public String ValuesStatus
        {
            get
            {
                String sRet = "";
                var p = this.Values;
                if ((p != null) && (m_count > 0))
                {
                    int nEnd = m_skip + m_taken;
                    if (nEnd > m_count)
                    {
                        nEnd = m_count;
                    }
                    sRet = String.Format("Valeurs {0} à {1} sur {2}", m_skip + 1, nEnd, m_count);
                }
                return sRet;
            }
            set
            {
                NotifyPropertyChanged("ValuesStatus");
            }
        }// ValuesStatus
        public ValueDescs Values
        {
            get
            {
                if (m_values == null)
                {
                    m_values = new ValueDescs();
                }
                return m_values;
            }
            set
            {
                if (value != m_values)
                {
                    m_values = value;
                    NotifyPropertyChanged("Values");
                    NotifyPropertyChanged("ValuesStatus");
                }
            }
        }// Values
        public int Skip
        {
            get
            {
                return m_skip;
            }
            set
            {
                if ((value != m_skip) && (value >= 0))
                {
                    m_skip = value;
                    NotifyPropertyChanged("Skip");
                }
            }
        }// Skip
        public int Taken
        {
            get
            {
                return m_taken;
            }
            set
            {
                if ((value != m_taken) && (value >= 0))
                {
                    m_taken = value;
                    NotifyPropertyChanged("Taken");
                }
            }
        }// Taken
        public VariableDesc CurrentVariable
        {
            get
            {
                return (m_currentvariable == null) ? new VariableDesc() : m_currentvariable;
            }
            set
            {
                if (value != m_currentvariable)
                {
                    m_currentvariable = value;
                    NotifyPropertyChanged("CurrentVariable");
                    NotifyPropertyChanged("VariableName");
                    NotifyPropertyChanged("VariableType");
                    if (m_currentvariable != null)
                    {
                        this.Skip = 0;
                        m_values = null;
                        NotifyPropertyChanged("Skip");
                        NotifyPropertyChanged("Values");
                    }
                    this.NormalModel = null;
                    this.HistogPlotModel = null;
                    this.BoxPlotModel = null;
                    this.RefreshCategBoxPlot();
                    this.RefreshNormalPlotModel();
                    this.RefreshHistogPlotModel();
                }
            }
        }// CurrentVariable
        public String VariableName
        {
            get
            {
                if (this.CurrentVariable != null)
                {
                    return this.CurrentVariable.Name;
                }
                return String.Empty;
            }
            set
            {
                NotifyPropertyChanged("VariableName");
            }
        }// VariableName
        public String VariableType
        {
            get
            {
                if (this.CurrentVariable != null)
                {
                    return this.CurrentVariable.DataType;
                }
                return String.Empty;
            }
            set
            {
                NotifyPropertyChanged("VariableType");
            }
        }// VariableType
        public String DataSetName
        {
            get
            {
                if (this.CurrentStatDataSet != null)
                {
                    return this.CurrentStatDataSet.Name;
                }
                return String.Empty;
            }
            set
            {
                NotifyPropertyChanged("DataSetName");
            }
        }// DataSetName
        public VariableDescs Variables
        {
            get
            {
                var p = this.CurrentStatDataSet;
                return ((p != null)  && p.IsValid)? p.Variables : new VariableDescs();
            }
            set
            {
                var p = this.CurrentStatDataSet;
                if (p != null)
                {
                    p.Variables = value;
                    NotifyPropertyChanged("Variables");
                }
               
            }
        }// Variables
        public StatDataSet CurrentStatDataSet
        {
            get
            {
                return (m_currentdataset == null) ? new StatDataSet() : m_currentdataset;
            }
            set
            {
                if (value != m_currentdataset)
                {
                    if (m_currentdataset != null)
                    {
                        m_currentdataset.PropertyChanged -= m_currentdataset_PropertyChanged;
                    }
                    m_currentdataset = value;
                    this.Variables = null;
                    if (m_currentdataset != null)
                    {
                        m_currentdataset.PropertyChanged += m_currentdataset_PropertyChanged;
                        if ((!m_currentdataset.HasVariables) || (!m_currentdataset.HasIndivs))
                        {
                            m_currentdataset.Refresh(this.DataService);
                        }
                        NotifyPropertyChanged("CurrentStatDataSet");
                        NotifyPropertyChanged("DataSetName");
                    }
                }
            }
        }

        void m_currentdataset_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            String name = e.PropertyName;
            if (name == "IsBusy")
            {
                NotifyPropertyChanged("IsBusy");
                return;
            }
            if (name == "IsDone")
            {
                NotifyPropertyChanged("CurrentStatDataSet");
                NotifyPropertyChanged("DataSetName");
                NotifyPropertyChanged("Variables");
                NotifyPropertyChanged("AllVariables");
                NotifyPropertyChanged("AllIndividus");
                NotifyPropertyChanged("ImagesDictionary");
                this.CurrentXVariable = null;
                this.CurrentYVariable = null;
                this.CurrentVariable = null;
                this.CategVariables = null;
                this.NumVariables = null;
                this.LeftNumVariables = null;
                this.LeftCategVariables = null;
                this.NormalModel = null;
                this.HistogPlotModel = null;
                this.CategBoxPlotModel = null;
                this.RefreshVariables();
            }
        }// CurrentDataSet
        public StatDataSets StatDataSets
        {
            get
            {
                if (m_statdatasets == null)
                {
                    m_statdatasets = new StatDataSets();
                }
                return m_statdatasets;
            }
            set
            {
                if (value != m_statdatasets)
                {
                    m_statdatasets = value;
                    NotifyPropertyChanged("StatDataSets");
                    this.CurrentStatDataSet = null;
                }
            }
        }// StatDataSets
        #endregion // Properties
        #region Methods
        public void RefreshGraphData()
        {
            this.RefreshNormalPlotModel();
            this.RefreshBoxPlotModel();
        }// RefreshGraphData
        public async void RefreshDataSets()
        {
            var pMan = this.DataService;
            if (pMan != null)
            {
                var xx = await Task.Run<Tuple<IEnumerable<StatDataSet>, Exception>>(() =>
                {
                    return pMan.GetAllStatDataSets();
                });
                if (xx != null)
                {
                    if ((xx.Item1 != null) && (xx.Item2 == null))
                    {
                        this.StatDataSets = new StatDataSets(xx.Item1);
                    }
                    else if (xx.Item2 != null)
                    {
                        ShowError(xx.Item2);
                    }
                }// xx
            }// pMan
        }// RefreshDataSets
        public async void RefreshVariables()
        {
            this.VariablesInfos = await CreateVariablesInfoDisplayAsync(this.Variables);
            this.CorrelationsDisplay = await ComputeCorrelationsDisplayAsync(this.Variables);
            this.RefreshInitialData();
        }// RefreshVariables
        public void AddCategVariables(IEnumerable<VariableDesc> col)
        {
            if (col == null)
            {
                return;
            }
            var old = this.CategVariables.ToList();
            var oLeft = this.LeftCategVariables;
            foreach (var v in col)
            {
                if ((v != null) && v.IsCategVar && (!old.Contains(v)))
                {
                    old.Add(v);
                    if (oLeft.Contains(v))
                    {
                        oLeft.Remove(v);
                    }
                }
            }// v
            if (old.Count > 1)
            {
                old.Sort();
            }
            this.CategVariables = new VariableDescs(old);
            NotifyPropertyChanged("LeftCategVariables");
        }//AddCategVariables
        public void RemoveCategVariables(IEnumerable<VariableDesc> col)
        {
            if (col == null)
            {
                return;
            }
            var src = col.ToList();
            var dest = this.CategVariables.ToList();
            var oLeft = this.LeftCategVariables;
            foreach (var v in src)
            {
                if ((v != null) && v.IsCategVar && dest.Contains(v))
                {
                    dest.Remove(v);
                    if (!oLeft.Contains(v))
                    {
                        oLeft.Add(v);
                    }
                }
            }// v
            if (dest.Count > 1)
            {
                dest.Sort();
            }
            VariableDesc oldCateg = this.CurrentCategVariable;
            this.CategVariables = new VariableDescs(dest);
            if ((oldCateg != null) && oldCateg.IsValid)
            {
                if (this.CategVariables.Contains(oldCateg))
                {
                    this.CurrentCategVariable = oldCateg;
                }
            }
            NotifyPropertyChanged("LeftCategVariables");
        }// RemoveCategVariables
        public void AddNumVariables(IEnumerable<VariableDesc> col)
        {
            if (col == null)
            {
                return;
            }
            var old = this.NumVariables.ToList();
            var oLeft = this.LeftNumVariables;
            foreach (var v in col)
            {
                if ((v != null) && v.IsNumVar)
                {
                    old.Add(v);
                    if (oLeft.Contains(v))
                    {
                        oLeft.Remove(v);
                    }
                }
            }// v
            if (old.Count > 1)
            {
                old.Sort();
            }
            this.NumVariables = new VariableDescs(old);
            NotifyPropertyChanged("LeftNumVariables");
        }//AddNumVariables
        public void RemoveNumVariables(IEnumerable<VariableDesc> col)
        {
            if (col == null)
            {
                return;
            }
            var src = col.ToList();
            var dest = this.NumVariables.ToList();
            var oLeft = this.LeftNumVariables;
            foreach (var v in src)
            {
                if ((v != null) && v.IsNumVar && dest.Contains(v))
                {
                    dest.Remove(v);
                    if (!oLeft.Contains(v))
                    {
                        oLeft.Add(v);
                    }
                }
            }// v
            if (dest.Count > 1)
            {
                dest.Sort();
            }
            this.NumVariables = new VariableDescs(dest);
            NotifyPropertyChanged("LeftNumVariables");
        }// RemoveNumgVariables
        #endregion
        #region Plot Helpers
        public Tuple<PlotModel, CorrelData> createCorrelationPlotModel()
        {
            PlotModel model = null;
            CorrelData odata = null;
            bool bLabel = this.HasLabels;
            bool bImage = this.HasImages;
            bool bPoints = this.HasPoints;
            if ((!bLabel) && (!bImage) && (!bPoints))
            {
                return new Tuple<PlotModel, CorrelData>(model, odata);
            }
            var pMan = this.DataService;
            VariableDesc oVarY = this.CurrentYVariable;
            VariableDesc oVarX = this.CurrentXVariable;
            if ((pMan == null) || (oVarX == null) || (oVarY == null))
            {
                return new Tuple<PlotModel, CorrelData>(model, odata);
            }
            if (oVarX == oVarY)
            {
                return new Tuple<PlotModel, CorrelData>(model, odata);
            }
            if ((!oVarX.IsNumVar) || (!oVarX.IsValid) || (!oVarY.IsNumVar) || (!oVarY.IsValid))
            {
                return new Tuple<PlotModel, CorrelData>(model, odata);
            }
            List<VariableDesc> clist = new List<VariableDesc>();
            clist.Add(oVarX);
            clist.Add(oVarY);
            var vAll = GetCommonValues(clist);
            IEnumerable<ValueDesc> xVals = null;
            IEnumerable<ValueDesc> yVals = null;
            foreach (var v in vAll.Keys)
            {
                if (v.Id == oVarX.Id)
                {
                    xVals = vAll[v];
                }
                else if (v.Id == oVarY.Id)
                {
                    yVals = vAll[v];
                }
            }// v
            if ((xVals == null) || (yVals == null))
            {
                return new Tuple<PlotModel, CorrelData>(null, null);
            }
            List<double> xlist = new List<double>();
            List<double> ylist = new List<double>();
            IEnumerable<ValueDesc> categVals = null;
            if ((this.CurrentCategVariable != null) && this.CurrentCategVariable.IsValid)
            {
                categVals = this.CurrentCategVariable.Values;
            }
            Dictionary<int, String> categDict = new Dictionary<int, string>();
            Dictionary<int, Tuple<double, double>> vals = new Dictionary<int, Tuple<double, double>>();
            List<DataPoint> alldata = new List<DataPoint>();
            Dictionary<int, String> namesdict = new Dictionary<int, string>();
            foreach (var vx in xVals)
            {
                int index = vx.Index;
                if (index >= 0)
                {
                    var qx = from x in this.AllIndividus where x.IndivIndex == index select x;
                    if (qx.Count() > 0)
                    {
                        var z = qx.First();
                        String sval = z.IdString;
                        if (String.IsNullOrEmpty(sval))
                        {
                            sval = z.Name;
                        }
                        if (!String.IsNullOrEmpty(sval))
                        {
                            namesdict[index] = sval;
                        }
                    }
                    var q = from x in yVals where x.Index == index select x;
                    if (q.Count() > 0)
                    {
                        var vy = q.First();
                        double x = vx.DoubleValue;
                        double y = vy.DoubleValue;
                        vals[index] = new Tuple<double, double>(x, y);
                        xlist.Add(x);
                        ylist.Add(y);
                        alldata.Add(new DataPoint(x, y));
                        if (categVals == null)
                        {
                            categDict[index] = DEFAULT_SERIE_NAME;
                        }
                        else
                        {
                            var qq = from z in categVals where z.Index == index select z;
                            if (qq.Count() > 0)
                            {
                                var key = StatHelpers.ConvertValue(qq.First().DataStringValue);
                                if (!String.IsNullOrEmpty(key))
                                {
                                    categDict[index] = key;
                                }
                                else
                                {
                                    categDict[index] = DEFAULT_SERIE_NAME;
                                }
                            }
                            else
                            {
                                categDict[index] = DEFAULT_SERIE_NAME;
                            }
                        }
                    }
                }
            }// v
            String sTitle = String.Format("{0} / {1}", oVarY.Name, oVarX.Name);
            model = new PlotModel(sTitle) { LegendPlacement = LegendPlacement.Outside };
            model.LegendPlacement = LegendPlacement.Outside;
            model.Axes.Add(new LinearAxis(AxisPosition.Bottom, xlist.Min(), xlist.Max(), oVarX.Name));
            model.Axes.Add(new LinearAxis(AxisPosition.Left, ylist.Min(), ylist.Max(), oVarY.Name));
            if (bLabel)
            {
                foreach (var index in namesdict.Keys)
                {
                    if (vals.ContainsKey(index))
                    {
                        IndivDesc ind = null;
                        var qz = from x in this.AllIndividus where x.IndivIndex == index select x;
                        if (qz.Count() > 0)
                        {
                            ind = qz.First();
                        }
                        var t = vals[index];
                        double xx = t.Item1;
                        double yy = t.Item2;
                        String name = namesdict[index];
                        if (!String.IsNullOrEmpty(name))
                        {
                            model.Annotations.Add(new TextAnnotation
                            {
                                Position = new DataPoint(xx, yy),
                                Text = name,
                                FontWeight = FontWeights.Bold,
                                Tag = ind
                            });
                        }
                    }
                }// index
            }// bLabel
            if (bImage && (this.ImagesDictionary != null))
            {
                if (xlist.Count < 2)
                {
                    return new Tuple<PlotModel, CorrelData>(null, null);
                }
                double xwidth = 3.0 * (xlist.Max() - xlist.Min()) / xlist.Count;
                foreach (var index in namesdict.Keys)
                {
                    if (vals.ContainsKey(index))
                    {
                        if (this.ImagesDictionary.ContainsKey(index))
                        {
                            IndivDesc ind = null;
                            var qz = from x in this.AllIndividus where x.IndivIndex == index select x;
                            if (qz.Count() > 0)
                            {
                                ind = qz.First();
                            }
                            var t = vals[index];
                            double xx = t.Item1;
                            double yy = t.Item2;
                            model.Annotations.Add(new ImageAnnotation
                            {
                                ImageSource = this.ImagesDictionary[index],
                                X = new PlotLength(xx, PlotLengthUnit.Data),
                                Y = new PlotLength(yy, PlotLengthUnit.Data),
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Middle,
                                Width = new PlotLength(xwidth, PlotLengthUnit.Data)
                            });
                        }
                    }
                }// index
            }// Image
            if (bPoints)
            {
                Dictionary<String, ScatterSeries> oSeries = new Dictionary<string, ScatterSeries>();
                foreach (var index in categDict.Keys)
                {
                    String scateg = categDict[index];
                    if (!oSeries.ContainsKey(scateg))
                    {
                        oSeries[scateg] = new ScatterSeries(scateg) { MarkerType = MarkerType.Circle };
                    }
                    var q = from x in this.AllIndividus where x.IndivIndex == index select x;
                    if (q.Count() > 0)
                    {
                        var ind = q.First();
                        if (vals.ContainsKey(index))
                        {
                            var t = vals[index];
                            double xx = t.Item1;
                            double yy = t.Item2;
                            (oSeries[scateg]).Points.Add(new ScatterPoint(xx, yy) { Tag = ind });
                        }
                    }//
                }// index
                foreach (var s1 in oSeries.Values)
                {
                    model.Series.Add(s1);
                }
            }// bPoints
            odata = ComputeCorrelation(ylist.ToArray(), xlist.ToArray());
            if (odata != null)
            {
                odata.FirstName = oVarY.Name;
                odata.SecondName = oVarX.Name;
            }// odata
            double a, b;
            LeastSquaresFit(alldata, out a, out b);
            model.Annotations.Add(new LineAnnotation { Slope = a, Intercept = b, Text = "Moindres Carrés" });
            return new Tuple<PlotModel, CorrelData>(model, odata);
        }//createCorrelationPlotModel
        public Task<PlotModel> createBoxPlotModelAsync()
        {
            return Task.Run<PlotModel>(() =>
            {

                var vars = this.NumVariables;
                List<String> names = new List<String>();
                String title = this.DataSetName;
                List<double[]> data = new List<double[]>();
                foreach (var v in vars)
                {
                    var vals = v.Values;
                    List<double> l = new List<double>();
                    foreach (var x in vals)
                    {
                        if (x.Index >= 0)
                        {
                            String s = StatHelpers.ConvertValue(x.DataStringValue);
                            if (!String.IsNullOrEmpty(s))
                            {
                                l.Add(x.DoubleValue);
                            }
                        }
                    }// v
                    if (l.Count > 1)
                    {
                        names.Add(v.Name);
                        data.Add(l.ToArray());
                    }
                }// v
                return createBoxPlotModel(data, names.ToArray(), title);
            });
        }// createBoxPlotModelAsyn
        public async void RefreshBoxPlotModel()
        {
            var model = await createBoxPlotModelAsync();
            this.BoxPlotModel = model;
        }// RefreshBoxPlotModel
        public async void RefreshNormalPlotModel()
        {
            VariableDesc oVar = this.CurrentVariable;
            if (oVar == null)
            {
                return;
            }
            if (!oVar.IsNumVar)
            {
                return;
            }
            var oCateg = this.CurrentCategVariable;
            var model = await Task.Run<PlotModel>(() =>
            {
                return createNormalPlotModel(oCateg, oVar);
            });
            this.NormalModel = model;
        }// RefreshNormalPlotMode
        public async void RefreshHistogPlotModel()
        {
            VariableDesc oVar = this.CurrentVariable;
            if (oVar == null)
            {
                return;
            }
            if (!oVar.IsNumVar)
            {
                return;
            }
            var col = oVar.Values;
            List<double> oList = new List<double>();
            foreach (var v in col)
            {
                oList.Add(v.DoubleValue);
            }// v
            double[] data = oList.ToArray();
            if (data.Length < 2)
            {
                return;
            }
            var model = await Task.Run<PlotModel>(() =>
            {
                return createHistogPlotModel(data, oVar.Name);
            });
            this.HistogPlotModel = model;
        }// RefreshBoxPlotModel
        public async void RefreshCategBoxPlot()
        {
            VariableDesc oVar = this.CurrentVariable;
            VariableDesc oCateg = this.CurrentCategVariable;
            if ((!oVar.IsNumVar) || (!oVar.IsValid))
            {
                return;
            }
            List<double[]> oListData = new List<double[]>();
            List<String> names = new List<string>();
            if ((oCateg != null) && (oCateg.IsValid))
            {
                var dict = ComputeVariableCategValues(oCateg, oVar);
                foreach (var s in dict.Keys)
                {
                    names.Add(s);
                    List<double> l = new List<double>();
                    var col = dict[s];
                    foreach (var v in col)
                    {
                        l.Add(v.DoubleValue);
                    }
                    oListData.Add(l.ToArray());
                }
            }
            else
            {
                var col = oVar.Values;
                List<double> l = new List<double>();
                foreach (var v in col)
                {
                    l.Add(v.DoubleValue);
                }
                oListData.Add(l.ToArray());
                names.Add(oVar.Name);
            }
            if (oListData.Count < 1)
            {
                return;
            }
            var model = await Task.Run<PlotModel>(() =>
            {
                return createBoxPlotModel(oListData, names.ToArray(), oVar.Name);
            });
            if (model != null)
            {
                model.Title = oVar.Name;
                String ss = (oCateg != null) ? oCateg.Name : "";
                ss += " ";
                foreach (var s in names)
                {
                    if (ss.Trim().Length < 1)
                    {
                        ss = s;
                    }
                    else
                    {
                        ss += ", " + s;
                    }
                }
                model.Subtitle = ss;
            }
            this.CategBoxPlotModel = model;
        }// RefreshCategBoxPlot
        public async void RefreshCorrelationPlot()
        {
            var xx = await Task.Run<Tuple<PlotModel, CorrelData>>(() =>
            {
                return createCorrelationPlotModel();
            });
            if (xx != null)
            {
                this.CorrelationPlotModel = xx.Item1;
                this.CorrelData = xx.Item2;
            }
        }// RefreshCorrelationPlot
        #endregion // PlotHelpers
        public async void RefreshInitialData()
        {
            this.InitialData = await CreateDataDisplayAsync(this.AllIndividus, this.Variables, this.Variables);
        }// refreshInitailData
    }// classMainModelView 
}
