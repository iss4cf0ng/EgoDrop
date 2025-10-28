using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoDrop
{
    internal class clsHttpListener : clsListener
    {
        public clsHttpListener(string szName, int nPort, string szDescription)
        {
            m_szName = szName;
            m_nPort = nPort;
            m_szDescription = szDescription;
            m_Protocol = clsSqlite.enListenerProtocol.HTTP;


        }
    }
}
