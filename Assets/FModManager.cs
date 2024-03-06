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

#if UNITY_EDITOR
        fmodStudio = GetModuleHandleA("fmodstudioL");
        resonanceAudio = GetModuleHandleA("resonanceAudio");
#else
        string fmodStudioFileDest = ExportDLL("fmodstudio");
        string resonanceAudioFileDest = ExportDLL("resonanceaudio");

        fmodStudio = NativeLibraryLoader.Load(fmodStudioFileDest);
        resonanceAudio = NativeLibraryLoader.Load(resonanceAudioFileDest);
#endif
        var camera = GameObject.FindObjectsOfType<Camera>().FirstOrDefault(c => c.name == "tk2dCamera");

        if (camera != null)
        {
            if (camera.GetComponent<StudioListener>() == null)
            {
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

    static string ExportDLL(string resourceName)
    {
        string fileName = resourceName;
        string ext = "";
        switch (NativeLibraryLoader.GetCurrentOS())
        {
            case NativeLibraryLoader.OS.Windows:
                resourceName = resourceName + ".windows";
                ext = ".dll";
                break;
            case NativeLibraryLoader.OS.Mac:
                resourceName = resourceName + ".mac";
                ext = ".dylib";
                break;
            case NativeLibraryLoader.OS.Linux:
                resourceName = resourceName + ".linux";
                ext = ".so";
                break;
            default:
                break;
        }

        var tempDirectory = PathUtilities.AddSlash(new DirectoryInfo(System.IO.Path.GetTempPath()).FullName);

        var fileDest = tempDirectory + fileName + ext;

        if (File.Exists(fileDest))
        {
            File.Delete(fileDest);
        }

        using (var fileStream = File.Create(fileDest))
        {
            if (!ResourceUtilities.Retrieve(resourceName, fileStream, typeof(AncientAspidMod).Assembly))
            {
                return null;
            }
        }

        return fileDest;
    }

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
