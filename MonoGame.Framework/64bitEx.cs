// #region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2009 The MonoGame Team
//

using System;
using Foundation;

namespace MonoGame.Framework.iOS
{
    public static class _64bitEx
    {
        public static DateTime NSDateToDateTime(this NSDate date)
        {
            DateTime reference = TimeZone.CurrentTimeZone.ToLocalTime( 
                new DateTime(2001, 1, 1, 0, 0, 0) );
            return reference.AddSeconds(date.SecondsSinceReferenceDate);
        }

        public static NSDate DateTimeToNSDate(this DateTime date)
        {
            DateTime reference = TimeZone.CurrentTimeZone.ToLocalTime(
                new DateTime(2001, 1, 1, 0, 0, 0) );
            return NSDate.FromTimeIntervalSinceReferenceDate(
                (date - reference).TotalSeconds);
        }
    }
}

