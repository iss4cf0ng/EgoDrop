using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoDrop
{
    public class clsListener
    {
        protected string m_szName { get; set; }
        protected int m_nPort { get; set; }
        protected string m_szDescription { get; set; }
        protected clsSqlite.enListenerProtocol m_Protocol { get; set; }

        public clsSqlite.stListener m_stListener { get; set; }
        public bool m_bIsListening { get; set; }

        public clsListener()
        {
            m_bIsListening = false;
        }

        public virtual void fnStart()
        {

        }

        public virtual void fnStop()
        {

        }
    }
}
