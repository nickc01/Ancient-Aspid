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
    [DllImport("kernel32.dll")]
    static extern IntPtr GetModuleHandleA(string moduleName);

    static bool banksLoaded = false;

    static IntPtr fmodStudio;
    public static IntPtr FmodStudio
    {
        get
        {
            if (fmodStudio == default)
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
            if (resonanceAudio == default)
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
    [OnInit(-99999)]
#endif
    static void LoadFMODDLLS()
    {
        if (fmodStudio != default)
        {
            return;
        }

        UnityEngine.Debug.Log("LOading FMOD DLLS");

#if UNITY_EDITOR
        fmodStudio = GetModuleHandleA("fmodstudioL");
        resonanceAudio = GetModuleHandleA("resonanceAudio");
#else
        string fmodStudioFileDest = NativeLibraryLoader.ExportDLL("fmodstudio", typeof(AncientAspidMod).Assembly);
        string resonanceAudioFileDest = NativeLibraryLoader.ExportDLL("resonanceaudio", typeof(AncientAspidMod).Assembly);

        if (NativeLibraryLoader.GetCurrentOS() != NativeLibraryLoader.OS.Mac)
        {
            UnityEngine.Debug.Log("DOING MAC EXPORT TEST");
            var result = NativeLibraryLoader.ExportDLL("fmodstudio", typeof(AncientAspid).Assembly, NativeLibraryLoader.OS.Mac);
            UnityEngine.Debug.Log("MAC EXPORT RESULT = " + result);
        }

        UnityEngine.Debug.Log("Loading FMOD part 1");
        fmodStudio = NativeLibraryLoader.Load(fmodStudioFileDest);

        UnityEngine.Debug.Log("Loading FMOD part 2");
        resonanceAudio = NativeLibraryLoader.Load(resonanceAudioFileDest);
        UnityEngine.Debug.Log("Loading FMOD done");
#endif
        var camera = GameObject.FindObjectsOfType<Camera>().FirstOrDefault(c => c.name == "tk2dCamera");

        UnityEngine.Debug.Log("Found Camera = " + camera);

        if (camera != null)
        {
            UnityEngine.Debug.Log("Already Added Studio Listener = " + camera.GetComponent<StudioListener>());
            if (camera.GetComponent<StudioListener>() == null)
            {
                UnityEngine.Debug.Log("Adding new Studio Listener");
                camera.gameObject.AddComponent<StudioListener>();
            }
        }
    }

    [WeaverCore.Attributes.AfterCameraLoad]
    static void OnCameraLoad()
    {
        if (WeaverCamera.Instance.GetComponent<StudioListener>() == null)
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
