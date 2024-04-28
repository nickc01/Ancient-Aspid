using FMOD.Studio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using WeaverBuildTools.Commands;
using WeaverCore;
using WeaverCore.Editor.Compilation;
using WeaverCore.Editor.Settings;
using WeaverCore.Utilities;

public class AncientAspidBuildCustomizer : BuildPipelineCustomizer
{
    public override Task<bool> OnAfterBuildFinished()
    {
        try
        {
            var modBuildFolder = BuildTools.GetModBuildFolder();
            var modName = BuildScreen.BuildSettings.ModName;
            var outputDLL = modBuildFolder + modName + Path.DirectorySeparatorChar + modName + ".dll";

            var secretFile = new FileInfo($"Assets{Path.DirectorySeparatorChar}secret.7z");

            EmbedResourceCMD.EmbedResource(outputDLL, secretFile.FullName, "secret.7z", compression: WeaverBuildTools.Enums.CompressionMethod.NoCompression);

            foreach (var bank in FModManager.FindAllBankPaths())
            {
                EmbedResourceCMD.EmbedResource(outputDLL, bank.FullName, bank.Name, compression: WeaverBuildTools.Enums.CompressionMethod.NoCompression);
            }

            Dictionary<NativeLibraryLoader.OS, List<string>> dllFolders = new Dictionary<NativeLibraryLoader.OS, List<string>>
        {
            {NativeLibraryLoader.OS.Windows, new List<string>() {
                "Assets/FMOD/platforms/win/lib/x86_64/fmodstudio.dll",
                //"Assets/FMOD/platforms/win/lib/x86_64/fmodstudioL.dll",
                "Assets/FMOD/platforms/win/lib/x86_64/resonanceaudio.dll"
            }},
            {NativeLibraryLoader.OS.Mac, new List<string>() {
                //"Assets/FMOD/platforms/mac/lib/fmodstudio.bundle/Contents/MacOS/fmodstudio",
                //"Assets/FMOD/platforms/mac/lib/fmodstudioL.bundle/Contents/MacOS/fmodstudioL",
                //"Assets/FMOD/platforms/mac/lib/resonanceaudio.bundle/Contents/MacOS/resonanceaudio"
                "Assets/FMOD/platforms/mac/lib/fmodstudio.bundle.7z",
                "Assets/FMOD/platforms/mac/lib/resonanceaudio.bundle.7z",
            } },
            {NativeLibraryLoader.OS.Linux, new List<string>() {
                "Assets/FMOD/platforms/linux/lib/x86_64/libfmodstudio.so",
                //"Assets/FMOD/platforms/linux/lib/x86_64/libfmodstudioL.so",
                "Assets/FMOD/platforms/linux/lib/x86_64/libresonanceaudio.so"
            } }
        };

            static string GetFullExtension(string fileName)
            {
                return fileName.Substring(fileName.IndexOf('.'));
            }

            static string FilterFileName(string fileName)
            {
                if (fileName.Contains("resonanceaudio"))
                {
                    return "resonanceaudio";
                }
                else if (fileName.Contains("fmodstudioL"))
                {
                    return "fmodstudioL";
                }
                else if (fileName.Contains("fmodstudio"))
                {
                    return "fmodstudio";
                }

                return "";
            }

            foreach (var osPair in dllFolders)
            {
                foreach (var file in osPair.Value)
                {
                    var fileInfo = new FileInfo(file);
                    EmbedResourceCMD.EmbedResource(outputDLL, fileInfo.FullName, $"{FilterFileName(fileInfo.Name)}.{osPair.Key.ToString().ToLower()}{GetFullExtension(fileInfo.Name)}", compression: WeaverBuildTools.Enums.CompressionMethod.NoCompression);
                }
            }

            return Task.FromResult(true);
        }
        catch (Exception e)
        {
            WeaverLog.LogException(e);
        }


        return Task.FromResult(false);
    }
}
