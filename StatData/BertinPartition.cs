using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatData
{
    public class PartitionData
    {
        public PartitionData() { }
        public int ClassesCount { get; set; }
        public float[] Limits { get; set; }
        public int[] Classes { get; set; }
    }// class PartitionData
    public class BertinPartition
    {
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
            if ((nClasses % 2) == 0)
            {
                ++nClasses;
            }
            float vMin = (from x in data select x).Min();
            float vMax = (from x in data select x).Max();
            if (vMin >= vMax)
            {
                return null;
            }
            float vMean = (from x in data select x).Average();
            PartitionData oRet = new PartitionData();
            oRet.ClassesCount = nClasses;
            oRet.Limits = new float[nClasses + 1];
            oRet.Classes = new int[n];
            float dn = (float)2.5;
            float deltaSup = (float)((vMax - vMean)/dn);
            float deltaInf = (float)((vMean - vMin) / dn);
            int iSup = nClasses;
            int iInf = 0;
            float fMin =vMin;
            float fMax = vMax;
            while (iInf < iSup)
            {
                oRet.Limits[iInf++] = fMin;
                oRet.Limits[iSup--] = fMax;
                fMin = (float)(fMin + deltaInf);
                fMax = (float)(fMax - deltaSup);
            }// while
            int zMax = nClasses - 1;
            for (int i = 0; i < n; ++i)
            {
                float x = data[i];
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
            float[] xdata = new float[n];
            for (int i = 0; i < n; ++i)
            {
                xdata[i] = (float)data[i];
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
            float[] xdata = new float[n];
            for (int i = 0; i < n; ++i)
            {
                xdata[i] = (float)data[i];
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
            float[] xdata = new float[n];
            for (int i = 0; i < n; ++i)
            {
                xdata[i] = (float)data[i];
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
                    if (String.IsNullOrEmpty(s))
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
                    if (String.IsNullOrEmpty(s))
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
