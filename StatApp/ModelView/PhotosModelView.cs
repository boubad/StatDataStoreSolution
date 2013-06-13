using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
/////////////////////////////////////
using System.IO;
using System.Drawing;
using System.Windows.Navigation;
//////////////////////////////
using MathNet.Numerics.Statistics;
using MathNet.Numerics.Distributions;
/////////////////////////
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
//////////////////////////////////
using StatData;
////////////////////////////////////
namespace StatApp.ModelView
{
    public class PhotosModelView : StatModelViewBase
    {
        private bool m_busy = false;
        private int m_count;
        private int m_skip = 0;
        private int m_taken = 20;
        private PhotoDescs m_photos;
        private PhotoDesc m_currentphoto;
        public PhotosModelView(IServiceLocator servicelocator, IPageLocator pageLocator, NavigationService pNav)
            : base(servicelocator, pageLocator, pNav)
        {
        }// PhotosModelView
        #region Properties
        public bool IsBusy
        {
            get
            {
                return m_busy;
            }
            set
            {
                if (value != m_busy)
                {
                    m_busy = value;
                    NotifyPropertyChanged("IsBusy");
                    NotifyPropertyChanged("IsNotNusy");
                    NotifyPropertyChanged("CanPrevPage");
                    NotifyPropertyChanged("CanNextPage");
                }
            }
        }// IsBusy
        public bool IsNotBusy
        {
            get
            {
                return !(this.IsBusy);
            }
            set
            {
                this.IsBusy = !value;
            }
        }// IsNotBusy
        public bool CanPrevPage
        {
            get
            {
                return (this.Skip > 0) && (!this.IsBusy);
            }
            set
            {
                NotifyPropertyChanged("CanPrevPage");
            }
        }// canPrevPage
        public bool CanNextPage
        {
            get
            {
                return (!this.IsBusy) &&  ((this.Skip + this.Taken) < this.TotalValuesCount);
            }
            set
            {
                NotifyPropertyChanged("CanNextPage");
            }
        }// CanBextPage
        public PhotoDesc CurrentPhoto
        {
            get
            {
                if (m_currentphoto == null)
                {
                    m_currentphoto = new PhotoDesc();
                }
                return m_currentphoto;
            }
            set
            {
                if (value != m_currentphoto)
                {
                    m_currentphoto = value;
                    NotifyPropertyChanged("CurrentPhoto");
                }
            }
        }// CurrentPhoto
        public PhotoDescs Photos
        {
            get
            {
                if (m_photos == null)
                {
                    m_photos = new PhotoDescs();
                }
                return m_photos;
            }
            set
            {
                if (value != m_photos)
                {
                    m_photos = value;
                    NotifyPropertyChanged("Photos");
                    NotifyPropertyChanged("ValuesStatus");
                    NotifyPropertyChanged("CanPrevPage");
                    NotifyPropertyChanged("CanNextPage");
                    this.CurrentPhoto = null;
                }
            }
        }// Photos
        public int TotalValuesCount
        {
            get
            {
                return m_count;
            }
            set
            {
                if (value != m_count)
                {
                    m_count = value;
                    NotifyPropertyChanged("TotalValuesCount");
                    NotifyPropertyChanged("ValuesStatus");
                }
            }
        }//TotalValusCount
        public String ValuesStatus
        {
            get
            {
                String sRet = "";
                var p = this.Photos;
                if ((p != null) && (m_count > 0))
                {
                    int nEnd = m_skip + m_taken;
                    if (nEnd > m_count)
                    {
                        nEnd = m_count;
                    }
                    if (nEnd < this.Photos.Count)
                    {
                        nEnd = this.Photos.Count;
                    }
                    sRet = String.Format("Images {0} à {1} sur {2}", m_skip + 1, nEnd, m_count);
                }
                return sRet;
            }
            set
            {
                NotifyPropertyChanged("ValuesStatus");
            }
        }// ValuesStatus
        public int Skip
        {
            get
            {
                return m_skip;
            }
            set
            {
                if ((value != m_skip) && (value >= 0))
                {
                    m_skip = value;
                    NotifyPropertyChanged("Skip");
                    this.RefreshPhotos();
                }
            }
        }// Skip
        public int Taken
        {
            get
            {
                return m_taken;
            }
            set
            {
                if ((value != m_taken) && (value >= 0))
                {
                    m_taken = value;
                    NotifyPropertyChanged("Taken");
                    this.RefreshPhotos();
                }
            }
        }// Taken
        #endregion // Properties
        #region Methods
        public void PrevPage()
        {
            int nSkip = this.Skip;
            if (nSkip > 0)
            {
                nSkip = nSkip - this.Taken;
                if (nSkip < 0)
                {
                    nSkip = 0;
                }
                this.Skip = nSkip;
            }
        }// PrevPage
        public void NextPage()
        {
            int nMax = this.TotalValuesCount;
            int nSkip = this.Skip;
            if ((nSkip + this.Taken) < nMax)
            {
                nSkip += this.Taken;
                this.Skip = nSkip;
            }
        }// NextPage
        public async void RefreshPhotos()
        {
            if (!this.IsBusy)
            {
                this.IsBusy = true;
                var xx = await ReadPhotosAsync();
                m_count = xx.Item1;
                this.Photos = xx.Item2;
                this.IsBusy = false;
            }
        }// RefreshPhotos
        public async void MaintainsPhoto()
        {
            if (this.IsBusy)
            {
                return;
            }
            var pMan = this.DataService;
            if (pMan == null)
            {
                return;
            }
            this.IsBusy = true;
            PhotoDesc oPhoto = this.CurrentPhoto;
            if ((oPhoto != null) && oPhoto.IsWriteable)
            {
                bool bAdd = (oPhoto.Id == 0);
                var xx = await Task.Run<Tuple<PhotoDesc, Exception>>(() =>
                {
                    return pMan.MaintainsPhoto(oPhoto);
                });
                this.NotifyPropertyChanged("CurrentPhoto");
                if ((xx != null) && (xx.Item1 != null) && (xx.Item2 == null))
                {
                    PhotoDesc p = xx.Item1;
                    if (bAdd)
                    {
                        List<PhotoDesc> ll = this.Photos.ToList();
                        ll.Add(p);
                        ll.Sort();
                        this.Photos = new PhotoDescs(ll);
                    }
                    PhotoDesc pp = null;
                    var q = from x in this.Photos where x.Id == p.Id select x;
                    if (q.Count() > 0)
                    {
                        pp = q.First();
                    }
                    this.CurrentPhoto = pp;
                    int n = this.Photos.Count;
                    if (n > this.TotalValuesCount)
                    {
                        this.TotalValuesCount = n;
                    }
                }
                else if ((xx != null) && (xx.Item2 != null))
                {
                    this.ShowError(xx.Item2);
                }
            }
            this.IsBusy = false;
        }// MaintainsPhotos
        public async void RemovePhoto()
        {
            if (this.IsBusy)
            {
                return;
            }
            var pMan = this.DataService;
            if (pMan == null)
            {
                return;
            }
            this.IsBusy = true;
            PhotoDesc oPhoto = this.CurrentPhoto;
            if ((oPhoto != null) && oPhoto.IsRemoveable)
            {
                var xx = await Task.Run<Tuple<bool, Exception>>(() =>
                {
                    return pMan.RemovePhoto(oPhoto);
                });
                if ((xx != null) && (xx.Item1) && (xx.Item2 == null))
                {
                    var cols = this.Photos;
                    var q = from x in cols where x.Id == oPhoto.Id select x;
                    if (q.Count() > 0)
                    {
                        cols.Remove(q.First());
                        this.CurrentPhoto = null;
                    }
                }
                else if ((xx != null) && (xx.Item2 != null))
                {
                    this.ShowError(xx.Item2);
                }
            }
            this.IsBusy = false;
        }// MaintainsPhotos
        public async void BrowsePhoto()
        {
            String filename = GetImportFilename();
            if (!String.IsNullOrEmpty(filename))
            {
                var xx = await Task.Run<Tuple<byte[], String, Exception>>(() => {
                    return GetJPEGBytes(filename);
                });
                byte[] data = xx.Item1;
                String name = xx.Item2;
                Exception err = xx.Item3;
                if ((data != null) && (data.Length > 1) && (err == null) && (!String.IsNullOrEmpty(name)))
                {
                    PhotoDesc oPhoto = this.CurrentPhoto;
                    this.CurrentPhoto = null;
                    oPhoto.DataBytes = data;
                    String sOld = oPhoto.Name;
                    if (String.IsNullOrEmpty(sOld))
                    {
                        oPhoto.Name = name;
                    }
                    this.CurrentPhoto = oPhoto;
                }
                else if (err != null)
                {
                    this.ShowError(err);
                }
            }
        }// BrowsePhoto
        #endregion // Methods
        #region Helpers
        public Task<Tuple<int, PhotoDescs>> ReadPhotosAsync()
        {
            return Task.Run<Tuple<int, PhotoDescs>>(() =>
            {
                PhotoDescs oRet = null;
                int nCount = 0;
                var pMan = this.DataService;
                if (pMan != null)
                {
                    var yy = pMan.GetPhotosCount();
                    if ((yy != null) && (yy.Item2 == null))
                    {
                        nCount = yy.Item1;
                        if (nCount > 0)
                        {
                            var xx = pMan.GetPhotos(this.Skip, this.Taken);
                            if ((xx != null) && (xx.Item1 != null) && (xx.Item2 == null))
                            {
                                oRet = new PhotoDescs(xx.Item1);
                            }
                        }
                        else
                        {
                            oRet = new PhotoDescs();
                        }
                    }// yy
                }// pMan
                return new Tuple<int, PhotoDescs>(nCount, oRet);
            });
        }// ReadPhotosAsync
        public String GetImportFilename()
        {
            String ext = ".jpg";
            String filter = "Images JPEG (*.jpg)|*.jpg|Images JPEG (*.jpeg)|*.jpeg";
            return GetImportFilename(ext, filter);
        }// ImportPhoto
        public static Tuple<byte[], String,Exception> GetJPEGBytes(String filename)
        {
            byte[] data = null;
            String name = null;
            Exception err = null;
            try
            {
                if ((!String.IsNullOrEmpty(filename)) && File.Exists(filename))
                {
                    FileInfo info = new FileInfo(filename);
                    int nSize = (int)info.Length;
                    name = info.Name;
                    if (nSize > 1)
                    {
                        using (var fs = new FileStream(filename, FileMode.Open))
                        {
                            byte[] dd = new byte[nSize];
                            int nRet = fs.Read(dd, 0, nSize);
                            if (nRet == nSize)
                            {
                                data = dd;
                            }
                        }// fs
                    }// nSize
                }// exists
            }// try
            catch (Exception ex)
            {
                err = ex;
            }
            return new Tuple<byte[], string,Exception>(data, name,err);
        }//GetJPEGBytes 
        #endregion // Helpers
    }// class PhotosModelView 
}
