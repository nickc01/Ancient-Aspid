using System.Runtime.InteropServices;
using System;
using WeaverCore;
using FMODUnity;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Utilities;
using FMOD;
using System.Linq;
using pdj.tiny7z.Archive;

public static class FModManager
{
    public static bool FMOD_DISABLED => NativeLibraryLoader.GetCurrentOS() == NativeLibraryLoader.OS.Mac;

    [DllImport("kernel32.dll")]
    static extern IntPtr GetModuleHandleA(string moduleName);

    static bool banksLoaded = false;

    static IntPtr fmodStudio;
    public static IntPtr FmodStudio
    {
        get
        {
            if (!FMOD_DISABLED && fmodStudio == default)
            {
                LoadFMODDLLS();
            }
            return fmodStudio;
        }
    }

    static IntPtr resonanceAudio;
    public static IntPtr ResonanceAudio
    {
        get
        {
            if (!FMOD_DISABLED && resonanceAudio == default)
            {
                LoadFMODDLLS();
            }

            return resonanceAudio;
        }
    }

    //public static Settings Settings { get; private set; }

    public static T GetProcInFModStudio<T>(string symbol) where T : Delegate => GetProc<T>(FmodStudio, symbol);

    public static T GetProcInResonanceAudio<T>(string symbol) where T : Delegate => GetProc<T>(ResonanceAudio, symbol);

    static T GetProc<T>(IntPtr handle, string symbol) where T : Delegate
    {
        if (FMOD_DISABLED)
        {
            return default;
        }
        var method_handle = NativeLibraryLoader.GetSymbol(handle, symbol);

        if (method_handle == default)
        {
            throw new Exception($"Couldn't find symbol {symbol}");
        }

        return (T)Marshal.GetDelegateForFunctionPointer(
        method_handle,
        typeof(T));
    }

    [OnRuntimeInit]
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void LoadFMODBanks()
    {
        if (banksLoaded)
        {
            return;
        }
        banksLoaded = true;

        LoadFMODDLLS();

        foreach (var foundBank in FindAllBanks())
        {
            LoadBank(foundBank);
        }
    }

#if UNITY_EDITOR
    sealed class DllAlloc : IDisposable
    {
        public readonly IntPtr LoadedDLL;

        public DllAlloc(string dllToAlloc)
        {
            LoadedDLL = NativeLibraryLoader.Load(dllToAlloc);
        }

        private void Dispose(bool disposing)
        {
            if (LoadedDLL != default)
            {
                NativeLibraryLoader.Free(LoadedDLL);
            }
        }

         ~DllAlloc()
         {
             Dispose(disposing: false);
         }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    static DllAlloc fmodStudioLoader;
    static DllAlloc resonanceAudioLoader;
#endif

#if UNITY_EDITOR
    [OnInit(-99999)]
#endif
    static void LoadFMODDLLS()
    {
        if (fmodStudio != default)
        {
            return;
        }

        //UnityEngine.Debug.Log("LOading FMOD DLLS");
        

#if UNITY_EDITOR
        if (NativeLibraryLoader.GetCurrentOS() == NativeLibraryLoader.OS.Windows) {
            fmodStudio = NativeLibraryLoader.GetLoadedHandle("fmodstudioL");
            resonanceAudio = NativeLibraryLoader.GetLoadedHandle("resonanceAudio");
        }
        else {
            var file = new DirectoryInfo("Assets").EnumerateFiles("libfmodstudioL.so", SearchOption.AllDirectories).FirstOrDefault();

            if (file == null)
            {
                WeaverLog.LogError("Failed to find fmodstudioL.so");
                return;
            }
            var assetsFolder = new DirectoryInfo("Assets").AddSlash();
            var folder = file.Directory.AddSlash();
            fmodStudioLoader = new DllAlloc(assetsFolder + "FMOD/platforms/linux/lib/x86_64/libfmodstudioL.so");
            fmodStudio = fmodStudioLoader.LoadedDLL;
            resonanceAudioLoader = new DllAlloc(assetsFolder + "FMOD/platforms/linux/lib/x86_64/libresonanceaudio.so");
            resonanceAudio = resonanceAudioLoader.LoadedDLL;
        }
#else
        string fmodStudioFileDest = NativeLibraryLoader.ExportDLL("fmodstudio", typeof(AncientAspidMod).Assembly);
        string resonanceAudioFileDest = NativeLibraryLoader.ExportDLL("resonanceaudio", typeof(AncientAspidMod).Assembly);

        if (!FMOD_DISABLED)
        {
            fmodStudio = NativeLibraryLoader.Load(fmodStudioFileDest);

            resonanceAudio = NativeLibraryLoader.Load(resonanceAudioFileDest);
        }
#endif
        var camera = GameObject.FindObjectsOfType<Camera>().FirstOrDefault(c => c.name == "tk2dCamera");

        if (!FMOD_DISABLED)
        {
            if (camera != null)
            {
                if (camera.GetComponent<StudioListener>() == null)
                {
                    camera.gameObject.AddComponent<StudioListener>();
                }
            }
        }
    }

    [WeaverCore.Attributes.AfterCameraLoad]
    static void OnCameraLoad()
    {
        if (!FMOD_DISABLED && WeaverCamera.Instance.GetComponent<StudioListener>() == null)
        {
            WeaverCamera.Instance.gameObject.AddComponent<StudioListener>();
        }
    }

    /*static string ExportDLL(string resourceName)
    {
        UnityEngine.Debug.Log("Beginning Export");
        string fileName = resourceName;
        //string ext = "";
        List<string> exts = new List<string>();
        switch (NativeLibraryLoader.GetCurrentOS())
        {
            case NativeLibraryLoader.OS.Windows:
                resourceName = resourceName + ".windows";
                exts.Add(".dll");
                break;
            case NativeLibraryLoader.OS.Mac:
                resourceName = resourceName + ".mac";
                exts.Add(".dylib");
                exts.Add(".bundle.zip");
                break;
            case NativeLibraryLoader.OS.Linux:
                resourceName = resourceName + ".linux";
                exts.Add(".so");
                break;
            default:
                break;
        }

        UnityEngine.Debug.Log("Beginning Exporting Resource = " + resourceName);
        UnityEngine.Debug.Log("Beginning Exporting Extension = " + ext);

        var tempDirectory = PathUtilities.AddSlash(new DirectoryInfo(System.IO.Path.GetTempPath()).FullName);

        UnityEngine.Debug.Log("Temp Dir = " + tempDirectory);

        var fileDest = tempDirectory + fileName + ext;

        UnityEngine.Debug.Log("Export File Dest = " + fileDest);

        if (File.Exists(fileDest))
        {
            UnityEngine.Debug.Log("Deleting already existing file = " + fileDest);
            File.Delete(fileDest);
        }

        using (var fileStream = File.Create(fileDest))
        {
            if (!ResourceUtilities.Retrieve(resourceName, fileStream, typeof(AncientAspidMod).Assembly))
            {
                UnityEngine.Debug.LogError("Error: Failed to retrieve resource and export it to file");
                return null;
            }
        }

        UnityEngine.Debug.Log("Finished Exporting = " + fileDest);

        return fileDest;
    }*/

    public static IEnumerable<FileInfo> FindAllBankPaths()
    {
#if UNITY_EDITOR
        foreach (var file in new DirectoryInfo("Assets/Banks").EnumerateFiles("*.bank"))
        {
            if (!file.FullName.EndsWith(".meta"))
            {
                yield return file;
            }
        }
#else
        throw new NotSupportedException();
#endif
    }

    public static IEnumerable<string> FindAllBanks()
    {
#if UNITY_EDITOR
        foreach (var file in new DirectoryInfo("Assets/Banks").EnumerateFiles("*.bank"))
        {
            if (!file.FullName.EndsWith(".meta"))
            {
                var name = file.Name;

                if (name.EndsWith(".bank"))
                {
                    name = name.Replace(".bank", "");
                }

                yield return name;
            }
        }
#else
        return typeof(AncientAspidMod).Assembly.GetManifestResourceNames().Where(r => r.EndsWith(".bank"));
#endif
    }

    public static void LoadBank(string bankName)
    {
        if (FMOD_DISABLED)
        {
            return;
        }

        Stream stream = null;
        try
        {
#if UNITY_EDITOR
            stream = File.OpenRead($"Assets/Banks/{bankName}.bank");
#else
            stream = typeof(AncientAspidMod).Assembly.GetManifestResourceStream(bankName);
#endif
            RuntimeManager.LoadBankFile(stream, false, bankName);
        }
        catch (Exception e)
        {
            WeaverLog.LogException(e);
        }
        finally
        {
            if (stream != null)
            {
                stream.Dispose();
            }
        }
    }
}
