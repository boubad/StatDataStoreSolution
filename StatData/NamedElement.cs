using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatData
{
    [Serializable]
    public class NamedElement: ARootElement
    {
        #region Instance Variables
        private String m_name;
        private String m_desc;
        #endregion // Instance Variables
        #region Constructors
        public NamedElement()
        {
        }
        #endregion // Constructors
        #region Properties
        public String Description
        {
            get
            {
                return String.IsNullOrEmpty(m_desc) ? String.Empty : m_desc.Trim();
            }
            set
            {
                String old = String.IsNullOrEmpty(m_desc) ? String.Empty : m_desc.Trim();
                String s = String.IsNullOrEmpty(value) ? String.Empty : value.Trim();
                if (s != old)
                {
                    m_desc = value;
                    this.IsModified = true;
                    NotifyPropertyChanged("Description");
                }
            }
        }// Description
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
                if (s.ToLower() != old.ToLower())
                {
                    m_name = value;
                    this.IsModified = true;
                    NotifyPropertyChanged("Name");
                }
            }
        }// Name
        #endregion // Properties
        #region Overrides
        protected override bool canBeRefreshed()
        {
            if (base.canBeRefreshed())
            {
                return true;
            }
            return (!String.IsNullOrEmpty(this.Name));
        }
        protected override List<string> getValidatedProperties()
        {
            List<string> oRet = new List<string>() { "Name" };
            return oRet;
        }
        protected override string getValidationError(string propertyName)
        {
            String sRet = null;
            if (propertyName == "Name")
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
            if (!(obj is NamedElement))
            {
                return false;
            }
            NamedElement p = obj as NamedElement;
            if ((this.Id != 0) && (this.Id == p.Id))
            {
                return true;
            }
            return (this.Name.ToLower() == p.Name.ToLower());
        }
        public override int CompareTo(object obj)
        {
            if (obj == null)
            {
                return -1;
            }
            if (!(obj is NamedElement))
            {
                return -1;
            }
            NamedElement p = obj as NamedElement;
            return this.Name.ToLower().CompareTo(p.Name.ToLower());
        }
        public override int GetHashCode()
        {
            return this.Name.ToLower().GetHashCode();
        }
        public override string ToString()
        {
            return this.Name;
        }
        #endregion // Overrides
    }// class NamedElement
}
