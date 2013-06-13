using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace StatData
{
    [Serializable]
    public class CorrelData
    {
        public CorrelData()
        {
        }
        public String FirstName { get; set; }
        public String SecondName { get; set; }
        public int Count { get; set; }
        public double Value { get; set; }
        public double Probability { get; set; }
        public double Minimum { get; set; }
        public double Maximum { get; set; }
    }// class CorrelData
    [Serializable]
    public class CorrelDatas : ObservableCollection<CorrelData>
    {
        public CorrelDatas()
            : base()
        {
        }
        public CorrelDatas(IEnumerable<CorrelData> col)
            : base(col)
        {
        }
    }
}
