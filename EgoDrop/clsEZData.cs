using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoDrop
{
    internal class clsEZData
    {
        public static string fnGenerateRandomStr(int nLength)
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
    }
}
