using Newtonsoft.Json;

namespace z3lx.solo2yolo.Deserialization.DataModels
{
    [System.Serializable]
    [JsonObject(ItemRequired = Required.Always)]
    public sealed class AnnotationDefinitions
    {
        [JsonProperty("annotationDefinitions")]
        public AnnotationDefinition[] Values { get; set; }
    }

    [System.Serializable]
    [JsonObject(ItemRequired = Required.Always)]
    public sealed class AnnotationDefinition
    {
        /// <summary>
        /// The class type of the annotation.
        /// </summary>
        [JsonProperty("@type")]
        public string Type { get; set; }

        /// <summary>
        /// The registered ID of the annotation, assigned in the Perception Camera UI.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The description of this annotation definition.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Format-specific specification for the annotation values.
        /// </summary>
        [JsonProperty("spec")]
        public Specification[] Specifications { get; set; }

        // TODO: Check annotation-specific properties
        [System.Serializable]
        [JsonObject(ItemRequired = Required.Always)]
        public sealed class Specification
        {
            [JsonProperty("label_id")]
            public int LabelId { get; set; }

            [JsonProperty("label_name")]
            public string labelName { get; set; }
        }
    }
}