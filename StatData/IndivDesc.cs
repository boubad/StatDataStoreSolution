using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Collections.ObjectModel;

namespace StatData
{
    [Serializable]
    public class IndivDesc : Observable, IComparable
    {
        #region Instance Variables
        private int m_index = -1;
        private String m_idstring = null;
        private String m_name = null;
        private ValueDescs m_values = null;
        private int m_photoid = 0;
        private byte[] m_photodata = null;
        private String m_photoname;
        #endregion // InstanceVariable
        #region Constructors
        public IndivDesc()
        {
        }// IndivDesc
        #endregion // Constructors
        #region Properties
        public bool IsValid
        {
            get
            {
                return (this.IndivIndex >= 0);
            }
            set { }
        }// IsValid
        public Object[] ValuesObjects
        {
            get
            {
                return GetValuesObjects();
            }
            set { }
        }// ValuesObjects
        public int IndivIndex
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
                    NotifyPropertyChanged("IndivIndex");
                }
            }
        }// IndivIndex
        public String IdString
        {
            get
            {
                return String.IsNullOrEmpty(m_idstring) ? String.Empty : m_idstring.Trim();
            }
            set
            {
                String old = String.IsNullOrEmpty(m_idstring) ? String.Empty : m_idstring.Trim();
                String s = String.IsNullOrEmpty(value) ? String.Empty : value.Trim();
                if (s != old)
                {
                    m_idstring = s;
                    NotifyPropertyChanged("IdString");
                }
            }
        }// IdString
        public String Name
        {
            get
            {
                return String.IsNullOrEmpty(m_name) ? String.Empty : m_name.Trim();
            }
            set
            {
                String old = String.IsNullOrEmpty(m_name) ? String.Empty : m_name.Trim();
                String s = String.IsNullOrEmpty(value) ? String.Empty : value.Trim();
                if (s != old)
                {
                    m_name = s;
                    NotifyPropertyChanged("Name");
                }
            }
        }// Name
        public String PhotoName
        {
            get
            {
                return String.IsNullOrEmpty(m_photoname) ? String.Empty : m_photoname.Trim();
            }
            set
            {
                String old = String.IsNullOrEmpty(m_photoname) ? String.Empty : m_photoname.Trim();
                String s = String.IsNullOrEmpty(value) ? String.Empty : value.Trim();
                if (s != old)
                {
                    m_photoname = s;
                    NotifyPropertyChanged("PhotoName");
                }
            }
        }// PhotoName
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
                    if (value != null)
                    {
                        if (value.Count > 1)
                        {
                            List<ValueDesc> oList = value.ToList();
                            oList.Sort();
                            m_values = new ValueDescs(oList);
                        }
                        else
                        {
                            m_values = value;
                        }
                    }
                    else
                    {
                        m_values = null;
                    }
                    NotifyPropertyChanged("Values");
                    NotifyPropertyChanged("ValuesObjects");
                }// 
            }
        }// Values
        public int PhotoId
        {
            get
            {
                return m_photoid;
            }
            set
            {
                if (value != m_photoid)
                {
                    m_photoid = value;
                    NotifyPropertyChanged("PhotoId");
                }
            }
        }// PhotoId
        public byte[] PhotoData
        {
            get
            {
                return m_photodata;
            }
            set
            {
                if (value != m_photodata)
                {
                    m_photodata = value;
                    NotifyPropertyChanged("PhotoData");
                    NotifyPropertyChanged("HasPhoto");
                }
            }
        }// PhotoData
        public bool HasPhoto
        {
            get
            {
                return ((m_photodata != null) && (m_photodata.Length > 0));
            }
            set
            {
                NotifyPropertyChanged("HasPhoto");
            }
        }// HasPhoto
        #endregion // Properties
        #region Methods
        public Object[] GetValuesObjects()
        {
            var col = this.Values.ToArray();
            int n = col.Length;
            Object[] pRet = new Object[n];
            for (int i = 0; i < n; ++i)
            {
                pRet[i] = null;
                ValueDesc v = col[i];
                if (v != null)
                {
                    String s = StatHelpers.ConvertValue(v.DataStringValue);
                    if (!String.IsNullOrEmpty(s))
                    {
                        String stype = v.VariableType;
                        if (stype == "double")
                        {
                            pRet[i] = v.DoubleValue;
                        }
                        else if (stype == "float")
                        {
                            pRet[i] = (float)v.DoubleValue;
                        }
                        else if (stype == "int")
                        {
                            pRet[i] = v.IntValue;
                        }
                        else if (stype == "short")
                        {
                            pRet[i] = (short)v.IntValue;
                        }
                        else if (stype == "string")
                        {
                            pRet[i] = v.StringValue;
                        }
                        else if (stype == "bool")
                        {
                            s = s.ToLower();
                            if (s.StartsWith("f") || s.StartsWith("n"))
                            {
                                pRet[i] = false;
                            }
                            else
                            {
                                pRet[i] = true;
                            }
                        }
                    }// not null
                }// v
            }// i
            return pRet;
        }// GetValueObjects
        #endregion // Methods
        #region overrides
        public override string ToString()
        {
            if (!String.IsNullOrEmpty(this.Name))
            {
                return this.Name;
            }
            else if (!String.IsNullOrEmpty(this.IdString))
            {
                return this.IdString;
            }
            else if (this.IndivIndex >= 0)
            {
                return String.Format("Indiv{0}", this.IndivIndex);
            }
            else
            {
                return "{}";
            }
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is IndivDesc))
            {
                return false;
            }
            IndivDesc p = obj as IndivDesc;
            return (this.IndivIndex == p.IndivIndex);
        }
        public override int GetHashCode()
        {
            return this.IndivIndex.GetHashCode();
        }
        #endregion // overrides
        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return -1;
            }
            if (!(obj is IndivDesc))
            {
                return -1;
            }
            IndivDesc p = obj as IndivDesc;
            String s1 = this.Name;
            String s2 = p.Name;
            if ((!String.IsNullOrEmpty(s1)) && (!String.IsNullOrEmpty(s2)))
            {
                return s1.ToLower().CompareTo(s2.ToLower());
            }
            if (this.IndivIndex < p.IndivIndex)
            {
                return -1;
            }
            else if (this.IndivIndex > p.IndivIndex)
            {
                return 1;
            }
            return 0;
        }
    }// IndivDesc
    [Serializable]
    public class IndivData : Observable,IComparable,ICloneable
    {
        private IndivDesc m_indiv;
        private double[] m_doubledata;
        private int[] m_intdata;
        private int m_utilityclusterindex = -1;
        private int m_kmeansclusterindex = -1;
        private int m_hierarclusterindex = -1;
        private int m_categindex = -1;
        private int m_position = -1;
        private int m_globalclusterindex = -1;
        private double m_x;
        private double m_y;
        private double m_z;
        #region Constructors
        public IndivData()
        {
        }
        public IndivData(IndivDesc ind)
        {
            m_indiv = ind;
        }
        public IndivData(IndivDesc indiv, double[] doubleval, int[] intval)
        {
            m_indiv = indiv;
            m_doubledata = doubleval;
            m_intdata = intval;
        }
        #endregion // Constructors
        #region Properties
        public bool IsValid
        {
            get
            {
                return this.Individu.IsValid;
            }
            set { }
        }// IsValid
        public IndivDesc Individu
        {
            get
            {
                if (m_indiv == null)
                {
                    m_indiv = new IndivDesc();
                }
                return m_indiv;
            }
            set
            {
                if (value != m_indiv)
                {
                    m_indiv = value;
                    NotifyPropertyChanged("Individu");
                }
            }
        }
        public double[] DoubleData
        {
            get
            {
                if (m_doubledata == null)
                {
                    m_doubledata = new double[0];
                }
                return m_doubledata;
            }
            set
            {
                if (value != m_doubledata)
                {
                    m_doubledata = value;
                    NotifyPropertyChanged("DoubleData");
                }
            }
        }// DoubleData
        public int[] IntData
        {
            get
            {
                if (m_intdata == null)
                {
                    m_intdata = new int[0];
                }
                return m_intdata;
            }
            set
            {
                if (value != m_intdata)
                {
                    m_intdata = value;
                    NotifyPropertyChanged("IntData");
                }
            }
        }// IntData
        public ValueDescs Values
        {
            get
            {
                return this.Individu.Values;
            }
            set {
                this.Individu.Values = value;
                NotifyPropertyChanged("Values");
            }
        }// Values
        public String CategString { get; set; }
        public String UtilityClusterString { get; set; }
        public String KMeansClusterString { get; set; }
        public String HierarClusterString { get; set; }
        public double X
        {
            get
            {
                return m_x;
            }
            set
            {
                if (value != m_x)
                {
                    m_x = value;
                    NotifyPropertyChanged("X");
                }
            }
        }// X
        public double Y
        {
            get
            {
                return m_y;
            }
            set
            {
                if (value != m_y)
                {
                    m_y = value;
                    NotifyPropertyChanged("Y");
                }
            }
        }// Y
        public double Z
        {
            get
            {
                return m_z;
            }
            set
            {
                if (value != m_z)
                {
                    m_z = value;
                    NotifyPropertyChanged("Z");
                }
            }
        }// Z
        public int Position
        {
            get
            {
                return m_position;
            }
            set
            {
                if (m_position != value)
                {
                    m_position = value;
                    NotifyChange("Position");
                }
            }
        }//Position
        public int UtilityClusterIndex
        {
            get
            {
                return m_utilityclusterindex;
            }
            set
            {
                if (value != m_utilityclusterindex)
                {
                    m_utilityclusterindex = value;
                    NotifyPropertyChanged("UtilityClusterIndex");
                    if (value < 0)
                    {
                        this.GlobalClusterIndex = -1;
                    }
                }
            }
        }// UtilityClusterIndex
        public int KMeansClusterIndex
        {
            get
            {
                return m_kmeansclusterindex;
            }
            set
            {
                if (value != m_kmeansclusterindex)
                {
                    m_kmeansclusterindex = value;
                    NotifyPropertyChanged("KMeansClusterIndex");
                    if (value < 0)
                    {
                        this.GlobalClusterIndex = -1;
                    }
                }
            }
        }// KMeansClusterIndex
        public int HierarClusterIndex
        {
            get
            {
                return m_hierarclusterindex;
            }
            set
            {
                if (value != m_hierarclusterindex)
                {
                    m_hierarclusterindex = value;
                    NotifyPropertyChanged("HierarClusterIndex");
                    if (value < 0)
                    {
                        this.GlobalClusterIndex = -1;
                    }
                }
            }
        }// HierarClusterIndex
        public int GlobalClusterIndex
        {
            get
            {
                return m_globalclusterindex;
            }
            set
            {
                if (value != m_globalclusterindex)
                {
                    m_globalclusterindex = value;
                    NotifyPropertyChanged("GlobalClusterIndex");
                }
            }
        }// GlobalClusterIndex
        public int CategIndex
        {
            get
            {
                return m_categindex;
            }
            set
            {
                if (value != m_categindex)
                {
                    m_categindex = value;
                    NotifyPropertyChanged("CategIndex");
                }
            }
        }// CategIndex
        public int IndivIndex
        {
            get
            {
                return this.Individu.IndivIndex;
            }
            set
            {
                this.Individu.IndivIndex = value;
                NotifyPropertyChanged("IndivIndex");
            }
        }// IndivIndex
        public String IdString
        {
            get
            {
                return this.Individu.IdString;
            }
            set
            {
                this.Individu.IdString = value;
                NotifyPropertyChanged("IdString");
            }
        }// IdString
        public String Name
        {
            get
            {
                return this.Individu.Name;
            }
            set
            {
                this.Individu.Name = value;
                NotifyPropertyChanged("Name");
            }
        }// Name
        public int PhotoId
        {
            get
            {
                return this.Individu.PhotoId;
            }
            set
            {
                this.Individu.PhotoId = value;
                NotifyPropertyChanged("PhotoId");
            }
        }// PhotoId
        public byte[] PhotoData
        {
            get
            {
                return this.Individu.PhotoData;
            }
            set
            {
                this.Individu.PhotoData = value;
                NotifyPropertyChanged("PhotoData");
            }
        }// PhotoData
        public bool HasPhoto
        {
            get
            {
                return this.Individu.HasPhoto;
            }
            set
            {
                NotifyPropertyChanged("HasPhoto");
            }
        }// HasPhoto
        public String PhotoName
        {
            get
            {
                return this.Individu.PhotoName;
            }
            set
            {
                this.Individu.PhotoName = value;
                NotifyPropertyChanged("PhotoName");
            }
        }// PhotoName
        #endregion // Properties
        #region Overrides
        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return -1;
            }
            if (!(obj is IndivData))
            {
                return -1;
            }
            IndivData p = obj as IndivData;
            if (this.CategIndex < p.CategIndex)
            {
                return -1;
            }
            else if (this.CategIndex > p.CategIndex)
            {
                return 1;
            }
            IndivDesc p1 = this.Individu;
            IndivDesc p2 = p.Individu;
            if ((p1 == null) || (p2 == null)){
                return 0;
            }
            String s1 = p1.Name;
            String s2 = p2.Name;
            if ((!String.IsNullOrEmpty(s1)) && (!String.IsNullOrEmpty(s2)))
            {
                return s1.ToLower().CompareTo(s2.ToLower());
            }
            if (p1.IndivIndex < p2.IndivIndex)
            {
                return -1;
            }
            else if (p1.IndivIndex > p2.IndivIndex)
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
            if (!(obj is IndivData))
            {
                return false;
            }
            IndivData p = obj as IndivData;
            IndivDesc p1 = this.Individu;
            IndivDesc p2 = p.Individu;
            if ((p1 == null) && (p2 == null))
            {
                return true;
            }
            else if ((p1 != null) && (p2 != null))
            {
                return p1.Equals(p2);
            }
            return false;
        }
        public override int GetHashCode()
        {
            IndivDesc p = this.Individu;
            if (p != null)
            {
                return p.GetHashCode();
            }
            else
            {
                return this.CategIndex.GetHashCode();
            }
        }
        public override string ToString()
        {
            IndivDesc p = this.Individu;
            if (p != null)
            {
                return p.ToString();
            }
            else
            {
                return "{}";
            }
        }
        #endregion // Overides
        public double ComputeDistance(IndivData other)
        {
            double dRet = 0.0;
            if (other == null)
            {
                return dRet;
            }
            double[] dd1 = this.DoubleData;
            double[] dd2 = other.DoubleData;
            int n = (dd1.Length < dd2.Length) ? dd1.Length : dd2.Length;
            for (int i = 0; i < n; ++i)
            {
                double x = dd1[i] - dd2[i];
                dRet += x * x;
            }// i
            return dRet;
        }// ComputeDistance
        public object Clone()
        {
            IndivData p = new IndivData();
            p.m_indiv = this.m_indiv;
            p.m_doubledata = this.m_doubledata;
            p.m_intdata = this.m_intdata;
            p.m_categindex = this.m_categindex;
            p.m_utilityclusterindex = this.m_utilityclusterindex;
            p.m_kmeansclusterindex = this.m_kmeansclusterindex;
            p.m_hierarclusterindex = this.m_hierarclusterindex;
            p.m_position = this.m_position;
            p.m_categindex = this.CategIndex;
            p.UtilityClusterString = this.UtilityClusterString;
            p.KMeansClusterString = this.KMeansClusterString;
            p.HierarClusterString = this.HierarClusterString;
            p.m_x = this.m_x;
            p.m_y = this.m_y;
            p.m_z = this.m_z;
            return p;
        }
    }//class IndivData
    [Serializable]
    public class IndivDatas : ObservableCollection<IndivData>,ICloneable
    {
        public IndivDatas()
            : base()
        {
        }
        public IndivDatas(IEnumerable<IndivData> col)
            : base(col)
        {
        }
        public void ClearUtilityClusterIndexes()
        {
            foreach (var p in this)
            {
                p.UtilityClusterIndex= -1;
                p.UtilityClusterString = String.Empty;
                
            }// p
        }
        public void ClearKMeansClusterIndexes()
        {
            foreach (var p in this)
            {
                p.KMeansClusterIndex = -1;
                p.KMeansClusterString = String.Empty;

            }// p
        }
        public void ClearHierarClusterIndexes()
        {
            foreach (var p in this)
            {
                p.HierarClusterIndex = -1;
                p.HierarClusterString = String.Empty;

            }// p
        }
        public void ClearCategIndexes()
        {
            foreach (var p in this)
            {
                p.CategIndex = -1;
                p.CategString = String.Empty;
            }// p
        }
        public object Clone()
        {
            IndivDatas pRet = new IndivDatas();
            foreach (var p in this)
            {
                var pp = (IndivData)p.Clone();
                pRet.Add(pp);
            }// p
            return pRet;
        }
    }
    [Serializable]
    public class IndivDescs : ObservableCollection<IndivDesc>
    {
        public IndivDescs()
            : base()
        {
        }
        public IndivDescs(IEnumerable<IndivDesc> col):base(col)
        {
        }
       
    }
}
