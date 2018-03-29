using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SalesForceMap
{
    class Setting_variabel
    {
        public static string settingPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public static string NAMA = "Muhammad Hanif Budiarto";
        public static bool IS_VISIBLE_CUST_MARKER = true;
        public static bool IS_VISIBLE_SALES_MARKER = true;

        public static string mykoneksi_sql = @"Server=118.98.73.151\gss; Database=Altius2; Trusted_Connection=true; user id=arief; password=testing; Integrated Security=false; Connect Timeout=10; MultipleActiveResultSets=true; Max Pool Size=200;";
       
        public static string CONS_ALAMATIP="118.98.73.151";
        public static string CONS_INSTANCE = "gss";
        public static string CONS_NAMADB = "Altius2";
        public static string CONS_USERNAME = "arief";
        public static string CONS_PASSWORD = "testing";
        public static void Update_UserPassmykoneksi_sql(string user, string pass)
        {
            CONS_USERNAME = user;
            CONS_PASSWORD = pass;
            mykoneksi_sql = @"Server = "+CONS_ALAMATIP+@"\"+CONS_INSTANCE+"; Database =" + CONS_NAMADB + "; Trusted_Connection=true; user id=" + user + "; password=" + pass + "; Integrated Security=false; Connect Timeout=10; MultipleActiveResultSets=true";
        }

       

    }
}
