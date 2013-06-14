using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.ObjectModel;
/////////////////////////////////////
using System.Windows.Navigation;
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
    public class OrdModelView : StatModelViewBase
    {
        private String[] TAG_AXES = new String[] { "Categ Utility", "KMeans", "Hierachical" };
        private MatriceComputeMode[] TAB_MODES = new MatriceComputeMode[]{MatriceComputeMode.modeNothing,MatriceComputeMode.modeNormalize,
            MatriceComputeMode.modeProfil,MatriceComputeMode.modeRank};
        #region Instance Variables
        private int[] m_pcolindex;
        private int[] m_prowindex;
        private PlotModel m_rowarrangeplot;
        private DisplayItemsArray m_sortedindivsdata;
        private DisplayItemsArray m_clusterdata;
        private bool m_haslabels = false;
        private bool m_haspoints = true;
        private bool m_hasimages = false;
        private DisplayItemsArray m_orddisplay;
        private String m_xaxe;
        private String m_yaxe;
        private ObservableCollection<String> m_axes;
        private MatriceModelView m_matricemodelview;
        private IndivDatas m_ordindivs;
        private TreeLinkType m_linktype = TreeLinkType.linkmean;
        private PlotModel m_hierarshowmodel;
        private PlotModel m_kmeansshowmodel;
        private PlotModel m_clustershowmodel;
        private PlotModel m_arrangeshowmodel;
        private PlotModel m_combinedshowmodel;
        private CategClusterSet m_hierarclusterset;
        private CategClusterSet m_categclusterset;
        private CategClusterSet m_kmeansclusterset;
        private VariableDesc m_currentX;
        private VariableDesc m_currentY;
        private bool m_busy = false;
        private int m_nbclusters = 5;
        private int m_categiterations = 20;
        private MainModelView m_main;
        private IndivDatas m_indivs;
        private VariableDescs m_currentvariables;
        private VariableDescs m_leftvariables;
        private int m_nclasses = 5;
        private IndivData m_currentindiv;
        private Dictionary<String, int> m_categdict;
        private ObservableCollection<String> m_linktypes;
        private String m_currentlinktype = "Lien Moyen";
        private MatriceComputeMode m_computemode = MatriceComputeMode.modeNothing;
        #endregion // Instance Variables
        #region Constructors
        public OrdModelView()
        {
        }
        public OrdModelView(MainModelView pMain)
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
            if (name == "AllIndividus")
            {
                this.Individus = null;
                RefreshVariablesValues();
            }
            else if (name == "AllComputeVariables")
            {
                this.AllComputeVariables = null;
                var col = new VariableDescs();
                foreach (var v in m_main.AllComputeVariables)
                {
                    col.Add(v);
                }
                this.LeftVariables = col;
                this.CurrentVariables = null;
            }
            else if (name == "AllCategVariables")
            {
                NotifyPropertyChanged("AllCategVariables");
            }
            else if (name == "CurrentCategVariable")
            {
                this.updateCategValues();
                NotifyPropertyChanged("CurrentCategVariable");
            }
            else if (name == "AllNumVariables")
            {
                this.CurrentXVariable = null;
                this.CurrentYVariable = null;
                NotifyPropertyChanged("NumVariables");
                NotifyPropertyChanged("AllNumVariables");
            }
            m_busy = false;
        }// PhotosModelView
        #endregion // Constructors
        public void RefreshLeftVariables()
        {
            var p = this.LeftVariables;
            if (p.Count < 1)
            {
                var col = new VariableDescs();
                foreach (var v in m_main.AllComputeVariables)
                {
                    col.Add(v);
                }
                this.LeftVariables = col;
                this.CurrentVariables = null;
            }
        }// RefreshComputeVariables
        #region Properties
        public int[] ColIndexes
        {
            get
            {
                if (m_pcolindex == null)
                {
                    m_pcolindex = new int[0];
                }
                return m_pcolindex;
            }
            set
            {
                if (value != m_pcolindex)
                {
                    m_pcolindex = value;
                    NotifyPropertyChanged("ColIndexes");
                }
            }
        }// Colindexes
        public int[] RowIndexes
        {
            get
            {
                if (m_prowindex == null)
                {
                    m_prowindex = new int[0];
                }
                return m_prowindex;
            }
            set
            {
                if (value != m_prowindex)
                {
                    m_prowindex = value;
                    NotifyPropertyChanged("RowsIndexes");
                }
            }
        }// Rowsindexes
        public PlotModel CombinedShowModel
        {
            get
            {
                return m_combinedshowmodel;
            }
            set
            {
                if (value != m_combinedshowmodel)
                {
                    m_combinedshowmodel = value;
                    NotifyPropertyChanged("CombinedShowModel");
                }
            }
        }// CombinedShoModel
        public PlotModel RowArrangePlot
        {
            get
            {
                return m_rowarrangeplot;
            }
            set
            {
                if (value != m_rowarrangeplot)
                {
                    m_rowarrangeplot = value;
                    NotifyPropertyChanged("RowArrangePlot");
                }
            }
        }// RowArrangePlot
        public DisplayItemsArray SortedIndivsData
        {
            get
            {
                if (m_sortedindivsdata == null)
                {
                    m_sortedindivsdata = new DisplayItemsArray();
                }
                return m_sortedindivsdata;
            }
            set
            {
                if (value != m_sortedindivsdata)
                {
                    m_sortedindivsdata = value;
                    NotifyPropertyChanged("SortedIndivsData");
                }
            }
        }// SortedIndivsData
        public MatriceComputeMode MatriceMode
        {
            get
            {
                return m_computemode;
            }
            set
            {
                if (value != m_computemode)
                {
                    m_computemode = value;
                    NotifyPropertyChanged("MatriceMode");
                    this.onMatriceModeChanged();
                }
            }
        }// MatriceMode
        public IEnumerable<MatriceComputeMode> MatriceModes
        {
            get
            {
                return TAB_MODES;
            }
            set { }
        }// MatriceModes
        public bool IsBusy
        {
            get
            {
                return m_main.IsBusy || m_busy;
            }
            set
            {
                if (value != m_busy)
                {
                    m_busy = value;
                    NotifyPropertyChanged("IsBusy");
                }
            }
        }// IsBuy
        public DisplayItemsArray ClustersData
        {
            get
            {
                if (m_clusterdata == null)
                {
                    m_clusterdata = new DisplayItemsArray();
                }
                return m_clusterdata;
            }
            set
            {
                if (value != m_clusterdata)
                {
                    m_clusterdata = value;
                    NotifyPropertyChanged("ClustersData");
                }
            }
        }// ClustersData
        public DisplayItemsArray OrdDisplayData
        {
            get
            {
                if (m_orddisplay == null)
                {
                    m_orddisplay = new DisplayItemsArray();
                }
                return m_orddisplay;
            }
            set
            {
                if (value != m_orddisplay)
                {
                    m_orddisplay = value;
                    NotifyPropertyChanged("OrdDisplayData");
                }
            }
        }// OrdDisplayData
        public ObservableCollection<String> OrdAxes
        {
            get
            {
                if (m_axes == null)
                {
                    m_axes = new ObservableCollection<string>(new String[] { "Categ Utility", "KMeans", "Hierachical" });
                }
                return m_axes;
            }
            set { }
        }// OrdAxes
        public String XAxe
        {
            get
            {
                return m_xaxe;
            }
            set
            {
                if (value != m_xaxe)
                {
                    m_xaxe = value;
                    NotifyPropertyChanged("XAxe");
                }
            }
        }// XAxe
        public String YAxe
        {
            get
            {
                return m_yaxe;
            }
            set
            {
                if (value != m_yaxe)
                {
                    m_yaxe = value;
                    NotifyPropertyChanged("YAxe");
                }
            }
        }// XAxe
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
                    this.RefreshShowPlot();
                }
            }
        }//HasLabels
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
                    this.RefreshShowPlot();
                }
            }
        }//HasPoints
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
                    this.RefreshShowPlot();
                }
            }
        }//HasImages
        public Dictionary<int, OxyImage> ImagesDictionary
        {
            get
            {
                return m_main.ImagesDictionary;
            }
            set
            {
                NotifyPropertyChanged("ImagesDictionary");
            }
        }// ImagesDictionary
        public MainModelView MainModelView
        {
            get
            {
                return m_main;
            }
            set { }
        }// MainModelView
        public MatriceModelView MatriceModelView
        {
            get
            {
                if (m_matricemodelview == null)
                {
                    m_matricemodelview = new MatriceModelView(this);
                }
                return m_matricemodelview;
            }
            set { }
        }// MaticeModelView
        public IndivDatas SortedIndivs
        {
            get
            {
                if (m_ordindivs == null)
                {
                    m_ordindivs = new IndivDatas();
                }
                return m_ordindivs;
            }
            set
            {
                if (value != m_ordindivs)
                {
                    m_ordindivs = value;
                    NotifyPropertyChanged("SortedIndivs");
                }//
            }
        }// SortedIndivs
        public ObservableCollection<String> LinkTypes
        {
            get
            {
                if (m_linktypes == null)
                {
                    m_linktypes = new ObservableCollection<string>(new String[] { "Lien Moyen", "Lien Minimal", "Lien Maximal" });
                }
                return m_linktypes;
            }
            set { }
        }// TLinkTypes
        public String CurrentLinkTypeString
        {
            get
            {
                return m_currentlinktype;
            }
            set
            {
                if (value != m_currentlinktype)
                {
                    m_currentlinktype = value;
                    if (!String.IsNullOrEmpty(m_currentlinktype))
                    {
                        String s = m_currentlinktype.Trim().ToLower();
                        if (s.Contains("moyen"))
                        {
                            this.LinkType = TreeLinkType.linkmean;
                        }
                        else if (s.Contains("minimal"))
                        {
                            this.LinkType = TreeLinkType.linkmin;
                        }
                        else if (s.Contains("maximal"))
                        {
                            this.LinkType = TreeLinkType.linkmax;
                        }
                    }
                }
            }
        }// CurrentLinkTypeString
        public TreeLinkType LinkType
        {
            get
            {
                return m_linktype;
            }
            set
            {
                if (value != m_linktype)
                {
                    m_linktype = value;
                    NotifyPropertyChanged("LinkType");
                    this.HierarClusterSet = null;
                    this.UpdateClusters(this.ClassesCount, this.IterationsCount, CancellationToken.None, null);
                }
            }
        }// LinkType
        public PlotModel HierarShowModel
        {
            get
            {
                return m_hierarshowmodel;
            }
            set
            {
                if (value != m_hierarshowmodel)
                {
                    m_hierarshowmodel = value;
                    NotifyPropertyChanged("HierarShowModel");
                }
            }
        }// HierarShowModel
        public PlotModel ArrangeShowModel
        {
            get
            {
                return m_arrangeshowmodel;
            }
            set
            {
                if (value != m_arrangeshowmodel)
                {
                    m_arrangeshowmodel = value;
                    NotifyPropertyChanged("ArrangeShowModel");
                }
            }
        }// ArrangeShowModel
        public PlotModel KMeansShowModel
        {
            get
            {
                return m_kmeansshowmodel;
            }
            set
            {
                if (value != m_kmeansshowmodel)
                {
                    m_kmeansshowmodel = value;
                    NotifyPropertyChanged("KMeansShowModel");
                }
            }
        }// KMeansShowModel
        public PlotModel ClusterShowModel
        {
            get
            {
                return m_clustershowmodel;
            }
            set
            {
                if (value != m_clustershowmodel)
                {
                    m_clustershowmodel = value;
                    NotifyPropertyChanged("ClusterShowModel");
                }
            }
        }// ClusterShowModel
        public CategClusterSet CategClusterSet
        {
            get
            {
                return m_categclusterset;
            }
            set
            {
                if (value != m_categclusterset)
                {
                    m_categclusterset = value;
                    NotifyPropertyChanged("CategClusterSet");
                    this.IsBusy = true;
                    this.RefreshClusterData();
                    this.updateCategClusterSet();
                }
            }
        }// CategClusterSet
        public CategClusterSet KMeansClusterSet
        {
            get
            {
                return m_kmeansclusterset;
            }
            set
            {
                if (value != m_kmeansclusterset)
                {
                    m_kmeansclusterset = value;
                    this.RefreshClusterData();
                    NotifyPropertyChanged("KMeansClusterSet");
                    this.IsBusy = true;
                    this.updateKMeansClusterSet();
                }
            }
        }// CategClusterSet
        public CategClusterSet HierarClusterSet
        {
            get
            {
                return m_hierarclusterset;
            }
            set
            {
                if (value != m_hierarclusterset)
                {
                    m_hierarclusterset = value;
                    this.RefreshClusterData();
                    NotifyPropertyChanged("HierarClusterSet");
                    this.IsBusy = true;
                    this.updateHierarClusterSet();
                }
            }
        }// CategClusterSet
        public VariableDescs NumVariables
        {
            get
            {
                return m_main.AllNumVariables;
            }
            set
            {
                NotifyPropertyChanged("NumVariables");
            }
        }// NumVariables
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
        public VariableDesc CurrentXVariable
        {
            get
            {
                if (m_currentX == null)
                {
                    var col = this.AllNumVariables;
                    if (col.Count > 0)
                    {
                        m_currentX = col.First();
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
                    RefreshShowPlot();
                }
            }
        }// CurrentXVariable
        public VariableDesc CurrentYVariable
        {
            get
            {
                if (m_currentY == null)
                {
                    var col = this.AllNumVariables;
                    var vx = this.CurrentXVariable;
                    foreach (var v in col)
                    {
                        if (v.IsValid)
                        {
                            if (vx.IsValid && (vx.Id == v.Id))
                            {
                                continue;
                            }
                            else
                            {
                                m_currentY = v;
                                break;
                            }
                        }
                    }// col
                }// m_currenty
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
                    RefreshShowPlot();
                }
            }
        }// CurrentXVariable
        public int ClustersCount
        {
            get
            {
                return m_nbclusters;
            }
            set
            {
                if ((value != m_nbclusters) && (value > 1))
                {
                    m_nbclusters = value;
                    this.NotifyPropertyChanged("ClustersCount");
                    this.UpdateClusters(this.ClassesCount, this.IterationsCount, CancellationToken.None, null);
                }
            }
        }// ClustersCount
        public int IterationsCount
        {
            get
            {
                return m_categiterations;
            }
            set
            {
                if ((value != m_categiterations) && (value > 0))
                {
                    m_categiterations = value;
                    this.NotifyPropertyChanged("IterationsCount");
                    this.UpdateClusters(this.ClassesCount, this.IterationsCount, CancellationToken.None, null);
                }
            }
        }// IterationsCount
        public Dictionary<String, int> CategDict
        {
            get
            {
                if (m_categdict == null)
                {
                    m_categdict = new Dictionary<string, int>();
                }
                return m_categdict;
            }
            set
            {
                if (value != m_categdict)
                {
                    m_categdict = value;
                    NotifyPropertyChanged("CategDict");
                }
            }
        }// CategDict
        public IndivData CurrentIndiv
        {
            get
            {
                if (m_currentindiv == null)
                {
                    m_currentindiv = new IndivData();
                }
                return m_currentindiv;
            }
            set
            {
                if (value != m_currentindiv)
                {
                    m_currentindiv = value;
                    NotifyPropertyChanged("CurrentIndiv");
                }
            }
        }// CurrentIndiv
        public int ClassesCount
        {
            get
            {
                return m_nclasses;
            }
            set
            {
                if ((value != m_nclasses) && (value > 0))
                {
                    m_nclasses = value;
                    if ((m_nclasses % 2) == 0)
                    {
                        ++m_nclasses;
                    }
                    NotifyPropertyChanged("ClassesCount");
                }
            }
        }// ClassesCount
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
                    this.CurrentIndiv = null;
                    this.refreshOrdData();
                }
            }
        }// Individus
        public VariableDescs AllComputeVariables
        {
            get
            {
                return m_main.AllComputeVariables;
            }
            set
            {
                NotifyPropertyChanged("AllComputeVariables");
            }
        }// AllComputeVariables
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
                if (m_leftvariables == null)
                {
                    m_leftvariables = new VariableDescs();
                }
                return m_leftvariables;
            }
            set
            {
                if (value != m_leftvariables)
                {
                    m_leftvariables = value;
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
                return m_main.CurrentCategVariable;
            }
            set
            {
                m_main.CurrentCategVariable = value;
                this.updateCategValues();
                NotifyPropertyChanged("CurrentCategVariable");
            }
        }// CurrentCategVariable
        #endregion // Properties
        #region Methods
        public void RefreshVariables()
        {
            m_main.RefreshVariables();
        }// RefreshVariables
        public IndivData FindIndiv(int indivIndex)
        {
            IndivData pRet = null;
            if (indivIndex >= 0)
            {
                var q = from x in this.Individus where x.Individu.IndivIndex == indivIndex select x;
                if (q.Count() > 0)
                {
                    pRet = q.First();
                }
            }
            return pRet;
        }// FindIndiv
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
        public void RefreshArrangements(int nbIterations, CancellationToken cancellationToken)
        {
            this.UpdateClusters(this.ClassesCount, nbIterations, cancellationToken, null);
        }//RefreshArrangements 
        public Tuple<int[], DisplayItemsArray, PlotModel> CreateArrangeData(int nbIterations, CancellationToken cancellationToken)
        {
            var pDataModel = this;
            var inds = pDataModel.Individus;
            var vars = pDataModel.CurrentVariables.ToArray();
            int nv = vars.Length;
            int[] pRows = null;
            DisplayItemsArray oDisp = null;
            PlotModel model = null;
            try
            {
                var t1 = ArrangeSet.ArrangeIndex(pDataModel, nbIterations, cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    return new Tuple<int[], DisplayItemsArray, PlotModel>(pRows, oDisp, model);
                }
                pRows = t1.Item1;
                int[] pCols = t1.Item2;
                if ((pRows == null) || (pCols == null))
                {
                    return new Tuple<int[], DisplayItemsArray, PlotModel>(pRows, oDisp, model);
                }
                if (pCols.Length < nv)
                {
                    return new Tuple<int[], DisplayItemsArray, PlotModel>(pRows, oDisp, model);
                }
                double somme = 0.0;
                List<double> xdelta = new List<double>();
                List<double> xcum = new List<double>();
                oDisp = new DisplayItemsArray();
                DisplayItems header = new DisplayItems();
                header.Add(new DisplayItem("Pos.", true));
                header.Add(new DisplayItem("Index", true));
                header.Add(new DisplayItem("ID", true));
                header.Add(new DisplayItem("Nom", true));
                header.Add(new DisplayItem("Photo", true));
                for (int i = 0; i < nv; ++i)
                {
                    int ii = pCols[i];
                    if (ii >= nv)
                    {
                        return new Tuple<int[], DisplayItemsArray, PlotModel>(pRows, oDisp, model);
                    }
                    String name = (vars[ii]).Name;
                    header.Add(new DisplayItem(name, true));
                }// i
                oDisp.Add(header);
                IndivData firstIndiv = null;
                int nr = pRows.Length;
                for (int i = 0; i < nr; ++i)
                {
                    int index = pRows[i];
                    var q = from x in inds where x.IndivIndex == index select x;
                    if (q.Count() > 0)
                    {
                        var ind = q.First();
                        DisplayItems line = new DisplayItems();
                        line.Tag = ind;
                        line.Add(new DisplayItem(i + 1));
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
                        double[] dd = ind.DoubleData;
                        if ((dd != null) && (dd.Length >= nv))
                        {
                            for (int j = 0; j < nv; ++j)
                            {
                                int ii = pCols[j];
                                double xx = dd[ii];
                                line.Add(new DisplayItem(xx));
                            }// i
                        }// dd
                        oDisp.Add(line);
                        if (firstIndiv == null)
                        {
                            firstIndiv = ind;
                        }
                        else
                        {
                            double dx = ind.ComputeDistance(firstIndiv);
                            somme += dx;
                            xdelta.Add(dx);
                            xcum.Add(somme);
                            firstIndiv = ind;
                        }
                    }// ok
                }// i
                if (somme > 0.0)
                {
                    double[] ddx = xdelta.ToArray();
                    double[] ddc = xcum.ToArray();
                    int nx = ddx.Length;
                    for (int i = 0; i < nx; ++i)
                    {
                        ddx[i] = ddx[i] / somme;
                        ddc[i] = ddc[i] / somme;
                    }// i
                    double vmin = ddx.Min();
                    double vmax = ddx.Max();
                    if (vmin < vmax)
                    {
                        double delta = vmax - vmin;
                        for (int i = 0; i < nx; ++i)
                        {
                            ddx[i] = (ddx[i] - vmin) / delta;
                        }// i
                        model = new PlotModel("Arrangements");
                        model.Axes.Add(new LinearAxis() { Title = "Position", Minimum = 0.0, Maximum = (double)(nx + 1), Position = AxisPosition.Bottom, FontWeight = FontWeights.Bold });
                        model.Axes.Add(new LinearAxis() { Title = "Distances", Minimum = ddx.Min(), Maximum = ddx.Max(), Position = AxisPosition.Left, FontWeight = FontWeights.Bold });
                        model.Axes.Add(new LinearAxis() { Title = "Position", Minimum = 0.0, Maximum = (double)(nx + 1), Position = AxisPosition.Top, FontWeight = FontWeights.Bold });
                        model.Axes.Add(new LinearAxis() { Title = "Cummul", Minimum = ddc.Min(), Maximum = ddc.Max(), Position = AxisPosition.Right, FontWeight = FontWeights.Bold });
                        var sx = new LineSeries { Title = "Distances", Smooth = true, FontWeight = FontWeights.Bold };
                        var sy = new LineSeries { Title = "Cummul", Smooth = true, FontWeight = FontWeights.Bold };
                        for (int i = 0; i < nx; ++i)
                        {
                            sx.Points.Add(new DataPoint((double)(i + 1), ddx[i]));
                            sy.Points.Add(new DataPoint((double)(i + 1), ddc[i]));
                        }// i
                        model.Series.Add(sx);
                        model.Series.Add(sy);
                    }// graph
                }// someme
            }// try
            catch (Exception /* ex */)
            {
            }
            return new Tuple<int[], DisplayItemsArray, PlotModel>(pRows, oDisp, model);
        }//CreateArrangeDataAsync 
        #endregion // Mehods
        #region Helpers
        public async void RefreshVariablesValues()
        {
            bool bOld = this.IsBusy;
            this.IsBusy = true;
            this.Individus = null;
            var inds = m_main.AllIndividus;
            var vars = this.CurrentVariables;
            MatriceComputeMode mode = this.MatriceMode;
            int nClasses = this.ClassesCount;
            var xx = await Task.Run<IEnumerable<IndivData>>(() =>
            {
                return GetIndivsData(inds, vars, mode, nClasses);
            });
            if (xx != null)
            {
                this.Individus = new IndivDatas(xx);
            }
            this.UpdateClusters(this.ClassesCount, 1, CancellationToken.None, null);
        }
        private async void refreshOrdData()
        {
            this.OrdDisplayData = await getOrdDisplayDataAsync();
        }
        private Task<Dictionary<VariableDesc, ValueDescs>> performRefreshDataAsync()
        {
            var col = this.CurrentVariables;
            return Task.Run<Dictionary<VariableDesc, ValueDescs>>(() =>
            {
                return this.GetCommonValues(col);
            });
        }// perform RefreshDataAsync
        private void updateCategValues()
        {
            if (m_busy)
            {
                return;
            }
            m_busy = true;
            Dictionary<string, int> dict = null;
            var v = this.CurrentCategVariable;
            this.Individus.ClearCategIndexes();
            if ((v != null) && v.IsValid)
            {
                ValueDescs col = v.Values;
                dict = new Dictionary<string, int>();
                int icur = 0;
                foreach (var vx in col)
                {
                    String s = StatHelpers.ConvertValue(vx.DataStringValue);
                    if (!String.IsNullOrEmpty(s))
                    {
                        String ss = s.ToLower();
                        if (!dict.ContainsKey(ss))
                        {
                            dict[ss] = icur++;
                        }
                    }
                }// v
                foreach (var ind in this.Individus)
                {
                    var vals = ind.Individu.Values;
                    var q = from x in vals where x.VariableId == v.Id select x;
                    if (q.Count() > 0)
                    {
                        var vx = q.First();
                        String s = StatHelpers.ConvertValue(vx.DataStringValue);
                        if (!String.IsNullOrEmpty(s))
                        {
                            ind.CategString = s;
                            String ss = s.ToLower();
                            if (dict.ContainsKey(ss))
                            {
                                ind.CategIndex = dict[ss];
                            }
                        }
                    }// q
                }// ind
            }
            this.CategDict = dict;
            var p = this.Individus;
            this.Individus = null;
            this.Individus = p;
            m_busy = false;
        }// updateCategValues
        public async void UpdateClusters(int nbClasses, int nbIterations,
            CancellationToken cancellationToken, IProgress<Tuple<int, CategClusterSet>> progress)
        {
            bool bOld = this.IsBusy;
            this.IsBusy = true;
            var tt = await UpdateClustersAsync(nbClasses, nbIterations, cancellationToken, progress);
            this.CategClusterSet = tt.Item1[0];
            this.KMeansClusterSet = tt.Item1[1];
            this.HierarClusterSet = tt.Item1[2];
            this.RowIndexes = tt.Item2;
            this.RowArrangePlot = tt.Item3;
            updateCategClusterSet();
            updateKMeansClusterSet();
            updateHierarClusterSet();
            this.SortedIndivsData = tt.Item4;
            this.RefreshShowPlot();
            this.IsBusy = bOld;
        }// updatePlots
        public Task<Tuple<CategClusterSet[], int[], PlotModel, DisplayItemsArray>> UpdateClustersAsync(int nbClasses, int nbIterations,
            CancellationToken cancellationToken, IProgress<Tuple<int, CategClusterSet>> progress)
        {
            OrdModelView model = this;
            int nClusters = this.ClustersCount;
            var ltype = this.LinkType;
            CategClusterSet[] oSets = new CategClusterSet[3];
            int[] pRows = null;
            DisplayItemsArray oDisp = null;
            PlotModel pPlot = null;
            return Task.Run<Tuple<CategClusterSet[], int[], PlotModel, DisplayItemsArray>>(() =>
            {
                Parallel.Invoke(() =>
                {
                    oSets[0] = CategClusterSet.Clusterize(model, nClusters, nbIterations, cancellationToken, progress);
                }, () =>
                {
                    var p = CategClusterSet.KMeans(model, nClusters, nbClasses, cancellationToken, progress);
                    oSets[1] = p;
                }, () =>
                {
                    oSets[2] = TreeItem.Hierar(model, nClusters, ltype, cancellationToken);
                }, () =>
                {
                    var tt = CreateArrangeData(nbIterations, cancellationToken);
                    pRows = tt.Item1;
                    oDisp = tt.Item2;
                    pPlot = tt.Item3;
                });
                return new Tuple<CategClusterSet[], int[], PlotModel, DisplayItemsArray>(oSets, pRows, pPlot, oDisp);
            }, cancellationToken);
        }// UpdateClustersAsync
        private void updateCategClusterSet()
        {
            var oSet = this.CategClusterSet;
            if (oSet == null)
            {
                return;
            }
            var inds = this.Individus;
            foreach (var ind in inds)
            {
                ind.UtilityClusterIndex = -1;
                ind.UtilityClusterString = String.Empty;
            }
            foreach (var cluster in oSet.Clusters)
            {
                int nClusterIndex = cluster.Index;
                String name = cluster.Name;
                foreach (var z in cluster.Elements)
                {
                    var q = from x in inds where x.IndivIndex == z.IndivIndex select x;
                    if (q.Count() > 0)
                    {
                        var v = q.First();
                        v.UtilityClusterIndex = nClusterIndex;
                        v.UtilityClusterString = name;
                    }
                }// z
            }// cluster
        }
        private void updateKMeansClusterSet()
        {
            var oSet = this.KMeansClusterSet;
            if (oSet == null)
            {
                return;
            }
            var inds = this.Individus;
            foreach (var ind in inds)
            {
                ind.KMeansClusterIndex = -1;
                ind.KMeansClusterString = String.Empty;
            }
            foreach (var cluster in oSet.Clusters)
            {
                int nClusterIndex = cluster.Index;
                String name = cluster.Name;
                foreach (var z in cluster.Elements)
                {
                    var q = from x in inds where x.IndivIndex == z.IndivIndex select x;
                    if (q.Count() > 0)
                    {
                        var v = q.First();
                        v.KMeansClusterIndex = nClusterIndex;
                        v.KMeansClusterString = name;
                    }
                }// z
            }// cluster
        }
        private void updateHierarClusterSet()
        {
            var oSet = this.HierarClusterSet;
            if (oSet == null)
            {
                return;
            }
            var inds = this.Individus;
            foreach (var ind in inds)
            {
                ind.HierarClusterIndex = -1;
                ind.HierarClusterString = String.Empty;
            }
            foreach (var cluster in oSet.Clusters)
            {
                int nClusterIndex = cluster.Index;
                String name = cluster.Name;
                foreach (var z in cluster.Elements)
                {
                    var q = from x in inds where x.IndivIndex == z.IndivIndex select x;
                    if (q.Count() > 0)
                    {
                        var v = q.First();
                        v.HierarClusterIndex = nClusterIndex;
                        v.HierarClusterString = name;
                    }
                }// z
            }// cluster
        }
        private PlotModel createComninedShowPlot()
        {
            PlotModel model = null;
            try
            {
                VariableDesc oVarY = this.CurrentYVariable;
                VariableDesc oVarX = this.CurrentXVariable;
                if ((oVarX == null) || (oVarY == null))
                {
                    return model;
                }
                if ((!oVarX.IsNumVar) || (!oVarY.IsNumVar) && (oVarX.Id == oVarY.Id))
                {
                    return model;
                }
                List<Tuple<double, double, Cluster>> oList = new List<Tuple<double, double, Cluster>>();
                var o1 = StatModelViewBase.GetClustersCenters(oVarX, oVarY, this.CategClusterSet);
                if ((o1 != null) && (o1.Count > 0))
                {
                    oList.AddRange(o1);
                }
                var o2 = StatModelViewBase.GetClustersCenters(oVarX, oVarY, this.KMeansClusterSet);
                if ((o2 != null) && (o2.Count > 0))
                {
                    oList.AddRange(o2);
                }
                var o3 = StatModelViewBase.GetClustersCenters(oVarX, oVarY, this.HierarClusterSet);
                if ((o3 != null) && (o3.Count > 0))
                {
                    oList.AddRange(o3);
                }
                var allIndivs = this.Individus;
                Dictionary<int, String> categDict = new Dictionary<int, string>();
                String sval = DEFAULT_SERIE_NAME;
                foreach (var ind in allIndivs)
                {
                    int index = ind.IndivIndex;
                    categDict[index] = sval;
                }// ind;
                bool bPoints = this.HasPoints;
                bool bLabels = this.HasLabels;
                bool bImages = this.HasImages;
                bool bZeroCrossing = true;
                bool bLeastSquares = false;
                var imagesDict = this.ImagesDictionary;
                model = CreateCartesianPlot("Classes", allIndivs, oVarX, oVarY, imagesDict, categDict, bPoints, bLabels, bImages, bZeroCrossing,
                    bLeastSquares, oList);
                if (model == null)
                {
                    return model;
                }

            }// try
            catch (Exception /* ex */)
            {
                model = null;
            }
            return model;
        }// createShowPlot
        public Task<PlotModel> CreateCombinedShowPlotAsync()
        {
            return Task.Run<PlotModel>(() => {
                return createComninedShowPlot();
            });
        }//CreateCombinedShowPlotAsync 
        private PlotModel createShowPlot(String title, String prefix, CategClusterSet oSet)
        {
            PlotModel model = null;
            try
            {
                VariableDesc oVarY = this.CurrentYVariable;
                VariableDesc oVarX = this.CurrentXVariable;
                if ((oVarX == null) || (oVarY == null))
                {
                    return model;
                }
                if ((!oVarX.IsNumVar) || (!oVarY.IsNumVar) && (oVarX.Id == oVarY.Id))
                {
                    return model;
                }
                var allIndivs = this.Individus;
                Dictionary<int, String> categDict = new Dictionary<int, string>();
                List<Tuple<double, double, Cluster>> oList = StatModelViewBase.GetClustersCenters(oVarX, oVarY, oSet);
                if ((oSet != null) && oSet.IsValid)
                {
                    var clusters = oSet.Clusters;
                    foreach (var cluster in clusters)
                    {
                        var col = cluster.Elements;
                        String key = cluster.Name;
                        if (String.IsNullOrEmpty(key))
                        {
                            key = String.Format(prefix, cluster.Index + 1);
                        }
                        foreach (var ind in col)
                        {
                            int index = ind.IndivIndex;
                            if (index >= 0)
                            {
                                categDict[index] = key;
                            }
                        }// col
                    }// cluster
                }
                else
                {
                    String sval = DEFAULT_SERIE_NAME;
                    foreach (var ind in allIndivs)
                    {
                        int index = ind.IndivIndex;
                        categDict[index] = sval;
                    }// ind
                }
                bool bPoints = this.HasPoints;
                bool bLabels = this.HasLabels;
                bool bImages = this.HasImages;
                bool bZeroCrossing = false;
                bool bLeastSquares = false;
                var imagesDict = this.ImagesDictionary;
                model = CreateCartesianPlot(title, allIndivs, oVarX, oVarY, imagesDict, categDict, bPoints, bLabels, bImages, bZeroCrossing,
                    bLeastSquares, oList);
                if (model == null)
                {
                    return model;
                }

            }// try
            catch (Exception /* ex */)
            {
                model = null;
            }
            return model;
        }// createShowPlot
        private Tuple<PlotModel, PlotModel> createKMeansPlots()
        {
            PlotModel histog = null;
            PlotModel show = null;
            try
            {
                var oSet = this.KMeansClusterSet;
                if (oSet == null)
                {
                    return new Tuple<PlotModel, PlotModel>(null, null);
                }
                var clusters = oSet.Clusters;
                Collection<HistogItem> items = new Collection<HistogItem>();
                foreach (var oCluster in clusters)
                {
                    HistogItem item = new HistogItem();
                    String label = oCluster.Name;
                    item.Value1 = oCluster.Count;
                    items.Add(item);
                }// oCluster
                var model = new PlotModel("Effectif des clusters")
                {
                    LegendPlacement = LegendPlacement.Outside,
                    LegendPosition = LegendPosition.RightTop,
                    LegendOrientation = LegendOrientation.Vertical
                };
                // Add the axes, note that MinimumPadding and AbsoluteMinimum should be set on the value axis.
                model.Axes.Add(new CategoryAxis { ItemsSource = items, LabelField = "Label" });
                model.Axes.Add(new LinearAxis(AxisPosition.Left) { MinimumPadding = 0, AbsoluteMinimum = 0 });
                model.Series.Add(new ColumnSeries { Title = DEFAULT_SERIE_NAME, ItemsSource = items, ValueField = "Value1" });
                histog = model;
                //
                var pMan = this.DataService;
                VariableDesc oVarY = this.CurrentYVariable;
                VariableDesc oVarX = this.CurrentXVariable;
                if ((pMan == null) || (oVarX == null) || (oVarY == null))
                {
                    return new Tuple<PlotModel, PlotModel>(histog, show);
                }
                if (oVarX == oVarY)
                {
                    return new Tuple<PlotModel, PlotModel>(histog, show);
                }
                if ((!oVarX.IsNumVar) || (!oVarX.IsValid) || (!oVarY.IsNumVar) || (!oVarY.IsValid))
                {
                    return new Tuple<PlotModel, PlotModel>(histog, show);
                }
                var dataX = ComputeVariableCategValues(this.CurrentCategVariable, oVarX);
                var dataY = ComputeVariableCategValues(this.CurrentCategVariable, oVarY);
                var keys = dataX.Keys;
                String sTitle = String.Format("{0} / {1}", oVarY.Name, oVarX.Name);
                model = new PlotModel(sTitle) { LegendPlacement = LegendPlacement.Outside };
                foreach (var cluster in clusters)
                {
                    var s1 = new ScatterSeries(cluster.Name);
                    foreach (var ind in cluster.Elements)
                    {
                        var col = ind.Values;
                        var qx = from x in col where x.VariableId == oVarX.Id select x;
                        var qy = from x in col where x.VariableId == oVarY.Id select x;
                        if ((qx.Count() > 0) && (qy.Count() > 0))
                        {
                            var vx = qx.First();
                            var vy = qy.First();
                            double xx = vx.DoubleValue;
                            double yy = vy.DoubleValue;
                            s1.Points.Add(new ScatterPoint(xx, yy) { Tag = ind });
                        }
                    }// ind
                    model.Series.Add(s1);
                }// cluster
                show = model;
            }
            catch (Exception /* ex */)
            {
            }
            return new Tuple<PlotModel, PlotModel>(histog, show);
        }// createKMeansPLots
        private PlotModel createClusterShowPlot()
        {
            String title = "Category Utility";
            String prefix = "UC{0}";
            CategClusterSet oSet = this.CategClusterSet;
            return this.createShowPlot(title, prefix, oSet);
        }// createCategHistogPlot
        private PlotModel createKMeansShowPlot()
        {
            String title = "Nuées dynamiques";
            String prefix = "KM{0}";
            CategClusterSet oSet = this.KMeansClusterSet;
            return this.createShowPlot(title, prefix, oSet);
        }// createCategHistogPlot
        private PlotModel createHierarShowPlot()
        {
            String title = "Classificatiion hiérarchique";
            String prefix = "HR{0}";
            CategClusterSet oSet = this.HierarClusterSet;
            return this.createShowPlot(title, prefix, oSet);
        }// createCategHistogPlot
        private async void RefreshShowPlot()
        {
            var pp = await RefreshShowPlotsAsync();
            if (pp != null)
            {
                this.ClusterShowModel = pp[0];
                this.KMeansShowModel = pp[1];
                this.HierarShowModel = pp[2];
                this.CombinedShowModel = pp[3];
            }// pp
        }
        public Task<PlotModel[]> RefreshShowPlotsAsync()
        {
            return Task.Run<PlotModel[]>(() =>
            {
                PlotModel[] pRet = new PlotModel[4];
                Parallel.Invoke(() =>
                {
                    pRet[0] = createClusterShowPlot();
                }, () =>
                {
                    pRet[1] = createKMeansShowPlot();
                }, () =>
                {
                    pRet[2] = createHierarShowPlot();
                }, () =>
                {
                    pRet[3] = createComninedShowPlot();
                });
                return pRet;
            });
        }// RefreshShowPlotsAsync
        private DisplayItemsArray createOrdDisplayData()
        {
            List<DisplayItems> oRet = null;
            try
            {
                var inds = this.Individus.ToArray();
                int nr = inds.Length;
                if (nr < 1)
                {
                    return null;
                }
                var pMan = this.DataService;
                if (pMan == null)
                {
                    return null;
                }
                var allVars = m_main.Variables;
                VariableDesc idsvars = null;
                VariableDesc namesvars = null;
                VariableDesc photosvars = null;
                bool bString = true;
                foreach (var v in allVars)
                {
                    if (v.IsIdVar)
                    {
                        idsvars = v;
                    }
                    if (v.IsNameVar)
                    {
                        namesvars = v;
                    }
                    if (v.IsImageVar)
                    {
                        photosvars = v;
                    }
                }// v
                if (photosvars != null)
                {
                    String stype = photosvars.DataType.Trim().ToLower();
                    if (!stype.Contains("string"))
                    {
                        bString = false;
                    }
                }// photovars
                var vars = this.CurrentVariables.ToArray();
                int nv = vars.Length;
                oRet = new List<DisplayItems>();
                DisplayItems header = new DisplayItems();
                header.Add(new DisplayItem("Num", true));
                header.Add(new DisplayItem("Index", true));
                header.Add(new DisplayItem("ID", true));
                header.Add(new DisplayItem("Nom", true));
                header.Add(new DisplayItem("Photo", true));
                //
                header.Add(new DisplayItem("CU", true));
                header.Add(new DisplayItem("KM", true));
                header.Add(new DisplayItem("HR", true));
                header.Add(new DisplayItem("AR", true));
                for (int i = 0; i < nv; ++i)
                {
                    header.Add(new DisplayItem(vars[i].Name, true));
                }// i
                oRet.Add(header);
                for (int irow = 0; irow < nr; ++irow)
                {
                    var ind = inds[irow];
                    DisplayItems line = new DisplayItems();
                    line.Tag = ind;
                    line.Add(new DisplayItem(irow + 1));
                    int index = ind.IndivIndex;
                    line.Add(new DisplayItem(index));
                    String sid = String.Empty;
                    if (idsvars != null)
                    {
                        var q = from x in ind.Values where x.VariableId == idsvars.Id select x;
                        if (q.Count() > 0)
                        {
                            sid = StatHelpers.ConvertValue(q.First().DataStringValue);
                        }
                    }// idsvars
                    line.Add(new DisplayItem(sid));
                    String sname = String.Empty;
                    if (namesvars != null)
                    {
                        var q = from x in ind.Values where x.VariableId == namesvars.Id select x;
                        if (q.Count() > 0)
                        {
                            sname = StatHelpers.ConvertValue(q.First().DataStringValue);
                        }
                    }// idsvars
                    line.Add(new DisplayItem(sname));
                    DisplayItem photo = new DisplayItem();
                    if (photosvars != null)
                    {
                        var q = from x in ind.Values where x.VariableId == photosvars.Id select x;
                        if (q.Count() > 0)
                        {
                            String sval = StatHelpers.ConvertValue(q.First().DataStringValue);
                            if (!String.IsNullOrEmpty(sval))
                            {
                                int nid = 0;
                                String name = null;
                                if (!bString)
                                {
                                    double d = 0.0;
                                    double.TryParse(sval, out d);
                                    nid = (int)d;
                                }
                                else
                                {
                                    name = sval;
                                }
                                PhotoDesc p = new PhotoDesc();
                                p.Id = nid;
                                p.Name = name;
                                var xx = pMan.FindPhoto(p);
                                if ((xx != null) && (xx.Item1 != null) && (xx.Item2 == null))
                                {
                                    photo = new DisplayItem(xx.Item1.DataBytes);
                                }
                            }// sval
                        }
                    }// photos
                    line.Add(photo);
                    //
                    line.Add(new DisplayItem(ind.UtilityClusterIndex));
                    line.Add(new DisplayItem(ind.KMeansClusterIndex));
                    line.Add(new DisplayItem(ind.HierarClusterIndex));
                    line.Add(new DisplayItem(ind.Position));
                    //
                    for (int i = 0; i < nv; ++i)
                    {
                        String sval = String.Empty;
                        var vx = vars[i];
                        var q = from x in ind.Values where x.VariableId == vx.Id select x;
                        if (q.Count() > 0)
                        {
                            sval = StatHelpers.ConvertValue(q.First().DataStringValue);
                        }
                        line.Add(new DisplayItem(sval));
                    }// i
                    //
                    oRet.Add(line);
                }// ind
            }
            catch (Exception /*ex */)
            {
                oRet = null;
            }
            return (oRet != null) ? new DisplayItemsArray(oRet) : null;
        }// createOrdDisplayData
        private DisplayItemsArray createSortedDisplayData()
        {
            List<DisplayItems> oRet = null;
            try
            {
                var inds = this.SortedIndivs.ToArray();
                int nr = inds.Length;
                if (nr < 1)
                {
                    return null;
                }
                oRet = new List<DisplayItems>();
                DisplayItems header = new DisplayItems();
                header.Add(new DisplayItem("Index", true));
                header.Add(new DisplayItem("ID", true));
                header.Add(new DisplayItem("Nom", true));
                header.Add(new DisplayItem("Photo", true));
                oRet.Add(header);
                for (int irow = 0; irow < nr; ++irow)
                {
                    var ind = inds[irow];
                    DisplayItems line = new DisplayItems();
                    line.Tag = ind;
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
                    oRet.Add(line);
                }// ind
            }
            catch (Exception /*ex */)
            {
                oRet = null;
            }
            return (oRet != null) ? new DisplayItemsArray(oRet) : null;
        }// createOrdDisplayData
        private DisplayItemsArray createSortedDisplayData(int[] pRows, int[] pCols)
        {
            List<DisplayItems> oRet = null;
            try
            {
                var inds = this.SortedIndivs.ToArray();
                int nr = inds.Length;
                if (nr < 1)
                {
                    return null;
                }
                oRet = new List<DisplayItems>();
                DisplayItems header = new DisplayItems();
                header.Add(new DisplayItem("Index", true));
                header.Add(new DisplayItem("ID", true));
                header.Add(new DisplayItem("Nom", true));
                header.Add(new DisplayItem("Photo", true));
                oRet.Add(header);
                for (int irow = 0; irow < nr; ++irow)
                {
                    var ind = inds[irow];
                    DisplayItems line = new DisplayItems();
                    line.Tag = ind;
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
                    oRet.Add(line);
                }// ind
            }
            catch (Exception /*ex */)
            {
                oRet = null;
            }
            return (oRet != null) ? new DisplayItemsArray(oRet) : null;
        }// createOrdDisplayData
        private Task<DisplayItemsArray> getOrdDisplayDataAsync()
        {
            return Task.Run<DisplayItemsArray>(() =>
            {
                return createOrdDisplayData();
            });
        }//getOrdDisplayDataAsync 
        private List<DisplayItems> getClusterSetData(CategClusterSet oSet)
        {
            List<DisplayItems> oRet = new List<DisplayItems>();
            if (oSet != null)
            {
                double somme = oSet.Variance;
                foreach (var c in oSet.Clusters)
                {
                    DisplayItems line = new DisplayItems();
                    line.Add(new DisplayItem(c.Index));
                    line.Add(new DisplayItem(c.Name));
                    line.Add(new DisplayItem(c.Count));
                    double v = c.Variance;
                    line.Add(new DisplayItem(v));
                    if (somme > 0.0)
                    {
                        line.Add(new DisplayItem(v / somme));
                    }
                    else
                    {
                        line.Add(new DisplayItem());
                    }
                    double[] cx = c.Center;
                    int nc = cx.Length;
                    for (int i = 0; i < nc; ++i)
                    {
                        line.Add(new DisplayItem(cx[i]));
                    }// i
                    oRet.Add(line);
                }// v
            }// oSet
            return oRet;
        }// getClusterSetData
        public DisplayItemsArray GetClustersData()
        {
            List<DisplayItems> oRet = new List<DisplayItems>();
            DisplayItems hh = new DisplayItems();
            hh.Add(new DisplayItem("Index", true));
            hh.Add(new DisplayItem("Nom", true));
            hh.Add(new DisplayItem("Taille", true));
            hh.Add(new DisplayItem("Var", true));
            hh.Add(new DisplayItem("Prop", true));
            var col = this.CurrentVariables.ToArray();
            int nc = col.Length;
            for (int i = 0; i < nc; ++i)
            {
                String name = (col[i]).Name;
                hh.Add(new DisplayItem(name, true));
            }//i
            oRet.Add(hh);
            var oSet1 = this.CategClusterSet;
            if (oSet1 != null)
            {
                var x = getClusterSetData(oSet1);
                if ((x != null) && (x.Count > 0))
                {
                    oRet.AddRange(x);
                }
            }
            var oSet2 = this.KMeansClusterSet;
            if (oSet2 != null)
            {
                var x = getClusterSetData(oSet2);
                if ((x != null) && (x.Count > 0))
                {
                    oRet.AddRange(x);
                }
            }
            var oSet3 = this.HierarClusterSet;
            if (oSet3 != null)
            {
                var x = getClusterSetData(oSet3);
                if ((x != null) && (x.Count > 0))
                {
                    oRet.AddRange(x);
                }
            }
            return new DisplayItemsArray(oRet);
        }// GetClustersData
        public async void RefreshClusterData()
        {
            var xx = await Task.Run<DisplayItemsArray>(() =>
            {
                return GetClustersData();
            });
            this.ClustersData = xx;
        }// RefreshClusterData
        private void onMatriceModeChanged()
        {
            RefreshVariablesValues();
        }// onMatriceModeChanged
        #endregion Helpers
    }
}
