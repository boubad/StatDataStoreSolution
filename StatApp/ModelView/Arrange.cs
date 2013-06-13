using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.ObjectModel;
using StatData;

namespace StatApp.ModelView
{
    public class ArrangeResult : Observable, ICloneable, IComparable
    {
        private double m_crit = -1.0;
        private List<IndivData> m_list = null;
        #region Constructors
        public ArrangeResult()
        {
        }
        public ArrangeResult(double crit, List<IndivData> oList)
        {
            this.Criteria = crit;
            this.Indivs = oList;
        }
        public object Clone()
        {
            ArrangeResult p = new ArrangeResult();
            p.Criteria = this.Criteria;
            p.Indivs = this.Indivs;
            return p;
        }
        #endregion // constructors
        #region Properties
        public bool IsValid
        {
            get
            {
                return (m_crit > 0.0) && (m_list != null) && (m_list.Count > 0);
            }
            set
            {
            }
        }// IsValid
        public double Criteria
        {
            get
            {
                return m_crit;
            }
            set
            {
                if (value != m_crit)
                {
                    m_crit = value;
                    NotifyPropertyChanged("Criteria");
                    NotifyPropertyChanged("IsValid");
                }
            }
        }// Criteria
        public List<IndivData> Indivs
        {
            get
            {
                if (m_list == null)
                {
                    m_list = new List<IndivData>();
                }
                return m_list;
            }
            set
            {
                if (value != m_list)
                {
                    if (value == null)
                    {
                        m_list = null;
                    }
                    else
                    {
                        m_list = new List<IndivData>();
                        m_list.AddRange(value);
                    }
                    NotifyPropertyChanged("Indivs");
                    NotifyPropertyChanged("IsValid");
                }
            }
        }// Indivs
        #endregion
        #region Methods
        public void Clear()
        {
            m_crit = -1.0;
            m_list = null;
            NotifyPropertyChanged("Criteria");
            NotifyPropertyChanged("Indivs");
            NotifyPropertyChanged("IsValid");
        }// Clear
        public bool Add(IndivData ind)
        {
            if (ind == null)
            {
                return false;
            }
            if (!ind.IsValid)
            {
                return false;
            }
            var q = from x in this.Indivs where x.IndivIndex == ind.IndivIndex select x;
            if (q.Count() > 0)
            {
                return false;
            }
            this.Indivs.Add(ind);
            NotifyPropertyChanged("Indivs");
            NotifyPropertyChanged("IsValid");
            NotifyPropertyChanged("Criteria");
            return true;
        }// Add
        #endregion // Methods
        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return -1;
            }
            if (!(obj is ArrangeResult))
            {
                return -1;
            }
            ArrangeResult p = obj as ArrangeResult;
            if (this.IsValid && p.IsValid)
            {
                if (this.Criteria < p.Criteria)
                {
                    return -1;
                }
                else if (this.Criteria > p.Criteria)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            else if (this.IsValid)
            {
                return -1;
            }
            else if (p.IsValid)
            {
                return 1;
            }
            return 0;
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is ArrangeResult))
            {
                return false;
            }
            ArrangeResult p = obj as ArrangeResult;
            if (this.IsValid && p.IsValid)
            {
                return (this.Criteria == p.Criteria);
            }
            return false;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override string ToString()
        {
            if (!this.IsValid)
            {
                return "{,[]}";
            }
            StringBuilder sb = new StringBuilder("{");
            sb.Append(this.Criteria);
            sb.Append(",[");
            int n = this.Indivs.Count;
            for (int i = 0; i < n; ++i)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }
                var ind = this.Indivs[i];
                if (ind != null)
                {
                    sb.Append(ind.IndivIndex);
                }
            }// i
            sb.Append(" ] }");
            return sb.ToString();
        }
    }// class ArrangeResult
    public class ArrangeSet
    {
        #region Instance variables
        private Random m_random = new Random();
        private double m_crit = -1.0;
        private List<IndivData> m_list = null;
        private List<IndivData> m_left = null;
        private Dictionary<int, Dictionary<int, double>> m_dist = null;
        #endregion // Instance variables
        #region Constructors
        public ArrangeSet()
        {
            m_list = new List<IndivData>();
            m_left = new List<IndivData>();
            m_crit = -1.0;
        }// ArrangeSet
        public ArrangeSet(IEnumerable<IndivData> inds, IndivData indStart)
        {
            m_list = new List<IndivData>();
            m_left = new List<IndivData>();
            m_crit = -1.0;
            if (inds != null)
            {
                m_left.AddRange(inds);
                if (indStart != null)
                {
                    var q = from x in m_left where x.IndivIndex == indStart.IndivIndex select x;
                    if (q.Count() > 0)
                    {
                        var p = q.First();
                        m_left.Remove(p);
                        m_list.Add(p);
                    }
                }// indStart
                computeDistances(inds);
            }// inds
        }// ArrangeSet
        public ArrangeSet(IEnumerable<IndivData> inds)
        {
            m_list = new List<IndivData>();
            m_left = new List<IndivData>();
            m_crit = -1.0;
            if (inds != null)
            {
                m_left.AddRange(inds);
                IndivData indStart = null;
                if (m_left.Count > 0)
                {
                    indStart = m_left.First();
                    m_left.Remove(indStart);
                    m_list.Add(indStart);
                }
                computeDistances(inds);
            }// inds
        }// ArrangeSet
        #endregion Constructors
        #region Properties
        public bool IsValid
        {
            get
            {
                return (m_list != null) && (m_list.Count > 0) && (m_dist != null);
            }
            set { }
        }// IsValid
        public double Criteria
        {
            get
            {
                return m_crit;
            }
            set
            {
                m_crit = value;
            }
        }// Criteria
        public List<IndivData> CurrentList
        {
            get
            {
                if (m_list == null)
                {
                    m_list = new List<IndivData>();
                }
                return m_list;
            }
            set { }
        }// CurrentList
        public List<IndivData> LefttList
        {
            get
            {
                if (m_left == null)
                {
                    m_left = new List<IndivData>();
                }
                return m_left;
            }
            set { }
        }// LeftList
        #endregion Properties
        #region Methods
        public static Task<ArrangeResult> ArrangeAsync(OrdModelView model, int nbIterations,
            CancellationToken cancellationToken, IProgress<Tuple<int, ArrangeResult>> progress)
        {
            return Task.Run<ArrangeResult>(() =>
            {
                ArrangeResult pRet = null;
                try
                {
                    Object syncObject = new Object();
                    var inds = model.Individus;
                    for (int i = 0; i < nbIterations; ++i)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return null;
                        }
                        ArrangeResult pp = null;
                        int f = (int)(((double)i / (double)nbIterations) * 100.0 + 0.5);
                        ArrangeResult pCur = Arrange(inds, cancellationToken);
                        if (pCur == null)
                        {
                            return null;
                        }
                        if (pCur.IsValid)
                        {
                            lock (syncObject)
                            {
                                if (pRet == null)
                                {
                                    pRet = pCur;
                                    if (progress != null)
                                    {
                                        pp = (ArrangeResult)pCur.Clone();
                                    }
                                }
                                else
                                {
                                    if (pCur.Criteria < pRet.Criteria)
                                    {
                                        pRet = pCur;
                                        if (progress != null)
                                        {
                                            pp = (ArrangeResult)pCur.Clone();
                                        }
                                    }
                                }
                            }// lock
                        }// valid
                        if (progress != null)
                        {
                            progress.Report(new Tuple<int, ArrangeResult>(f, pp));
                        }
                    }// i
                }
                catch (Exception /*ex */)
                {
                    pRet = null;
                }
                return pRet;
            });
        }// ArranceAsync
        public static int[] ArrangeIndex(IEnumerable<IndivData> inds, int nbIterations, CancellationToken cancellationToken)
        {
            int[] pRet = null;
            ArrangeResult oSet = null;
            try
            {
                for (int i = 0; i < nbIterations; ++i)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return null;
                    }
                    ArrangeResult pCur = Arrange(inds, cancellationToken);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return null;
                    }
                    if (pCur == null)
                    {
                        return null;
                    }
                    if (pCur.IsValid)
                    {
                        if (oSet == null)
                        {
                            oSet = pCur;
                        }
                        else
                        {
                            if (pCur.Criteria < oSet.Criteria)
                            {
                                oSet = pCur;
                            }
                        }
                    }// valid
                }// i
                if (oSet != null)
                {
                    List<int> indexes = new List<int>();
                    foreach (var v in oSet.Indivs)
                    {
                        indexes.Add(v.IndivIndex);
                    }
                    pRet = indexes.ToArray();
                }// oSet
            }
            catch (Exception /*ex */)
            {
                pRet = null;
            }
            return pRet;
        }// ArrangeIndex
        public static Task<int[]> ArrangeOneIndexAsync(IEnumerable<IndivData> inds, int nbIterations, CancellationToken cancellationToken)
        {
            return Task.Run<int[]>(() => {
                return ArrangeIndex(inds, nbIterations, cancellationToken);
            });
        }// ArrangeOneIndexAsync
        public static Tuple<int[], int[]> ArrangeIndex(OrdModelView model, int nbIterations, CancellationToken cancellationToken)
        {
                int[] pRows = null;
                int[] pCols = null;
                var inds = model.Individus;
                List<IndivData> oList = transpose(inds);
                int nx = (nbIterations > 5) ? 5 : nbIterations;
                pCols = ArrangeIndex(oList, nx, cancellationToken);
                pRows = ArrangeIndex(inds, nbIterations, cancellationToken);
                return new Tuple<int[], int[]>(pRows, pCols);
        }// ArrangeIndexAsync
        public static Task<Tuple<int[], int[]>> ArrangeIndexAsync(OrdModelView model, int nbIterations, CancellationToken cancellationToken)
        {
            return Task.Run<Tuple<int[], int[]>>(() =>
            {
                return ArrangeIndex(model, nbIterations, cancellationToken);
            }, cancellationToken);
        }// ArrangeIndexAsync
        #endregion // Methods
        #region Helpers
        private void computeDistances(IEnumerable<IndivData> inds)
        {
            if (inds == null)
            {
                return;
            }
            var col = inds.ToArray();
            int n = col.Length;
            m_dist = new Dictionary<int, Dictionary<int, double>>();
            for (int i = 0; i < n; ++i)
            {
                var ind1 = col[i];
                int iIndex = ind1.IndivIndex;
                m_dist[iIndex] = new Dictionary<int, double>();
                for (int j = 0; j <= i; ++j)
                {
                    var ind2 = col[j];
                    int jIndex = ind2.IndivIndex;
                    if (!m_dist.ContainsKey(jIndex))
                    {
                        m_dist[jIndex] = new Dictionary<int, double>();
                    }
                    if (iIndex == jIndex)
                    {
                        (m_dist[iIndex])[iIndex] = 0.0;
                    }
                    else
                    {
                        double d = ind1.ComputeDistance(ind2);
                        (m_dist[iIndex])[jIndex] = d;
                        (m_dist[jIndex])[iIndex] = d;
                    }
                }// j
            }// i
        }// inds
        private double getDistance(IndivData ind1, IndivData ind2)
        {
            double dRet = 0.0;
            if ((m_dist != null) && (ind1 != null) && (ind2 != null))
            {
                int iIndex = ind1.IndivIndex;
                int jIndex = ind2.IndivIndex;
                if (iIndex != jIndex)
                {
                    dRet = (m_dist[iIndex])[jIndex];
                }
            }// ok
            return dRet;
        }// getDistance
        private void updateCriteria()
        {
            if (!this.IsValid)
            {
                return;
            }
            int n = this.CurrentList.Count;
            if (n < 2)
            {
                return;
            }
            double sum = 0.0;
            for (int i = 1; i < n; ++i)
            {
                var ind1 = this.CurrentList[i - 1];
                var ind2 = this.CurrentList[i];
                double d = getDistance(ind1, ind2);
                sum += d;
            }// i
            this.m_crit = sum;
        }// updateCriteria
        private List<IndivData> getNearest()
        {
            List<IndivData> oRet = null;
            var col = this.CurrentList;
            int n = col.Count;
            if (n < 1)
            {
                return null;
            }
            var colsource = this.LefttList;
            int ns = colsource.Count;
            if (ns < 1)
            {
                return null;
            }
            var ind = col[n - 1];
            double dMin = -1.0;
            for (int i = 0; i < ns; ++i)
            {
                var cur = colsource[i];
                double d = getDistance(ind, cur);
                if (oRet == null)
                {
                    oRet = new List<IndivData>();
                    oRet.Add(cur);
                    dMin = d;
                }
                else if (d < dMin)
                {
                    oRet.Clear();
                    oRet.Add(cur);
                }
                else if (d == dMin)
                {
                    oRet.Add(cur);
                }
            }// i
            return oRet;
        }// getNearset
        private bool arrange(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }
            if (!this.IsValid)
            {
                return false;
            }
            while (this.LefttList.Count > 0)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }
                List<IndivData> oListCandidates = getNearest();
                if (oListCandidates == null)
                {
                    break;
                }
                int nMax = oListCandidates.Count;
                if (nMax < 1)
                {
                    break;
                }
                if (nMax == 1)
                {
                    var cur = oListCandidates.First();
                    var q = from x in this.LefttList where x.IndivIndex == cur.IndivIndex select x;
                    if (q.Count() < 1)
                    {
                        return false;
                    }
                    var p = q.First();
                    this.LefttList.Remove(p);
                    this.CurrentList.Add(p);
                }
                else
                {
                    int ii = m_random.Next(0, nMax);
                    var cur = oListCandidates[ii];
                    var q = from x in this.LefttList where x.IndivIndex == cur.IndivIndex select x;
                    if (q.Count() < 1)
                    {
                        return false;
                    }
                    var p = q.First();
                    this.LefttList.Remove(p);
                    this.CurrentList.Add(p);
                }
            }// while
            this.updateCriteria();
            return true;
        }// arrange
        private static ArrangeResult Arrange(IEnumerable<IndivData> inds, CancellationToken cancellationToken)
        {
            ArrangeSet oRet = null;
            try
            {
                var indivs = inds.ToList();
                int n = indivs.Count;
                if (!cancellationToken.IsCancellationRequested)
                {
                    for (int i = 0; i < n; ++i)
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            ArrangeSet pSet = new ArrangeSet(indivs, indivs[i]);
                            if (pSet.arrange(cancellationToken))
                            {
                                if (pSet.IsValid && (!cancellationToken.IsCancellationRequested))
                                {
                                    if (oRet == null)
                                    {
                                        oRet = pSet;
                                    }
                                    else if (pSet.Criteria < oRet.Criteria)
                                    {
                                        oRet = pSet;
                                    }
                                }// valid
                            }// arrange
                        }// process
                    }// i
                }// ok
            }
            catch (Exception /* ex */)
            {
                oRet = null;
            }
            if (cancellationToken.IsCancellationRequested)
            {
                oRet = null;
            }
            ArrangeResult pRet = null;
            if ((oRet != null) && oRet.IsValid)
            {
                pRet = new ArrangeResult(oRet.Criteria, oRet.CurrentList);
            }
            return pRet;
        }// Arrange
        private static List<IndivData> transpose(IEnumerable<IndivData> oInds)
        {
            List<IndivData> oRet = null;
            try
            {
                var src = oInds.ToArray();
                int nr = src.Length;
                if (nr > 0)
                {
                    int nv = (src[0]).DoubleData.Length;
                    if (nv > 0)
                    {
                        double[,] data = new double[nv, nr];
                        for (int i = 0; i < nr; ++i)
                        {
                            double[] dd = (src[i]).DoubleData;
                            for (int j = 0; j < nv; ++j)
                            {
                                data[j, i] = dd[j];
                            }// j
                        }// i
                        oRet = new List<IndivData>();
                        for (int i = 0; i < nv; ++i)
                        {
                            IndivData ind = new IndivData();
                            ind.Individu.IndivIndex = i;
                            double[] dd = new double[nr];
                            for (int j = 0; j < nr; ++j)
                            {
                                dd[j] = data[i, j];
                            }// j
                            ind.DoubleData = dd;
                            oRet.Add(ind);
                        }// i
                    }// nv
                }// nre
            }
            catch (Exception /* ex */)
            {
                oRet = null;
            }
            return oRet;
        }// transpose
        #endregion //Helpers
    }// class ArrangeSet
}
