using Newtonsoft.Json;
using System.Numerics;
using z3lx.solo2yolo.Json.Converters;

namespace z3lx.solo2yolo.Json.DataModels
{
    /// <summary>
    /// Extends the capture class with data specific for an RGB camera sensor.
    /// </summary>
    [Serializable]
    public sealed class RgbCapture : Capture
    {
        /// <summary>
        /// A single file that stores sensor captured data.
        /// </summary>
        [JsonProperty("filename")]
        public string FileName { get; set; }

        /// <summary>
        /// The format of the sensor captured file.
        /// </summary>
        [JsonProperty("imageFormat")]
        public string ImageFormat { get; set; }

        /// <summary>
        /// The image size in pixels (width/height).
        /// </summary>
        [JsonProperty("dimension")]
        [JsonConverter(typeof(Vector2Converter))]
        public Vector2 Dimension { get; set; }

        /// <summary>
        /// Holds the type of projection the camera used for the capture (perspective/orthographic).
        /// </summary>
        [JsonProperty("projection")]
        public string Projection { get; set; }

        /// <summary>
        /// The projection matrix of the camera.
        /// </summary>
        [JsonProperty("matrix")]
        public float[] Matrix { get; set; }
    }
}