using Newtonsoft.Json;

namespace z3lx.solo2yolo.Json.DataModels
{
    [Serializable]
    [JsonObject(ItemRequired = Required.Always)]
    public sealed class Metadata
    {
        [JsonProperty("unityVersion")]
        public string UnityVersion { get; set; }

        [JsonProperty("perceptionVersion")]
        public string PerceptionVersion { get; set; }

        [JsonProperty("renderPipeline")]
        public string RenderPipeline { get; set; }

        [JsonProperty("simulationStartTime")]
        public string SimulationStartTime { get; set; }

        [JsonProperty("scenarioRandomSeed")]
        public int ScenarioRandomSeed { get; set; }

        [JsonProperty("scenarioActiveRandomizers")]
        public string[] ScenarioActiveRandomizers { get; set; }

        [JsonProperty("totalFrames")]
        public int TotalFrames { get; set; }

        [JsonProperty("totalSequences")]
        public int TotalSequences { get; set; }

        [JsonProperty("sensors")]
        public string[] Sensors { get; set; }

        [JsonProperty("metricCollectors")]
        public string[] MetricCollectors { get; set; }

        [JsonProperty("simulationEndTime")]
        public string SimulationEndTime { get; set; }

        [JsonProperty("annotators")]
        public Annotator[] Annotators { get; set; }

        [Serializable]
        public class Annotator
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }
        }
    }
}