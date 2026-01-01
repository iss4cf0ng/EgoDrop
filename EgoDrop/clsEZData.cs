using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoDrop
{
    internal class clsEZData
    {
        public static string fnGenerateRandomStr(int nLength = 10)
        {
            const string szPattern = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            StringBuilder sb = new StringBuilder();
            Random rand = new Random();
            for (int i = 0; i < nLength; i++)
                sb.Append(szPattern[rand.Next(0, szPattern.Length)]);

            return sb.ToString();
        }

        public static string fnStrE2B64(string szInput) => Convert.ToBase64String(Encoding.UTF8.GetBytes(szInput));
        public static string fnB64D2Str(string szInput) => Encoding.UTF8.GetString(Convert.FromBase64String(szInput));
        public static List<string> fnLsE2B64(List<string> lsInput) => lsInput.Select(x => fnStrE2B64(x)).ToList();
        public static List<string> fnLB64D2S(List<string> lsInput) => lsInput.Select(x => fnB64D2Str(x)).ToList();
        public static List<string> fnlsB64D2Str(string szInput, string szSplitter = ",") => fnLB64D2S(szInput.Split(szSplitter).ToList());

        public static List<List<string>> fn2dLB64Decode(string szInput, string szSplitter = ",")
        {
            List<string> ls = szInput.Split(szSplitter).Select(x => fnB64D2Str(x)).ToList();
            List<List<string>> lsResult = new List<List<string>>();
            foreach (string s in ls)
            {
                List<string> l = s.Split(',').Select(x => fnB64D2Str(x)).ToList();
                lsResult.Add(l);
            }

            return lsResult;
        }

        public static string fnszDateString()
        {
            DateTime date = DateTime.Now;
            return string.Join(string.Empty, new int[]
            {
                date.Year,
                date.Month,
                date.Day,
                date.Hour,
                date.Minute,
                date.Second,
                date.Millisecond,
            });
        }
        public static string fnszDateFileName(string szExt = "txt") => $"{fnszDateString()}{(string.Equals(string.Empty, szExt) ? string.Empty : "." + szExt)}";
    }
}
