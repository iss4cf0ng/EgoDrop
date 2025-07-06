using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;

namespace EgoDrop
{
    internal class clsSqlite
    {
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
            fnQuery(szQuery);
        }

        private DataTable fnQuery(string szQuery)
        {
            DataTable dt = new DataTable();
            try
            {
                using (var adapter = new SQLiteDataAdapter(szQuery, m_sqlConn))
                {
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
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
