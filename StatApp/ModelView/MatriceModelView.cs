using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.ObjectModel;
/////////////////////////////////////
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
    public class MatriceModelView : StatModelViewBase
    {
        private OrdModelView m_main;
        private bool m_busy = false;
         public MatriceModelView(OrdModelView pMain)
            : base(pMain.ServiceLocator, pMain.PageLocator, pMain.NavigationService)
        {
            m_main = pMain;
            m_main.PropertyChanged += m_main_PropertyChanged;
        }

         void m_main_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
         {
             if (!m_busy)
             {
                 return;
             }
             m_busy = true;
             String name = e.PropertyName;
             if (name == "IsBusy")
             {
                 NotifyPropertyChanged("IsBusy");
                 return;
             }
             if (name == "SortedIndivs")
             {
                 this.indivSortChanged();
             }
             m_busy = false;
         }
        #region Helpers
         private void indivSortChanged()
         {
         }// indivSortCganged
        #endregion // Helpers
    }// class  MatriceModelView 
}
