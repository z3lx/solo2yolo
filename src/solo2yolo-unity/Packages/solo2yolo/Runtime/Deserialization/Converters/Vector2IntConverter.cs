using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

namespace z3lx.solo2yolo.Deserialization.Converters
{
    public class Vector2IntConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Vector2Int);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JArray jsonArray = JArray.Load(reader);
            if (jsonArray.Count != 2)
                throw new JsonException("Invalid array length for Vector2Int");

            int x = jsonArray[0].Value<int>();
            int y = jsonArray[1].Value<int>();

            return new Vector2Int(x, y);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}