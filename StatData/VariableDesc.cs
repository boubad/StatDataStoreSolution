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
    public class VariableDesc : NamedElement
    {
        #region static variables
        private static readonly String[] TAG_TYPES = { "bool", "short", "int", "float", "double", "string" };
        #endregion // static variables
        #region instance variables
        private String m_datatype;
        private int m_datasetid = 0;
        private bool m_isid = false;
        private bool m_isname = false;
        private bool m_iscateg = false;
        private bool m_isimage = false;
        private bool m_isinfo = false;
        private ValueDescs m_values;
        private VariableInfo m_info;
        #endregion // Instance variables
        #region Constructors
        public VariableDesc()
        {
        }
        #endregion // Constructors
        #region Properties
        public VariableInfo Info
        {
            get
            {
                return (m_info == null) ? new VariableInfo() : m_info;
            }
            set
            {
                if (value != m_info)
                {
                    m_info = value;
                    NotifyPropertyChanged("Info");
                    NotifyPropertyChanged("HasInfo");
                }
            }
        }// Info
        public bool HasInfo
        {
            get
            {
                return (m_info != null) && (m_info.IsValid);
            }
            set { }
        }// HasInfo
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
                    if ((m_values != null) && (m_values.Count > 1))
                    {
                        List<ValueDesc> oList = m_values.ToList();
                        oList.Sort();
                        m_values = new ValueDescs(oList);
                    }
                    NotifyPropertyChanged("Values");
                    NotifyPropertyChanged("HasValues");
                }
            }
        }// Values
        public bool HasValues
        {
            get
            {
                return ((m_values != null) && (m_values.Count > 0));
            }
            set { }
        }// HasValues
        public bool IsIdVar
        {
            get
            {
                return m_isid;
            }
            set
            {
                if (value != m_isid)
                {
                    m_isid = value;
                    NotifyPropertyChanged("IsIdVar");
                    this.IsModified = true;
                }
            }
        }// IsIdVar
        public bool IsNameVar
        {
            get
            {
                return m_isname;
            }
            set
            {
                if (value != m_isname)
                {
                    m_isname = value;
                    NotifyPropertyChanged("IsNameVar");
                    this.IsModified = true;
                }
            }
        }// IsNameVar
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
                    this.IsModified = true;
                    NotifyPropertyChanged("IsCategVar");
                    NotifyPropertyChanged("IsNumVar");
                }
            }
        }// IsCategVar
        public bool IsImageVar
        {
            get
            {
                return m_isimage;
            }
            set
            {
                if (value != m_isimage)
                {
                    m_isimage = value;
                    this.IsModified = true;
                    NotifyPropertyChanged("IsImageVar");
                }
            }
        }// IsImageVar
        public bool IsInfoVar
        {
            get
            {
                return m_isinfo;
            }
            set
            {
                if (value != m_isinfo)
                {
                    m_isinfo = value;
                    this.IsModified = true;
                    NotifyPropertyChanged("IsInfoVar");
                }
            }
        }// IsInfoVar
        [XmlIgnore]
        public bool IsNumVar
        {
            get
            {
                return (!this.IsCategVar);
            }
            set
            {
                bool bold = !this.IsCategVar;
                if (value != bold)
                {
                    m_iscateg = !value;
                    NotifyPropertyChanged("IsCategVar");
                    NotifyPropertyChanged("IsNumVar");
                }
            }
        }// IsNumVar
        public ObservableCollection<String> DataTypes
        {
            get
            {
                return new ObservableCollection<string>(TAG_TYPES);
            }
            set { }
        }// DataTypes
        public String DataType
        {
            get
            {
                return String.IsNullOrEmpty(m_datatype) ? String.Empty : m_datatype.Trim();
            }
            set
            {
                String s = String.IsNullOrEmpty(value) ? String.Empty : value.Trim().ToLower();
                foreach (var x in TAG_TYPES)
                {
                    if (x == s)
                    {
                        String old = String.IsNullOrEmpty(m_datatype) ? String.Empty : m_datatype.Trim().ToLower();
                        if (old != s)
                        {
                            m_datatype = s;
                            NotifyPropertyChanged("DataType");
                            this.IsModified = true;
                        }
                        break;
                    }
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
        #endregion // Properties
        #region Overrides
        protected override bool canBeRefreshed()
        {
            if (this.Id != 0)
            {
                return true;
            }
            return (this.DataSetId != 0) && (!String.IsNullOrEmpty(this.Name));
        }
        protected override List<string> getValidatedProperties()
        {
            List<string> oRet = new List<string>() { "DataSetId", "Name", "DataType" };
            return oRet;
        }
        protected override string getValidationError(string propertyName)
        {
            String sRet = null;
            if (propertyName == "DataSetId")
            {
                if (this.DataSetId == 0)
                {
                    sRet = "Identifiant de DataSet incorrect.";
                }
            }
            else if (propertyName == "DataType")
            {
                String s = this.DataType;
                if (!String.IsNullOrEmpty(s))
                {
                    s = s.Trim();
                }
                if (String.IsNullOrEmpty(s))
                {
                    sRet = "Le type de données ne doit pas être vide.";
                }
                else if (s.Length > 31)
                {
                    sRet = "La longueur du type de données ne doit pas dépasser 31 caractères.";
                }
            }
            else if (propertyName == "Name")
            {
                String s = this.Name;
                if (!String.IsNullOrEmpty(s))
                {
                    s = s.Trim();
                }
                if (String.IsNullOrEmpty(s))
                {
                    sRet = "Le nom ne doit pas être vide.";
                }
                else if (s.Length > 31)
                {
                    sRet = "La longueur du nom ne doit pas dépasser 31 caractères.";
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
            if (!(obj is VariableDesc))
            {
                return false;
            }
            VariableDesc p = obj as VariableDesc;
            if ((this.Id != 0) && (this.Id == p.Id))
            {
                return true;
            }
            return (this.DataSetId == p.DataSetId) && (this.Name.ToLower() == p.Name.ToLower());
        }
        public override int CompareTo(object obj)
        {
            if (obj == null)
            {
                return -1;
            }
            if (!(obj is VariableDesc))
            {
                return -1;
            }
            VariableDesc p = obj as VariableDesc;
            int nRet = this.DataSetId.CompareTo(p.DataSetId);
            if (nRet != 0)
            {
                return nRet;
            }
            if (this.IsIdVar && (!p.IsIdVar))
            {
                return -1;
            }
            else if ((!this.IsIdVar) && p.IsIdVar)
            {
                return 1;
            }
            if (this.IsNameVar && (!p.IsNameVar))
            {
                return -1;
            }
            else if ((!this.IsNameVar) && p.IsNameVar)
            {
                return 1;
            }
            if (this.IsImageVar && (!p.IsImageVar))
            {
                return -1;
            }
            else if ((!this.IsImageVar) && p.IsImageVar)
            {
                return 1;
            }
            if (this.IsInfoVar && (!p.IsInfoVar))
            {
                return -1;
            }
            else if ((!this.IsInfoVar) && p.IsInfoVar)
            {
                return 1;
            }
            if (this.IsCategVar && (!p.IsCategVar))
            {
                return -1;
            }
            else if ((!this.IsCategVar) && p.IsCategVar)
            {
                return 1;
            }
            return this.Name.ToLower().CompareTo(p.Name.ToLower());
        }
        public override int GetHashCode()
        {
            return this.Name.ToLower().GetHashCode();
        }
        #endregion // overrides
    }// class VariableDesc
    [Serializable]
    public class VariableDescs : ObservableCollection<VariableDesc>
    {
        public VariableDescs()
            : base()
        {
        }
        public VariableDescs(IEnumerable<VariableDesc> col)
            : base(col)
        {
        }
    }// class VariableDescs
}
