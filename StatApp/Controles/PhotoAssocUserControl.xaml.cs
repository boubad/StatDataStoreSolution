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
using StatApp.ModelView;
using StatData;
namespace StatApp.Controles
{
    /// <summary>
    /// Logique d'interaction pour PhotoAssocUserControl.xaml
    /// </summary>
    public partial class PhotoAssocUserControl : UserControl
    {
        String[] TAB_PX = new String[] { "WorkDone","IsModified","IsBusy", "DisplayIndivs", "CurrentIndiv", "Photos", "CurrentPhoto" };
        private PhotosAssocModelView m_model;
        private bool m_busy = false;
        public PhotoAssocUserControl()
        {
            InitializeComponent();
        }
        private PhotosAssocModelView getModel()
        {
            PhotosAssocModelView pRet = null;
            Object obj = this.DataContext;
            if ((obj != null) && (obj is PhotosAssocModelView))
            {
                pRet = obj as PhotosAssocModelView;
            }
            return pRet;
        }
        private void myUpdateUI()
        {
            var model = getModel();
            if (model == null)
            {
                return;
            }
            bool bOk = !model.IsBusy;
            this.buttonCommitChanges.IsEnabled = bOk && model.IsModified;
            this.buttonPrev.IsEnabled = bOk && (model.Skip > 0);
            this.buttonNext.IsEnabled = bOk && ((model.Skip + model.Taken) < model.TotalPhotosCount);
            this.buttonAssociate.IsEnabled = bOk && (model.CurrentIndiv.IsValid) && (model.CurrentPhotoData != null) && (model.CurrentPhotoData.Length > 1);
            this.buttonDissociate.IsEnabled = bOk && (model.CurrentIndiv.PhotoData != null) && (model.CurrentIndiv.PhotoData.Length > 1);
        }// myUpdateUI
        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            m_model = getModel();
            if (m_model != null)
            {
                m_model.PropertyChanged += m_model_PropertyChanged;
            }// m_model
        }

        void m_model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (m_busy)
            {
                return;
            }
            m_busy = true;
            String name = e.PropertyName;
            if (name == "WorkDone")
            {
                this.buttonCommitChanges.IsEnabled = true;
            }
            foreach (var s in TAB_PX)
            {
                if (s == name)
                {
                    myUpdateUI();
                    m_model = getModel();
                    if (m_model != null)
                    {
                        if (!m_model.IsBusy)
                        {
                            this.buttonCommitChanges.IsEnabled = m_model.IsModified;
                        }
                    }// m_model
                    break;
                }
            }
            m_busy = false;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var model = getModel();
            if (model == null)
            {
                return;
            }
            model.RefreshPhotos();
            myUpdateUI();
        }

        private void buttonCommitChanges_Click(object sender, RoutedEventArgs e)
        {
            var model = getModel();
            if (model == null)
            {
                return;
            }
            model.CommitChanges();
        }

        private void buttonPrev_Click(object sender, RoutedEventArgs e)
        {
            var model = getModel();
            if (model == null)
            {
                return;
            }
            model.Skip = model.Skip - model.Taken;
        }

        private void buttonNext_Click(object sender, RoutedEventArgs e)
        {
            var model = getModel();
            if (model == null)
            {
                return;
            }
            model.Skip = model.Skip + model.Taken;
        }

        private void buttonAssociate_Click(object sender, RoutedEventArgs e)
        {
            var model = getModel();
            if (model == null)
            {
                return;
            }
            model.Associate();
        }

        private void buttonDissociate_Click(object sender, RoutedEventArgs e)
        {
            var model = getModel();
            if (model == null)
            {
                return;
            }
            model.Dissociate();
        }


        private void listboxPhotos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var model = getModel();
            if (model == null)
            {
                return;
            }
            PhotoDesc p = null;
            Object obj = listboxPhotos.SelectedItem;
            if ((obj != null) && (obj is DisplayItems))
            {
                DisplayItems d = obj as DisplayItems;
                var oo = d.Tag;
                if ((oo != null) && (oo is PhotoDesc))
                {
                    p = oo as PhotoDesc;
                }
            }
            model.CurrentPhoto = p;
        }

        private void listboxIndivs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var model = getModel();
            if (model == null)
            {
                return;
            }
            DisplayItems disp = null;
            Object obj = listboxIndivs.SelectedItem;
            if ((obj != null) && (obj is DisplayItems))
            {
                disp = obj as DisplayItems;
            }
            model.CurrentDisplayIndiv = disp;
        }// getModel
    }// class PhotoAssocUserControl
}
