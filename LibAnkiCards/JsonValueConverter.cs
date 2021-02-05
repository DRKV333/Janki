using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace LibAnkiCards
{
    public static class JsonValueConverter<T>
    {
        private static readonly ValueConverter<T, string> instace = new ValueConverter<T, string>(
            x => JsonConvert.SerializeObject(x),
            x => JsonConvert.DeserializeObject<T>(x)
        );

        public static ValueConverter<T, string> Instace => instace;
    }
}