using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StatData;
namespace StatApp.ModelView
{
    public enum ClassificationType
    {
        Utility, KMeans, Hierar
    }
    public class Cluster : Observable, ICloneable
    {
        private int m_index = -1;
        private List<IndivData> m_elements;
        private double[] m_center;
        private String m_name;
        private double m_variance;
        public Cluster()
        {
        }
        public Cluster(IndivData p, ClassificationType t)
        {
            switch (t)
            {
                case ClassificationType.Hierar:
                    AddHierar(p);
                    break;
                case ClassificationType.KMeans:
                    AddKMeans(p);
                    break;
                case ClassificationType.Utility:
                    AddUtility(p);
                    break;
                default:
                    break;
            }
        }
        public int Index
        {
            get
            {
                return m_index;
            }
            set
            {
                if (value != m_index)
                {
                    m_index = value;
                    NotifyPropertyChanged("Index");
                }
            }
        }// Index
        public bool IsEmpty
        {
            get
            {
                return (this.Elements.Count < 1);
            }
            set { }
        }// IsEmpty
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
            set
            {
                if (value != m_center)
                {
                    m_center = value;
                    NotifyPropertyChanged("Center");
                }
            }
        }// Center
        public String Name
        {
            get
            {
                return String.IsNullOrEmpty(m_name) ? String.Empty : m_name.Trim();
            }
            set
            {
                if (value != m_name)
                {
                    m_name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }// Name
        public int Count
        {
            get
            {
                return this.Elements.Count;
            }
            set
            {
            }
        }// Count
        public double Variance
        {
            get
            {
                return m_variance;
            }
            set { }
        }// Variance
        public List<IndivData> Elements
        {
            get
            {
                if (m_elements == null)
                {
                    m_elements = new List<IndivData>();
                }
                return m_elements;
            }
            set
            {
                if (value != m_elements)
                {
                    if (value != null)
                    {
                        m_elements = value;
                        if (m_elements.Count > 1)
                        {
                            m_elements.Sort();
                        }
                    }
                    else
                    {
                        m_elements = null;
                    }
                    NotifyPropertyChanged("Elements");
                    NotifyPropertyChanged("Count");
                    NotifyPropertyChanged("IsEmpty");
                }
            }
        }
        #region Methods
        public bool AddUtility(IndivData p)
        {
            if (p == null)
            {
                return false;
            }
            if (!p.IsValid)
            {
                return false;
            }
            var q = from x in this.Elements where x.IndivIndex == p.IndivIndex select x;
            if (q.Count() > 0)
            {
                return true;
            }
            p.UtilityClusterIndex = this.Index;
            this.Elements.Add(p);
            if (this.Elements.Count > 1)
            {
                this.Elements.Sort();
            }
            this.NotifyPropertyChanged("Elements");
            NotifyPropertyChanged("Count");
            NotifyPropertyChanged("IsEmpty");
            return true;
        }// Add
        public bool AddKMeans(IndivData p)
        {
            if (p == null)
            {
                return false;
            }
            if (!p.IsValid)
            {
                return false;
            }
            var q = from x in this.Elements where x.IndivIndex == p.IndivIndex select x;
            if (q.Count() > 0)
            {
                return true;
            }
            p.KMeansClusterIndex = this.Index;
            this.Elements.Add(p);
            if (this.Elements.Count > 1)
            {
                this.Elements.Sort();
            }
            this.NotifyPropertyChanged("Elements");
            NotifyPropertyChanged("Count");
            NotifyPropertyChanged("IsEmpty");
            return true;
        }// Add
        public bool AddHierar(IndivData p)
        {
            if (p == null)
            {
                return false;
            }
            if (!p.IsValid)
            {
                return false;
            }
            var q = from x in this.Elements where x.IndivIndex == p.IndivIndex select x;
            if (q.Count() > 0)
            {
                return true;
            }
            p.HierarClusterIndex = this.Index;
            this.Elements.Add(p);
            if (this.Elements.Count > 1)
            {
                this.Elements.Sort();
            }
            this.NotifyPropertyChanged("Elements");
            NotifyPropertyChanged("Count");
            NotifyPropertyChanged("IsEmpty");
            return true;
        }// Add
        public bool RemoveUtility(IndivData p)
        {
            if (p == null)
            {
                return false;
            }
            if (!p.IsValid)
            {
                return false;
            }
            var q = from x in this.Elements where x.IndivIndex == p.IndivIndex select x;
            if (q.Count() < 1)
            {
                return false;
            }
            var xx = q.First();
            xx.UtilityClusterIndex = -1;
            this.Elements.Remove(xx);
            this.NotifyPropertyChanged("Elements");
            NotifyPropertyChanged("Count");
            NotifyPropertyChanged("IsEmpty");
            return true;
        }// Add
        public bool RemoveKMeans(IndivData p)
        {
            if (p == null)
            {
                return false;
            }
            if (!p.IsValid)
            {
                return false;
            }
            var q = from x in this.Elements where x.IndivIndex == p.IndivIndex select x;
            if (q.Count() < 1)
            {
                return false;
            }
            var xx = q.First();
            xx.KMeansClusterIndex = -1;
            this.Elements.Remove(xx);
            this.NotifyPropertyChanged("Elements");
            NotifyPropertyChanged("Count");
            NotifyPropertyChanged("IsEmpty");
            return true;
        }// Add
        public bool RemoveKHierar(IndivData p)
        {
            if (p == null)
            {
                return false;
            }
            if (!p.IsValid)
            {
                return false;
            }
            var q = from x in this.Elements where x.IndivIndex == p.IndivIndex select x;
            if (q.Count() < 1)
            {
                return false;
            }
            var xx = q.First();
            xx.HierarClusterIndex = -1;
            this.Elements.Remove(xx);
            this.NotifyPropertyChanged("Elements");
            NotifyPropertyChanged("Count");
            NotifyPropertyChanged("IsEmpty");
            return true;
        }// Add
        public void Clear()
        {
            this.Elements = null;
        }// Clear
        public void UpdateCenter()
        {
            var col = this.Elements;
            int nc = col.Count;
            if (nc < 1)
            {
                this.Center = null;
                return;
            }
            int n = col.First().DoubleData.Length;
            if (n < 1)
            {
                this.Center = null;
                return;
            }
            double[] sums = new double[n];
            for (int i = 0; i < n; ++i)
            {
                sums[i] = 0.0;
            }
            foreach (var ind in col)
            {
                double[] d = ind.DoubleData;
                if (d.Length < n)
                {
                    continue;
                }
                for (int i = 0; i < n; ++i)
                {
                    sums[i] = sums[i] + d[i];
                }
            }// ind
            for (int i = 0; i < n; ++i)
            {
                sums[i] = sums[i] / (double)nc;
            }
            this.Center = sums;
            NotifyPropertyChanged("Center");
            double svar = 0.0;
            foreach (var ind in col)
            {
                double x = this.GetDistance(ind);
                svar += x;
            }// ind
            m_variance = svar / nc;
            NotifyPropertyChanged("Variance");
        }// UpdateCenter
        public double GetDistance(IndivData pIndivData)
        {
            double dRet = 0.0;
            if (pIndivData == null)
            {
                return dRet;
            }
            double[] dd1 = this.Center;
            double[] dd2 = pIndivData.DoubleData;
            if ((dd1 == null) || (dd2 == null))
            {
                return dRet;
            }
            int n = (dd1.Length < dd2.Length) ? dd1.Length : dd2.Length;
            for (int i = 0; i < n; ++i)
            {
                double x = dd1[i] - dd2[i];
                dRet += x * x;
            }// i
            return dRet;
        }// GetDistance
        #endregion // Methods
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("{");
            sb.Append(this.Index);
            sb.Append(",[");
            int i = 0;
            foreach (var ind in this.Elements)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }
                sb.Append(ind.ToString());
                ++i;
            }
            sb.Append("] }");
            return sb.ToString();
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is Cluster))
            {
                return false;
            }
            Cluster p = obj as Cluster;
            var oList1 = this.Elements;
            var oList2 = p.Elements;
            if (oList1.Count() != oList2.Count())
            {
                return false;
            }
            foreach (var ind in oList1)
            {
                if (!oList2.Contains(ind))
                {
                    return false;
                }
            }
            return true;
        }// Equals
        public override int GetHashCode()
        {
            return this.Index.GetHashCode();
        }
        public object Clone()
        {
            Cluster pRet = new Cluster();
            pRet.m_index = this.m_index;
            pRet.m_name = this.m_name;
            pRet.m_center = (double[])this.Center.Clone();
            if (this.m_elements != null)
            {
                pRet.m_elements = new List<IndivData>();
                foreach (var p in this.m_elements)
                {
                    pRet.m_elements.Add((IndivData)p.Clone());
                }
            }
            return pRet;
        }
    }// class Cluster
}
