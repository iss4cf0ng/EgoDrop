using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
