using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
///////////////////////////////
using StatData;
/////////////////////////////////////////
using StatApp.Data;
////////////////////////////////////////
namespace StatApp
{
    public class ServiceLocator : IServiceLocator
    {
        #region static variables
        private static ServiceLocator m_locator = new ServiceLocator();
        #endregion // sttaic variables
        #region Instance variables
        private DomainDataManager m_datamanager = new DomainDataManager();
        #endregion // instance variables
        #region Constructors
        #endregion // Constrictore
        #region static methods
        public static ServiceLocator GetInstance()
        {
            return m_locator;
        }// GetInstance
        #endregion
        #region instance methods
        public IStoreDataManager GetDataService()
        {
            return m_datamanager;
        }
        #endregion // instance Methods
    }//class ServiceLocatorImpl
}
