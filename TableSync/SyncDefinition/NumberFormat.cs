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
        Decimal,
        Decimal1,
        Decimal2,
        Percent,
        Percent1,
        Percent2,
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
                case NumberFormat.Decimal:
                    return ("0");
                case NumberFormat.Decimal1:
                    return ("0.0");
                case NumberFormat.Decimal2:
                    return ("0.00");
                case NumberFormat.Percent:
                    return ("0%");
                case NumberFormat.Percent1:
                    return ("0.0%");
                case NumberFormat.Percent2:
                    return ("0.00%");
                default:
                    throw new ArgumentException("value");
            }
        }
    }
}
