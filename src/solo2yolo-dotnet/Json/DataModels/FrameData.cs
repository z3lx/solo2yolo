using Newtonsoft.Json;

namespace z3lx.solo2yolo.Json.DataModels
{
    [Serializable]
    public sealed class FrameData
    {
        /// <summary>
        /// The integer ID of the frame.
        /// </summary>
        [JsonProperty("frame")]
        public int Frame { get; set; }

        /// <summary>
        /// The sequence number.
        /// </summary>
        [JsonProperty("sequence")]
        public int Sequence { get; set; }

        /// <summary>
        /// The step inside the sequence.
        /// </summary>
        [JsonProperty("step")]
        public int Step { get; set; }

        /// <summary>
        /// Timestamp in milliseconds since the sequence started.
        /// </summary>
        [JsonProperty("timestamp")]
        public float Timestamp { get; set; }

        /// <summary>
        /// The list of captures.
        /// </summary>
        [JsonProperty("captures")]
        public Capture[]? Captures { get; set; }
    }
}