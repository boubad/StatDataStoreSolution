using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using System.Linq.Expressions;
using System.Reflection;

namespace StatData
{
    [Serializable]
    public class ARootElement : Observable, IDataErrorInfo, IComparable
    {
        #region Instance Variables
        private int m_id = 0;
        private bool m_modified = false;
        #endregion // Instance Variables
        #region Constructors
        public ARootElement()
        {
        }// ARootElement
        #endregion // Constructore
        #region Properties
        public int Id
        {
            get
            {
                return m_id;
            }
            set
            {
                if (value != m_id)
                {
                    m_id = value;
                    this.IsModified = true;
                    NotifyPropertyChanged("Id");
                }
            }
        }// Id
        [XmlIgnore]
        public bool IsRefreshed
        {
            get
            {
                return true;
            }
            set { }
        }
        [XmlIgnore]
        public bool IsModified
        {
            get
            {
                return m_modified;
            }
            set
            {
                if (value != m_modified)
                {
                    m_modified = value;
                    NotifyPropertyChanged("IsModified");
                    NotifyPropertyChanged("IsWriteable");
                    NotifyPropertyChanged("IsRemoveable");
                }
            }
        }// IsModified
        [XmlIgnore]
        public bool IsValid
        {
            get
            {
                return canBeRefreshed();
            }
            set { }
        }// IsValid
        [XmlIgnore]
        public bool IsWriteable
        {
            get
            {
                String s = this.Error;
                return String.IsNullOrEmpty(s);
            }
            set { }
        }// IsWriteable
        [XmlIgnore]
        public bool IsRemoveable
        {
            get
            {
                return (this.Id != 0);
            }
            set { }
        }// IsRemoveable
        #endregion // Properties
        [XmlIgnore]
        public String DisplayName
        {
            get
            {
                return this.ToString();
            }
            set { }
        }// DisplayName
        #region IDataErrorInfo implementation
        [XmlIgnore]
        public string Error
        {
            get
            {
                String sRet = "";
                IEnumerable<String> pp = getValidatedProperties();
                if (pp != null)
                {
                    foreach (var p in pp)
                    {
                        if (!String.IsNullOrWhiteSpace(p))
                        {
                            String sx = getValidationError(p);
                            if (!String.IsNullOrWhiteSpace(sx))
                            {
                                sRet += "\r\n" + sx;
                            }
                        }
                    }// p
                }// pp
                String sr = String.IsNullOrWhiteSpace(sRet) ? String.Empty : sRet.Trim();
                String sh = getGlobalErrors();
                if (!String.IsNullOrEmpty(sh))
                {
                    sr += "\r\n" + sh;
                }
                sr = sr.Trim();
                return String.IsNullOrEmpty(sr) ? null : sr;
            }

        }
        [XmlIgnore]
        public string this[string columnName]
        {
            get
            {
                String sx = getValidationError(columnName);
                return (!String.IsNullOrWhiteSpace(sx)) ? sx.Trim() : null;
            }
        }
        #endregion //IDataErrorInfo
        #region virtual methods
        protected virtual bool canBeRefreshed()
        {
            return (this.Id != 0);
        }
        protected virtual String getValidationError(string propertyName)
        {
            return null;
        }// getValidationError
        protected virtual List<String> getValidatedProperties()
        {
            List<String> oRet = new List<String>();
            return oRet;
        }// getValidatedProperties
        protected virtual String getGlobalErrors()
        {
            return null;
        }// getGlobalErrors
        #endregion // virtual methods
        #region Overrides
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is ARootElement))
            {
                return false;
            }
            ARootElement p = obj as ARootElement;
            return (this.Id == p.Id);
        }
        public virtual int CompareTo(object obj)
        {
            if (obj == null)
            {
                return -1;
            }
            if (!(obj is ARootElement))
            {
                return -1;
            }
            ARootElement p = obj as ARootElement;
            return this.Id.CompareTo(p.Id);
        }
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
        #endregion // Overrides
    } // class ARootItem
}
