using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinImplantCS48
{
    public class clsProxySocks5
    {
        private clsVictim m_victim { get; set; }
        private int m_nStreamId { get; set; }
        private string m_szIPv4 { get; set; }
        private int m_nPort { get; set; }

        public clsProxySocks5(clsVictim victim, int nStreamId, string szIPv4, int nPort)
        {
            m_victim = victim;
            m_nStreamId = nStreamId;
            m_szIPv4 = szIPv4;
            m_nPort = nPort;
        }


    }
}
