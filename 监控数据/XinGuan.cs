using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace 监控数据
{
    public class XinGuan
    {
        //#region 有信息无标本数据明细
        //public void GetXinguang_yxxwbb(string khdh, int fw)
        //{
        //    string sql_yxxwbb;
        //    if (string.IsNullOrEmpty(khdh))
        //    {
        //        sql_yxxwbb = @"
        //        SELECT Barcode,PatientName,CreateDate FROM LisSampleDB.dbo.SampleInfo
        //        WHERE LEFT(Barcode,6) <> '999999'
        //        AND DATEDIFF(HOUR,CreateDate,GETDATE())= @fw
        //        AND NOT EXISTS (
	       //         SELECT ybid FROM lisdb.dbo.lis_ybxx WHERE yqdh = 'pcr' AND cyrq >= GETDATE() -3 
	       //         AND LEFT(ybid,6) <> '999999'
	       //         AND Barcode = ybid
        //        )
        //        ORDER BY CreateDate
        //        ";
        //    }
        //    else
        //    {
        //        sql_yxxwbb = @"
        //        SELECT Barcode,PatientName,CreateDate FROM LisSampleDB.dbo.SampleInfo
        //        WHERE LEFT(Barcode,6) IN @khdh
        //        AND DATEDIFF(HOUR,CreateDate,GETDATE())= @fw
        //        AND NOT EXISTS (
	       //         SELECT ybid FROM lisdb.dbo.lis_ybxx WHERE yqdh = 'pcr' AND cyrq >= GETDATE() -3 
	       //         AND LEFT(ybid,6) IN @khdh
	       //         AND Barcode = ybid
        //        )
        //        ORDER BY CreateDate
        //        ";
        //    }
        //}
        //#endregion



    }

}
