using System.Diagnostics;

namespace FileEraser
{
    public static class Format
    {
        static string[] sizeSuffixes = {
        "byte", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        public static string ByteSize(long size)
        {
            Debug.Assert(sizeSuffixes.Length > 0);

            const string formatTemplate = "{0}{1:0.##} {2}{3}";

            if (size == 0)
            {
                return string.Format(formatTemplate, null, 0, sizeSuffixes[0], "s");
            }

            var absSize = Math.Abs((double)size);
            var fpPower = Math.Log(absSize, 1000);
            var intPower = (int)fpPower;
            var iUnit = intPower >= sizeSuffixes.Length
                ? sizeSuffixes.Length - 1
                : intPower;
            var normSize = absSize / Math.Pow(1000, iUnit);

            return string.Format(
                formatTemplate,
                size < 0 ? "-" : null, normSize, sizeSuffixes[iUnit], (iUnit == 0 && size != 1) ? "s" : "");
        }
    }
}
