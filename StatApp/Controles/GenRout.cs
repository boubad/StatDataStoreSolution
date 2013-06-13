using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using MathNet.Numerics.Statistics;
using MathNet.Numerics.Random;

namespace StatApp.Controles
{
    public static class GenRout
    {
        private static Mrg32k3a mrg32K3A = new Mrg32k3a();
        //
        public static void ComputeProfilDistances(double[,] data, double[,] colDist, double[,] rowDist)
        {
            if ((data != null) && (colDist != null) && (rowDist != null))
            {
                int nr = data.GetLength(0);
                int nv = data.GetLength(1);
                if ((nr > 0) && (nv > 0) && (colDist.GetLength(0) >= nv) && (colDist.GetLength(1) >= nv) && (rowDist.GetLength(0) >= nr)
                    && (rowDist.GetLength(1) >= nr))
                {
                    double[,] pData = ComputeProfils(data);
                    if (pData != null)
                    {
                        Parallel.Invoke(() =>
                        {
                            double[] p = new double[nv];
                            if (p != null)
                            {
                                double f = 1.0 / nv;
                                for (int i = 0; i < nv; ++i)
                                {
                                    p[i] = f;
                                }
                                ComputeDistances(pData, p, rowDist);
                            }
                        },
                            () =>
                            {
                                double[,] pTrans = Transpose(pData);
                                if (pTrans != null)
                                {
                                    double[] p = new double[nr];
                                    if (p != null)
                                    {
                                        float f = (float)(1.0 / nr);
                                        for (int i = 0; i < nr; ++i)
                                        {
                                            p[i] = f;
                                        }
                                        ComputeDistances(pTrans, p, colDist);
                                    }
                                }
                            });
                    }// pData
                }// size
            }// arg
        }//ComputeProfilDistances
        public static void NormalizeData(double[,] data, double[] pWeights, double[,] pNormData)
        {
            int nr = data.GetLength(0);
            int nv = data.GetLength(1);
            Parallel.For((int)0, nv, ivar =>
            {
                double[] curdata = new double[nr];
                for (int i = 0; i < nr; ++i)
                {
                    curdata[i] = data[i, ivar];
                }
                double mean = curdata.Average();
                double s2 = curdata.Average(x => (x - mean) * (x - mean));
                pWeights[ivar] = (float)(1.0 / s2);
                double ec = Math.Sqrt(s2);
                if (ec > 0.0)
                {
                    for (int i = 0; i < nr; ++i)
                    {
                        pNormData[i, ivar] = (curdata[i] - mean) / ec;
                    }
                }// ec
            });
            double ss = pWeights.Sum();
            for (int i = 0; i < nv; ++i)
            {
                pWeights[i] = pWeights[i] / ss;
            }
        }// NormalizeData
        public static void NormalizeData(double[,] data, double[] pWeights, double[,] corr, double[,] pNormData)
        {
            int nr = data.GetLength(0);
            int nv = data.GetLength(1);
            double[] ecs = new double[nv];
            double[,] temp = new double[nr, nv];
            Parallel.For((int)0, nv, ivar =>
            {
                double[] curdata = new double[nr];
                for (int i = 0; i < nr; ++i)
                {
                    curdata[i] = data[i, ivar];
                }
                double mean = curdata.Average();
                double s2 = curdata.Average(x => (x - mean) * (x - mean));
                pWeights[ivar] = (float)(1.0 / s2);
                double ec = Math.Sqrt(s2);
                ecs[ivar] = ec;
                if (ec > 0.0)
                {
                    for (int i = 0; i < nr; ++i)
                    {
                        double x = curdata[i] - mean;
                        temp[i, ivar] = x;
                        pNormData[i, ivar] = x / ec;
                    }
                }
            });
            Parallel.For((int)0, nv, ivar =>
            {
                corr[ivar, ivar] = (float)1.0;
                double ec1 = ecs[ivar];
                if (ec1 > 0.0)
                {
                    for (int ivar2 = 0; ivar2 < ivar; ++ivar2)
                    {
                        double ec2 = ecs[ivar2];
                        if (ec2 > 0.0)
                        {
                            double somme = 0;
                            for (int irow = 0; irow < nr; ++irow)
                            {
                                double x = temp[irow, ivar] * temp[irow, ivar2];
                                somme += x;
                            }// irow
                            somme /= nr;
                            double c = somme / (ec1 * ec2);
                            corr[ivar, ivar2] = c;
                            corr[ivar2, ivar] = c;
                        }// ec2
                    }// ivar2
                }// ec1
            });
            double ss = pWeights.Sum();
            for (int i = 0; i < nv; ++i)
            {
                pWeights[i] = pWeights[i] / ss;
            }
        }// NormalizeData
        public static void ComputeClasses(int nClasses, double[,] data, bool bGlobal, int[,] classes)
        {
            int nr = data.GetLength(0);
            int nv = data.GetLength(1);
            int nc = nClasses;
            if ((nc % 2) == 0)
            {
                nc++;
            }
            double[] fLimits = new double[nc + 1];
            if (bGlobal)
            {
                double vMin = data[0, 0];
                double vMax = data[0, 0];
                double mean = 0;
                for (int i = 0; i < nr; ++i)
                {
                    for (int j = 0; j < nv; ++j)
                    {
                        double x = data[i, j];
                        mean += x;
                        if (x < vMin)
                        {
                            vMin = x;
                        }
                        else if (x > vMax)
                        {
                            vMax = x;
                        }
                    }// j
                }// i
                mean /= nr * nv;
                double delta = (vMax - vMin) / nc;
                double delta2 = delta / 2;
                int n1 = nc / 2;
                int n2 = n1 + 1;
                double f1 = mean - delta2;
                double f2 = f1 + delta;
                while ((n1 >= 0) && (n2 <= nc))
                {
                    fLimits[n1--] = f1;
                    fLimits[n2++] = f2;
                    f1 -= delta;
                    if (f1 < vMin)
                    {
                        f1 = vMin;
                    }
                    f2 += delta;
                    if (f2 > vMax)
                    {
                        f2 = vMax;
                    }
                }// while
                Parallel.For((int)0, nr, irow =>
                {
                    Parallel.For((int)0, nv, ivar =>
                    {
                        double x = data[irow, ivar];
                        int k = -1;
                        if (x <= vMin)
                        {
                            k = 0;
                        }
                        else if (x >= vMax)
                        {
                            k = nc - 1;
                        }
                        else
                        {
                            for (int i = 0; i <= nc; ++i)
                            {
                                if (x <= fLimits[i])
                                {
                                    k = i;
                                    break;
                                }
                            }// i
                        }
                        if (k < 0)
                        {
                            k = 0;
                        }
                        else if (k >= nc)
                        {
                            k = nc - 1;
                        }
                        classes[irow, ivar] = k;
                    });
                });

            }
            else
            {
                Parallel.For((int)0, nv, ivar =>
                {
                    double[] curdata = new double[nr];
                    for (int i = 0; i < nr; ++i)
                    {
                        curdata[i] = data[i, ivar];
                    }
                    double vMin = curdata.Min();
                    double vMax = curdata.Max();
                    double mean = curdata.Average();
                    double delta = (vMax - vMin) / nc;
                    double delta2 = delta / 2;
                    int n1 = nc / 2;
                    int n2 = n1 + 1;
                    double f1 = mean - delta2;
                    double f2 = f1 + delta;
                    while ((n1 >= 0) && (n2 <= nc))
                    {
                        fLimits[n1--] = f1;
                        fLimits[n2++] = f2;
                        f1 -= delta;
                        if (f1 < vMin)
                        {
                            f1 = vMin;
                        }
                        f2 += delta;
                        if (f2 > vMax)
                        {
                            f2 = vMax;
                        }
                    }// while
                    Parallel.For((int)0, nr, irow =>
                    {
                        double x = data[irow, ivar];
                        int k = -1;
                        if (x <= vMin)
                        {
                            k = 0;
                        }
                        else if (x >= vMax)
                        {
                            k = nc - 1;
                        }
                        else
                        {
                            for (int i = 0; i <= nc; ++i)
                            {
                                if (x <= fLimits[i])
                                {
                                    k = i;
                                    break;
                                }
                            }// i
                        }
                        if (k < 0)
                        {
                            k = 0;
                        }
                        else if (k >= nc)
                        {
                            k = nc - 1;
                        }
                        classes[irow, ivar] = k;
                    });
                });
            }
        }// ComputeClasses
        public static void ComputeDistances(double[,] data, double[] pWeights, double[,] pDist)
        {
            int nr = data.GetLength(0);
            int nv = data.GetLength(1);
            Parallel.For((int)0, nr, irow1 =>
            {
                pDist[irow1, irow1] = 0;
                Parallel.For((int)0, irow1, irow2 =>
                {
                    double s = 0;
                    for (int i = 0; i < nv; ++i)
                    {
                        double x = pWeights[i] * (data[irow1, i] - data[irow2, i]);
                        s += x * x;
                    }// i
                    s /= nv;
                    pDist[irow1, irow2] = s;
                    pDist[irow2, irow1] = s;
                });
            });
        }// ComputeDistances
        public static int NextRandom(int nMin, int nMax)
        {
            return mrg32K3A.Next(nMin, nMax);
        }
       
        public static double[,] computeCorrelations(double[,] data)
        {
            double[,] vRet = null;
            if (data != null)
            {
                int nr = data.GetLength(0);
                int nv = data.GetLength(1);
                if ((nv > 1) && (nr > 0))
                {
                    List<double[]> oList = new List<double[]>(nv);
                    for (int ivar = 0; ivar < nv; ++ivar)
                    {
                        double[] d = new double[nr];
                        for (int i = 0; i < nr; ++i)
                        {
                            d[i] = data[i, ivar];
                        }
                        oList.Add(d);
                    }
                    vRet = new double[nv, nv];
                    Parallel.For((int)0, nv, ivar =>
                    {
                        vRet[ivar, ivar] = 1;
                        double[] d1 = oList[ivar];
                        for (int j = 0; j < ivar; ++j)
                        {
                            double[] d2 = oList[j];
                            float c = (float)Correlation.Pearson(d1, d2);
                            vRet[ivar, j] = c;
                            vRet[j, ivar] = c;
                        }// j
                    });
                }// size
            }// data
            return vRet;
        }// ComputeCorrelations
        public static double[,] computeCorrelationsDistances(double[,] data)
        {
            double[,] vRet = null;
            double[,] corr = computeCorrelations(data);
            if (corr != null)
            {
                int n = corr.GetLength(0);
                vRet = new double[n, n];
                if (vRet != null)
                {
                    for (int i = 0; i < n; ++i)
                    {
                        vRet[i, i] = 0;
                        for (int j = 0; j < i; ++j)
                        {
                            double c = 1.0 - corr[i, j];
                            vRet[i, j] = c;
                            vRet[j, i] = c;
                        }// j
                    }// i
                }// vRet
            }// corr
            return vRet;
        }// computeCorrelationsDistances
        public static double[,] Transpose(double[,] data)
        {
            double[,] vRet = null;
            if (data != null)
            {
                int nr = data.GetLength(0);
                int nv = data.GetLength(1);
                vRet = new double[nv, nr];
                if (vRet != null)
                {
                    Parallel.For((int)0, nr, irow =>
                    {
                        for (int i = 0; i < nv; ++i)
                        {
                            vRet[i, irow] = data[irow, i];
                        }
                    });
                }// vRet
            }// data
            return vRet;
        }// Transpose
        public static bool ComputeClasses(double[,] data, int nc, int[,] classes, bool bCommon)
        {
            if ((data == null) || (classes == null) || (nc < 1) || ((nc % 2) == 0))
            {
                return false;
            }
            int nr = data.GetLength(0);
            int nv = data.GetLength(1);
            if ((classes.GetLength(0) < nr) || (classes.GetLength(1) < nv) || (nr < 1) || (nv < 1))
            {
                return false;
            }
            if (bCommon)
            {
                double vMin = data[0, 0];
                double vMax = vMin;
                double somme = 0;
                for (int i = 0; i < nr; ++i)
                {
                    for (int j = 0; j < nv; ++j)
                    {
                        double x = data[i, j];
                        somme += x;
                        if (x < vMin)
                        {
                            vMin = x;
                        }
                        else if (x > vMax)
                        {
                            vMax = x;
                        }
                    }// j
                }// i
                if (vMin > vMax)
                {
                    return false;
                }
                double mean = somme / (nv * nr);
                double[] flimits = new double[nc + 1];
                if (!ComputeClassesLimits(nc, vMin, vMax, mean, flimits))
                {
                    return false;
                }
                Parallel.For((int)0, nv, (int ivar) =>
                {
                    int[] cc = new int[nr];
                    double[] dd = new double[nr];
                    for (int i = 0; i < nr; ++i)
                    {
                        dd[i] = data[i, ivar];
                    }// i
                    ComputeClasses(dd, flimits, cc);
                    for (int i = 0; i < nr; ++i)
                    {
                        classes[i, ivar] = cc[i];
                    }// i
                });
            }
            else
            {
                Parallel.For((int)0, nv, (int ivar) =>
                {
                    double[] dd = new double[nr];
                    for (int i = 0; i < nr; ++i)
                    {
                        dd[i] = data[i, ivar];
                    }// i
                    double vMin = dd.Min();
                    double vMax = dd.Max();
                    if (vMin < vMax)
                    {
                        double mean = dd.Average();
                        double[] flimits = new double[nc + 1];
                        if (ComputeClassesLimits(nc, vMin, vMax, mean, flimits))
                        {
                            int[] cc = new int[nr];
                            ComputeClasses(dd, flimits, cc);
                            for (int i = 0; i < nr; ++i)
                            {
                                classes[i, ivar] = cc[i];
                            }// i
                        }// 
                    }// vMean
                });
            }
            return true;
        }// ComputeClasses
        public static bool ComputeClasses(double[] data, double[] flimits, int[] classes)
        {
            if ((data == null) || (flimits == null) || (classes == null))
            {
                return false;
            }
            int n = data.Length;
            if (classes.Length < n)
            {
                return false;
            }
            int nv = flimits.Length - 1;
            Parallel.For((int)0, n, (int i) =>
            {
                int nRes = -1;
                double x = data[i];
                for (int j = 0; j < nv; ++j)
                {
                    if ((x >= flimits[j]) && (x < flimits[j + 1]))
                    {
                        nRes = j;
                        break;
                    }
                }// j
                classes[i] = nRes;
            });
            return true;
        }// ComputeClasses
        public static bool ComputeClassesLimits(int nc, double vMin, double vMax, double mean, double[] flimits)
        {
            if ((nc < 1) || (vMin >= vMax) || (flimits == null) || (mean < vMin) || (mean > vMax))
            {
                return false;
            }
            if (((nc % 2) == 0) || (flimits.Length < (nc + 1)))
            {
                return false;
            }
            int n2 = nc / 2;
            double dx1 = 2 * (mean - vMin) / (2 * n2 + 1);
            double dx2 = 2 * (vMax - mean) / (2 * n2 + 1);
            flimits[n2] = mean - (dx1 / 2.0);
            flimits[n2 + 1] = mean + (dx2 / 2.0);
            for (int i = n2; i > 0; --i)
            {
                flimits[i - 1] = flimits[i] - dx1;
            }// i
            flimits[0] = vMin;
            for (int i = n2 + 2; i <= nc; ++i)
            {
                flimits[i] = flimits[i - 1] + dx2;
            }// i
            flimits[nc] = vMax;
            return true;
        }// ComputeClassesLimits
        public static int[,] recodeInt(double[,] data, int nMax, int nMin)
        {
            int[,] vRet = null;
            if ((data != null) && (nMax > nMin))
            {
                int nr = data.GetLength(0);
                int nv = data.GetLength(1);
                double vMin = data[0, 0];
                double vMax = data[0, 0];
                for (int i = 0; i < nr; ++i)
                {
                    for (int j = 0; j < nv; ++j)
                    {
                        double v = data[i, j];
                        if (v < vMin)
                        {
                            vMin = v;
                        }
                        else if (v > vMax)
                        {
                            vMax = v;
                        }
                    }// j
                }// i
                if (vMin < vMax)
                {
                    vRet = new int[nr, nv];
                    if (vRet != null)
                    {
                        double delta = ((double)nMax - (double)nMin) / (vMax - vMin);
                        for (int i = 0; i < nr; ++i)
                        {
                            for (int j = 0; j < nv; ++j)
                            {
                                vRet[i, j] = (int)(((data[i, j] - vMin) * delta) + nMin);
                            }// j
                        }// i
                    }// vRet
                }// ok
            }// data
            return vRet;
        }// recodeInt
        public static void recodeInt(double[,] data, int nMax, int nMin, int[,] classes)
        {
            int[,] vRet = classes;
            if ((data != null) && (nMax > nMin))
            {
                int nr = data.GetLength(0);
                int nv = data.GetLength(1);
                double vMin = data[0, 0];
                double vMax = data[0, 0];
                for (int i = 0; i < nr; ++i)
                {
                    for (int j = 0; j < nv; ++j)
                    {
                        double v = data[i, j];
                        if (v < vMin)
                        {
                            vMin = v;
                        }
                        else if (v > vMax)
                        {
                            vMax = v;
                        }
                    }// j
                }// i
                if (vMin < vMax)
                {
                    if (vRet != null)
                    {
                        double delta = ((double)nMax - (double)nMin) / (vMax - vMin);
                        for (int i = 0; i < nr; ++i)
                        {
                            for (int j = 0; j < nv; ++j)
                            {
                                vRet[i, j] = (int)(((data[i, j] - vMin) * delta) + nMin);
                            }// j
                        }// i
                    }// vRet
                }// ok
            }// data
        }// recodeInt
        public static double[,] ComputeProfils(double[,] data)
        {
            double[,] vRet = null;
            if (data != null)
            {
                int nr = data.GetLength(0);
                int nv = data.GetLength(1);
                vRet = new double[nr, nv];
                if (vRet != null)
                {
                    Parallel.For((int)0, nv, ivar =>
                    {
                        double[] d = new double[nr];
                        if (d != null)
                        {
                            for (int i = 0; i < nr; ++i)
                            {
                                d[i] = data[i, ivar];
                            }
                            double vMin = d.Min();
                            double vMax = d.Max();
                            if (vMax > vMin)
                            {
                                double delta = 1.0 / (vMax - vMin);
                                for (int i = 0; i < nr; ++i)
                                {
                                    vRet[i, ivar] = (d[i] - vMin) * delta;
                                }
                            }// ok
                        }// d
                    });
                }// vRet
            }// data
            return vRet;
        }// compute Profils
    }// class Genrout
}
