using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MGDataAccessLibrary.BussinessLogic
{
    public static class AppVersionProcessor
    {
        private const string GoogleAppId = "https://play.google.com/store/apps/details?id=com.ManageGo.ManageGo";
        private const string AppleAppId = "https://apps.apple.com/us/app/managego-tenants/id1459683580";
        public static async Task<bool> AppNeedsUpdate(int currentVersion, DevicePlatform devicePlatform)
        {
            // var url = devicePlatform == DevicePlatform.Android ? GoogleAppId : AppleAppId;

            var content = await DataAccess.WebAPI.Get("https://docs.google.com/spreadsheets/d/e/2PACX-1vS4BqQ3tUSShsGYHjWLZcbsNwROaf1fV1cLTz4m4feBiTI-FfVj4_tBpUAY-qP0jYcQGj8HCzSnq_p1/pub?gid=0&single=true&output=csv" +
                $"&localhash={Guid.NewGuid().ToString("N")}");
            if (string.IsNullOrWhiteSpace(content))
                return false;
            var lines = content.Split("\r\n".ToCharArray());
            if (lines.Count() < 2)
                return false;
            var lastLine = lines.Last();
            var versions = lastLine.Split(',');
            if (versions.Count() < 2)
                return false;
            if (devicePlatform == DevicePlatform.Android && int.TryParse(versions[0].Replace(".", ""), out int droidVersion))
            {
                var result = droidVersion > currentVersion;
                return result;
            }
            else if (devicePlatform == DevicePlatform.iOS && int.TryParse(versions[1].Replace(".", ""), out int iosVersion))
            {
                var result = iosVersion > currentVersion;
                return result;
            }
            return false;
            /*
            string pattern;
            if (devicePlatform == DevicePlatform.Android)
            {
                pattern = "Current Version</div><span class=\"htlgb\"><div class=\"IQ1z0d\"><span class=\"htlgb\">(?<version>\\d*.{0,1}\\d*.{0,1}\\d*)</span>";
            }
            else
            {
                pattern = "versionString\":\"(?<version>\\d.{0,1}\\d.{0,1}\\d)\",\"releaseDate\":";
            }
            var match = Regex.Match(content, pattern);
            if (match.Success)
            {
                var ver = match.Groups["version"]?.ToString().Replace(".", "");
                var result = int.TryParse(ver, out int digitVer) && digitVer > currentVersion;
                return result;
            }

            return false;
            */
        }
    }


}
