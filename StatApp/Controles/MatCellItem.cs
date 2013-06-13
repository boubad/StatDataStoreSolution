using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Media;
using StatData;
namespace StatApp.Controles
{
    public enum DisplayItemType
    {
        eDisplayDefault,eDisplayIndex,eDisplayText,eDisplayImage,eDisplayNumber,eDisplayTitle
    }
    public class DisplayItem : Observable
    {
        private Object m_tag;
        private double m_dval;
        private int m_ival = -1;
        private String m_sval = null;
        private byte[] m_data = null;
        public DisplayItem()
        {
            this.DisplayType = DisplayItemType.eDisplayDefault;
        }
        public DisplayItem(int ival)
        {
            this.DisplayType = DisplayItemType.eDisplayIndex;
            this.IntValue = ival;
        }
        public DisplayItem(double ival)
        {
            this.DisplayType = DisplayItemType.eDisplayNumber;
            this.DoubleValue = ival;
        }
        public DisplayItem(String ival)
        {
            this.DisplayType = DisplayItemType.eDisplayText;
            this.StringValue = ival;
        }
        public DisplayItem(String ival,bool bTitle)
        {
            this.DisplayType = DisplayItemType.eDisplayTitle;
            this.StringValue = ival;
        }
        public DisplayItem(byte[] ival)
        {
            this.DisplayType = DisplayItemType.eDisplayImage;
            this.DataBytes = ival;
        }
        public DisplayItemType DisplayType { get; set; }
        public Object Tag
        {
            get { return m_tag; }
            set
            {
                if (value != m_tag)
                {
                    m_tag = value;
                    NotifyPropertyChanged("Tag");
                }
            }
        }
        public int IntValue {
            get
            {
                return m_ival;
            }
            set
            {
                if (value != m_ival)
                {
                    m_ival = value;
                    NotifyPropertyChanged("IntValue");
                }
            }
        }
        public double DoubleValue {
            get
            {
                return m_dval;
            }
            set
            {
                if (value != m_dval)
                {
                    m_dval = myconvert10000(value);
                    NotifyPropertyChanged("DoubleValue");
                }
            }
        }
        public String StringValue {
            get
            {
                return m_sval;
            }
            set
            {
                m_sval = value;
                NotifyPropertyChanged("StringValue");
            }
        }
        public byte[] DataBytes {
            get
            {
                return m_data;
            }
            set
            {
                m_data = value;
                NotifyPropertyChanged("DataBytes");
            }
        }
        public override string ToString()
        {
            String sRet = "N/A";
            switch (this.DisplayType)
            {
                case DisplayItemType.eDisplayIndex:
                    if (this.IntValue >= 0)
                    {
                    }
                    sRet = Convert.ToString(this.IntValue);
                    break;
                case DisplayItemType.eDisplayNumber:
                    sRet = Convert.ToString(this.DoubleValue);
                    break;
                case DisplayItemType.eDisplayText:
                case DisplayItemType.eDisplayTitle:
                    sRet = this.StringValue;
                    break;
                default:
                    break;
            }// type
            return sRet.Replace(",", "."); ;
        }
        protected static double myconvert10000(double x)
        {
            double xx = 10000.0 * x;
            if (x < 0.0)
            {
                xx -= 0.5;
            }
            else
            {
                xx += 0.5;
            }
            int nx = (int)xx;
            return (double)(nx / 10000.0);
        }// myconvert10000
    }// class DisplatItem
    public class DisplayItems : ObservableCollection<DisplayItem>
    {
        private Object m_tag;
        public DisplayItems()
            : base()
        {
        }
        public DisplayItems(IEnumerable<DisplayItem> col)
            : base(col)
        {
        }
        public Object Tag
        {
            get { return m_tag; }
            set
            {
                if (value != m_tag)
                {
                    m_tag = value;
                }
            }
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            bool bFirst = true;
            foreach (var p in this)
            {
                if (!bFirst)
                {
                    sb.Append("\t");
                }
                sb.Append(p.ToString());
                bFirst = false;
            }// p
            return sb.ToString();
        }
    }
    public class DisplayItemsArray : ObservableCollection<DisplayItems>
    {
        public DisplayItemsArray()
            : base()
        {
        }
        public DisplayItemsArray(IEnumerable<DisplayItems> col)
            : base(col)
        {
        }
        public void CopyToClipboard()
        {
            try
            {
                String sval = this.ToString();
                Clipboard.SetData(DataFormats.Text, sval);
            }
            catch (Exception /* ex */)
            {
            }
        }//CopyToClipboard 
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            bool bFirst = true;
            foreach (var p in this)
            {
                if (!bFirst)
                {
                    sb.Append("\n");
                }
                sb.Append(p.ToString());
                bFirst = false;
            }// p
            return sb.ToString();
        }
    }
    //
    public enum MatItemType
    {
        typeNothing, typeVar, typeInd, typeValInf, typeValSup, typeSummary
    }
    public enum MatDisplayMode
    {
        modeGrayscale, modeHistog
    }
    public enum MatScaleMode
    {
        scaleOwn, scaleCommon
    }
    public enum MatriceComputeMode
    {
        modeNothing,
        modeNormalize,
        modeProfil,
        modeRank
    }
    public class MatCellItem : Observable
    {
        private MatItemType m_type;
        private MatDisplayMode m_mode;
        private int m_ncur;
        private int m_nmax;
        private String m_text;
        private int m_totalheight;
        private int m_width;
        //
        public MatCellItem()
        {
            m_type = MatItemType.typeNothing;
            m_mode = MatDisplayMode.modeGrayscale;
            m_ncur = 0;
            m_nmax = 0;
            m_text = String.Empty;
            m_width = 40;
            m_totalheight = 40;
        }
        public MatCellItem(MatItemType eType, String sVal)
        {
            m_type = eType;
            m_mode = MatDisplayMode.modeGrayscale;
            m_ncur = 0;
            m_nmax = 0;
            m_text = (String.IsNullOrWhiteSpace(sVal)) ? String.Empty : sVal.Trim();
            m_width = 40;
            m_totalheight = 40;
        }
        public MatCellItem(MatItemType eType, int nCur, int nMax)
        {
            m_type = eType;
            m_mode = MatDisplayMode.modeGrayscale;
            m_ncur = nCur;
            m_nmax = nMax;
            m_text = String.Empty;
            m_width = 40;
            m_totalheight = 40;
        }
        public int CurrentValue
        {
            get
            {
                return m_ncur;
            }
            set
            {
                if (value != m_ncur)
                {
                    m_ncur = value;
                   NotifyPropertyChanged("CurrentValue");
                   NotifyPropertyChanged("SupHeight");
                   NotifyPropertyChanged("InfHeight");
                   NotifyPropertyChanged("Ratio");
                }
            }
        }
        public int MaxValue
        {
            get
            {
                return m_nmax;
            }
            set
            {
                if (value != m_nmax)
                {
                    m_nmax = value;
                   NotifyPropertyChanged("MaxValue");
                   NotifyPropertyChanged("SupHeight");
                   NotifyPropertyChanged("InfHeight");
                   NotifyPropertyChanged("Ratio");
                }
            }
        }
        public String Text
        {
            get
            {
                return m_text;
            }
            set
            {
                if (m_text != value)
                {
                    m_text = value;
                   NotifyPropertyChanged("Text");
                }
            }
        }
        public MatItemType CellType
        {
            get { return m_type; }
            set
            {
                if (m_type != value)
                {
                    m_type = value;
                   NotifyPropertyChanged("CellType");
                }
            }
        }
        public MatDisplayMode DisplayMode
        {
            get { return m_mode; }
            set
            {
                if (m_mode != value)
                {
                    m_mode = value;
                   NotifyPropertyChanged("DisplayMode");
                }
            }
        }
        public int TotalHeight
        {
            get
            {
                return m_totalheight;
            }
            set
            {
                if (m_totalheight != value)
                {
                    m_totalheight = value;
                   NotifyPropertyChanged("TotalHeight");
                   NotifyPropertyChanged("SupHeight");
                   NotifyPropertyChanged("InfHeight");
                   NotifyPropertyChanged("Ratio");
                }
            }
        }
        public int TotalWidth
        {
            get
            {
                return m_width;
            }
            set
            {
                if (m_width != value)
                {
                    m_width = value;
                   NotifyPropertyChanged("TotalWidth");
                }
            }
        }
        public double Ratio
        {
            get
            {
                double dRet = 0.0;
                if ((m_nmax > 0) && (m_ncur <= m_nmax))
                {
                    dRet = (double)m_ncur / (double)m_nmax;
                }
                return dRet;
            }
            set
            {
            }
        }// Ratio
        public int ValueHeight
        {
            get
            {
                int nRet = 0;
                if (m_nmax > 0)
                {
                    if (m_ncur <= m_nmax)
                    {
                        nRet = (int)(this.Ratio * this.TotalHeight);
                        if (nRet < 0)
                        {
                            nRet = -nRet;
                        }
                    }
                }// m_nmax
                return nRet;
            }
            set { }
        }// ValueHeight
        public int InfHeight
        {
            get
            {
                int nRet = 0;
                if (m_nmax > 0)
                {
                    if (m_ncur <= (m_nmax / 2))
                    {
                        nRet = (int)(this.Ratio * this.TotalHeight);
                        if (nRet < 0)
                        {
                            nRet = -nRet;
                        }
                    }
                }// m_nmax
                return nRet;
            }
            set { }
        }// InfHeight
        public int SupHeight
        {
            get
            {
                int nRet = 0;
                if (m_nmax > 0)
                {
                    if (m_ncur > (m_nmax / 2))
                    {
                        double d = (double)(m_ncur - (m_nmax / 2)) / (double)m_nmax;
                        nRet = (int)(d * this.TotalHeight);
                        if (nRet < 0)
                        {
                            nRet = -nRet;
                        }
                    }
                }// m_nmax
                return nRet;
            }
            set { }
        }// InfHeight
        public int Diameter
        {
            get
            {
                int nRet = 0;
                if (m_nmax > 0)
                {
                    nRet = (int)(this.Ratio * this.TotalHeight);
                }// m_nmax
                return nRet;
            }
            set { }
        }// Diameter
        
    }// class MatCellItem
    public class MatCellItems : ObservableCollection<MatCellItem>
    {
        public MatCellItems()
            : base()
        {

        }
        public MatCellItems(IEnumerable<MatCellItem> col)
            : base(col)
        {
        }
    }
}
