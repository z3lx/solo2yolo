using Newtonsoft.Json;
using System.Diagnostics;
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
        /// <summary>
        /// Converts a SOLO dataset to the YOLO format.
        /// </summary>
        /// <param name="soloPath">The path to the SOLO dataset.</param>
        /// <param name="outputPath">The path to store the converted YOLO dataset.</param>
        /// <param name="task">The computer vision task conversion to perform.</param>
        public static void Convert(string soloPath, string outputPath, ComputerVisionTask task)
        {
            // Validate and sanitize paths
            try
            {
                ValidateAndSanitizePath(ref soloPath);
                ValidateAndSanitizePath(ref outputPath);
            }
            catch (Exception ex)
            {
                ConsoleUtility.PrintError($"{ex.Message} Aborting.");
                return;
            }

            // Read metadata
            string metadataPath = Path.Combine(soloPath, "metadata.json");
            if (!TryDeserializeObjectFromFile(metadataPath, out Metadata metadata))
                return;

            // Read annotation definitions
            string annotationDefinitionsPath = Path.Combine(soloPath, "annotation_definitions.json");
            if (!TryDeserializeObjectFromFile(annotationDefinitionsPath, out AnnotationDefinitions annotationDefinitions))
                return;

            // Find frames
            if (!TryFindFrames(soloPath, metadata, out IEnumerable<string> frameDataPaths, out int frameCount))
                return;

            // TODO: Check for write permission
            // Create output directory
            ConsoleUtility.PrintInfo("Creating YOLO directory...");
            string yoloPath = CreateYoloDirectory(outputPath);

            // Convert dataset format
            ConsoleUtility.PrintInfo("Starting dataset format conversion...");
            Stopwatch stopwatch = Stopwatch.StartNew();

            int soloIndex = 0;
            int yoloIndex = 0;
            foreach (string frameDataPath in frameDataPaths)
            {
                soloIndex++;

                // Deserialize frame data json
                if (!TryDeserializeObjectFromFile(frameDataPath, out FrameData frameData))
                {
                    ConsoleUtility.PrintError($"Skipped frame {soloIndex} out of {frameCount}: " +
                        "could not deserialize frame data.");
                    continue;
                }

                // Check for captures
                if (frameData.Captures == null || frameData.Captures.Length == 0)
                {
                    ConsoleUtility.PrintWarning($"Skipped frame {soloIndex} out of {frameCount}: " +
                        "no reported captures.");
                    continue;
                }

                // Hardcoded RGB capture
                RgbCapture capture = frameData.Captures.OfType<RgbCapture>().FirstOrDefault();
                if (string.IsNullOrEmpty(capture.FileName) || !File.Exists(Path.Combine(Directory.GetParent(frameDataPath).FullName, capture.FileName)))
                {
                    ConsoleUtility.PrintWarning($"Skipped frame {soloIndex} out of {frameCount}: " +
                        "no associated image file.");
                    continue;
                }

                // Convert annotations for the frame depending on the task
                string labelingData = ConvertLabelingData(capture, task);
                if (string.IsNullOrEmpty(labelingData))
                {
                    ConsoleUtility.PrintWarning($"Skipped frame {soloIndex} out of {frameCount}: " +
                        "no reported labels.");
                    continue;
                }

                // Write labeling data to output
                string labelingDataPath = Path.Combine(yoloPath, "labels", $"{yoloIndex:D12}.txt");
                File.WriteAllText(labelingDataPath, labelingData);

                // Copy image to output
                string sourceImagePath = Path.Combine(Directory.GetParent(frameDataPath).FullName, capture.FileName);
                string destImagePath = Path.Combine(yoloPath, "images", $"{yoloIndex:D12}.{capture.ImageFormat.ToLower()}");
                File.Copy(sourceImagePath, destImagePath);

                ConsoleUtility.PrintInfo($"Processed frame {soloIndex} out of {frameCount}.");

                yoloIndex++;
            }

            // Create dataset.yaml
            ConsoleUtility.PrintInfo("Creating dataset.yaml...");
            AnnotationDefinition annotationDefinition = annotationDefinitions.Values[0];
            GenerateDatasetYaml(yoloPath, annotationDefinition);

            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            ConsoleUtility.PrintInfo($"Finished dataset format conversion in {ts.TotalMilliseconds:F3} ms.");
        }

        /// <summary>
        /// Tries to deserialize a JSON file into an object.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <param name="filePath">The path to the JSON file.</param>
        /// <param name="result">The deserialized object.</param>
        private static bool TryDeserializeObjectFromFile<T>(string filePath, out T result)
        {
            try
            {
                result = DeserializeObjectFromFile<T>(filePath);
                return true;
            }
            catch (Exception ex)
            {
                ConsoleUtility.PrintError($"{ex.Message} Aborting.");
                result = default;
                return false;
            }
        }

        /// <summary>
        /// Deserializes a JSON file into an object.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <param name="filePath">The path to the JSON file.</param>
        /// <returns>The deserialized object.</returns>
        private static T DeserializeObjectFromFile<T>(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File '{filePath}' not found.");

            try
            {
                string data = File.ReadAllText(filePath);
                if (string.IsNullOrEmpty(data))
                    throw new JsonSerializationException($"File '{filePath}' is empty.");

                T deserializedObject = JsonConvert.DeserializeObject<T>(data);
                if (deserializedObject == null)
                    throw new JsonSerializationException($"Failed to deserialize {typeof(T)} from the file '{filePath}'.");

                return deserializedObject;
            }
            catch (Exception ex)
            {
                throw new JsonSerializationException($"An error occurred while deserializing the file '{filePath}': {ex.Message}", ex);
            }
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
                throw new DirectoryNotFoundException($"Path '{path}' contains invalid characters.");

            if (!Path.IsPathRooted(path))
                throw new DirectoryNotFoundException($"Path '{path}' is not an absolute path.");

            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException($"Path '{path}' does not exist.");
        }

        /// <summary>
        /// Tries to find frame paths in the SOLO dataset.
        /// </summary>
        /// <param name="soloPath">The path to the SOLO dataset.</param>
        /// /// <param name="metadata">The metadata associated to the SOLO dataset.</param>
        /// <param name="frameDataPaths">The frame paths.</param>
        /// <param name="frameCount">The frame count.</param>
        /// <returns></returns>
        private static bool TryFindFrames(string soloPath, Metadata metadata, out IEnumerable<string> frameDataPaths, out int frameCount)
        {
            ConsoleUtility.PrintInfo("Finding frame(s)...");
            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                frameDataPaths = FindFrames(soloPath);
                frameCount = frameDataPaths.Count();
            }
            catch (Exception ex)
            {
                ConsoleUtility.PrintError($"{ex.Message} Aborting.");
                frameDataPaths = default;
                frameCount = -1;
                return false;
            }

            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;

            if (frameCount == metadata.TotalFrames)
            {
                ConsoleUtility.PrintInfo(string.Format("Found {0} frame{1} in {2:F3} ms.",
                    frameCount, frameCount == 1 ? "" : "s", ts.TotalMilliseconds));
            }
            else if (frameCount == 0)
            {
                ConsoleUtility.PrintError($"Did not find any frames in '{soloPath}'. Aborting.");
                return false;
            }
            else
            {
                ConsoleUtility.PrintWarning(string.Format("Found {0} frame{1} in {2:F3} ms, expected {3}.",
                    frameCount, frameCount == 1 ? "" : "s", ts.TotalMilliseconds, metadata.TotalFrames));
            }

            return true;
        }

        /// <summary>
        /// Finds frame paths in the SOLO dataset.
        /// </summary>
        /// <param name="soloPath">The path to the SOLO dataset.</param>
        /// <returns>The frame paths.</returns>
        private static IEnumerable<string> FindFrames(string soloPath)
        {
            try
            {
                string sequencePattern = "sequence.*";
                IEnumerable<string> sequencePaths = Directory.EnumerateDirectories(soloPath, sequencePattern, SearchOption.TopDirectoryOnly)
                    .OrderBy(sequencePath => GetIndexFromPath(sequencePath, sequencePattern));

                string framePattern = "step*.frame_data.json";
                IEnumerable<string> frameDataPaths = sequencePaths
                    .SelectMany(sequencePath => Directory.EnumerateFiles(sequencePath, framePattern, SearchOption.TopDirectoryOnly)
                    .OrderBy(frameDataPath => GetIndexFromPath(frameDataPath, framePattern)));

                return frameDataPaths;
            }
            catch (Exception ex)
            {
                throw new IOException($"An error occurred while finding frames in {soloPath}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Extracts the index from a given path using the provided format.
        /// </summary>
        /// <param name="path">The path to get the index from.</param>
        /// <param name="format">The path format.</param>
        /// <returns>The extracted index.</returns>
        private static int GetIndexFromPath(string path, string format)
        {
            string fileName = Path.GetFileName(path);
            string pattern = format.Replace("*", "(\\d+)");
            Match match = Regex.Match(fileName, pattern);

            if (!match.Success || !(match.Groups.Count > 1))
                return -1;

            string indexString = match.Groups[1].Value;
            return int.Parse(indexString);
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
                    throw new ArgumentOutOfRangeException(nameof(task), task, "Unsupported task type. ");
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