using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoDrop
{
    public class clsListener
    {
        protected string m_szName;
        protected int m_nPort;
        protected string m_szDescription;
        protected ListenerType m_listenerType;

        public clsListener()
        {

        }

        public virtual void fnStart()
        {

        }

        public virtual void fnStop()
        {

        }
    }
}
