using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using StatData;

namespace StatApp.ModelView
{
    public enum TreeLinkType
    {
        linkmean, linkmin, linkmax
    }
    public class TreeItem
    {
        #region Instance variables
        private TreeLinkType m_link = TreeLinkType.linkmean;
        private IndivData m_indiv = null;
        private List<TreeItem> m_children = null;
        private double[] m_center = null;
        private double[] m_distances = null;
        #endregion // Instance variables
        #region Constructors
        public TreeItem()
        {
        }
        public TreeItem(IEnumerable<IndivData> indivs, TreeLinkType t)
        {
            m_link = t;
            m_children = new List<TreeItem>();
            foreach (var ind in indivs)
            {
                TreeItem tt = new TreeItem();
                tt.LinkType = t;
                tt.Indiv = ind;
                m_children.Add(tt);
            }// ind
        }// TreeItem
        #endregion // Constructors
        #region Properties
        public double[] Distances
        {
            get
            {
                if (m_distances == null)
                {
                    var col = this.Children;
                    int n = col.Count;
                    m_distances = new double[n * n];
                }
                return m_distances;
            }
            set { }
        }// Distances
        public TreeLinkType LinkType
        {
            get
            {
                return m_link;
            }
            set
            {
                m_link = value;
            }
        }// LinkType
        public IndivData Indiv
        {
            get
            {
                return m_indiv;
            }
            set
            {
                m_indiv = value;
                if (m_indiv != null)
                {
                    m_center = (double[])m_indiv.DoubleData.Clone();
                }
            }
        }
        public List<TreeItem> Children
        {
            get
            {
                if (m_indiv == null)
                {
                    if (m_children == null)
                    {
                        m_children = new List<TreeItem>();
                    }
                }
                return m_children;
            }
        }// Children
        public bool IsLeaf
        {
            get
            {
                return (m_indiv != null) && (this.Children == null);
            }
            set { }
        }// IsLeaf
        public double[] Center
        {
            get
            {
                if (m_center == null)
                {
                    m_center = new double[0];
                }
                return m_center;
            }
            set { }
        }
        #endregion // Properties
        #region Methods
        public static Task<CategClusterSet> Hierar(OrdModelView model, int nbClusters, TreeLinkType t,CancellationToken cancellationToken)
        {
            return Task.Run<CategClusterSet>(() => {
                CategClusterSet pSet = null;
                try
                {
                    TreeItem oTree = new TreeItem(model.Individus, t);
                    oTree.update(cancellationToken);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return null;
                    }
                    var oList = oTree.Clusterize(nbClusters, cancellationToken);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return null;
                    }
                    if (oList == null)
                    {
                        return null;
                    }
                    pSet = new CategClusterSet();
                    List<Cluster> lc = new List<Cluster>();
                    int icur = 0;
                    foreach (var ll in oList)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return null;
                        }
                        Cluster c = new Cluster();
                        c.Index = icur;
                        c.Name = String.Format("HC{0}",icur+1);
                        c.Elements = ll;
                        foreach (var p in c.Elements)
                        {
                            p.HierarClusterIndex = icur;
                            p.HierarClusterString = c.Name;
                        }// p
                        c.UpdateCenter();
                        lc.Add(c);
                        ++icur;
                    }// ll
                    pSet.Clusters = lc;
                    pSet.Indivs = model.Individus;
                    pSet.Model = model;
                }
                catch (Exception /*ex */)
                {
                }
                return pSet;
            },cancellationToken);
        }// Hiera
        public List<List<IndivData> > Clusterize(int nbClusters, CancellationToken cancellationToken)
        {
            List<List<IndivData>> oRet = null;
            if (cancellationToken.IsCancellationRequested)
            {
                return oRet;
            }
            while (this.Children.Count > nbClusters)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return oRet;
                }
                if (!oneStep(cancellationToken))
                {
                    return oRet;
                }
            }// while
            oRet = new List<List<IndivData>>();
            foreach (var v in this.Children)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }
                List<IndivData> oList = new List<IndivData>();
                v.getLeaves(oList, cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }
                if (oList.Count < 1)
                {
                    continue;
                }
                oRet.Add(oList);
            }// v
            return oRet;
        }// Clusterize
        #endregion // Methods
        #region Helpers
        protected double computeDistance(TreeItem other,CancellationToken cancellationToken)
        {
            double dRet = 0.0;
            if (other != null)
            {
                double[] dd1 = this.Center;
                double[] dd2 = other.Center;
                int n = (dd1.Length < dd2.Length) ? dd1.Length : dd2.Length;
                for (int i = 0; i < n; ++i)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return dRet;
                    }
                    double x = dd1[i] - dd2[i];
                    dRet += x * x;
                }// i
            }// other
            return dRet;
        }// getDistance
        protected double getDistance(TreeItem other, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return 0.0;
            }
            double dRet = 0.0;
            if (other == null)
            {
                return dRet;
            }
            if (this.IsLeaf && other.IsLeaf)
            {
                return computeDistance(other,cancellationToken);
            }
            if (this.IsLeaf && (!other.IsLeaf))
            {
                var col = other.Children.ToArray();
                if (col.Length < 1)
                {
                    return dRet;
                }
                dRet = computeDistance(col[0],cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    return dRet;
                }
                switch (this.LinkType)
                {
                    case TreeLinkType.linkmax:
                        {
                            foreach (var t in col)
                            {
                                if (cancellationToken.IsCancellationRequested)
                                {
                                    return dRet;
                                }
                                double d = computeDistance(t,cancellationToken);
                                if (d > dRet)
                                {
                                    dRet = d;
                                }
                            }// t
                        }
                        break;
                    case TreeLinkType.linkmin:
                        {
                            foreach (var t in col)
                            {
                                if (cancellationToken.IsCancellationRequested)
                                {
                                    return dRet;
                                }
                                double d = computeDistance(t,cancellationToken);
                                if (d < dRet)
                                {
                                    dRet = d;
                                }
                            }// t
                        }
                        break;
                    case TreeLinkType.linkmean:
                        {
                            int nc = 0;
                            dRet = 0.0;
                            foreach (var t in col)
                            {
                                if (cancellationToken.IsCancellationRequested)
                                {
                                    return dRet;
                                }
                                ++nc;
                                double d = computeDistance(t,cancellationToken);
                                dRet += d;
                            }// t
                            if (nc > 0)
                            {
                                dRet = dRet / nc;
                            }
                        }
                        break;
                }// type
            }
            else if ((!this.IsLeaf) && other.IsLeaf)
            {
                return other.getDistance(this,cancellationToken);
            }
            else
            {
                var col1 = this.Children.ToArray();
                var col2 = other.Children.ToArray();
                int n1 = col1.Length;
                int n2 = col2.Length;
                dRet = 0.0;
                switch (this.LinkType)
                {
                    case TreeLinkType.linkmax:
                        {
                            for (int i = 0; i < n1; i++)
                            {
                                if (cancellationToken.IsCancellationRequested)
                                {
                                    return dRet;
                                }
                                var t1 = col1[i];
                                for (int j = 0; j < n2; ++j)
                                {
                                    if (cancellationToken.IsCancellationRequested)
                                    {
                                        return dRet;
                                    }
                                    double d = t1.computeDistance(col2[j],cancellationToken);
                                    if ((i == 0) && (j == 0))
                                    {
                                        dRet = d;
                                    }
                                    else if (d > dRet)
                                    {
                                        dRet = d;
                                    }
                                }// j
                            }// i
                        }
                        break;
                    case TreeLinkType.linkmin:
                        {
                            for (int i = 0; i < n1; i++)
                            {
                                if (cancellationToken.IsCancellationRequested)
                                {
                                    return dRet;
                                }
                                var t1 = col1[i];
                                for (int j = 0; j < n2; ++j)
                                {
                                    if (cancellationToken.IsCancellationRequested)
                                    {
                                        return dRet;
                                    }
                                    double d = t1.computeDistance(col2[j],cancellationToken);
                                    if ((i == 0) && (j == 0))
                                    {
                                        dRet = d;
                                    }
                                    else if (d < dRet)
                                    {
                                        dRet = d;
                                    }
                                }// j
                            }// i
                        }
                        break;
                    case TreeLinkType.linkmean:
                        {
                            int nc = 0;
                            for (int i = 0; i < n1; i++)
                            {
                                if (cancellationToken.IsCancellationRequested)
                                {
                                    return dRet;
                                }
                                var t1 = col1[i];
                                for (int j = 0; j < n2; ++j)
                                {
                                    double d = t1.computeDistance(col2[j],cancellationToken);
                                    dRet += d;
                                    ++nc;
                                }// j
                            }// i
                            if (nc > 0)
                            {
                                dRet = dRet / nc;
                            }
                        }
                        break;
                }// type
            }
            return dRet;
        }// getDistance
        protected void updateDistances(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            if (this.IsLeaf)
            {
                return;
            }
            var col = this.Children.ToArray();
            int n = col.Length;
            m_distances = new double[n * n];
            for (int i = 0; i < n; ++i)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                int ii = i * n + i;
                m_distances[ii] = 0.0;
                var t = col[i];
                for (int j = 0; j < i; ++j)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                    double d = t.getDistance(col[j],cancellationToken);
                    int i1 = i * n + j;
                    m_distances[i1] = d;
                    int i2 = j * n + i;
                    m_distances[i2] = d;
                }// j
            }// i
        }// updateDistances
        protected void updateCenter(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            if (this.IsLeaf)
            {
                return;
            }
            var col = this.Children.ToArray();
            int n = getNbVars(cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            m_center = new double[n];
            for (int i = 0; i < n; ++i)
            {
                m_center[i] = 0.0;
            }
            int nc = col.Length;
            if (nc < 1)
            {
                return;
            }
            for (int i = 0; i < nc; ++i)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                var t = col[i];
                double[] xx = t.Center;
                for (int j = 0; j < n; ++j)
                {
                    m_center[j] = m_center[j] + xx[j];
                }
            }// i
            for (int i = 0; i < n; ++i)
            {
                m_center[i] = m_center[i] / nc;
            }
        }// updateCenter
        protected int getNbVars(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return 0;
            }
            int nRet = 0;
            if (this.IsLeaf)
            {
                nRet = this.Center.Length;
            }
            else
            {
                foreach (var v in Children)
                {
                    if (v.IsLeaf)
                    {
                        nRet = v.Center.Length;
                        break;
                    }
                    else
                    {
                        nRet = v.getNbVars(cancellationToken);
                        break;
                    }
                }// v
            }
            return nRet;
        }// getNbVars
        protected void update(CancellationToken cancellationToken)
        {
            if (!this.IsLeaf)
            {
                updateCenter(cancellationToken);
                updateDistances(cancellationToken);
            }
        }// update
        protected List<TreeItem> getNearest(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }
            if (this.IsLeaf)
            {
                return null;
            }
            List<TreeItem> oRet = new List<TreeItem>();
            double dMin = 0;
            var col = this.Children.ToArray();
            int n = col.Length;
            for (int i = 0; i < n; ++i)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }
                var ti = col[i];
                for (int j = 0; j < i; ++j)
                {
                    var tj = col[j];
                    double d = m_distances[i * n + j];
                    if (oRet.Count < 1)
                    {
                        oRet.Add(ti);
                        oRet.Add(tj);
                        dMin = d;
                    }
                    else if (d < dMin)
                    {
                        oRet.Clear();
                        oRet.Add(ti);
                        oRet.Add(tj);
                        dMin = d;
                    }
                    else if (d == dMin)
                    {
                        var q = from x in oRet where x == ti select x;
                        if (q.Count() < 0)
                        {
                            oRet.Add(ti);
                        }
                        var qx = from x in oRet where x == tj select x;
                        if (qx.Count() < 0)
                        {
                            oRet.Add(tj);
                        }
                    }
                }// j
            }// i
            return oRet;
        }// getNearest
        protected bool oneStep(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }
            List<TreeItem> oList = getNearest(cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }
            if (oList == null)
            {
                return false;
            }
            if (oList.Count < 1)
            {
                return false;
            }
            foreach (var cur in oList)
            {
                this.Children.Remove(cur);
            }// cur
            if (this.Children.Count < 1)
            {
                this.m_children = oList;
            }
            else
            {
                TreeItem pNew = new TreeItem();
                pNew.m_link = this.LinkType;
                pNew.m_children = oList;
                pNew.update(cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }
                this.Children.Add(pNew);
            }
            this.update(cancellationToken);
            return true;
        }// oneStep
        protected void getLeaves(List<IndivData> oList, CancellationToken cancelletionToken)
        {
            if (cancelletionToken.IsCancellationRequested)
            {
                return;
            }
            if (this.IsLeaf)
            {
                IndivData p = this.Indiv;
                if (p != null)
                {
                    oList.Add(p);
                }
                return;
            }
            foreach (var v in this.Children)
            {
                if (cancelletionToken.IsCancellationRequested)
                {
                    return;
                }
                v.getLeaves(oList,cancelletionToken);
            }
        }// getLeaves
        #endregion
    }// class TreeItem
}
