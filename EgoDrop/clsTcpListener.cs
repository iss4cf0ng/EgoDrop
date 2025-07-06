using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoDrop
{
    internal class clsTcpListener : clsListener
    {
        public clsTcpListener(string szName, int nPort, string szDescription)
        {
            m_szName = szName;
            m_nPort = nPort;
            m_szDescription = szDescription;
        }

        public override void fnStart()
        {
            
        }

        public override void fnStop()
        {

        }
    }
}
