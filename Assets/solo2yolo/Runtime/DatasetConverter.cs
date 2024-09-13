using Newtonsoft.Json;
using System;
using System.IO;
using Unity.Properties;
using UnityEngine;
using z3lx.solo2yolo.Deserialization.DataModels;

namespace z3lx.solo2yolo
{
    public static class DatasetConverter
    {
        public static void Convert(string soloPath, string outPath, ComputerVisionTask task, bool log = false)
        {
            // TODO: Add validations for solo and yolo paths
            // TODO: Add image dimension verification
            soloPath = soloPath.Replace('/', '\\');
            outPath = outPath.Replace('/', '\\');

            if (soloPath.EndsWith(@"\"))
                soloPath = soloPath.TrimEnd('\\');
            if (outPath.EndsWith(@"\"))
                outPath = outPath.TrimEnd('\\');

            if (!Directory.Exists(soloPath))
                throw new InvalidPathException("Invalid SOLO path.");

            // Create new YOLO directory and cache its path
            (string parent, string images, string labels) yoloPath = CreateYoloDirectory(outPath);

            // Find and deserialize metadata
            // TODO: Add validations
            Metadata metadata = GetMetadata(soloPath);

            // Iterate through all sequences and frames
            int totalFrameCount = 0; // labeled frames
            int sequenceCount = 0;
            for (int i = 0; sequenceCount < metadata.TotalSequences; i++)
            {
                string sequencePath = $@"{soloPath}\sequence.{i}";
                if (!Directory.Exists(sequencePath)) continue;
                sequenceCount++;

                int frameCount = 0;
                int framesInSequence = Directory.GetFiles(sequencePath, "*.json").Length;
                for (int j = 0; frameCount < framesInSequence; j++)
                {
                    string frameDataPath = $@"{sequencePath}\step{j}.frame_data.json";
                    if (!File.Exists(frameDataPath)) continue;
                    frameCount++;

                    // TODO: Add validations
                    string frameDataContent = File.ReadAllText(frameDataPath);
                    FrameData frameData = JsonConvert.DeserializeObject<FrameData>(frameDataContent);

                    // Hardcoded RGB capture
                    RgbCapture capture = frameData.Captures[0] as RgbCapture;
                    
                    // Continue with the next frame if there isn't an image associated with the capture
                    if (string.IsNullOrEmpty(capture.FileName)) continue;

                    // Generate annotations for the frame depending on the task
                    string labelingData = "";
                    switch (task)
                    {
                        case ComputerVisionTask.Classify:
                            throw new NotImplementedException();
                        case ComputerVisionTask.Detect:
                            // Can there be multiple of the same annotation type?
                            BoundingBox2DAnnotation annotation = null;
                            foreach (Annotation e in capture.Annotations)
                            {
                                if (e is BoundingBox2DAnnotation boundingBox2DAnnotation)
                                {
                                    annotation = boundingBox2DAnnotation;
                                    break;
                                }
                            }

                            // Continue with the next frame if there isn't an annotation of type BoundingBox2DAnnotation associated with the capture
                            if (annotation == null) continue;

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
                    }

                    // Write labeling data to output
                    File.WriteAllText($@"{yoloPath.labels}\{totalFrameCount:D12}.txt", labelingData);

                    // Copy image to output
                    File.Copy($@"{sequencePath}\{capture.FileName}", $@"{yoloPath.images}\{totalFrameCount:D12}.{capture.ImageFormat.ToLower()}");

                    totalFrameCount++;
                }
            }

            // TODO: Add validations
            AnnotationDefinition annotationDefinition = GetAnnotationDefinition(soloPath).Values[0];

            // Generate dataset.yaml
            string content = "";
            content += $"path: {yoloPath.parent} # dataset root dir\n";
            content += $"train: images # train images (relative to 'path')\n";
            content += $"val: images # val images (relative to 'path')\n";
            content += $"test: #test images (optional)\n";
            content += $"\n";
            content += $"# Classes ({annotationDefinition.Specifications.Length} classes)\n";
            content += $"names:\n";
            foreach (AnnotationDefinition.Specification spec in annotationDefinition.Specifications)
                content += $"  {spec.LabelId}: {spec.labelName}\n";
            File.WriteAllText($@"{yoloPath.parent}\dataset.yaml", content);
        }

        private static (string, string, string) CreateYoloDirectory(string path)
        {
            int i = 0;
            string newYoloDirPath = $@"{path}\yolo";

            while (Directory.Exists(newYoloDirPath))
            {
                i++;
                newYoloDirPath = $@"{path}\yolo_{i}";
            }

            string parent = $@"{newYoloDirPath}";
            string images = $@"{newYoloDirPath}\images";
            string labels = $@"{newYoloDirPath}\labels";
            Directory.CreateDirectory(newYoloDirPath);
            Directory.CreateDirectory(images);
            Directory.CreateDirectory(labels);
            return (parent, images, labels);
        }

        private static Metadata GetMetadata(string soloPath)
        {
            string metadataPath = $@"{soloPath}\metadata.json";
            if (!File.Exists(metadataPath))
                throw new FileNotFoundException("Could not find metadata.json");
            string metadataContent = File.ReadAllText(metadataPath);
            return JsonConvert.DeserializeObject<Metadata>(metadataContent);
        }

        private static AnnotationDefinitions GetAnnotationDefinition(string soloPath)
        {
            string annotationDefinitionPath = $@"{soloPath}\annotation_definitions.json";
            if (!File.Exists(annotationDefinitionPath))
                throw new FileNotFoundException("Could not find annotation_definitions.json");
            string annotationDefinitionContent = File.ReadAllText(annotationDefinitionPath);
            return JsonConvert.DeserializeObject<AnnotationDefinitions>(annotationDefinitionContent);
        }
    }
}