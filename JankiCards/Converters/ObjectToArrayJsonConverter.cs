using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JankiCards.Converters
{
    //https://stackoverflow.com/questions/39461518/how-to-deserialize-an-array-of-values-with-a-fixed-schema-to-a-strongly-typed-da/39462464#39462464
    internal class ObjectToArrayJsonConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(T) == objectType;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Type objectType = value.GetType();

            if (!(serializer.ContractResolver.ResolveContract(objectType) is JsonObjectContract contract))
                throw new JsonSerializationException(string.Format("invalid type {0}.", objectType.FullName));

            writer.WriteStartArray();

            foreach (var property in SerializableProperties(contract))
            {
                object propertyValue = property.ValueProvider.GetValue(value);
                if (property.Converter != null && property.Converter.CanWrite)
                    property.Converter.WriteJson(writer, propertyValue, serializer);
                else
                    serializer.Serialize(writer, propertyValue);
            }

            writer.WriteEndArray();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (!(serializer.ContractResolver.ResolveContract(objectType) is JsonObjectContract contract))
                throw new JsonSerializationException(string.Format("invalid type {0}.", objectType.FullName));

            if (MoveToContentAndAssert(reader).TokenType == JsonToken.Null)
                return null;
            if (reader.TokenType != JsonToken.StartArray)
                throw new JsonSerializationException(string.Format("token {0} was not JsonToken.StartArray", reader.TokenType));

            // Not implemented: JsonObjectContract.CreatorParameters, serialization callbacks,
            existingValue = existingValue ?? contract.DefaultCreator();

            foreach (var property in SerializableProperties(contract))
            {
                if (ReadToContentAndAssert(reader).TokenType == JsonToken.EndArray)
                    return existingValue;

                object propertyValue;

                // Not implemented:
                // https://www.newtonsoft.com/json/help/html/Properties_T_Newtonsoft_Json_Serialization_JsonProperty.htm
                // JsonProperty.ItemConverter, ItemIsReference, ItemReferenceLoopHandling, ItemTypeNameHandling, DefaultValue, DefaultValueHandling, ReferenceLoopHandling, Required, TypeNameHandling, ...

                if (property.Converter != null && property.Converter.CanRead)
                    propertyValue = property.Converter.ReadJson(reader, property.PropertyType, property.ValueProvider.GetValue(existingValue), serializer);
                else
                    propertyValue = serializer.Deserialize(reader, property.PropertyType);

                property.ValueProvider.SetValue(existingValue, propertyValue);
            }

            while (ReadToContentAndAssert(reader).TokenType != JsonToken.EndArray)
                reader.Skip();

            return existingValue;
        }

        private static IEnumerable<JsonProperty> SerializableProperties(JsonObjectContract contract)
        {
            return contract.Properties.Where(p => !p.Ignored && p.Readable && p.Writable);
        }

        private static JsonReader ReadToContentAndAssert(JsonReader reader)
        {
            return MoveToContentAndAssert(ReadAndAssert(reader));
        }

        private static JsonReader MoveToContentAndAssert(JsonReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            if (reader.TokenType == JsonToken.None)       // Skip past beginning of stream.
                ReadAndAssert(reader);
            while (reader.TokenType == JsonToken.Comment) // Skip past comments.
                ReadAndAssert(reader);
            return reader;
        }

        private static JsonReader ReadAndAssert(JsonReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            if (!reader.Read())
                throw new JsonReaderException("Unexpected end of JSON stream.");
            return reader;
        }
    }
}