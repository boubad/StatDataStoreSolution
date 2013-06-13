using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StatData;
using StatApp.Controles;
namespace StatApp.ModelView
{
    public class PhotosAssocModelView : StatModelViewBase
    {
        private bool m_busy = false;
        private bool m_modified = false;
        private MainModelView m_main;
        private DisplayItemsArray m_photos;
        private PhotoDesc m_currentphoto;
        private IndivData m_currentindiv;
        private int m_ntotalphotos;
        private int m_nskip;
        private int m_ntaken = 20;
        private String m_search;
        private IndivDatas m_indivs;
        private VariableDesc m_idvar;
        private VariableDesc m_namevar;
        private VariableDesc m_photovar;
        private String m_idsvarname = "IDS";
        private String m_namesvarname = "NOMS";
        private String m_photosvarname = "PHOTOS";
        private DisplayItemsArray m_displayindivs;
        private DisplayItems m_currentdisplay;
        //
        public PhotosAssocModelView(MainModelView pMain)
            : base(pMain.ServiceLocator, pMain.PageLocator, pMain.NavigationService)
        {
            m_main = pMain;
            m_main.PropertyChanged += m_main_PropertyChanged;
        }
        void m_main_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            String name = e.PropertyName;
            if (name == "IsBusy")
            {
                NotifyPropertyChanged("IsBusy");
                return;
            }
            if (name == "CurrentStatDataSet")
            {
                NotifyPropertyChanged("DataSetName");
            }
            else if (name == "AllIndividus")
            {
                this.refreshIndivs();
            }
            else if (name == "Variables")
            {
                if (!m_busy)
                {
                    this.refreshVariables();
                }
            }
        }
        #region Properties
        public bool WorkDone
        {
            get
            {
                return true;
            }
            set
            {
                NotifyPropertyChanged("WorkDone");
            }
        }// WorkDone
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
                }
            }
        }// IsModified
        public int CurrentIndex
        {
            get
            {
                return this.CurrentIndiv.IndivIndex;
            }
            set
            {
                NotifyPropertyChanged("CurrentIndex");
            }
        }// CurrentIndex
        public String CurrentIdString
        {
            get
            {
                return this.CurrentIndiv.IdString;
            }
            set
            {
                String old = this.CurrentIndiv.IdString;
                String s = String.IsNullOrEmpty(value) ? String.Empty : value.Trim();
                if (s != old)
                {
                    this.CurrentIndiv.IdString = s;
                    modifyIndiv(this.CurrentIndiv,false);
                    NotifyPropertyChanged("CurrentIdString");
                    this.IsModified = true;
                }
            }
        }// CurrentIdsString
        public String CurrentNameString
        {
            get
            {
                return this.CurrentIndiv.Name;
            }
            set
            {
                String old = this.CurrentIndiv.Name;
                String s = String.IsNullOrEmpty(value) ? String.Empty : value.Trim();
                if (s != old)
                {
                    this.CurrentIndiv.Name = s;
                    modifyIndiv(this.CurrentIndiv,false);
                    NotifyPropertyChanged("CurrentNameString");
                    this.IsModified = true;
                }
            }
        }// CurrentIdsString
        public byte[] CurrentPhotoData
        {
            get
            {
                return this.CurrentPhoto.DataBytes;
            }
            set
            {
                NotifyPropertyChanged("CurrentPhotoData");
            }
        }// CurrentPhotoData
        public byte[] IndivCurrentPhotoData
        {
            get
            {
                return this.CurrentIndiv.PhotoData;
            }
            set
            {
                NotifyPropertyChanged("IndivCurrentPhotoData");
            }
        }// IndivCurrentPhotoData
        public String DataSetName
        {
            get
            {
                return m_main.CurrentStatDataSet.Name;
            }
            set
            {
                NotifyPropertyChanged("DataSetName");
            }
        }// DataSetName
        public DisplayItems CurrentDisplayIndiv
        {
            get
            {
                if (m_currentdisplay == null)
                {
                    m_currentdisplay = new DisplayItems();
                }
                return m_currentdisplay;
            }
            set
            {
                if (value != m_currentdisplay)
                {
                    m_currentdisplay = value;
                    this.CurrentIndiv = null;
                    IndivData cur = null;
                    if (m_currentdisplay != null)
                    {
                        Object obj = m_currentdisplay.Tag;
                        if ((obj != null) && (obj is IndivData))
                        {
                            cur = obj as IndivData;
                        }
                    }
                    this.CurrentIndiv = cur;
                    NotifyPropertyChanged("CurrentIndex");
                    NotifyPropertyChanged("CurrentIdString");
                    NotifyPropertyChanged("CurrentNameString");
                    NotifyPropertyChanged("IndivCurrentPhotoData");
                }
            }
        }// CurrentDisplayIndiv
        public DisplayItemsArray DisplayIndivs
        {
            get
            {
                if (m_displayindivs == null)
                {
                    m_displayindivs = new DisplayItemsArray();
                }
                return m_displayindivs;
            }
            set
            {
                if (value != m_displayindivs)
                {
                    m_displayindivs = value;
                    NotifyPropertyChanged("DisplayIndivs");
                }
            }
        }// DisplayIndivs
        public bool IsBusy
        {
            get
            {
                return m_busy || m_main.IsBusy;
            }
            set
            {
                if (value != m_busy)
                {
                    m_busy = value;
                    NotifyPropertyChanged("IsBusy");
                }
            }
        }// IsBusy
        public String IdsVarName
        {
            get
            {
                return m_idsvarname;
            }
            set
            {
                String old = String.IsNullOrEmpty(m_idsvarname) ? String.Empty : m_idsvarname.Trim();
                String s = String.IsNullOrEmpty(value) ? String.Empty : value.Trim();
                if (s != value)
                {
                    m_idsvarname = value;
                    NotifyPropertyChanged("IdsVarName");
                }
            }
        }// IdsVarName
        public String NamesVarName
        {
            get
            {
                return m_namesvarname;
            }
            set
            {
                String old = String.IsNullOrEmpty(m_namesvarname) ? String.Empty : m_namesvarname.Trim();
                String s = String.IsNullOrEmpty(value) ? String.Empty : value.Trim();
                if (s != value)
                {
                    m_namesvarname = value;
                    NotifyPropertyChanged("NamesVarName");
                }
            }
        }// NamesVarName
        public String PhotosVarName
        {
            get
            {
                return m_photosvarname;
            }
            set
            {
                String old = String.IsNullOrEmpty(m_photosvarname) ? String.Empty : m_photosvarname.Trim();
                String s = String.IsNullOrEmpty(value) ? String.Empty : value.Trim();
                if (s != value)
                {
                    m_photosvarname = value;
                    NotifyPropertyChanged("PhotosVarName");
                }
            }
        }// PhotosVarName
        public IndivData CurrentIndiv
        {
            get
            {
                if (m_currentindiv == null)
                {
                    m_currentindiv = new IndivData();
                }
                return m_currentindiv;
            }
            set
            {
                if (value != m_currentindiv)
                {
                    m_currentindiv = value;
                    NotifyPropertyChanged("CurrentIndiv");
                    NotifyPropertyChanged("CurrentIndex");
                    NotifyPropertyChanged("CurrentIdString");
                    NotifyPropertyChanged("CurrentNameString");
                    NotifyPropertyChanged("IndivCurrentPhotoData");
                }
            }
        }// CurrentIndiv
        public IndivDatas Individus
        {
            get
            {
                if (m_indivs == null)
                {
                    m_indivs = new IndivDatas();
                }
                return m_indivs;
            }
            set
            {
                if (value != m_indivs)
                {
                    m_indivs = value;
                    NotifyPropertyChanged("Indivius");
                    this.CurrentIndiv = null;
                }
            }
        }// Individus
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
                    NotifyPropertyChanged("CurrentPhotoData");
                }
            }
        }// CurrentPhoto
        public DisplayItemsArray Photos
        {
            get
            {
                if (m_photos == null)
                {
                    m_photos = new DisplayItemsArray();
                }
                return m_photos;
            }
            set
            {
                if (value != m_photos)
                {
                    m_photos = value;
                    NotifyPropertyChanged("Photos");
                    NotifyPropertyChanged("PhotoStatusString");
                    this.CurrentPhoto = null;
                }
            }
        }// Photos
        public String PhotosStatusString
        {
            get
            {
                String sRet = "";
                int nMax = this.TotalPhotosCount;
                if ((nMax > 0) && (this.Photos.Count > 0))
                {
                    int nEnd = this.Skip + this.Photos.Count;
                    sRet = String.Format("Photos {0} à {1} sur {2}", this.Skip + 1, nEnd, nMax);
                }
                return sRet;
            }
            set {
                NotifyPropertyChanged("PhotosStatusString");
            }
        }// PhotosStatusString
        public int TotalPhotosCount
        {
            get
            {
                return m_ntotalphotos;
            }
            set
            {
                if ((value != m_ntotalphotos) && (value >= 0))
                {
                    m_ntotalphotos = value;
                    NotifyPropertyChanged("TotalPhotosCount");
                    NotifyPropertyChanged("PhotosStatusString");
                }
            }
        }// Skip
        public int Skip
        {
            get
            {
                return m_nskip;
            }
            set
            {
                if ((value != m_nskip) && (value >= 0))
                {
                    m_nskip = value;
                    this.RefreshPhotos();
                }
            }
        }// Skip
        public int Taken
        {
            get
            {
                return m_ntaken;
            }
            set
            {
                if ((value != m_ntaken) && (value >= 0))
                {
                    m_ntaken = value;
                    this.RefreshPhotos();
                }
            }
        }// Skip
        public String SearchString
        {
            get
            {
                return m_search;
            }
            set
            {
                String old = String.IsNullOrEmpty(m_search) ? String.Empty : m_search.Trim();
                String s = String.IsNullOrEmpty(value) ? String.Empty : value.Trim();
                if (old.ToLower() == s.ToLower())
                {
                    m_search = value;
                    m_nskip = 0;
                    NotifyPropertyChanged("Skip");
                    NotifyPropertyChanged("SearchString");
                    this.RefreshPhotos();
                }
            }
        }// SearcString
        #endregion // Properties
        #region Methods
        public void Associate()
        {
            var v = this.CurrentDisplayIndiv;
            IndivData ind = null;
            if (v != null)
            {
                Object obj = v.Tag;
                if ((obj != null) && (obj is IndivData))
                {
                    ind = obj as IndivData;
                }
            }// v
            var photo = this.CurrentPhoto;
            if ((ind != null) && (photo != null) && (ind.IndivIndex >= 0) && (photo.Id != 0) && (photo.DataBytes != null) && (photo.DataBytes.Length > 1))
            {
                ind.PhotoId = photo.Id;
                ind.PhotoData = photo.DataBytes;
                ind.PhotoName = photo.Name;
                modifyIndiv(ind,true);
                var old = this.DisplayIndivs;
                var oldc = v;
                this.DisplayIndivs = null;
                this.DisplayIndivs = old;
                this.CurrentDisplayIndiv = null;
                this.CurrentDisplayIndiv = oldc;
                this.IsModified = true;
            }
        }// Associate
        public void Dissociate()
        {
            var v = this.CurrentDisplayIndiv;
            IndivData ind = null;
            if (v != null)
            {
                Object obj = v.Tag;
                if ((obj != null) && (obj is IndivData))
                {
                    ind = obj as IndivData;
                }
            }// v
            if (ind != null)
            {
                ind.PhotoId = 0;
                ind.PhotoData = null;
                modifyIndiv(ind,true);
                var old = this.DisplayIndivs;
                var oldc = v;
                this.DisplayIndivs = null;
                this.DisplayIndivs = old;
                this.CurrentDisplayIndiv = null;
                this.CurrentDisplayIndiv = oldc;
                this.IsModified = true;
            }
        }// Dissociate
        public async  void CommitChanges()
        {
            var pMan = this.DataService;
            if (pMan == null)
            {
                return;
            }
            var oSet = m_main.CurrentStatDataSet;
            if (oSet == null)
            {
                return;
            }
            int nSetId = oSet.Id;
            if (nSetId == 0)
            {
                return;
            }
            this.IsBusy = true;
            try
            {
                VariableDesc vIds = m_idvar;
                if (vIds == null)
                {
                    String sName = this.IdsVarName;
                    if (!String.IsNullOrEmpty(sName))
                    {
                        vIds = new VariableDesc();
                        vIds.DataSetId = nSetId;
                        vIds.Name = sName;
                        vIds.IsCategVar = true;
                        vIds.IsIdVar = true;
                        vIds.IsInfoVar = true;
                        vIds.DataType = "string";
                    }
                }
                VariableDesc vNames = m_namevar;
                if (vNames == null)
                {
                    String sName = this.NamesVarName;
                    if (!String.IsNullOrEmpty(sName))
                    {
                        vNames = new VariableDesc();
                        vNames.DataSetId = nSetId;
                        vNames.Name = sName;
                        vNames.IsCategVar = true;
                        vNames.IsNameVar = true;
                        vNames.IsInfoVar = true;
                        vNames.DataType = "string";
                    }
                }
                VariableDesc vPhotos = m_photovar;
                if (vPhotos == null)
                {
                    String sName = this.PhotosVarName;
                    if (!String.IsNullOrEmpty(sName))
                    {
                        vPhotos = new VariableDesc();
                        vPhotos.DataSetId = nSetId;
                        vPhotos.Name = sName;
                        vPhotos.IsCategVar = false;
                        vPhotos.IsNumVar = true;
                        vPhotos.IsImageVar = true;
                        vNames.IsInfoVar = true;
                        vPhotos.DataType = "int";
                    }
                }
                var inds = this.Individus;
                foreach (var ind in inds)
                {
                    int index = ind.IndivIndex;
                    if (index < 0)
                    {
                        continue;
                    }
                    if (vIds != null)
                    {
                        if (vIds.Id != 0)
                        {
                            var q = from x in vIds.Values where x.Index == index select x;
                            if (q.Count() > 0)
                            {
                                var vx = q.First();
                                vx.DataStringValue = ind.IdString;
                                vx.IsModified = true;
                            }
                            else
                            {
                                var vx = new ValueDesc();
                                vx.VariableId = vIds.Id;
                                vx.Index = index;
                                vx.DataStringValue = ind.IdString;
                                vx.IsModified = true;
                                vIds.Values.Add(vx);
                            }
                        }
                        else
                        {
                            var vx = new ValueDesc();
                            vx.Index = index;
                            vx.DataStringValue = ind.IdString;
                            vx.IsModified = true;
                            vIds.Values.Add(vx);
                        }
                    }// Ids
                    //
                    if (vNames != null)
                    {
                        if (vNames.Id != 0)
                        {
                            var q = from x in vNames.Values where x.Index == index select x;
                            if (q.Count() > 0)
                            {
                                var vx = q.First();
                                vx.DataStringValue = ind.Name;
                                vx.IsModified = true;
                            }
                            else
                            {
                                var vx = new ValueDesc();
                                vx.VariableId = vNames.Id;
                                vx.Index = index;
                                vx.DataStringValue = ind.Name;
                                vx.IsModified = true;
                                vNames.Values.Add(vx);
                            }
                        }
                        else
                        {
                            var vx = new ValueDesc();
                            vx.Index = index;
                            vx.DataStringValue = ind.Name;
                            vx.IsModified = true;
                            vNames.Values.Add(vx);
                        }
                    }// Names
                    //
                    if (vPhotos != null)
                    {
                        if (vPhotos.Id != 0)
                        {
                            var q = from x in vPhotos.Values where x.Index == index select x;
                            if (q.Count() > 0)
                            {
                                var vx = q.First();
                                if (vPhotos.DataType == "string")
                                {
                                    vx.DataStringValue = ind.PhotoName;
                                }
                                else
                                {
                                    vx.DataStringValue = Convert.ToString(ind.PhotoId);
                                }
                                vx.IsModified = true;
                            }
                            else
                            {
                                var vx = new ValueDesc();
                                vx.VariableId = vPhotos.Id;
                                vx.Index = index;
                                if (vPhotos.DataType == "string")
                                {
                                    vx.DataStringValue = ind.PhotoName;
                                }
                                else
                                {
                                    vx.DataStringValue = Convert.ToString(ind.PhotoId);
                                }
                                vx.IsModified = true;
                                vPhotos.Values.Add(vx);
                            }
                        }
                        else
                        {
                            var vx = new ValueDesc();
                            vx.Index = index;
                            if (vPhotos.DataType == "string")
                            {
                                vx.DataStringValue = ind.PhotoName;
                            }
                            else
                            {
                                vx.DataStringValue = Convert.ToString(ind.PhotoId);
                            }
                            vx.IsModified = true;
                            vPhotos.Values.Add(vx);
                        }
                    }// Names
                }// ind
                List<VariableDesc> oVars = new List<VariableDesc>();
                if (vIds != null)
                {
                    oVars.Add(vIds);
                }// ids
                if (vNames != null)
                {
                    oVars.Add(vNames);
                }
                if (vPhotos != null)
                {
                    oVars.Add(vPhotos);
                }
                if (oVars.Count > 0)
                {
                    var xx = await Task.Run<Tuple<bool, Exception>>(() =>
                    {
                        return pMan.MaintainsVariableAndValues(oVars);
                    });
                    if ((xx != null) && (xx.Item2 != null))
                    {
                        ShowError(xx.Item2);
                    }
                    else if ((xx != null) && (xx.Item1) && (xx.Item2 == null))
                    {
                        m_main.RefreshVariables();
                       
                    }
                }// oVars
            }// try
            catch (Exception ex)
            {
                ShowError(ex);
            }
            this.IsBusy = false;
            this.IsModified = false;
        }// CommitChanges
        #endregion // Methods
        #region Helpers
        private Tuple<int, DisplayItemsArray> refreshPhotosAsync(IStoreDataManager pMan)
        {
            DisplayItemsArray oRet = null;
            int nRet = 0;
            String s = this.SearchString;
            var xx = pMan.SearchPhotosCount(s);
            if ((xx != null) && (xx.Item2 == null))
            {
                nRet = xx.Item1;
            }
            var yy = pMan.SearchPhotos(s, this.Skip, this.Taken);
            if ((yy != null) && (yy.Item2 == null))
            {
                var col = yy.Item1;
                oRet = new DisplayItemsArray();
                foreach (var v in col)
                {
                    DisplayItems vv = new DisplayItems();
                    vv.Tag = v;
                    vv.Add(new DisplayItem(v.DataBytes));
                    vv.Add(new DisplayItem(v.Name));
                    oRet.Add(vv);
                }// v
            }
            return new Tuple<int, DisplayItemsArray>(nRet, oRet);
        }// refreshPhotosAsync
        public async void RefreshPhotos()
        {
            var pMan = this.DataService;
            var xx = await Task.Run<Tuple<int, DisplayItemsArray>>(() =>
            {
                return refreshPhotosAsync(pMan);
            });
            this.TotalPhotosCount = xx.Item1;
            this.Photos = xx.Item2;
        }// RefreshPhotos
        public void refreshIndivs()
        {
            if (m_busy)
            {
                return;
            }
            m_busy = true;
            this.refreshVariables();
            var oRet = new List<IndivData>();
            var col = m_main.AllIndividus;
            DisplayItemsArray oDisp = new DisplayItemsArray();
            foreach (var ind in col)
            {
                int index = ind.IndivIndex;
                var vv = new IndivData(ind);
                oRet.Add(vv);
                DisplayItems dd = new DisplayItems();
                dd.Tag = vv;
                dd.Add(new DisplayItem(vv.IndivIndex));
                String sz = vv.IdString;
                dd.Add(new DisplayItem(sz));
                dd.Add(new DisplayItem(vv.Name));
                if ((vv.PhotoData != null) && (vv.PhotoData.Length > 1))
                {
                    dd.Add(new DisplayItem(vv.PhotoData));
                }
                else
                {
                    dd.Add(new DisplayItem());
                }
                oDisp.Add(dd);
            }// ind
            if (oRet.Count > 1)
            {
                oRet.Sort();
            }
            this.Individus = new IndivDatas(oRet);
            this.DisplayIndivs = oDisp;
            this.IsModified = false;
            m_busy = false;
            NotifyPropertyChanged("WorkDone");
        }// refreshIndivs
        private void refreshVariables()
        {
            m_idvar = null;
            m_namevar = null;
            m_photovar = null;
            var col = m_main.Variables;
            foreach (var v in col)
            {
                if (v.IsIdVar)
                {
                    m_idvar = v;
                    this.IdsVarName = v.Name;
                }
                if (v.IsNameVar)
                {
                    m_namevar = v;
                    this.NamesVarName = v.Name;
                }
                if (v.IsImageVar)
                {
                    m_photovar = v;
                    this.PhotosVarName = v.Name;
                }
            }// v
            if (m_idsvarname == m_namesvarname)
            {
                m_idsvarname = null;
                this.IdsVarName = null;
            }
        }
        private void modifyIndiv(IndivData ind,bool bImage)
        {
            int index = ind.IndivIndex;
            foreach (var dd in this.DisplayIndivs)
            {
                var obj = dd.Tag;
                if ((obj != null) && (obj is IndivData))
                {
                    IndivData dx = obj as IndivData;
                    if (dx.IndivIndex == index)
                    {
                        var col = dd.ToArray();
                        col[1].StringValue = ind.IdString;
                        col[2].StringValue = ind.Name;
                        if (bImage)
                        {
                            if ((ind.PhotoData != null) && (ind.PhotoData.Length > 1))
                            {
                                col[3].DisplayType = DisplayItemType.eDisplayImage;
                                col[3].DataBytes = ind.PhotoData;
                            }
                            else
                            {
                                col[3].DisplayType = DisplayItemType.eDisplayDefault;
                            }
                        }// bImage
                        break;
                    }// found
                }
            }// dd
        }// ModifyIndex
        #endregion // Helpers
    }// class PhotosAssocModelView
}
