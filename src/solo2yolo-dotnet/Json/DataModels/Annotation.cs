using Newtonsoft.Json;
using z3lx.solo2yolo.Json.Converters;

namespace z3lx.solo2yolo.Json.DataModels
{
    /// <summary>
    /// An annotation record contains the ground truth for a sensor either inline or in a separate file. A single capture may
    /// contain many annotations each corresponding to one active Labeler in the simulation.
    /// </summary>
    [Serializable]
    [JsonObject(ItemRequired = Required.Always)]
    [JsonConverter(typeof(AnnotationConverter))]
    public abstract class Annotation
    {
        /// <summary>
        /// The class type of the annotation.
        /// </summary>
        [JsonProperty("@type")]
        public string Type { get; set; }

        /// <summary>
        /// The registered ID of the annotation.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The ID of the sensor that this annotation is attached to.
        /// </summary>
        [JsonProperty("sensorId")]
        public string SensorId { get; set; }

        /// <summary>
        /// The human readable description of the sensor.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
