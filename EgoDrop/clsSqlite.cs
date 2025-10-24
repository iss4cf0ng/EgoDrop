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
    internal class clsSqlite
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

        private struct stListener
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
        private enum enListenerProtocol
        {
            RAW_TCP,
            TLS_TCP,
            ENCRYPTED_TCP,

            RAW_UDP,
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
    }
}
