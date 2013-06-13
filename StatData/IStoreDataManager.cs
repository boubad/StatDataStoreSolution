using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
//
namespace StatData
{
    public interface IStoreDataManager
    {
        Tuple<IEnumerable<StatDataSet>, Exception> GetAllStatDataSets();
        Tuple<StatDataSet, Exception> GetDataSet(StatDataSet oSet);
        Tuple<StatDataSet, Exception> MaintainsDataSet(StatDataSet oSet);
        Tuple<bool, Exception> RemoveDataSet(StatDataSet oSet);
        Tuple<IEnumerable<VariableDesc>, Exception> GetDataSetVariables(StatDataSet oSet);
        Tuple<IEnumerable<VariableDesc>, Exception> GetDataSetVariablesAndData(StatDataSet oSet);
        Tuple<IndivDescs, VariableDescs, Exception> FetchAllDataSetData(StatDataSet oSet);
        Tuple<VariableDesc, Exception> MaintainsVariable(VariableDesc oVar);
        Tuple<bool, Exception> MaintainsVariableAndValues(IEnumerable<VariableDesc> oVars);
        Tuple<bool, Exception> RemoveVariable(VariableDesc oVar);
        Tuple<int, Exception> GetVariableValuesCount(VariableDesc oVar);
        Tuple<IEnumerable<ValueDesc>, Exception> GetVariableValues(VariableDesc oVar, int skip, int taken);
        Tuple<bool, Exception> WriteValues(IEnumerable<ValueDesc> vals);
        Tuple<bool, Exception> RemoveValues(IEnumerable<ValueDesc> vals);
        Tuple<VariableInfo, Exception> GetVariableInfo(VariableDesc oVar);
        Tuple<IEnumerable<int>, Exception> GetIndexes(IEnumerable<VariableDesc> oVars);
        Tuple<Dictionary<VariableDesc, Dictionary<int, ValueDesc>>, Exception> ReadValues(IEnumerable<VariableDesc> oVars, IEnumerable<int> oIndexes);
        Tuple<Dictionary<VariableDesc, double[]>, Exception> ReadNumValues(IEnumerable<VariableDesc> oVars);
        Tuple<Dictionary<String, List<ValueDesc> >, Exception> ReadCategValues(VariableDesc oCategVar, VariableDesc oDataVar);
        Tuple<Dictionary<String, double[]>, Exception> ReadNumCategValues(VariableDesc oCategVar, VariableDesc oDataVar);
        Tuple<Dictionary<String, Dictionary<VariableDesc, List<ValueDesc>>>, Exception> ReadCategValues(VariableDesc oCategVar, IEnumerable<VariableDesc> oDataVars);
        Tuple<Dictionary<String, Dictionary<VariableDesc, double[]>>, Exception> ReadNumCategValues(VariableDesc oCategVar, IEnumerable<VariableDesc> oDataVars);
        Tuple<int, Exception> GetPhotosCount();
        Tuple<IEnumerable<PhotoDesc>, Exception> GetPhotos(int skip, int taken);
        Tuple<PhotoDesc, Exception> FindPhoto(PhotoDesc oPhoto);
        Tuple<IEnumerable<PhotoDesc>, Exception> GetPhotos(IEnumerable<PhotoDesc> oPhotos);
        Tuple<IEnumerable<PhotoDesc>, Exception> SearchPhotos(String searchString,int skip,int taken);
        Tuple<int, Exception> SearchPhotosCount(String searchString);
        Tuple<PhotoDesc, Exception> MaintainsPhoto(PhotoDesc oPhoto);
        Tuple<bool, Exception> RemovePhoto(PhotoDesc oPhoto);
        Tuple<IEnumerable<int>, Exception> GetDataSetIndexes(StatDataSet oSet);
        Tuple<IEnumerable<ValueDesc>, Exception> GetDataSetIndivValues(StatDataSet oSet, int iIndex);
        Tuple<bool, Exception> ReplaceDataSet(StatDataSet oSet, CancellationToken token, IProgress<int> progress);
        Tuple<IndivDescs, Exception> GetDataSetIndivs(StatDataSet oSet);
        Tuple<IndivDesc, Exception> GetDataSetIndiv(StatDataSet oSet, int indivIndex);
    }// IStoreDataManager
}
