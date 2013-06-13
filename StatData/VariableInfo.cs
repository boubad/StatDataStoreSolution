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
    public class CategValueDesc : ARootElement
    {
        public CategValueDesc()
        {
        }
        public String StringValue { get; set; }
        public int Count { get; set; }
        public double DoubleValue { get; set; }
        public int VariableId { get; set; }
    }//CategValueDesc
    [Serializable]
    public class CategValueDescs : ObservableCollection<CategValueDesc>
    {
        public CategValueDescs()
            : base()
        {
        }
        public CategValueDescs(IEnumerable<CategValueDesc> col)
            : base(col)
        {
        }
    }// class CategValueDescs
    [Serializable]
    public class VariableInfo : ARootElement
    {
        #region Instance variables
        private int m_datasetid = 0;
        private String m_datasetname;
        private int m_varid = 0;
        private String m_varname;
        private String m_datatype;
        private bool m_iscateg = false;
        private int m_ntotal = 0;
        private int m_nmissing = 0;
        private List<String> m_values;
        private double m_min = 0.0;
        private double m_max = 0.0;
        private double m_mean = 0.0;
        private double m_median = 0.0;
        private double m_std = 0.0;
        private double m_skew = 0.0;
        private double m_flat = 0.0;
        private CategValueDescs m_categvalues;
        #endregion // InstanceVariable
        #region Constructors
        public VariableInfo()
        {
        }
        #endregion // Constructors
        #region Properties
        public double Quantile05 { get; set; }
        public double Quantile10 { get; set; }
        public double Quantile25 { get; set; }
        public double Quantile75 { get; set; }
        public double Quantile90 { get; set; }
        public double Quantile95 { get; set; }
        public CategValueDescs CategValues
        {
            get
            {
                if (m_categvalues == null)
                {
                    m_categvalues = new CategValueDescs();
                }
                return m_categvalues;
            }
            set
            {
                if (value != m_categvalues)
                {
                    m_categvalues = value;
                    NotifyPropertyChanged("CategValues");
                }
            }
        }// CategValues
        public double Flatness
        {
            get
            {
                return m_flat;
            }
            set
            {
                if (value != m_flat)
                {
                    m_flat = value;
                    NotifyPropertyChanged("Flatness");
                    this.IsModified = true;
                }
            }
        }// Flatness
        public double Skewness
        {
            get
            {
                return m_skew;
            }
            set
            {
                if (value != m_skew)
                {
                    m_skew = value;
                    NotifyPropertyChanged("Skewness");
                    this.IsModified = true;
                }
            }
        }// Skewness
        public double Deviation
        {
            get
            {
                return m_std;
            }
            set
            {
                if (value != m_std)
                {
                    m_std = value;
                    NotifyPropertyChanged("Deviation");
                    this.IsModified = true;
                }
            }
        }// Deviation
        public double Median
        {
            get
            {
                return m_median;
            }
            set
            {
                if (value != m_median)
                {
                    m_median = value;
                    NotifyPropertyChanged("Median");
                    this.IsModified = true;
                }
            }
        }// Median
        public String FlatnessString
        {
            get
            {
                return (this.IsCategVar) ? String.Empty : String.Format("{0}", this.Flatness);
            }
            set { }
        }
        public String SkewnessString
        {
            get
            {
                return (this.IsCategVar) ? String.Empty : String.Format("{0}", this.Skewness);
            }
            set { }
        }
        public String MedianString
        {
            get
            {
                return (this.IsCategVar) ? String.Empty : String.Format("{0}", this.Median);
            }
            set { }
        }
        public String DeviationString
        {
            get
            {
                return (this.IsCategVar) ? String.Empty : String.Format("{0}", this.Deviation);
            }
            set { }
        }
        public String MinString
        {
            get
            {
                return (this.IsCategVar) ? String.Empty : String.Format("{0}", this.MinValue);
            }
            set { }
        }
        public String MaxString
        {
            get
            {
                return (this.IsCategVar) ? String.Empty : String.Format("{0}", this.MaxValue);
            }
            set { }
        }
        public String MeanString
        {
            get
            {
                return (this.IsCategVar) ? String.Empty : String.Format("{0}", this.MeanValue);
            }
            set { }
        }
        public String TotalString
        {
            get
            {
                return String.Format("{0}", this.TotalValuesCount);
            }
            set { }
        }
        public String MissingString
        {
            get
            {
                return String.Format("{0}", this.MissingValuesCount);
            }
            set { }
        }
        public double MinValue
        {
            get
            {
                return m_min;
            }
            set
            {
                if (value != m_min)
                {
                    m_min = value;
                    NotifyPropertyChanged("MinValue");
                    this.IsModified = true;
                }
            }
        }// MinValue
        public double MeanValue
        {
            get
            {
                return m_mean;
            }
            set
            {
                if (value != m_mean)
                {
                    m_mean = value;
                    NotifyPropertyChanged("MeanValue");
                    this.IsModified = true;
                }
            }
        }// MeanValue
        public double MaxValue
        {
            get
            {
                return m_max;
            }
            set
            {
                if (value != m_max)
                {
                    m_max = value;
                    NotifyPropertyChanged("MaxValue");
                    this.IsModified = true;
                }
            }
        }// MaxValue
        public List<String> DistinctValues
        {
            get
            {
                if (m_values == null)
                {
                    m_values = new List<string>();
                }
                return m_values;
            }
            set
            {
                if (value != m_values)
                {
                    m_values = value;
                    NotifyPropertyChanged("DistinctValues");
                    this.IsModified = true;
                }
            }
        }// DistanctValues
        public ObservableCollection<String> Values
        {
            get
            {
                return new ObservableCollection<string>(this.DistinctValues);
            }
            set
            {
                NotifyPropertyChanged("Values");
            }
        }
        public int TotalValuesCount
        {
            get
            {
                return m_ntotal;
            }
            set
            {
                if (value != m_ntotal)
                {
                    m_ntotal = value;
                    NotifyPropertyChanged("TotalValuesCount");
                    this.IsModified = true;
                }
            }
        }// TotalValuesCount
        public int MissingValuesCount
        {
            get
            {
                return m_nmissing;
            }
            set
            {
                if (value != m_nmissing)
                {
                    m_nmissing = value;
                    NotifyPropertyChanged("MissingValuesCount");
                    this.IsModified = true;
                }
            }
        }// MissingValuesCount
        public bool IsCategVar
        {
            get
            {
                return m_iscateg;
            }
            set
            {
                if (value != m_iscateg)
                {
                    m_iscateg = value;
                    NotifyPropertyChanged("IsCategVar");
                    this.IsModified = true;
                }
            }
        }// IsCategVar
        public String DataType
        {
            get
            {
                return String.IsNullOrEmpty(m_datatype) ? String.Empty : m_datatype.Trim().ToLower();
            }
            set
            {
                String old = String.IsNullOrEmpty(m_datatype) ? String.Empty : m_datatype.Trim().ToLower();
                String s = String.IsNullOrEmpty(value) ? String.Empty : value.Trim().ToLower();
                if (old != s)
                {
                    m_datatype = s;
                    NotifyPropertyChanged("DataType");
                    this.IsModified = true;
                }
            }
        }// DataType
        public int DataSetId
        {
            get
            {
                return m_datasetid;
            }
            set
            {
                if (value != m_datasetid)
                {
                    m_datasetid = value;
                    NotifyPropertyChanged("DataSetId");
                    this.IsModified = true;
                }
            }
        }// DataSetId
        public String DataSetName
        {
            get
            {
                return String.IsNullOrEmpty(m_datasetname) ? String.Empty : m_datasetname.Trim().ToLower();
            }
            set
            {
                String old = String.IsNullOrEmpty(m_datasetname) ? String.Empty : m_datasetname.Trim().ToLower();
                String s = String.IsNullOrEmpty(value) ? String.Empty : value.Trim().ToLower();
                if (old != s)
                {
                    m_datasetname = s;
                    NotifyPropertyChanged("DataSetName");
                    this.IsModified = true;
                }
            }
        }// DataSetName
        public int VariableId
        {
            get
            {
                return m_varid;
            }
            set
            {
                if (value != m_varid)
                {
                    m_varid = value;
                    NotifyPropertyChanged("VariableId");
                    this.IsModified = true;
                }
            }
        }// VariableId
        public String VariableName
        {
            get
            {
                return String.IsNullOrEmpty(m_varname) ? String.Empty : m_varname.Trim().ToLower();
            }
            set
            {
                String old = String.IsNullOrEmpty(m_varname) ? String.Empty : m_varname.Trim().ToLower();
                String s = String.IsNullOrEmpty(value) ? String.Empty : value.Trim().ToLower();
                if (old != s)
                {
                    m_varname = s;
                    NotifyPropertyChanged("VariableName");
                    this.IsModified = true;
                }
            }
        }// VariableName
        #endregion // Properties
        #region Overrides
        protected override bool canBeRefreshed()
        {
            return (this.DataSetId != 0) && (this.VariableId != 0);
        }
        protected override List<string> getValidatedProperties()
        {
            List<string> oRet = new List<string>() {"DataSetId","DataSetName", "VariableId", "VariableName","DataType" };
            return oRet;
        }
        protected override string getValidationError(string propertyName)
        {
            String sRet = null;
            if (propertyName == "VariableId")
            {
                if (this.VariableId == 0)
                {
                    sRet = "Identifiant de Variable incorrect.";
                }
            }
            else if (propertyName == "VariableName")
            {
                if (String.IsNullOrEmpty(this.VariableName))
                {
                    sRet = "Nom de variable  incorrect.";
                }
            }
            else if (propertyName == "DataSetId")
            {
                if (this.DataSetId == 0)
                {
                    sRet = "Identifiant de DataSet incorrect.";
                }
            }
            else if (propertyName == "DataSetName")
            {
                if (String.IsNullOrEmpty(this.DataSetName))
                {
                    sRet = "Nom de DataSet  incorrect.";
                }
            }
            else if (propertyName == "DataType")
            {
                if (String.IsNullOrEmpty(this.DataType))
                {
                    sRet = "Type de données incorrect  incorrect.";
                }
            }
            return sRet;
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is VariableInfo))
            {
                return false;
            }
            VariableInfo p = obj as VariableInfo;
            return (this.DataSetId == p.DataSetId) && (this.VariableId == p.VariableId);
        }
        public override int CompareTo(object obj)
        {
            if (obj == null)
            {
                return -1;
            }
            if (!(obj is VariableInfo))
            {
                return -1;
            }
            VariableInfo p = obj as VariableInfo;
            int nRet = this.DataSetId.CompareTo(p.DataSetId);
            if (nRet != 0)
            {
                return nRet;
            }
            return (this.VariableName.CompareTo(p.VariableName));
        }
        public override int GetHashCode()
        {
            return (this.VariableId.GetHashCode() + this.DataSetId.GetHashCode());
        }
        public override string ToString()
        {
            return this.VariableName;
        }
        #endregion // Overrides
    }// class VariableInfo
    [Serializable]
    public class VariableInfos : ObservableCollection<VariableInfo>
    {
        public VariableInfos()
            : base()
        {
        }
        public VariableInfos(IEnumerable<VariableInfo> col)
            : base(col)
        {
        }
    }// class VariableInfos
}
