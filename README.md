# **solo2yolo**
solo2yolo is a tool that enables the conversion of SOLO datasets to YOLO format directly within the Unity editor. **Please note that this package is currently under development.**

## **About SOLO**
SOLO stands for Synthetic Optimized Labeled Objects. A SOLO dataset is a combination of JSON and image files. This tool utilizes this schema, which provides a generic structure for simulation output that can be easily consumed for statistical analysis or machine learning model training. 

For more information about the SOLO dataset schema, refer to the **[Unity documentation](https://docs.unity3d.com/Packages/com.unity.perception@1.0/manual/Schema/SoloSchema.html)**.

## **Compatibility**
solo2yolo has been tested with Unity HDRP 2021 LTS and Perception package version 1.0.0-preview.1. However, it should work with any Unity editor version that supports HDRP.

## **Installation**
This project requires the **[.NET 6.0 runtime library](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)** to be installed on your system.

### Using Git
Before proceeding with this method, ensure you have Git installed on your system and a version of Unity that supports path query parameters for Git packages.
1. Open Unity and navigate to **Window > Package Manager**.
2. Click on the top left button with a "+" on it to add a new package. In the dropdown, select **"Add package from git URL"**.
3. Enter the following git URL: `https://github.com/z3lx/solo2yolo.git?path=src/solo2yolo-unity/Packages/solo2yolo`

### Manual Installation
1. Clone this repository to your local machine and unzip it.
2. Open Unity and navigate to **Window > Package Manager**.
3. Click on the top left button with a "+" on it to add a new package. In the dropdown, select **"Add package from disk"**.
4. Navigate to the unzipped package, and locate the **package.json** file within the **src/solo2yolo-unity/Packages/solo2yolo** directory.

**Note: If you prefer to build the executable used in the unity package from source, you can find the necessary files in the solo2yolo-dotnet folder.**

## **Usage**
Currently, solo2yolo only supports the conversion of BoundingBox2D annotations to YOLO format for object detection tasks. However, this tool is under development, and additional computer vision task support may be added in the future. 

### Editor GUI
To use solo2yolo using the editor GUI, follow these steps:
1. Navigate to **Tools > solo2yolo Converter** in the Unity editor.
2. Select the directory where your SOLO dataset is located.
3. Choose the output directory where the converted dataset will be stored.
4. Click on the **Confirm** button to start the conversion process.

### CLI
You can also use solo2yolo from the command line using the following options:
```
Usage:
solo2yolo [-i <input_path>] [-o <output_path>] [-t <task_type>]

Example:
solo2yolo -i /path/to/solo_dataset -o /path/to/yolo_dataset -t detect

Mandatory Flags:
-i: Specifies the input path for the SOLO dataset.
-o: Specifies the output path for the converted YOLO dataset.
-t: Specifies the computer vision task of the converted dataset.
    Available options: classify, detect, segment, pose.

Other Flags:
-h: Displays the help page.
```

Please note that the conversion process may take some time, depending on the size of your dataset. Furthermore, make sure to review the output files with a dataset viewer to ensure successful conversion.

## **Contributing**
Contributions are welcome! If you encounter any issues, have suggestions, or want to contribute new features, please open an issue or submit a pull request. Your contributions help improve the tool and make it more useful for the community.

## **License**
This project is licensed under the MIT License. See **[LICENSE](https://github.com/z3lx/solo2yolo/blob/main/LICENSE)** for details. 

If you find solo2yolo helpful, please consider giving this repository a star. Your support is highly appreciated! ⭐️
