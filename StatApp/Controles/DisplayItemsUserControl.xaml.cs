using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace StatApp.Controles
{
    /// <summary>
    /// Logique d'interaction pour DisplayItemsUserControl.xaml
    /// </summary>
    public partial class DisplayItemsUserControl : UserControl
    {
        public DisplayItemsUserControl()
        {
            InitializeComponent();
        }
        private DisplayItemsArray getModel()
        {
            DisplayItemsArray pRet = null;
            Object obj = this.DataContext;
            if ((obj != null) && (obj is DisplayItemsArray))
            {
                pRet = obj as DisplayItemsArray;
            }
            return pRet;
        }// getModel
        private void myUpdateUI()
        {
            var p = getModel();
            bool b = (p != null) && (p.Count > 1);
            this.buttonCopy.IsEnabled = b;
            this.buttonExport.IsEnabled = b;
        }// myUpdateUI
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.myUpdateUI();
        }

        private void buttonCopy_Click(object sender, RoutedEventArgs e)
        {
            var model = getModel();
            if ((model != null) && (model.Count > 1))
            {
                this.buttonCopy.IsEnabled = false;
                model.CopyToClipboard();
                this.buttonCopy.IsEnabled = true;
            }//model
        }

        private  async void buttonExport_Click(object sender, RoutedEventArgs e)
        {
            var model = getModel();
            if ((model != null) && (model.Count > 1))
            {
                try
                {
                    Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                    dlg.DefaultExt = ".tsv";
                    dlg.Filter = "Tab Separated values (*.tsv)|*.tsv";
                    Nullable<bool> result = dlg.ShowDialog();
                    String filename = null;
                    if ((result != null) && result.HasValue && (result.Value == true))
                    {
                        filename = dlg.FileName;
                    }
                    if (!String.IsNullOrEmpty(filename)){
                        this.buttonExport.IsEnabled = false;
                         await performExport(model, filename);
                    }
                }
                catch (Exception/* ex */)
                {
                }
                this.buttonExport.IsEnabled = true;
            }//model
        }
        private Task performExport(DisplayItemsArray oAr, String filename)
        {
            return Task.Run(() => {
                using (var fs = new FileStream(filename, FileMode.OpenOrCreate))
                {
                    String sVal = oAr.ToString();
                    StreamWriter writer = new StreamWriter(fs);
                    writer.Write(sVal);
                }// fs
            });
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            myUpdateUI();
        }// performExport
    }
}
