using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinImplantCS48
{
    public class clsEZData
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
        public static string fnszLsE2B64(List<string> lsInput, string szSplitter = ",") => string.Join(szSplitter, fnLsE2B64(lsInput));

        public static List<List<string>> fn2dLB64Decode(string szInput, string szSplitter = ",")
        {
            List<string> rows = szInput
                .Split(new[] { szSplitter }, StringSplitOptions.None)
                .Select(x => fnB64D2Str(x))
                .ToList();

            List<List<string>> lsResult = new List<List<string>>();

            foreach (string row in rows)
            {
                List<string> cols = row
                    .Split(new[] { szSplitter }, StringSplitOptions.None)
                    .Select(x => fnB64D2Str(x))
                    .ToList();

                lsResult.Add(cols);
            }

            return lsResult;
        }

        public static string fnszSendParser(List<string> lsInput, string szSplitter = ",")
        {
            return string.Join(szSplitter, fnLsE2B64(lsInput));
        }

        public static string fnszSend2dParser(List<List<string>> lsInput, string szSplitter = ",")
        {
            List<string> encodedRows = new List<string>();

            foreach (var row in lsInput)
            {
                string s = fnszSendParser(row, szSplitter);
                encodedRows.Add(s);
            }

            return fnszSendParser(encodedRows, szSplitter);
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

        public static byte[] fnCombineBytes(byte[] first_bytes, int first_idx, int first_len, byte[] second_bytes, int second_idx, int second_len)
        {
            byte[] bytes = null;
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(first_bytes, first_idx, first_len);
                ms.Write(second_bytes, second_idx, second_len);
                bytes = ms.ToArray();
            }

            return bytes;
        }
    }
}
