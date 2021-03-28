using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Collections.Generic;

namespace LibAnkiCards.Converters
{
    internal static class SplitStringValueConverter
    {
        public static ValueConverter<List<string>, string> SeparatedBy(char separator) => new ValueConverter<List<string>, string>(
            x => string.Join(separator.ToString(), x),
            x => new List<string>(x.Split(separator))
        );
    }
}