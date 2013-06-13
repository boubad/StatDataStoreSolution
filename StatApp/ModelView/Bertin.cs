using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StatData;
namespace StatApp.ModelView
{
    public class PartitionData
    {
        public PartitionData() { }
        public int ClassesCount { get; set; }
        public double[] Limits { get; set; }
        public int[] Classes { get; set; }
    }// class PartitionData
    public class BertinPartition
    {
        public static double[] GetRanks(IEnumerable<double> data)
        {
            var col = data.ToArray();
            int n = col.Length;
            List<Tuple<int, double>> oList = new List<Tuple<int, double>>();
            for (int i = 0; i < n; ++i)
            {
                Tuple<int, double> t = new Tuple<int, double>(i, col[i]);
                oList.Add(t);
            }// i
            oList.Sort((t1, t2) => {
                double x1 = t1.Item2;
                double x2 = t2.Item2;
                int nRet = 0;
                if (x1 < x2)
                {
                    nRet = -1;
                }
                else if (x1 > x2)
                {
                    nRet = 1;
                }
                return nRet;
            });
            int[] indexes = new int[n];
            for (int i = 0; i < n; ++i)
            {
                indexes[i] = (oList[i]).Item1;
            }// i
            double dLast = 0.0;
            int iLast = 1;
            int nc = 0;
            double[] dd = new double[n];
            int icur = 0;
            while (icur < n)
            {
                int ii = indexes[icur];
                double x = col[ii];
                if (icur == 0)
                {
                    dLast = x;
                }
                while (x == dLast)
                {
                    dd[ii] = iLast;
                    ++nc;
                    ++icur;
                    if (icur >= n)
                    {
                        break;
                    }
                    ii = indexes[icur];
                    x = col[ii];
                }// x
                if (icur >= n)
                {
                    break;
                }
                dLast = x;
                iLast += nc;
                nc = 0;
            }// icur
            return dd;
        }// getRanks
        public static Dictionary<VariableDesc, Dictionary<int, int>> GetPartition(Dictionary<VariableDesc, ValueDescs> oData,
            int nClasses)
        {
            Dictionary<VariableDesc, Dictionary<int, int>> oRet = new Dictionary<VariableDesc, Dictionary<int, int>>();
            var keys = oData.Keys;
            foreach (var key in keys)
            {
                oRet[key] = new Dictionary<int, int>();
                var vals = (oData[key]).ToArray();
                int n = vals.Length;
                if (key.IsNumVar)
                {
                    double[] data = new double[n];
                    for (int i = 0; i < n; ++i)
                    {
                        var v = vals[i];
                        data[i] = v.DoubleValue;
                    }// i
                    var p = GetPartition(data, nClasses);
                    if (p == null)
                    {
                        return null;
                    }
                    var cc = p.Classes;
                    var dict = oRet[key];
                    for (int i = 0; i < n; ++i)
                    {
                        var v = vals[i];
                        int ind = v.Index;
                        dict[ind] = cc[i];
                    }// i
                }
                else
                {
                    String[] data = new String[n];
                    for (int i = 0; i < n; ++i)
                    {
                        var v = vals[i];
                        data[i] = v.StringValue;
                    }// i
                    var p = GetPartition(data);
                    if (p == null)
                    {
                        return null;
                    }
                    var cc = p.Classes;
                    var dict = oRet[key];
                    for (int i = 0; i < n; ++i)
                    {
                        var v = vals[i];
                        int ind = v.Index;
                        dict[ind] = cc[i];
                    }// i
                }
            }
            return oRet;
        }//GetDoublePartition
        public static Dictionary<VariableDesc, Dictionary<int, int>> GetDoublePartition(Dictionary<VariableDesc, ValueDescs> oData,
            int nClasses)
        {
            Dictionary<VariableDesc, Dictionary<int, int>> oRet = new Dictionary<VariableDesc, Dictionary<int, int>>();
            var keys = oData.Keys;
            foreach (var key in keys) 
            {
                oRet[key] = new Dictionary<int, int>();
                var vals = (oData[key]).ToArray();
                int n = vals.Length;
                double[] data = new double[n];
                for (int i = 0; i < n; ++i)
                {
                    var v = vals[i];
                    data[i] = v.DoubleValue;
                }// i
                var p = GetPartition(data, nClasses);
                if (p == null)
                {
                    return null;
                }
                var cc = p.Classes;
                var dict = oRet[key];
                for (int i = 0; i < n; ++i)
                {
                    var v = vals[i];
                    int ind = v.Index;
                    dict[ind] = cc[i];
                }// i
            }
            return oRet;
        }//GetDoublePartition
        public static Dictionary<VariableDesc, Dictionary<int, int>> GetDoublePartition(Dictionary<VariableDesc, ValueDescs> oData)
        {
            Dictionary<VariableDesc, Dictionary<int, int>> oRet = new Dictionary<VariableDesc, Dictionary<int, int>>();
            var keys = oData.Keys;
            foreach (var key in keys)
            {
                oRet[key] = new Dictionary<int, int>();
                var vals = (oData[key]).ToArray();
                int n = vals.Length;
                String[] data = new String[n];
                for (int i = 0; i < n; ++i)
                {
                    var v = vals[i];
                    data[i] = v.StringValue;
                }// i
                var p = GetPartition(data);
                if (p == null)
                {
                    return null;
                }
                var cc = p.Classes;
                var dict = oRet[key];
                for (int i = 0; i < n; ++i)
                {
                    var v = vals[i];
                    int ind = v.Index;
                    dict[ind] = cc[i];
                }// i
            }
            return oRet;
        }//GeStringPartition
        public static PartitionData GetPartition(double[] data, int nClasses)
        {
            if ((data == null) || (nClasses < 1))
            {
                return null;
            }
            int n = data.Length;
            if (n < 2)
            {
                return null;
            }
            if ((nClasses % 2) == 0)
            {
                ++nClasses;
            }
            double vMin = (from x in data select x).Min();
            double vMax = (from x in data select x).Max();
            if (vMin >= vMax)
            {
                return null;
            }
            double vMean = (from x in data select x).Average();
            PartitionData oRet = new PartitionData();
            oRet.ClassesCount = nClasses;
            oRet.Limits = new double[nClasses + 1];
            oRet.Classes = new int[n];
            double dn = (double)2.5;
            double deltaSup = (double)((vMax - vMean) / dn);
            double deltaInf = (double)((vMean - vMin) / dn);
            int iSup = nClasses;
            int iInf = 0;
            double fMin = vMin;
            double fMax = vMax;
            while (iInf < iSup)
            {
                oRet.Limits[iInf++] = fMin;
                oRet.Limits[iSup--] = fMax;
                fMin = (double)(fMin + deltaInf);
                fMax = (double)(fMax - deltaSup);
            }// while
            int zMax = nClasses - 1;
            for (int i = 0; i < n; ++i)
            {
                double x = data[i];
                if (x <= vMin)
                {
                    oRet.Classes[i] = 0;
                }
                else if (x >= vMax)
                {
                    oRet.Classes[i] = zMax;
                }
                else
                {
                    for (int j = 0; j < nClasses; ++j)
                    {
                        if ((x >= oRet.Limits[j]) && (x < oRet.Limits[j + 1]))
                        {
                            oRet.Classes[i] = j;
                            break;
                        }
                    }// j
                }
            }// i
            return oRet;
        }// GetPartition
        public static PartitionData GetPartition(float[] data, int nClasses)
        {
            if ((data == null) || (nClasses < 1))
            {
                return null;
            }
            int n = data.Length;
            if (n < 2)
            {
                return null;
            }
            double[] xdata = new double[n];
            for (int i = 0; i < n; ++i)
            {
                xdata[i] = (double)data[i];
            }
            return GetPartition(xdata, nClasses);
        }// GetPartition
        public static PartitionData GetPartition(int[] data, int nClasses)
        {
            if ((data == null) || (nClasses < 1))
            {
                return null;
            }
            int n = data.Length;
            if (n < 2)
            {
                return null;
            }
            double[] xdata = new double[n];
            for (int i = 0; i < n; ++i)
            {
                xdata[i] = (double)data[i];
            }
            return GetPartition(xdata, nClasses);
        }// GetPartition
        public static PartitionData GetPartition(short[] data, int nClasses)
        {
            if ((data == null) || (nClasses < 1))
            {
                return null;
            }
            int n = data.Length;
            if (n < 2)
            {
                return null;
            }
            double[] xdata = new double[n];
            for (int i = 0; i < n; ++i)
            {
                xdata[i] = (double)data[i];
            }
            return GetPartition(xdata, nClasses);
        }// GetPartition
        public static PartitionData GetPartition(String[] data)
        {
            if (data == null)
            {
                return null;
            }
            Dictionary<String, int> oDict = new Dictionary<string, int>();
            int n = data.Length;
            int iCur = 0;
            for (int i = 0; i < n; ++i)
            {
                String s = data[i];
                if (!String.IsNullOrEmpty(s))
                {
                    s = s.Trim().ToLower();
                    if (!String.IsNullOrEmpty(s))
                    {
                        if (!oDict.ContainsKey(s))
                        {
                            oDict[s] = iCur++;
                        }
                    }
                }
            }// i
            if (iCur < 2)
            {
                return null;
            }
            PartitionData oRet = new PartitionData();
            oRet.ClassesCount = iCur;
            oRet.Classes = new int[n];
            for (int i = 0; i < n; ++i)
            {
                String s = data[i];
                if (!String.IsNullOrEmpty(s))
                {
                    s = s.Trim().ToLower();
                    if (!String.IsNullOrEmpty(s))
                    {
                        if (!oDict.ContainsKey(s))
                        {
                            oRet.Classes[i] = -1;
                        }
                        else
                        {
                            oRet.Classes[i] = oDict[s];
                        }
                    }
                }
            }// i
            return oRet;
        }// GetPartition
        public static PartitionData GetPartition(bool[] data)
        {
            if (data == null)
            {
                return null;
            }
            int n = data.Length;
            if (n < 2)
            {
                return null;
            }
            PartitionData oRet = new PartitionData();
            oRet.ClassesCount = 2;
            oRet.Classes = new int[n];
            for (int i = 0; i < n; ++i)
            {
                if (data[i])
                {
                    oRet.Classes[i] = 1;
                }
                else
                {
                    oRet.Classes[i] = 0;
                }
            }// i
            return oRet;
        }// GetPartition
    }// class BertinPartition
}
