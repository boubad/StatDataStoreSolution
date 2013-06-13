using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//////////////////////
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Generic.Factorization;
/////////////////////////////////////////
using StatData;

namespace StatApp.ModelView
{
    public class Anacompo : Observable
    {
        #region Instance Variables
        private int[] m_indexes;
        private double[,] m_initialdata;
        private double[] m_means;
        private double[] m_stds;
        private double[,] m_corr;
        private double[] m_vals;
        private double[,] m_vectors;
        private double[,] m_vars;
        private double[,] m_inds;
        private double m_vmin;
        private double m_vmax;
        private VariableDescs m_variablesdescs;
        #endregion
        #region Properties
        public int[] Indexes
        {
            get
            {
                return m_indexes;
            }
            set {
                m_indexes = value;
            }
        }// Indexes
        public VariableDescs Variables
        {
            get
            {
                return m_variablesdescs;
            }
            set { }
        }// Variables
        public bool IsValid
        {
            get
            {
                return (m_inds != null) && (m_vars != null) && (m_inds.GetLength(0) > 1) &&
                    (m_vars.GetLength(0) > 0) && (m_inds.GetLength(1) == m_vars.GetLength(0)) &&
                    (m_vars.GetLength(0) == m_vars.GetLength(1));
            }
            set { }
        }// isValid
        public int Cols
        {
            get
            {
                return (m_vars != null) ? m_vars.GetLength(0) : 0;
            }
            set { }
        }// Cols
        public int Rows
        {
            get
            {
                return (m_inds != null) ? m_inds.GetLength(0) : 0;
            }
            set { }
        }// Rows
        public double[,] RecodedVars
        {
            get
            {
                return m_vars;
            }
            set { }
        }// RecodedVars
        public double[,] RecodedInds
        {
            get
            {
                return m_inds;
            }
            set { }
        }// RecodedInds
        public double[,] InitialData
        {
            get
            {
                return m_initialdata;
            }
            set
            {
            }
        }// InitialData
        public double[] Means
        {
            get
            {
                return m_means;
            }
            set { }
        }// Means
        public double[] StandardDeviations
        {
            get
            {
                return m_stds;
            }
            set { }
        }// StandartDeviations
        public double[,] Correlations
        {
            get
            {
                return m_corr;
            }
            set { }
        }// Correlations
        public double[] EigenValues
        {
            get
            {
                return m_vals;
            }
            set { }
        }// EigenValues
        public double[,] EigenVectors
        {
            get
            {
                return m_vectors;
            }
            set { }
        }// EigenVectors
        #endregion // Properties
        //
        public void BroadcastChanges()
        {
            this.NotifyChanges(new String[] { "Variables","IsValid",
            "Cols","Rows","RecodedVars","RecodedInds","InitialData","Means","StandardDeviation",
            "Correlations","EigenValues","EigenVectors"});
        }// BroadcastChanges
        private void myInit()
        {
            m_means = null;
            m_stds = null;
            m_corr = null;
            m_vals = null;
            m_vectors = null;
            m_vars = null;
            m_inds = null;
            m_vmin = 0;
            m_vmax = 0;
        }// myInit
        public Tuple<bool, Exception> ComputeEigen(Dictionary<VariableDesc, ValueDescs> dataDict)
        {
            bool bRet = false;
            Exception err = null;
            try
            {
                List<VariableDesc> oList = dataDict.Keys.ToList();
                oList.Sort();
                int nv = oList.Count;
                if (nv < 2)
                {
                    return new Tuple<bool, Exception>(bRet, new ArgumentException());
                }
                int nr = 0;
                foreach (var v in dataDict.Keys)
                {
                    var dd = dataDict[v];
                    if (dd == null)
                    {
                        return new Tuple<bool, Exception>(bRet, new ArgumentException());
                    }
                    if (nr < 1)
                    {
                        nr = dd.Count;
                    }
                    if (nr > dd.Count)
                    {
                        nr = dd.Count;
                    }
                } // v
                if (nr <= nv)
                {
                    return new Tuple<bool, Exception>(false, new ArgumentException());
                }
                m_indexes = new int[nr];
                double[,] data = new double[nr, nv];
                for (int ivar = 0; ivar < nv; ++ivar)
                {
                    var v = oList[ivar];
                    var dd = (dataDict[v]).ToArray();
                    for (int irow = 0; irow < nr; ++irow)
                    {
                        var vv = dd[irow];
                        m_indexes[irow] = vv.Index;
                        data[irow, ivar] = vv.DoubleValue;
                    }// irow
                }// ivar
                if (!ComputeEigen(data, out err))
                {
                    m_initialdata = null;
                    myInit();
                    return new Tuple<bool, Exception>(false, err);
                }
                this.m_variablesdescs = new VariableDescs(oList);
                bRet = true;
            }// try
            catch (Exception ex)
            {
                err = ex;
            }
            return new Tuple<bool, Exception>(bRet, err);
        }// ComputeEigen
        public Tuple<bool, Exception> ComputeEigen(Dictionary<VariableDesc, double[]> dataDict)
        {
            bool bRet = false;
            Exception err = null;
            try
            {
                List<VariableDesc> oList = dataDict.Keys.ToList();
                oList.Sort();
                int nv = oList.Count;
                if (nv < 2)
                {
                    return new Tuple<bool, Exception>(bRet, new ArgumentException());
                }
                int nr = 0;
                foreach (var v in dataDict.Keys)
                {
                    var dd = dataDict[v];
                    if (dd == null)
                    {
                        return new Tuple<bool, Exception>(bRet, new ArgumentException());
                    }
                    if (nr < 1)
                    {
                        nr = dd.Length;
                    }
                    if (nr > dd.Length)
                    {
                        nr = dd.Length;
                    }
                } // v
                if (nr <= nv)
                {
                    return new Tuple<bool, Exception>(false, new ArgumentException());
                }
                double[,] data = new double[nr, nv];
                for (int ivar = 0; ivar < nv; ++ivar)
                {
                    var v = oList[ivar];
                    double[] dd = dataDict[v];
                    for (int irow = 0; irow < nr; ++irow)
                    {
                        data[irow, ivar] = dd[irow];
                    }// irow
                }// ivar
                if (!ComputeEigen(data, out err))
                {
                    m_initialdata = null;
                    myInit();
                    return new Tuple<bool, Exception>(false, err);
                }
                this.m_variablesdescs = new VariableDescs(oList);
                bRet = true;
            }// try
            catch (Exception ex)
            {
                err = ex;
            }
            return new Tuple<bool, Exception>(bRet, err);
        }// ComputeEigen
        public bool ComputeEigen(double[,] data, out Exception err)
        {
            err = null;
            bool bRet = false;
            myInit();
            try
            {
                int nRows = data.GetLength(0);
                int nVars = data.GetLength(1);
                m_initialdata = data;
                m_means = new double[nVars];
                m_stds = new double[nVars];
                m_corr = new double[nVars, nVars];
                List<double[]> oData = new List<double[]>();
                for (int ivar = 0; ivar < nVars; ++ivar)
                {
                    double[] x1 = new double[nRows];
                    for (int irow = 0; irow < nRows; ++irow)
                    {
                        x1[irow] = m_initialdata[irow, ivar];
                    }// irow
                    oData.Add(x1);
                    DescriptiveStatistics stst = new DescriptiveStatistics(x1);
                    if (stst.Maximum <= stst.Minimum)
                    {
                        return false;
                    }
                    m_means[ivar] = stst.Mean;
                    m_stds[ivar] = stst.StandardDeviation;
                    m_corr[ivar, ivar] = 1.0;
                    for (int ivar1 = 0; ivar1 < ivar; ++ivar1)
                    {
                        double[] x2 = oData.ElementAt(ivar1);
                        double c = Correlation.Pearson(x1, x2);
                        m_corr[ivar, ivar1] = c;
                        m_corr[ivar1, ivar] = c;
                    }// ivar1
                }// ivar
                m_vals = new double[nVars];
                m_vectors = new double[nVars, nVars];
                m_vars = new double[nVars, nVars];
                m_inds = new double[nRows, nVars];
                var matrix = DenseMatrix.OfArray(m_corr);
                var evd = matrix.Evd();
                double[,] dd = evd.D().ToArray();
                double[,] de = evd.EigenVectors().ToArray();
                for (int i = 0; i < nVars; ++i)
                {
                    int k = nVars - i - 1;
                    m_vals[k] = dd[i, i];
                    for (int j = 0; j < nVars; ++j)
                    {
                        m_vectors[j, k] = de[j, i];
                    }
                }// i
                // Recode
                double act2 = Math.Sqrt((double)nVars);
                bool bFirst = true;
                m_vmax = 0;
                m_vmin = 0;
                for (int iFact = 0; iFact < nVars; ++iFact)
                {
                    double v = m_vals[iFact];
                    v = (v > 0.0)? Math.Sqrt(v) : Math.Sqrt(-v);
                    for (int i = 0; i < nVars; ++i)
                    {
                        double f = v * m_vectors[i, iFact];
                        f = ((int)(10000.0 * f + 0.5)) / 10000.0;
                        if (bFirst)
                        {
                            m_vmin = f;
                            m_vmax = f;
                            bFirst = false;
                        }
                        if (f < m_vmin)
                        {
                            m_vmin = f;
                        }
                        if (f > m_vmax)
                        {
                            m_vmax = f;
                        }
                        m_vars[i, iFact] = f;
                    }// i
                    for (int irow = 0; irow < nRows; ++irow)
                    {
                        double s = 0;
                        for (int ivar = 0; ivar < nVars; ++ivar)
                        {
                            double vn = (m_initialdata[irow, ivar] - m_means[ivar]) / m_stds[ivar];
                            s += m_vectors[ivar, iFact] * vn;
                        }// ivar
                        double f = s / act2;
                        f = ((int)(10000.0 * f + 0.5)) / 10000.0;
                        if (bFirst)
                        {
                            m_vmin = f;
                            m_vmax = f;
                            bFirst = false;
                        }
                        if (f < m_vmin)
                        {
                            m_vmin = f;
                        }
                        if (f > m_vmax)
                        {
                            m_vmax = f;
                        }
                        m_inds[irow, iFact] = f;
                    }// irow
                }// iFact
                bRet = true;
            }
            catch (Exception ex)
            {
                err = ex;
            }
            return bRet;
        }// ComputeEigen
        //
    }//Anacompo
}
