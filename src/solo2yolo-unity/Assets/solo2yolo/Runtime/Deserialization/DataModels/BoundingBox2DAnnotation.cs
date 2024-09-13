using Newtonsoft.Json;
using UnityEngine;
using z3lx.solo2yolo.Deserialization.Converters;

namespace z3lx.solo2yolo.Deserialization.DataModels
{
    /// <summary>
    /// Each bounding box record maps a tuple of (instance, label) to a set of 4 variables (x, y, width, height) that draws a
    /// bounding box. The OpenCV 2D coordinate system is followed, where the origin (0,0), (x = 0, y = 0) is at the top left of the image.
    /// </summary>
    [System.Serializable]
    public sealed class BoundingBox2DAnnotation : Annotation
    {
        /// <summary>
        /// Array of bounding boxes in the frame.
        /// </summary>
        [JsonProperty("values")]
        public Value[] Values { get; set; }

        [System.Serializable]
        [JsonObject(ItemRequired = Required.Always)]
        public class Value
        {
            /// <summary>
            /// Integer id of the entity.
            /// </summary>
            [JsonProperty("instanceId")]
            public int InstanceId { get; set; }

            /// <summary>
            /// Integer identifier of the label.
            /// </summary>
            [JsonProperty("labelId")]
            public int LabelId { get; set; }

            /// <summary>
            /// String identifier of the label.
            /// </summary>
            [JsonProperty("labelName")]
            public string LabelName { get; set; }

            /// <summary>
            /// The pixel location of the upper left corner of the box.
            /// </summary>
            [JsonProperty("origin")]
            [JsonConverter(typeof(Vector2Converter))]
            public Vector2 Origin { get; set; }

            /// <summary>
            /// The number of pixels in the x and y direction.
            /// </summary>
            [JsonProperty("dimension")]
            [JsonConverter(typeof(Vector2Converter))]
            public Vector2 Dimension { get; set; }
        }
    }
}