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
    public class ValueDesc : ARootElement
    {
        #region Instance variables
        private int m_variableid = 0;
        private int m_index = -1;
        private String m_sval;
        private double m_doubletag;
        private String m_varname;
        private String m_vartype;
        #endregion // Instance variables
        #region Constructors
        public ValueDesc()
        {
        }
        #endregion // Constructors
        #region Properties 
        public String VariableName
        {
            get
            {
                return String.IsNullOrEmpty(m_varname) ? String.Empty : m_varname.Trim();
            }
            set
            {
                String old = String.IsNullOrEmpty(m_varname) ? String.Empty : m_varname.Trim() ;
                String s = String.IsNullOrEmpty(value) ? String.Empty : value.Trim();
                if (old.ToLower() != s.ToLower())
                {
                    m_varname = value;
                    NotifyPropertyChanged("VariableName");
                }
            }
        }// VariableName
        public String VariableType
        {
            get
            {
                return String.IsNullOrEmpty(m_vartype) ? String.Empty : m_vartype.Trim().ToLower();
            }
            set
            {
                String old = String.IsNullOrEmpty(m_vartype) ? String.Empty : m_vartype.Trim().ToLower();
                String s = String.IsNullOrEmpty(value) ? String.Empty : value.Trim().ToLower();
                if (old != s)
                {
                    m_vartype = s;
                    NotifyPropertyChanged("VariableType");
                }
            }
        }// VariableName
        public String StringTag { get; set; }
        public double DoubleTag
        {
            get
            {
                return m_doubletag;
            }
            set
            {
                m_doubletag = value;
            }
        }
        public int VariableId
        {
            get
            {
                return m_variableid;
            }
            set
            {
                if (value != m_variableid)
                {
                    m_variableid = value;
                    NotifyPropertyChanged("VariableId");
                }
            }
        }// VariableId
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
                    this.IsModified = true;
                    NotifyPropertyChanged("Index");
                }
            }
        }// index
        public String DataStringValue
        {
            get
            {
                return String.IsNullOrWhiteSpace(m_sval) ? null : m_sval.Trim();
            }
            set
            {
                String old = String.IsNullOrEmpty(m_sval) ? String.Empty : m_sval.Trim();
                String s = String.IsNullOrEmpty(value) ? String.Empty : value.Trim();
                if (!String.IsNullOrEmpty(s))
                {
                    String ss = StatHelpers.ConvertValue(s);
                    if (String.IsNullOrEmpty(ss))
                    {
                        s = String.Empty;
                    }
                }
                if (old.ToLower() != s.ToLower())
                {
                    if (String.IsNullOrEmpty(s))
                    {
                        s = null;
                    }
                    m_sval = s;
                    this.IsModified = true;
                    NotifyPropertyChanged("DataStringValue");
                }
            }
        }// DataStringValue
        public double DoubleValue
        {
            get
            {
                String s = (String.IsNullOrEmpty(m_sval)) ? null : m_sval.Trim();
                if (String.IsNullOrEmpty(s))
                {
                    return 0.0;
                }
                double d = 0.0;
                double.TryParse(s, out d);
                return d;
            }
            set
            {
                String s = Convert.ToString(value);
                this.DataStringValue = s;
                NotifyPropertyChanged("DoubleValue");
            }
        }// DoubleValue
        public int IntValue
        {
            get
            {
                String s = (String.IsNullOrEmpty(m_sval)) ? null : m_sval.Trim();
                if (String.IsNullOrEmpty(s))
                {
                    return -1;
                }
                int d = -1;
                int.TryParse(s, out d);
                return d;
            }
            set
            {
                String s = Convert.ToString(value);
                this.DataStringValue = s;
                NotifyPropertyChanged("IntValue");
            }
        }// IntValue
        public String StringValue
        {
            get
            {
                return this.DataStringValue;
            }
            set
            {
                this.DataStringValue = value;
                NotifyPropertyChanged("StringValue");
            }
        }// StringValue
        #endregion // Properties
        #region overrides
        protected override bool canBeRefreshed()
        {
            if (this.Id != 0)
            {
                return true;
            }
            return (this.Index >= 0) && (this.VariableId != 0);
        }
        protected override List<string> getValidatedProperties()
        {
            List<string> oRet = new List<string>() { "VariableId", "Index" };
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
            else if (propertyName == "Index")
            {
                if (this.Index < 0 )
                {
                    sRet = "Index  incorrect.";
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
            if (!(obj is ValueDesc))
            {
                return false;
            }
            ValueDesc p = obj as ValueDesc;
            if ((this.Id != 0) && (this.Id == p.Id))
            {
                return true;
            }
            return (this.VariableId == p.VariableId) && (this.Index == p.Index);
        }
        public override int CompareTo(object obj)
        {
            if (obj == null)
            {
                return -1;
            }
            if (!(obj is ValueDesc))
            {
                return -1;
            }
            ValueDesc p = obj as ValueDesc;
            int nRet = this.VariableId.CompareTo(p.VariableId);
            if (nRet != 0)
            {
                return nRet;
            }
            if (this.Index < p.Index)
            {
                return -1;
            }
            else if (this.Index > p.Index)
            {
                return 1;
            }
            return 0;
        }
        public override int GetHashCode()
        {
            return (this.VariableId.GetHashCode() +  this.Index.GetHashCode());
        }
        public override string ToString()
        {
            return this.DataStringValue;
        }
        #endregion // overrides
    }// class ValueDesc
    [Serializable]
    public class ValueDescs : ObservableCollection<ValueDesc>
    {
        public ValueDescs()
            : base()
        {
        }
        public ValueDescs(IEnumerable<ValueDesc> col)
            : base(col)
        {
        }
    }// class ValueDescs
}
