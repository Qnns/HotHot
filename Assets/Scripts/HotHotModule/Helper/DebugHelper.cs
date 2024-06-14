namespace HotHot
{
    public static class DebugHelper
    {
        public static string AddPrefixLine4Long(this string str) => string.Format("------------------------------   {0,-30}", str);
        public static string AddPrefixLine4Short(this string str) => string.Format("--- {0,-18}", str);


        public static string ToGreen(this string str) => string.Format("<color=green>{0}</color>", str);

        public static string ToRed(this string str) => string.Format("<color=red>{0}</color>", str);

    }
}