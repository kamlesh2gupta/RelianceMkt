// Helpers/DBHelper.cs
using System.Configuration;

public static class DBHelper
{
    public static string GetConnectionString()
    {
        string env = ConfigurationManager.AppSettings["ActiveDB"];

        if (env == "Server")
        {
            // Server wali string — same jo server Web.config mein hai
            return "Data Source=10.126.143.86,1981;Initial Catalog=DIGIMYIN;" +
                   "User ID=reliance_user;Password=pass@123;" +
                   "MultipleActiveResultSets=True;Connection Timeout=10000;";
        }
        else
        {
            // Local wali string
            return ConfigurationManager
                .ConnectionStrings["LocalConnection"].ConnectionString;
        }
    }
}