using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;
using System.Data.Entity.Core.Metadata.Edm;

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
                "Logs", new string[]
                {
                    "Type",
                    "OnlineID",
                    "Func",
                    "Message",
                    "CreationDate",
                }
            },
            {
                "Group", new string[]
                {
                    "Name",
                    "CreationDate",
                }
            },
            {
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
                "Listener", new string[]
                {
                    "Name",
                    "Protocol",
                    "Port",
                    "Description",
                    "CreationDate",
                }
            },
        };

        private string m_szFileName { get; set; }
        private string m_szConnString { get { return $"Data Source={m_szFileName};Compress=True;"; } }
        private SQLiteConnection m_sqlConn { get; set; }

        public struct stListener
        {
            public string szName { get; set; }
            public enListenerProtocol protoListener { get; set; }
            public int nPort { get; set; }
            public string szDescription { get; set; }
            public DateTime dtCreationDate { get; set; }

            public stListener(
                string szName,
                enListenerProtocol proto,
                int nPort,
                string szDescription,
                DateTime dtDate
            )
            {
                this.szName = szName;
                protoListener = proto;
                this.nPort = nPort;
                this.szDescription = szDescription;
                dtCreationDate = dtDate;
            }

        };
        public enum enListenerProtocol
        {
            TCP,
            TLS,

            DNS,

            HTTP,
        };

        public clsSqlite(string szFileName)
        {
            m_szFileName = szFileName;
            m_sqlConn = new SQLiteConnection(m_szConnString);

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
            szQuery = $"CREATE TABLE {szTableName} ({szQuery});";
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

        private bool fnbListenerExist(string szName)
        {
            try
            {
                string szQuery = $"SELECT EXISTS(SELECT 1 FROM \"Listener\" WHERE \"Name\" = \"{szName}\");";
                DataTable dt = fnQuery(szQuery);

                return (Int64)dt.Rows[0][0] == (Int64)1;
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

        private bool fnbListenerIsEqual(stListener lA, stListener lB)
        {
            return (
                lA.szName == lB.szName
                && lA.protoListener == lB.protoListener
                && lA.nPort == lB.nPort
                && lA.szDescription == lB.szDescription
            );
        }

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
                    string szName = (string)dr["Name"];
                    int nPort = int.Parse((string)dr["Port"]);
                    string szDescription = (string)dr["Description"];

                    enListenerProtocol proto = (enListenerProtocol)Enum.Parse(typeof(enListenerProtocol), (string)dr["Protocol"]);
                    DateTime date = DateTime.Parse((string)dr["CreationDate"]);

                    stListener st = new stListener(
                        szName,
                        proto,
                        nPort,
                        szDescription,
                        date
                    );

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
                        $"\"Name\"=\"{listener.szName}\"," +
                        $"\"Protocol\"=\"{Enum.GetName(listener.protoListener)}\"," +
                        $"\"Port\"=\"{listener.nPort}\"," +
                        $"\"Description\"=\"{listener.szDescription}\"," +
                        $"\"CreationDate\"=\"{listener.dtCreationDate.ToString("F")}\"" +
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
                        $"\"{listener.dtCreationDate.ToString("F")}\"" +
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
    }
}
