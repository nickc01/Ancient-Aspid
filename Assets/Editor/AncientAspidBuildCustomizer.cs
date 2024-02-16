using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using WeaverBuildTools.Commands;
using WeaverCore.Editor.Compilation;
using WeaverCore.Editor.Settings;
using WeaverCore.Utilities;

public class AncientAspidBuildCustomizer : BuildPipelineCustomizer
{
    public override Task<bool> OnAfterBuildFinished()
    {
        var modBuildFolder = BuildTools.GetModBuildFolder();
        var modName = BuildScreen.BuildSettings.ModName;
        var outputDLL = modBuildFolder + modName + "\\" + modName + ".dll";

        var secretFile = new FileInfo("Assets\\secret.7z");

        EmbedResourceCMD.EmbedResource(outputDLL, secretFile.FullName, "secret.7z", compression: WeaverBuildTools.Enums.CompressionMethod.NoCompression);

        return Task.FromResult(true);
    }
}
