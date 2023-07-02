using Newtonsoft.Json;

namespace z3lx.solo2yolo.Deserialization.DataModels
{
    [System.Serializable]
    public abstract class AnnotationDefinition
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
    }
}