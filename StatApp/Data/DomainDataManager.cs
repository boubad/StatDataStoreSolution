using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
//
using StatData;
//
using MathNet.Numerics.Statistics;
//
namespace StatApp.Data
{
    public class DomainDataManager : IStoreDataManager
    {
        #region static variables
        private static readonly String TAB_DATASET = "DataSet";
        private static readonly String TAB_VARIABLE = "Variable";
        private static readonly String TAB_VALEUR = "Valeur";
        private static readonly String TAB_PHOTO = "Photo";
        #endregion // static variables
        //
        public DomainDataManager()
        {
        }// DomainDataManager
        #region Helpers
        protected static double myconvert(double x)
        {
            double xx = 10000.0 * x;
            if (x < 0.0)
            {
                xx -= 0.5;
            }
            else
            {
                xx += 0.5;
            }
            int nx = (int)xx;
            return (double)(nx / 10000.0);
        }// myconvert
        protected statdataEntities getContext()
        {
            return new statdataEntities();
        }// getContext
        protected int[] nextIds(statdataEntities ctx, String name, int count)
        {
            int nRet = 0;
            string sname = name.Trim().ToLower();
            var q = from x in ctx.DbKeys where x.Nom.Trim().ToLower() == sname select x;
            if (q.Count() > 0)
            {
                DbKey p = q.First();
                nRet = p.Valeur;
                p.Valeur = nRet + count;
            }
            else
            {
                DbKey p = new DbKey();
                p.Nom = sname;
                p.Valeur = count + 1;
                ctx.DbKeys.Add(p);
                ctx.SaveChanges();
                nRet = 1;
            }
            int[] pRet = new int[count];
            for (int i = 0; i < count; ++i)
            {
                pRet[i] = nRet + i;
            }
            return pRet;
        }// nextIds
        protected int nextId(statdataEntities ctx, String name)
        {
            int nRet = 0;
            string sname = name.Trim().ToLower();
            var q = from x in ctx.DbKeys where x.Nom.Trim().ToLower() == sname select x;
            if (q.Count() > 0)
            {
                DbKey p = q.First();
                nRet = p.Valeur;
                p.Valeur = nRet + 1;
            }
            else
            {
                DbKey p = new DbKey();
                p.Nom = sname;
                p.Valeur = 2;
                ctx.DbKeys.Add(p);
                ctx.SaveChanges();
                nRet = 1;
            }
            return nRet;
        }// nextId
        protected void convertDataSet(DbDataSet p, StatDataSet pp)
        {
            pp.Id = p.Id;
            pp.LastIndex = p.LastIndex;
            pp.Name = p.Name;
            pp.Description = p.Description;
            pp.IsModified = false;
        }// convertDataSet
        protected void convertVariable(DbVariable p, VariableDesc pp)
        {
            pp.Id = p.Id;
            pp.DataSetId = p.DataSetId;
            pp.DataType = p.VarType;
            pp.Name = p.Name;
            pp.IsCategVar = p.IsCateg;
            pp.IsIdVar = p.IsId;
            pp.IsNameVar = p.IsName;
            pp.IsImageVar = p.IsImageVar;
            pp.IsInfoVar = p.IsInfoVar;
            pp.Description = p.Description;
            pp.IsModified = false;
        }// convertVariable
        protected void convertValue(DbValue p, ValueDesc pp)
        {
            pp.Id = p.Id;
            pp.VariableId = p.VariableId;
            pp.Index = p.Index;
            pp.DataStringValue = p.Value;
            pp.VariableName = p.Variable.Name;
            pp.VariableType = p.Variable.VarType;
            pp.IsModified = false;
        }// convertValue
        protected void convertPhoto(DbPhoto p, PhotoDesc pp)
        {
            pp.Id = p.Id;
            pp.Name = p.Name;
            pp.DataBytes = p.DataBytes;
            pp.IsModified = false;
        }// convertPhoto
        protected DbPhoto findPhoto(statdataEntities ctx, PhotoDesc oPhoto)
        {
            DbPhoto pRet = null;
            if (oPhoto == null)
            {
                return pRet;
            }
            if (oPhoto.Id != 0)
            {
                var q = from x in ctx.DbPhotoes where x.Id == oPhoto.Id select x;
                if (q.Count() > 0)
                {
                    return q.First();
                }
            }
            String s = oPhoto.Name;
            if (!String.IsNullOrEmpty(s))
            {
                String ss = s.ToLower();
                var qq = from x in ctx.DbPhotoes where x.Name.Trim().ToLower() == ss select x;
                if (qq.Count() > 0)
                {
                    return qq.First();
                }
            }
            return pRet;
        }// findPhoto
        protected DbDataSet findDataSet(statdataEntities ctx, StatDataSet oSet)
        {
            DbDataSet pRet = null;
            if (oSet != null)
            {
                if (oSet.Id != 0)
                {
                    var q = from x in ctx.DbDataSets where x.Id == oSet.Id select x;
                    if (q.Count() > 0)
                    {
                        return q.First();
                    }
                }
                String sname = oSet.Name.Trim().ToLower();
                if (!String.IsNullOrEmpty(sname))
                {
                    var q = from x in ctx.DbDataSets where x.Name.Trim().ToLower() == sname select x;
                    if (q.Count() > 0)
                    {
                        return q.First();
                    }
                }
            }// oSet
            return pRet;
        }// findDataSet
        protected DbDataSet maintainsDataSet(statdataEntities ctx, StatDataSet oSet)
        {
            if (oSet == null)
            {
                return null;
            }
            String sname = oSet.Name.Trim();
            if (String.IsNullOrEmpty(sname))
            {
                return null;
            }
            DbDataSet pRet = findDataSet(ctx, oSet);
            if (pRet != null)
            {
                pRet.Name = oSet.Name;
                pRet.Description = oSet.Description;
            }
            else
            {
                pRet = new DbDataSet();
                pRet.Id = nextId(ctx, TAB_DATASET);
                pRet.LastIndex = 0;
                pRet.Name = sname;
                pRet.Description = oSet.Description;
                ctx.DbDataSets.Add(pRet);
                ctx.SaveChanges();
            }
            return pRet;
        }// maintainsDataSet
        protected DbVariable findVariable(statdataEntities ctx, VariableDesc oVar)
        {
            DbVariable pRet = null;
            if (oVar != null)
            {
                if (oVar.Id != 0)
                {
                    var q = from x in ctx.DbVariables where x.Id == oVar.Id select x;
                    if (q.Count() > 0)
                    {
                        return q.First();
                    }
                }
                String sname = oVar.Name.Trim().ToLower();
                int nId = oVar.DataSetId;
                if ((nId != 0) && (!String.IsNullOrEmpty(sname)))
                {
                    var q = from x in ctx.DbVariables where (x.DataSetId == nId) && x.Name.Trim().ToLower() == sname select x;
                    if (q.Count() > 0)
                    {
                        return q.First();
                    }
                }
            }// oSet
            return pRet;
        }// findVariable
        protected DbVariable maintainsVariable(statdataEntities ctx, VariableDesc oVar)
        {
            DbVariable pRet = null;
            if (oVar != null)
            {
                String sname = oVar.Name.Trim();
                int nId = oVar.DataSetId;
                String stype = oVar.DataType.Trim();
                if (String.IsNullOrEmpty(sname) || (nId == 0) || String.IsNullOrEmpty(stype))
                {
                    return null;
                }
                pRet = findVariable(ctx, oVar);
                if (pRet != null)
                {
                    pRet.Name = sname;
                    pRet.VarType = stype;
                    pRet.IsCateg = oVar.IsCategVar;
                    pRet.IsId = oVar.IsIdVar;
                    pRet.IsName = oVar.IsNameVar;
                    pRet.IsImageVar = oVar.IsImageVar;
                    pRet.IsInfoVar = oVar.IsInfoVar;
                    pRet.Description = oVar.Description;
                }
                else
                {
                    DbDataSet pSet = null;
                    {
                        var qq = from x in ctx.DbDataSets where x.Id == nId select x;
                        if (qq.Count() < 1)
                        {
                            return null;
                        }
                        pSet = qq.First();
                    }
                    pRet = new DbVariable();
                    pRet.Id = nextId(ctx, TAB_VARIABLE);
                    pRet.DataSet = pSet;
                    pRet.Name = sname;
                    pRet.VarType = stype;
                    pRet.IsCateg = oVar.IsCategVar;
                    pRet.IsId = oVar.IsIdVar;
                    pRet.IsName = oVar.IsNameVar;
                    pRet.IsImageVar = oVar.IsImageVar;
                    pRet.IsInfoVar = oVar.IsInfoVar;
                    pRet.Description = oVar.Description;
                    ctx.DbVariables.Add(pRet);
                }
            }
            return pRet;
        }// 
        protected Tuple<VariableInfo, Exception> getVariableInfo(statdataEntities ctx, VariableDesc oVar)
        {
            Exception err = null;
            VariableInfo info = null;
            if (oVar == null)
            {
                return new Tuple<VariableInfo, Exception>(info, new ArgumentNullException());
            }

            DbVariable pVar = findVariable(ctx, oVar);
            if (pVar == null)
            {
                return new Tuple<VariableInfo, Exception>(info, err);
            }
            info = new VariableInfo();
            String stype = pVar.VarType.Trim().ToLower();
            info.VariableId = pVar.Id;
            info.VariableName = pVar.Name;
            info.DataSetId = pVar.DataSetId;
            info.DataSetName = pVar.DataSet.Name;
            info.DataType = stype;
            info.IsCategVar = pVar.IsCateg;
            if ((stype == "bool") || (stype == "string"))
            {
                info.IsCategVar = true;
            }
            {
                var q1 = pVar.Values;
                int nMissing = 0;
                int nCount = 0;
                foreach (var p in q1)
                {
                    String sval = p.Value;
                    if (!String.IsNullOrEmpty(sval))
                    {
                        sval = StatHelpers.ConvertValue(sval);
                    }
                    if (String.IsNullOrEmpty(sval))
                    {
                        ++nMissing;
                    }
                    ++nCount;
                }// p
                info.MissingValuesCount = nMissing;
                info.TotalValuesCount = nCount;
            }
            if (info.IsCategVar)
            {
                Dictionary<String, int> dict = new Dictionary<string, int>();
                var q2 = from x in pVar.Values select x.Value;
                foreach (var p in q2)
                {
                    String sval = StatHelpers.ConvertValue(p);
                    if (!String.IsNullOrEmpty(sval))
                    {
                        if (!dict.ContainsKey(sval))
                        {
                            dict[sval] = 1;
                        }
                        else
                        {
                            dict[sval] = dict[sval] + 1;
                        }
                    }
                }// p
                CategValueDescs vv = new CategValueDescs();
                var keys = dict.Keys;
                foreach (var s in keys)
                {
                    CategValueDesc c = new CategValueDesc();
                    c.StringValue = s;
                    c.Count = dict[s];
                    c.VariableId = info.VariableId;
                    vv.Add(c);
                }
                info.CategValues = vv;
                info.DistinctValues = keys.ToList();
            }
            else
            {
                var q3 = pVar.Values;
                int nCount = 0;
                List<double> dList = new List<double>();
                foreach (var p in q3)
                {
                    String sval = StatHelpers.ConvertValue(p.Value);
                    if (!String.IsNullOrEmpty(sval))
                    {
                        double xcur = 0;
                        if (double.TryParse(sval, out xcur))
                        {
                            dList.Add(xcur);
                        }// ok
                    }
                }// p
                nCount = dList.Count();
                if (nCount > 0)
                {
                    DescriptiveStatistics st = new DescriptiveStatistics(dList);
                    info.MinValue = st.Minimum;
                    info.MaxValue = st.Maximum;
                    info.MeanValue = myconvert(st.Mean);
                    info.Deviation = myconvert(st.StandardDeviation);
                    info.Skewness = myconvert(st.Skewness);
                    info.Flatness = myconvert(st.Kurtosis);
                    dList.Sort();
                    var oAr = dList.ToArray();
                    info.Quantile05 = myconvert(SortedArrayStatistics.Quantile(oAr, 0.05));
                    info.Quantile10 = myconvert(SortedArrayStatistics.Quantile(oAr, 0.10));
                    info.Quantile25 = myconvert(SortedArrayStatistics.Quantile(oAr, 0.25));
                    info.Median = myconvert(SortedArrayStatistics.Quantile(oAr, 0.5));
                    info.Quantile75 = myconvert(SortedArrayStatistics.Quantile(oAr, 0.75));
                    info.Quantile90 = myconvert(SortedArrayStatistics.Quantile(oAr, 0.90));
                    info.Quantile95 = myconvert(SortedArrayStatistics.Quantile(oAr, 0.10));
                }
            }
            info.IsModified = false;
            //
            return new Tuple<VariableInfo, Exception>(info, err);
        }// GetVariableInfo
        protected Tuple<IEnumerable<int>, Exception> getIndexes(statdataEntities ctx, IEnumerable<VariableDesc> oVars)
        {
            if (oVars == null)
            {
                return new Tuple<IEnumerable<int>, Exception>(null, new ArgumentNullException());
            }
            List<int> oRet = null;
            Exception err = null;
            try
            {
                HashSet<int> oSet = null;
                foreach (var oVar in oVars)
                {
                    DbVariable pVar = findVariable(ctx, oVar);
                    if (pVar == null)
                    {
                        return new Tuple<IEnumerable<int>, Exception>(null, new ArgumentException());
                    }// pVar
                    var q = from x in pVar.Values where (x.Index >= 0) select x.Index;
                    HashSet<int> oCur = new HashSet<int>();
                    foreach (var ind in q)
                    {
                        oCur.Add(ind);
                    }// q
                    if (oSet == null)
                    {
                        oSet = oCur;
                    }
                    else
                    {
                        HashSet<int> oDel = new HashSet<int>();
                        foreach (var ind in oSet)
                        {
                            if (!oCur.Contains(ind))
                            {
                                oDel.Add(ind);
                            }
                        }// ind
                        foreach (var ind in oDel)
                        {
                            oSet.Remove(ind);
                        }
                        if (oSet.Count < 1)
                        {
                            break;
                        }
                    }// bFirst
                }// oVar
                oRet = (oSet != null) ? oSet.ToList() : new List<int>();
            }
            catch (Exception ex)
            {
                err = ex;
            }
            return new Tuple<IEnumerable<int>, Exception>(oRet, err);
        }//GetIndexes
        protected Tuple<Dictionary<VariableDesc, Dictionary<int, ValueDesc>>, Exception> readValues(statdataEntities ctx, IEnumerable<VariableDesc> oVars, IEnumerable<int> oIndexes)
        {
            Dictionary<VariableDesc, Dictionary<int, ValueDesc>> oDictRet = null;
            Exception err = null;
            if ((oVars == null) || (oIndexes == null))
            {
                return new Tuple<Dictionary<VariableDesc, Dictionary<int, ValueDesc>>, Exception>(null, new ArgumentNullException());
            }
            try
            {
                oDictRet = new Dictionary<VariableDesc, Dictionary<int, ValueDesc>>();
                foreach (var oVar in oVars)
                {
                    DbVariable pVar = findVariable(ctx, oVar);
                    if (pVar == null)
                    {
                        return new Tuple<Dictionary<VariableDesc, Dictionary<int, ValueDesc>>, Exception>(null, new ArgumentException());
                    }
                    var curDict = new Dictionary<int, ValueDesc>();
                    foreach (var ind in oIndexes)
                    {
                        if (ind >= 0)
                        {
                            var qq = from x in pVar.Values where x.Index == ind select x;
                            if (qq.Count() > 0)
                            {
                                var v = qq.First();
                                var vv = new ValueDesc();
                                convertValue(v, vv);
                                curDict[ind] = vv;
                            }
                        }// ind
                    }// ind
                    oDictRet[oVar] = curDict;
                }// oVar
            }
            catch (Exception ex)
            {
                err = ex;
            }
            return new Tuple<Dictionary<VariableDesc, Dictionary<int, ValueDesc>>, Exception>(oDictRet, err);
        }//ReadValues
        protected Tuple<Dictionary<string, List<ValueDesc>>, Exception> readCategValues(statdataEntities ctx,
            VariableDesc oCategVar, VariableDesc oDataVar)
        {
            Exception err = null;
            Dictionary<String, List<ValueDesc>> xRet = null;
            if ((oCategVar == null) || (oDataVar == null))
            {
                return new Tuple<Dictionary<string, List<ValueDesc>>, Exception>(null, new ArgumentNullException());
            }
            try
            {
                DbVariable pCategVar = findVariable(ctx, oCategVar);
                DbVariable pDataVar = findVariable(ctx, oDataVar);
                if ((pCategVar == null) || (pDataVar == null))
                {
                    return new Tuple<Dictionary<string, List<ValueDesc>>, Exception>(null, new ArgumentException());
                }
                xRet = new Dictionary<string, List<ValueDesc>>();
                var qcateg = from x in ctx.DbValues where x.VariableId == pCategVar.Id select x;
                foreach (var vcateg in qcateg)
                {
                    int index = vcateg.Index;
                    String scateg = StatHelpers.ConvertValue(vcateg.Value);
                    if ((index >= 0) && (!String.IsNullOrEmpty(scateg)))
                    {
                        var qdata = from x in ctx.DbValues where (x.VariableId == pDataVar.Id) && (x.Index == index) select x;
                        foreach (var vdata in qdata)
                        {
                            String sdata = StatHelpers.ConvertValue(vdata.Value);
                            if (!String.IsNullOrEmpty(sdata))
                            {
                                ValueDesc vv = new ValueDesc();
                                convertValue(vdata, vv);
                                if (!xRet.ContainsKey(scateg))
                                {
                                    List<ValueDesc> ll = new List<ValueDesc>();
                                    ll.Add(vv);
                                    xRet[scateg] = ll;
                                }
                                else
                                {
                                    (xRet[scateg]).Add(vv);
                                }
                            }// sdata
                        }// vdata
                    }// vcateg
                }// vcateg
            }
            catch (Exception ex)
            {
                err = ex;
            }
            return new Tuple<Dictionary<string, List<ValueDesc>>, Exception>(xRet, err);
        }// ReadCategValues
        protected Tuple<Dictionary<string, Dictionary<VariableDesc, List<ValueDesc>>>, Exception> readCategValues(statdataEntities ctx,
            VariableDesc oCategVar, IEnumerable<VariableDesc> oDataVars)
        {
            if ((oCategVar == null) || (oDataVars == null))
            {
                return new Tuple<Dictionary<string, Dictionary<VariableDesc, List<ValueDesc>>>, Exception>(null, new ArgumentNullException());
            }
            List<VariableDesc> oList = new List<VariableDesc>();
            oList.Add(oCategVar);
            oList.AddRange(oDataVars);
            var xx = getIndexes(ctx, oList);
            if (xx == null)
            {
                return new Tuple<Dictionary<string, Dictionary<VariableDesc, List<ValueDesc>>>, Exception>(null, new ArgumentException());
            }
            var oIndexes = xx.Item1;
            if ((oIndexes == null) || (xx.Item2 != null))
            {
                return new Tuple<Dictionary<string, Dictionary<VariableDesc, List<ValueDesc>>>, Exception>(null, xx.Item2);
            }
            Dictionary<string, Dictionary<VariableDesc, List<ValueDesc>>> oRet = new Dictionary<string, Dictionary<VariableDesc, List<ValueDesc>>>();
            DbVariable pCategVar = findVariable(ctx, oCategVar);
            if (pCategVar == null)
            {
                return new Tuple<Dictionary<string, Dictionary<VariableDesc, List<ValueDesc>>>, Exception>(null, new ArgumentException());
            }
            var qcateg = pCategVar.Values;
            foreach (var ind in oIndexes)
            {
                var q = from x in qcateg where x.Index == ind select x;
                if (q.Count() > 0)
                {
                    DbValue vcateg = q.First();
                    String scateg = StatHelpers.ConvertValue(vcateg.Value);
                    if (!String.IsNullOrEmpty(scateg))
                    {
                        if (!oRet.ContainsKey(scateg))
                        {
                            oRet[scateg] = new Dictionary<VariableDesc, List<ValueDesc>>();
                        }
                        var dict = oRet[scateg];
                        foreach (var vdata in oDataVars)
                        {
                            if (!dict.ContainsKey(vdata))
                            {
                                dict[vdata] = new List<ValueDesc>();
                            }
                            List<ValueDesc> xlist = dict[vdata];
                            var qy = from x in ctx.DbValues where (x.VariableId == vdata.Id) && (x.Index == ind) select x;
                            if (qy.Count() > 0)
                            {
                                var v = qy.First();
                                String s = StatHelpers.ConvertValue(v.Value);
                                if (!String.IsNullOrEmpty(s))
                                {
                                    ValueDesc vz = new ValueDesc();
                                    convertValue(v, vz);
                                    xlist.Add(vz);
                                }
                            }// qy
                        }// vdata
                    }// scateg
                }// qCount
            }// ind
            if (oRet.Count < 1)
            {
                return new Tuple<Dictionary<string, Dictionary<VariableDesc, List<ValueDesc>>>, Exception>(null, null);
            }
            return new Tuple<Dictionary<string, Dictionary<VariableDesc, List<ValueDesc>>>, Exception>(oRet, null);
        }//readCategValues 
        protected bool writeValues(statdataEntities ctx, IEnumerable<ValueDesc> oVals)
        {
            bool bRet = false;
            foreach (var val in oVals)
            {
                if (val != null)
                {
                    int nId = val.Id;
                    int index = val.Index;
                    int nVarId = val.VariableId;
                    String sval = StatHelpers.ConvertValue(val.DataStringValue);
                    DbValue pVal = null;
                    if (nId != 0)
                    {
                        var q = from x in ctx.DbValues where x.Id == nId select x;
                        if (q.Count() > 0)
                        {
                            pVal = q.First();
                            pVal.Value = sval;
                            bRet = true;
                        }
                    }
                    if (pVal == null)
                    {
                        var q = from x in ctx.DbValues where (x.VariableId == nVarId) && (x.Index == index) select x;
                        if (q.Count() > 0)
                        {
                            pVal = q.First();
                            pVal.Value = sval;
                            bRet = true;
                        }
                    }
                    if (pVal == null)
                    {
                        pVal = new DbValue();
                        pVal.Id = nextId(ctx, TAB_VALEUR);
                        pVal.VariableId = nVarId;
                        pVal.Index = index;
                        pVal.Value = sval;
                        ctx.DbValues.Add(pVal);
                        bRet = true;
                    }
                }// val
            }// val
            return bRet;
        }// writesValues
        #endregion // helpers
        #region IStoreDataManager implementation
        public Tuple<IEnumerable<StatDataSet>, Exception> GetAllStatDataSets()
        {
            List<StatDataSet> oRet = null;
            Exception err = null;
            try
            {
                using (var ctx = getContext())
                {
                    var q = from x in ctx.DbDataSets orderby x.Name ascending select x;
                    oRet = new List<StatDataSet>();
                    foreach (var p in q)
                    {
                        StatDataSet pp = new StatDataSet();
                        convertDataSet(p, pp);
                        oRet.Add(pp);
                    }// p
                }// ctx
            }
            catch (Exception ex)
            {
                err = ex;
            }
            return new Tuple<IEnumerable<StatDataSet>, Exception>(oRet, err);
        }// getAllDataSets
        public Tuple<bool, Exception> RemoveDataSet(StatDataSet oSet)
        {
            bool bRet = false;
            Exception err = null;
            if (oSet == null)
            {
                return new Tuple<bool, Exception>(false, new ArgumentNullException());
            }
            try
            {
                using (var ctx = getContext())
                {
                    DbDataSet pSet = findDataSet(ctx, oSet);
                    if (pSet != null)
                    {
                        ctx.DbDataSets.Remove(pSet);
                        ctx.SaveChanges();
                        bRet = true;
                    }
                }
            }
            catch (Exception ex)
            {
                err = ex;
            }
            return new Tuple<bool, Exception>(bRet, err);
        }// RemoveDataSet
        public Tuple<StatDataSet, Exception> GetDataSet(StatDataSet oSet)
        {
            StatDataSet oRet = null;
            Exception err = null;
            if (oSet == null)
            {
                return new Tuple<StatDataSet, Exception>(null, new ArgumentException());
            }
            try
            {
                using (var ctx = getContext())
                {
                    DbDataSet pSet = this.findDataSet(ctx, oSet);
                    if (pSet != null)
                    {
                        oRet = new StatDataSet();
                        convertDataSet(pSet, oRet);
                    }// pSet
                }// ctx
            }
            catch (Exception ex)
            {
                err = ex;
            }
            return new Tuple<StatDataSet, Exception>(oRet, err);
        }// GetDataSet
        public Tuple<StatDataSet, Exception> MaintainsDataSet(StatDataSet oSet)
        {
            StatDataSet oRet = null;
            Exception err = null;
            if (oSet == null)
            {
                return new Tuple<StatDataSet, Exception>(null, new ArgumentNullException());
            }
            try
            {
                using (var ctx = getContext())
                {
                    DbDataSet pSet = maintainsDataSet(ctx, oSet);
                    if (pSet != null)
                    {
                        ctx.SaveChanges();
                        oRet = new StatDataSet();
                        convertDataSet(pSet, oRet);
                    }
                }// ctx
            }
            catch (Exception ex)
            {
                err = ex;
            }
            return new Tuple<StatDataSet, Exception>(oRet, err);
        }// MaintainsDataSet
        public Tuple<IEnumerable<VariableDesc>, Exception> GetDataSetVariables(StatDataSet oSet)
        {
            List<VariableDesc> oRet = null;
            Exception err = null;
            if (oSet == null)
            {
                return new Tuple<IEnumerable<VariableDesc>, Exception>(null, new ArgumentNullException());
            }
            try
            {
                using (var ctx = getContext())
                {
                    DbDataSet pSet = findDataSet(ctx, oSet);
                    if (pSet != null)
                    {
                        var q = from x in pSet.Variables orderby x.Name ascending select x;
                        oRet = new List<VariableDesc>();
                        foreach (var p in q)
                        {
                            VariableDesc pp = new VariableDesc();
                            convertVariable(p, pp);
                            var xx = getVariableInfo(ctx, pp);
                            if (xx.Item2 != null)
                            {
                                return new Tuple<IEnumerable<VariableDesc>, Exception>(oRet, err);
                            }
                            pp.Info = xx.Item1;

                            oRet.Add(pp);
                        }// p
                    }// pSet
                }// ctx
                if ((oRet != null) && (oRet.Count > 1))
                {
                    oRet.Sort();
                }
            }
            catch (Exception ex)
            {
                err = ex;
            }
            return new Tuple<IEnumerable<VariableDesc>, Exception>(oRet, err);
        }//GetDataSetVariables
        public Tuple<IEnumerable<VariableDesc>, Exception> GetDataSetVariablesAndData(StatDataSet oSet)
        {
            List<VariableDesc> oRet = null;
            Exception err = null;
            if (oSet == null)
            {
                return new Tuple<IEnumerable<VariableDesc>, Exception>(null, new ArgumentNullException());
            }
            try
            {
                using (var ctx = getContext())
                {
                    DbDataSet pSet = findDataSet(ctx, oSet);
                    if (pSet != null)
                    {
                        var q = from x in pSet.Variables orderby x.Name ascending select x;
                        oRet = new List<VariableDesc>();
                        foreach (var p in q)
                        {
                            VariableDesc pp = new VariableDesc();
                            convertVariable(p, pp);
                            List<ValueDesc> oList = new List<ValueDesc>();
                            var col = p.Values;
                            foreach (var v in col)
                            {
                                if (v.Index >= 0)
                                {
                                    ValueDesc vv = new ValueDesc();
                                    convertValue(v, vv);
                                    oList.Add(vv);
                                }// v
                            }// v
                            pp.Values = new ValueDescs(oList);
                            var xx = getVariableInfo(ctx, pp);
                            if (xx.Item2 != null)
                            {
                                return new Tuple<IEnumerable<VariableDesc>, Exception>(oRet, err);
                            }
                            pp.Info = xx.Item1;
                            oRet.Add(pp);
                        }// p
                    }// pSet
                }// ctx
                if ((oRet != null) && (oRet.Count > 1))
                {
                    oRet.Sort();
                }
            }
            catch (Exception ex)
            {
                err = ex;
            }
            return new Tuple<IEnumerable<VariableDesc>, Exception>(oRet, err);
        }//GetDataSetVariablesAndData
        public Tuple<VariableDesc, Exception> MaintainsVariable(VariableDesc oVar)
        {
            VariableDesc oRet = null;
            Exception err = null;
            if (oVar == null)
            {
                return new Tuple<VariableDesc, Exception>(null, new ArgumentNullException());
            }
            try
            {
                using (var ctx = getContext())
                {
                    DbVariable p = maintainsVariable(ctx, oVar);
                    if (p != null)
                    {
                        ctx.SaveChanges();
                        oRet = new VariableDesc();
                        convertVariable(p, oRet);
                    }
                }// ctx
            }
            catch (Exception ex)
            {
                err = ex;
            }
            return new Tuple<VariableDesc, Exception>(oRet, err);
        }// MaintainsVariable
        public Tuple<bool, Exception> RemoveVariable(VariableDesc oVar)
        {
            bool bRet = false;
            Exception err = null;
            if (oVar == null)
            {
                return new Tuple<bool, Exception>(false, new ArgumentNullException());
            }
            try
            {
                using (var ctx = getContext())
                {
                    DbVariable pVar = findVariable(ctx, oVar);
                    if (pVar != null)
                    {
                        ctx.DbVariables.Remove(pVar);
                        ctx.SaveChanges();
                        bRet = true;
                    }
                }
            }
            catch (Exception ex)
            {
                err = ex;
            }
            return new Tuple<bool, Exception>(bRet, err);
        }// RemoveVariable
        public Tuple<int, Exception> GetVariableValuesCount(VariableDesc oVar)
        {
            int nRet = 0;
            Exception err = null;
            if (oVar == null)
            {
                return new Tuple<int, Exception>(0, new ArgumentNullException());
            }
            try
            {
                using (var ctx = getContext())
                {
                    DbVariable pVar = findVariable(ctx, oVar);
                    if (pVar != null)
                    {
                        nRet = pVar.Values.Count();
                    }
                }// ctx
            }
            catch (Exception ex)
            {
                err = ex;
            }
            return new Tuple<int, Exception>(nRet, err);
        }//GetVariableValuesCount
        public Tuple<IEnumerable<ValueDesc>, Exception> GetVariableValues(VariableDesc oVar, int skip, int taken)
        {
            List<ValueDesc> oRet = new List<ValueDesc>();
            Exception err = null;
            if (oVar == null)
            {
                return new Tuple<IEnumerable<ValueDesc>, Exception>(null, new ArgumentNullException());
            }
            try
            {
                using (var ctx = getContext())
                {
                    DbVariable pVar = findVariable(ctx, oVar);
                    if (pVar == null)
                    {
                        return new Tuple<IEnumerable<ValueDesc>, Exception>(null, new ArgumentException());
                    }
                    int nMax = pVar.Values.Count();
                    if (skip < 0)
                    {
                        skip = 0;
                    }
                    if (taken < 0)
                    {
                        taken = 0;
                    }
                    if ((skip + taken) > nMax)
                    {
                        taken = nMax - skip;
                        if (taken < 0)
                        {
                            taken = 0;
                        }
                    }
                    if (taken == 0)
                    {
                        if (skip > 0)
                        {
                            var q = (from x in pVar.Values orderby x.Index ascending select x).Skip(skip);
                            foreach (var p in q)
                            {
                                ValueDesc pp = new ValueDesc();
                                convertValue(p, pp);
                                oRet.Add(pp);
                            }// p
                        }
                        else
                        {
                            var q = from x in pVar.Values orderby x.Index ascending select x;
                            foreach (var p in q)
                            {
                                ValueDesc pp = new ValueDesc();
                                convertValue(p, pp);
                                oRet.Add(pp);
                            }// p
                        }

                    }
                    else
                    {
                        if (skip > 0)
                        {
                            var q = (from x in pVar.Values orderby x.Index ascending select x).Skip(skip).Take(taken);
                            foreach (var p in q)
                            {
                                ValueDesc pp = new ValueDesc();
                                convertValue(p, pp);
                                oRet.Add(pp);
                            }// p
                        }
                        else
                        {
                            var q = (from x in pVar.Values orderby x.Index ascending select x).Take(taken);
                            foreach (var p in q)
                            {
                                ValueDesc pp = new ValueDesc();
                                convertValue(p, pp);
                                oRet.Add(pp);
                            }// p
                        }
                    }
                }// ctx
            }
            catch (Exception ex)
            {
                err = ex;
            }
            return new Tuple<IEnumerable<ValueDesc>, Exception>(oRet, err);
        }//GetVariableValues(
        public Tuple<bool, Exception> WriteValues(IEnumerable<ValueDesc> vals)
        {
            bool bRet = false;
            Exception err = null;
            if (vals == null)
            {
                return new Tuple<bool, Exception>(false, new ArgumentNullException());
            }
            try
            {
                int nMax = 0;
                foreach (var v in vals)
                {
                    if ((v != null) && (v.Id == 0) && (v.Index >= 0))
                    {
                        ++nMax;
                    }
                }// v
                int[] ids = null;
                using (var ctx = getContext())
                {
                    if (nMax > 0)
                    {
                        ids = nextIds(ctx, TAB_VALEUR, nMax);
                    }
                    int icur = 0;
                    HashSet<int> oSet = new HashSet<int>();
                    foreach (var v in vals)
                    {
                        if ((v != null) && (v.Index >= 0))
                        {
                            DbValue pVal = null;
                            if (v.Id != 0)
                            {
                                var q = from x in ctx.DbValues where x.Id == v.Id select x;
                                if (q.Count() > 0)
                                {
                                    pVal = q.First();
                                    pVal.Index = v.Index;
                                    pVal.Value = StatHelpers.ConvertValue(v.DataStringValue);
                                }
                            }
                            else
                            {
                                int nVarId = v.VariableId;
                                if (nVarId == 0)
                                {
                                    continue;
                                }
                                if (!oSet.Contains(nVarId))
                                {
                                    var qx = from x in ctx.DbVariables where x.Id == nVarId select x.Id;
                                    if (qx.Count() < 1)
                                    {
                                        continue;
                                    }
                                    oSet.Add(nVarId);
                                }
                                var q = from x in ctx.DbValues
                                        where (x.VariableId == nVarId) &&
                                            (x.Index == v.Index)
                                        select x;
                                if (q.Count() > 0)
                                {
                                    pVal = q.First();
                                    pVal.Value = StatHelpers.ConvertValue(v.DataStringValue);
                                }
                                else
                                {
                                    pVal = new DbValue();
                                    pVal.Id = ids[icur++];
                                    pVal.VariableId = nVarId;
                                    pVal.Index = v.Index;
                                    pVal.Value = StatHelpers.ConvertValue(v.DataStringValue);
                                    ctx.DbValues.Add(pVal);
                                    ctx.SaveChanges();
                                }
                            }
                        }// v
                    }// v
                    ctx.SaveChanges();
                    bRet = true;
                }// ctx
            }// try
            catch (Exception ex)
            {
                err = ex;
            }
            return new Tuple<bool, Exception>(bRet, err);
        }// WriteValue
        public Tuple<bool, Exception> RemoveValues(IEnumerable<ValueDesc> vals)
        {
            bool bRet = false;
            Exception err = null;
            if (vals == null)
            {
                return new Tuple<bool, Exception>(false, new ArgumentNullException());
            }
            try
            {
                using (var ctx = getContext())
                {
                    List<DbValue> oList = new List<DbValue>();
                    foreach (var v in vals)
                    {
                        if (v != null)
                        {
                            DbValue pVal = null;
                            if (v.Id != 0)
                            {
                                var q = from x in ctx.DbValues where x.Id == v.Id select x;
                                if (q.Count() > 0)
                                {
                                    pVal = q.First();
                                }
                            }
                            if (pVal == null)
                            {
                                var q = from x in ctx.DbValues
                                        where (x.VariableId == v.VariableId) &&
                                            (x.Index == v.Index)
                                        select x;
                                if (q.Count() > 0)
                                {
                                    pVal = q.First();
                                }
                            }
                            if (pVal != null)
                            {
                                oList.Add(pVal);
                            }
                        }// v
                    }// v
                    foreach (var p in oList)
                    {
                        ctx.DbValues.Remove(p);
                    }// p
                    ctx.SaveChanges();
                }// ctx
            }
            catch (Exception ex)
            {
                err = ex;
            }
            return new Tuple<bool, Exception>(bRet, err);
        }// RemoveValues
        public Tuple<VariableInfo, Exception> GetVariableInfo(VariableDesc oVar)
        {
            VariableInfo oRet = null;
            Exception err = null;
            try
            {
                using (var ctx = getContext())
                {
                    var xx = getVariableInfo(ctx, oVar);
                    if (xx != null)
                    {
                        oRet = xx.Item1;
                        err = xx.Item2;
                    }
                }// ctx
            }
            catch (Exception ex)
            {
                err = ex;
            }
            return new Tuple<VariableInfo, Exception>(oRet, err);
        }// GetVariableInfo
        public Tuple<IEnumerable<int>, Exception> GetIndexes(IEnumerable<VariableDesc> oVars)
        {
            if (oVars == null)
            {
                return new Tuple<IEnumerable<int>, Exception>(null, new ArgumentNullException());
            }
            List<int> oRet = null;
            Exception err = null;
            try
            {
                using (var ctx = getContext())
                {
                    return getIndexes(ctx, oVars);
                }// ctx
            }
            catch (Exception ex)
            {
                err = ex;
            }
            return new Tuple<IEnumerable<int>, Exception>(oRet, err);
        }//GetIndexes
        public Tuple<Dictionary<VariableDesc, Dictionary<int, ValueDesc>>, Exception> ReadValues(IEnumerable<VariableDesc> oVars, IEnumerable<int> oIndexes)
        {
            Dictionary<VariableDesc, Dictionary<int, ValueDesc>> oDictRet = null;
            Exception err = null;
            if ((oVars == null) || (oIndexes == null))
            {
                return new Tuple<Dictionary<VariableDesc, Dictionary<int, ValueDesc>>, Exception>(null, new ArgumentNullException());
            }
            try
            {
                using (var ctx = getContext())
                {
                    return readValues(ctx, oVars, oIndexes);
                }// ctx
            }
            catch (Exception ex)
            {
                err = ex;
            }
            return new Tuple<Dictionary<VariableDesc, Dictionary<int, ValueDesc>>, Exception>(oDictRet, err);
        }//ReadValues
        public Tuple<Dictionary<VariableDesc, double[]>, Exception> ReadNumValues(IEnumerable<VariableDesc> oVars)
        {
            Dictionary<VariableDesc, double[]> oRet = null;
            Exception err = null;
            if (oVars == null)
            {
                return new Tuple<Dictionary<VariableDesc, double[]>, Exception>(null, new ArgumentNullException());
            }
            List<VariableDesc> oList = new List<VariableDesc>();
            foreach (var v in oVars)
            {
                if ((v != null) && (!v.IsCategVar))
                {
                    oList.Add(v);
                }
            }// v
            if (oList.Count < 1)
            {
                return new Tuple<Dictionary<VariableDesc, double[]>, Exception>(null, null);
            }
            Dictionary<VariableDesc, Dictionary<int, ValueDesc>> oVals = null;
            IEnumerable<int> oIndexes = null;
            using (var ctx = getContext())
            {
                var xx = getIndexes(ctx, oList);
                oIndexes = xx.Item1;
                if ((oIndexes == null) || (xx.Item2 != null))
                {
                    return new Tuple<Dictionary<VariableDesc, double[]>, Exception>(null, xx.Item2);
                }
                var yy = readValues(ctx, oList, oIndexes);
                oVals = yy.Item1;
                if ((oVals == null) || (yy.Item2 != null))
                {
                    return new Tuple<Dictionary<VariableDesc, double[]>, Exception>(null, yy.Item2);
                }
            }// ctx
            int nRows = oIndexes.Count();
            oRet = new Dictionary<VariableDesc, double[]>();
            var keys = oVals.Keys;
            foreach (var oVar in keys)
            {
                double[] dVals = new double[nRows];
                int i = 0;
                Dictionary<int, ValueDesc> dict = oVals[oVar];
                foreach (var ind in oIndexes)
                {
                    ValueDesc v = dict[ind];
                    double d = 0.0;
                    if (v != null)
                    {
                        String s = StatHelpers.ConvertValue(v.DataStringValue);
                        if (!String.IsNullOrEmpty(s))
                        {
                            double.TryParse(s, out d);
                        }
                    }// v
                    dVals[i++] = d;
                }// ind
                oRet[oVar] = dVals;
            }// oVar
            return new Tuple<Dictionary<VariableDesc, double[]>, Exception>(oRet, err);
        }// ReadNumValues
        public Tuple<Dictionary<string, List<ValueDesc>>, Exception> ReadCategValues(VariableDesc oCategVar, VariableDesc oDataVar)
        {
            using (var ctx = getContext())
            {
                return readCategValues(ctx, oCategVar, oDataVar);
            }
        }
        public Tuple<Dictionary<string, double[]>, Exception> ReadNumCategValues(VariableDesc oCategVar, VariableDesc oDataVar)
        {
            Dictionary<string, double[]> oRet = null;
            var xx = ReadCategValues(oCategVar, oDataVar);
            var srcDict = xx.Item1;
            var err = xx.Item2;
            if ((err != null) || (srcDict == null))
            {
                return new Tuple<Dictionary<string, double[]>, Exception>(oRet, err);
            }
            oRet = new Dictionary<string, double[]>();
            var keys = srcDict.Keys;
            foreach (var key in keys)
            {
                List<double> dd = new List<double>();
                var vals = srcDict[key];
                foreach (var v in vals)
                {
                    if (v != null)
                    {
                        String s = StatHelpers.ConvertValue(v.DataStringValue);
                        if (!String.IsNullOrEmpty(s))
                        {
                            double d = 0.0;
                            if (double.TryParse(s, out d))
                            {
                                dd.Add(d);
                            }
                        }
                    }// v
                }// v
                if (dd.Count > 0)
                {
                    oRet[key] = dd.ToArray();
                }
            }// key
            return new Tuple<Dictionary<string, double[]>, Exception>(oRet, err);
        }
        public Tuple<Dictionary<string, Dictionary<VariableDesc, List<ValueDesc>>>, Exception> ReadCategValues(VariableDesc oCategVar, IEnumerable<VariableDesc> oDataVars)
        {
            Dictionary<string, Dictionary<VariableDesc, List<ValueDesc>>> oRet = null;
            Exception err = null;
            try
            {
                using (var ctx = getContext())
                {
                    var xx = readCategValues(ctx, oCategVar, oDataVars);
                    oRet = xx.Item1;
                    err = xx.Item2;
                }// ctx
            }
            catch (Exception ex)
            {
                err = ex;
            }
            return new Tuple<Dictionary<string, Dictionary<VariableDesc, List<ValueDesc>>>, Exception>(oRet, err);
        }//ReadCategValues
        public Tuple<Dictionary<string, Dictionary<VariableDesc, double[]>>, Exception> ReadNumCategValues(VariableDesc oCategVar, IEnumerable<VariableDesc> oDataVars)
        {
            Dictionary<string, Dictionary<VariableDesc, double[]>> oRet = null;
            Exception err = null;
            try
            {
                Dictionary<string, Dictionary<VariableDesc, List<ValueDesc>>> srcDict = null;
                using (var ctx = getContext())
                {
                    var xx = readCategValues(ctx, oCategVar, oDataVars);
                    srcDict = xx.Item1;
                    if ((srcDict == null) || (xx.Item2 != null))
                    {
                        return new Tuple<Dictionary<string, Dictionary<VariableDesc, double[]>>, Exception>(null, xx.Item2);
                    }
                }// ctx
                oRet = new Dictionary<string, Dictionary<VariableDesc, double[]>>();
                var keys = srcDict.Keys;
                foreach (var key in keys)
                {
                    oRet[key] = new Dictionary<VariableDesc, double[]>();
                    var curDict = oRet[key];
                    var src = srcDict[key];
                    foreach (var vdata in src.Keys)
                    {
                        List<double> dd = new List<double>();
                        var ll = src[vdata];
                        foreach (var v in ll)
                        {
                            double d = 0.0;
                            if (double.TryParse(StatHelpers.ConvertValue(v.DataStringValue), out d))
                            {
                                dd.Add(d);
                            }
                        }// v
                        curDict[vdata] = dd.ToArray();
                    }// vsata
                }// key
            }
            catch (Exception ex)
            {
                err = ex;
            }
            return new Tuple<Dictionary<string, Dictionary<VariableDesc, double[]>>, Exception>(oRet, null);
        }// ReadNumCategValues
        public Tuple<IEnumerable<int>, Exception> GetDataSetIndexes(StatDataSet oSet)
        {
            if (oSet == null)
            {
                return new Tuple<IEnumerable<int>, Exception>(null, new ArgumentNullException());
            }
            List<int> oList = null;
            Exception err = null;
            try
            {
                using (var ctx = getContext())
                {
                    var pSet = findDataSet(ctx, oSet);
                    if (pSet != null)
                    {
                        var q = (from x in ctx.DbValues
                                 where x.Variable.DataSetId == pSet.Id
                                 orderby x.Index ascending
                                 select x.Index).Distinct();
                        oList = q.ToList();
                    }
                }// ctx
            }
            catch (Exception ex)
            {
                err = ex;
            }
            return new Tuple<IEnumerable<int>, Exception>(oList, err);
        }
        public Tuple<IEnumerable<ValueDesc>, Exception> GetDataSetIndivValues(StatDataSet oSet, int iIndex)
        {
            if (oSet == null)
            {
                return new Tuple<IEnumerable<ValueDesc>, Exception>(null, new ArgumentNullException());
            }
            if (iIndex < 0)
            {
                return new Tuple<IEnumerable<ValueDesc>, Exception>(null, new ArgumentOutOfRangeException());
            }
            List<ValueDesc> oRet = null;
            Exception err = null;
            try
            {
                using (var ctx = getContext())
                {
                    oRet = new List<ValueDesc>();
                    var pSet = findDataSet(ctx, oSet);
                    if (pSet != null)
                    {
                        var q = from x in ctx.DbValues
                                where (x.Variable.DataSetId == pSet.Id) &&
                                (x.Index == iIndex)
                                select x;
                        foreach (var p in q)
                        {
                            ValueDesc pp = new ValueDesc();
                            convertValue(p, pp);
                            oRet.Add(pp);
                        }// p
                    }
                }// ctx
            }
            catch (Exception ex)
            {
                err = ex;
            }
            return new Tuple<IEnumerable<ValueDesc>, Exception>(oRet, err);
        }
        public Tuple<bool, Exception> ReplaceDataSet(StatDataSet oSet, CancellationToken cancelletionToken, IProgress<int> progress)
        {
            if (cancelletionToken.IsCancellationRequested)
            {
                return new Tuple<bool, Exception>(false, null);
            }
            if (oSet == null)
            {
                return new Tuple<bool, Exception>(false, new ArgumentNullException());
            }
            String sRealDataSetName = oSet.Name;
            if (String.IsNullOrEmpty(sRealDataSetName))
            {
                return new Tuple<bool, Exception>(false, new ArgumentException());
            }
            String tempName = String.Format("{0}_tmpwork", sRealDataSetName);
            if (progress != null)
            {
                progress.Report(1);
            }
            int nTotalValues = 0;
            int nTotalVars = 0;
            foreach (var v in oSet.Variables)
            {
                ++nTotalVars;
                nTotalValues += v.Values.Count();
            }// v
            if ((nTotalVars < 1) || (nTotalValues < 1))
            {
                return new Tuple<bool, Exception>(false, new ArgumentNullException());
            }
            if (cancelletionToken.IsCancellationRequested)
            {
                return new Tuple<bool, Exception>(false, null);
            }
            double dTotal = (double)(2 * nTotalVars + 5);
            double dCur = 1;
            bool bRet = false;
            Exception err = null;
            bool bCont = true;
            try
            {
                using (var ctx = getContext())
                {
                    DbDataSet pSet = null;
                    var qq = from x in ctx.DbDataSets where x.Name.Trim().ToLower() == tempName.Trim().ToLower() select x;
                    if (qq.Count() > 0)
                    {
                        pSet = qq.First();
                    }
                    if (pSet != null)
                    {
                        ctx.DbDataSets.Remove(pSet);
                        ctx.SaveChanges();
                        pSet = null;
                    }// pSet
                    if (progress != null)
                    {
                        dCur = dCur + 1.0;
                        int nx = (int)((dCur / dTotal) * 100.0 + 0.5);
                        progress.Report(nx);
                    }
                    int nDataSetId = nextId(ctx, TAB_DATASET);
                    int[] varIndexes = nextIds(ctx, TAB_VARIABLE, nTotalVars);
                    int[] valIndexes = nextIds(ctx, TAB_VALEUR, nTotalValues);
                    pSet = new DbDataSet();
                    pSet.Id = nDataSetId;
                    pSet.Name = tempName;
                    pSet.Description = oSet.Description;
                    ctx.DbDataSets.Add(pSet);
                    nDataSetId = pSet.Id;
                    int iCurrentVar = 0;
                    int iCurrentVal = 0;
                    HashSet<String> oSetNames = new HashSet<string>();
                    foreach (var v in oSet.Variables)
                    {
                        if (!bCont)
                        {
                            break;
                        }
                        if (v == null)
                        {
                            continue;
                        }
                        if (cancelletionToken.IsCancellationRequested)
                        {
                            bCont = false;
                            break;
                        }
                        String name = v.Name;
                        String stype = v.DataType;
                        if (String.IsNullOrEmpty(name) || String.IsNullOrEmpty(stype))
                        {
                            continue;
                        }
                        if (oSetNames.Contains(name))
                        {
                            continue;
                        }
                        DbVariable vv = new DbVariable();
                        int nVarId = varIndexes[iCurrentVar++];
                        vv.Id = nVarId;
                        vv.DataSetId = pSet.Id;
                        vv.Name = name;
                        vv.VarType = stype;
                        vv.Description = v.Description;
                        vv.IsCateg = v.IsCategVar;
                        vv.IsId = v.IsIdVar;
                        vv.IsImageVar = v.IsImageVar;
                        vv.IsInfoVar = v.IsInfoVar;
                        vv.IsName = v.IsNameVar;
                        ctx.DbVariables.Add(vv);
                        HashSet<int> oSetIndexes = new HashSet<int>();
                        foreach (var vx in v.Values)
                        {
                            if (!bCont)
                            {
                                break;
                            }
                            if (vx == null)
                            {
                                continue;
                            }
                            int ind = vx.Index;
                            if ((ind < 0) || oSetIndexes.Contains(ind))
                            {
                                continue;
                            }
                            oSetIndexes.Add(ind);
                            DbValue vz = new DbValue();
                            vz.Id = valIndexes[iCurrentVal++];
                            vz.VariableId = nVarId;
                            vz.Index = ind;
                            vz.Value = StatHelpers.ConvertValue(vx.DataStringValue);
                            ctx.DbValues.Add(vz);
                        }// vx
                        if (progress != null)
                        {
                            dCur = dCur + 1.0;
                            int nx = (int)((dCur / dTotal) * 100.0 + 0.5);
                            progress.Report(nx);
                        }
                    }// v
                    if (bCont)
                    {
                        ctx.SaveChanges();
                        var qqq = from x in ctx.DbDataSets where x.Name.Trim().ToLower() == tempName.Trim().ToLower() select x;
                        if (qqq.Count() > 0)
                        {
                            pSet = qqq.First();
                            pSet.Name = sRealDataSetName;
                            ctx.SaveChanges();
                            bRet = true;
                        }
                    }
                }// ctx
            }
            catch (Exception ex)
            {
                err = ex;
            }
            return new Tuple<bool, Exception>(bRet, err);
        }// ReplaceDataSet
        public Tuple<IndivDescs, Exception> GetDataSetIndivs(StatDataSet oSet)
        {
            if (oSet == null)
            {
                return new Tuple<IndivDescs, Exception>(null, new ArgumentNullException());
            }
            IndivDescs oRet = null;
            Exception err = null;
            try
            {
                using (var ctx = getContext())
                {
                    DbDataSet pSet = findDataSet(ctx, oSet);
                    if (pSet == null)
                    {
                        return new Tuple<IndivDescs, Exception>(oRet, err);
                    }
                    DbVariable pIdVar = null;
                    DbVariable pNameVar = null;
                    DbVariable pImageVar = null;
                    var q = from x in pSet.Variables select x;
                    foreach (var v in q)
                    {
                        if (v.IsId)
                        {
                            pIdVar = v;
                        }
                        if (v.IsName)
                        {
                            pNameVar = v;
                        }
                        if (v.IsImageVar)
                        {
                            pImageVar = v;
                        }
                    }// v
                    bool bImageInt = false;
                    if (pImageVar != null)
                    {
                        String s = pImageVar.VarType.Trim().ToLower();
                        if ((s != "string") && (s != "bool"))
                        {
                            bImageInt = true;
                        }
                    }
                    List<IndivDesc> oList = new List<IndivDesc>();
                    var indexes = (from x in ctx.DbValues where x.Variable.DataSetId == pSet.Id select x.Index).Distinct();
                    foreach (var ind in indexes)
                    {
                        IndivDesc pp = new IndivDesc();
                        pp.IndivIndex = ind;
                        if (pIdVar != null)
                        {
                            var qx = from x in ctx.DbValues where (x.VariableId == pIdVar.Id) && (x.Index == ind) select x;
                            if (qx.Count() > 0)
                            {
                                pp.IdString = qx.First().Value;
                            }
                        }// Id
                        if (pNameVar != null)
                        {
                            var qx = from x in ctx.DbValues where (x.VariableId == pNameVar.Id) && (x.Index == ind) select x;
                            if (qx.Count() > 0)
                            {
                                pp.Name = qx.First().Value;
                            }
                        }// name
                        if (pImageVar != null)
                        {
                            var qx = from x in ctx.DbValues where (x.VariableId == pImageVar.Id) && (x.Index == ind) select x;
                            if (qx.Count() > 0)
                            {
                                var px = qx.First();
                                String sx = StatHelpers.ConvertValue(px.Value);
                                if (!String.IsNullOrEmpty(sx))
                                {
                                    if (bImageInt)
                                    {
                                        double dval = 0.0;
                                        if (double.TryParse(sx, out dval))
                                        {
                                            int ival = (int)dval;
                                            if (ival != 0)
                                            {
                                                var qz = from x in ctx.DbPhotoes where x.Id == ival select x;
                                                if (qz.Count() > 0)
                                                {
                                                    var pz = qz.First();
                                                    pp.PhotoId = pz.Id;
                                                    pp.PhotoData = pz.DataBytes;
                                                }
                                            }// ival
                                        }
                                    }
                                    else
                                    {
                                        var qz = from x in ctx.DbPhotoes where x.Name.Trim().ToLower() == sx.ToLower() select x;
                                        if (qz.Count() > 0)
                                        {
                                            var pz = qz.First();
                                            pp.PhotoId = pz.Id;
                                            pp.PhotoData = pz.DataBytes;
                                        }
                                    }// ival
                                }// sx
                            }
                        }// photo
                        ValueDescs vv = new ValueDescs();
                        var qq = from x in ctx.DbValues where (x.Variable.DataSetId == pSet.Id) && (x.Index == ind) select x;
                        foreach (var v in qq)
                        {
                            ValueDesc fx = new ValueDesc();
                            convertValue(v, fx);
                            vv.Add(fx);
                        }// v
                        pp.Values = vv;
                        oList.Add(pp);
                    }// ind
                    if (oList.Count > 1)
                    {
                        oList.Sort();
                    }
                    oRet = new IndivDescs(oList);
                }// ctx
            }// try
            catch (Exception ex)
            {
                err = ex;
            }
            return new Tuple<IndivDescs, Exception>(oRet, err);
        }// GetDataSetIndivs
        public Tuple<IndivDescs, VariableDescs, Exception> FetchAllDataSetData(StatDataSet oSet)
        {
            IndivDescs indivs = null;
            VariableDescs vars = null;
            Exception err = null;
            try
            {
                using (var ctx = getContext())
                {
                    DbDataSet pSet = findDataSet(ctx, oSet);
                    if (pSet == null)
                    {
                        return new Tuple<IndivDescs, VariableDescs, Exception>(indivs, vars, err);
                    }
                    DbVariable pIdVar = null;
                    DbVariable pNameVar = null;
                    DbVariable pImageVar = null;
                    var q = from x in pSet.Variables orderby x.Name select x;
                    foreach (var v in q)
                    {
                        if (v.IsId)
                        {
                            pIdVar = v;
                        }
                        if (v.IsName)
                        {
                            pNameVar = v;
                        }
                        if (v.IsImageVar)
                        {
                            pImageVar = v;
                        }
                    }// v
                    bool bImageInt = false;
                    if (pImageVar != null)
                    {
                        String s = pImageVar.VarType.Trim().ToLower();
                        if ((s != "string") && (s != "bool"))
                        {
                            bImageInt = true;
                        }
                    }
                    List<VariableDesc> oRet = new List<VariableDesc>();
                    HashSet<int> oSetIndex = new HashSet<int>();
                    List<IndivDesc> listIndivs = new List<IndivDesc>();
                    Dictionary<int, ValueDescs> vals = new Dictionary<int, ValueDescs>();
                    foreach (var p in q)
                    {
                        VariableDesc pp = new VariableDesc();
                        convertVariable(p, pp);
                        var col = p.Values;
                        List<ValueDesc> pList = new List<ValueDesc>();
                        foreach (var v in col)
                        {
                            int index = v.Index;
                            if (index >= 0)
                            {
                                ValueDesc vv = new ValueDesc();
                                convertValue(v, vv);
                                pList.Add(vv);
                                if (!vals.ContainsKey(index))
                                {
                                    vals[index] = new ValueDescs();
                                }
                                (vals[index]).Add(vv);
                                if (!oSetIndex.Contains(index))
                                {
                                    oSetIndex.Add(index);
                                    var ind = new IndivDesc();
                                    listIndivs.Add(ind);
                                    ind.IndivIndex = index;
                                    if (pIdVar != null)
                                    {
                                        var qx = from x in pIdVar.Values where x.Index == index select x;
                                        if (qx.Count() > 0)
                                        {
                                            ind.IdString = qx.First().Value;
                                        }
                                    }// Id
                                    if (pNameVar != null)
                                    {
                                        var qx = from x in pNameVar.Values where x.Index == index select x;
                                        if (qx.Count() > 0)
                                        {
                                            ind.Name = qx.First().Value;
                                        }
                                    }// name
                                    if (pImageVar != null)
                                    {
                                        var qx = from x in pImageVar.Values where x.Index == index select x;
                                        if (qx.Count() > 0)
                                        {
                                            var px = qx.First();
                                            String sx = StatHelpers.ConvertValue(px.Value);
                                            if (!String.IsNullOrEmpty(sx))
                                            {
                                                if (bImageInt)
                                                {
                                                    double dval = 0.0;
                                                    if (double.TryParse(sx, out dval))
                                                    {
                                                        int ival = (int)dval;
                                                        if (ival != 0)
                                                        {
                                                            var qz = from x in ctx.DbPhotoes where x.Id == ival select x;
                                                            if (qz.Count() > 0)
                                                            {
                                                                var pz = qz.First();
                                                                ind.PhotoId = pz.Id;
                                                                ind.PhotoData = pz.DataBytes;
                                                            }
                                                        }// ival
                                                    }
                                                }
                                                else
                                                {
                                                    var qz = from x in ctx.DbPhotoes where x.Name.Trim().ToLower() == sx.ToLower() select x;
                                                    if (qz.Count() > 0)
                                                    {
                                                        var pz = qz.First();
                                                        ind.PhotoId = pz.Id;
                                                        ind.PhotoData = pz.DataBytes;
                                                    }
                                                }// ival
                                            }// sx
                                        }
                                    }// photo

                                }// add value
                            }// v
                        }// v
                        var xx = getVariableInfo(ctx, pp);
                        if (xx.Item2 != null)
                        {
                            return new Tuple<IndivDescs, VariableDescs, Exception>(indivs, vars, err);
                        }
                        pp.Info = xx.Item1;
                        pp.Values = new ValueDescs(pList);
                        oRet.Add(pp);
                    }// p
                    foreach (var ind in listIndivs)
                    {
                        int index = ind.IndivIndex;
                        if (vals.ContainsKey(index))
                        {
                            ind.Values = vals[index];
                        }
                    }// ind
                    if (listIndivs.Count > 1)
                    {
                        listIndivs.Sort();
                    }
                    if (oRet.Count > 1)
                    {
                        oRet.Sort();
                    }
                    indivs = new IndivDescs(listIndivs);
                    vars = new VariableDescs(oRet);
                }// ctx
            }// try
            catch (Exception ex)
            {
                err = ex;
            }
            return new Tuple<IndivDescs, VariableDescs, Exception>(indivs, vars, err);
        }// FetchAllDataSetData
        public Tuple<IndivDesc, Exception> GetDataSetIndiv(StatDataSet oSet, int indivIndex)
        {
            if (oSet == null)
            {
                return new Tuple<IndivDesc, Exception>(null, new ArgumentNullException());
            }
            IndivDesc pp = null;
            Exception err = null;
            try
            {
                using (var ctx = getContext())
                {
                    DbDataSet pSet = findDataSet(ctx, oSet);
                    if (pSet == null)
                    {
                        return new Tuple<IndivDesc, Exception>(pp, err);
                    }
                    var qm = from x in ctx.DbValues where (x.Variable.DataSetId == pSet.Id) && (x.Index == indivIndex) select x;
                    if (qm.Count() < 1)
                    {
                        return new Tuple<IndivDesc, Exception>(pp, null);
                    }
                    DbVariable pIdVar = null;
                    DbVariable pNameVar = null;
                    DbVariable pImageVar = null;
                    var q = from x in pSet.Variables select x;
                    foreach (var v in q)
                    {
                        if (v.IsId)
                        {
                            pIdVar = v;
                        }
                        if (v.IsName)
                        {
                            pNameVar = v;
                        }
                        if (v.IsImageVar)
                        {
                            pImageVar = v;
                        }
                    }// v
                    bool bImageInt = false;
                    if (pImageVar != null)
                    {
                        String s = pImageVar.VarType.Trim().ToLower();
                        if ((s != "string") && (s != "bool"))
                        {
                            bImageInt = true;
                        }
                    }
                    pp = new IndivDesc();
                    pp.IndivIndex = indivIndex;
                    int ind = indivIndex;
                    if (pIdVar != null)
                    {
                        var qx = from x in ctx.DbValues where (x.VariableId == pIdVar.Id) && (x.Index == ind) select x;
                        if (qx.Count() > 0)
                        {
                            pp.IdString = qx.First().Value;
                        }
                    }// Id
                    if (pNameVar != null)
                    {
                        var qx = from x in ctx.DbValues where (x.VariableId == pNameVar.Id) && (x.Index == ind) select x;
                        if (qx.Count() > 0)
                        {
                            pp.Name = qx.First().Value;
                        }
                    }// name
                    if (pImageVar != null)
                    {
                        var qx = from x in ctx.DbValues where (x.VariableId == pImageVar.Id) && (x.Index == ind) select x;
                        if (qx.Count() > 0)
                        {
                            var px = qx.First();
                            String sx = StatHelpers.ConvertValue(px.Value);
                            if (!String.IsNullOrEmpty(sx))
                            {
                                if (bImageInt)
                                {
                                    double dval = 0.0;
                                    if (double.TryParse(sx, out dval))
                                    {
                                        int ival = (int)dval;
                                        if (ival != 0)
                                        {
                                            var qz = from x in ctx.DbPhotoes where x.Id == ival select x;
                                            if (qz.Count() > 0)
                                            {
                                                var pz = qz.First();
                                                pp.PhotoId = pz.Id;
                                                pp.PhotoData = pz.DataBytes;
                                            }
                                        }// ival
                                    }
                                }
                                else
                                {
                                    var qz = from x in ctx.DbPhotoes where x.Name.Trim().ToLower() == sx.ToLower() select x;
                                    if (qz.Count() > 0)
                                    {
                                        var pz = qz.First();
                                        pp.PhotoId = pz.Id;
                                        pp.PhotoData = pz.DataBytes;
                                    }
                                }// ival
                            }// sx
                        }
                    }// photo
                    ValueDescs vv = new ValueDescs();
                    var qq = from x in ctx.DbValues where (x.Variable.DataSetId == pSet.Id) && (x.Index == ind) select x;
                    foreach (var v in qq)
                    {
                        ValueDesc fx = new ValueDesc();
                        convertValue(v, fx);
                        vv.Add(fx);
                    }// v
                    pp.Values = vv;
                }// ctx
            }// try
            catch (Exception ex)
            {
                err = ex;
            }
            return new Tuple<IndivDesc, Exception>(pp, err);
        }// GetDataSetIndiv
        public Tuple<bool, Exception> MaintainsVariableAndValues(IEnumerable<VariableDesc> oVars)
        {
            if (oVars == null)
            {
                return new Tuple<bool, Exception>(false, new ArgumentException());
            }
            bool bRet = false;
            Exception err = null;
            try
            {
                using (var ctx = getContext())
                {
                    bool xRet = false;
                    foreach (var oVar in oVars)
                    {
                        var v = maintainsVariable(ctx, oVar);
                        if (v == null)
                        {
                            return new Tuple<bool, Exception>(false, new ArgumentException());
                        }
                        xRet = true;
                        List<ValueDesc> oVals = new List<ValueDesc>();
                        foreach (var vx in oVar.Values)
                        {
                            if (vx != null)
                            {
                                ValueDesc vy = new ValueDesc();
                                vy.Id = vx.Id;
                                vy.VariableId = v.Id;
                                vy.Index = vx.Index;
                                vy.DataStringValue = vx.DataStringValue;
                                oVals.Add(vy);
                            }
                        }// vx
                        if (oVals.Count > 0)
                        {
                            xRet = xRet && writeValues(ctx, oVals);
                        }
                    }// oVar
                    if (xRet)
                    {
                        ctx.SaveChanges();
                        bRet = true;
                    }
                }// ctx
            }
            catch (Exception ex)
            {
                err = ex;
            }
            return new Tuple<bool, Exception>(bRet, err);
        }
        #endregion // IStoreDataManager implementation

        #region Photos
        public Tuple<int, Exception> GetPhotosCount()
        {
            int nRet = 0;
            Exception err = null;
            try
            {
                using (var ctx = getContext())
                {
                    nRet = ctx.DbPhotoes.Count();
                }// ctx
            }
            catch (Exception ex)
            {
                err = ex;
            }
            return new Tuple<int, Exception>(nRet, err);
        }
        public Tuple<IEnumerable<PhotoDesc>, Exception> GetPhotos(int skip, int taken)
        {
            List<PhotoDesc> oRet = new List<PhotoDesc>();
            Exception err = null;
            try
            {
                using (var ctx = getContext())
                {
                    int nMax = ctx.DbPhotoes.Count();
                    if (skip < 0)
                    {
                        skip = 0;
                    }
                    if (taken < 0)
                    {
                        taken = 0;
                    }
                    if ((skip + taken) > nMax)
                    {
                        taken = nMax - skip;
                        if (taken < 0)
                        {
                            taken = 0;
                        }
                    }
                    if (taken == 0)
                    {
                        if (skip > 0)
                        {
                            var q = (from x in ctx.DbPhotoes orderby x.Name ascending select x).Skip(skip);
                            foreach (var p in q)
                            {
                                PhotoDesc pp = new PhotoDesc();
                                convertPhoto(p, pp);
                                oRet.Add(pp);
                            }// p
                        }
                        else
                        {
                            var q = from x in ctx.DbPhotoes orderby x.Name ascending select x;
                            foreach (var p in q)
                            {
                                PhotoDesc pp = new PhotoDesc();
                                convertPhoto(p, pp);
                                oRet.Add(pp);
                            }// p
                        }

                    }
                    else
                    {
                        if (skip > 0)
                        {
                            var q = (from x in ctx.DbPhotoes orderby x.Name ascending select x).Skip(skip).Take(taken);
                            foreach (var p in q)
                            {
                                PhotoDesc pp = new PhotoDesc();
                                convertPhoto(p, pp);
                                oRet.Add(pp);
                            }// p
                        }
                        else
                        {
                            var q = (from x in ctx.DbPhotoes orderby x.Name ascending select x).Take(taken);
                            foreach (var p in q)
                            {
                                PhotoDesc pp = new PhotoDesc();
                                convertPhoto(p, pp);
                                oRet.Add(pp);
                            }// p
                        }
                    }
                }// ctx
            }
            catch (Exception ex)
            {
                err = ex;
            }
            return new Tuple<IEnumerable<PhotoDesc>, Exception>(oRet, err);
        }
        public Tuple<PhotoDesc, Exception> FindPhoto(PhotoDesc oPhoto)
        {
            PhotoDesc pRet = null;
            Exception err = null;
            if (oPhoto == null)
            {
                return new Tuple<PhotoDesc, Exception>(null, new ArgumentNullException());
            }
            try
            {
                using (var ctx = getContext())
                {
                    DbPhoto p = findPhoto(ctx, oPhoto);
                    if (p != null)
                    {
                        pRet = new PhotoDesc();
                        convertPhoto(p, pRet);
                    }
                }// ctx
            }
            catch (Exception ex)
            {
                err = ex;
            }
            return new Tuple<PhotoDesc, Exception>(pRet, err);
        }
        public Tuple<IEnumerable<PhotoDesc>, Exception> GetPhotos(IEnumerable<PhotoDesc> oPhotos)
        {
            if (oPhotos == null)
            {
                return new Tuple<IEnumerable<PhotoDesc>, Exception>(null, new ArgumentNullException());
            }
            List<PhotoDesc> oRet = new List<PhotoDesc>();
            Exception err = null;
            try
            {
                using (var ctx = getContext())
                {
                    foreach (var oPhoto in oPhotos)
                    {
                        DbPhoto p = findPhoto(ctx, oPhoto);
                        if (p != null)
                        {
                            PhotoDesc pp = new PhotoDesc();
                            convertPhoto(p, pp);
                            oRet.Add(pp);
                        }
                    }// oPhoto
                }// ctx
            }
            catch (Exception ex)
            {
                err = ex;
            }
            return new Tuple<IEnumerable<PhotoDesc>, Exception>(oRet, err);
        }
        public Tuple<PhotoDesc, Exception> MaintainsPhoto(PhotoDesc oPhoto)
        {
            if (oPhoto == null)
            {
                return new Tuple<PhotoDesc, Exception>(null, new ArgumentNullException());
            }
            if (!oPhoto.IsWriteable)
            {
                return new Tuple<PhotoDesc, Exception>(null, new ArgumentException());
            }
            PhotoDesc pRet = null;
            Exception err = null;
            try
            {
                using (var ctx = getContext())
                {
                    DbPhoto p = findPhoto(ctx, oPhoto);
                    if (p != null)
                    {
                        p.Name = oPhoto.Name;
                        p.DataBytes = oPhoto.DataBytes;
                        ctx.SaveChanges();
                    }
                    else
                    {
                        p = new DbPhoto();
                        p.Id = nextId(ctx, TAB_PHOTO);
                        p.Name = oPhoto.Name;
                        p.DataBytes = oPhoto.DataBytes;
                        ctx.DbPhotoes.Add(p);
                        ctx.SaveChanges();
                    }
                    pRet = new PhotoDesc();
                    convertPhoto(p, pRet);
                }// ctd
            }
            catch (Exception ex)
            {
                err = ex;
            }
            return new Tuple<PhotoDesc, Exception>(pRet, err);
        }// MaintainsPhoto
        public Tuple<bool, Exception> RemovePhoto(PhotoDesc oPhoto)
        {
            if (oPhoto == null)
            {
                return new Tuple<bool, Exception>(false, new ArgumentNullException());
            }
            bool bRet = false;
            Exception err = null;
            try
            {
                using (var ctx = getContext())
                {
                    DbPhoto p = findPhoto(ctx, oPhoto);
                    if (p != null)
                    {
                        ctx.DbPhotoes.Remove(p);
                        ctx.SaveChanges();
                        bRet = true;
                    }
                }// cts
            }
            catch (Exception ex)
            {
                err = ex;
            }
            return new Tuple<bool, Exception>(bRet, err);
        }
        public Tuple<IEnumerable<PhotoDesc>, Exception> SearchPhotos(string searchString, int skip, int taken)
        {
            String ss = String.IsNullOrEmpty(searchString) ? null : searchString.Trim().ToLower();
            if (String.IsNullOrEmpty(ss))
            {
                return GetPhotos(skip, taken);
            }
            List<PhotoDesc> oRet = null;
            Exception err = null;
            try
            {
                if (skip < 0)
                {
                    skip = 0;
                }
                if (taken < 0)
                {
                    taken = 0;
                }

                using (var ctx = getContext())
                {
                    oRet = new List<PhotoDesc>();
                    IEnumerable<DbPhoto> q = null;
                    if (taken < 1)
                    {
                        if (skip > 0)
                        {
                            q = (from x in ctx.DbPhotoes
                                 where x.Name.Trim().ToLower().Contains(ss)
                                 orderby x.Name ascending
                                 select x).Skip(skip);
                        }
                        else
                        {
                            q = from x in ctx.DbPhotoes
                                where x.Name.Trim().ToLower().Contains(ss)
                                orderby x.Name ascending
                                select x;
                        }
                    }
                    else
                    {
                        if (skip > 0)
                        {
                            q = (from x in ctx.DbPhotoes
                                 where x.Name.Trim().ToLower().Contains(ss)
                                 orderby x.Name ascending
                                 select x).Skip(skip).Take(taken);
                        }
                        else
                        {
                            q = (from x in ctx.DbPhotoes
                                 where x.Name.Trim().ToLower().Contains(ss)
                                 orderby x.Name ascending
                                 select x).Take(taken);
                        }
                    }
                    foreach (var p in q)
                    {
                        PhotoDesc pp = new PhotoDesc();
                        convertPhoto(p, pp);
                        oRet.Add(pp);
                    }// p
                }// ctx
            }// try
            catch (Exception ex)
            {
                err = ex;
            }
            return new Tuple<IEnumerable<PhotoDesc>, Exception>(oRet, err);
        }//SearchPhotos
        public Tuple<int, Exception> SearchPhotosCount(string searchString)
        {
            String ss = String.IsNullOrEmpty(searchString) ? null : searchString.Trim().ToLower();
            if (String.IsNullOrEmpty(ss))
            {
                return GetPhotosCount();
            }
            int nRet = 0;
            Exception err = null;
            try
            {
                using (var ctx = getContext())
                {
                    var q = from x in ctx.DbPhotoes
                            where x.Name.Trim().ToLower().Contains(ss)
                            select x;
                    nRet = q.Count();
                }
            }
            catch (Exception ex)
            {
                err = ex;
            }
            return new Tuple<int, Exception>(nRet, err);
        }
        #endregion // Photos














    }// class DomainDataManager
}
