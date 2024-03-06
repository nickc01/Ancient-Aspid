using System.Collections.Generic;
using System.IO;
using UnityEngine;
using WeaverCore.Attributes;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_STANDALONE_OSX && !UNITY_EDITOR
namespace FMOD
{
    public partial class VERSION
    {
        public const string dll = "fmodstudio" + dllSuffix;
    }
}

namespace FMOD.Studio
{
    public partial class STUDIO_VERSION
    {
        public const string dll = "fmodstudio" + dllSuffix;
    }
}
#endif

namespace FMODUnity
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class PlatformMac : Platform
    {
        [OnRuntimeInit(-99998)]
        static void AddPlatform()
        {
            Settings.AddPlatformTemplate<PlatformMac>("52eb9df5db46521439908db3a29a1bbb");
        }

        public override string DisplayName { get { return "macOS"; } }
        public override void DeclareRuntimePlatforms(Settings settings)
        {
            settings.DeclareRuntimePlatform(RuntimePlatform.OSXPlayer, this);
        }

#if UNITY_EDITOR
        public override IEnumerable<BuildTarget> GetBuildTargets()
        {
            yield return BuildTarget.StandaloneOSX;
        }

        public override Legacy.Platform LegacyIdentifier { get { return Legacy.Platform.Mac; } }

        protected override BinaryAssetFolderInfo GetBinaryAssetFolder(BuildTarget buildTarget)
        {
            return new BinaryAssetFolderInfo("mac", "Plugins");
        }

        protected override IEnumerable<FileRecord> GetBinaryFiles(BuildTarget buildTarget, bool allVariants, string suffix)
        {
            yield return new FileRecord(string.Format("fmodstudio{0}.bundle", suffix));
        }

        protected override IEnumerable<FileRecord> GetOptionalBinaryFiles(BuildTarget buildTarget, bool allVariants)
        {
            yield return new FileRecord("gvraudio.bundle");
            yield return new FileRecord("resonanceaudio.bundle");
        }

        public override bool SupportsAdditionalCPP(BuildTarget target)
        {
            return false;
        }
#endif

        public override string GetPluginPath(string pluginName)
        {
            string pluginPath = string.Format("{0}/{1}.bundle", GetPluginBasePath(), pluginName);
            if (System.IO.Directory.Exists((pluginPath)))
            {
                return pluginPath;
            }
            else
            {
                return string.Format("{0}/{1}.dylib", GetPluginBasePath(), pluginName);
            }
        }
#if UNITY_EDITOR
        public override OutputType[] ValidOutputTypes
        {
            get
            {
                return sValidOutputTypes;
            }
        }

        private static OutputType[] sValidOutputTypes = {
           new OutputType() { displayName = "Core Audio", outputType = FMOD.OUTPUTTYPE.COREAUDIO },
        };
#endif

        public override List<CodecChannelCount> DefaultCodecChannels { get { return staticCodecChannels; } }

        private static List<CodecChannelCount> staticCodecChannels = new List<CodecChannelCount>()
        {
            new CodecChannelCount { format = CodecType.FADPCM, channels = 0 },
            new CodecChannelCount { format = CodecType.Vorbis, channels = 32 },
        };
    }
}
