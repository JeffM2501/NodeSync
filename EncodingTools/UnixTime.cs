using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EncodingTools
{
    public static class UnixTime
    {
        public static UInt32 GetUnixTime()
        {
            return (UInt32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        public static UInt32 GetTokenMinutes()
        {
            return (UInt32)(DateTime.UtcNow.Subtract(new DateTime(2000, 6, 1))).TotalMinutes;
        }

        public static int TimeDelta(string time)
        {
            UInt32 t = UInt32.MaxValue;
            UInt32.TryParse(time, out t);
            return (int)((Int64)GetUnixTime() - (Int64)t);
        }
    }
}
