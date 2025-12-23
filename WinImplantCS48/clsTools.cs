using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace WinImplantCS48
{
    internal class clsTools
    {
        public static DataTable fnWmiQuery(string szQuery)
        {
            DataTable dt = new DataTable();
            try
            {
                using (var searcher = new ManagementObjectSearcher(szQuery))
                {
                    using (var coll = searcher.Get())
                    {
                        bool bCollAdd = false;
                        foreach (var obj in coll)
                        {
                            if (!bCollAdd)
                            {
                                foreach (PropertyData prop in obj.Properties)
                                    dt.Columns.Add(prop.Name.ToString());

                                bCollAdd = true;
                            }

                            DataRow dr = dt.NewRow();
                            foreach (PropertyData prop in obj.Properties)
                                dr[prop.Name] = prop.Value ?? DBNull.Value;

                            dt.Rows.Add(dr);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DataRow dr = dt.NewRow();
                dt.Columns.Add("Error");
                dr["Error"] = ex.Message;

                dt.Rows.Add(dr);
            }

            return dt;
        }

        public static string fnszImageToString(Image img)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, ImageFormat.Png);
                byte[] abBuffer = ms.ToArray();

                return Convert.ToBase64String(abBuffer);
            }
        }

        public static Image fnStringToImage(string szB64)
        {
            byte[] abBuffer = Convert.FromBase64String(szB64);
            using (MemoryStream ms = new MemoryStream(abBuffer))
            {
                return Image.FromStream(ms);
            }
        }
    }
}
