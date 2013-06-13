using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace StatData
{
    [Serializable]
    public class StatDataSet : NamedElement
    {
        #region Instance variables
        private bool m_busy = false;
        private int m_lastindex;
        private VariableDescs m_variables;
        private IndivDescs m_indivs;
        #endregion // instance variables
        #region Constructors
        public StatDataSet()
        {
            m_lastindex = -1;
        }
        #endregion // Construcyors
        #region Properties
        public bool IsDone
        {
            get
            {
                return !IsBusy;
            }
            set
            {
                NotifyPropertyChanged("IsDone");
            }
        }// isDone
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
                }
            }
        }// IsBusy
        public bool HasIndivs
        {
            get
            {
                return (this.Individus != null) && (this.Individus.Count > 0);
            }
            set { }
        }// HasIndivs
        public IndivDescs Individus
        {
            get
            {
                if (m_indivs == null)
                {
                    m_indivs = new IndivDescs();
                }
                return m_indivs;
            }
            set
            {
                if (value != m_indivs)
                {
                    m_indivs = value;
                    NotifyPropertyChanged("Individus");
                    NotifyPropertyChanged("HasIndivs");
                }
            }
        }// Individus
        public int LastIndex
        {
            get
            {
                return m_lastindex;
            }
            set
            {
                if (value != m_lastindex)
                {
                    m_lastindex = value;
                    NotifyPropertyChanged("LastIndex");
                    this.IsModified = true;
                }
            }
        }// LastIndex
        public VariableDescs Variables
        {
            get
            {
                if (m_variables == null)
                {
                    m_variables = new VariableDescs();
                }
                return m_variables;
            }
            set
            {
                if (value != m_variables)
                {
                    m_variables = value;
                    NotifyPropertyChanged("Variables");
                    NotifyPropertyChanged("HasVariables");
                    this.IsModified = true;
                }
            }
        }
        public bool HasVariables
        {
            get
            {
                return (this.Variables != null) && (this.Variables.Count > 0);
            }
            set
            {
            }
        }// HasVariables
        #endregion // Properties
        #region Methods
        public async void Refresh(IStoreDataManager pMan)
        {
            if (pMan == null)
            {
                return;
            }
            if (!this.IsValid)
            {
                return;
            }
            if (this.IsBusy)
            {
                return;
            }
            var oSet = this;
            this.IsBusy = true;
            var tt = await Task.Run<Tuple<IndivDescs, VariableDescs, Exception>>(() =>
            {
                return pMan.FetchAllDataSetData(oSet);
            });
            this.IsBusy = false;
            if ((tt != null) && (tt.Item3 == null))
            {
                this.Individus = tt.Item1;
                this.Variables = tt.Item2;
                this.IsDone = true;
            }
        }// Refresh
        public async void Maintains(IStoreDataManager pMan)
        {
            if (pMan == null)
            {
                return;
            }
            if (!this.IsWriteable)
            {
                return;
            }
            if (this.IsBusy)
            {
                return;
            }
            var oSet = this;
            this.IsBusy = true;
            var tt = await Task.Run<Tuple<StatDataSet, Exception>>(() => {
                return pMan.MaintainsDataSet(oSet);
            });
            this.IsBusy = false;
            if ((tt != null) && (tt.Item1 != null) && (tt.Item2 == null))
            {
                var xSet = tt.Item1;
                this.Id = xSet.Id;
                this.LastIndex = xSet.LastIndex;
                this.Name = xSet.Name;
                this.Description = xSet.Description;
                this.IsDone = true;
            }
        }// Maintains
        public async void MaintainsVariable(VariableDesc oVar, IStoreDataManager pMan)
        {
            if ((oVar == null) || (pMan == null))
            {
                return;
            }
            oVar.DataSetId = this.Id;
            if (!oVar.IsWriteable)
            {
                return;
            }
            this.IsBusy = true;
            var tt = await Task.Run<Tuple<VariableDesc, Exception>>(() =>
            {
                return pMan.MaintainsVariable(oVar);
            });
            if ((tt != null) && (tt.Item1 != null) && (tt.Item2 == null))
            {
                var xVar = tt.Item1;
                oVar.Id = xVar.Id;
                oVar.Name = xVar.Name;
                oVar.DataSetId = xVar.DataSetId;
                oVar.DataType = xVar.DataType;
                oVar.Description = xVar.Description;
                oVar.IsCategVar = xVar.IsCategVar;
                oVar.IsIdVar = xVar.IsIdVar;
                oVar.IsImageVar = xVar.IsImageVar;
                oVar.IsInfoVar = xVar.IsInfoVar;
                oVar.IsNameVar = xVar.IsNameVar;
            }
            this.IsBusy = false;
        }// Maintains
        #endregion // Methods
    }// class StatDataSet
    [Serializable]
    public class StatDataSets : ObservableCollection<StatDataSet>
    {
        public StatDataSets()
            : base()
        {
        }
        public StatDataSets(IEnumerable<StatDataSet> col)
            : base(col)
        {
        }
    }// class StatDataSets
}
