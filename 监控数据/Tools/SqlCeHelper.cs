using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Text;

namespace 监控数据
{
    public class SqlCeHelper
    {
        private string strDBNew = Directory.GetCurrentDirectory() + @"\set.sdf";
        private string strConn;
        public SqlCeHelper()
        {
            strConn = "Data Source=" + strDBNew + ";Password='adicon'";
        }
        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="sqlText"></param>
        /// <returns></returns>
        public DataTable Query(string sqlText)
        {
            SqlCeConnection cc = new SqlCeConnection(strConn);
            cc.Open();
            DataTable dt = new DataTable();
            SqlCeDataAdapter da = new SqlCeDataAdapter(sqlText, cc);
            da.Fill(dt);
            cc.Close();
            return dt;
        }
        /// <summary>
        /// 执行非查询
        /// </summary>
        /// <param name="sqlText"></param>
        /// <returns></returns>
        public int ExecNonQuery(string sqlText)
        {
            SqlCeConnection cc = new SqlCeConnection(strConn);
            cc.Open();
            SqlCeCommand cmd = new SqlCeCommand(sqlText, cc);
            int iResult = cmd.ExecuteNonQuery();
            cc.Close();
            return iResult;
        }
        public int ExecNonQuery(List<string> sqlText)
        {
            SqlCeConnection cc = new SqlCeConnection(strConn);
            cc.Open();
            int iResult = 0;
            for (int i = 0; i < sqlText.Count; i++)
            {
                SqlCeCommand cmd = new SqlCeCommand(sqlText[i], cc);
                iResult += cmd.ExecuteNonQuery();
            }
            cc.Close();
            return iResult;
        }
    }
}
