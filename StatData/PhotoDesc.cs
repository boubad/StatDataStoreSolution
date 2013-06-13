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
    public class PhotoDesc : NamedElement
    {
        private byte[] m_data;
        public PhotoDesc()
        {
        }
        public byte[] DataBytes
        {
            get
            {
                return m_data;
            }
            set
            {
                if (value != m_data)
                {
                    m_data = value;
                    NotifyPropertyChanged("DataBytes");
                    this.IsModified = true;
                }
            }
        }// DataBytes
        protected override bool canBeRefreshed()
        {
            return (this.Id != 0) || (!String.IsNullOrEmpty(this.Name));
        }
        protected override List<string> getValidatedProperties()
        {
            return new List<string>() { "Name","DataBytes"};
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
                else if (s.Length > 98)
                {
                    sRet = "La longueur du nom ne doit pas dépasser 98 caractères.";
                }
            }
            else if (propertyName == "DataBytes")
            {
                var p = this.DataBytes;
                if (p == null)
                {
                    sRet = "Le contenu de l'image manque.";
                }
                else if (p.Length < 2)
                {
                    sRet = "Taille de l'image incorrecte";
                }
            }
            return sRet;
        }
    }// class PhotoDesc
    [Serializable]
    public class PhotoDescs : ObservableCollection<PhotoDesc>
    {
        public PhotoDescs()
            : base()
        {
        }
        public PhotoDescs(IEnumerable<PhotoDesc> col)
            : base(col)
        {
        }
    }
}
