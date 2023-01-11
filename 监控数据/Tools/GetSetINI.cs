using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
//using System.Threading.Tasks;

namespace Common
{
    public class GetSetINI
    {
        #region 导入读写INI文件方法

        [DllImport("kernel32")]
        private static extern bool WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string defVal, StringBuilder retVal, int size, string filePath);
        //[DllImport("kernel32")] private static extern int GetPrivateProfileInt(string lpAppName, string lpKeyName, int nDefault, string lpFileName);

        //写INI文件
        /// <summary>
        /// 写INI文件 ----------> SetiniProfile(String section/*节*/, String key/*键*/, String val/*值*/, String filepath/*INI文件*/)-------------------->  
        /// 节:要写入的段落名 键:要写入的键，如果该key存在则覆盖写入 值:key所对应的值 INI文件:INI文件的完整路径和文件名
        /// </summary>
        /// <param name="section">要写入的段落名</param>
        /// <param name="key">要写入的键，如果该key存在则覆盖写入</param>
        /// <param name="val">key所对应的值</param>
        /// <param name="filepath">INI文件的完整路径和文件名</param>
        /// <returns></returns>
        public static bool SetiniProfile(String section/*节*/, String key/*键*/, String val/*值*/, String filepath/*INI文件*/)
        {
            //StringBuilder tem = new StringBuilder("server=127.0.0.1;database=whsj;uid=tlg;pwd=123");
            return WritePrivateProfileString(section, key, val, filepath);
            //return tem.ToString();
        }

        // 读INI文件
        /// <summary>
        /// 读INI文件 ---> GetiniProfile(String section/*节*/, String key/*键*/, String defval/*缺省值*/, int size/*值大小*/, String filepath/*INI文件*/)
        /// 节:要读取的段落名  键:要读取的键 缺省值:读取异常的情况下的缺省值  值大小:值允许的大小 INI文件:INI文件的完整路径和文件名
        /// </summary>
        /// <param name="section">要读取的段落名</param>
        /// <param name="key">要读取的键</param>
        /// <param name="defval">读取异常的情况下的缺省值</param>
        /// <param name="size">值允许的大小</param>
        /// <param name="filepath">INI文件的完整路径和文件名</param>
        /// <returns></returns>
        public static String GetiniProfile(String section, String key, String defval, int size, String filepath)
        {
            StringBuilder retvaltemp = new StringBuilder(size);//key所对应的值，如果该key不存在则返回空值
            GetPrivateProfileString(section, key, defval, retvaltemp, size, filepath);
            return retvaltemp.ToString();
        }
        #endregion
    }
}
