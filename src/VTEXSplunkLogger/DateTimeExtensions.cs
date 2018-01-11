using System;

namespace Vtex
{
    /// <summary>
    /// This class contains DateTime extension method to simplify small DateTime usage.
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Return informed DateTime without seconds and milisecond.
        /// </summary>
        /// <returns>The second mili second.</returns>
        /// <param name="baseDate">Base date.</param>
        public static DateTime RemoveSecondMiliSecond(this DateTime baseDate)
        {
            return new DateTime(baseDate.Year, baseDate.Month, baseDate.Day, baseDate.Hour, baseDate.Minute, 0);
        }
    }
}