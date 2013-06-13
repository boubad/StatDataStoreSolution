using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Windows;
using System.Text.RegularExpressions;
using System.IO;
using System.Data.Objects.DataClasses;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Controls;
using StatApp.ModelView;

namespace StatApp.Controles
{
    class ConverterHelper
    {
        public static BitmapImage dataBytesToBitmapImage(Object value)
        {
            BitmapImage ImgSource = null;
            try
            {
                if ((value != null) && (value is byte[]))
                {
                    byte[] data = value as byte[];
                    ImgSource = new BitmapImage();
                    ImgSource.BeginInit();
                    ImgSource.StreamSource = new MemoryStream(data);
                    ImgSource.EndInit();
                }// value
            }
            catch (Exception /*ex */)
            {
            }
            return (ImgSource == null) ? new BitmapImage() : ImgSource;
        }// dataBytesToBitmapImage
    }// ConverterHleper
    [ValueConversion(typeof(byte[]), typeof(BitmapImage))]
    public class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ConverterHelper.dataBytesToBitmapImage(value);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Method not implemented.");
        }
    }// class ImageConverter
    [ValueConversion(typeof(double), typeof(String))]
    public class DoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double val = (double)value;
            if (val >= 0.0)
            {
                int n = (int)(10000.0 * val + 0.5);
                double f = (double)n / 10000.0;
                return System.Convert.ToString(f);
            }
            else
            {
                int n = (int)(10000.0 * val - 0.5);
                double f = (double)n / 10000.0;
                return System.Convert.ToString(f);
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double dval = -1.0;
            if (value != null)
            {
                string strValue = value.ToString();
                Double.TryParse(strValue, out dval);
            }
            return dval;
        }
    }// class DoubleConverter
    [ValueConversion(typeof(int), typeof(String))]
    public class IntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int val = (int)value;
            return System.Convert.ToString(val);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int dval = -1;
            if (value != null)
            {
                string strValue = value.ToString();
                int.TryParse(strValue, out dval);
            }
            return dval;
        }
    }// class IntConverter
    [ValueConversion(typeof(int), typeof(String))]
    public class IndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int val = (int)value;
            if (val >= 0)
            {
                return System.Convert.ToString(val);
            }
            else
            {
                return String.Empty;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int dval = -1;
            if (value != null)
            {
                string strValue = value.ToString();
                int.TryParse(strValue, out dval);
            }
            return dval;
        }
    }// class IntConverter
    [ValueConversion(typeof(TreeLinkType), typeof(String))]
    public class LinkTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TreeLinkType val = (TreeLinkType)value;
            String sRet = "";
            if (val == TreeLinkType.linkmax)
            {
                sRet = "MAX";
            }
            else if (val == TreeLinkType.linkmin)
            {
                sRet = "MIN";
            }
            else if (val == TreeLinkType.linkmean)
            {
                sRet = "MEAN";
            }
            return sRet;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TreeLinkType dval = TreeLinkType.linkmean;
            if (value != null)
            {
                string strValue = value.ToString().Trim().ToUpper();
                if (strValue == "MIN")
                {
                    dval = TreeLinkType.linkmin;
                }
                else if (strValue == "MAX")
                {
                    dval = TreeLinkType.linkmin;
                }
            }
            return dval;
        }
    }// class IntConverter
    [ValueConversion(typeof(double), typeof(SolidColorBrush))]
    public class MatGrayScaleConterver : IValueConverter
    {
        //
        private static Dictionary<byte, SolidColorBrush> m_bruhes = new Dictionary<byte, SolidColorBrush>();
        //
        public static void InitBrushes()
        {
            for (int i = 0; i < 256; ++i)
            {
                byte b = (byte)i;
                System.Windows.Media.Color c = System.Windows.Media.Color.FromArgb(255, b, b, b);
                SolidColorBrush bRet = new SolidColorBrush(c);
                m_bruhes[b] = bRet;
            }
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush bRet = null;
            double val = (double)value;
            if (val < 0)
            {
                val = 0;
            }
            else if (val > 1.0)
            {
                val = 1.0;
            }
            val = 1.0 - val;
            byte bval = (byte)(255.0 * val);
            if (bval > 255)
            {
                bval = 255;
            }
            if (m_bruhes.Keys.Count < 1)
            {
                InitBrushes();
            }
            if (m_bruhes.ContainsKey(bval))
            {
                bRet = m_bruhes[bval];
            }
            else
            {
                bRet = new SolidColorBrush(Colors.White);
            }
            return bRet;
        }// Convert
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }// class MatGraySacleConverter
    [ValueConversion(typeof(MatriceComputeMode), typeof(String))]
    public class MatricteComputeModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            MatriceComputeMode val = (MatriceComputeMode)value;
            String sRet = null;
            if (val == MatriceComputeMode.modeNothing)
            {
                sRet = "Aucune transformation";
            }
            else if (val == MatriceComputeMode.modeNormalize)
            {
                sRet = "Valeurs centrées réduites";
            }
            else if (val == MatriceComputeMode.modeProfil)
            {
                sRet = "Profils";
            }
            else if (val == MatriceComputeMode.modeRank)
            {
                sRet = "Rangs";
            }
            return sRet;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            MatriceComputeMode dval = MatriceComputeMode.modeNothing;
            if (value != null)
            {
                string strValue = value.ToString().Trim().ToUpper();
                if (strValue.Contains("AUCUN"))
                {
                    dval = MatriceComputeMode.modeNothing;
                }
                else if (strValue.Contains("CENT"))
                {
                    dval = MatriceComputeMode.modeNormalize;
                }
                else if (strValue.Contains("PROFIL"))
                {
                    dval = MatriceComputeMode.modeProfil;
                }
                else if (strValue.Contains("RANG"))
                {
                    dval = MatriceComputeMode.modeRank;
                }
            }
            return dval;
        }
    }// class IntConverter
}
