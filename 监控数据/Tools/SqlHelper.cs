using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using 监控数据;
//using System.Threading.Tasks;

namespace Common
{
    public static class SqlHelper
    {
        //public string connStr = null;
        //private static string connStr = ConfigurationManager.ConnectionStrings["his"].ConnectionString;
        public static string serverip = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("LIS库连接设置", "服务器名称", "10.5.0.1", 128, ".\\set.ini"), "fzadicon");
        public static string servername = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("LIS库连接设置", "数据库名称", "lisdb", 128, ".\\set.ini"), "fzadicon");
        public static string login = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("LIS库连接设置", "登录名", "lisuser", 128, ".\\set.ini"), "fzadicon");
        public static string password = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("LIS库连接设置", "密码", "lisuser", 128, ".\\set.ini"), "fzadicon");
        public static string connStr = String.Format("Data Source={0};Initial Catalog={1};Persist Security Info=True;User ID={2};Password={3}", serverip, servername, login, password);

        public static string serveripLISDB = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("LISDB库连接设置", "服务器名称", "10.5.0.1", 128, ".\\set.ini"), "fzadicon");
        public static string servernameLISDB = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("LISDB库连接设置", "数据库名称", "lisdb", 128, ".\\set.ini"), "fzadicon");
        public static string loginLISDB = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("LISDB库连接设置", "登录名", "lisuser", 128, ".\\set.ini"), "fzadicon");
        public static string passwordLISDB = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("LISDB库连接设置", "密码", "lisuser", 128, ".\\set.ini"), "fzadicon");
        public static string connStrLISDB = String.Format("Data Source={0};Initial Catalog={1};Persist Security Info=True;User ID={2};Password={3}", serveripLISDB, servernameLISDB, loginLISDB, passwordLISDB);

        public static DataTable GetList(string sql, params SqlParameter[] ps)
        {

                //构造连接对象
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    //构造桥接器对象
                    SqlDataAdapter adapter = new SqlDataAdapter(sql, conn);
                    //添加参数
                    adapter.SelectCommand.Parameters.AddRange(ps);
                    //数据表对象
                    DataTable table = new DataTable();
                    //将数据存到Tabel
                    adapter.Fill(table);
                    //返回数据表
                    return table;
                }
        }

        public static int ExecuteNonQuery(string sql, params SqlParameter[] ps)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddRange(ps);
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public static int Add(string sql, params SqlParameter[] ps)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                SqlCommand cmd=new SqlCommand(sql,conn);
                cmd.Parameters.AddRange(ps);
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public static object FromDbValue(object value)
        {
            if (value == DBNull.Value)
            {
                return null;
            }
            else
            {
                return value;
            }
        }

        public static object ToDbValue(object value)
        {
            if (value == null)
            {
                return DBNull.Value;
            }
            else
            {
                return value;
            }
        }

        #region 编写带参数的SQL语句相关方法（能够很好的解决注入式攻击问题）

        /// <summary>
        /// 执行带参数的SQL的更新方法（增删改操作）
        /// </summary>
        /// <param name="sql">带参数的SQL语句</param>
        /// <param name="param">参数的数组</param>
        /// <returns>返回受影响的行数</returns>
        public static int Update(string sql,SqlParameter[] param)
        {
            SqlConnection conn = new SqlConnection(connStr);
            SqlCommand cmd = new SqlCommand(sql, conn);
            try
            {
                conn.Open();
                cmd.Parameters.AddRange(param);//给command对象添加参数
                return cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                //将错误信息写入系统日志
                string info = "调用public static int Update(string sql,SqlParameter[] param)方法时发生错误：" + ex.Message;
                WriteLog(ex.Message);
                throw new Exception(info);
            }
            finally
            {
                conn.Close();
            }
        }

        /// <summary>
        /// 使用带参数的SQL语句返回单一结果
        /// </summary>
        /// <param name="sql">带参数的SQL语句</param>
        /// <param name="param">参数的数组</param>
        /// <returns>返回受影响的行数</returns>
        public static object GetSingleResult(string sql,SqlParameter[] param)
        {
            SqlConnection conn = new SqlConnection(connStr);
            SqlCommand cmd = new SqlCommand(sql, conn);
            try
            {
                conn.Open();
                cmd.Parameters.AddRange(param);//给command对象添加参数
                return cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                //将错误信息写入系统日志
                string info = "调用public static object GetSingleResult(string sql, SqlParameter[] param)方法时发生错误：" + ex.Message;
                WriteLog(ex.Message);
                throw new Exception(info);
            }
            finally
            {
                conn.Close();
            }
        }

        /// <summary>
        /// 使用带参数的SQL语句返回一个结果集
        /// </summary>
        /// <param name="sql">带参数的SQL语句</param>
        /// <param name="param">参数的数组</param>
        /// <returns>返回受影响的行数</returns>
        public static SqlDataReader GetReader(string sql,SqlParameter[] param)
        {     
            SqlConnection conn = new SqlConnection(connStr);
            SqlCommand cmd = new SqlCommand(sql, conn);
            try
            {
                conn.Open();
                cmd.Parameters.AddRange(param);//给command对象添加参数
                return cmd.ExecuteReader(CommandBehavior.CloseConnection);                
            }
            catch (Exception ex)
            {
                //将错误信息写入系统日志
                string info = "[LIS]调用public static SqlDataReader GetReader(string sql,SqlParameter[] param)方法时发生错误：" + ex.Message;
                WriteLog(ex.Message);
                throw new Exception(info);
            }
        }
        public static SqlDataReader GetReaderLISDB(string sql, SqlParameter[] param)
        {
            SqlConnection conn = new SqlConnection(connStrLISDB);
            SqlCommand cmd = new SqlCommand(sql, conn);
            try
            {
                conn.Open();
                cmd.Parameters.AddRange(param);//给command对象添加参数
                return cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception ex)
            {
                //将错误信息写入系统日志
                string info = "[LISDB]调用public static SqlDataReader GetReader(string sql,SqlParameter[] param)方法时发生错误：" + ex.Message;
                WriteLog(ex.Message);
                throw new Exception(info);
            }
        }

        #endregion

        #region 编写带参数的存储过程相关方法


        public static int UpdateByProcedure(string sqName, SqlParameter[] param)
        {
            SqlConnection conn = new SqlConnection(connStr);
            SqlCommand cmd = new SqlCommand(sqName, conn);
            cmd.CommandType = CommandType.StoredProcedure;//指明当前的操作是使用存储过程
            try
            {
                conn.Open();
                cmd.Parameters.AddRange(param);//给command对象添加参数
                return cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                //将错误信息写入系统日志
                string info = "调用public static int UpdateByProcedure(string sqName, SqlParameter[] param)方法时发生错误：" + ex.Message;
                WriteLog(ex.Message);
                throw new Exception(info);
            }
            finally
            {
                conn.Close();
            }
        }


        public static object GetSingleResultByProcedure(string sqName, SqlParameter[] param)
        {
            SqlConnection conn = new SqlConnection(connStr);
            SqlCommand cmd = new SqlCommand(sqName, conn);
            cmd.CommandType = CommandType.StoredProcedure;//指明当前的操作是使用存储过程
            try
            {
                conn.Open();
                cmd.Parameters.AddRange(param);//给command对象添加参数
                return cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                //将错误信息写入系统日志
                string info = "调用public static object GetSingleResultByProcedure(string sqName, SqlParameter[] param)方法时发生错误：" + ex.Message;
                WriteLog(ex.Message);
                throw new Exception(info);
            }
            finally
            {
                conn.Close();
            }
        }


        public static SqlDataReader GetReaderByProcedure(string sqName, SqlParameter[] param)
        {
            SqlConnection conn = new SqlConnection(connStr);
            SqlCommand cmd = new SqlCommand(sqName,conn);
            cmd.CommandType = CommandType.StoredProcedure;//指明当前的操作是使用存储过程
            try
            {
                conn.Open();
                cmd.Parameters.AddRange(param);//给command对象添加参数
                return cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception ex)
            {
                //将错误信息写入系统日志
                string info = "调用public static SqlDataReader GetReaderByProcedure(string sqName, SqlParameter[] param)方法时发生错误：" + ex.Message;
                WriteLog(ex.Message);
                throw new Exception(info);
            }
        }

        #endregion

        #region 其他方法

        /// <summary>
        /// 将错误信息写入日志
        /// </summary>
        /// <param name="log"></param>
        public static void WriteLog(string log)
        {
            FileStream fs=new FileStream("sqlhelper.log",FileMode.Append);
            StreamWriter sw=new StreamWriter(fs);
            sw.WriteLine(DateTime.Now.ToString()+"错误信息："+log);
            sw.Close();
            fs.Close();
        }

        #endregion


    }
}
