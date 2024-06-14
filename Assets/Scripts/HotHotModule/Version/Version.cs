using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace HotHot
{
    public class Version<T> where T : RawABInfo
    {
        public string number;
        public long timestamp;
        public List<T> abInfoList;
    }

    public static class VersionMethod
    {
        public static Version<RawABInfo> ToRawVersion(this Version<ABInfo> self)
        {
            var ret = new Version<RawABInfo>();
            ret.number = self.number;
            ret.timestamp = self.timestamp;
            ret.abInfoList = self.abInfoList.Select(abInfo => abInfo as RawABInfo).ToList();
            return ret;
        }

        public static Version<ABInfo> ToVersion(this Version<RawABInfo> self)
        {
            var ret = new Version<ABInfo>();
            ret.number = self.number;
            ret.timestamp = self.timestamp;
            ret.abInfoList = self.abInfoList.Select(rawABInfo => new ABInfo { fileName = rawABInfo.fileName, md5 = rawABInfo.md5, address = self.timestamp }).ToList();
            return ret;
        }

        public static void WriteVersionFile(string path, Version<ABInfo> version)
        {
            var versionBytes = JsonConvert.SerializeObject(version).StrToUtf8();
            FileHelper.CreateFile(Path.Combine(path, VersionStaticField.versionFileName), versionBytes);
        }

        public static Version<ABInfo> ReadVersionFile(string path)
        {
            var jsonStr = File.ReadAllBytes(path).Utf8ToStr();
            return JsonConvert.DeserializeObject<Version<ABInfo>>(jsonStr);
        }

        public static List<RawABInfo> FetchChangesList(Version<RawABInfo> x, Version<RawABInfo> y)
        {
            //使 y 时间戳大于 x 
            if (y.timestamp < x.timestamp)
                (y, x) = (x, y);
            var xList = x.abInfoList;
            var yList = y.abInfoList;
            return yList.Where(yInfo => !xList.Exists(xInfo => xInfo.fileName == yInfo.fileName && xInfo.md5 == yInfo.md5)).ToList();
        }

        public static List<RawABInfo> FetchUnchangesList(Version<RawABInfo> x, Version<RawABInfo> y)
        {
            //使 y 时间戳大于 x 
            if (y.timestamp < x.timestamp)
                (y, x) = (x, y);
            var xList = x.abInfoList;
            var yList = y.abInfoList;
            return yList.Where(yInfo => xList.Exists(xInfo => xInfo.fileName == yInfo.fileName && xInfo.md5 == yInfo.md5)).ToList();
        }
    }

    public static class VersionStaticField
    {
        public static readonly string versionFileName = "Version.v";
    }
}