using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using Unity.Properties;
using z3lx.solo2yolo.Deserialization.DataModels;

namespace z3lx.solo2yolo
{
    /// <summary>
    /// Provides functionality to convert a SOLO dataset to the YOLO format.
    /// </summary>
    public static class DatasetConverter
    {
        // TODO: Add image dimension verification
        // TODO: Add logging capabilities
        /// <summary>
        /// Converts a SOLO dataset to the YOLO format.
        /// </summary>
        /// <param name="soloPath">The path to the SOLO dataset.</param>
        /// <param name="outputPath">The path to store the converted YOLO dataset.</param>
        /// <param name="task">The computer vision task conversion to perform.</param>
        /// <param name="log">Flag indicating whether to enable logging.</param>
        public static void Convert(string soloPath, string outputPath, ComputerVisionTask task, bool log = false)
        {
            ValidateAndSanitizePath(ref soloPath);
            ValidateAndSanitizePath(ref outputPath);

            string yoloPath = CreateYoloDirectory(outputPath);

            Metadata metadata = DeserializeObjectFromFile<Metadata>(Path.Combine(soloPath, "metadata.json"));

            // Iterate through all sequences and frames
            int labelingFrameCount = 0;
            int sequenceCount = 0;
            for (int i = 0; sequenceCount < metadata.TotalSequences; i++)
            {
                string sequencePath = Path.Combine(soloPath, $"sequence.{i}");
                if (!Directory.Exists(sequencePath))
                    continue;

                sequenceCount++;

                int frameCount = 0;
                int framesInSequence = Directory.EnumerateFiles(sequencePath, "*.json").ToArray().Length;
                for (int j = 0; frameCount < framesInSequence; j++)
                {
                    string frameDataPath = Path.Combine(sequencePath, $"step{j}.frame_data.json");
                    if (!File.Exists(frameDataPath))
                        continue;

                    frameCount++;

                    FrameData frameData = DeserializeObjectFromFile<FrameData>(frameDataPath);

                    // Hardcoded RGB capture
                    RgbCapture capture = frameData.Captures.OfType<RgbCapture>().FirstOrDefault();
                    if (capture == null || string.IsNullOrEmpty(capture.FileName))
                        continue;

                    // Convert annotations for the frame depending on the task
                    string labelingData = ConvertLabelingData(capture, task);
                    if (string.IsNullOrEmpty(labelingData))
                        continue;

                    // Write labeling data to output
                    string labelingDataPath = Path.Combine(yoloPath, "labels", $"{labelingFrameCount:D12}.txt");
                    File.WriteAllText(labelingDataPath, labelingData);

                    // Copy image to output
                    string sourceImagePath = Path.Combine(sequencePath, capture.FileName);
                    string destImagePath = Path.Combine(yoloPath, "images", $"{labelingFrameCount:D12}.{capture.ImageFormat.ToLower()}");
                    File.Copy(sourceImagePath, destImagePath);

                    labelingFrameCount++;
                }
            }

            // TODO: Add support for multiple annotation definitions
            AnnotationDefinition annotationDefinition = DeserializeObjectFromFile<AnnotationDefinitions>(Path.Combine(soloPath, "annotation_definitions.json")).Values[0];
            GenerateDatasetYaml(yoloPath, annotationDefinition);
        }

        // TODO: Add validations
        /// <summary>
        /// Deserializes a JSON file into an object.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <param name="filePath">The path to the JSON file.</param>
        /// <returns>The deserialized object.</returns>
        private static T DeserializeObjectFromFile<T>(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Could not find {Path.GetFileName(filePath)}.");

            string data = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<T>(data);
        }

        /// <summary>
        /// Validates and sanitizes a file or directory path.
        /// </summary>
        /// <param name="path">The path to validate and sanitize.</param>
        private static void ValidateAndSanitizePath(ref string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new InvalidPathException("Path cannot be null or empty.");

            path = path.Trim();

            if (Path.GetInvalidPathChars().Any(path.Contains))
                throw new InvalidPathException("Path contains invalid characters.");

            if (!Path.IsPathRooted(path))
                throw new InvalidPathException("Path is not an absolute path.");

            if (!Directory.Exists(path))
                throw new InvalidPathException("Path does not exist.");
        }

        /// <summary>
        /// Creates a YOLO directory for storing the converted dataset.
        /// </summary>
        /// <param name="path">The base path to create the YOLO directory.</param>
        /// <returns>The path to the created YOLO directory.</returns>
        private static string CreateYoloDirectory(string path)
        {
            int i = 0;
            string yoloPath = Path.Combine(path, "yolo");

            while (Directory.Exists(yoloPath))
            {
                i++;
                yoloPath = Path.Combine(path, $"yolo_{i}");
            }

            Directory.CreateDirectory(yoloPath);
            Directory.CreateDirectory(Path.Combine(yoloPath, "images"));
            Directory.CreateDirectory(Path.Combine(yoloPath, "labels"));
            return yoloPath;
        }

        // Can there be multiple of the same annotation type?
        // TODO: Add support for other computer vision tasks
        /// <summary>
        /// Converts the labeling data based on the computer vision task.
        /// </summary>
        /// <param name="capture">The capture to convert.</param>
        /// <param name="task">The computer vision task.</param>
        /// <returns>The converted labeling data.</returns>
        private static string ConvertLabelingData(RgbCapture capture, ComputerVisionTask task)
        {
            string labelingData = string.Empty;

            switch (task)
            {
                case ComputerVisionTask.Classify:
                    throw new NotImplementedException();

                case ComputerVisionTask.Detect:
                    BoundingBox2DAnnotation annotation = capture.Annotations.OfType<BoundingBox2DAnnotation>().FirstOrDefault();
                    if (annotation == null)
                        return null;

                    foreach (BoundingBox2DAnnotation.Value value in annotation.Values)
                    {
                        int objectClassId = value.LabelId;
                        double x = (double)(value.Origin.x + (value.Dimension.x / 2)) / capture.Dimension.x;
                        double y = (double)(value.Origin.y + (value.Dimension.y / 2)) / capture.Dimension.y;
                        double width = (double)value.Dimension.x / capture.Dimension.x;
                        double height = (double)value.Dimension.y / capture.Dimension.y;
                        labelingData += $"{objectClassId} {x} {y} {width} {height}\n";
                    }
                    break;

                case ComputerVisionTask.Segment:
                    throw new NotImplementedException();

                case ComputerVisionTask.Pose:
                    throw new NotImplementedException();

                default:
                    throw new ArgumentOutOfRangeException(nameof(task), task, "Unsupported task type.");
            }

            return labelingData;
        }

        /// <summary>
        /// Generates the dataset YAML file for YOLO.
        /// </summary>
        /// <param name="yoloPath">The path to the YOLO directory.</param>
        /// <param name="annotationDefinition">The annotation definition.</param>
        private static void GenerateDatasetYaml(string yoloPath, AnnotationDefinition annotationDefinition)
        {
            string content =
                $"path: {yoloPath} # dataset root dir\n" +
                "train: images # train images (relative to 'path')\n" +
                "val: images # val images (relative to 'path')\n" +
                "test: # test images (optional)\n" +
                "\n" +
                "# Classes\n" +
                "names:\n";

            foreach (AnnotationDefinition.Specification spec in annotationDefinition.Specifications)
                content += $"  {spec.LabelId}: {spec.labelName}\n";

            string yamlFilePath = Path.Combine(yoloPath, "dataset.yaml");
            File.WriteAllText(yamlFilePath, content);
        }
    }
}