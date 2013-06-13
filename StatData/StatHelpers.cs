using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatData
{
    public static class StatHelpers
    {
        private static readonly String[] MISSING_VALS = new String[] {
            "n/a","na","nan","empty","?","vide","missing","manquant","nothing","err"
        };
        public static String ConvertValue(String s)
        {
            if (String.IsNullOrWhiteSpace(s))
            {
                return null;
            }
            String ss = s.Trim().ToLower();
            if (String.IsNullOrEmpty(ss))
            {
                return null;
            }
            if (ss.Contains("?"))
            {
                return null;
            }
            foreach (String p in MISSING_VALS)
            {
                if (p == ss)
                {
                    return null;
                }
            }
            return ss;
        }// convertValue
    }// class StatHelpers
}
