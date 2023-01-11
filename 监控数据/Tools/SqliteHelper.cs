using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;
using System.Collections;
using System.Data.SQLite;
using System.Data.Common;
using System.Collections.Generic;
using System.Data.SqlServerCe;

namespace 监控数据.Tools
{

    public class SQLiteHelper
    {
        private string strDBNew = Directory.GetCurrentDirectory() + @"\set.db";
        private string strConn;
        public SQLiteHelper()
        {
            strConn = "Data Source=" + strDBNew + ";Max Pool Size=10";
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="sqlText"></param>
        /// <returns></returns>
        public DataTable Query(string sqlText)
        {
            SQLiteConnection cc = new SQLiteConnection(strConn);
            cc.Open();
            DataTable dt = new DataTable();
            SQLiteDataAdapter da = new SQLiteDataAdapter(sqlText, cc);
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
            SQLiteConnection cc = new SQLiteConnection(strConn);
            cc.Open();
            SQLiteCommand cmd = new SQLiteCommand(sqlText, cc);
            int iResult = cmd.ExecuteNonQuery();
            cc.Close();
            return iResult;
        }
        public int ExecNonQuery(List<string> sqlText)
        {
            SQLiteConnection cc = new SQLiteConnection(strConn);
            cc.Open();
            int iResult = 0;
            for (int i = 0; i < sqlText.Count; i++)
            {
                SQLiteCommand cmd = new SQLiteCommand(sqlText[i], cc);
                iResult += cmd.ExecuteNonQuery();
            }
            cc.Close();
            return iResult;
        }
    }
}
