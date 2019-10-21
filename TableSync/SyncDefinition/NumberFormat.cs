using System;

namespace TableSync
{
    public enum NumberFormat
    {
        None,
        DateTime,
        Date,
        Time,
        Currency,
        Decimal1,
        Percent,
        Custom
    }

    public static class NumberFormatExtensions
    {
        public static string FormatString(this NumberFormat value)
        {
            switch (value)
            {
                case NumberFormat.None:
                    return null;
                case NumberFormat.DateTime:
                    return("dd.mm.yyyy HH:MM");
                case NumberFormat.Date:
                    return ("dd.mm.yyyy");
                case NumberFormat.Time:
                    return ("HH:MM");
                case NumberFormat.Currency:
                    return ("#,##0.00 €");
                case NumberFormat.Decimal1:
                    return ("0.0");
                case NumberFormat.Percent:
                    return ("0%");
                default:
                    throw new ArgumentException("value");
            }
        }
    }
}
