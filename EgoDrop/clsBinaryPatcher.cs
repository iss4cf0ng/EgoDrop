using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoDrop
{
    class clsBinaryPatcher
    {
        private string m_szSrcFilePath { get; init; }
        private string m_szDstFilePath { get; init; }

        public clsBinaryPatcher(string szSrcFilePath, string szDstFilePath)
        {
            if (!File.Exists(szSrcFilePath))
                throw new Exception("File not found: " + szSrcFilePath);

            m_szSrcFilePath = szSrcFilePath;
            m_szDstFilePath = szDstFilePath;
        }

        public bool fnbDoReplacement(string szOriginalPattern, string szReplacePattern)
        {
            byte[] abData = File.ReadAllBytes(m_szSrcFilePath);
            int nCount = fnPatchString(ref abData, szOriginalPattern, szReplacePattern);
            if (nCount == 0)
                throw new Exception("Replacement is failed.");

            File.WriteAllBytes(m_szDstFilePath, abData);

            return true;
        }

        private bool fnbIsMatch(byte[] abData, int nOffset, byte[] abPattern)
        {
            for (int i = 0; i < abPattern.Length; i++)
                if (abData[nOffset + i] != abPattern[i])
                    return false;

            return true;
        }

        private int fnPatchString(ref byte[] abData, string szSearchStr, string szReplaceStr, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            byte[] abPattern = encoding.GetBytes(szSearchStr);
            byte[] abReplace = encoding.GetBytes(szReplaceStr);

            if (abReplace.Length > abPattern.Length)
                throw new Exception("The size of replacement string cannot larger than the size of original string.");

            int nCount = 0;
            int i = 0;

            while (i <= abData.Length - abPattern.Length)
            {
                if (fnbIsMatch(abData, i, abPattern))
                {
                    Array.Copy(abReplace, 0, abData, i, abReplace.Length);

                    // null byte
                    for (int j = abReplace.Length; j < abPattern.Length; j++)
                        abData[i + j] = 0x00;

                    i += abPattern.Length;
                    nCount++;
                }
                else
                {
                    i++;
                }
            }

            return nCount;
        }
    }
}
