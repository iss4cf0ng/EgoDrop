using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace EgoDrop
{
    internal class clsIniMgr
    {
        //Win32 API
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern long WritePrivateProfileString(string szSection, string szKey, string szValue, string szFilePath);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern long GetPrivateProfileString(string szSection, string szKey, string szDefaultValue, StringBuilder sbRetVal, int nSize, string szFilePath);

        private string m_szFilePath { get; set; }

        public clsIniMgr(string szFilePath)
        {
            m_szFilePath = new FileInfo(szFilePath).FullName;
        }

        /// <summary>
        /// Read INI file.
        /// </summary>
        /// <param name="szSection"></param>
        /// <param name="szKey"></param>
        /// <returns></returns>
        public string fnRead(string szSection, string szKey)
        {
            var retVal = new StringBuilder(255);
            GetPrivateProfileString(szSection, szKey, string.Empty, retVal, 255, m_szFilePath);
            
            return retVal.ToString();
        }

        /// <summary>
        /// Write INI file.
        /// </summary>
        /// <param name="szSection"></param>
        /// <param name="szKey"></param>
        /// <param name="szValue"></param>
        public void fnWrite(string szSection, string szKey, string szValue)
        {
            WritePrivateProfileString(szSection, szKey, szValue, m_szFilePath);
        }
    }
}
