using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using StatData;
namespace StatApp.ModelView
{
    
    public class CategClusterSet : Observable, ICloneable
    {
        #region Instance Variables
        private OrdModelView m_model;
        private IndivDatas m_indivs;
        private int m_nvars;
        private double m_currentcrit;
        private double m_globalcrit;
        private List<Cluster> m_clusters;
        #endregion // Instance Variables
        #region Constructors
        public CategClusterSet()
        {
        }
        public CategClusterSet(OrdModelView model)
        {
            m_model = model;
            if (model != null)
            {
                m_indivs = (IndivDatas)model.Individus.Clone();
                if ((m_indivs != null) && (m_indivs.Count > 0))
                {
                    var p = m_indivs.First();
                    m_nvars = p.IntData.Length;
                }
            }
        }
        public CategClusterSet(IndivDatas inds)
        {
            if (inds != null)
            {
                m_indivs = (IndivDatas)inds.Clone();
                if ((m_indivs != null) && (m_indivs.Count > 0))
                {
                    var p = m_indivs.First();
                    m_nvars = p.IntData.Length;
                }
            }
        }
        public object Clone()
        {
            CategClusterSet pRet = new CategClusterSet();
            pRet.m_model = this.m_model;
            pRet.m_nvars = this.m_nvars;
            pRet.m_currentcrit = this.m_currentcrit;
            pRet.m_globalcrit = this.m_globalcrit;
            pRet.m_indivs = this.m_indivs;
            if (this.m_clusters != null)
            {
                pRet.m_clusters = new List<Cluster>();
                foreach (var x in this.m_clusters)
                {
                    pRet.m_clusters.Add((Cluster)x.Clone());
                }
            }
            return pRet;
        }
        #endregion // Constructors
        #region Properties
        public bool IsValid
        {
            get
            {
                return (this.GlobalCrit != 0.0) && (this.CurrentCrit != 0.0) && (this.Clusters.Count > 0);
            }
            set { }
        }// IsValid
        public OrdModelView Model
        {
            get
            {
                return m_model;
            }
            set
            {
                if (value != m_model)
                {
                    m_model = value;
                    this.Indivs = null;
                    if (m_model != null)
                    {
                        this.Indivs = (IndivDatas)m_model.Individus.Clone();
                        if ((m_indivs != null) && (m_indivs.Count > 0))
                        {
                            var p = m_indivs.First();
                            m_nvars = p.IntData.Length;
                            NotifyPropertyChanged("VariablesCount");
                        }
                    }
                    NotifyPropertyChanged("Model");
                }
            }
        }// Model
        public IndivDatas Indivs
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
                if (m_indivs != value)
                {
                    m_indivs = value;
                    if ((m_indivs != null) && (m_indivs.Count > 0))
                    {
                        var p = m_indivs.First();
                        m_nvars = p.IntData.Length;
                        NotifyPropertyChanged("VariablesCount");
                    }
                    NotifyPropertyChanged("Indivs");
                }
            }
        }// Indivs
        public List<Cluster> Clusters
        {
            get
            {
                if (m_clusters == null)
                {
                    m_clusters = new List<Cluster>();
                }
                return m_clusters;
            }
            set
            {
                if (value != m_clusters)
                {
                    m_clusters = value;
                    NotifyPropertyChanged("Clusters");
                }
            }
        }// Clusters
        public int VariablesCount
        {
            get
            {
                return m_nvars;
            }
            set
            {
                if (value != m_nvars)
                {
                    m_nvars = value;
                    NotifyPropertyChanged("VariablesCount");
                }
            }
        }// VariablesCount
        public double GlobalCrit
        {
            get
            {
                return m_globalcrit;
            }
            set
            {
                if (value != m_globalcrit)
                {
                    m_globalcrit = value;
                    NotifyPropertyChanged("GlobalCrit");
                }
            }
        }// GlobalCrit
        public double CurrentCrit
        {
            get
            {
                return m_currentcrit;
            }
            set
            {
                if (value != CurrentCrit)
                {
                    m_currentcrit = value;
                    NotifyPropertyChanged("CurrentCrit");
                }
            }
        }// CurrentCrit
        public double Variance
        {
            get
            {
                double dRet = 0.0;
                foreach (var v in this.Clusters)
                {
                    dRet += v.Variance;
                }
                return dRet;
            }
            set { }
        }// Variance
        #endregion Properties
        #region Methods
        public void UpdateCenters()
        {
            foreach (var oCluster in this.Clusters)
            {
                oCluster.UpdateCenter();
            }
        }// UpdateCenters
        public static Task<CategClusterSet> Clusterize(OrdModelView model, int nClusters, int nbIterations,
            CancellationToken cancellationToken, IProgress<Tuple<int,CategClusterSet> > progress)
        {
            return Task.Run<CategClusterSet>(() => {
                CategClusterSet oBest = null;
                double bestcrit = 0.0;
                for (int i = 0; i < nbIterations; ++i)
                {
                    CategClusterSet pp = null;
                    int f = (int)(((double)i / (double)nbIterations) * 100.0 + 0.5);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                    var p = ProcessAsync(model, nClusters, cancellationToken);
                    if (p == null)
                    {
                        break;
                    }
                    if (oBest == null)
                    {
                        oBest = p;
                        bestcrit = p.CurrentCrit;
                        if (progress != null)
                        {
                            pp = (CategClusterSet)oBest.Clone();
                            pp.UpdateCenters();
                        }
                    }
                    else if (p.CurrentCrit > bestcrit)
                    {
                        oBest = p;
                        bestcrit = p.CurrentCrit;
                        if (progress != null)
                        {
                           pp = (CategClusterSet)oBest.Clone();
                           pp.UpdateCenters();
                        }
                    }
                    if (progress != null)
                    {
                        progress.Report(new Tuple<int, CategClusterSet>(f, pp));
                    }
                }// i
                if (oBest != null)
                {
                    oBest.UpdateCenters();
                }
                return oBest;
            }, cancellationToken);
        }// Clusterize
        public static CategClusterSet ProcessAsync(OrdModelView model, int nClusters, CancellationToken cancellationToken)
        {
            if ((model == null) || (nClusters < 2))
            {
                return null;
            }
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }
            var rd = new Random();
            CategClusterSet oRet = new CategClusterSet(model);
            Cluster[] cc = new Cluster[nClusters];
            IndivData[] inds = oRet.Indivs.ToArray();
            int nMax = inds.Length;
            if (nMax < nClusters)
            {
                return null;
            }
            HashSet<int> oCur = new HashSet<int>();
            for (int i = 0; i < nClusters; ++i)
            {
                int nx = rd.Next(0, nMax);
                while (nx >= nMax)
                {
                    nx = rd.Next(nMax);
                }
                IndivData indiv = inds[nx];
                int index = indiv.IndivIndex;
                while (oCur.Contains(index))
                {
                    nx = rd.Next(0, nMax);
                    while (nx >= nMax)
                    {
                        nx = rd.Next(0, nMax);
                    }
                    indiv = inds[nx];
                    index = indiv.IndivIndex;
                }
                oCur.Add(index);
                var xc = new Cluster(indiv,ClassificationType.Utility);
                xc.Name = String.Format("CU{0}", i + 1);
                xc.Index = i;
                cc[i] = xc;
            }// i
            oRet.Clusters = cc.ToList();
            double xx = oRet.compute_global_crit(cancellationToken);
            oRet.GlobalCrit = xx;
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }
            foreach (var ind in inds)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }
                if (!oCur.Contains(ind.IndivIndex))
                {
                    if (!oRet.processOne(ind, cancellationToken))
                    {
                        return null;
                    }
                }
            }// inds
            return oRet;
        }// Process
        public static Task<CategClusterSet> KMeans(OrdModelView model, int nClusters, int nbIterations, CancellationToken cancellationToken,
            IProgress<Tuple<int, CategClusterSet>> progress)
        {
            return Task.Run<CategClusterSet>(() =>
            {
                CategClusterSet oBest = new CategClusterSet(model);
                oBest.initializeCiusters(nClusters);
                for (int i = 0; i < nbIterations; ++i)
                {
                    CategClusterSet pp = new CategClusterSet();
                    pp.m_model = oBest.m_model;
                    pp.m_nvars = oBest.m_nvars;
                    pp.m_indivs = oBest.m_indivs;
                    List<Cluster> oList = new List<Cluster>();
                    foreach (var c in oBest.Clusters)
                    {
                        Cluster cc = new Cluster();
                        cc.Index = c.Index;
                        cc.Name = c.Name;
                        cc.Center = (double[])c.Center.Clone();
                        oList.Add(cc);
                    }// c
                    pp.Clusters = oList;
                    int f = (int)(((double)i / (double)nbIterations) * 100.0 + 0.5);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                    pp.kmeansstep(cancellationToken);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                    if (oBest.Equals(pp))
                    {
                        break;
                    }
                    oBest = pp;
                    if (progress != null)
                    {
                        progress.Report(new Tuple<int, CategClusterSet>(f, pp));
                    }
                }// i
                return oBest;
            }, cancellationToken);
        }//KMeans 
        #endregion // Methods
        #region helpers
        private double compute_global_crit(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return 0.0;
            }
            var source = this.Indivs;
            if (source == null)
            {
                return 0.0;
            }
            int nv = this.VariablesCount;
            if (nv < 1)
            {
                return 0.0;
            }
            Dictionary<int, Dictionary<int, int>> oDict = new Dictionary<int, Dictionary<int, int>>();
            double sum = 0.0;
            int nCount = 0;
            foreach (var ind in source)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return 0.0;
                }
                var dd = ind.IntData;
                if (dd == null)
                {
                    continue;
                }
                if (dd.Length < nv)
                {
                    continue;
                }
                ++nCount;
                for (int icol = 0; icol < nv; ++icol)
                {
                    if (!oDict.ContainsKey(icol))
                    {
                        oDict[icol] = new Dictionary<int, int>();
                    }
                    var oMap = oDict[icol];
                    for (int i = 0; i < nv; ++i)
                    {
                        int ival = dd[i];
                        if (!oMap.ContainsKey(ival))
                        {
                            oMap[ival] = 1;
                        }
                        else
                        {
                            oMap[ival] = oMap[ival] + 1;
                        }
                    }// icol
                }// icol
            }// ind
            if (nCount < 1)
            {
                return 0.0;
            }
            if (cancellationToken.IsCancellationRequested)
            {
                return 0.0;
            }
            double dCount = (double)nCount;
            foreach (var key in oDict.Keys)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return 0.0;
                }
                var oMap = oDict[key];
                foreach (var c in oMap.Values)
                {
                    double xx = (double)c / dCount;
                    sum += xx * xx;
                }// c
            }// key
            return sum;
        }//compute_global_crit 
        private double compute_current_crit(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return 0.0;
            }
            int nv = this.VariablesCount;
            if (nv < 1)
            {
                return 0.0;
            }
            Dictionary<int, Dictionary<int, int>> oDict = new Dictionary<int, Dictionary<int, int>>();
            double sum = 0.0;
            var clusters = this.Clusters;
            int nCount = 0;
            double xcount = 0.0;
            foreach (var oCluster in clusters)
            {
                xcount += 1.0;
                if (cancellationToken.IsCancellationRequested)
                {
                    return 0.0;
                }
                if (oCluster.IsEmpty)
                {
                    continue;
                }
                int nClusterIndex = oCluster.Index;
                if (nClusterIndex < 0)
                {
                    continue;
                }
                if (!oDict.ContainsKey(nClusterIndex))
                {
                    oDict[nClusterIndex] = new Dictionary<int, int>();

                }
                var oMap = oDict[nClusterIndex];
                var col = oCluster.Elements;
                foreach (var ind in col)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return 0.0;
                    }
                    var dd = ind.IntData;
                    if (dd == null)
                    {
                        continue;
                    }
                    if (dd.Length < nv)
                    {
                        continue;
                    }
                    ++nCount;
                    for (int icol = 0; icol < nv; ++icol)
                    {
                        int ival = dd[icol];
                        if (!oMap.ContainsKey(ival))
                        {
                            oMap[ival] = 1;
                        }
                        else
                        {
                            oMap[ival] = oMap[ival] + 1;
                        }
                    }// icol
                }// ind
            }// oCluster
            if (cancellationToken.IsCancellationRequested)
            {
                return 0.0;
            }
            if (nCount < 1)
            {
                return 0.0;
            }
            double dCount = (double)nCount;
            double dGlobal = this.GlobalCrit;
            foreach (var key in oDict.Keys)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return 0.0;
                }
                var oMap = oDict[key];
                var q = from x in clusters where x.Index == key select x;
                if (q.Count() > 0)
                {
                    var oCluster = q.First();
                    int total = oCluster.Elements.Count();
                    if (total > 0)
                    {
                        double den = (double)total;
                        double ss = 0.0;
                        foreach (var c in oMap.Values)
                        {
                            double xx = (double)c / den;
                            ss += xx * xx;
                        }// c
                        double y = ss - dGlobal;
                        sum += (den / dCount) * y;
                    }// total
                }//
            }// key
            if (xcount > 0)
            {
                return (sum / xcount);
            }
            else
            {
                return sum;
            }
        }//compute_current_crit 
        private bool processOne(IndivData oIndiv, CancellationToken cancellationToken)
        {
            int nBestIndex = -1;
            double bestCrit = 0.0;
            foreach (var cluster in this.Clusters)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }
                int index = cluster.Index;
                cluster.AddUtility(oIndiv);
                double c = compute_current_crit(cancellationToken);
                cluster.RemoveUtility(oIndiv);
                if (nBestIndex < 0)
                {
                    if (c != 0.0)
                    {
                        nBestIndex = index;
                        bestCrit = c;
                    }
                }
                else if (c > bestCrit)
                {
                    nBestIndex = index;
                    bestCrit = c;
                }
            }// cluster
            if ((nBestIndex < 0) || (bestCrit == 0.0))
            {
                return false;
            }
            var qq = from x in this.Clusters where x.Index == nBestIndex select x;
            if (qq.Count() > 0)
            {
                var p = qq.First();
                p.AddUtility(oIndiv);
                this.CurrentCrit = bestCrit;
                return true;
            }
            return false;
        }// processOne
        private bool initializeCiusters(int nbClusters)
        {
            bool oRet = false;
            var inds = this.Indivs;
           if ((inds == null) || (nbClusters < 2))
           {
               return oRet;
           }
           if (inds.Count < nbClusters)
           {
               return oRet;
           }
           double[] dd = inds.First().DoubleData;
           int nv = dd.Length;
           if (nv < 1)
           {
               return oRet;
           }
            double[,] dLimits  = new double[3,nv];
            for (int i = 0; i < nv; ++i)
            {
                double x = dd[i];
                dLimits[0, i] = x;
                dLimits[1, i] = x;
                dLimits[2, i] = 0.0;
            }// i
            int nc = 0;
            foreach (var ind in inds)
            {
                double[] ddx = ind.DoubleData;
                if (ddx.Length < nv)
                {
                    continue;
                }
                ++nc;
                for (int i = 0; i < nv; ++i)
                {
                    double x = ddx[i];
                    if (x < dLimits[0, i])
                    {
                        dLimits[0, i] = x;
                    }
                    else if (x > dLimits[1, i])
                    {
                        dLimits[1, i] = x;
                    }
                }// i
            }// ind
            if (nc < 1)
            {
                return oRet;
            }
            for (int i = 0; i < nv; ++i)
            {
                double x1 = dLimits[0, i];
                double x2 = dLimits[1, i];
                double step = (x2 - x1) / (nbClusters + 2);
                if (step <= 0.0)
                {
                    return oRet;
                }
                dLimits[2, i] = step;
            }// i
            Cluster[] cc = new Cluster[nbClusters];
            for (int i = 0; i < nbClusters; ++i)
            {
                double[] xc = new double[nv];
                 for (int j = 0; j < nv; ++j)
                {
                    xc[j] = dLimits[0, j] + (i + 1) * dLimits[2, j];
                }// j
                Cluster c = new Cluster();
                c.Index = i;
                c.Name = String.Format("KM{0}", i + 1);
                c.Center = xc;
                cc[i] = c;
            }// i
            this.m_clusters = cc.ToList();
            return true;
        }// initializeClusters
        private int findNearestClusterIndex(IndivData ind, out double dMin)
        {
            int nRet = -1;
            dMin = 0.0;
            if (ind == null)
            {
                return nRet;
            }
            foreach (var cluster in this.Clusters)
            {
                double d = cluster.GetDistance(ind);
                if (nRet < 0)
                {
                    nRet = cluster.Index;
                    dMin = d;
                }
                else if (d < dMin)
                {
                    nRet = cluster.Index;
                    dMin = d;
                }
            }// cluster
            return nRet;
        }// findNearestCluster
        private void kmeansstep(CancellationToken cancellationToken)
        {
            foreach (var ind in this.Indivs)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                double dMin = 0.0;
                int nIndex = findNearestClusterIndex(ind, out dMin);
                if (nIndex >= 0)
                {
                    var q = from x in this.Clusters where x.Index == nIndex select x;
                    if (q.Count() > 0)
                    {
                        var c = q.First();
                        c.AddKMeans(ind);
                    }
                }
            }// ind
            foreach (var cluster in this.Clusters)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                cluster.UpdateCenter();
            }
        }// kmeansstep
        #endregion // helpers
        private bool contains(Cluster oCluster)
        {
            Cluster cluster = null;
            foreach (var ind1 in oCluster.Elements)
            {
                if (cluster != null)
                {
                    break;
                }
                int nIndex = ind1.IndivIndex;
                foreach (var c in this.Clusters)
                {
                    var q = from x in c.Elements where x.IndivIndex == nIndex select x;
                    if (q.Count() > 0)
                    {
                        cluster = c;
                        break;
                    }
                }// c
            }// ind1
            if (cluster == null)
            {
                return false;
            }
            if (cluster.Count != oCluster.Count)
            {
                return false;
            }
            foreach (var ind1 in oCluster.Elements)
            {
                var q = from x in cluster.Elements where x.IndivIndex == ind1.IndivIndex select x;
                if (q.Count() < 1)
                {
                    return false;
                }
            }// ind1
            return true;
        }// contains
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is CategClusterSet))
            {
                return false;
            }
            CategClusterSet p = obj as CategClusterSet;
            foreach (var oCluster in this.Clusters)
            {
                if (!p.contains(oCluster))
                {
                    return false;
                }
            }// oCluster
            return true;
        }// Equals
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }// class CategClusterSet
}
