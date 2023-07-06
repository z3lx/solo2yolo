using Newtonsoft.Json;
using System.Text.RegularExpressions;
using z3lx.solo2yolo.Json.DataModels;

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

            // TODO: Do checks for correct metadata/definition json

            Metadata metadata = DeserializeObjectFromFile<Metadata>(Path.Combine(soloPath, "metadata.json"));

            IEnumerable<string> sequencePaths = Directory.EnumerateDirectories(soloPath, "sequence.*", SearchOption.TopDirectoryOnly)
                .OrderBy(sequencePath => GetIndexFromPath(sequencePath, "sequence.*"));

            IEnumerable<string> frameDataPaths = sequencePaths
                .SelectMany(sequencePath => Directory.EnumerateFiles(sequencePath, "step*.frame_data.json", SearchOption.TopDirectoryOnly)
                .OrderBy(frameDataPath => GetIndexFromPath(frameDataPath, "step*.frame_data.json")));

            string yoloPath = CreateYoloDirectory(outputPath);

            int index = 0;
            foreach (string frameDataPath in frameDataPaths)
            {
                FrameData frameData = DeserializeObjectFromFile<FrameData>(frameDataPath);
                if (frameData.Captures == null)
                    continue;

                // Hardcoded RGB capture
                RgbCapture capture = frameData.Captures.OfType<RgbCapture>().FirstOrDefault();
                if (capture == null || string.IsNullOrEmpty(capture.FileName))
                    continue;

                // Convert annotations for the frame depending on the task
                string labelingData = ConvertLabelingData(capture, task);
                if (string.IsNullOrEmpty(labelingData))
                    continue;

                // Write labeling data to output
                string labelingDataPath = Path.Combine(yoloPath, "labels", $"{index:D12}.txt");
                File.WriteAllText(labelingDataPath, labelingData);

                // Copy image to output
                string sourceImagePath = Path.Combine(Directory.GetParent(frameDataPath).FullName, capture.FileName);
                string destImagePath = Path.Combine(yoloPath, "images", $"{index:D12}.{capture.ImageFormat.ToLower()}");
                File.Copy(sourceImagePath, destImagePath);

                index++;
            }

            AnnotationDefinition annotationDefinition = DeserializeObjectFromFile<AnnotationDefinitions>(Path.Combine(soloPath, "annotation_definitions.json")).Values[0];
            GenerateDatasetYaml(yoloPath, annotationDefinition);
        }

        static int GetIndexFromPath(string path, string format)
        {
            string fileName = Path.GetFileName(path);
            string pattern = format.Replace("*", "(\\d+)");
            Match match = Regex.Match(fileName, pattern);

            if (!match.Success || !(match.Groups.Count > 1))
                return -1;

            string indexString = match.Groups[1].Value;
            return int.Parse(indexString);
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
                throw new DirectoryNotFoundException("Path cannot be null or empty.");

            path = path.Trim();

            if (Path.GetInvalidPathChars().Any(path.Contains))
                throw new DirectoryNotFoundException("Path contains invalid characters.");

            if (!Path.IsPathRooted(path))
                throw new DirectoryNotFoundException("Path is not an absolute path.");

            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException("Path does not exist.");
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
                        double x = (double)(value.Origin.X + (value.Dimension.X / 2)) / capture.Dimension.X;
                        double y = (double)(value.Origin.Y + (value.Dimension.Y / 2)) / capture.Dimension.Y;
                        double width = (double)value.Dimension.X / capture.Dimension.X;
                        double height = (double)value.Dimension.Y / capture.Dimension.Y;
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