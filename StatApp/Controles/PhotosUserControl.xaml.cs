using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using StatApp.ModelView;
using StatData;

namespace StatApp.Controles
{
    /// <summary>
    /// Logique d'interaction pour PhotosUserControl.xaml
    /// </summary>
    public partial class PhotosUserControl : UserControl
    {
        private CancellationTokenSource m_cts;
        private PhotosModelView m_model;
        public PhotosUserControl()
        {
            InitializeComponent();
            this.progressBar.Visibility = Visibility.Hidden;
            this.buttonCancel.IsEnabled = false;
        }
        private void myUpdateUI(bool b)
        {
            var model = getModel();
            bool bEnable = b && (model != null);
            bEnable = bEnable && (model.DataService != null);
            this.buttonBrowse.IsEnabled = bEnable;
            this.buttonImportAll.IsEnabled = bEnable;
            this.buttonNew.IsEnabled = bEnable;
            this.buttonNextPage.IsEnabled = bEnable && ((model.Skip + model.Taken) < model.TotalValuesCount);
            this.buttonPrevPage.IsEnabled = bEnable && (model.Skip > 0);
            this.buttonRefresh.IsEnabled = bEnable;
            this.buttonRemove.IsEnabled = bEnable && model.CurrentPhoto.IsRemoveable;
            this.buttonSave.IsEnabled = bEnable && model.CurrentPhoto.IsWriteable;
        }// myUpdateUI
        private PhotosModelView getModel()
        {
            PhotosModelView oRet = null;
            Object obj = this.DataContext;
            if ((obj != null) && (obj is PhotosModelView))
            {
                oRet = obj as PhotosModelView;
            }
            return oRet;
        }// getModel
        private void buttonPrevPage_Click(object sender, RoutedEventArgs e)
        {
            var model = getModel();
            if (model != null)
            {
                myUpdateUI(false);
                model.PrevPage();
            }
        }
        private void buttonNextPage_Click(object sender, RoutedEventArgs e)
        {
            var model = getModel();
            if (model != null)
            {
                myUpdateUI(false);
                model.NextPage();
            }
        }
        private void buttonNew_Click(object sender, RoutedEventArgs e)
        {
            var model = getModel();
            if (model != null)
            {
                model.CurrentPhoto = new PhotoDesc();
                model.BrowsePhoto();
            }
        }
        private void buttonBrowse_Click(object sender, RoutedEventArgs e)
        {
            var model = getModel();
            if (model != null)
            {
                model.BrowsePhoto();
            }
        }
        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            var model = getModel();
            if (model != null)
            {
                myUpdateUI(false);
                model.MaintainsPhoto();
                model.RefreshPhotos();
            }

        }
        private void buttonRemove_Click(object sender, RoutedEventArgs e)
        {
            var model = getModel();
            if (model != null)
            {
                PhotoDesc p = model.CurrentPhoto;
                if ((p != null) && p.IsRemoveable)
                {
                    var res = MessageBox.Show("Voulez-vous vraiment supprimer cette photo?", "StatApp", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (res == MessageBoxResult.Yes)
                    {
                        myUpdateUI(false);
                        model.RemovePhoto();
                        model.RefreshPhotos();
                    }
                }
            }
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var model = getModel();
            if (model != null)
            {
                myUpdateUI(false);
                model.RefreshPhotos();
            }
        }
        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var model = getModel();
            m_model = model;
            if (model != null)
            {
                m_model.PropertyChanged += m_model_PropertyChanged;
                myUpdateUI(false);
                model.RefreshPhotos();
            }
        }
        void m_model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            String name = e.PropertyName;
            if ((name == "Photos") || (name == "CurrentPhoto"))
            {
                this.myUpdateUI(true);
            }
        }
        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            var model = getModel();
            if (model != null)
            {
                myUpdateUI(false);
                model.RefreshPhotos();
            }
        }
        private Task importPhotosAsync(IStoreDataManager pMan, String[] filenames,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            return Task.Run(() =>
            {
                try
                {
                    for (int i = 0; i < filenames.Length; ++i)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }
                        String filename = filenames[i];
                        if (!String.IsNullOrEmpty(filename))
                        {
                            var xx = PhotosModelView.GetJPEGBytes(filename);
                            byte[] data = xx.Item1;
                            String name = xx.Item2;
                            Exception err = xx.Item3;
                            if ((data != null) && (data.Length > 1) && (err == null) && (!String.IsNullOrEmpty(name)))
                            {
                                PhotoDesc oPhoto = new PhotoDesc();
                                oPhoto.DataBytes = data;
                                oPhoto.Name = name;
                                var yy = pMan.MaintainsPhoto(oPhoto);
                                if ((yy.Item1 == null) || (yy.Item2 != null))
                                {
                                    break;
                                }
                            }// data
                        }
                        if (progress != null)
                        {
                            progress.Report(i);
                        }
                    }// i
                }
                catch (Exception /*ex */)
                {
                }
            }, cancellationToken);
        }//importPhotosAsync
        private async void buttonImportAll_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = "*.jpg";
            dlg.Filter = "Images JPEG (*.jpg)|*.jpg|Images JPEG (*.jpeg)|*.jpeg|Images PNG (*.png)|*.png";
            dlg.Multiselect = true;
            Nullable<bool> result = dlg.ShowDialog();
            String[] filenames = null;
            if ((result != null) && result.HasValue && (result.Value == true))
            {
                filenames = dlg.FileNames;
            }
            if (filenames == null)
            {
                return;
            }
            if (filenames.Length < 1)
            {
                return;
            }
            var model = getModel();
            if (model == null)
            {
                return;
            }
            var pMan = model.DataService;
            myUpdateUI(false);
            this.progressBar.Visibility = Visibility.Visible;
            this.progressBar.Maximum = filenames.Length;
            m_cts = new CancellationTokenSource();
            var progress = new Progress<int>((int ival) =>
            {
                this.progressBar.Value = ival;
            });
            this.buttonCancel.IsEnabled = true;
            await importPhotosAsync(pMan,filenames, m_cts.Token, progress);
            m_cts = null;
            this.progressBar.Visibility = Visibility.Hidden;
            this.buttonCancel.IsEnabled = false;
            model.RefreshPhotos();
        }
        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            if ((m_cts != null) && (!m_cts.IsCancellationRequested))
            {
                m_cts.Cancel();
                this.buttonCancel.IsEnabled = false;
            }
        }
    }
}
