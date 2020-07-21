using Consul;

using System;
using System.Configuration;
using System.Net;
using System.Text;
using System.Threading.Tasks;

public static class ConsulHelper
{
    private const string connectionStringsDirectory = "ConnectionStrings";
    private const string settingsDirectory = "Settings";
    public static string GetConnectionString(string connectionString)
    {
        var connStr = ConfigurationManager.ConnectionStrings[connectionString];
        return connStr != null ?
            connStr.ConnectionString :
            GetValueByKey($"{connectionStringsDirectory}/{connectionString}").Result;
    }
    public static string GetSettingsValue(string key)
    {
        var value = ConfigurationManager.AppSettings[key];
        return !string.IsNullOrEmpty(value) ?
            value :
            GetValueByKey($"{settingsDirectory}/{key}").Result;
    }

    private static async Task<string> GetValueByKey(string key)
    {

        using (IConsulClient client = new ConsulClient(
            config => config.Address = new Uri(ConfigurationManager.ConnectionStrings["ConsulClient"].ConnectionString),
            hc => hc.Timeout = TimeSpan.FromSeconds(double.Parse(ConfigurationManager.AppSettings["ConsulTimeout"])))
        )
        {
            string value = string.Empty;
            try
            {
                var response = await client.KV.Get(key);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    value = Encoding.UTF8.GetString(response.Response.Value);
                }
            }
            catch (Exception ex)
            {
            }
            return value;
        }
    }
}