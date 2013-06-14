using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows;
using System.Drawing;
using System.IO;
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
using StatApp.Controles;
using StatData;
/////////////////////////////////
namespace StatApp.ModelView
{
    public class HistogItem
    {
        public HistogItem() { }
        public String Label { get; set; }
        public double Value1 { get; set; }
        public double Value2 { get; set; }
    }//class HistogItem
    public class StatModelViewBase : Observable
    {
        #region Static Variables
        public static readonly String DEFAULT_SERIE_NAME = "Individus";
        private static byte[] DEFAULT_IMAGE_BYTES = null;
        protected static readonly double IMAGE_SCALE_FACTOR = 3.0;
        public static readonly OxyColor[] TAB_COLORS = new OxyColor[]{
                OxyColors.Black,OxyColors.Red,OxyColors.Green,OxyColors.Blue,
                OxyColors.Cyan, OxyColors.Magenta,OxyColors.Yellow,OxyColors.Violet,
                OxyColors.Chocolate,OxyColors.DarkGray, OxyColors.DarkKhaki
                };
        #endregion //Static Variables
        #region Instance Variables
        private IServiceLocator m_servicelocator;
        private IPageLocator m_pagelocator;
        private NavigationService m_navig;
        #endregion // Instance Variables
        #region constructors
        public StatModelViewBase()
        {
        }
        public StatModelViewBase(IServiceLocator servicelocator, IPageLocator pageLocator, NavigationService pNav)
        {
            m_servicelocator = servicelocator;
            m_pagelocator = pageLocator;
            m_navig = pNav;
        }
        #endregion // Constructore
        #region Properties
        public IPageLocator PageLocator
        {
            get
            {
                return m_pagelocator;
            }
            set
            {
                if (value != m_pagelocator)
                {
                    m_pagelocator = value;
                    NotifyPropertyChanged("PageLocator");
                }
            }
        }// PageLocator
        public IServiceLocator ServiceLocator
        {
            get
            {
                return m_servicelocator;
            }
            set
            {
                if (value != m_servicelocator)
                {
                    m_servicelocator = value;
                    NotifyPropertyChanged("ServiceLocator");
                }
            }
        }// ServiceLocator
        public IStoreDataManager DataService
        {
            get
            {
                var p = this.ServiceLocator;
                if (p != null)
                {
                    return p.GetDataService();
                }
                return null;
            }
        }// DataService
        public NavigationService NavigationService
        {
            get
            {
                return m_navig;
            }
            set
            {
                if (value != m_navig)
                {
                    m_navig = value;
                    NotifyPropertyChanged("NavigationService");
                }
            }
        }// NavigationService
        #endregion // Properties
        #region async data tasks
        public Task<StatDataSets> GetAllStatDataSetsAsync()
        {
            return Task.Run<StatDataSets>(() =>
            {
                StatDataSets oRet = null;
                var pMan = this.DataService;
                if (pMan != null)
                {
                    var xx = pMan.GetAllStatDataSets();
                    if ((xx != null) && (xx.Item1 != null) && (xx.Item2 == null))
                    {
                        oRet = new StatDataSets(xx.Item1);
                    }
                }// pMan
                return oRet;
            });
        }// GetAllStatDataSetsAsync
        #endregion // async data tasks
        #region Methods
        public static OxyColor GetColor(int index)
        {
            if (index < 0)
            {
                return TAB_COLORS[(-index) % TAB_COLORS.Length];
            }
            else
            {
                return TAB_COLORS[index % TAB_COLORS.Length];
            }
        }// getColor
        public static List<Tuple<double, double, Cluster>> GetClustersCenters(VariableDesc oVarX, VariableDesc oVarY, CategClusterSet oSet)
        {
            List<Tuple<double,double, Cluster>> oList = new List<Tuple<double, double,Cluster>>();
            if ((oVarX == null) || (oVarY == null) || (oSet == null))
            {
                return oList;
            }
            var col = new VariableDesc[] { oVarX, oVarY};
            var tt = GetClustersCenters(col, oSet);
            if (tt != null)
            {
                foreach (var t in tt)
                {
                    double[] dd = t.Item1;
                    if ((dd != null) && (dd.Length > 1))
                    {
                        double x = dd[0];
                        double y = dd[1];
                        Cluster c = t.Item2;
                        oList.Add(new Tuple<double, double, Cluster>(x, y, c));
                    }
                }// t
            }// tt
            return oList;
        }//GetClustersCenters 
        public static List<Tuple<double[], Cluster>> GetClustersCenters(IEnumerable<VariableDesc> oVars, CategClusterSet oSet)
        {
            List<Tuple<double[], Cluster>> oList = new List<Tuple<double[], Cluster>>();
            if ((oVars == null) || (oSet == null))
            {
                return oList;
            }
            if (!oSet.IsValid)
            {
                return oList;
            }
            var oAr = oVars.ToArray();
            int n = oAr.Length;
            if (n < 1)
            {
                return oList;
            }
            var clusters = oSet.Clusters;
            foreach (var cluster in clusters)
            {
                double[] dd = new double[n];
                int[] count = new int[n];
                for (int i = 0; i < n; ++i)
                {
                    dd[i] = 0.0;
                    count[i] = 0;
                }// i
                var elements = cluster.Elements;
                foreach (var ind in elements)
                {
                    var vals = ind.Values;
                    for (int i = 0; i < n; ++i)
                    {
                        int nx = (oAr[i]).Id;
                        var q = from x in vals where x.VariableId == nx select x;
                        if (q.Count() > 0)
                        {
                            double xx = q.First().DoubleValue;
                            count[i] = count[i] + 1;
                            dd[i] = dd[i] + xx;
                        }
                    }// i
                }// ind
                for (int i = 0; i < n; ++i)
                {
                    int nc = count[i];
                    if (nc > 0)
                    {
                        dd[i] = dd[i] / nc;
                    }
                }// i
               oList.Add(new Tuple<double[],Cluster>(dd,cluster));
            }// cluster
            return oList;
        }// GetClustersList
        public PlotModel CreateCartesianPlot(String sTitle, IEnumerable<IndivData> inds,
            VariableDesc oVarX, VariableDesc oVarY,
            Dictionary<int, OxyImage> imagesDict = null,
            Dictionary<int, String> categDict = null,
            bool bPoints = true,
            bool bLabels = false,
            bool bImages = false,
            bool bZeroCrossing = false,
            bool bLeastSquares = false,
            List<Tuple<double,double,Cluster>> oCenters = null)
        {
            if ((inds == null) || (oVarX == null) || (oVarY == null))
            {
                return null;
            }
            PlotModel model = null;
            try
            {
                ValueDescs xvars = oVarX.Values;
                ValueDescs yvars = oVarY.Values;
                if ((xvars == null) || (yvars == null))
                {
                    return null;
                }
                Dictionary<int, Tuple<double, double, IndivData>> vals = new Dictionary<int, Tuple<double, double, IndivData>>();
                List<double> xlist = new List<double>();
                List<double> ylist = new List<double>();
                List<DataPoint> pointsList = null;
                if (bLeastSquares)
                {
                    pointsList = new List<DataPoint>();
                }
                foreach (var ind in inds)
                {
                    if ((ind != null) && ind.IsValid)
                    {
                        int index = ind.IndivIndex;
                        var qx = from x in xvars where x.Index == index select x;
                        var qy = from y in yvars where y.Index == index select y;
                        if ((qx.Count() > 0) && (qy.Count() > 0))
                        {
                            var vx = qx.First();
                            var vy = qy.First();
                            double xx = vx.DoubleValue;
                            double yy = vy.DoubleValue;
                            xlist.Add(xx);
                            ylist.Add(yy);
                            vals[index] = new Tuple<double, double, IndivData>(xx, yy, ind);
                            if (bLeastSquares)
                            {
                                pointsList.Add(new DataPoint(xx, yy));
                            }
                        }
                    }// ind
                }// ind
                int nr = xlist.Count;
                if (nr < 1)
                {
                    return null;
                }
                double xmin = xlist.Min();
                double xmax = xlist.Max();
                double ymin = ylist.Min();
                double ymax = ylist.Max();
                if ((xmin >= xmax) || (ymin >= ymax))
                {
                    return null;
                }
                double xwidth = IMAGE_SCALE_FACTOR * (xmax - xmin) / nr;
                model = new PlotModel(sTitle);
                model.Subtitle = String.Format("{0} / {1}", oVarY.Name, oVarX.Name);
                model.LegendPlacement = LegendPlacement.Outside;
                if (bZeroCrossing)
                {
                    model.Axes.Add(new LinearAxis(AxisPosition.Bottom, xmin, xmax, oVarX.Name) { PositionAtZeroCrossing = true });
                    model.Axes.Add(new LinearAxis(AxisPosition.Left, ymin, ymax, oVarY.Name) { PositionAtZeroCrossing = true });
                }
                else
                {
                    model.Axes.Add(new LinearAxis(AxisPosition.Bottom, xmin, xmax, oVarX.Name));
                    model.Axes.Add(new LinearAxis(AxisPosition.Left, ymin, ymax, oVarY.Name));
                }
                Dictionary<String, ScatterSeries> oSeries = new Dictionary<string, ScatterSeries>();
                var indexes = vals.Keys;
                int nColor = 0;
                foreach (var index in indexes)
                {
                    var t = vals[index];
                    double x = t.Item1;
                    double y = t.Item2;
                    IndivData ind = t.Item3;
                    if (bPoints)
                    {
                        if ((categDict != null) && categDict.ContainsKey(index))
                        {
                            String scateg = categDict[index];
                            if (!String.IsNullOrEmpty(scateg))
                            {
                                if (!oSeries.ContainsKey(scateg))
                                {
                                    OxyColor color = StatModelViewBase.GetColor(nColor++);
                                    oSeries[scateg] = new ScatterSeries(scateg) { MarkerType = MarkerType.Circle,MarkerFill=color };
                                }
                                oSeries[scateg].Points.Add(new ScatterPoint(x, y) { Tag = ind });
                            }
                            else
                            {
                                if (!oSeries.ContainsKey(DEFAULT_SERIE_NAME))
                                {
                                    OxyColor color = StatModelViewBase.GetColor(nColor++);
                                    oSeries[DEFAULT_SERIE_NAME] = new ScatterSeries(DEFAULT_SERIE_NAME) { MarkerType = MarkerType.Circle,MarkerFill=color };
                                }
                                oSeries[DEFAULT_SERIE_NAME].Points.Add(new ScatterPoint(x, y) { Tag = ind });
                            }
                        }
                        else
                        {
                            if (!oSeries.ContainsKey(DEFAULT_SERIE_NAME))
                            {
                                OxyColor color = StatModelViewBase.GetColor(nColor++);
                                oSeries[DEFAULT_SERIE_NAME] = new ScatterSeries(DEFAULT_SERIE_NAME) { MarkerType = MarkerType.Circle,MarkerFill=color };
                            }
                            oSeries[DEFAULT_SERIE_NAME].Points.Add(new ScatterPoint(x, y) { Tag = ind });
                        }
                    }// points
                    if (bLabels)
                    {
                        String name = ind.IdString;
                        if (String.IsNullOrEmpty(name))
                        {
                            name = ind.Name;
                        }
                        if (!String.IsNullOrEmpty(name))
                        {
                            model.Annotations.Add(new TextAnnotation
                            {
                                Position = new DataPoint(x, y),
                                Text = name,
                                FontWeight = OxyPlot.FontWeights.Bold,
                                HorizontalAlignment = OxyPlot.HorizontalAlignment.Center,
                                VerticalAlignment = OxyPlot.VerticalAlignment.Middle,
                                Tag = ind
                            });
                        }// write
                    }// labels
                    if (bImages && (imagesDict != null) && imagesDict.ContainsKey(index))
                    {
                        OxyImage image = imagesDict[index];
                        var data = ind.PhotoData;
                        if (image != null)
                        {
                            model.Annotations.Add(new ImageAnnotation
                            {
                                ImageSource = image,
                                X = new PlotLength(x, PlotLengthUnit.Data),
                                Y = new PlotLength(y, PlotLengthUnit.Data),
                                HorizontalAlignment = OxyPlot.HorizontalAlignment.Center,
                                VerticalAlignment = OxyPlot.VerticalAlignment.Middle,
                                Width = new PlotLength(xwidth, PlotLengthUnit.Data)
                            });
                        }// image
                    }// images
                }// index
                if (bPoints)
                {
                    foreach (var s1 in oSeries.Values)
                    {
                        model.Series.Add(s1);
                    }// s1
                }// bPoints
                if ((oCenters != null) && (oCenters.Count > 0))
                {
                    foreach (var t in oCenters)
                    {
                        Cluster c = t.Item3;
                        if (c != null)
                        {
                            String name = c.Name;
                            if (String.IsNullOrEmpty(name))
                            {
                                name = String.Format("CL{0}", c.Index + 1);
                            }
                            double x = t.Item1;
                            double y = t.Item2;
                            model.Annotations.Add(new TextAnnotation
                            {
                                Position = new DataPoint(x, y),
                                Text = name,
                                FontWeight = OxyPlot.FontWeights.Bold,
                                HorizontalAlignment = OxyPlot.HorizontalAlignment.Center,
                                VerticalAlignment = OxyPlot.VerticalAlignment.Middle
                            });
                        }// c
                    }// t
                }
                if (bLeastSquares)
                {
                    double a, b;
                    LeastSquaresFit(pointsList, out a, out b);
                    model.Annotations.Add(new LineAnnotation { Slope = a, Intercept = b, Text = "Moindres Carrés" });
                }// leastsquares
            }// try
            catch (Exception /* ex */)
            {
                model = null;
            }
            return model;
        }// CreateCartesianPlot
        public static byte[] GetDefaultImagesBytes()
        {
            byte[] pRet = DEFAULT_IMAGE_BYTES;
            if ((pRet != null) && (pRet.Length > 1))
            {
                return pRet;
            }
            try
            {
                Image img = StatApp.Properties.Resources.Individu;
                MemoryStream ms = new MemoryStream();
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                pRet = ms.ToArray();
            }
            catch (Exception /*ex */)
            {
            }
            return pRet;
        }//  GetDefaultImagesBytes
        public static String GetImportFilename(String defaultExt, String filter)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = defaultExt;
            dlg.Filter = filter;
            dlg.Multiselect = false;
            Nullable<bool> result = dlg.ShowDialog();
            if ((result != null) && result.HasValue && (result.Value == true))
            {
                return dlg.FileName;
            }
            return null;
        }// ImportPhoto
        public static CorrelData ComputeCorrelation(IEnumerable<double> d1, IEnumerable<double> d2)
        {
            CorrelData oRet = null;
            if ((d1 == null) || (d2 == null))
            {
                return null;
            }
            double[] dd1 = d1.ToArray();
            double[] dd2 = d2.ToArray();
            int n = (dd1.Length < dd2.Length) ? dd1.Length : dd2.Length;
            if (n < 3)
            {
                return null;
            }
            oRet = new CorrelData();
            oRet.Count = n;
            double corr = myconvert10000(Correlation.Pearson(dd1.Take(n), dd2.Take(n)));
            if (corr < -1.0)
            {
                corr = -1.0;
            }
            if (corr > 1.0)
            {
                corr = 1.0;
            }
            oRet.Value = corr;
            double xn = (double)n;
            double rr = (corr < 0.0) ? -corr : corr;
            double crit = rr * Math.Sqrt(xn - 2.0) / Math.Sqrt(1.0 - rr * rr);
            StudentT st = new StudentT(0.0, 1.0, xn - 2);
            double pb = st.CumulativeDistribution(crit);
            oRet.Probability = myconvert10000(1.0 - pb);
            double s1 = 0.5 * Math.Log((1.0 + corr) / (1.0 - corr));
            double s2 = 1.96 / Math.Sqrt(xn - 3.0);
            double s3 = corr / Math.Sqrt(xn - 1.0);
            double z1 = s1 - s2 - s3;
            double z2 = s1 + s2 - s3;
            oRet.Minimum = myconvert10000(Math.Tanh(z1));
            oRet.Maximum = myconvert10000(Math.Tanh(z2));
            return oRet;
        }// ComputeCorrelation
        public CorrelData ComputeCorrelation(VariableDesc oVar1, VariableDesc oVar2)
        {
            CorrelData oRet = null;
            if ((oVar1 == null) || (oVar2 == null))
            {
                return null;
            }
            if (!oVar1.IsNumVar)
            {
                return null;
            }
            if (!oVar2.IsNumVar)
            {
                return null;
            }
            try
            {
                var dict = GetCommonValues(new List<VariableDesc>() { oVar1, oVar2 });
                if (dict == null)
                {
                    return null;
                }
                var vars = dict.Keys.ToArray();
                if (vars.Length < 2)
                {
                    return null;
                }
                List<double> oList1 = new List<double>();
                List<double> oList2 = new List<double>();
                var v1 = vars[0];
                var vals1 = dict[v1];
                var v2 = vars[1];
                var vals2 = dict[v2];
                foreach (var vx in vals1)
                {
                    int index = vx.Index;
                    var q = from x in vals2 where x.Index == index select x;
                    if (q.Count() > 0)
                    {
                        var vy = q.First();
                        oList1.Add(vx.DoubleValue);
                        oList2.Add(vy.DoubleValue);
                    }
                }// vx
                if (oList1.Count < 4)
                {
                    return null;
                }
                oRet = ComputeCorrelation(oList1, oList2);
                if (oRet != null)
                {
                    oRet.FirstName = v1.Name;
                    oRet.SecondName = v2.Name;
                }
            }
            catch (Exception /*ex */)
            {
                oRet = null;
            }
            return oRet;
        }// Compute Correlation
        public IEnumerable<CorrelData> ComputeCorrelations(IEnumerable<VariableDesc> oVars)
        {
            List<CorrelData> oRet = new List<CorrelData>();
            var col = oVars.ToArray();
            int n = col.Length;
            for (int i = 0; i < n; ++i)
            {
                VariableDesc v1 = col[i];
                if ((v1 != null) && v1.IsNumVar && (!v1.IsInfoVar) && (!v1.IsImageVar))
                {
                    for (int j = 0; j < i; ++j)
                    {
                        VariableDesc v2 = col[j];
                        if ((v2 != null) && v2.IsNumVar && (!v2.IsInfoVar) && (!v2.IsImageVar))
                        {
                            CorrelData c = ComputeCorrelation(v1, v2);
                            if (c != null)
                            {
                                oRet.Add(c);
                            }
                        }// v2
                    }// j
                }// v1
            }// i
            return oRet;
        }// ComputeCorretaions
        public DisplayItemsArray ComputeCorrelationsDisplay(IEnumerable<VariableDesc> oVars)
        {
            var col = ComputeCorrelations(oVars);
            if (col == null)
            {
                return null;
            }
            DisplayItemsArray oRet = new DisplayItemsArray();
            DisplayItems hh = new DisplayItems();
            hh.Add(new DisplayItem("V1", true));
            hh.Add(new DisplayItem("V2", true));
            hh.Add(new DisplayItem("N", true));
            hh.Add(new DisplayItem("Prob.", true));
            hh.Add(new DisplayItem("Corr.", true));
            hh.Add(new DisplayItem("Min", true));
            hh.Add(new DisplayItem("Max", true));
            oRet.Add(hh);
            foreach (var c in col)
            {
                DisplayItems line = new DisplayItems();
                line.Add(new DisplayItem(c.FirstName));
                line.Add(new DisplayItem(c.SecondName));
                line.Add(new DisplayItem(c.Count));
                line.Add(new DisplayItem(c.Probability));
                line.Add(new DisplayItem(c.Value));
                line.Add(new DisplayItem(c.Minimum));
                line.Add(new DisplayItem(c.Maximum));
                oRet.Add(line);
            }// c
            return oRet;
        }// ComputeCorrelationsDisplay
        public Task<DisplayItemsArray> ComputeCorrelationsDisplayAsync(IEnumerable<VariableDesc> oVars)
        {
            return Task.Run<DisplayItemsArray>(() =>
            {
                return ComputeCorrelationsDisplay(oVars);
            });
        }// ComputeCorrelationsDisplayAsync
        public static void LeastSquaresFit(IEnumerable<DataPoint> points, out double a, out double b)
        {
            double Sx = 0;
            double Sy = 0;
            double Sxy = 0;
            double Sxx = 0;
            int m = 0;
            foreach (var p in points)
            {
                Sx += p.X;
                Sy += p.Y;
                Sxy += p.X * p.Y;
                Sxx += p.X * p.X;
                m++;
            }
            double d = Sx * Sx - m * Sxx;
            a = 1 / d * (Sx * Sy - m * Sxy);
            b = 1 / d * (Sx * Sxy - Sxx * Sy);
        }
        public static double GetMedian(IEnumerable<double> values, bool bSort)
        {
            var sortedInterval = new List<double>(values);
            if (bSort)
            {
                sortedInterval.Sort();
            }
            double dRet = SortedArrayStatistics.Quantile(sortedInterval.ToArray(), 0.5);
            return dRet;
        } // GetMedian
        public Dictionary<String, ValueDescs> ComputeVariableCategValues(VariableDesc oCateg, VariableDesc oVar)
        {
            Dictionary<String, ValueDescs> oRet = new Dictionary<string, ValueDescs>();
            ValueDescs categVals = oCateg.Values;
            ValueDescs dataVals = oVar.Values;
            if (categVals.Count < 1)
            {
                oRet[DEFAULT_SERIE_NAME] = dataVals;
            }
            else
            {
                foreach (var vcateg in categVals)
                {
                    String skey = StatHelpers.ConvertValue(vcateg.DataStringValue);
                    if (!String.IsNullOrEmpty(skey))
                    {
                        var q = from x in dataVals where x.Index == vcateg.Index select x;
                        if (q.Count() > 0)
                        {
                            var vdata = q.First();
                            String sval = StatHelpers.ConvertValue(vdata.DataStringValue);
                            if (!String.IsNullOrEmpty(sval))
                            {
                                if (!oRet.ContainsKey(skey))
                                {
                                    oRet[skey] = new ValueDescs();
                                }
                                oRet[skey].Add(vdata);
                            }// ok
                        }
                    }// sKey
                }// vcateg
            }
            return oRet;
        }//ComputeVariableCategValues
        public Dictionary<int, String> ComputeCategDict(VariableDesc oCateg)
        {
            Dictionary<int, String> oRet = new Dictionary<int, string>();
            ValueDescs categVals = null;
            if ((oCateg != null) && oCateg.HasValues)
            {
                categVals = oCateg.Values;
            }
            if (categVals == null)
            {
                return oRet;
            }
            foreach (var v in categVals)
            {
                int key = v.Index;
                String s = StatHelpers.ConvertValue(v.DataStringValue);
                if ((key >= 0) && (!String.IsNullOrEmpty(s)))
                {
                    oRet[key] = s;
                }
            }// v
            return oRet;
        }// ComputeCategDisr
        public IEnumerable<int> GetCommonIndexes(VariableDesc oVar1, VariableDesc oVar2)
        {
            List<VariableDesc> oList = new List<VariableDesc>();
            oList.Add(oVar1);
            oList.Add(oVar2);
            return GetCommonIndexes(oList);
        }// GetCommonIndexes
        public IEnumerable<int> GetCommonIndexes(IEnumerable<VariableDesc> oVars)
        {
            List<int> oRet = new List<int>();
            if (oVars == null)
            {
                return oRet;
            }
            HashSet<int> oSet = null;
            foreach (var v in oVars)
            {
                ValueDescs vals = v.Values;
                HashSet<int> oSetCur = new HashSet<int>();
                foreach (var vx in vals)
                {
                    if ((vx != null) && (vx.Index >= 0))
                    {
                        String s = StatHelpers.ConvertValue(vx.DataStringValue);
                        if (!String.IsNullOrEmpty(s))
                        {
                            oSetCur.Add(vx.Index);
                        }
                    }// v
                }// vx
                if (oSet == null)
                {
                    oSet = oSetCur;
                }
                else
                {
                    HashSet<int> oDel = new HashSet<int>();
                    foreach (var ind in oSet)
                    {
                        if (!oSetCur.Contains(ind))
                        {
                            oDel.Add(ind);
                        }
                    }// ind
                    foreach (var ind in oDel)
                    {
                        oSet.Remove(ind);
                    } // ind
                    if (oSet.Count < 1)
                    {
                        break;
                    }
                }
            }// v
            if (oSet == null)
            {
                oSet = new HashSet<int>();
            }
            oRet = oSet.ToList();
            return oRet;
        }// GetCommonIndexes
        public Dictionary<VariableDesc, ValueDescs> GetCommonValues(IEnumerable<VariableDesc> oVars)
        {
            Dictionary<VariableDesc, ValueDescs> oRet = new Dictionary<VariableDesc, ValueDescs>();
            if (oVars != null)
            {
                var indexes = GetCommonIndexes(oVars);
                if (indexes == null)
                {
                    return oRet;
                }
                foreach (var v in oVars)
                {
                    if (v != null)
                    {
                        ValueDescs src = v.Values;
                        List<ValueDesc> vals = new List<ValueDesc>();
                        foreach (var ind in indexes)
                        {
                            var q = from x in src where x.Index == ind select x;
                            if (q.Count() > 0)
                            {
                                vals.Add(q.First());
                            }
                        }// ind
                        if (vals.Count < 1)
                        {
                            continue;
                        }
                        oRet[v] = new ValueDescs(vals);
                    }// v
                }// v
            }// oVars
            return oRet;
        }// GetCommonValues
        #endregion // Methods
        #region Helpers
        public void NavigateToPage(String sPageKey)
        {
            IPageLocator locator = this.PageLocator;
            NavigationService pServ = this.NavigationService;
            if ((locator != null) && (pServ != null))
            {
                var xPage = locator.GetPage(sPageKey);
                if (xPage != null)
                {
                    pServ.Navigate(xPage);
                }
            }
        }// NavigateToPage
        public void ShowError(Exception err)
        {
            if (err != null)
            {
                MessageBox.Show(err.Message, "StatApp", MessageBoxButton.OK, MessageBoxImage.Error);
            }// err
        }// showEror
        #endregion // Helpers
        #region Plot Helpers
        public PlotModel createBoxPlotModel(List<double[]> data, String[] varnames, String title)
        {
            if ((data == null) || (varnames == null))
            {
                return null;
            }
            int nv = data.Count;
            if ((nv < 1) || (varnames.Length < nv))
            {
                return null;
            }
            PlotModel model = null;
            try
            {
                String sText = "";
                for (int i = 0; i < nv; ++i)
                {
                    if (i > 0)
                    {
                        sText += ", " + varnames[i];
                    }
                    else
                    {
                        sText = varnames[i];
                    }
                }// i
                model = new PlotModel(sText) { LegendPlacement = LegendPlacement.Outside };
                var s1 = new BoxPlotSeries { Title = title, BoxWidth = 0.3 };
                double x = 0;
                foreach (var vv in data)
                {
                    if (vv == null)
                    {
                        continue;
                    }
                    List<double> values = vv.ToList();
                    values.Sort();
                    var median = GetMedian(values, false);
                    int r = values.Count % 2;
                    double firstQuartil = GetMedian(values.Take((values.Count + r) / 2), false);
                    double thirdQuartil = GetMedian(values.Skip((values.Count - r) / 2), false);
                    var iqr = thirdQuartil - firstQuartil;
                    var step = iqr * 1.5;
                    var upperWhisker = thirdQuartil + step;
                    upperWhisker = values.Where(v => v <= upperWhisker).Max();
                    var lowerWhisker = firstQuartil - step;
                    lowerWhisker = values.Where(v => v >= lowerWhisker).Min();
                    var outliers = values.Where(v => v > upperWhisker || v < lowerWhisker).ToList();
                    s1.Items.Add(new BoxPlotItem(x, lowerWhisker, firstQuartil, median, thirdQuartil, upperWhisker, outliers));
                    ++x;
                }// ivar
                model.Series.Add(s1);
                model.Axes.Add(new LinearAxis(AxisPosition.Left));
                model.Axes.Add(new LinearAxis(AxisPosition.Bottom) { MinimumPadding = 0.1, MaximumPadding = 0.1 });
            }
            catch (Exception /* ex */)
            {
            }
            return model;
        }//createBoxPlotModel
        public PlotModel createNormalPlotModel(VariableDesc oCategVar, VariableDesc oVar)
        {
            if (oVar == null)
            {
                return null;
            }
            var srcData = ComputeVariableCategValues(oCategVar, oVar);
            if (srcData == null)
            {
                return null;
            }
            List<ValueDesc> sortedData = new List<ValueDesc>();
            List<double> oList = new List<double>();
            Dictionary<String, ScatterSeries> oDict = new Dictionary<string, ScatterSeries>();
            foreach (var s in srcData.Keys)
            {
                oDict[s] = new ScatterSeries(s) { MarkerType = MarkerType.Circle };
                var vals = srcData[s];
                foreach (var v in vals)
                {
                    v.StringTag = s;
                    sortedData.Add(v);
                    oList.Add(v.DoubleValue);
                }
            }// s
            sortedData.Sort((v1, v2) =>
            {
                double d1 = v1.DoubleValue;
                double d2 = v2.DoubleValue;
                int nRet = -1;
                if (d1 > d2)
                {
                    nRet = 1;
                }
                else if (d1 == d2)
                {
                    nRet = 0;
                }
                return nRet;
            });
            int n = sortedData.Count;
            if (n < 2)
            {
                return null;
            }
            DescriptiveStatistics st = new DescriptiveStatistics(oList);
            double mean = st.Mean;
            double dev = st.StandardDeviation;
            if (dev <= 0.0)
            {
                return null;
            }
            var dist = new Normal();
            var s2 = new LineSeries { Title = "Distribution normale" };
            int nTotal = 1;
            int i = 0;
            double dn = (double)n;
            var oAr = sortedData.ToArray();
            ValueDesc vCur = null;
            while (i < n)
            {
                vCur = oAr[i];
                double xMax = vCur.DoubleValue;
                int c = 0;
                while ((vCur.DoubleValue <= xMax) && (i < n))
                {
                    double x = vCur.DoubleValue;
                    double xr = (x - mean) / dev;
                    double yr = dist.CumulativeDistribution(xr);
                    s2.Points.Add(new DataPoint(x, yr));
                    double yc = (double)nTotal / dn;
                    vCur.DoubleTag = yc;
                    String scateg = vCur.StringTag;
                    if ((!String.IsNullOrEmpty(scateg)) && oDict.ContainsKey(scateg))
                    {
                        oDict[scateg].Points.Add(new ScatterPoint(x, yc) { Tag = vCur.Index });
                    }
                    ++i;
                    ++c;
                    if (i >= n)
                    {
                        break;
                    }
                    vCur = oAr[i];
                }
                nTotal += c;
            }// i
            PlotModel model = new PlotModel(oVar.Name);
            foreach (var ss in oDict.Values)
            {
                model.Series.Add(ss);
            }
            s2.Smooth = true;
            model.Series.Add(s2);
            return model;
        }// createNormalPlotModel
        public PlotModel createHistogPlotModel(IEnumerable<double> data, String varname)
        {
            if (data == null)
            {
                return null;
            }
            double[] oAr = data.ToArray();
            int n = oAr.Length;
            if (n < 2)
            {
                return null;
            }
            ColumnSeries s2 = new ColumnSeries();
            DescriptiveStatistics st = new DescriptiveStatistics(oAr);
            double mean = st.Mean;
            double dev = st.StandardDeviation;
            if (dev <= 0.0)
            {
                return null;
            }
            var histog = new Histogram(oAr, 11);
            int nc = histog.BucketCount;
            Normal dist = new Normal();
            Collection<HistogItem> items = new Collection<HistogItem>();
            for (int i = 0; i < nc; ++i)
            {
                HistogItem item = new HistogItem();
                var b = histog[i];
                double x = (b.LowerBound + b.UpperBound) / 2.0;
                String label = String.Format("{0}", myconvert(x));
                int nx = (int)(dist.Density((x - mean) / dev) * n);
                item.Label = label;
                item.Value1 = b.Count;
                item.Value2 = nx;
                items.Add(item);
            }// i
            var model = new PlotModel(varname)
            {
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.RightTop,
                LegendOrientation = LegendOrientation.Vertical
            };
            // Add the axes, note that MinimumPadding and AbsoluteMinimum should be set on the value axis.
            model.Axes.Add(new CategoryAxis { ItemsSource = items, LabelField = "Label" });
            model.Axes.Add(new LinearAxis(AxisPosition.Left) { MinimumPadding = 0, AbsoluteMinimum = 0 });
            model.Series.Add(new ColumnSeries { Title = varname, ItemsSource = items, ValueField = "Value1" });
            model.Series.Add(new ColumnSeries { Title = "DistributionNormale", ItemsSource = items, ValueField = "Value2" });
            return model;
        }// createNormalPlotModel
        public DisplayItemsArray CreateDataDisplay(IEnumerable<IndivData> oIndivs,
            IEnumerable<VariableDesc> allVars, IEnumerable<VariableDesc> oVars)
        {
            if ((oIndivs == null) || (allVars == null) || (oVars == null))
            {
                return null;
            }
            if ((oIndivs.Count() < 1) || (allVars.Count() < 1) || (oVars.Count() < 1))
            {
                return null;
            }
            List<DisplayItems> oRet = null;
            try
            {
                var pMan = this.DataService;
                if (pMan == null)
                {
                    return null;
                }
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
                var vars = oVars.ToArray();
                int nv = vars.Length;
                oRet = new List<DisplayItems>();
                DisplayItems header = new DisplayItems();
                header.Add(new DisplayItem("Num", true));
                header.Add(new DisplayItem("Index", true));
                header.Add(new DisplayItem("ID", true));
                header.Add(new DisplayItem("Nom", true));
                header.Add(new DisplayItem("Photo", true));
                for (int i = 0; i < nv; ++i)
                {
                    header.Add(new DisplayItem(vars[i].Name, true));
                }// i
                oRet.Add(header);
                int irow = 1;
                foreach (var ind in oIndivs)
                {
                    DisplayItems line = new DisplayItems();
                    line.Tag = ind;
                    line.Add(new DisplayItem(irow++));
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
        }// CreateInitialDataDisplay
        public DisplayItemsArray CreateDataDisplay(IEnumerable<IndivDesc> oIndivs,
            IEnumerable<VariableDesc> allVars, IEnumerable<VariableDesc> oVars)
        {
            if ((oIndivs == null) || (allVars == null) || (oVars == null))
            {
                return null;
            }
            if ((oIndivs.Count() < 1) || (allVars.Count() < 1) || (oVars.Count() < 1))
            {
                return null;
            }
            List<DisplayItems> oRet = null;
            try
            {
                var pMan = this.DataService;
                if (pMan == null)
                {
                    return null;
                }
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
                var vars = oVars.ToArray();
                int nv = vars.Length;
                oRet = new List<DisplayItems>();
                DisplayItems header = new DisplayItems();
                header.Add(new DisplayItem("Num", true));
                header.Add(new DisplayItem("Index", true));
                header.Add(new DisplayItem("ID", true));
                header.Add(new DisplayItem("Nom", true));
                header.Add(new DisplayItem("Photo", true));
                for (int i = 0; i < nv; ++i)
                {
                    header.Add(new DisplayItem(vars[i].Name, true));
                }// i
                oRet.Add(header);
                int irow = 1;
                foreach (var ind in oIndivs)
                {
                    DisplayItems line = new DisplayItems();
                    line.Add(new DisplayItem(irow++));
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
        }// CreateInitialDataDisplay
        public Task<DisplayItemsArray> CreateDataDisplayAsync(IEnumerable<IndivData> oIndivs,
            IEnumerable<VariableDesc> allVars, IEnumerable<VariableDesc> oVars)
        {
            return Task.Run<DisplayItemsArray>(() =>
            {
                return CreateDataDisplay(oIndivs, allVars, oVars);
            });
        }// CreateDataDisplayAsync
        public Task<DisplayItemsArray> CreateDataDisplayAsync(IEnumerable<IndivDesc> oIndivs,
            IEnumerable<VariableDesc> allVars, IEnumerable<VariableDesc> oVars)
        {
            return Task.Run<DisplayItemsArray>(() =>
            {
                return CreateDataDisplay(oIndivs, allVars, oVars);
            });
        }// CreateDataDisplayAsync
        public DisplayItemsArray CreateVariablesInfoDisplay(IEnumerable<VariableInfo> oVars)
        {
            DisplayItemsArray oRet = new DisplayItemsArray();
            try
            {
                DisplayItems hh = new DisplayItems();
                hh.Add(new DisplayItem("Num", true));
                hh.Add(new DisplayItem("Variable", true));
                hh.Add(new DisplayItem("N", true));
                hh.Add(new DisplayItem("Min", true));
                hh.Add(new DisplayItem("Max", true));
                hh.Add(new DisplayItem("Median", true));
                hh.Add(new DisplayItem("Mean", true));
                hh.Add(new DisplayItem("Dev.", true));
                hh.Add(new DisplayItem("Skew.", true));
                hh.Add(new DisplayItem("Kurt.", true));
                hh.Add(new DisplayItem("Q05", true));
                hh.Add(new DisplayItem("Q10", true));
                hh.Add(new DisplayItem("Q25", true));
                hh.Add(new DisplayItem("Q75", true));
                hh.Add(new DisplayItem("Q90", true));
                hh.Add(new DisplayItem("Q95", true));
                oRet.Add(hh);
                int icur = 1;
                foreach (var v in oVars)
                {
                    DisplayItems line = new DisplayItems();
                    line.Add(new DisplayItem(icur++));
                    line.Add(new DisplayItem(v.VariableName));
                    line.Add(new DisplayItem(v.TotalValuesCount));
                    line.Add(new DisplayItem(v.MinValue));
                    line.Add(new DisplayItem(v.MaxValue));
                    line.Add(new DisplayItem(v.Median));
                    line.Add(new DisplayItem(v.MeanValue));
                    line.Add(new DisplayItem(v.Deviation));
                    line.Add(new DisplayItem(v.Skewness));
                    line.Add(new DisplayItem(v.Flatness));
                    line.Add(new DisplayItem(v.Quantile05));
                    line.Add(new DisplayItem(v.Quantile10));
                    line.Add(new DisplayItem(v.Quantile25));
                    line.Add(new DisplayItem(v.Quantile75));
                    line.Add(new DisplayItem(v.Quantile90));
                    line.Add(new DisplayItem(v.Quantile95));
                    oRet.Add(line);
                }// v
            }
            catch (Exception /*ex*/)
            {
                oRet = null;
            }
            return oRet;
        }// CreateVariablesInfoDisplay
        public Task<DisplayItemsArray> CreateVariablesInfoDisplayAsync(IEnumerable<VariableDesc> oVars)
        {
            return Task.Run<DisplayItemsArray>(() =>
            {
                DisplayItemsArray oRet = null;
                if (oVars != null)
                {
                    List<VariableInfo> oList = new List<VariableInfo>();
                    foreach (var v in oVars)
                    {
                        if ((v != null) && v.IsNumVar)
                        {
                            var vv = v.Info;
                            if ((vv != null) && (vv.TotalValuesCount > 0))
                            {
                                oList.Add(vv);
                            }
                        }
                    }// v
                    oRet = CreateVariablesInfoDisplay(oList);
                }// oVars
                return oRet;
            });
        }//CreateVariablesInfoDisplayAsync
        public static Dictionary<VariableDesc, Dictionary<int, double>> GetDoubleData(Dictionary<VariableDesc, ValueDescs> oData,
           MatriceComputeMode mode)
        {
            Dictionary<VariableDesc, Dictionary<int, double>> oRet = new Dictionary<VariableDesc, Dictionary<int, double>>();
            var keys = oData.Keys;
            foreach (var key in keys)
            {
                oRet[key] = new Dictionary<int, double>();
                var rdict = oRet[key];
                var vals = (oData[key]).ToArray();
                int n = vals.Length;
                double[] ddata = new double[n];
                if (key.IsNumVar)
                {
                    for (int i = 0; i < n; ++i)
                    {
                        var v = vals[i];
                        double vv = v.DoubleValue;
                        ddata[i] = vv;
                    }// i
                    if (mode == MatriceComputeMode.modeNormalize)
                    {
                        DescriptiveStatistics st = new DescriptiveStatistics(ddata);
                        double ec = st.StandardDeviation;
                        if (ec <= 0.0)
                        {
                            return null;
                        }
                        double mean = st.Mean;
                        for (int i = 0; i < n; ++i)
                        {
                            ddata[i] = (ddata[i] - mean) / ec;
                        }// i
                    }
                    else if (mode == MatriceComputeMode.modeProfil)
                    {
                        DescriptiveStatistics st = new DescriptiveStatistics(ddata);
                        double vmin = st.Minimum;
                        double vmax = st.Maximum;
                        if (vmax <= vmin)
                        {
                            return null;
                        }
                        double rg = vmax - vmin;
                        for (int i = 0; i < n; ++i)
                        {
                            ddata[i] = (ddata[i] - vmin) / rg;
                        }
                    }
                    else if (mode == MatriceComputeMode.modeRank)
                    {
                        double[] dz = BertinPartition.GetRanks(ddata);
                        if (dz == null)
                        {
                            return null;
                        }
                        for (int i = 0; i < n; ++i)
                        {
                            ddata[i] = dz[i];
                        }
                    }
                }
                else
                {
                    String[] data = new String[n];
                    for (int i = 0; i < n; ++i)
                    {
                        var v = vals[i];
                        data[i] = v.StringValue;
                    }// i
                    var p = BertinPartition.GetPartition(data);
                    if (p == null)
                    {
                        return null;
                    }
                    var cc = p.Classes;
                    for (int i = 0; i < n; ++i)
                    {
                        ddata[i] = (double)cc[i];
                    }// i
                }
                for (int i = 0; i < n; ++i)
                {
                    int index = vals[i].Index;
                    rdict[index] = ddata[i];
                }// i
            }// key
            return oRet;
        }//GetDoubleData
        public static Dictionary<VariableDesc, Dictionary<int, int>> GetIntData(Dictionary<VariableDesc, ValueDescs> oData,
           MatriceComputeMode mode, int nClasses)
        {
            Dictionary<VariableDesc, Dictionary<int, int>> oRet = new Dictionary<VariableDesc, Dictionary<int, int>>();
            var keys = oData.Keys;
            foreach (var key in keys)
            {
                oRet[key] = new Dictionary<int, int>();
                var rdict = oRet[key];
                var vals = (oData[key]).ToArray();
                int n = vals.Length;
                int[] ddata = new int[n];
                if (key.IsNumVar)
                {
                    double[] data = new double[n];
                    for (int i = 0; i < n; ++i)
                    {
                        var v = vals[i];
                        double vv = v.DoubleValue;
                        data[i] = vv;
                    }// i
                    if (mode == MatriceComputeMode.modeNormalize)
                    {
                        DescriptiveStatistics st = new DescriptiveStatistics(data);
                        double ec = st.StandardDeviation;
                        if (ec <= 0.0)
                        {
                            return null;
                        }
                        double mean = st.Mean;
                        for (int i = 0; i < n; ++i)
                        {
                            data[i] = (data[i] - mean) / ec;
                        }// i
                    }
                    else if (mode == MatriceComputeMode.modeProfil)
                    {
                        DescriptiveStatistics st = new DescriptiveStatistics(data);
                        double vmin = st.Minimum;
                        double vmax = st.Maximum;
                        if (vmax <= vmin)
                        {
                            return null;
                        }
                        double rg = vmax - vmin;
                        for (int i = 0; i < n; ++i)
                        {
                            data[i] = (data[i] - vmin) / rg;
                        }
                    }
                    else if (mode == MatriceComputeMode.modeRank)
                    {
                        double[] dz = BertinPartition.GetRanks(data);
                        if (dz == null)
                        {
                            return null;
                        }
                        for (int i = 0; i < n; ++i)
                        {
                            data[i] = dz[i];
                        }
                    }
                    var pc = BertinPartition.GetPartition(data, nClasses);
                    if (pc == null)
                    {
                        return null;
                    }
                    var cc = pc.Classes;
                    for (int i = 0; i < n; ++i)
                    {
                        var v = vals[i];
                        int ind = v.Index;
                        rdict[ind] = cc[i];
                    }// i
                }
                else
                {
                    String[] data = new String[n];
                    for (int i = 0; i < n; ++i)
                    {
                        var v = vals[i];
                        data[i] = v.StringValue;
                    }// i
                    var p = BertinPartition.GetPartition(data);
                    if (p == null)
                    {
                        return null;
                    }
                    var cc = p.Classes;
                    for (int i = 0; i < n; ++i)
                    {
                        var v = vals[i];
                        int ind = v.Index;
                        rdict[ind] = cc[i];
                    }// i
                }
            }// key
            return oRet;
        }//GetIntData
        public IEnumerable<IndivData> GetIndivsData(IEnumerable<IndivDesc> inds, IEnumerable<VariableDesc> oVars, MatriceComputeMode mode,
            int nClasses)
        {
            List<IndivData> oRet = null;
            try
            {
                var vars = oVars.ToArray();
                int nv = vars.Length;
                if (nv < 1)
                {
                    return null;
                }
                var vals = GetCommonValues(oVars);
                if (vals == null)
                {
                    return null;
                }
                var dictDouble = GetDoubleData(vals, mode);
                var dictInt = GetIntData(vals, mode, nClasses);
                if ((dictDouble == null) || (dictInt == null))
                {
                    return null;
                }
                oRet = new List<IndivData>();
                foreach (var ind in inds)
                {
                    int index = ind.IndivIndex;
                    if (index < 0)
                    {
                        continue;
                    }
                    double[] dd = new double[nv];
                    int[] di = new int[nv];
                    for (int j = 0; j < nv; ++j)
                    {
                        var vv = vars[j];
                        foreach (var x in dictDouble.Keys)
                        {
                            if (x.Id == vv.Id)
                            {
                                var dict = dictDouble[x];
                                if (dict.ContainsKey(index))
                                {
                                    dd[j] = dict[index];
                                }
                                break;
                            }//
                        }// double
                        foreach (var x in dictInt.Keys)
                        {
                            if (x.Id == vv.Id)
                            {
                                var dict = dictInt[x];
                                if (dict.ContainsKey(index))
                                {
                                    di[j] = dict[index];
                                }
                                break;
                            }//
                        }// double
                    }// j
                    var vz = new IndivData(ind, dd, di);
                    oRet.Add(vz);
                }// inds
            }// try
            catch (Exception /*ex */)
            {
                oRet = null;
            }
            return oRet;
        }// GetIndivsDtata
        #endregion // Plot Helpers
        #region Misc
        protected static double myconvert(double x)
        {
            double xx = 10.0 * x;
            if (x < 0.0)
            {
                xx -= 0.5;
            }
            else
            {
                xx += 0.5;
            }
            int nx = (int)xx;
            return (double)(nx / 10.0);
        }// myconvert
        protected static double myconvert10000(double x)
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
        }// myconvert10000
        #endregion // Misc
    }// class StatViewModelBase
}
