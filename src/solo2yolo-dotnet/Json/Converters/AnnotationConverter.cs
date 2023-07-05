using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using z3lx.solo2yolo.Json.DataModels;

namespace z3lx.solo2yolo.Json.Converters
{
    public class AnnotationConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Annotation);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);

            JToken jsonToken = jsonObject["@type"];
            if (jsonToken == null || jsonToken.Type != JTokenType.String)
                throw new JsonSerializationException("Invalid @type property.");

            string type = jsonObject["@type"].Value<string>();
            Annotation annotation = type switch
            {
                "type.unity.com/unity.solo.BoundingBox2DAnnotation" => new BoundingBox2DAnnotation(),
                "type.unity.com/unity.solo.InstanceSegmentationAnnotation" => throw new NotImplementedException(),
                "type.unity.com/unity.solo.KeypointAnnotation" => throw new NotImplementedException(),
                _ => throw new NotImplementedException(),
            };
            serializer.Populate(jsonObject.CreateReader(), annotation);
            return annotation;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}