using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data.SqlClient;
using System.Data;

namespace WebService
{
    /// <summary>
    /// Service 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class Service : System.Web.Services.WebService
    {

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }

        [WebMethod]
        public DataTable GetPBL1()
        {
            string ConnectionString = @"server=192.6.94.25\QYHY;database=Plane_1;uid=sa;pwd=xiaoming";
            SqlConnection conn = new SqlConnection(ConnectionString);
            SqlCommand cmd = conn.CreateCommand();

            //连接数据库
            conn.Open();

            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            string sql = "select * from PJ_PBL";
            SqlDataAdapter da = new SqlDataAdapter(sql, conn);
            da.Fill(ds, "PJ_PBL");
            dt = ds.Tables["PJ_PBL"];
            dt.Columns.Remove("RowId");
            dt.Columns["Part"].ColumnName = "程序名";
            dt.Columns["RJ_Start"].ColumnName = "起始行";
            dt.Columns["RJ_Stop"].ColumnName = "结束行";
            dt.Columns["RJ_Total"].ColumnName = "总行数";

            //关闭连接
            conn.Close();

            return dt;
        }

        [WebMethod]
        public DataTable GetPBL2()
        {
            string ConnectionString = @"server=192.6.94.25\QYHY;database=Plane_1;uid=sa;pwd=xiaoming";
            SqlConnection conn = new SqlConnection(ConnectionString);
            SqlCommand cmd = conn.CreateCommand();

            //连接数据库
            conn.Open();

            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            string sql = "select * from PW_PBL";
            SqlDataAdapter da = new SqlDataAdapter(sql, conn);
            da.Fill(ds, "PW_PBL");
            dt = ds.Tables["PW_PBL"];
            dt.Columns.Remove("RowId");
            dt.Columns["Part"].ColumnName = "程序名";
            dt.Columns["RJ_Start"].ColumnName = "J起始行";
            dt.Columns["RJ_Stop"].ColumnName = "J结束行";
            dt.Columns["RJ_Total"].ColumnName = "J总行数";
            dt.Columns["RW_Start"].ColumnName = "W起始行";
            dt.Columns["RW_Stop"].ColumnName = "W结束行";
            dt.Columns["RW_Total"].ColumnName = "W总行数";

            //关闭连接
            conn.Close();

            return dt;
        }

        [WebMethod]
        public bool SetPaintData1(int Parameter, string IOs, double LL, double PF, double WH)
        {
            string ConnectionString = @"server=192.6.94.25\QYHY;database=Plane_1;uid=sa;pwd=xiaoming";
            SqlConnection conn = new SqlConnection(ConnectionString);
            SqlCommand cmd = conn.CreateCommand();

            bool isOk = false;

            //连接数据库
            conn.Open();

            //执行插入
            cmd = conn.CreateCommand();
            cmd.CommandText = "update PSD_RJ set IOs='" + IOs + "',LL=" + LL.ToString() + ",PF=" +
                            PF.ToString() + ",WH=" + WH.ToString() + " where RowId=" + Parameter.ToString();
            if (0 != cmd.ExecuteNonQuery())
            {
                isOk = true;
            }

            //关闭连接
            conn.Close();

            return isOk;
        }

        [WebMethod]
        public bool SetPaintData2(int Parameter, string IOs, double LL, double PF, double WH)
        {
            string ConnectionString = @"server=192.6.94.25\QYHY;database=Plane_1;uid=sa;pwd=xiaoming";
            SqlConnection conn = new SqlConnection(ConnectionString);
            SqlCommand cmd = conn.CreateCommand();

            bool isOk = false;

            //连接数据库
            conn.Open();

            //执行插入
            cmd = conn.CreateCommand();
            cmd.CommandText = "update PSD_RW set IOs='" + IOs + "',LL=" + LL.ToString() + ",PF=" +
                            PF.ToString() + ",WH=" + WH.ToString() + " where RowId=" + Parameter.ToString();
            if (0 != cmd.ExecuteNonQuery())
            {
                isOk = true;
            }

            //关闭连接
            conn.Close();

            return isOk;
        }

        [WebMethod]
        public DataTable GetPaintData1(int Parameter)
        {
            string ConnectionString = @"server=192.6.94.25\QYHY;database=Plane_1;uid=sa;pwd=xiaoming";
            SqlConnection conn = new SqlConnection(ConnectionString);
            SqlCommand cmd = conn.CreateCommand();

            //连接数据库
            conn.Open();

            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            string sql = "select * from PSD_RJ where RowId=" + Parameter.ToString();
            SqlDataAdapter da = new SqlDataAdapter(sql, conn);
            da.Fill(ds, "PSD_RJ");
            dt = ds.Tables["PSD_RJ"];

            //关闭连接
            conn.Close();

            return dt;
        }

        [WebMethod]
        public DataTable GetPaintData2(int Parameter)
        {
            string ConnectionString = @"server=192.6.94.25\QYHY;database=Plane_1;uid=sa;pwd=xiaoming";
            SqlConnection conn = new SqlConnection(ConnectionString);
            SqlCommand cmd = conn.CreateCommand();

            //连接数据库
            conn.Open();

            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            string sql = "select * from PSD_RW where RowId=" + Parameter.ToString();
            SqlDataAdapter da = new SqlDataAdapter(sql, conn);
            da.Fill(ds, "PSD_RW");
            dt = ds.Tables["PSD_RW"];

            //关闭连接
            conn.Close();

            return dt;
        }

        [WebMethod]
        public DataTable QueryLog(DateTime start, DateTime end)
        {
            string ConnectionString = @"server=192.6.94.25\QYHY;database=Plane_1;uid=sa;pwd=xiaoming";
            SqlConnection conn = new SqlConnection(ConnectionString);
            SqlCommand cmd = conn.CreateCommand();

            //连接数据库
            conn.Open();

            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            string sql = "select * from LOG Where DateTime between '" + start.ToString("yyyy-MM-dd HH:mm:ss") + "' and '" + end.ToString("yyyy-MM-dd HH:mm:ss") + "'";
            SqlDataAdapter da = new SqlDataAdapter(sql, conn);
            da.Fill(ds, "LOG");
            dt = ds.Tables["LOG"];

            //关闭连接
            conn.Close();

            return dt;
        }
    }
}
