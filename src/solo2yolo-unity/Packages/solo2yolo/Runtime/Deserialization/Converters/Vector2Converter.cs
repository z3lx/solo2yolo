using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

namespace z3lx.solo2yolo.Deserialization.Converters
{
    public class Vector2Converter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Vector2);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JArray jsonArray = JArray.Load(reader);
            if (jsonArray.Count != 2)
                throw new JsonException("Invalid array length for Vector2");

            float x = jsonArray[0].Value<float>();
            float y = jsonArray[1].Value<float>();

            return new Vector2(x, y);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}