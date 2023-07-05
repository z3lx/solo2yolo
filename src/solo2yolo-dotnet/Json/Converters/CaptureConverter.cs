using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using z3lx.solo2yolo.Json.DataModels;

namespace z3lx.solo2yolo.Json.Converters
{
    public class CaptureConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Capture);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);

            JToken jsonToken = jsonObject["@type"];
            if (jsonToken == null || jsonToken.Type != JTokenType.String)
                throw new JsonSerializationException("Invalid @type property.");

            string type = jsonObject["@type"].Value<string>();
            Capture capture = type switch
            {
                "type.unity.com/unity.solo.RGBCamera" => new RgbCapture(),
                _ => throw new NotImplementedException(),
            };
            serializer.Populate(jsonObject.CreateReader(), capture);
            return capture;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}