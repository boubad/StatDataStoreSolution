using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
////////////////////////////////
using StatData;
//////////////////////////////////
namespace StatApp
{
    public interface IServiceLocator
    {
        IStoreDataManager GetDataService();
    }// interface IServiceLocator
}
