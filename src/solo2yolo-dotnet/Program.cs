namespace z3lx.solo2yolo
{
    internal static class Program
    {
        private const string MissingSoloPathErrorMessage = "Missing SOLO path for -i flag.";
        private const string MissingYoloPathErrorMessage = "Missing YOLO path for -o flag.";
        private const string MissingTaskTypeErrorMessage = "Missing task type for -t flag.";
        private const string InvalidTaskTypeErrorMessage = "Invalid task type for -t flag: {0}";
        private const string UnknownFlagErrorMessage = "Unknown flag: {0}";
        private const string UnknownArgumentErrorMessage = "Unknown argument: {0}";

        private static string soloPath = string.Empty;
        private static string yoloPath = string.Empty;
        private static ComputerVisionTask? task = null;
        private static bool unityEditor = false;

        private static HashSet<string> errorMessages = new HashSet<string>();

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                ShowHelp();
                return;
            }

            ParseArguments(args);

            if (!ValidateArguments())
                return;

            DatasetConverter.Convert(soloPath, yoloPath, task.Value);

            if (unityEditor)
            {
                Console.WriteLine("\nPress any key to close this window...");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Parses the command-line arguments and assigns values to corresponding variables.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        private static void ParseArguments(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string argument = args[i];

                if (argument.StartsWith("--"))
                {
                    string flag = argument.Substring(2);

                    switch (flag)
                    {
                        case "unity-editor":
                            unityEditor = true;
                            break;
                    }
                }
                else if (argument.StartsWith("-"))
                {
                    string flag = argument.Substring(1);

                    switch (flag)
                    {
                        case "i":
                            if (TryGetNextArgumentValue(args, i, out string soloPathValue))
                            {
                                soloPath = soloPathValue;
                                i++;
                            }
                            else
                            {
                                errorMessages.Add(MissingSoloPathErrorMessage);
                            }
                            break;

                        case "o":
                            if (TryGetNextArgumentValue(args, i, out string yoloPathValue))
                            {
                                yoloPath = yoloPathValue;
                                i++;
                            }
                            else
                            {
                                errorMessages.Add(MissingYoloPathErrorMessage);
                            }
                            break;

                        case "t":
                            if (TryGetNextArgumentValue(args, i, out string taskValue))
                            {
                                if (TryParseComputerVisionTask(taskValue, out ComputerVisionTask parsedTask))
                                {
                                    task = parsedTask;
                                }
                                else
                                {
                                    errorMessages.Add(string.Format(InvalidTaskTypeErrorMessage, taskValue));
                                }
                                i++;
                            }
                            else
                            {
                                errorMessages.Add(MissingTaskTypeErrorMessage);
                            }
                            break;

                        case "h":
                            ShowHelp();
                            Environment.Exit(0);
                            break;

                        default:
                            errorMessages.Add(string.Format(UnknownFlagErrorMessage, flag));
                            break;
                    }
                }
                else
                {
                    errorMessages.Add(string.Format(UnknownArgumentErrorMessage, argument));
                }
            }
        }

        /// <summary>
        /// Tries to retrieve the value of the next argument from the command-line arguments array.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <param name="currentIndex">The current index in the arguments array.</param>
        /// <param name="value">The retrieved argument value.</param>
        /// <returns><c>true</c> if the value was successfully retrieved; otherwise, <c>false</c>.</returns>
        private static bool TryGetNextArgumentValue(string[] args, int currentIndex, out string value)
        {
            value = string.Empty;

            if (currentIndex + 1 < args.Length && !args[currentIndex + 1].StartsWith("-"))
            {
                value = args[currentIndex + 1];
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to parse the given task value to a <see cref="ComputerVisionTask"/> enum value.
        /// </summary>
        /// <param name="taskValue">The task value to parse.</param>
        /// <param name="parsedTask">The parsed <see cref="ComputerVisionTask"/> value.</param>
        /// <returns><c>true</c> if the parsing was successful; otherwise, <c>false</c>.</returns>
        private static bool TryParseComputerVisionTask(string taskValue, out ComputerVisionTask parsedTask)
        {
            return Enum.TryParse(char.ToUpper(taskValue[0]) + taskValue.Substring(1).ToLower(), out parsedTask);
        }

        /// <summary>
        /// Validates the parsed arguments.
        /// </summary>
        /// <returns><c>true</c> if the arguments are valid; otherwise, <c>false</c>.</returns>
        private static bool ValidateArguments()
        {
            if (string.IsNullOrEmpty(soloPath))
                errorMessages.Add(MissingSoloPathErrorMessage);

            if (string.IsNullOrEmpty(yoloPath))
                errorMessages.Add(MissingYoloPathErrorMessage);

            if (task == null)
                errorMessages.Add(MissingTaskTypeErrorMessage);

            if (errorMessages.Count > 0)
            {
                foreach (string errorMessage in errorMessages)
                    Console.WriteLine(errorMessage);

                return false;
            }

            return true;
        }

        /// <summary>
        /// Displays the help page with usage information.
        /// </summary>
        private static void ShowHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("solo2yolo [-i <input_path>] [-o <output_path>] [-t <task_type>]");

            Console.WriteLine("\nExample:");
            Console.WriteLine("solo2yolo -i /path/to/solo_dataset -o /path/to/yolo_dataset -t detect");

            Console.WriteLine("\nMandatory Flags:");
            Console.WriteLine("-i: Specifies the input path for the SOLO dataset.");
            Console.WriteLine("-o: Specifies the output path for the converted YOLO dataset.");
            Console.WriteLine("-t: Specifies the computer vision task of the converted dataset.");
            Console.WriteLine("    Available options: classify, detect, segment, pose.");

            Console.WriteLine("\nOther Flags:");
            Console.WriteLine("-h: Displays this help page.");
        }
    }
}