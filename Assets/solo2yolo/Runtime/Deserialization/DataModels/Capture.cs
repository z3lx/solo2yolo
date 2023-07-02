using Newtonsoft.Json;
using UnityEngine;
using z3lx.solo2yolo.Deserialization.Converters;

namespace z3lx.solo2yolo.Deserialization.DataModels
{
    /// <summary>
    /// A capture record contains the relationship between a captured file, a collection of annotations, and extra
    /// metadata that describes the state of the sensor.
    /// </summary>
    [System.Serializable]
    [JsonConverter(typeof(CaptureConverter))]
    public abstract class Capture
    {
        /// <summary>
        /// The class type of the sensor.
        /// </summary>
        [JsonProperty("@type")]
        public string Type { get; set; }

        /// <summary>
        /// The ID of the sensor that made the capture.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Human readable description of the sensor.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Position in meters: (x, y, z) with respect to the global coordinate system.
        /// </summary>
        [JsonProperty("position")]
        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 Position { get; set; }

        /// <summary>
        /// Orientation as quaternion: w, x, y, z.
        /// </summary>
        [JsonProperty("rotation")]
        [JsonConverter(typeof(Vector4Converter))]
        public Vector4 Rotation { get; set; }

        /// <summary>
        /// Velocity in meters per second as v_x, v_y, v_z.
        /// </summary>
        [JsonProperty("velocity")]
        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 Velocity { get; set; }

        /// <summary>
        /// Acceleration in meters per second^2 as a_x, a_y, a_z.
        /// </summary>
        [JsonProperty("acceleration")]
        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 Acceleration { get; set; }

        /// <summary>
        /// List of the annotations in this capture.
        /// </summary>
        [JsonProperty("annotations")]
        public Annotation[] Annotations { get; set; }
    }
}