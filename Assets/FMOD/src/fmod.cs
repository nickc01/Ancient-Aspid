/* ======================================================================================== */
/* FMOD Core API - C# wrapper.                                                              */
/* Copyright (c), Firelight Technologies Pty, Ltd. 2004-2023.                               */
/*                                                                                          */
/* For more detail visit:                                                                   */
/* https://fmod.com/docs/2.02/api/core-api.html                                             */
/* ======================================================================================== */

using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace FMOD
{
    /*
        FMOD version number.  Check this against FMOD::System::getVersion / System_GetVersion
        0xaaaabbcc -> aaaa = major version number.  bb = minor version number.  cc = development version number.
    */
    public partial class VERSION
    {
        public const int    number = 0x00020219;
#if !UNITY_2019_4_OR_NEWER
        public const string dll    = "fmod";
#endif
    }

    public class CONSTANTS
    {
        public const int MAX_CHANNEL_WIDTH = 32;
        public const int MAX_LISTENERS = 8;
        public const int REVERB_MAXINSTANCES = 4;
        public const int MAX_SYSTEMS = 8;
    }

    /*
        FMOD core types
    */
    public enum RESULT : int
    {
        OK,
        ERR_BADCOMMAND,
        ERR_CHANNEL_ALLOC,
        ERR_CHANNEL_STOLEN,
        ERR_DMA,
        ERR_DSP_CONNECTION,
        ERR_DSP_DONTPROCESS,
        ERR_DSP_FORMAT,
        ERR_DSP_INUSE,
        ERR_DSP_NOTFOUND,
        ERR_DSP_RESERVED,
        ERR_DSP_SILENCE,
        ERR_DSP_TYPE,
        ERR_FILE_BAD,
        ERR_FILE_COULDNOTSEEK,
        ERR_FILE_DISKEJECTED,
        ERR_FILE_EOF,
        ERR_FILE_ENDOFDATA,
        ERR_FILE_NOTFOUND,
        ERR_FORMAT,
        ERR_HEADER_MISMATCH,
        ERR_HTTP,
        ERR_HTTP_ACCESS,
        ERR_HTTP_PROXY_AUTH,
        ERR_HTTP_SERVER_ERROR,
        ERR_HTTP_TIMEOUT,
        ERR_INITIALIZATION,
        ERR_INITIALIZED,
        ERR_INTERNAL,
        ERR_INVALID_FLOAT,
        ERR_INVALID_HANDLE,
        ERR_INVALID_PARAM,
        ERR_INVALID_POSITION,
        ERR_INVALID_SPEAKER,
        ERR_INVALID_SYNCPOINT,
        ERR_INVALID_THREAD,
        ERR_INVALID_VECTOR,
        ERR_MAXAUDIBLE,
        ERR_MEMORY,
        ERR_MEMORY_CANTPOINT,
        ERR_NEEDS3D,
        ERR_NEEDSHARDWARE,
        ERR_NET_CONNECT,
        ERR_NET_SOCKET_ERROR,
        ERR_NET_URL,
        ERR_NET_WOULD_BLOCK,
        ERR_NOTREADY,
        ERR_OUTPUT_ALLOCATED,
        ERR_OUTPUT_CREATEBUFFER,
        ERR_OUTPUT_DRIVERCALL,
        ERR_OUTPUT_FORMAT,
        ERR_OUTPUT_INIT,
        ERR_OUTPUT_NODRIVERS,
        ERR_PLUGIN,
        ERR_PLUGIN_MISSING,
        ERR_PLUGIN_RESOURCE,
        ERR_PLUGIN_VERSION,
        ERR_RECORD,
        ERR_REVERB_CHANNELGROUP,
        ERR_REVERB_INSTANCE,
        ERR_SUBSOUNDS,
        ERR_SUBSOUND_ALLOCATED,
        ERR_SUBSOUND_CANTMOVE,
        ERR_TAGNOTFOUND,
        ERR_TOOMANYCHANNELS,
        ERR_TRUNCATED,
        ERR_UNIMPLEMENTED,
        ERR_UNINITIALIZED,
        ERR_UNSUPPORTED,
        ERR_VERSION,
        ERR_EVENT_ALREADY_LOADED,
        ERR_EVENT_LIVEUPDATE_BUSY,
        ERR_EVENT_LIVEUPDATE_MISMATCH,
        ERR_EVENT_LIVEUPDATE_TIMEOUT,
        ERR_EVENT_NOTFOUND,
        ERR_STUDIO_UNINITIALIZED,
        ERR_STUDIO_NOT_LOADED,
        ERR_INVALID_STRING,
        ERR_ALREADY_LOCKED,
        ERR_NOT_LOCKED,
        ERR_RECORD_DISCONNECTED,
        ERR_TOOMANYSAMPLES,
    }

    public enum CHANNELCONTROL_TYPE : int
    {
        CHANNEL,
        CHANNELGROUP,
        MAX
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct VECTOR
    {
        public float x;
        public float y;
        public float z;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ATTRIBUTES_3D
    {
        public VECTOR position;
        public VECTOR velocity;
        public VECTOR forward;
        public VECTOR up;
    }

    [StructLayout(LayoutKind.Sequential)]
    public partial struct GUID
    {
        public int Data1;
        public int Data2;
        public int Data3;
        public int Data4;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ASYNCREADINFO
    {
        public IntPtr   handle;
        public uint     offset;
        public uint     sizebytes;
        public int      priority;

        public IntPtr   userdata;
        public IntPtr   buffer;
        public uint     bytesread;
        public FILE_ASYNCDONE_FUNC done;
    }

    public enum OUTPUTTYPE : int
    {
        AUTODETECT,

        UNKNOWN,
        NOSOUND,
        WAVWRITER,
        NOSOUND_NRT,
        WAVWRITER_NRT,

        WASAPI,
        ASIO,
        PULSEAUDIO,
        ALSA,
        COREAUDIO,
        AUDIOTRACK,
        OPENSL,
        AUDIOOUT,
        AUDIO3D,
        WEBAUDIO,
        NNAUDIO,
        WINSONIC,
        AAUDIO,
        AUDIOWORKLET,
        PHASE,
        OHAUDIO,

        MAX,
    }

    public enum PORT_TYPE : int
    {
        MUSIC,
        COPYRIGHT_MUSIC,
        VOICE,
        CONTROLLER,
        PERSONAL,
        VIBRATION,
        AUX,

        MAX
    }

    public enum DEBUG_MODE : int
    {
        TTY,
        FILE,
        CALLBACK,
    }

    [Flags]
    public enum DEBUG_FLAGS : uint
    {
        NONE                    = 0x00000000,
        ERROR                   = 0x00000001,
        WARNING                 = 0x00000002,
        LOG                     = 0x00000004,

        TYPE_MEMORY             = 0x00000100,
        TYPE_FILE               = 0x00000200,
        TYPE_CODEC              = 0x00000400,
        TYPE_TRACE              = 0x00000800,

        DISPLAY_TIMESTAMPS      = 0x00010000,
        DISPLAY_LINENUMBERS     = 0x00020000,
        DISPLAY_THREAD          = 0x00040000,
    }

    [Flags]
    public enum MEMORY_TYPE : uint
    {
        NORMAL                  = 0x00000000,
        STREAM_FILE             = 0x00000001,
        STREAM_DECODE           = 0x00000002,
        SAMPLEDATA              = 0x00000004,
        DSP_BUFFER              = 0x00000008,
        PLUGIN                  = 0x00000010,
        PERSISTENT              = 0x00200000,
        ALL                     = 0xFFFFFFFF
    }

    public enum SPEAKERMODE : int
    {
        DEFAULT,
        RAW,
        MONO,
        STEREO,
        QUAD,
        SURROUND,
        _5POINT1,
        _7POINT1,
        _7POINT1POINT4,

        MAX,
    }

    public enum SPEAKER : int
    {
        NONE = -1,
        FRONT_LEFT,
        FRONT_RIGHT,
        FRONT_CENTER,
        LOW_FREQUENCY,
        SURROUND_LEFT,
        SURROUND_RIGHT,
        BACK_LEFT,
        BACK_RIGHT,
        TOP_FRONT_LEFT,
        TOP_FRONT_RIGHT,
        TOP_BACK_LEFT,
        TOP_BACK_RIGHT,

        MAX,
    }

    [Flags]
    public enum CHANNELMASK : uint
    {
        FRONT_LEFT             = 0x00000001,
        FRONT_RIGHT            = 0x00000002,
        FRONT_CENTER           = 0x00000004,
        LOW_FREQUENCY          = 0x00000008,
        SURROUND_LEFT          = 0x00000010,
        SURROUND_RIGHT         = 0x00000020,
        BACK_LEFT              = 0x00000040,
        BACK_RIGHT             = 0x00000080,
        BACK_CENTER            = 0x00000100,

        MONO                   = (FRONT_LEFT),
        STEREO                 = (FRONT_LEFT | FRONT_RIGHT),
        LRC                    = (FRONT_LEFT | FRONT_RIGHT | FRONT_CENTER),
        QUAD                   = (FRONT_LEFT | FRONT_RIGHT | SURROUND_LEFT | SURROUND_RIGHT),
        SURROUND               = (FRONT_LEFT | FRONT_RIGHT | FRONT_CENTER | SURROUND_LEFT | SURROUND_RIGHT),
        _5POINT1               = (FRONT_LEFT | FRONT_RIGHT | FRONT_CENTER | LOW_FREQUENCY | SURROUND_LEFT | SURROUND_RIGHT),
        _5POINT1_REARS         = (FRONT_LEFT | FRONT_RIGHT | FRONT_CENTER | LOW_FREQUENCY | BACK_LEFT | BACK_RIGHT),
        _7POINT0               = (FRONT_LEFT | FRONT_RIGHT | FRONT_CENTER | SURROUND_LEFT | SURROUND_RIGHT | BACK_LEFT | BACK_RIGHT),
        _7POINT1               = (FRONT_LEFT | FRONT_RIGHT | FRONT_CENTER | LOW_FREQUENCY | SURROUND_LEFT | SURROUND_RIGHT | BACK_LEFT | BACK_RIGHT)
    }

    public enum CHANNELORDER : int
    {
        DEFAULT,
        WAVEFORMAT,
        PROTOOLS,
        ALLMONO,
        ALLSTEREO,
        ALSA,

        MAX,
    }

    public enum PLUGINTYPE : int
    {
        OUTPUT,
        CODEC,
        DSP,

        MAX,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PLUGINLIST
    {
        PLUGINTYPE type;
        IntPtr description;
    }

    [Flags]
    public enum INITFLAGS : uint
    {
        NORMAL                     = 0x00000000,
        STREAM_FROM_UPDATE         = 0x00000001,
        MIX_FROM_UPDATE            = 0x00000002,
        _3D_RIGHTHANDED            = 0x00000004,
        CLIP_OUTPUT                = 0x00000008,
        CHANNEL_LOWPASS            = 0x00000100,
        CHANNEL_DISTANCEFILTER     = 0x00000200,
        PROFILE_ENABLE             = 0x00010000,
        VOL0_BECOMES_VIRTUAL       = 0x00020000,
        GEOMETRY_USECLOSEST        = 0x00040000,
        PREFER_DOLBY_DOWNMIX       = 0x00080000,
        THREAD_UNSAFE              = 0x00100000,
        PROFILE_METER_ALL          = 0x00200000,
        MEMORY_TRACKING            = 0x00400000,
    }

    public enum SOUND_TYPE : int
    {
        UNKNOWN,
        AIFF,
        ASF,
        DLS,
        FLAC,
        FSB,
        IT,
        MIDI,
        MOD,
        MPEG,
        OGGVORBIS,
        PLAYLIST,
        RAW,
        S3M,
        USER,
        WAV,
        XM,
        XMA,
        AUDIOQUEUE,
        AT9,
        VORBIS,
        MEDIA_FOUNDATION,
        MEDIACODEC,
        FADPCM,
        OPUS,

        MAX,
    }

    public enum SOUND_FORMAT : int
    {
        NONE,
        PCM8,
        PCM16,
        PCM24,
        PCM32,
        PCMFLOAT,
        BITSTREAM,

        MAX
    }

    [Flags]
    public enum MODE : uint
    {
        DEFAULT                     = 0x00000000,
        LOOP_OFF                    = 0x00000001,
        LOOP_NORMAL                 = 0x00000002,
        LOOP_BIDI                   = 0x00000004,
        _2D                         = 0x00000008,
        _3D                         = 0x00000010,
        CREATESTREAM                = 0x00000080,
        CREATESAMPLE                = 0x00000100,
        CREATECOMPRESSEDSAMPLE      = 0x00000200,
        OPENUSER                    = 0x00000400,
        OPENMEMORY                  = 0x00000800,
        OPENMEMORY_POINT            = 0x10000000,
        OPENRAW                     = 0x00001000,
        OPENONLY                    = 0x00002000,
        ACCURATETIME                = 0x00004000,
        MPEGSEARCH                  = 0x00008000,
        NONBLOCKING                 = 0x00010000,
        UNIQUE                      = 0x00020000,
        _3D_HEADRELATIVE            = 0x00040000,
        _3D_WORLDRELATIVE           = 0x00080000,
        _3D_INVERSEROLLOFF          = 0x00100000,
        _3D_LINEARROLLOFF           = 0x00200000,
        _3D_LINEARSQUAREROLLOFF     = 0x00400000,
        _3D_INVERSETAPEREDROLLOFF   = 0x00800000,
        _3D_CUSTOMROLLOFF           = 0x04000000,
        _3D_IGNOREGEOMETRY          = 0x40000000,
        IGNORETAGS                  = 0x02000000,
        LOWMEM                      = 0x08000000,
        VIRTUAL_PLAYFROMSTART       = 0x80000000
    }

    public enum OPENSTATE : int
    {
        READY = 0,
        LOADING,
        ERROR,
        CONNECTING,
        BUFFERING,
        SEEKING,
        PLAYING,
        SETPOSITION,

        MAX,
    }

    public enum SOUNDGROUP_BEHAVIOR : int
    {
        BEHAVIOR_FAIL,
        BEHAVIOR_MUTE,
        BEHAVIOR_STEALLOWEST,

        MAX,
    }

    public enum CHANNELCONTROL_CALLBACK_TYPE : int
    {
        END,
        VIRTUALVOICE,
        SYNCPOINT,
        OCCLUSION,

        MAX,
    }

    public struct CHANNELCONTROL_DSP_INDEX
    {
        public const int HEAD    = -1;
        public const int FADER   = -2;
        public const int TAIL    = -3;
    }

    public enum ERRORCALLBACK_INSTANCETYPE : int
    {
        NONE,
        SYSTEM,
        CHANNEL,
        CHANNELGROUP,
        CHANNELCONTROL,
        SOUND,
        SOUNDGROUP,
        DSP,
        DSPCONNECTION,
        GEOMETRY,
        REVERB3D,
        STUDIO_SYSTEM,
        STUDIO_EVENTDESCRIPTION,
        STUDIO_EVENTINSTANCE,
        STUDIO_PARAMETERINSTANCE,
        STUDIO_BUS,
        STUDIO_VCA,
        STUDIO_BANK,
        STUDIO_COMMANDREPLAY
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ERRORCALLBACK_INFO
    {
        public  RESULT                      result;
        public  ERRORCALLBACK_INSTANCETYPE  instancetype;
        public  IntPtr                      instance;
        public  StringWrapper               functionname;
        public  StringWrapper               functionparams;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CPU_USAGE
    {
        public float    dsp;                    /* DSP mixing CPU usage. */
        public float    stream;                 /* Streaming engine CPU usage. */
        public float    geometry;               /* Geometry engine CPU usage. */
        public float    update;                 /* System::update CPU usage. */
        public float    convolution1;           /* Convolution reverb processing thread #1 CPU usage */
        public float    convolution2;           /* Convolution reverb processing thread #2 CPU usage */
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DSP_DATA_PARAMETER_INFO
    {
        public IntPtr   data;
        public uint     length;
        public int      index;
    }

    [Flags]
    public enum SYSTEM_CALLBACK_TYPE : uint
    {
        DEVICELISTCHANGED      = 0x00000001,
        DEVICELOST             = 0x00000002,
        MEMORYALLOCATIONFAILED = 0x00000004,
        THREADCREATED          = 0x00000008,
        BADDSPCONNECTION       = 0x00000010,
        PREMIX                 = 0x00000020,
        POSTMIX                = 0x00000040,
        ERROR                  = 0x00000080,
        MIDMIX                 = 0x00000100,
        THREADDESTROYED        = 0x00000200,
        PREUPDATE              = 0x00000400,
        POSTUPDATE             = 0x00000800,
        RECORDLISTCHANGED      = 0x00001000,
        BUFFEREDNOMIX          = 0x00002000,
        DEVICEREINITIALIZE     = 0x00004000,
        OUTPUTUNDERRUN         = 0x00008000,
        RECORDPOSITIONCHANGED  = 0x00010000,
        ALL                    = 0xFFFFFFFF,
    }

    /*
        FMOD Callbacks
    */
    public delegate RESULT DEBUG_CALLBACK           (DEBUG_FLAGS flags, IntPtr file, int line, IntPtr func, IntPtr message);
    public delegate RESULT SYSTEM_CALLBACK          (IntPtr system, SYSTEM_CALLBACK_TYPE type, IntPtr commanddata1, IntPtr commanddata2, IntPtr userdata);
    public delegate RESULT CHANNELCONTROL_CALLBACK  (IntPtr channelcontrol, CHANNELCONTROL_TYPE controltype, CHANNELCONTROL_CALLBACK_TYPE callbacktype, IntPtr commanddata1, IntPtr commanddata2);
    public delegate RESULT DSP_CALLBACK             (IntPtr dsp, DSP_CALLBACK_TYPE type, IntPtr data);
    public delegate RESULT SOUND_NONBLOCK_CALLBACK  (IntPtr sound, RESULT result);
    public delegate RESULT SOUND_PCMREAD_CALLBACK   (IntPtr sound, IntPtr data, uint datalen);
    public delegate RESULT SOUND_PCMSETPOS_CALLBACK (IntPtr sound, int subsound, uint position, TIMEUNIT postype);
    public delegate RESULT FILE_OPEN_CALLBACK       (IntPtr name, ref uint filesize, ref IntPtr handle, IntPtr userdata);
    public delegate RESULT FILE_CLOSE_CALLBACK      (IntPtr handle, IntPtr userdata);
    public delegate RESULT FILE_READ_CALLBACK       (IntPtr handle, IntPtr buffer, uint sizebytes, ref uint bytesread, IntPtr userdata);
    public delegate RESULT FILE_SEEK_CALLBACK       (IntPtr handle, uint pos, IntPtr userdata);
    public delegate RESULT FILE_ASYNCREAD_CALLBACK  (IntPtr info, IntPtr userdata);
    public delegate RESULT FILE_ASYNCCANCEL_CALLBACK(IntPtr info, IntPtr userdata);
    public delegate void   FILE_ASYNCDONE_FUNC      (IntPtr info, RESULT result);
    public delegate IntPtr MEMORY_ALLOC_CALLBACK    (uint size, MEMORY_TYPE type, IntPtr sourcestr);
    public delegate IntPtr MEMORY_REALLOC_CALLBACK  (IntPtr ptr, uint size, MEMORY_TYPE type, IntPtr sourcestr);
    public delegate void   MEMORY_FREE_CALLBACK     (IntPtr ptr, MEMORY_TYPE type, IntPtr sourcestr);
    public delegate float  CB_3D_ROLLOFF_CALLBACK   (IntPtr channelcontrol, float distance);

    public enum DSP_RESAMPLER : int
    {
        DEFAULT,
        NOINTERP,
        LINEAR,
        CUBIC,
        SPLINE,

        MAX,
    }

    public enum DSP_CALLBACK_TYPE : int
    {
        DATAPARAMETERRELEASE,

        MAX,
    }

    public enum DSPCONNECTION_TYPE : int
    {
        STANDARD,
        SIDECHAIN,
        SEND,
        SEND_SIDECHAIN,

        MAX,
    }

    public enum TAGTYPE : int
    {
        UNKNOWN = 0,
        ID3V1,
        ID3V2,
        VORBISCOMMENT,
        SHOUTCAST,
        ICECAST,
        ASF,
        MIDI,
        PLAYLIST,
        FMOD,
        USER,

        MAX
    }

    public enum TAGDATATYPE : int
    {
        BINARY = 0,
        INT,
        FLOAT,
        STRING,
        STRING_UTF16,
        STRING_UTF16BE,
        STRING_UTF8,

        MAX
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TAG
    {
        public  TAGTYPE           type;
        public  TAGDATATYPE       datatype;
        public  StringWrapper     name;
        public  IntPtr            data;
        public  uint              datalen;
        public  bool              updated;
    }

    [Flags]
    public enum TIMEUNIT : uint
    {
        MS          = 0x00000001,
        PCM         = 0x00000002,
        PCMBYTES    = 0x00000004,
        RAWBYTES    = 0x00000008,
        PCMFRACTION = 0x00000010,
        MODORDER    = 0x00000100,
        MODROW      = 0x00000200,
        MODPATTERN  = 0x00000400,
    }

    public struct PORT_INDEX
    {
        public const ulong NONE               = 0xFFFFFFFFFFFFFFFF;
        public const ulong FLAG_VR_CONTROLLER = 0x1000000000000000;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CREATESOUNDEXINFO
    {
        public int                         cbsize;
        public uint                        length;
        public uint                        fileoffset;
        public int                         numchannels;
        public int                         defaultfrequency;
        public SOUND_FORMAT                format;
        public uint                        decodebuffersize;
        public int                         initialsubsound;
        public int                         numsubsounds;
        public IntPtr                      inclusionlist;
        public int                         inclusionlistnum;
        public IntPtr                      pcmreadcallback_internal;
        public IntPtr                      pcmsetposcallback_internal;
        public IntPtr                      nonblockcallback_internal;
        public IntPtr                      dlsname;
        public IntPtr                      encryptionkey;
        public int                         maxpolyphony;
        public IntPtr                      userdata;
        public SOUND_TYPE                  suggestedsoundtype;
        public IntPtr                      fileuseropen_internal;
        public IntPtr                      fileuserclose_internal;
        public IntPtr                      fileuserread_internal;
        public IntPtr                      fileuserseek_internal;
        public IntPtr                      fileuserasyncread_internal;
        public IntPtr                      fileuserasynccancel_internal;
        public IntPtr                      fileuserdata;
        public int                         filebuffersize;
        public CHANNELORDER                channelorder;
        public IntPtr                      initialsoundgroup;
        public uint                        initialseekposition;
        public TIMEUNIT                    initialseekpostype;
        public int                         ignoresetfilesystem;
        public uint                        audioqueuepolicy;
        public uint                        minmidigranularity;
        public int                         nonblockthreadid;
        public IntPtr                      fsbguid;

        public SOUND_PCMREAD_CALLBACK pcmreadcallback
        {
            set { pcmreadcallback_internal = (value == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(value)); }
            get { return pcmreadcallback_internal == IntPtr.Zero ? null : (SOUND_PCMREAD_CALLBACK)Marshal.GetDelegateForFunctionPointer(pcmreadcallback_internal, typeof(SOUND_PCMREAD_CALLBACK)); }
        }
        public SOUND_PCMSETPOS_CALLBACK pcmsetposcallback
        {
            set { pcmsetposcallback_internal = (value == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(value)); }
            get { return pcmsetposcallback_internal == IntPtr.Zero ? null : (SOUND_PCMSETPOS_CALLBACK)Marshal.GetDelegateForFunctionPointer(pcmsetposcallback_internal, typeof(SOUND_PCMSETPOS_CALLBACK)); }
        }
        public SOUND_NONBLOCK_CALLBACK nonblockcallback
        {
            set { nonblockcallback_internal = (value == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(value)); }
            get { return nonblockcallback_internal == IntPtr.Zero ? null : (SOUND_NONBLOCK_CALLBACK)Marshal.GetDelegateForFunctionPointer(nonblockcallback_internal, typeof(SOUND_NONBLOCK_CALLBACK)); }
        }
        public FILE_OPEN_CALLBACK fileuseropen
        {
            set { fileuseropen_internal = (value == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(value)); }
            get { return fileuseropen_internal == IntPtr.Zero ? null : (FILE_OPEN_CALLBACK)Marshal.GetDelegateForFunctionPointer(fileuseropen_internal, typeof(FILE_OPEN_CALLBACK)); }
        }
        public FILE_CLOSE_CALLBACK fileuserclose
        {
            set { fileuserclose_internal = (value == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(value)); }
            get { return fileuserclose_internal == IntPtr.Zero ? null : (FILE_CLOSE_CALLBACK)Marshal.GetDelegateForFunctionPointer(fileuserclose_internal, typeof(FILE_CLOSE_CALLBACK)); }
        }
        public FILE_READ_CALLBACK fileuserread
        {
            set { fileuserread_internal = (value == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(value)); }
            get { return fileuserread_internal == IntPtr.Zero ? null : (FILE_READ_CALLBACK)Marshal.GetDelegateForFunctionPointer(fileuserread_internal, typeof(FILE_READ_CALLBACK)); }
        }
        public FILE_SEEK_CALLBACK fileuserseek
        {
            set { fileuserseek_internal = (value == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(value)); }
            get { return fileuserseek_internal == IntPtr.Zero ? null : (FILE_SEEK_CALLBACK)Marshal.GetDelegateForFunctionPointer(fileuserseek_internal, typeof(FILE_SEEK_CALLBACK)); }
        }
        public FILE_ASYNCREAD_CALLBACK fileuserasyncread
        {
            set { fileuserasyncread_internal = (value == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(value)); }
            get { return fileuserasyncread_internal == IntPtr.Zero ? null : (FILE_ASYNCREAD_CALLBACK)Marshal.GetDelegateForFunctionPointer(fileuserasyncread_internal, typeof(FILE_ASYNCREAD_CALLBACK)); }
        }
        public FILE_ASYNCCANCEL_CALLBACK fileuserasynccancel
        {
            set { fileuserasynccancel_internal = (value == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(value)); }
            get { return fileuserasynccancel_internal == IntPtr.Zero ? null : (FILE_ASYNCCANCEL_CALLBACK)Marshal.GetDelegateForFunctionPointer(fileuserasynccancel_internal, typeof(FILE_ASYNCCANCEL_CALLBACK)); }
        }

    }

#pragma warning disable 414
    [StructLayout(LayoutKind.Sequential)]
    public struct REVERB_PROPERTIES
    {
        public float DecayTime;
        public float EarlyDelay;
        public float LateDelay;
        public float HFReference;
        public float HFDecayRatio;
        public float Diffusion;
        public float Density;
        public float LowShelfFrequency;
        public float LowShelfGain;
        public float HighCut;
        public float EarlyLateMix;
        public float WetLevel;

        #region wrapperinternal
        public REVERB_PROPERTIES(float decayTime, float earlyDelay, float lateDelay, float hfReference,
            float hfDecayRatio, float diffusion, float density, float lowShelfFrequency, float lowShelfGain,
            float highCut, float earlyLateMix, float wetLevel)
        {
            DecayTime = decayTime;
            EarlyDelay = earlyDelay;
            LateDelay = lateDelay;
            HFReference = hfReference;
            HFDecayRatio = hfDecayRatio;
            Diffusion = diffusion;
            Density = density;
            LowShelfFrequency = lowShelfFrequency;
            LowShelfGain = lowShelfGain;
            HighCut = highCut;
            EarlyLateMix = earlyLateMix;
            WetLevel = wetLevel;
        }
        #endregion
    }
#pragma warning restore 414

    public class PRESET
    {
        public static REVERB_PROPERTIES OFF()                 { return new REVERB_PROPERTIES(  1000,    7,  11, 5000, 100, 100, 100, 250, 0,    20,  96, -80.0f );}
        public static REVERB_PROPERTIES GENERIC()             { return new REVERB_PROPERTIES(  1500,    7,  11, 5000,  83, 100, 100, 250, 0, 14500,  96,  -8.0f );}
        public static REVERB_PROPERTIES PADDEDCELL()          { return new REVERB_PROPERTIES(   170,    1,   2, 5000,  10, 100, 100, 250, 0,   160,  84,  -7.8f );}
        public static REVERB_PROPERTIES ROOM()                { return new REVERB_PROPERTIES(   400,    2,   3, 5000,  83, 100, 100, 250, 0,  6050,  88,  -9.4f );}
        public static REVERB_PROPERTIES BATHROOM()            { return new REVERB_PROPERTIES(  1500,    7,  11, 5000,  54, 100,  60, 250, 0,  2900,  83,   0.5f );}
        public static REVERB_PROPERTIES LIVINGROOM()          { return new REVERB_PROPERTIES(   500,    3,   4, 5000,  10, 100, 100, 250, 0,   160,  58, -19.0f );}
        public static REVERB_PROPERTIES STONEROOM()           { return new REVERB_PROPERTIES(  2300,   12,  17, 5000,  64, 100, 100, 250, 0,  7800,  71,  -8.5f );}
        public static REVERB_PROPERTIES AUDITORIUM()          { return new REVERB_PROPERTIES(  4300,   20,  30, 5000,  59, 100, 100, 250, 0,  5850,  64, -11.7f );}
        public static REVERB_PROPERTIES CONCERTHALL()         { return new REVERB_PROPERTIES(  3900,   20,  29, 5000,  70, 100, 100, 250, 0,  5650,  80,  -9.8f );}
        public static REVERB_PROPERTIES CAVE()                { return new REVERB_PROPERTIES(  2900,   15,  22, 5000, 100, 100, 100, 250, 0, 20000,  59, -11.3f );}
        public static REVERB_PROPERTIES ARENA()               { return new REVERB_PROPERTIES(  7200,   20,  30, 5000,  33, 100, 100, 250, 0,  4500,  80,  -9.6f );}
        public static REVERB_PROPERTIES HANGAR()              { return new REVERB_PROPERTIES( 10000,   20,  30, 5000,  23, 100, 100, 250, 0,  3400,  72,  -7.4f );}
        public static REVERB_PROPERTIES CARPETTEDHALLWAY()    { return new REVERB_PROPERTIES(   300,    2,  30, 5000,  10, 100, 100, 250, 0,   500,  56, -24.0f );}
        public static REVERB_PROPERTIES HALLWAY()             { return new REVERB_PROPERTIES(  1500,    7,  11, 5000,  59, 100, 100, 250, 0,  7800,  87,  -5.5f );}
        public static REVERB_PROPERTIES STONECORRIDOR()       { return new REVERB_PROPERTIES(   270,   13,  20, 5000,  79, 100, 100, 250, 0,  9000,  86,  -6.0f );}
        public static REVERB_PROPERTIES ALLEY()               { return new REVERB_PROPERTIES(  1500,    7,  11, 5000,  86, 100, 100, 250, 0,  8300,  80,  -9.8f );}
        public static REVERB_PROPERTIES FOREST()              { return new REVERB_PROPERTIES(  1500,  162,  88, 5000,  54,  79, 100, 250, 0,   760,  94, -12.3f );}
        public static REVERB_PROPERTIES CITY()                { return new REVERB_PROPERTIES(  1500,    7,  11, 5000,  67,  50, 100, 250, 0,  4050,  66, -26.0f );}
        public static REVERB_PROPERTIES MOUNTAINS()           { return new REVERB_PROPERTIES(  1500,  300, 100, 5000,  21,  27, 100, 250, 0,  1220,  82, -24.0f );}
        public static REVERB_PROPERTIES QUARRY()              { return new REVERB_PROPERTIES(  1500,   61,  25, 5000,  83, 100, 100, 250, 0,  3400, 100,  -5.0f );}
        public static REVERB_PROPERTIES PLAIN()               { return new REVERB_PROPERTIES(  1500,  179, 100, 5000,  50,  21, 100, 250, 0,  1670,  65, -28.0f );}
        public static REVERB_PROPERTIES PARKINGLOT()          { return new REVERB_PROPERTIES(  1700,    8,  12, 5000, 100, 100, 100, 250, 0, 20000,  56, -19.5f );}
        public static REVERB_PROPERTIES SEWERPIPE()           { return new REVERB_PROPERTIES(  2800,   14,  21, 5000,  14,  80,  60, 250, 0,  3400,  66,   1.2f );}
        public static REVERB_PROPERTIES UNDERWATER()          { return new REVERB_PROPERTIES(  1500,    7,  11, 5000,  10, 100, 100, 250, 0,   500,  92,   7.0f );}
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ADVANCEDSETTINGS
    {
        public int                 cbSize;
        public int                 maxMPEGCodecs;
        public int                 maxADPCMCodecs;
        public int                 maxXMACodecs;
        public int                 maxVorbisCodecs;
        public int                 maxAT9Codecs;
        public int                 maxFADPCMCodecs;
        public int                 maxPCMCodecs;
        public int                 ASIONumChannels;
        public IntPtr              ASIOChannelList;
        public IntPtr              ASIOSpeakerList;
        public float               vol0virtualvol;
        public uint                defaultDecodeBufferSize;
        public ushort              profilePort;
        public uint                geometryMaxFadeTime;
        public float               distanceFilterCenterFreq;
        public int                 reverb3Dinstance;
        public int                 DSPBufferPoolSize;
        public DSP_RESAMPLER       resamplerMethod;
        public uint                randomSeed;
        public int                 maxConvolutionThreads;
        public int                 maxOpusCodecs;
    }

    [Flags]
    public enum DRIVER_STATE : uint
    {
        CONNECTED = 0x00000001,
        DEFAULT   = 0x00000002,
    }

    public enum THREAD_PRIORITY : int
    {
        /* Platform specific priority range */
        PLATFORM_MIN        = -32 * 1024,
        PLATFORM_MAX        =  32 * 1024,

        /* Platform agnostic priorities, maps internally to platform specific value */
        DEFAULT             = PLATFORM_MIN - 1,
        LOW                 = PLATFORM_MIN - 2,
        MEDIUM              = PLATFORM_MIN - 3,
        HIGH                = PLATFORM_MIN - 4,
        VERY_HIGH           = PLATFORM_MIN - 5,
        EXTREME             = PLATFORM_MIN - 6,
        CRITICAL            = PLATFORM_MIN - 7,

        /* Thread defaults */
        MIXER               = EXTREME,
        FEEDER              = CRITICAL,
        STREAM              = VERY_HIGH,
        FILE                = HIGH,
        NONBLOCKING         = HIGH,
        RECORD              = HIGH,
        GEOMETRY            = LOW,
        PROFILER            = MEDIUM,
        STUDIO_UPDATE       = MEDIUM,
        STUDIO_LOAD_BANK    = MEDIUM,
        STUDIO_LOAD_SAMPLE  = MEDIUM,
        CONVOLUTION1        = VERY_HIGH,
        CONVOLUTION2        = VERY_HIGH

    }

    public enum THREAD_STACK_SIZE : uint
    {
        DEFAULT             = 0,
        MIXER               = 80  * 1024,
        FEEDER              = 16  * 1024,
        STREAM              = 96  * 1024,
        FILE                = 64  * 1024,
        NONBLOCKING         = 112 * 1024,
        RECORD              = 16  * 1024,
        GEOMETRY            = 48  * 1024,
        PROFILER            = 128 * 1024,
        STUDIO_UPDATE       = 96  * 1024,
        STUDIO_LOAD_BANK    = 96  * 1024,
        STUDIO_LOAD_SAMPLE  = 96  * 1024,
        CONVOLUTION1        = 16  * 1024,
        CONVOLUTION2        = 16  * 1024
    }

    [Flags]
    public enum THREAD_AFFINITY : long
    {
        /* Platform agnostic thread groupings */
        GROUP_DEFAULT       = 0x4000000000000000,
        GROUP_A             = 0x4000000000000001,
        GROUP_B             = 0x4000000000000002,
        GROUP_C             = 0x4000000000000003,

        /* Thread defaults */
        MIXER               = GROUP_A,
        FEEDER              = GROUP_C,
        STREAM              = GROUP_C,
        FILE                = GROUP_C,
        NONBLOCKING         = GROUP_C,
        RECORD              = GROUP_C,
        GEOMETRY            = GROUP_C,
        PROFILER            = GROUP_C,
        STUDIO_UPDATE       = GROUP_B,
        STUDIO_LOAD_BANK    = GROUP_C,
        STUDIO_LOAD_SAMPLE  = GROUP_C,
        CONVOLUTION1        = GROUP_C,
        CONVOLUTION2        = GROUP_C,

        /* Core mask, valid up to 1 << 61 */
        CORE_ALL            = 0,
        CORE_0              = 1 << 0,
        CORE_1              = 1 << 1,
        CORE_2              = 1 << 2,
        CORE_3              = 1 << 3,
        CORE_4              = 1 << 4,
        CORE_5              = 1 << 5,
        CORE_6              = 1 << 6,
        CORE_7              = 1 << 7,
        CORE_8              = 1 << 8,
        CORE_9              = 1 << 9,
        CORE_10             = 1 << 10,
        CORE_11             = 1 << 11,
        CORE_12             = 1 << 12,
        CORE_13             = 1 << 13,
        CORE_14             = 1 << 14,
        CORE_15             = 1 << 15
    }

    public enum THREAD_TYPE : int
    {
        MIXER,
        FEEDER,
        STREAM,
        FILE,
        NONBLOCKING,
        RECORD,
        GEOMETRY,
        PROFILER,
        STUDIO_UPDATE,
        STUDIO_LOAD_BANK,
        STUDIO_LOAD_SAMPLE,
        CONVOLUTION1,
        CONVOLUTION2,

        MAX
    }

    /*
        FMOD System factory functions.  Use this to create an FMOD System Instance.  below you will see System init/close to get started.
    */
    public struct Factory
    {
        public static RESULT System_Create(out System system)
        {
            return FMOD5_System_Create(out system.handle, VERSION.number);
        }

        #region importfunctions
        private delegate RESULT FMOD5_System_Create_Delegate(out IntPtr system, uint headerversion);
        private static FMOD5_System_Create_Delegate FMOD5_System_Create_Internal = null;
        private static FMOD5_System_Create_Delegate FMOD5_System_Create => FMOD5_System_Create_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_Create_Delegate>(nameof(FMOD5_System_Create));



        /*private delegate RESULT FMOD5_System_CreateDelegate(out IntPtr system, uint headerversion);
        private static FMOD5_System_CreateDelegate FMOD5_System_Create_Internal = null;
        private static FMOD5_System_CreateDelegate FMOD5_System_Create => FMOD5_System_Create_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_CreateDelegate>(nameof(FMOD5_System_Create));*/

        #endregion
    }

    /*
        FMOD global system functions (optional).
    */
    public struct Memory
    {
        public static RESULT Initialize(IntPtr poolmem, int poollen, MEMORY_ALLOC_CALLBACK useralloc, MEMORY_REALLOC_CALLBACK userrealloc, MEMORY_FREE_CALLBACK userfree, MEMORY_TYPE memtypeflags = MEMORY_TYPE.ALL)
        {
            return FMOD5_Memory_Initialize(poolmem, poollen, useralloc, userrealloc, userfree, memtypeflags);
        }

        public static RESULT GetStats(out int currentalloced, out int maxalloced, bool blocking = true)
        {
            return FMOD5_Memory_GetStats(out currentalloced, out maxalloced, blocking);
        }

        #region importfunctions
        private delegate RESULT FMOD5_Memory_Initialize_Delegate(IntPtr poolmem, int poollen, MEMORY_ALLOC_CALLBACK useralloc, MEMORY_REALLOC_CALLBACK userrealloc, MEMORY_FREE_CALLBACK userfree, MEMORY_TYPE memtypeflags);
        private static FMOD5_Memory_Initialize_Delegate FMOD5_Memory_Initialize_Internal = null;
        private static FMOD5_Memory_Initialize_Delegate FMOD5_Memory_Initialize => FMOD5_Memory_Initialize_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Memory_Initialize_Delegate>(nameof(FMOD5_Memory_Initialize));

        private delegate RESULT FMOD5_Memory_GetStats_Delegate(out int currentalloced, out int maxalloced, bool blocking);
        private static FMOD5_Memory_GetStats_Delegate FMOD5_Memory_GetStats_Internal = null;
        private static FMOD5_Memory_GetStats_Delegate FMOD5_Memory_GetStats => FMOD5_Memory_GetStats_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Memory_GetStats_Delegate>(nameof(FMOD5_Memory_GetStats));


        #endregion
    }

    public struct Debug
    {
        public static RESULT Initialize(DEBUG_FLAGS flags, DEBUG_MODE mode = DEBUG_MODE.TTY, DEBUG_CALLBACK callback = null, string filename = null)
        {
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                return FMOD5_Debug_Initialize(flags, mode, callback, encoder.byteFromStringUTF8(filename));
            }
        }

        #region importfunctions
        private delegate RESULT FMOD5_Debug_Initialize_Delegate(DEBUG_FLAGS flags, DEBUG_MODE mode, DEBUG_CALLBACK callback, byte[] filename);
        private static FMOD5_Debug_Initialize_Delegate FMOD5_Debug_Initialize_Internal = null;
        private static FMOD5_Debug_Initialize_Delegate FMOD5_Debug_Initialize => FMOD5_Debug_Initialize_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Debug_Initialize_Delegate>(nameof(FMOD5_Debug_Initialize));


        #endregion
    }

    public struct Thread
    {
        public static RESULT SetAttributes(THREAD_TYPE type, THREAD_AFFINITY affinity = THREAD_AFFINITY.GROUP_DEFAULT, THREAD_PRIORITY priority = THREAD_PRIORITY.DEFAULT, THREAD_STACK_SIZE stacksize = THREAD_STACK_SIZE.DEFAULT)
        {
            return FMOD5_Thread_SetAttributes(type, affinity, priority, stacksize);
        }

        #region importfunctions
        private delegate RESULT FMOD5_Thread_SetAttributes_Delegate(THREAD_TYPE type, THREAD_AFFINITY affinity, THREAD_PRIORITY priority, THREAD_STACK_SIZE stacksize);
        private static FMOD5_Thread_SetAttributes_Delegate FMOD5_Thread_SetAttributes_Internal = null;
        private static FMOD5_Thread_SetAttributes_Delegate FMOD5_Thread_SetAttributes => FMOD5_Thread_SetAttributes_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Thread_SetAttributes_Delegate>(nameof(FMOD5_Thread_SetAttributes));

        #endregion
    }

    /*
        'System' API.
    */
    public struct System
    {
        public RESULT release()
        {
            return FMOD5_System_Release(this.handle);
        }

        // Setup functions.
        public RESULT setOutput(OUTPUTTYPE output)
        {
            return FMOD5_System_SetOutput(this.handle, output);
        }
        public RESULT getOutput(out OUTPUTTYPE output)
        {
            return FMOD5_System_GetOutput(this.handle, out output);
        }
        public RESULT getNumDrivers(out int numdrivers)
        {
            return FMOD5_System_GetNumDrivers(this.handle, out numdrivers);
        }
        public RESULT getDriverInfo(int id, out string name, int namelen, out Guid guid, out int systemrate, out SPEAKERMODE speakermode, out int speakermodechannels)
        {
            IntPtr stringMem = Marshal.AllocHGlobal(namelen);

            RESULT result = FMOD5_System_GetDriverInfo(this.handle, id, stringMem, namelen, out guid, out systemrate, out speakermode, out speakermodechannels);
            using (StringHelper.ThreadSafeEncoding encoding = StringHelper.GetFreeHelper())
            {
                name = encoding.stringFromNative(stringMem);
            }
            Marshal.FreeHGlobal(stringMem);

            return result;
        }
        public RESULT getDriverInfo(int id, out Guid guid, out int systemrate, out SPEAKERMODE speakermode, out int speakermodechannels)
        {
            return FMOD5_System_GetDriverInfo(this.handle, id, IntPtr.Zero, 0, out guid, out systemrate, out speakermode, out speakermodechannels);
        }
        public RESULT setDriver(int driver)
        {
            return FMOD5_System_SetDriver(this.handle, driver);
        }
        public RESULT getDriver(out int driver)
        {
            return FMOD5_System_GetDriver(this.handle, out driver);
        }
        public RESULT setSoftwareChannels(int numsoftwarechannels)
        {
            return FMOD5_System_SetSoftwareChannels(this.handle, numsoftwarechannels);
        }
        public RESULT getSoftwareChannels(out int numsoftwarechannels)
        {
            return FMOD5_System_GetSoftwareChannels(this.handle, out numsoftwarechannels);
        }
        public RESULT setSoftwareFormat(int samplerate, SPEAKERMODE speakermode, int numrawspeakers)
        {
            return FMOD5_System_SetSoftwareFormat(this.handle, samplerate, speakermode, numrawspeakers);
        }
        public RESULT getSoftwareFormat(out int samplerate, out SPEAKERMODE speakermode, out int numrawspeakers)
        {
            return FMOD5_System_GetSoftwareFormat(this.handle, out samplerate, out speakermode, out numrawspeakers);
        }
        public RESULT setDSPBufferSize(uint bufferlength, int numbuffers)
        {
            return FMOD5_System_SetDSPBufferSize(this.handle, bufferlength, numbuffers);
        }
        public RESULT getDSPBufferSize(out uint bufferlength, out int numbuffers)
        {
            return FMOD5_System_GetDSPBufferSize(this.handle, out bufferlength, out numbuffers);
        }
        public RESULT setFileSystem(FILE_OPEN_CALLBACK useropen, FILE_CLOSE_CALLBACK userclose, FILE_READ_CALLBACK userread, FILE_SEEK_CALLBACK userseek, FILE_ASYNCREAD_CALLBACK userasyncread, FILE_ASYNCCANCEL_CALLBACK userasynccancel, int blockalign)
        {
            return FMOD5_System_SetFileSystem(this.handle, useropen, userclose, userread, userseek, userasyncread, userasynccancel, blockalign);
        }
        public RESULT attachFileSystem(FILE_OPEN_CALLBACK useropen, FILE_CLOSE_CALLBACK userclose, FILE_READ_CALLBACK userread, FILE_SEEK_CALLBACK userseek)
        {
            return FMOD5_System_AttachFileSystem(this.handle, useropen, userclose, userread, userseek);
        }
        public RESULT setAdvancedSettings(ref ADVANCEDSETTINGS settings)
        {
            settings.cbSize = MarshalHelper.SizeOf(typeof(ADVANCEDSETTINGS));
            return FMOD5_System_SetAdvancedSettings(this.handle, ref settings);
        }
        public RESULT getAdvancedSettings(ref ADVANCEDSETTINGS settings)
        {
            settings.cbSize = MarshalHelper.SizeOf(typeof(ADVANCEDSETTINGS));
            return FMOD5_System_GetAdvancedSettings(this.handle, ref settings);
        }
        public RESULT setCallback(SYSTEM_CALLBACK callback, SYSTEM_CALLBACK_TYPE callbackmask = SYSTEM_CALLBACK_TYPE.ALL)
        {
            return FMOD5_System_SetCallback(this.handle, callback, callbackmask);
        }

        // Plug-in support.
        public RESULT setPluginPath(string path)
        {
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                return FMOD5_System_SetPluginPath(this.handle, encoder.byteFromStringUTF8(path));
            }
        }
        public RESULT loadPlugin(string filename, out uint handle, uint priority = 0)
        {
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                return FMOD5_System_LoadPlugin(this.handle, encoder.byteFromStringUTF8(filename), out handle, priority);
            }
        }
        public RESULT unloadPlugin(uint handle)
        {
            return FMOD5_System_UnloadPlugin(this.handle, handle);
        }
        public RESULT getNumNestedPlugins(uint handle, out int count)
        {
            return FMOD5_System_GetNumNestedPlugins(this.handle, handle, out count);
        }
        public RESULT getNestedPlugin(uint handle, int index, out uint nestedhandle)
        {
            return FMOD5_System_GetNestedPlugin(this.handle, handle, index, out nestedhandle);
        }
        public RESULT getNumPlugins(PLUGINTYPE plugintype, out int numplugins)
        {
            return FMOD5_System_GetNumPlugins(this.handle, plugintype, out numplugins);
        }
        public RESULT getPluginHandle(PLUGINTYPE plugintype, int index, out uint handle)
        {
            return FMOD5_System_GetPluginHandle(this.handle, plugintype, index, out handle);
        }
        public RESULT getPluginInfo(uint handle, out PLUGINTYPE plugintype, out string name, int namelen, out uint version)
        {
            IntPtr stringMem = Marshal.AllocHGlobal(namelen);

            RESULT result = FMOD5_System_GetPluginInfo(this.handle, handle, out plugintype, stringMem, namelen, out version);
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                name = encoder.stringFromNative(stringMem);
            }
            Marshal.FreeHGlobal(stringMem);

            return result;
        }
        public RESULT getPluginInfo(uint handle, out PLUGINTYPE plugintype, out uint version)
        {
            return FMOD5_System_GetPluginInfo(this.handle, handle, out plugintype, IntPtr.Zero, 0, out version);
        }
        public RESULT setOutputByPlugin(uint handle)
        {
            return FMOD5_System_SetOutputByPlugin(this.handle, handle);
        }
        public RESULT getOutputByPlugin(out uint handle)
        {
            return FMOD5_System_GetOutputByPlugin(this.handle, out handle);
        }
        public RESULT createDSPByPlugin(uint handle, out DSP dsp)
        {
            return FMOD5_System_CreateDSPByPlugin(this.handle, handle, out dsp.handle);
        }
        public RESULT getDSPInfoByPlugin(uint handle, out IntPtr description)
        {
            return FMOD5_System_GetDSPInfoByPlugin(this.handle, handle, out description);
        }
        public RESULT registerDSP(ref DSP_DESCRIPTION description, out uint handle)
        {
            return FMOD5_System_RegisterDSP(this.handle, ref description, out handle);
        }

        // Init/Close.
        public RESULT init(int maxchannels, INITFLAGS flags, IntPtr extradriverdata)
        {
            return FMOD5_System_Init(this.handle, maxchannels, flags, extradriverdata);
        }
        public RESULT close()
        {
            return FMOD5_System_Close(this.handle);
        }

        // General post-init system functions.
        public RESULT update()
        {
            return FMOD5_System_Update(this.handle);
        }
        public RESULT setSpeakerPosition(SPEAKER speaker, float x, float y, bool active)
        {
            return FMOD5_System_SetSpeakerPosition(this.handle, speaker, x, y, active);
        }
        public RESULT getSpeakerPosition(SPEAKER speaker, out float x, out float y, out bool active)
        {
            return FMOD5_System_GetSpeakerPosition(this.handle, speaker, out x, out y, out active);
        }
        public RESULT setStreamBufferSize(uint filebuffersize, TIMEUNIT filebuffersizetype)
        {
            return FMOD5_System_SetStreamBufferSize(this.handle, filebuffersize, filebuffersizetype);
        }
        public RESULT getStreamBufferSize(out uint filebuffersize, out TIMEUNIT filebuffersizetype)
        {
            return FMOD5_System_GetStreamBufferSize(this.handle, out filebuffersize, out filebuffersizetype);
        }
        public RESULT set3DSettings(float dopplerscale, float distancefactor, float rolloffscale)
        {
            return FMOD5_System_Set3DSettings(this.handle, dopplerscale, distancefactor, rolloffscale);
        }
        public RESULT get3DSettings(out float dopplerscale, out float distancefactor, out float rolloffscale)
        {
            return FMOD5_System_Get3DSettings(this.handle, out dopplerscale, out distancefactor, out rolloffscale);
        }
        public RESULT set3DNumListeners(int numlisteners)
        {
            return FMOD5_System_Set3DNumListeners(this.handle, numlisteners);
        }
        public RESULT get3DNumListeners(out int numlisteners)
        {
            return FMOD5_System_Get3DNumListeners(this.handle, out numlisteners);
        }
        public RESULT set3DListenerAttributes(int listener, ref VECTOR pos, ref VECTOR vel, ref VECTOR forward, ref VECTOR up)
        {
            return FMOD5_System_Set3DListenerAttributes(this.handle, listener, ref pos, ref vel, ref forward, ref up);
        }
        public RESULT get3DListenerAttributes(int listener, out VECTOR pos, out VECTOR vel, out VECTOR forward, out VECTOR up)
        {
            return FMOD5_System_Get3DListenerAttributes(this.handle, listener, out pos, out vel, out forward, out up);
        }
        public RESULT set3DRolloffCallback(CB_3D_ROLLOFF_CALLBACK callback)
        {
            return FMOD5_System_Set3DRolloffCallback(this.handle, callback);
        }
        public RESULT mixerSuspend()
        {
            return FMOD5_System_MixerSuspend(this.handle);
        }
        public RESULT mixerResume()
        {
            return FMOD5_System_MixerResume(this.handle);
        }
        public RESULT getDefaultMixMatrix(SPEAKERMODE sourcespeakermode, SPEAKERMODE targetspeakermode, float[] matrix, int matrixhop)
        {
            return FMOD5_System_GetDefaultMixMatrix(this.handle, sourcespeakermode, targetspeakermode, matrix, matrixhop);
        }
        public RESULT getSpeakerModeChannels(SPEAKERMODE mode, out int channels)
        {
            return FMOD5_System_GetSpeakerModeChannels(this.handle, mode, out channels);
        }

        // System information functions.
        public RESULT getVersion(out uint version)
        {
            return FMOD5_System_GetVersion(this.handle, out version);
        }
        public RESULT getOutputHandle(out IntPtr handle)
        {
            return FMOD5_System_GetOutputHandle(this.handle, out handle);
        }
        public RESULT getChannelsPlaying(out int channels)
        {
            return FMOD5_System_GetChannelsPlaying(this.handle, out channels, IntPtr.Zero);
        }
        public RESULT getChannelsPlaying(out int channels, out int realchannels)
        {
            return FMOD5_System_GetChannelsPlaying2(this.handle, out channels, out realchannels);
        }
        public RESULT getCPUUsage(out CPU_USAGE usage)
        {
            return FMOD5_System_GetCPUUsage(this.handle, out usage);
        }
        public RESULT getFileUsage(out Int64 sampleBytesRead, out Int64 streamBytesRead, out Int64 otherBytesRead)
        {
            return FMOD5_System_GetFileUsage(this.handle, out sampleBytesRead, out streamBytesRead, out otherBytesRead);
        }

        // Sound/DSP/Channel/FX creation and retrieval.
        public RESULT createSound(string name, MODE mode, ref CREATESOUNDEXINFO exinfo, out Sound sound)
        {
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                 return FMOD5_System_CreateSound(this.handle, encoder.byteFromStringUTF8(name), mode, ref exinfo, out sound.handle);
            }
        }
        public RESULT createSound(byte[] data, MODE mode, ref CREATESOUNDEXINFO exinfo, out Sound sound)
        {
            return FMOD5_System_CreateSound(this.handle, data, mode, ref exinfo, out sound.handle);
        }
        public RESULT createSound(IntPtr name_or_data, MODE mode, ref CREATESOUNDEXINFO exinfo, out Sound sound)
        {
            return FMOD5_System_CreateSound2(this.handle, name_or_data, mode, ref exinfo, out sound.handle);
        }
        public RESULT createSound(string name, MODE mode, out Sound sound)
        {
            CREATESOUNDEXINFO exinfo = new CREATESOUNDEXINFO();
            exinfo.cbsize = MarshalHelper.SizeOf(typeof(CREATESOUNDEXINFO));

            return createSound(name, mode, ref exinfo, out sound);
        }
        public RESULT createStream(string name, MODE mode, ref CREATESOUNDEXINFO exinfo, out Sound sound)
        {
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                return FMOD5_System_CreateStream(this.handle, encoder.byteFromStringUTF8(name), mode, ref exinfo, out sound.handle);
            }
        }
        public RESULT createStream(byte[] data, MODE mode, ref CREATESOUNDEXINFO exinfo, out Sound sound)
        {
            return FMOD5_System_CreateStream(this.handle, data, mode, ref exinfo, out sound.handle);
        }
        public RESULT createStream(IntPtr name_or_data, MODE mode, ref CREATESOUNDEXINFO exinfo, out Sound sound)
        {
            return FMOD5_System_CreateStream2(this.handle, name_or_data, mode, ref exinfo, out sound.handle);
        }
        public RESULT createStream(string name, MODE mode, out Sound sound)
        {
            CREATESOUNDEXINFO exinfo = new CREATESOUNDEXINFO();
            exinfo.cbsize = MarshalHelper.SizeOf(typeof(CREATESOUNDEXINFO));

            return createStream(name, mode, ref exinfo, out sound);
        }
        public RESULT createDSP(ref DSP_DESCRIPTION description, out DSP dsp)
        {
            return FMOD5_System_CreateDSP(this.handle, ref description, out dsp.handle);
        }
        public RESULT createDSPByType(DSP_TYPE type, out DSP dsp)
        {
            return FMOD5_System_CreateDSPByType(this.handle, type, out dsp.handle);
        }
        public RESULT createChannelGroup(string name, out ChannelGroup channelgroup)
        {
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                return FMOD5_System_CreateChannelGroup(this.handle, encoder.byteFromStringUTF8(name), out channelgroup.handle);
            }
        }
        public RESULT createSoundGroup(string name, out SoundGroup soundgroup)
        {
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                return FMOD5_System_CreateSoundGroup(this.handle, encoder.byteFromStringUTF8(name), out soundgroup.handle);
            }
        }
        public RESULT createReverb3D(out Reverb3D reverb)
        {
            return FMOD5_System_CreateReverb3D(this.handle, out reverb.handle);
        }
        public RESULT playSound(Sound sound, ChannelGroup channelgroup, bool paused, out Channel channel)
        {
            return FMOD5_System_PlaySound(this.handle, sound.handle, channelgroup.handle, paused, out channel.handle);
        }
        public RESULT playDSP(DSP dsp, ChannelGroup channelgroup, bool paused, out Channel channel)
        {
            return FMOD5_System_PlayDSP(this.handle, dsp.handle, channelgroup.handle, paused, out channel.handle);
        }
        public RESULT getChannel(int channelid, out Channel channel)
        {
            return FMOD5_System_GetChannel(this.handle, channelid, out channel.handle);
        }
        public RESULT getDSPInfoByType(DSP_TYPE type, out IntPtr description)
        {
            return FMOD5_System_GetDSPInfoByType(this.handle, type, out description);
        }
        public RESULT getMasterChannelGroup(out ChannelGroup channelgroup)
        {
            return FMOD5_System_GetMasterChannelGroup(this.handle, out channelgroup.handle);
        }
        public RESULT getMasterSoundGroup(out SoundGroup soundgroup)
        {
            return FMOD5_System_GetMasterSoundGroup(this.handle, out soundgroup.handle);
        }

        // Routing to ports.
        public RESULT attachChannelGroupToPort(PORT_TYPE portType, ulong portIndex, ChannelGroup channelgroup, bool passThru = false)
        {
            return FMOD5_System_AttachChannelGroupToPort(this.handle, portType, portIndex, channelgroup.handle, passThru);
        }
        public RESULT detachChannelGroupFromPort(ChannelGroup channelgroup)
        {
            return FMOD5_System_DetachChannelGroupFromPort(this.handle, channelgroup.handle);
        }

        // Reverb api.
        public RESULT setReverbProperties(int instance, ref REVERB_PROPERTIES prop)
        {
            return FMOD5_System_SetReverbProperties(this.handle, instance, ref prop);
        }
        public RESULT getReverbProperties(int instance, out REVERB_PROPERTIES prop)
        {
            return FMOD5_System_GetReverbProperties(this.handle, instance, out prop);
        }

        // System level DSP functionality.
        public RESULT lockDSP()
        {
            return FMOD5_System_LockDSP(this.handle);
        }
        public RESULT unlockDSP()
        {
            return FMOD5_System_UnlockDSP(this.handle);
        }

        // Recording api
        public RESULT getRecordNumDrivers(out int numdrivers, out int numconnected)
        {
            return FMOD5_System_GetRecordNumDrivers(this.handle, out numdrivers, out numconnected);
        }
        public RESULT getRecordDriverInfo(int id, out string name, int namelen, out Guid guid, out int systemrate, out SPEAKERMODE speakermode, out int speakermodechannels, out DRIVER_STATE state)
        {
            IntPtr stringMem = Marshal.AllocHGlobal(namelen);

            RESULT result = FMOD5_System_GetRecordDriverInfo(this.handle, id, stringMem, namelen, out guid, out systemrate, out speakermode, out speakermodechannels, out state);

            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                name = encoder.stringFromNative(stringMem);
            }
            Marshal.FreeHGlobal(stringMem);

            return result;
        }
        public RESULT getRecordDriverInfo(int id, out Guid guid, out int systemrate, out SPEAKERMODE speakermode, out int speakermodechannels, out DRIVER_STATE state)
        {
            return FMOD5_System_GetRecordDriverInfo(this.handle, id, IntPtr.Zero, 0, out guid, out systemrate, out speakermode, out speakermodechannels, out state);
        }
        public RESULT getRecordPosition(int id, out uint position)
        {
            return FMOD5_System_GetRecordPosition(this.handle, id, out position);
        }
        public RESULT recordStart(int id, Sound sound, bool loop)
        {
            return FMOD5_System_RecordStart(this.handle, id, sound.handle, loop);
        }
        public RESULT recordStop(int id)
        {
            return FMOD5_System_RecordStop(this.handle, id);
        }
        public RESULT isRecording(int id, out bool recording)
        {
            return FMOD5_System_IsRecording(this.handle, id, out recording);
        }

        // Geometry api
        public RESULT createGeometry(int maxpolygons, int maxvertices, out Geometry geometry)
        {
            return FMOD5_System_CreateGeometry(this.handle, maxpolygons, maxvertices, out geometry.handle);
        }
        public RESULT setGeometrySettings(float maxworldsize)
        {
            return FMOD5_System_SetGeometrySettings(this.handle, maxworldsize);
        }
        public RESULT getGeometrySettings(out float maxworldsize)
        {
            return FMOD5_System_GetGeometrySettings(this.handle, out maxworldsize);
        }
        public RESULT loadGeometry(IntPtr data, int datasize, out Geometry geometry)
        {
            return FMOD5_System_LoadGeometry(this.handle, data, datasize, out geometry.handle);
        }
        public RESULT getGeometryOcclusion(ref VECTOR listener, ref VECTOR source, out float direct, out float reverb)
        {
            return FMOD5_System_GetGeometryOcclusion(this.handle, ref listener, ref source, out direct, out reverb);
        }

        // Network functions
        public RESULT setNetworkProxy(string proxy)
        {
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                return FMOD5_System_SetNetworkProxy(this.handle, encoder.byteFromStringUTF8(proxy));
            }
        }
        public RESULT getNetworkProxy(out string proxy, int proxylen)
        {
            IntPtr stringMem = Marshal.AllocHGlobal(proxylen);

            RESULT result = FMOD5_System_GetNetworkProxy(this.handle, stringMem, proxylen);
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                proxy = encoder.stringFromNative(stringMem);
            }
            Marshal.FreeHGlobal(stringMem);

            return result;
        }
        public RESULT setNetworkTimeout(int timeout)
        {
            return FMOD5_System_SetNetworkTimeout(this.handle, timeout);
        }
        public RESULT getNetworkTimeout(out int timeout)
        {
            return FMOD5_System_GetNetworkTimeout(this.handle, out timeout);
        }

        // Userdata set/get
        public RESULT setUserData(IntPtr userdata)
        {
            return FMOD5_System_SetUserData(this.handle, userdata);
        }
        public RESULT getUserData(out IntPtr userdata)
        {
            return FMOD5_System_GetUserData(this.handle, out userdata);
        }

        #region importfunctions
        private delegate RESULT FMOD5_System_Release_Delegate(IntPtr system);
        private static FMOD5_System_Release_Delegate FMOD5_System_Release_Internal = null;
        private static FMOD5_System_Release_Delegate FMOD5_System_Release => FMOD5_System_Release_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_Release_Delegate>(nameof(FMOD5_System_Release));

        private delegate RESULT FMOD5_System_SetOutput_Delegate(IntPtr system, OUTPUTTYPE output);
        private static FMOD5_System_SetOutput_Delegate FMOD5_System_SetOutput_Internal = null;
        private static FMOD5_System_SetOutput_Delegate FMOD5_System_SetOutput => FMOD5_System_SetOutput_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_SetOutput_Delegate>(nameof(FMOD5_System_SetOutput));

        private delegate RESULT FMOD5_System_GetOutput_Delegate(IntPtr system, out OUTPUTTYPE output);
        private static FMOD5_System_GetOutput_Delegate FMOD5_System_GetOutput_Internal = null;
        private static FMOD5_System_GetOutput_Delegate FMOD5_System_GetOutput => FMOD5_System_GetOutput_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_GetOutput_Delegate>(nameof(FMOD5_System_GetOutput));

        private delegate RESULT FMOD5_System_GetNumDrivers_Delegate(IntPtr system, out int numdrivers);
        private static FMOD5_System_GetNumDrivers_Delegate FMOD5_System_GetNumDrivers_Internal = null;
        private static FMOD5_System_GetNumDrivers_Delegate FMOD5_System_GetNumDrivers => FMOD5_System_GetNumDrivers_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_GetNumDrivers_Delegate>(nameof(FMOD5_System_GetNumDrivers));

        private delegate RESULT FMOD5_System_GetDriverInfo_Delegate(IntPtr system, int id, IntPtr name, int namelen, out Guid guid, out int systemrate, out SPEAKERMODE speakermode, out int speakermodechannels);
        private static FMOD5_System_GetDriverInfo_Delegate FMOD5_System_GetDriverInfo_Internal = null;
        private static FMOD5_System_GetDriverInfo_Delegate FMOD5_System_GetDriverInfo => FMOD5_System_GetDriverInfo_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_GetDriverInfo_Delegate>(nameof(FMOD5_System_GetDriverInfo));

        private delegate RESULT FMOD5_System_SetDriver_Delegate(IntPtr system, int driver);
        private static FMOD5_System_SetDriver_Delegate FMOD5_System_SetDriver_Internal = null;
        private static FMOD5_System_SetDriver_Delegate FMOD5_System_SetDriver => FMOD5_System_SetDriver_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_SetDriver_Delegate>(nameof(FMOD5_System_SetDriver));

        private delegate RESULT FMOD5_System_GetDriver_Delegate(IntPtr system, out int driver);
        private static FMOD5_System_GetDriver_Delegate FMOD5_System_GetDriver_Internal = null;
        private static FMOD5_System_GetDriver_Delegate FMOD5_System_GetDriver => FMOD5_System_GetDriver_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_GetDriver_Delegate>(nameof(FMOD5_System_GetDriver));

        private delegate RESULT FMOD5_System_SetSoftwareChannels_Delegate(IntPtr system, int numsoftwarechannels);
        private static FMOD5_System_SetSoftwareChannels_Delegate FMOD5_System_SetSoftwareChannels_Internal = null;
        private static FMOD5_System_SetSoftwareChannels_Delegate FMOD5_System_SetSoftwareChannels => FMOD5_System_SetSoftwareChannels_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_SetSoftwareChannels_Delegate>(nameof(FMOD5_System_SetSoftwareChannels));

        private delegate RESULT FMOD5_System_GetSoftwareChannels_Delegate(IntPtr system, out int numsoftwarechannels);
        private static FMOD5_System_GetSoftwareChannels_Delegate FMOD5_System_GetSoftwareChannels_Internal = null;
        private static FMOD5_System_GetSoftwareChannels_Delegate FMOD5_System_GetSoftwareChannels => FMOD5_System_GetSoftwareChannels_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_GetSoftwareChannels_Delegate>(nameof(FMOD5_System_GetSoftwareChannels));

        private delegate RESULT FMOD5_System_SetSoftwareFormat_Delegate(IntPtr system, int samplerate, SPEAKERMODE speakermode, int numrawspeakers);
        private static FMOD5_System_SetSoftwareFormat_Delegate FMOD5_System_SetSoftwareFormat_Internal = null;
        private static FMOD5_System_SetSoftwareFormat_Delegate FMOD5_System_SetSoftwareFormat => FMOD5_System_SetSoftwareFormat_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_SetSoftwareFormat_Delegate>(nameof(FMOD5_System_SetSoftwareFormat));

        private delegate RESULT FMOD5_System_GetSoftwareFormat_Delegate(IntPtr system, out int samplerate, out SPEAKERMODE speakermode, out int numrawspeakers);
        private static FMOD5_System_GetSoftwareFormat_Delegate FMOD5_System_GetSoftwareFormat_Internal = null;
        private static FMOD5_System_GetSoftwareFormat_Delegate FMOD5_System_GetSoftwareFormat => FMOD5_System_GetSoftwareFormat_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_GetSoftwareFormat_Delegate>(nameof(FMOD5_System_GetSoftwareFormat));

        private delegate RESULT FMOD5_System_SetDSPBufferSize_Delegate(IntPtr system, uint bufferlength, int numbuffers);
        private static FMOD5_System_SetDSPBufferSize_Delegate FMOD5_System_SetDSPBufferSize_Internal = null;
        private static FMOD5_System_SetDSPBufferSize_Delegate FMOD5_System_SetDSPBufferSize => FMOD5_System_SetDSPBufferSize_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_SetDSPBufferSize_Delegate>(nameof(FMOD5_System_SetDSPBufferSize));

        private delegate RESULT FMOD5_System_GetDSPBufferSize_Delegate(IntPtr system, out uint bufferlength, out int numbuffers);
        private static FMOD5_System_GetDSPBufferSize_Delegate FMOD5_System_GetDSPBufferSize_Internal = null;
        private static FMOD5_System_GetDSPBufferSize_Delegate FMOD5_System_GetDSPBufferSize => FMOD5_System_GetDSPBufferSize_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_GetDSPBufferSize_Delegate>(nameof(FMOD5_System_GetDSPBufferSize));

        private delegate RESULT FMOD5_System_SetFileSystem_Delegate(IntPtr system, FILE_OPEN_CALLBACK useropen, FILE_CLOSE_CALLBACK userclose, FILE_READ_CALLBACK userread, FILE_SEEK_CALLBACK userseek, FILE_ASYNCREAD_CALLBACK userasyncread, FILE_ASYNCCANCEL_CALLBACK userasynccancel, int blockalign);
        private static FMOD5_System_SetFileSystem_Delegate FMOD5_System_SetFileSystem_Internal = null;
        private static FMOD5_System_SetFileSystem_Delegate FMOD5_System_SetFileSystem => FMOD5_System_SetFileSystem_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_SetFileSystem_Delegate>(nameof(FMOD5_System_SetFileSystem));

        private delegate RESULT FMOD5_System_AttachFileSystem_Delegate(IntPtr system, FILE_OPEN_CALLBACK useropen, FILE_CLOSE_CALLBACK userclose, FILE_READ_CALLBACK userread, FILE_SEEK_CALLBACK userseek);
        private static FMOD5_System_AttachFileSystem_Delegate FMOD5_System_AttachFileSystem_Internal = null;
        private static FMOD5_System_AttachFileSystem_Delegate FMOD5_System_AttachFileSystem => FMOD5_System_AttachFileSystem_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_AttachFileSystem_Delegate>(nameof(FMOD5_System_AttachFileSystem));

        private delegate RESULT FMOD5_System_SetAdvancedSettings_Delegate(IntPtr system, ref ADVANCEDSETTINGS settings);
        private static FMOD5_System_SetAdvancedSettings_Delegate FMOD5_System_SetAdvancedSettings_Internal = null;
        private static FMOD5_System_SetAdvancedSettings_Delegate FMOD5_System_SetAdvancedSettings => FMOD5_System_SetAdvancedSettings_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_SetAdvancedSettings_Delegate>(nameof(FMOD5_System_SetAdvancedSettings));

        private delegate RESULT FMOD5_System_GetAdvancedSettings_Delegate(IntPtr system, ref ADVANCEDSETTINGS settings);
        private static FMOD5_System_GetAdvancedSettings_Delegate FMOD5_System_GetAdvancedSettings_Internal = null;
        private static FMOD5_System_GetAdvancedSettings_Delegate FMOD5_System_GetAdvancedSettings => FMOD5_System_GetAdvancedSettings_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_GetAdvancedSettings_Delegate>(nameof(FMOD5_System_GetAdvancedSettings));

        private delegate RESULT FMOD5_System_SetCallback_Delegate(IntPtr system, SYSTEM_CALLBACK callback, SYSTEM_CALLBACK_TYPE callbackmask);
        private static FMOD5_System_SetCallback_Delegate FMOD5_System_SetCallback_Internal = null;
        private static FMOD5_System_SetCallback_Delegate FMOD5_System_SetCallback => FMOD5_System_SetCallback_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_SetCallback_Delegate>(nameof(FMOD5_System_SetCallback));

        private delegate RESULT FMOD5_System_SetPluginPath_Delegate(IntPtr system, byte[] path);
        private static FMOD5_System_SetPluginPath_Delegate FMOD5_System_SetPluginPath_Internal = null;
        private static FMOD5_System_SetPluginPath_Delegate FMOD5_System_SetPluginPath => FMOD5_System_SetPluginPath_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_SetPluginPath_Delegate>(nameof(FMOD5_System_SetPluginPath));

        private delegate RESULT FMOD5_System_LoadPlugin_Delegate(IntPtr system, byte[] filename, out uint handle, uint priority);
        private static FMOD5_System_LoadPlugin_Delegate FMOD5_System_LoadPlugin_Internal = null;
        private static FMOD5_System_LoadPlugin_Delegate FMOD5_System_LoadPlugin => FMOD5_System_LoadPlugin_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_LoadPlugin_Delegate>(nameof(FMOD5_System_LoadPlugin));

        private delegate RESULT FMOD5_System_UnloadPlugin_Delegate(IntPtr system, uint handle);
        private static FMOD5_System_UnloadPlugin_Delegate FMOD5_System_UnloadPlugin_Internal = null;
        private static FMOD5_System_UnloadPlugin_Delegate FMOD5_System_UnloadPlugin => FMOD5_System_UnloadPlugin_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_UnloadPlugin_Delegate>(nameof(FMOD5_System_UnloadPlugin));

        private delegate RESULT FMOD5_System_GetNumNestedPlugins_Delegate(IntPtr system, uint handle, out int count);
        private static FMOD5_System_GetNumNestedPlugins_Delegate FMOD5_System_GetNumNestedPlugins_Internal = null;
        private static FMOD5_System_GetNumNestedPlugins_Delegate FMOD5_System_GetNumNestedPlugins => FMOD5_System_GetNumNestedPlugins_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_GetNumNestedPlugins_Delegate>(nameof(FMOD5_System_GetNumNestedPlugins));

        private delegate RESULT FMOD5_System_GetNestedPlugin_Delegate(IntPtr system, uint handle, int index, out uint nestedhandle);
        private static FMOD5_System_GetNestedPlugin_Delegate FMOD5_System_GetNestedPlugin_Internal = null;
        private static FMOD5_System_GetNestedPlugin_Delegate FMOD5_System_GetNestedPlugin => FMOD5_System_GetNestedPlugin_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_GetNestedPlugin_Delegate>(nameof(FMOD5_System_GetNestedPlugin));

        private delegate RESULT FMOD5_System_GetNumPlugins_Delegate(IntPtr system, PLUGINTYPE plugintype, out int numplugins);
        private static FMOD5_System_GetNumPlugins_Delegate FMOD5_System_GetNumPlugins_Internal = null;
        private static FMOD5_System_GetNumPlugins_Delegate FMOD5_System_GetNumPlugins => FMOD5_System_GetNumPlugins_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_GetNumPlugins_Delegate>(nameof(FMOD5_System_GetNumPlugins));

        private delegate RESULT FMOD5_System_GetPluginHandle_Delegate(IntPtr system, PLUGINTYPE plugintype, int index, out uint handle);
        private static FMOD5_System_GetPluginHandle_Delegate FMOD5_System_GetPluginHandle_Internal = null;
        private static FMOD5_System_GetPluginHandle_Delegate FMOD5_System_GetPluginHandle => FMOD5_System_GetPluginHandle_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_GetPluginHandle_Delegate>(nameof(FMOD5_System_GetPluginHandle));

        private delegate RESULT FMOD5_System_GetPluginInfo_Delegate(IntPtr system, uint handle, out PLUGINTYPE plugintype, IntPtr name, int namelen, out uint version);
        private static FMOD5_System_GetPluginInfo_Delegate FMOD5_System_GetPluginInfo_Internal = null;
        private static FMOD5_System_GetPluginInfo_Delegate FMOD5_System_GetPluginInfo => FMOD5_System_GetPluginInfo_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_GetPluginInfo_Delegate>(nameof(FMOD5_System_GetPluginInfo));

        private delegate RESULT FMOD5_System_SetOutputByPlugin_Delegate(IntPtr system, uint handle);
        private static FMOD5_System_SetOutputByPlugin_Delegate FMOD5_System_SetOutputByPlugin_Internal = null;
        private static FMOD5_System_SetOutputByPlugin_Delegate FMOD5_System_SetOutputByPlugin => FMOD5_System_SetOutputByPlugin_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_SetOutputByPlugin_Delegate>(nameof(FMOD5_System_SetOutputByPlugin));

        private delegate RESULT FMOD5_System_GetOutputByPlugin_Delegate(IntPtr system, out uint handle);
        private static FMOD5_System_GetOutputByPlugin_Delegate FMOD5_System_GetOutputByPlugin_Internal = null;
        private static FMOD5_System_GetOutputByPlugin_Delegate FMOD5_System_GetOutputByPlugin => FMOD5_System_GetOutputByPlugin_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_GetOutputByPlugin_Delegate>(nameof(FMOD5_System_GetOutputByPlugin));

        private delegate RESULT FMOD5_System_CreateDSPByPlugin_Delegate(IntPtr system, uint handle, out IntPtr dsp);
        private static FMOD5_System_CreateDSPByPlugin_Delegate FMOD5_System_CreateDSPByPlugin_Internal = null;
        private static FMOD5_System_CreateDSPByPlugin_Delegate FMOD5_System_CreateDSPByPlugin => FMOD5_System_CreateDSPByPlugin_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_CreateDSPByPlugin_Delegate>(nameof(FMOD5_System_CreateDSPByPlugin));

        private delegate RESULT FMOD5_System_GetDSPInfoByPlugin_Delegate(IntPtr system, uint handle, out IntPtr description);
        private static FMOD5_System_GetDSPInfoByPlugin_Delegate FMOD5_System_GetDSPInfoByPlugin_Internal = null;
        private static FMOD5_System_GetDSPInfoByPlugin_Delegate FMOD5_System_GetDSPInfoByPlugin => FMOD5_System_GetDSPInfoByPlugin_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_GetDSPInfoByPlugin_Delegate>(nameof(FMOD5_System_GetDSPInfoByPlugin));

        private delegate RESULT FMOD5_System_RegisterDSP_Delegate(IntPtr system, ref DSP_DESCRIPTION description, out uint handle);
        private static FMOD5_System_RegisterDSP_Delegate FMOD5_System_RegisterDSP_Internal = null;
        private static FMOD5_System_RegisterDSP_Delegate FMOD5_System_RegisterDSP => FMOD5_System_RegisterDSP_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_RegisterDSP_Delegate>(nameof(FMOD5_System_RegisterDSP));

        private delegate RESULT FMOD5_System_Init_Delegate(IntPtr system, int maxchannels, INITFLAGS flags, IntPtr extradriverdata);
        private static FMOD5_System_Init_Delegate FMOD5_System_Init_Internal = null;
        private static FMOD5_System_Init_Delegate FMOD5_System_Init => FMOD5_System_Init_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_Init_Delegate>(nameof(FMOD5_System_Init));

        private delegate RESULT FMOD5_System_Close_Delegate(IntPtr system);
        private static FMOD5_System_Close_Delegate FMOD5_System_Close_Internal = null;
        private static FMOD5_System_Close_Delegate FMOD5_System_Close => FMOD5_System_Close_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_Close_Delegate>(nameof(FMOD5_System_Close));

        private delegate RESULT FMOD5_System_Update_Delegate(IntPtr system);
        private static FMOD5_System_Update_Delegate FMOD5_System_Update_Internal = null;
        private static FMOD5_System_Update_Delegate FMOD5_System_Update => FMOD5_System_Update_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_Update_Delegate>(nameof(FMOD5_System_Update));

        private delegate RESULT FMOD5_System_SetSpeakerPosition_Delegate(IntPtr system, SPEAKER speaker, float x, float y, bool active);
        private static FMOD5_System_SetSpeakerPosition_Delegate FMOD5_System_SetSpeakerPosition_Internal = null;
        private static FMOD5_System_SetSpeakerPosition_Delegate FMOD5_System_SetSpeakerPosition => FMOD5_System_SetSpeakerPosition_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_SetSpeakerPosition_Delegate>(nameof(FMOD5_System_SetSpeakerPosition));

        private delegate RESULT FMOD5_System_GetSpeakerPosition_Delegate(IntPtr system, SPEAKER speaker, out float x, out float y, out bool active);
        private static FMOD5_System_GetSpeakerPosition_Delegate FMOD5_System_GetSpeakerPosition_Internal = null;
        private static FMOD5_System_GetSpeakerPosition_Delegate FMOD5_System_GetSpeakerPosition => FMOD5_System_GetSpeakerPosition_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_GetSpeakerPosition_Delegate>(nameof(FMOD5_System_GetSpeakerPosition));

        private delegate RESULT FMOD5_System_SetStreamBufferSize_Delegate(IntPtr system, uint filebuffersize, TIMEUNIT filebuffersizetype);
        private static FMOD5_System_SetStreamBufferSize_Delegate FMOD5_System_SetStreamBufferSize_Internal = null;
        private static FMOD5_System_SetStreamBufferSize_Delegate FMOD5_System_SetStreamBufferSize => FMOD5_System_SetStreamBufferSize_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_SetStreamBufferSize_Delegate>(nameof(FMOD5_System_SetStreamBufferSize));

        private delegate RESULT FMOD5_System_GetStreamBufferSize_Delegate(IntPtr system, out uint filebuffersize, out TIMEUNIT filebuffersizetype);
        private static FMOD5_System_GetStreamBufferSize_Delegate FMOD5_System_GetStreamBufferSize_Internal = null;
        private static FMOD5_System_GetStreamBufferSize_Delegate FMOD5_System_GetStreamBufferSize => FMOD5_System_GetStreamBufferSize_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_GetStreamBufferSize_Delegate>(nameof(FMOD5_System_GetStreamBufferSize));

        private delegate RESULT FMOD5_System_Set3DSettings_Delegate(IntPtr system, float dopplerscale, float distancefactor, float rolloffscale);
        private static FMOD5_System_Set3DSettings_Delegate FMOD5_System_Set3DSettings_Internal = null;
        private static FMOD5_System_Set3DSettings_Delegate FMOD5_System_Set3DSettings => FMOD5_System_Set3DSettings_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_Set3DSettings_Delegate>(nameof(FMOD5_System_Set3DSettings));

        private delegate RESULT FMOD5_System_Get3DSettings_Delegate(IntPtr system, out float dopplerscale, out float distancefactor, out float rolloffscale);
        private static FMOD5_System_Get3DSettings_Delegate FMOD5_System_Get3DSettings_Internal = null;
        private static FMOD5_System_Get3DSettings_Delegate FMOD5_System_Get3DSettings => FMOD5_System_Get3DSettings_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_Get3DSettings_Delegate>(nameof(FMOD5_System_Get3DSettings));

        private delegate RESULT FMOD5_System_Set3DNumListeners_Delegate(IntPtr system, int numlisteners);
        private static FMOD5_System_Set3DNumListeners_Delegate FMOD5_System_Set3DNumListeners_Internal = null;
        private static FMOD5_System_Set3DNumListeners_Delegate FMOD5_System_Set3DNumListeners => FMOD5_System_Set3DNumListeners_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_Set3DNumListeners_Delegate>(nameof(FMOD5_System_Set3DNumListeners));

        private delegate RESULT FMOD5_System_Get3DNumListeners_Delegate(IntPtr system, out int numlisteners);
        private static FMOD5_System_Get3DNumListeners_Delegate FMOD5_System_Get3DNumListeners_Internal = null;
        private static FMOD5_System_Get3DNumListeners_Delegate FMOD5_System_Get3DNumListeners => FMOD5_System_Get3DNumListeners_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_Get3DNumListeners_Delegate>(nameof(FMOD5_System_Get3DNumListeners));

        private delegate RESULT FMOD5_System_Set3DListenerAttributes_Delegate(IntPtr system, int listener, ref VECTOR pos, ref VECTOR vel, ref VECTOR forward, ref VECTOR up);
        private static FMOD5_System_Set3DListenerAttributes_Delegate FMOD5_System_Set3DListenerAttributes_Internal = null;
        private static FMOD5_System_Set3DListenerAttributes_Delegate FMOD5_System_Set3DListenerAttributes => FMOD5_System_Set3DListenerAttributes_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_Set3DListenerAttributes_Delegate>(nameof(FMOD5_System_Set3DListenerAttributes));

        private delegate RESULT FMOD5_System_Get3DListenerAttributes_Delegate(IntPtr system, int listener, out VECTOR pos, out VECTOR vel, out VECTOR forward, out VECTOR up);
        private static FMOD5_System_Get3DListenerAttributes_Delegate FMOD5_System_Get3DListenerAttributes_Internal = null;
        private static FMOD5_System_Get3DListenerAttributes_Delegate FMOD5_System_Get3DListenerAttributes => FMOD5_System_Get3DListenerAttributes_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_Get3DListenerAttributes_Delegate>(nameof(FMOD5_System_Get3DListenerAttributes));

        private delegate RESULT FMOD5_System_Set3DRolloffCallback_Delegate(IntPtr system, CB_3D_ROLLOFF_CALLBACK callback);
        private static FMOD5_System_Set3DRolloffCallback_Delegate FMOD5_System_Set3DRolloffCallback_Internal = null;
        private static FMOD5_System_Set3DRolloffCallback_Delegate FMOD5_System_Set3DRolloffCallback => FMOD5_System_Set3DRolloffCallback_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_Set3DRolloffCallback_Delegate>(nameof(FMOD5_System_Set3DRolloffCallback));

        private delegate RESULT FMOD5_System_MixerSuspend_Delegate(IntPtr system);
        private static FMOD5_System_MixerSuspend_Delegate FMOD5_System_MixerSuspend_Internal = null;
        private static FMOD5_System_MixerSuspend_Delegate FMOD5_System_MixerSuspend => FMOD5_System_MixerSuspend_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_MixerSuspend_Delegate>(nameof(FMOD5_System_MixerSuspend));

        private delegate RESULT FMOD5_System_MixerResume_Delegate(IntPtr system);
        private static FMOD5_System_MixerResume_Delegate FMOD5_System_MixerResume_Internal = null;
        private static FMOD5_System_MixerResume_Delegate FMOD5_System_MixerResume => FMOD5_System_MixerResume_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_MixerResume_Delegate>(nameof(FMOD5_System_MixerResume));

        private delegate RESULT FMOD5_System_GetDefaultMixMatrix_Delegate(IntPtr system, SPEAKERMODE sourcespeakermode, SPEAKERMODE targetspeakermode, float[] matrix, int matrixhop);
        private static FMOD5_System_GetDefaultMixMatrix_Delegate FMOD5_System_GetDefaultMixMatrix_Internal = null;
        private static FMOD5_System_GetDefaultMixMatrix_Delegate FMOD5_System_GetDefaultMixMatrix => FMOD5_System_GetDefaultMixMatrix_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_GetDefaultMixMatrix_Delegate>(nameof(FMOD5_System_GetDefaultMixMatrix));

        private delegate RESULT FMOD5_System_GetSpeakerModeChannels_Delegate(IntPtr system, SPEAKERMODE mode, out int channels);
        private static FMOD5_System_GetSpeakerModeChannels_Delegate FMOD5_System_GetSpeakerModeChannels_Internal = null;
        private static FMOD5_System_GetSpeakerModeChannels_Delegate FMOD5_System_GetSpeakerModeChannels => FMOD5_System_GetSpeakerModeChannels_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_GetSpeakerModeChannels_Delegate>(nameof(FMOD5_System_GetSpeakerModeChannels));

        private delegate RESULT FMOD5_System_GetVersion_Delegate(IntPtr system, out uint version);
        private static FMOD5_System_GetVersion_Delegate FMOD5_System_GetVersion_Internal = null;
        private static FMOD5_System_GetVersion_Delegate FMOD5_System_GetVersion => FMOD5_System_GetVersion_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_GetVersion_Delegate>(nameof(FMOD5_System_GetVersion));

        private delegate RESULT FMOD5_System_GetOutputHandle_Delegate(IntPtr system, out IntPtr handle);
        private static FMOD5_System_GetOutputHandle_Delegate FMOD5_System_GetOutputHandle_Internal = null;
        private static FMOD5_System_GetOutputHandle_Delegate FMOD5_System_GetOutputHandle => FMOD5_System_GetOutputHandle_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_GetOutputHandle_Delegate>(nameof(FMOD5_System_GetOutputHandle));

        private delegate RESULT FMOD5_System_GetChannelsPlaying_Delegate(IntPtr system, out int channels, IntPtr zero);
        private static FMOD5_System_GetChannelsPlaying_Delegate FMOD5_System_GetChannelsPlaying_Internal = null;
        private static FMOD5_System_GetChannelsPlaying_Delegate FMOD5_System_GetChannelsPlaying => FMOD5_System_GetChannelsPlaying_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_GetChannelsPlaying_Delegate>(nameof(FMOD5_System_GetChannelsPlaying));

        private delegate RESULT FMOD5_System_GetChannelsPlaying_Delegate2(IntPtr system, out int channels, out int realchannels);
        private static FMOD5_System_GetChannelsPlaying_Delegate2 FMOD5_System_GetChannelsPlaying_Internal2 = null;
        private static FMOD5_System_GetChannelsPlaying_Delegate2 FMOD5_System_GetChannelsPlaying2 => FMOD5_System_GetChannelsPlaying_Internal2 ??= FModManager.GetProcInFModStudio<FMOD5_System_GetChannelsPlaying_Delegate2>(nameof(FMOD5_System_GetChannelsPlaying));

        private delegate RESULT FMOD5_System_GetCPUUsage_Delegate(IntPtr system, out CPU_USAGE usage);
        private static FMOD5_System_GetCPUUsage_Delegate FMOD5_System_GetCPUUsage_Internal = null;
        private static FMOD5_System_GetCPUUsage_Delegate FMOD5_System_GetCPUUsage => FMOD5_System_GetCPUUsage_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_GetCPUUsage_Delegate>(nameof(FMOD5_System_GetCPUUsage));

        private delegate RESULT FMOD5_System_GetFileUsage_Delegate(IntPtr system, out Int64 sampleBytesRead, out Int64 streamBytesRead, out Int64 otherBytesRead);
        private static FMOD5_System_GetFileUsage_Delegate FMOD5_System_GetFileUsage_Internal = null;
        private static FMOD5_System_GetFileUsage_Delegate FMOD5_System_GetFileUsage => FMOD5_System_GetFileUsage_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_GetFileUsage_Delegate>(nameof(FMOD5_System_GetFileUsage));

        private delegate RESULT FMOD5_System_CreateSound_Delegate(IntPtr system, byte[] name_or_data, MODE mode, ref CREATESOUNDEXINFO exinfo, out IntPtr sound);
        private static FMOD5_System_CreateSound_Delegate FMOD5_System_CreateSound_Internal = null;
        private static FMOD5_System_CreateSound_Delegate FMOD5_System_CreateSound => FMOD5_System_CreateSound_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_CreateSound_Delegate>(nameof(FMOD5_System_CreateSound));

        private delegate RESULT FMOD5_System_CreateSound_Delegate2(IntPtr system, IntPtr name_or_data, MODE mode, ref CREATESOUNDEXINFO exinfo, out IntPtr sound);
        private static FMOD5_System_CreateSound_Delegate2 FMOD5_System_CreateSound_Internal2 = null;
        private static FMOD5_System_CreateSound_Delegate2 FMOD5_System_CreateSound2 => FMOD5_System_CreateSound_Internal2 ??= FModManager.GetProcInFModStudio<FMOD5_System_CreateSound_Delegate2>(nameof(FMOD5_System_CreateSound));

        private delegate RESULT FMOD5_System_CreateStream_Delegate(IntPtr system, byte[] name_or_data, MODE mode, ref CREATESOUNDEXINFO exinfo, out IntPtr sound);
        private static FMOD5_System_CreateStream_Delegate FMOD5_System_CreateStream_Internal = null;
        private static FMOD5_System_CreateStream_Delegate FMOD5_System_CreateStream => FMOD5_System_CreateStream_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_CreateStream_Delegate>(nameof(FMOD5_System_CreateStream));

        private delegate RESULT FMOD5_System_CreateStream_Delegate2(IntPtr system, IntPtr name_or_data, MODE mode, ref CREATESOUNDEXINFO exinfo, out IntPtr sound);
        private static FMOD5_System_CreateStream_Delegate2 FMOD5_System_CreateStream_Internal2 = null;
        private static FMOD5_System_CreateStream_Delegate2 FMOD5_System_CreateStream2 => FMOD5_System_CreateStream_Internal2 ??= FModManager.GetProcInFModStudio<FMOD5_System_CreateStream_Delegate2>(nameof(FMOD5_System_CreateStream));

        private delegate RESULT FMOD5_System_CreateDSP_Delegate(IntPtr system, ref DSP_DESCRIPTION description, out IntPtr dsp);
        private static FMOD5_System_CreateDSP_Delegate FMOD5_System_CreateDSP_Internal = null;
        private static FMOD5_System_CreateDSP_Delegate FMOD5_System_CreateDSP => FMOD5_System_CreateDSP_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_CreateDSP_Delegate>(nameof(FMOD5_System_CreateDSP));

        private delegate RESULT FMOD5_System_CreateDSPByType_Delegate(IntPtr system, DSP_TYPE type, out IntPtr dsp);
        private static FMOD5_System_CreateDSPByType_Delegate FMOD5_System_CreateDSPByType_Internal = null;
        private static FMOD5_System_CreateDSPByType_Delegate FMOD5_System_CreateDSPByType => FMOD5_System_CreateDSPByType_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_CreateDSPByType_Delegate>(nameof(FMOD5_System_CreateDSPByType));

        private delegate RESULT FMOD5_System_CreateChannelGroup_Delegate(IntPtr system, byte[] name, out IntPtr channelgroup);
        private static FMOD5_System_CreateChannelGroup_Delegate FMOD5_System_CreateChannelGroup_Internal = null;
        private static FMOD5_System_CreateChannelGroup_Delegate FMOD5_System_CreateChannelGroup => FMOD5_System_CreateChannelGroup_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_CreateChannelGroup_Delegate>(nameof(FMOD5_System_CreateChannelGroup));

        private delegate RESULT FMOD5_System_CreateSoundGroup_Delegate(IntPtr system, byte[] name, out IntPtr soundgroup);
        private static FMOD5_System_CreateSoundGroup_Delegate FMOD5_System_CreateSoundGroup_Internal = null;
        private static FMOD5_System_CreateSoundGroup_Delegate FMOD5_System_CreateSoundGroup => FMOD5_System_CreateSoundGroup_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_CreateSoundGroup_Delegate>(nameof(FMOD5_System_CreateSoundGroup));

        private delegate RESULT FMOD5_System_CreateReverb3D_Delegate(IntPtr system, out IntPtr reverb);
        private static FMOD5_System_CreateReverb3D_Delegate FMOD5_System_CreateReverb3D_Internal = null;
        private static FMOD5_System_CreateReverb3D_Delegate FMOD5_System_CreateReverb3D => FMOD5_System_CreateReverb3D_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_CreateReverb3D_Delegate>(nameof(FMOD5_System_CreateReverb3D));

        private delegate RESULT FMOD5_System_PlaySound_Delegate(IntPtr system, IntPtr sound, IntPtr channelgroup, bool paused, out IntPtr channel);
        private static FMOD5_System_PlaySound_Delegate FMOD5_System_PlaySound_Internal = null;
        private static FMOD5_System_PlaySound_Delegate FMOD5_System_PlaySound => FMOD5_System_PlaySound_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_PlaySound_Delegate>(nameof(FMOD5_System_PlaySound));

        private delegate RESULT FMOD5_System_PlayDSP_Delegate(IntPtr system, IntPtr dsp, IntPtr channelgroup, bool paused, out IntPtr channel);
        private static FMOD5_System_PlayDSP_Delegate FMOD5_System_PlayDSP_Internal = null;
        private static FMOD5_System_PlayDSP_Delegate FMOD5_System_PlayDSP => FMOD5_System_PlayDSP_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_PlayDSP_Delegate>(nameof(FMOD5_System_PlayDSP));

        private delegate RESULT FMOD5_System_GetChannel_Delegate(IntPtr system, int channelid, out IntPtr channel);
        private static FMOD5_System_GetChannel_Delegate FMOD5_System_GetChannel_Internal = null;
        private static FMOD5_System_GetChannel_Delegate FMOD5_System_GetChannel => FMOD5_System_GetChannel_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_GetChannel_Delegate>(nameof(FMOD5_System_GetChannel));

        private delegate RESULT FMOD5_System_GetDSPInfoByType_Delegate(IntPtr system, DSP_TYPE type, out IntPtr description);
        private static FMOD5_System_GetDSPInfoByType_Delegate FMOD5_System_GetDSPInfoByType_Internal = null;
        private static FMOD5_System_GetDSPInfoByType_Delegate FMOD5_System_GetDSPInfoByType => FMOD5_System_GetDSPInfoByType_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_GetDSPInfoByType_Delegate>(nameof(FMOD5_System_GetDSPInfoByType));

        private delegate RESULT FMOD5_System_GetMasterChannelGroup_Delegate(IntPtr system, out IntPtr channelgroup);
        private static FMOD5_System_GetMasterChannelGroup_Delegate FMOD5_System_GetMasterChannelGroup_Internal = null;
        private static FMOD5_System_GetMasterChannelGroup_Delegate FMOD5_System_GetMasterChannelGroup => FMOD5_System_GetMasterChannelGroup_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_GetMasterChannelGroup_Delegate>(nameof(FMOD5_System_GetMasterChannelGroup));

        private delegate RESULT FMOD5_System_GetMasterSoundGroup_Delegate(IntPtr system, out IntPtr soundgroup);
        private static FMOD5_System_GetMasterSoundGroup_Delegate FMOD5_System_GetMasterSoundGroup_Internal = null;
        private static FMOD5_System_GetMasterSoundGroup_Delegate FMOD5_System_GetMasterSoundGroup => FMOD5_System_GetMasterSoundGroup_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_GetMasterSoundGroup_Delegate>(nameof(FMOD5_System_GetMasterSoundGroup));

        private delegate RESULT FMOD5_System_AttachChannelGroupToPort_Delegate(IntPtr system, PORT_TYPE portType, ulong portIndex, IntPtr channelgroup, bool passThru);
        private static FMOD5_System_AttachChannelGroupToPort_Delegate FMOD5_System_AttachChannelGroupToPort_Internal = null;
        private static FMOD5_System_AttachChannelGroupToPort_Delegate FMOD5_System_AttachChannelGroupToPort => FMOD5_System_AttachChannelGroupToPort_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_AttachChannelGroupToPort_Delegate>(nameof(FMOD5_System_AttachChannelGroupToPort));

        private delegate RESULT FMOD5_System_DetachChannelGroupFromPort_Delegate(IntPtr system, IntPtr channelgroup);
        private static FMOD5_System_DetachChannelGroupFromPort_Delegate FMOD5_System_DetachChannelGroupFromPort_Internal = null;
        private static FMOD5_System_DetachChannelGroupFromPort_Delegate FMOD5_System_DetachChannelGroupFromPort => FMOD5_System_DetachChannelGroupFromPort_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_DetachChannelGroupFromPort_Delegate>(nameof(FMOD5_System_DetachChannelGroupFromPort));

        private delegate RESULT FMOD5_System_SetReverbProperties_Delegate(IntPtr system, int instance, ref REVERB_PROPERTIES prop);
        private static FMOD5_System_SetReverbProperties_Delegate FMOD5_System_SetReverbProperties_Internal = null;
        private static FMOD5_System_SetReverbProperties_Delegate FMOD5_System_SetReverbProperties => FMOD5_System_SetReverbProperties_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_SetReverbProperties_Delegate>(nameof(FMOD5_System_SetReverbProperties));

        private delegate RESULT FMOD5_System_GetReverbProperties_Delegate(IntPtr system, int instance, out REVERB_PROPERTIES prop);
        private static FMOD5_System_GetReverbProperties_Delegate FMOD5_System_GetReverbProperties_Internal = null;
        private static FMOD5_System_GetReverbProperties_Delegate FMOD5_System_GetReverbProperties => FMOD5_System_GetReverbProperties_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_GetReverbProperties_Delegate>(nameof(FMOD5_System_GetReverbProperties));

        private delegate RESULT FMOD5_System_LockDSP_Delegate(IntPtr system);
        private static FMOD5_System_LockDSP_Delegate FMOD5_System_LockDSP_Internal = null;
        private static FMOD5_System_LockDSP_Delegate FMOD5_System_LockDSP => FMOD5_System_LockDSP_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_LockDSP_Delegate>(nameof(FMOD5_System_LockDSP));

        private delegate RESULT FMOD5_System_UnlockDSP_Delegate(IntPtr system);
        private static FMOD5_System_UnlockDSP_Delegate FMOD5_System_UnlockDSP_Internal = null;
        private static FMOD5_System_UnlockDSP_Delegate FMOD5_System_UnlockDSP => FMOD5_System_UnlockDSP_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_UnlockDSP_Delegate>(nameof(FMOD5_System_UnlockDSP));

        private delegate RESULT FMOD5_System_GetRecordNumDrivers_Delegate(IntPtr system, out int numdrivers, out int numconnected);
        private static FMOD5_System_GetRecordNumDrivers_Delegate FMOD5_System_GetRecordNumDrivers_Internal = null;
        private static FMOD5_System_GetRecordNumDrivers_Delegate FMOD5_System_GetRecordNumDrivers => FMOD5_System_GetRecordNumDrivers_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_GetRecordNumDrivers_Delegate>(nameof(FMOD5_System_GetRecordNumDrivers));

        private delegate RESULT FMOD5_System_GetRecordDriverInfo_Delegate(IntPtr system, int id, IntPtr name, int namelen, out Guid guid, out int systemrate, out SPEAKERMODE speakermode, out int speakermodechannels, out DRIVER_STATE state);
        private static FMOD5_System_GetRecordDriverInfo_Delegate FMOD5_System_GetRecordDriverInfo_Internal = null;
        private static FMOD5_System_GetRecordDriverInfo_Delegate FMOD5_System_GetRecordDriverInfo => FMOD5_System_GetRecordDriverInfo_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_GetRecordDriverInfo_Delegate>(nameof(FMOD5_System_GetRecordDriverInfo));

        private delegate RESULT FMOD5_System_GetRecordPosition_Delegate(IntPtr system, int id, out uint position);
        private static FMOD5_System_GetRecordPosition_Delegate FMOD5_System_GetRecordPosition_Internal = null;
        private static FMOD5_System_GetRecordPosition_Delegate FMOD5_System_GetRecordPosition => FMOD5_System_GetRecordPosition_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_GetRecordPosition_Delegate>(nameof(FMOD5_System_GetRecordPosition));

        private delegate RESULT FMOD5_System_RecordStart_Delegate(IntPtr system, int id, IntPtr sound, bool loop);
        private static FMOD5_System_RecordStart_Delegate FMOD5_System_RecordStart_Internal = null;
        private static FMOD5_System_RecordStart_Delegate FMOD5_System_RecordStart => FMOD5_System_RecordStart_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_RecordStart_Delegate>(nameof(FMOD5_System_RecordStart));

        private delegate RESULT FMOD5_System_RecordStop_Delegate(IntPtr system, int id);
        private static FMOD5_System_RecordStop_Delegate FMOD5_System_RecordStop_Internal = null;
        private static FMOD5_System_RecordStop_Delegate FMOD5_System_RecordStop => FMOD5_System_RecordStop_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_RecordStop_Delegate>(nameof(FMOD5_System_RecordStop));

        private delegate RESULT FMOD5_System_IsRecording_Delegate(IntPtr system, int id, out bool recording);
        private static FMOD5_System_IsRecording_Delegate FMOD5_System_IsRecording_Internal = null;
        private static FMOD5_System_IsRecording_Delegate FMOD5_System_IsRecording => FMOD5_System_IsRecording_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_IsRecording_Delegate>(nameof(FMOD5_System_IsRecording));

        private delegate RESULT FMOD5_System_CreateGeometry_Delegate(IntPtr system, int maxpolygons, int maxvertices, out IntPtr geometry);
        private static FMOD5_System_CreateGeometry_Delegate FMOD5_System_CreateGeometry_Internal = null;
        private static FMOD5_System_CreateGeometry_Delegate FMOD5_System_CreateGeometry => FMOD5_System_CreateGeometry_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_CreateGeometry_Delegate>(nameof(FMOD5_System_CreateGeometry));

        private delegate RESULT FMOD5_System_SetGeometrySettings_Delegate(IntPtr system, float maxworldsize);
        private static FMOD5_System_SetGeometrySettings_Delegate FMOD5_System_SetGeometrySettings_Internal = null;
        private static FMOD5_System_SetGeometrySettings_Delegate FMOD5_System_SetGeometrySettings => FMOD5_System_SetGeometrySettings_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_SetGeometrySettings_Delegate>(nameof(FMOD5_System_SetGeometrySettings));

        private delegate RESULT FMOD5_System_GetGeometrySettings_Delegate(IntPtr system, out float maxworldsize);
        private static FMOD5_System_GetGeometrySettings_Delegate FMOD5_System_GetGeometrySettings_Internal = null;
        private static FMOD5_System_GetGeometrySettings_Delegate FMOD5_System_GetGeometrySettings => FMOD5_System_GetGeometrySettings_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_GetGeometrySettings_Delegate>(nameof(FMOD5_System_GetGeometrySettings));

        private delegate RESULT FMOD5_System_LoadGeometry_Delegate(IntPtr system, IntPtr data, int datasize, out IntPtr geometry);
        private static FMOD5_System_LoadGeometry_Delegate FMOD5_System_LoadGeometry_Internal = null;
        private static FMOD5_System_LoadGeometry_Delegate FMOD5_System_LoadGeometry => FMOD5_System_LoadGeometry_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_LoadGeometry_Delegate>(nameof(FMOD5_System_LoadGeometry));

        private delegate RESULT FMOD5_System_GetGeometryOcclusion_Delegate(IntPtr system, ref VECTOR listener, ref VECTOR source, out float direct, out float reverb);
        private static FMOD5_System_GetGeometryOcclusion_Delegate FMOD5_System_GetGeometryOcclusion_Internal = null;
        private static FMOD5_System_GetGeometryOcclusion_Delegate FMOD5_System_GetGeometryOcclusion => FMOD5_System_GetGeometryOcclusion_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_GetGeometryOcclusion_Delegate>(nameof(FMOD5_System_GetGeometryOcclusion));

        private delegate RESULT FMOD5_System_SetNetworkProxy_Delegate(IntPtr system, byte[] proxy);
        private static FMOD5_System_SetNetworkProxy_Delegate FMOD5_System_SetNetworkProxy_Internal = null;
        private static FMOD5_System_SetNetworkProxy_Delegate FMOD5_System_SetNetworkProxy => FMOD5_System_SetNetworkProxy_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_SetNetworkProxy_Delegate>(nameof(FMOD5_System_SetNetworkProxy));

        private delegate RESULT FMOD5_System_GetNetworkProxy_Delegate(IntPtr system, IntPtr proxy, int proxylen);
        private static FMOD5_System_GetNetworkProxy_Delegate FMOD5_System_GetNetworkProxy_Internal = null;
        private static FMOD5_System_GetNetworkProxy_Delegate FMOD5_System_GetNetworkProxy => FMOD5_System_GetNetworkProxy_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_GetNetworkProxy_Delegate>(nameof(FMOD5_System_GetNetworkProxy));

        private delegate RESULT FMOD5_System_SetNetworkTimeout_Delegate(IntPtr system, int timeout);
        private static FMOD5_System_SetNetworkTimeout_Delegate FMOD5_System_SetNetworkTimeout_Internal = null;
        private static FMOD5_System_SetNetworkTimeout_Delegate FMOD5_System_SetNetworkTimeout => FMOD5_System_SetNetworkTimeout_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_SetNetworkTimeout_Delegate>(nameof(FMOD5_System_SetNetworkTimeout));

        private delegate RESULT FMOD5_System_GetNetworkTimeout_Delegate(IntPtr system, out int timeout);
        private static FMOD5_System_GetNetworkTimeout_Delegate FMOD5_System_GetNetworkTimeout_Internal = null;
        private static FMOD5_System_GetNetworkTimeout_Delegate FMOD5_System_GetNetworkTimeout => FMOD5_System_GetNetworkTimeout_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_GetNetworkTimeout_Delegate>(nameof(FMOD5_System_GetNetworkTimeout));

        private delegate RESULT FMOD5_System_SetUserData_Delegate(IntPtr system, IntPtr userdata);
        private static FMOD5_System_SetUserData_Delegate FMOD5_System_SetUserData_Internal = null;
        private static FMOD5_System_SetUserData_Delegate FMOD5_System_SetUserData => FMOD5_System_SetUserData_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_SetUserData_Delegate>(nameof(FMOD5_System_SetUserData));

        private delegate RESULT FMOD5_System_GetUserData_Delegate(IntPtr system, out IntPtr userdata);
        private static FMOD5_System_GetUserData_Delegate FMOD5_System_GetUserData_Internal = null;
        private static FMOD5_System_GetUserData_Delegate FMOD5_System_GetUserData => FMOD5_System_GetUserData_Internal ??= FModManager.GetProcInFModStudio<FMOD5_System_GetUserData_Delegate>(nameof(FMOD5_System_GetUserData));

        #endregion

        #region wrapperinternal

        public IntPtr handle;

        public System(IntPtr ptr)   { this.handle = ptr; }
        public bool hasHandle()     { return this.handle != IntPtr.Zero; }
        public void clearHandle()   { this.handle = IntPtr.Zero; }

        #endregion
    }


    /*
        'Sound' API.
    */
    public struct Sound
    {
        public RESULT release()
        {
            return FMOD5_Sound_Release(this.handle);
        }
        public RESULT getSystemObject(out System system)
        {
            return FMOD5_Sound_GetSystemObject(this.handle, out system.handle);
        }

        // Standard sound manipulation functions.
        public RESULT @lock(uint offset, uint length, out IntPtr ptr1, out IntPtr ptr2, out uint len1, out uint len2)
        {
            return FMOD5_Sound_Lock(this.handle, offset, length, out ptr1, out ptr2, out len1, out len2);
        }
        public RESULT unlock(IntPtr ptr1, IntPtr ptr2, uint len1, uint len2)
        {
            return FMOD5_Sound_Unlock(this.handle, ptr1, ptr2, len1, len2);
        }
        public RESULT setDefaults(float frequency, int priority)
        {
            return FMOD5_Sound_SetDefaults(this.handle, frequency, priority);
        }
        public RESULT getDefaults(out float frequency, out int priority)
        {
            return FMOD5_Sound_GetDefaults(this.handle, out frequency, out priority);
        }
        public RESULT set3DMinMaxDistance(float min, float max)
        {
            return FMOD5_Sound_Set3DMinMaxDistance(this.handle, min, max);
        }
        public RESULT get3DMinMaxDistance(out float min, out float max)
        {
            return FMOD5_Sound_Get3DMinMaxDistance(this.handle, out min, out max);
        }
        public RESULT set3DConeSettings(float insideconeangle, float outsideconeangle, float outsidevolume)
        {
            return FMOD5_Sound_Set3DConeSettings(this.handle, insideconeangle, outsideconeangle, outsidevolume);
        }
        public RESULT get3DConeSettings(out float insideconeangle, out float outsideconeangle, out float outsidevolume)
        {
            return FMOD5_Sound_Get3DConeSettings(this.handle, out insideconeangle, out outsideconeangle, out outsidevolume);
        }
        public RESULT set3DCustomRolloff(ref VECTOR points, int numpoints)
        {
            return FMOD5_Sound_Set3DCustomRolloff(this.handle, ref points, numpoints);
        }
        public RESULT get3DCustomRolloff(out IntPtr points, out int numpoints)
        {
            return FMOD5_Sound_Get3DCustomRolloff(this.handle, out points, out numpoints);
        }

        public RESULT getSubSound(int index, out Sound subsound)
        {
            return FMOD5_Sound_GetSubSound(this.handle, index, out subsound.handle);
        }
        public RESULT getSubSoundParent(out Sound parentsound)
        {
            return FMOD5_Sound_GetSubSoundParent(this.handle, out parentsound.handle);
        }
        public RESULT getName(out string name, int namelen)
        {
            IntPtr stringMem = Marshal.AllocHGlobal(namelen);

            RESULT result = FMOD5_Sound_GetName(this.handle, stringMem, namelen);
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                name = encoder.stringFromNative(stringMem);
            }
            Marshal.FreeHGlobal(stringMem);

            return result;
        }
        public RESULT getLength(out uint length, TIMEUNIT lengthtype)
        {
            return FMOD5_Sound_GetLength(this.handle, out length, lengthtype);
        }
        public RESULT getFormat(out SOUND_TYPE type, out SOUND_FORMAT format, out int channels, out int bits)
        {
            return FMOD5_Sound_GetFormat(this.handle, out type, out format, out channels, out bits);
        }
        public RESULT getNumSubSounds(out int numsubsounds)
        {
            return FMOD5_Sound_GetNumSubSounds(this.handle, out numsubsounds);
        }
        public RESULT getNumTags(out int numtags, out int numtagsupdated)
        {
            return FMOD5_Sound_GetNumTags(this.handle, out numtags, out numtagsupdated);
        }
        public RESULT getTag(string name, int index, out TAG tag)
        {
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                return FMOD5_Sound_GetTag(this.handle, encoder.byteFromStringUTF8(name), index, out tag);
            }
        }
        public RESULT getOpenState(out OPENSTATE openstate, out uint percentbuffered, out bool starving, out bool diskbusy)
        {
            return FMOD5_Sound_GetOpenState(this.handle, out openstate, out percentbuffered, out starving, out diskbusy);
        }
        public RESULT readData(byte[] buffer)
        {
            return FMOD5_Sound_ReadData(this.handle, buffer, (uint)buffer.Length, IntPtr.Zero);
        }
        public RESULT readData(byte[] buffer, out uint read)
        {
            return FMOD5_Sound_ReadData2(this.handle, buffer, (uint)buffer.Length, out read);
        }
        [Obsolete("Use Sound.readData(byte[], out uint) or Sound.readData(byte[]) instead.")]
        public RESULT readData(IntPtr buffer, uint length, out uint read)
        {
            return FMOD5_Sound_ReadData3(this.handle, buffer, length, out read);
        }
        public RESULT seekData(uint pcm)
        {
            return FMOD5_Sound_SeekData(this.handle, pcm);
        }
        public RESULT setSoundGroup(SoundGroup soundgroup)
        {
            return FMOD5_Sound_SetSoundGroup(this.handle, soundgroup.handle);
        }
        public RESULT getSoundGroup(out SoundGroup soundgroup)
        {
            return FMOD5_Sound_GetSoundGroup(this.handle, out soundgroup.handle);
        }

        // Synchronization point API.  These points can come from markers embedded in wav files, and can also generate channel callbacks.
        public RESULT getNumSyncPoints(out int numsyncpoints)
        {
            return FMOD5_Sound_GetNumSyncPoints(this.handle, out numsyncpoints);
        }
        public RESULT getSyncPoint(int index, out IntPtr point)
        {
            return FMOD5_Sound_GetSyncPoint(this.handle, index, out point);
        }
        public RESULT getSyncPointInfo(IntPtr point, out string name, int namelen, out uint offset, TIMEUNIT offsettype)
        {
            IntPtr stringMem = Marshal.AllocHGlobal(namelen);

            RESULT result = FMOD5_Sound_GetSyncPointInfo(this.handle, point, stringMem, namelen, out offset, offsettype);
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                name = encoder.stringFromNative(stringMem);
            }
            Marshal.FreeHGlobal(stringMem);

            return result;
        }
        public RESULT getSyncPointInfo(IntPtr point, out uint offset, TIMEUNIT offsettype)
        {
            return FMOD5_Sound_GetSyncPointInfo(this.handle, point, IntPtr.Zero, 0, out offset, offsettype);
        }
        public RESULT addSyncPoint(uint offset, TIMEUNIT offsettype, string name, out IntPtr point)
        {
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                return FMOD5_Sound_AddSyncPoint(this.handle, offset, offsettype, encoder.byteFromStringUTF8(name), out point);
            }
        }
        public RESULT deleteSyncPoint(IntPtr point)
        {
            return FMOD5_Sound_DeleteSyncPoint(this.handle, point);
        }

        // Functions also in Channel class but here they are the 'default' to save having to change it in Channel all the time.
        public RESULT setMode(MODE mode)
        {
            return FMOD5_Sound_SetMode(this.handle, mode);
        }
        public RESULT getMode(out MODE mode)
        {
            return FMOD5_Sound_GetMode(this.handle, out mode);
        }
        public RESULT setLoopCount(int loopcount)
        {
            return FMOD5_Sound_SetLoopCount(this.handle, loopcount);
        }
        public RESULT getLoopCount(out int loopcount)
        {
            return FMOD5_Sound_GetLoopCount(this.handle, out loopcount);
        }
        public RESULT setLoopPoints(uint loopstart, TIMEUNIT loopstarttype, uint loopend, TIMEUNIT loopendtype)
        {
            return FMOD5_Sound_SetLoopPoints(this.handle, loopstart, loopstarttype, loopend, loopendtype);
        }
        public RESULT getLoopPoints(out uint loopstart, TIMEUNIT loopstarttype, out uint loopend, TIMEUNIT loopendtype)
        {
            return FMOD5_Sound_GetLoopPoints(this.handle, out loopstart, loopstarttype, out loopend, loopendtype);
        }

        // For MOD/S3M/XM/IT/MID sequenced formats only.
        public RESULT getMusicNumChannels(out int numchannels)
        {
            return FMOD5_Sound_GetMusicNumChannels(this.handle, out numchannels);
        }
        public RESULT setMusicChannelVolume(int channel, float volume)
        {
            return FMOD5_Sound_SetMusicChannelVolume(this.handle, channel, volume);
        }
        public RESULT getMusicChannelVolume(int channel, out float volume)
        {
            return FMOD5_Sound_GetMusicChannelVolume(this.handle, channel, out volume);
        }
        public RESULT setMusicSpeed(float speed)
        {
            return FMOD5_Sound_SetMusicSpeed(this.handle, speed);
        }
        public RESULT getMusicSpeed(out float speed)
        {
            return FMOD5_Sound_GetMusicSpeed(this.handle, out speed);
        }

        // Userdata set/get.
        public RESULT setUserData(IntPtr userdata)
        {
            return FMOD5_Sound_SetUserData(this.handle, userdata);
        }
        public RESULT getUserData(out IntPtr userdata)
        {
            return FMOD5_Sound_GetUserData(this.handle, out userdata);
        }

        #region importfunctions
        private delegate RESULT FMOD5_Sound_Release_Delegate(IntPtr sound);
        private static FMOD5_Sound_Release_Delegate FMOD5_Sound_Release_Internal = null;
        private static FMOD5_Sound_Release_Delegate FMOD5_Sound_Release => FMOD5_Sound_Release_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_Release_Delegate>(nameof(FMOD5_Sound_Release));

        private delegate RESULT FMOD5_Sound_GetSystemObject_Delegate(IntPtr sound, out IntPtr system);
        private static FMOD5_Sound_GetSystemObject_Delegate FMOD5_Sound_GetSystemObject_Internal = null;
        private static FMOD5_Sound_GetSystemObject_Delegate FMOD5_Sound_GetSystemObject => FMOD5_Sound_GetSystemObject_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_GetSystemObject_Delegate>(nameof(FMOD5_Sound_GetSystemObject));

        private delegate RESULT FMOD5_Sound_Lock_Delegate(IntPtr sound, uint offset, uint length, out IntPtr ptr1, out IntPtr ptr2, out uint len1, out uint len2);
        private static FMOD5_Sound_Lock_Delegate FMOD5_Sound_Lock_Internal = null;
        private static FMOD5_Sound_Lock_Delegate FMOD5_Sound_Lock => FMOD5_Sound_Lock_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_Lock_Delegate>(nameof(FMOD5_Sound_Lock));

        private delegate RESULT FMOD5_Sound_Unlock_Delegate(IntPtr sound, IntPtr ptr1,  IntPtr ptr2, uint len1, uint len2);
        private static FMOD5_Sound_Unlock_Delegate FMOD5_Sound_Unlock_Internal = null;
        private static FMOD5_Sound_Unlock_Delegate FMOD5_Sound_Unlock => FMOD5_Sound_Unlock_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_Unlock_Delegate>(nameof(FMOD5_Sound_Unlock));

        private delegate RESULT FMOD5_Sound_SetDefaults_Delegate(IntPtr sound, float frequency, int priority);
        private static FMOD5_Sound_SetDefaults_Delegate FMOD5_Sound_SetDefaults_Internal = null;
        private static FMOD5_Sound_SetDefaults_Delegate FMOD5_Sound_SetDefaults => FMOD5_Sound_SetDefaults_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_SetDefaults_Delegate>(nameof(FMOD5_Sound_SetDefaults));

        private delegate RESULT FMOD5_Sound_GetDefaults_Delegate(IntPtr sound, out float frequency, out int priority);
        private static FMOD5_Sound_GetDefaults_Delegate FMOD5_Sound_GetDefaults_Internal = null;
        private static FMOD5_Sound_GetDefaults_Delegate FMOD5_Sound_GetDefaults => FMOD5_Sound_GetDefaults_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_GetDefaults_Delegate>(nameof(FMOD5_Sound_GetDefaults));

        private delegate RESULT FMOD5_Sound_Set3DMinMaxDistance_Delegate(IntPtr sound, float min, float max);
        private static FMOD5_Sound_Set3DMinMaxDistance_Delegate FMOD5_Sound_Set3DMinMaxDistance_Internal = null;
        private static FMOD5_Sound_Set3DMinMaxDistance_Delegate FMOD5_Sound_Set3DMinMaxDistance => FMOD5_Sound_Set3DMinMaxDistance_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_Set3DMinMaxDistance_Delegate>(nameof(FMOD5_Sound_Set3DMinMaxDistance));

        private delegate RESULT FMOD5_Sound_Get3DMinMaxDistance_Delegate(IntPtr sound, out float min, out float max);
        private static FMOD5_Sound_Get3DMinMaxDistance_Delegate FMOD5_Sound_Get3DMinMaxDistance_Internal = null;
        private static FMOD5_Sound_Get3DMinMaxDistance_Delegate FMOD5_Sound_Get3DMinMaxDistance => FMOD5_Sound_Get3DMinMaxDistance_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_Get3DMinMaxDistance_Delegate>(nameof(FMOD5_Sound_Get3DMinMaxDistance));

        private delegate RESULT FMOD5_Sound_Set3DConeSettings_Delegate(IntPtr sound, float insideconeangle, float outsideconeangle, float outsidevolume);
        private static FMOD5_Sound_Set3DConeSettings_Delegate FMOD5_Sound_Set3DConeSettings_Internal = null;
        private static FMOD5_Sound_Set3DConeSettings_Delegate FMOD5_Sound_Set3DConeSettings => FMOD5_Sound_Set3DConeSettings_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_Set3DConeSettings_Delegate>(nameof(FMOD5_Sound_Set3DConeSettings));

        private delegate RESULT FMOD5_Sound_Get3DConeSettings_Delegate(IntPtr sound, out float insideconeangle, out float outsideconeangle, out float outsidevolume);
        private static FMOD5_Sound_Get3DConeSettings_Delegate FMOD5_Sound_Get3DConeSettings_Internal = null;
        private static FMOD5_Sound_Get3DConeSettings_Delegate FMOD5_Sound_Get3DConeSettings => FMOD5_Sound_Get3DConeSettings_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_Get3DConeSettings_Delegate>(nameof(FMOD5_Sound_Get3DConeSettings));

        private delegate RESULT FMOD5_Sound_Set3DCustomRolloff_Delegate(IntPtr sound, ref VECTOR points, int numpoints);
        private static FMOD5_Sound_Set3DCustomRolloff_Delegate FMOD5_Sound_Set3DCustomRolloff_Internal = null;
        private static FMOD5_Sound_Set3DCustomRolloff_Delegate FMOD5_Sound_Set3DCustomRolloff => FMOD5_Sound_Set3DCustomRolloff_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_Set3DCustomRolloff_Delegate>(nameof(FMOD5_Sound_Set3DCustomRolloff));

        private delegate RESULT FMOD5_Sound_Get3DCustomRolloff_Delegate(IntPtr sound, out IntPtr points, out int numpoints);
        private static FMOD5_Sound_Get3DCustomRolloff_Delegate FMOD5_Sound_Get3DCustomRolloff_Internal = null;
        private static FMOD5_Sound_Get3DCustomRolloff_Delegate FMOD5_Sound_Get3DCustomRolloff => FMOD5_Sound_Get3DCustomRolloff_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_Get3DCustomRolloff_Delegate>(nameof(FMOD5_Sound_Get3DCustomRolloff));

        private delegate RESULT FMOD5_Sound_GetSubSound_Delegate(IntPtr sound, int index, out IntPtr subsound);
        private static FMOD5_Sound_GetSubSound_Delegate FMOD5_Sound_GetSubSound_Internal = null;
        private static FMOD5_Sound_GetSubSound_Delegate FMOD5_Sound_GetSubSound => FMOD5_Sound_GetSubSound_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_GetSubSound_Delegate>(nameof(FMOD5_Sound_GetSubSound));

        private delegate RESULT FMOD5_Sound_GetSubSoundParent_Delegate(IntPtr sound, out IntPtr parentsound);
        private static FMOD5_Sound_GetSubSoundParent_Delegate FMOD5_Sound_GetSubSoundParent_Internal = null;
        private static FMOD5_Sound_GetSubSoundParent_Delegate FMOD5_Sound_GetSubSoundParent => FMOD5_Sound_GetSubSoundParent_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_GetSubSoundParent_Delegate>(nameof(FMOD5_Sound_GetSubSoundParent));

        private delegate RESULT FMOD5_Sound_GetName_Delegate(IntPtr sound, IntPtr name, int namelen);
        private static FMOD5_Sound_GetName_Delegate FMOD5_Sound_GetName_Internal = null;
        private static FMOD5_Sound_GetName_Delegate FMOD5_Sound_GetName => FMOD5_Sound_GetName_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_GetName_Delegate>(nameof(FMOD5_Sound_GetName));

        private delegate RESULT FMOD5_Sound_GetLength_Delegate(IntPtr sound, out uint length, TIMEUNIT lengthtype);
        private static FMOD5_Sound_GetLength_Delegate FMOD5_Sound_GetLength_Internal = null;
        private static FMOD5_Sound_GetLength_Delegate FMOD5_Sound_GetLength => FMOD5_Sound_GetLength_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_GetLength_Delegate>(nameof(FMOD5_Sound_GetLength));

        private delegate RESULT FMOD5_Sound_GetFormat_Delegate(IntPtr sound, out SOUND_TYPE type, out SOUND_FORMAT format, out int channels, out int bits);
        private static FMOD5_Sound_GetFormat_Delegate FMOD5_Sound_GetFormat_Internal = null;
        private static FMOD5_Sound_GetFormat_Delegate FMOD5_Sound_GetFormat => FMOD5_Sound_GetFormat_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_GetFormat_Delegate>(nameof(FMOD5_Sound_GetFormat));

        private delegate RESULT FMOD5_Sound_GetNumSubSounds_Delegate(IntPtr sound, out int numsubsounds);
        private static FMOD5_Sound_GetNumSubSounds_Delegate FMOD5_Sound_GetNumSubSounds_Internal = null;
        private static FMOD5_Sound_GetNumSubSounds_Delegate FMOD5_Sound_GetNumSubSounds => FMOD5_Sound_GetNumSubSounds_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_GetNumSubSounds_Delegate>(nameof(FMOD5_Sound_GetNumSubSounds));

        private delegate RESULT FMOD5_Sound_GetNumTags_Delegate(IntPtr sound, out int numtags, out int numtagsupdated);
        private static FMOD5_Sound_GetNumTags_Delegate FMOD5_Sound_GetNumTags_Internal = null;
        private static FMOD5_Sound_GetNumTags_Delegate FMOD5_Sound_GetNumTags => FMOD5_Sound_GetNumTags_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_GetNumTags_Delegate>(nameof(FMOD5_Sound_GetNumTags));

        private delegate RESULT FMOD5_Sound_GetTag_Delegate(IntPtr sound, byte[] name, int index, out TAG tag);
        private static FMOD5_Sound_GetTag_Delegate FMOD5_Sound_GetTag_Internal = null;
        private static FMOD5_Sound_GetTag_Delegate FMOD5_Sound_GetTag => FMOD5_Sound_GetTag_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_GetTag_Delegate>(nameof(FMOD5_Sound_GetTag));

        private delegate RESULT FMOD5_Sound_GetOpenState_Delegate(IntPtr sound, out OPENSTATE openstate, out uint percentbuffered, out bool starving, out bool diskbusy);
        private static FMOD5_Sound_GetOpenState_Delegate FMOD5_Sound_GetOpenState_Internal = null;
        private static FMOD5_Sound_GetOpenState_Delegate FMOD5_Sound_GetOpenState => FMOD5_Sound_GetOpenState_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_GetOpenState_Delegate>(nameof(FMOD5_Sound_GetOpenState));

        private delegate RESULT FMOD5_Sound_ReadData_Delegate(IntPtr sound, byte[] buffer, uint length, IntPtr zero);
        private static FMOD5_Sound_ReadData_Delegate FMOD5_Sound_ReadData_Internal = null;
        private static FMOD5_Sound_ReadData_Delegate FMOD5_Sound_ReadData => FMOD5_Sound_ReadData_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_ReadData_Delegate>(nameof(FMOD5_Sound_ReadData));

        private delegate RESULT FMOD5_Sound_ReadData_Delegate2(IntPtr sound, byte[] buffer, uint length, out uint read);
        private static FMOD5_Sound_ReadData_Delegate2 FMOD5_Sound_ReadData_Internal2 = null;
        private static FMOD5_Sound_ReadData_Delegate2 FMOD5_Sound_ReadData2 => FMOD5_Sound_ReadData_Internal2 ??= FModManager.GetProcInFModStudio<FMOD5_Sound_ReadData_Delegate2>(nameof(FMOD5_Sound_ReadData));

        private delegate RESULT FMOD5_Sound_ReadData_Delegate3(IntPtr sound, IntPtr buffer, uint length, out uint read);
        private static FMOD5_Sound_ReadData_Delegate3 FMOD5_Sound_ReadData_Internal3 = null;
        private static FMOD5_Sound_ReadData_Delegate3 FMOD5_Sound_ReadData3 => FMOD5_Sound_ReadData_Internal3 ??= FModManager.GetProcInFModStudio<FMOD5_Sound_ReadData_Delegate3>(nameof(FMOD5_Sound_ReadData));

        private delegate RESULT FMOD5_Sound_SeekData_Delegate(IntPtr sound, uint pcm);
        private static FMOD5_Sound_SeekData_Delegate FMOD5_Sound_SeekData_Internal = null;
        private static FMOD5_Sound_SeekData_Delegate FMOD5_Sound_SeekData => FMOD5_Sound_SeekData_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_SeekData_Delegate>(nameof(FMOD5_Sound_SeekData));

        private delegate RESULT FMOD5_Sound_SetSoundGroup_Delegate(IntPtr sound, IntPtr soundgroup);
        private static FMOD5_Sound_SetSoundGroup_Delegate FMOD5_Sound_SetSoundGroup_Internal = null;
        private static FMOD5_Sound_SetSoundGroup_Delegate FMOD5_Sound_SetSoundGroup => FMOD5_Sound_SetSoundGroup_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_SetSoundGroup_Delegate>(nameof(FMOD5_Sound_SetSoundGroup));

        private delegate RESULT FMOD5_Sound_GetSoundGroup_Delegate(IntPtr sound, out IntPtr soundgroup);
        private static FMOD5_Sound_GetSoundGroup_Delegate FMOD5_Sound_GetSoundGroup_Internal = null;
        private static FMOD5_Sound_GetSoundGroup_Delegate FMOD5_Sound_GetSoundGroup => FMOD5_Sound_GetSoundGroup_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_GetSoundGroup_Delegate>(nameof(FMOD5_Sound_GetSoundGroup));

        private delegate RESULT FMOD5_Sound_GetNumSyncPoints_Delegate(IntPtr sound, out int numsyncpoints);
        private static FMOD5_Sound_GetNumSyncPoints_Delegate FMOD5_Sound_GetNumSyncPoints_Internal = null;
        private static FMOD5_Sound_GetNumSyncPoints_Delegate FMOD5_Sound_GetNumSyncPoints => FMOD5_Sound_GetNumSyncPoints_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_GetNumSyncPoints_Delegate>(nameof(FMOD5_Sound_GetNumSyncPoints));

        private delegate RESULT FMOD5_Sound_GetSyncPoint_Delegate(IntPtr sound, int index, out IntPtr point);
        private static FMOD5_Sound_GetSyncPoint_Delegate FMOD5_Sound_GetSyncPoint_Internal = null;
        private static FMOD5_Sound_GetSyncPoint_Delegate FMOD5_Sound_GetSyncPoint => FMOD5_Sound_GetSyncPoint_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_GetSyncPoint_Delegate>(nameof(FMOD5_Sound_GetSyncPoint));

        private delegate RESULT FMOD5_Sound_GetSyncPointInfo_Delegate(IntPtr sound, IntPtr point, IntPtr name, int namelen, out uint offset, TIMEUNIT offsettype);
        private static FMOD5_Sound_GetSyncPointInfo_Delegate FMOD5_Sound_GetSyncPointInfo_Internal = null;
        private static FMOD5_Sound_GetSyncPointInfo_Delegate FMOD5_Sound_GetSyncPointInfo => FMOD5_Sound_GetSyncPointInfo_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_GetSyncPointInfo_Delegate>(nameof(FMOD5_Sound_GetSyncPointInfo));

        private delegate RESULT FMOD5_Sound_AddSyncPoint_Delegate(IntPtr sound, uint offset, TIMEUNIT offsettype, byte[] name, out IntPtr point);
        private static FMOD5_Sound_AddSyncPoint_Delegate FMOD5_Sound_AddSyncPoint_Internal = null;
        private static FMOD5_Sound_AddSyncPoint_Delegate FMOD5_Sound_AddSyncPoint => FMOD5_Sound_AddSyncPoint_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_AddSyncPoint_Delegate>(nameof(FMOD5_Sound_AddSyncPoint));

        private delegate RESULT FMOD5_Sound_DeleteSyncPoint_Delegate(IntPtr sound, IntPtr point);
        private static FMOD5_Sound_DeleteSyncPoint_Delegate FMOD5_Sound_DeleteSyncPoint_Internal = null;
        private static FMOD5_Sound_DeleteSyncPoint_Delegate FMOD5_Sound_DeleteSyncPoint => FMOD5_Sound_DeleteSyncPoint_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_DeleteSyncPoint_Delegate>(nameof(FMOD5_Sound_DeleteSyncPoint));

        private delegate RESULT FMOD5_Sound_SetMode_Delegate(IntPtr sound, MODE mode);
        private static FMOD5_Sound_SetMode_Delegate FMOD5_Sound_SetMode_Internal = null;
        private static FMOD5_Sound_SetMode_Delegate FMOD5_Sound_SetMode => FMOD5_Sound_SetMode_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_SetMode_Delegate>(nameof(FMOD5_Sound_SetMode));

        private delegate RESULT FMOD5_Sound_GetMode_Delegate(IntPtr sound, out MODE mode);
        private static FMOD5_Sound_GetMode_Delegate FMOD5_Sound_GetMode_Internal = null;
        private static FMOD5_Sound_GetMode_Delegate FMOD5_Sound_GetMode => FMOD5_Sound_GetMode_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_GetMode_Delegate>(nameof(FMOD5_Sound_GetMode));

        private delegate RESULT FMOD5_Sound_SetLoopCount_Delegate(IntPtr sound, int loopcount);
        private static FMOD5_Sound_SetLoopCount_Delegate FMOD5_Sound_SetLoopCount_Internal = null;
        private static FMOD5_Sound_SetLoopCount_Delegate FMOD5_Sound_SetLoopCount => FMOD5_Sound_SetLoopCount_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_SetLoopCount_Delegate>(nameof(FMOD5_Sound_SetLoopCount));

        private delegate RESULT FMOD5_Sound_GetLoopCount_Delegate(IntPtr sound, out int loopcount);
        private static FMOD5_Sound_GetLoopCount_Delegate FMOD5_Sound_GetLoopCount_Internal = null;
        private static FMOD5_Sound_GetLoopCount_Delegate FMOD5_Sound_GetLoopCount => FMOD5_Sound_GetLoopCount_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_GetLoopCount_Delegate>(nameof(FMOD5_Sound_GetLoopCount));

        private delegate RESULT FMOD5_Sound_SetLoopPoints_Delegate(IntPtr sound, uint loopstart, TIMEUNIT loopstarttype, uint loopend, TIMEUNIT loopendtype);
        private static FMOD5_Sound_SetLoopPoints_Delegate FMOD5_Sound_SetLoopPoints_Internal = null;
        private static FMOD5_Sound_SetLoopPoints_Delegate FMOD5_Sound_SetLoopPoints => FMOD5_Sound_SetLoopPoints_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_SetLoopPoints_Delegate>(nameof(FMOD5_Sound_SetLoopPoints));

        private delegate RESULT FMOD5_Sound_GetLoopPoints_Delegate(IntPtr sound, out uint loopstart, TIMEUNIT loopstarttype, out uint loopend, TIMEUNIT loopendtype);
        private static FMOD5_Sound_GetLoopPoints_Delegate FMOD5_Sound_GetLoopPoints_Internal = null;
        private static FMOD5_Sound_GetLoopPoints_Delegate FMOD5_Sound_GetLoopPoints => FMOD5_Sound_GetLoopPoints_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_GetLoopPoints_Delegate>(nameof(FMOD5_Sound_GetLoopPoints));

        private delegate RESULT FMOD5_Sound_GetMusicNumChannels_Delegate(IntPtr sound, out int numchannels);
        private static FMOD5_Sound_GetMusicNumChannels_Delegate FMOD5_Sound_GetMusicNumChannels_Internal = null;
        private static FMOD5_Sound_GetMusicNumChannels_Delegate FMOD5_Sound_GetMusicNumChannels => FMOD5_Sound_GetMusicNumChannels_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_GetMusicNumChannels_Delegate>(nameof(FMOD5_Sound_GetMusicNumChannels));

        private delegate RESULT FMOD5_Sound_SetMusicChannelVolume_Delegate(IntPtr sound, int channel, float volume);
        private static FMOD5_Sound_SetMusicChannelVolume_Delegate FMOD5_Sound_SetMusicChannelVolume_Internal = null;
        private static FMOD5_Sound_SetMusicChannelVolume_Delegate FMOD5_Sound_SetMusicChannelVolume => FMOD5_Sound_SetMusicChannelVolume_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_SetMusicChannelVolume_Delegate>(nameof(FMOD5_Sound_SetMusicChannelVolume));

        private delegate RESULT FMOD5_Sound_GetMusicChannelVolume_Delegate(IntPtr sound, int channel, out float volume);
        private static FMOD5_Sound_GetMusicChannelVolume_Delegate FMOD5_Sound_GetMusicChannelVolume_Internal = null;
        private static FMOD5_Sound_GetMusicChannelVolume_Delegate FMOD5_Sound_GetMusicChannelVolume => FMOD5_Sound_GetMusicChannelVolume_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_GetMusicChannelVolume_Delegate>(nameof(FMOD5_Sound_GetMusicChannelVolume));

        private delegate RESULT FMOD5_Sound_SetMusicSpeed_Delegate(IntPtr sound, float speed);
        private static FMOD5_Sound_SetMusicSpeed_Delegate FMOD5_Sound_SetMusicSpeed_Internal = null;
        private static FMOD5_Sound_SetMusicSpeed_Delegate FMOD5_Sound_SetMusicSpeed => FMOD5_Sound_SetMusicSpeed_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_SetMusicSpeed_Delegate>(nameof(FMOD5_Sound_SetMusicSpeed));

        private delegate RESULT FMOD5_Sound_GetMusicSpeed_Delegate(IntPtr sound, out float speed);
        private static FMOD5_Sound_GetMusicSpeed_Delegate FMOD5_Sound_GetMusicSpeed_Internal = null;
        private static FMOD5_Sound_GetMusicSpeed_Delegate FMOD5_Sound_GetMusicSpeed => FMOD5_Sound_GetMusicSpeed_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_GetMusicSpeed_Delegate>(nameof(FMOD5_Sound_GetMusicSpeed));

        private delegate RESULT FMOD5_Sound_SetUserData_Delegate(IntPtr sound, IntPtr userdata);
        private static FMOD5_Sound_SetUserData_Delegate FMOD5_Sound_SetUserData_Internal = null;
        private static FMOD5_Sound_SetUserData_Delegate FMOD5_Sound_SetUserData => FMOD5_Sound_SetUserData_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_SetUserData_Delegate>(nameof(FMOD5_Sound_SetUserData));

        private delegate RESULT FMOD5_Sound_GetUserData_Delegate(IntPtr sound, out IntPtr userdata);
        private static FMOD5_Sound_GetUserData_Delegate FMOD5_Sound_GetUserData_Internal = null;
        private static FMOD5_Sound_GetUserData_Delegate FMOD5_Sound_GetUserData => FMOD5_Sound_GetUserData_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Sound_GetUserData_Delegate>(nameof(FMOD5_Sound_GetUserData));

        #endregion

        #region wrapperinternal

        public IntPtr handle;

        public Sound(IntPtr ptr)    { this.handle = ptr; }
        public bool hasHandle()     { return this.handle != IntPtr.Zero; }
        public void clearHandle()   { this.handle = IntPtr.Zero; }

        #endregion
    }

    /*
        'ChannelControl' API
    */
    interface IChannelControl
    {
        RESULT getSystemObject              (out System system);

        // General control functionality for Channels and ChannelGroups.
        RESULT stop                         ();
        RESULT setPaused                    (bool paused);
        RESULT getPaused                    (out bool paused);
        RESULT setVolume                    (float volume);
        RESULT getVolume                    (out float volume);
        RESULT setVolumeRamp                (bool ramp);
        RESULT getVolumeRamp                (out bool ramp);
        RESULT getAudibility                (out float audibility);
        RESULT setPitch                     (float pitch);
        RESULT getPitch                     (out float pitch);
        RESULT setMute                      (bool mute);
        RESULT getMute                      (out bool mute);
        RESULT setReverbProperties          (int instance, float wet);
        RESULT getReverbProperties          (int instance, out float wet);
        RESULT setLowPassGain               (float gain);
        RESULT getLowPassGain               (out float gain);
        RESULT setMode                      (MODE mode);
        RESULT getMode                      (out MODE mode);
        RESULT setCallback                  (CHANNELCONTROL_CALLBACK callback);
        RESULT isPlaying                    (out bool isplaying);

        // Note all 'set' functions alter a final matrix, this is why the only get function is getMixMatrix, to avoid other get functions returning incorrect/obsolete values.
        RESULT setPan                       (float pan);
        RESULT setMixLevelsOutput           (float frontleft, float frontright, float center, float lfe, float surroundleft, float surroundright, float backleft, float backright);
        RESULT setMixLevelsInput            (float[] levels, int numlevels);
        RESULT setMixMatrix                 (float[] matrix, int outchannels, int inchannels, int inchannel_hop);
        RESULT getMixMatrix                 (float[] matrix, out int outchannels, out int inchannels, int inchannel_hop);

        // Clock based functionality.
        RESULT getDSPClock                  (out ulong dspclock, out ulong parentclock);
        RESULT setDelay                     (ulong dspclock_start, ulong dspclock_end, bool stopchannels);
        RESULT getDelay                     (out ulong dspclock_start, out ulong dspclock_end);
        RESULT getDelay                     (out ulong dspclock_start, out ulong dspclock_end, out bool stopchannels);
        RESULT addFadePoint                 (ulong dspclock, float volume);
        RESULT setFadePointRamp             (ulong dspclock, float volume);
        RESULT removeFadePoints             (ulong dspclock_start, ulong dspclock_end);
        RESULT getFadePoints                (ref uint numpoints, ulong[] point_dspclock, float[] point_volume);

        // DSP effects.
        RESULT getDSP                       (int index, out DSP dsp);
        RESULT addDSP                       (int index, DSP dsp);
        RESULT removeDSP                    (DSP dsp);
        RESULT getNumDSPs                   (out int numdsps);
        RESULT setDSPIndex                  (DSP dsp, int index);
        RESULT getDSPIndex                  (DSP dsp, out int index);

        // 3D functionality.
        RESULT set3DAttributes              (ref VECTOR pos, ref VECTOR vel);
        RESULT get3DAttributes              (out VECTOR pos, out VECTOR vel);
        RESULT set3DMinMaxDistance          (float mindistance, float maxdistance);
        RESULT get3DMinMaxDistance          (out float mindistance, out float maxdistance);
        RESULT set3DConeSettings            (float insideconeangle, float outsideconeangle, float outsidevolume);
        RESULT get3DConeSettings            (out float insideconeangle, out float outsideconeangle, out float outsidevolume);
        RESULT set3DConeOrientation         (ref VECTOR orientation);
        RESULT get3DConeOrientation         (out VECTOR orientation);
        RESULT set3DCustomRolloff           (ref VECTOR points, int numpoints);
        RESULT get3DCustomRolloff           (out IntPtr points, out int numpoints);
        RESULT set3DOcclusion               (float directocclusion, float reverbocclusion);
        RESULT get3DOcclusion               (out float directocclusion, out float reverbocclusion);
        RESULT set3DSpread                  (float angle);
        RESULT get3DSpread                  (out float angle);
        RESULT set3DLevel                   (float level);
        RESULT get3DLevel                   (out float level);
        RESULT set3DDopplerLevel            (float level);
        RESULT get3DDopplerLevel            (out float level);
        RESULT set3DDistanceFilter          (bool custom, float customLevel, float centerFreq);
        RESULT get3DDistanceFilter          (out bool custom, out float customLevel, out float centerFreq);

        // Userdata set/get.
        RESULT setUserData                  (IntPtr userdata);
        RESULT getUserData                  (out IntPtr userdata);
    }

    /*
        'Channel' API
    */
    public struct Channel : IChannelControl
    {
        // Channel specific control functionality.
        public RESULT setFrequency(float frequency)
        {
            return FMOD5_Channel_SetFrequency(this.handle, frequency);
        }
        public RESULT getFrequency(out float frequency)
        {
            return FMOD5_Channel_GetFrequency(this.handle, out frequency);
        }
        public RESULT setPriority(int priority)
        {
            return FMOD5_Channel_SetPriority(this.handle, priority);
        }
        public RESULT getPriority(out int priority)
        {
            return FMOD5_Channel_GetPriority(this.handle, out priority);
        }
        public RESULT setPosition(uint position, TIMEUNIT postype)
        {
            return FMOD5_Channel_SetPosition(this.handle, position, postype);
        }
        public RESULT getPosition(out uint position, TIMEUNIT postype)
        {
            return FMOD5_Channel_GetPosition(this.handle, out position, postype);
        }
        public RESULT setChannelGroup(ChannelGroup channelgroup)
        {
            return FMOD5_Channel_SetChannelGroup(this.handle, channelgroup.handle);
        }
        public RESULT getChannelGroup(out ChannelGroup channelgroup)
        {
            return FMOD5_Channel_GetChannelGroup(this.handle, out channelgroup.handle);
        }
        public RESULT setLoopCount(int loopcount)
        {
            return FMOD5_Channel_SetLoopCount(this.handle, loopcount);
        }
        public RESULT getLoopCount(out int loopcount)
        {
            return FMOD5_Channel_GetLoopCount(this.handle, out loopcount);
        }
        public RESULT setLoopPoints(uint loopstart, TIMEUNIT loopstarttype, uint loopend, TIMEUNIT loopendtype)
        {
            return FMOD5_Channel_SetLoopPoints(this.handle, loopstart, loopstarttype, loopend, loopendtype);
        }
        public RESULT getLoopPoints(out uint loopstart, TIMEUNIT loopstarttype, out uint loopend, TIMEUNIT loopendtype)
        {
            return FMOD5_Channel_GetLoopPoints(this.handle, out loopstart, loopstarttype, out loopend, loopendtype);
        }

        // Information only functions.
        public RESULT isVirtual(out bool isvirtual)
        {
            return FMOD5_Channel_IsVirtual(this.handle, out isvirtual);
        }
        public RESULT getCurrentSound(out Sound sound)
        {
            return FMOD5_Channel_GetCurrentSound(this.handle, out sound.handle);
        }
        public RESULT getIndex(out int index)
        {
            return FMOD5_Channel_GetIndex(this.handle, out index);
        }

        public RESULT getSystemObject(out System system)
        {
            return FMOD5_Channel_GetSystemObject(this.handle, out system.handle);
        }

        // General control functionality for Channels and ChannelGroups.
        public RESULT stop()
        {
            return FMOD5_Channel_Stop(this.handle);
        }
        public RESULT setPaused(bool paused)
        {
            return FMOD5_Channel_SetPaused(this.handle, paused);
        }
        public RESULT getPaused(out bool paused)
        {
            return FMOD5_Channel_GetPaused(this.handle, out paused);
        }
        public RESULT setVolume(float volume)
        {
            return FMOD5_Channel_SetVolume(this.handle, volume);
        }
        public RESULT getVolume(out float volume)
        {
            return FMOD5_Channel_GetVolume(this.handle, out volume);
        }
        public RESULT setVolumeRamp(bool ramp)
        {
            return FMOD5_Channel_SetVolumeRamp(this.handle, ramp);
        }
        public RESULT getVolumeRamp(out bool ramp)
        {
            return FMOD5_Channel_GetVolumeRamp(this.handle, out ramp);
        }
        public RESULT getAudibility(out float audibility)
        {
            return FMOD5_Channel_GetAudibility(this.handle, out audibility);
        }
        public RESULT setPitch(float pitch)
        {
            return FMOD5_Channel_SetPitch(this.handle, pitch);
        }
        public RESULT getPitch(out float pitch)
        {
            return FMOD5_Channel_GetPitch(this.handle, out pitch);
        }
        public RESULT setMute(bool mute)
        {
            return FMOD5_Channel_SetMute(this.handle, mute);
        }
        public RESULT getMute(out bool mute)
        {
            return FMOD5_Channel_GetMute(this.handle, out mute);
        }
        public RESULT setReverbProperties(int instance, float wet)
        {
            return FMOD5_Channel_SetReverbProperties(this.handle, instance, wet);
        }
        public RESULT getReverbProperties(int instance, out float wet)
        {
            return FMOD5_Channel_GetReverbProperties(this.handle, instance, out wet);
        }
        public RESULT setLowPassGain(float gain)
        {
            return FMOD5_Channel_SetLowPassGain(this.handle, gain);
        }
        public RESULT getLowPassGain(out float gain)
        {
            return FMOD5_Channel_GetLowPassGain(this.handle, out gain);
        }
        public RESULT setMode(MODE mode)
        {
            return FMOD5_Channel_SetMode(this.handle, mode);
        }
        public RESULT getMode(out MODE mode)
        {
            return FMOD5_Channel_GetMode(this.handle, out mode);
        }
        public RESULT setCallback(CHANNELCONTROL_CALLBACK callback)
        {
            return FMOD5_Channel_SetCallback(this.handle, callback);
        }
        public RESULT isPlaying(out bool isplaying)
        {
            return FMOD5_Channel_IsPlaying(this.handle, out isplaying);
        }

        // Note all 'set' functions alter a final matrix, this is why the only get function is getMixMatrix, to avoid other get functions returning incorrect/obsolete values.
        public RESULT setPan(float pan)
        {
            return FMOD5_Channel_SetPan(this.handle, pan);
        }
        public RESULT setMixLevelsOutput(float frontleft, float frontright, float center, float lfe, float surroundleft, float surroundright, float backleft, float backright)
        {
            return FMOD5_Channel_SetMixLevelsOutput(this.handle, frontleft, frontright, center, lfe, surroundleft, surroundright, backleft, backright);
        }
        public RESULT setMixLevelsInput(float[] levels, int numlevels)
        {
            return FMOD5_Channel_SetMixLevelsInput(this.handle, levels, numlevels);
        }
        public RESULT setMixMatrix(float[] matrix, int outchannels, int inchannels, int inchannel_hop = 0)
        {
            return FMOD5_Channel_SetMixMatrix(this.handle, matrix, outchannels, inchannels, inchannel_hop);
        }
        public RESULT getMixMatrix(float[] matrix, out int outchannels, out int inchannels, int inchannel_hop = 0)
        {
            return FMOD5_Channel_GetMixMatrix(this.handle, matrix, out outchannels, out inchannels, inchannel_hop);
        }

        // Clock based functionality.
        public RESULT getDSPClock(out ulong dspclock, out ulong parentclock)
        {
            return FMOD5_Channel_GetDSPClock(this.handle, out dspclock, out parentclock);
        }
        public RESULT setDelay(ulong dspclock_start, ulong dspclock_end, bool stopchannels = true)
        {
            return FMOD5_Channel_SetDelay(this.handle, dspclock_start, dspclock_end, stopchannels);
        }
        public RESULT getDelay(out ulong dspclock_start, out ulong dspclock_end)
        {
            return FMOD5_Channel_GetDelay(this.handle, out dspclock_start, out dspclock_end, IntPtr.Zero);
        }
        public RESULT getDelay(out ulong dspclock_start, out ulong dspclock_end, out bool stopchannels)
        {
            return FMOD5_Channel_GetDelay2(this.handle, out dspclock_start, out dspclock_end, out stopchannels);
        }
        public RESULT addFadePoint(ulong dspclock, float volume)
        {
            return FMOD5_Channel_AddFadePoint(this.handle, dspclock, volume);
        }
        public RESULT setFadePointRamp(ulong dspclock, float volume)
        {
            return FMOD5_Channel_SetFadePointRamp(this.handle, dspclock, volume);
        }
        public RESULT removeFadePoints(ulong dspclock_start, ulong dspclock_end)
        {
            return FMOD5_Channel_RemoveFadePoints(this.handle, dspclock_start, dspclock_end);
        }
        public RESULT getFadePoints(ref uint numpoints, ulong[] point_dspclock, float[] point_volume)
        {
            return FMOD5_Channel_GetFadePoints(this.handle, ref numpoints, point_dspclock, point_volume);
        }

        // DSP effects.
        public RESULT getDSP(int index, out DSP dsp)
        {
            return FMOD5_Channel_GetDSP(this.handle, index, out dsp.handle);
        }
        public RESULT addDSP(int index, DSP dsp)
        {
            return FMOD5_Channel_AddDSP(this.handle, index, dsp.handle);
        }
        public RESULT removeDSP(DSP dsp)
        {
            return FMOD5_Channel_RemoveDSP(this.handle, dsp.handle);
        }
        public RESULT getNumDSPs(out int numdsps)
        {
            return FMOD5_Channel_GetNumDSPs(this.handle, out numdsps);
        }
        public RESULT setDSPIndex(DSP dsp, int index)
        {
            return FMOD5_Channel_SetDSPIndex(this.handle, dsp.handle, index);
        }
        public RESULT getDSPIndex(DSP dsp, out int index)
        {
            return FMOD5_Channel_GetDSPIndex(this.handle, dsp.handle, out index);
        }

        // 3D functionality.
        public RESULT set3DAttributes(ref VECTOR pos, ref VECTOR vel)
        {
            return FMOD5_Channel_Set3DAttributes(this.handle, ref pos, ref vel);
        }
        public RESULT get3DAttributes(out VECTOR pos, out VECTOR vel)
        {
            return FMOD5_Channel_Get3DAttributes(this.handle, out pos, out vel);
        }
        public RESULT set3DMinMaxDistance(float mindistance, float maxdistance)
        {
            return FMOD5_Channel_Set3DMinMaxDistance(this.handle, mindistance, maxdistance);
        }
        public RESULT get3DMinMaxDistance(out float mindistance, out float maxdistance)
        {
            return FMOD5_Channel_Get3DMinMaxDistance(this.handle, out mindistance, out maxdistance);
        }
        public RESULT set3DConeSettings(float insideconeangle, float outsideconeangle, float outsidevolume)
        {
            return FMOD5_Channel_Set3DConeSettings(this.handle, insideconeangle, outsideconeangle, outsidevolume);
        }
        public RESULT get3DConeSettings(out float insideconeangle, out float outsideconeangle, out float outsidevolume)
        {
            return FMOD5_Channel_Get3DConeSettings(this.handle, out insideconeangle, out outsideconeangle, out outsidevolume);
        }
        public RESULT set3DConeOrientation(ref VECTOR orientation)
        {
            return FMOD5_Channel_Set3DConeOrientation(this.handle, ref orientation);
        }
        public RESULT get3DConeOrientation(out VECTOR orientation)
        {
            return FMOD5_Channel_Get3DConeOrientation(this.handle, out orientation);
        }
        public RESULT set3DCustomRolloff(ref VECTOR points, int numpoints)
        {
            return FMOD5_Channel_Set3DCustomRolloff(this.handle, ref points, numpoints);
        }
        public RESULT get3DCustomRolloff(out IntPtr points, out int numpoints)
        {
            return FMOD5_Channel_Get3DCustomRolloff(this.handle, out points, out numpoints);
        }
        public RESULT set3DOcclusion(float directocclusion, float reverbocclusion)
        {
            return FMOD5_Channel_Set3DOcclusion(this.handle, directocclusion, reverbocclusion);
        }
        public RESULT get3DOcclusion(out float directocclusion, out float reverbocclusion)
        {
            return FMOD5_Channel_Get3DOcclusion(this.handle, out directocclusion, out reverbocclusion);
        }
        public RESULT set3DSpread(float angle)
        {
            return FMOD5_Channel_Set3DSpread(this.handle, angle);
        }
        public RESULT get3DSpread(out float angle)
        {
            return FMOD5_Channel_Get3DSpread(this.handle, out angle);
        }
        public RESULT set3DLevel(float level)
        {
            return FMOD5_Channel_Set3DLevel(this.handle, level);
        }
        public RESULT get3DLevel(out float level)
        {
            return FMOD5_Channel_Get3DLevel(this.handle, out level);
        }
        public RESULT set3DDopplerLevel(float level)
        {
            return FMOD5_Channel_Set3DDopplerLevel(this.handle, level);
        }
        public RESULT get3DDopplerLevel(out float level)
        {
            return FMOD5_Channel_Get3DDopplerLevel(this.handle, out level);
        }
        public RESULT set3DDistanceFilter(bool custom, float customLevel, float centerFreq)
        {
            return FMOD5_Channel_Set3DDistanceFilter(this.handle, custom, customLevel, centerFreq);
        }
        public RESULT get3DDistanceFilter(out bool custom, out float customLevel, out float centerFreq)
        {
            return FMOD5_Channel_Get3DDistanceFilter(this.handle, out custom, out customLevel, out centerFreq);
        }

        // Userdata set/get.
        public RESULT setUserData(IntPtr userdata)
        {
            return FMOD5_Channel_SetUserData(this.handle, userdata);
        }
        public RESULT getUserData(out IntPtr userdata)
        {
            return FMOD5_Channel_GetUserData(this.handle, out userdata);
        }

        #region importfunctions
        private delegate RESULT FMOD5_Channel_SetFrequency_Delegate(IntPtr channel, float frequency);
        private static FMOD5_Channel_SetFrequency_Delegate FMOD5_Channel_SetFrequency_Internal = null;
        private static FMOD5_Channel_SetFrequency_Delegate FMOD5_Channel_SetFrequency => FMOD5_Channel_SetFrequency_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_SetFrequency_Delegate>(nameof(FMOD5_Channel_SetFrequency));

        private delegate RESULT FMOD5_Channel_GetFrequency_Delegate(IntPtr channel, out float frequency);
        private static FMOD5_Channel_GetFrequency_Delegate FMOD5_Channel_GetFrequency_Internal = null;
        private static FMOD5_Channel_GetFrequency_Delegate FMOD5_Channel_GetFrequency => FMOD5_Channel_GetFrequency_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_GetFrequency_Delegate>(nameof(FMOD5_Channel_GetFrequency));

        private delegate RESULT FMOD5_Channel_SetPriority_Delegate(IntPtr channel, int priority);
        private static FMOD5_Channel_SetPriority_Delegate FMOD5_Channel_SetPriority_Internal = null;
        private static FMOD5_Channel_SetPriority_Delegate FMOD5_Channel_SetPriority => FMOD5_Channel_SetPriority_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_SetPriority_Delegate>(nameof(FMOD5_Channel_SetPriority));

        private delegate RESULT FMOD5_Channel_GetPriority_Delegate(IntPtr channel, out int priority);
        private static FMOD5_Channel_GetPriority_Delegate FMOD5_Channel_GetPriority_Internal = null;
        private static FMOD5_Channel_GetPriority_Delegate FMOD5_Channel_GetPriority => FMOD5_Channel_GetPriority_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_GetPriority_Delegate>(nameof(FMOD5_Channel_GetPriority));

        private delegate RESULT FMOD5_Channel_SetPosition_Delegate(IntPtr channel, uint position, TIMEUNIT postype);
        private static FMOD5_Channel_SetPosition_Delegate FMOD5_Channel_SetPosition_Internal = null;
        private static FMOD5_Channel_SetPosition_Delegate FMOD5_Channel_SetPosition => FMOD5_Channel_SetPosition_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_SetPosition_Delegate>(nameof(FMOD5_Channel_SetPosition));

        private delegate RESULT FMOD5_Channel_GetPosition_Delegate(IntPtr channel, out uint position, TIMEUNIT postype);
        private static FMOD5_Channel_GetPosition_Delegate FMOD5_Channel_GetPosition_Internal = null;
        private static FMOD5_Channel_GetPosition_Delegate FMOD5_Channel_GetPosition => FMOD5_Channel_GetPosition_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_GetPosition_Delegate>(nameof(FMOD5_Channel_GetPosition));

        private delegate RESULT FMOD5_Channel_SetChannelGroup_Delegate(IntPtr channel, IntPtr channelgroup);
        private static FMOD5_Channel_SetChannelGroup_Delegate FMOD5_Channel_SetChannelGroup_Internal = null;
        private static FMOD5_Channel_SetChannelGroup_Delegate FMOD5_Channel_SetChannelGroup => FMOD5_Channel_SetChannelGroup_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_SetChannelGroup_Delegate>(nameof(FMOD5_Channel_SetChannelGroup));

        private delegate RESULT FMOD5_Channel_GetChannelGroup_Delegate(IntPtr channel, out IntPtr channelgroup);
        private static FMOD5_Channel_GetChannelGroup_Delegate FMOD5_Channel_GetChannelGroup_Internal = null;
        private static FMOD5_Channel_GetChannelGroup_Delegate FMOD5_Channel_GetChannelGroup => FMOD5_Channel_GetChannelGroup_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_GetChannelGroup_Delegate>(nameof(FMOD5_Channel_GetChannelGroup));

        private delegate RESULT FMOD5_Channel_SetLoopCount_Delegate(IntPtr channel, int loopcount);
        private static FMOD5_Channel_SetLoopCount_Delegate FMOD5_Channel_SetLoopCount_Internal = null;
        private static FMOD5_Channel_SetLoopCount_Delegate FMOD5_Channel_SetLoopCount => FMOD5_Channel_SetLoopCount_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_SetLoopCount_Delegate>(nameof(FMOD5_Channel_SetLoopCount));

        private delegate RESULT FMOD5_Channel_GetLoopCount_Delegate(IntPtr channel, out int loopcount);
        private static FMOD5_Channel_GetLoopCount_Delegate FMOD5_Channel_GetLoopCount_Internal = null;
        private static FMOD5_Channel_GetLoopCount_Delegate FMOD5_Channel_GetLoopCount => FMOD5_Channel_GetLoopCount_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_GetLoopCount_Delegate>(nameof(FMOD5_Channel_GetLoopCount));

        private delegate RESULT FMOD5_Channel_SetLoopPoints_Delegate(IntPtr channel, uint  loopstart, TIMEUNIT loopstarttype, uint  loopend, TIMEUNIT loopendtype);
        private static FMOD5_Channel_SetLoopPoints_Delegate FMOD5_Channel_SetLoopPoints_Internal = null;
        private static FMOD5_Channel_SetLoopPoints_Delegate FMOD5_Channel_SetLoopPoints => FMOD5_Channel_SetLoopPoints_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_SetLoopPoints_Delegate>(nameof(FMOD5_Channel_SetLoopPoints));

        private delegate RESULT FMOD5_Channel_GetLoopPoints_Delegate(IntPtr channel, out uint loopstart, TIMEUNIT loopstarttype, out uint loopend, TIMEUNIT loopendtype);
        private static FMOD5_Channel_GetLoopPoints_Delegate FMOD5_Channel_GetLoopPoints_Internal = null;
        private static FMOD5_Channel_GetLoopPoints_Delegate FMOD5_Channel_GetLoopPoints => FMOD5_Channel_GetLoopPoints_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_GetLoopPoints_Delegate>(nameof(FMOD5_Channel_GetLoopPoints));

        private delegate RESULT FMOD5_Channel_IsVirtual_Delegate(IntPtr channel, out bool isvirtual);
        private static FMOD5_Channel_IsVirtual_Delegate FMOD5_Channel_IsVirtual_Internal = null;
        private static FMOD5_Channel_IsVirtual_Delegate FMOD5_Channel_IsVirtual => FMOD5_Channel_IsVirtual_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_IsVirtual_Delegate>(nameof(FMOD5_Channel_IsVirtual));

        private delegate RESULT FMOD5_Channel_GetCurrentSound_Delegate(IntPtr channel, out IntPtr sound);
        private static FMOD5_Channel_GetCurrentSound_Delegate FMOD5_Channel_GetCurrentSound_Internal = null;
        private static FMOD5_Channel_GetCurrentSound_Delegate FMOD5_Channel_GetCurrentSound => FMOD5_Channel_GetCurrentSound_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_GetCurrentSound_Delegate>(nameof(FMOD5_Channel_GetCurrentSound));

        private delegate RESULT FMOD5_Channel_GetIndex_Delegate(IntPtr channel, out int index);
        private static FMOD5_Channel_GetIndex_Delegate FMOD5_Channel_GetIndex_Internal = null;
        private static FMOD5_Channel_GetIndex_Delegate FMOD5_Channel_GetIndex => FMOD5_Channel_GetIndex_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_GetIndex_Delegate>(nameof(FMOD5_Channel_GetIndex));

        private delegate RESULT FMOD5_Channel_GetSystemObject_Delegate(IntPtr channel, out IntPtr system);
        private static FMOD5_Channel_GetSystemObject_Delegate FMOD5_Channel_GetSystemObject_Internal = null;
        private static FMOD5_Channel_GetSystemObject_Delegate FMOD5_Channel_GetSystemObject => FMOD5_Channel_GetSystemObject_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_GetSystemObject_Delegate>(nameof(FMOD5_Channel_GetSystemObject));

        private delegate RESULT FMOD5_Channel_Stop_Delegate(IntPtr channel);
        private static FMOD5_Channel_Stop_Delegate FMOD5_Channel_Stop_Internal = null;
        private static FMOD5_Channel_Stop_Delegate FMOD5_Channel_Stop => FMOD5_Channel_Stop_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_Stop_Delegate>(nameof(FMOD5_Channel_Stop));

        private delegate RESULT FMOD5_Channel_SetPaused_Delegate(IntPtr channel, bool paused);
        private static FMOD5_Channel_SetPaused_Delegate FMOD5_Channel_SetPaused_Internal = null;
        private static FMOD5_Channel_SetPaused_Delegate FMOD5_Channel_SetPaused => FMOD5_Channel_SetPaused_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_SetPaused_Delegate>(nameof(FMOD5_Channel_SetPaused));

        private delegate RESULT FMOD5_Channel_GetPaused_Delegate(IntPtr channel, out bool paused);
        private static FMOD5_Channel_GetPaused_Delegate FMOD5_Channel_GetPaused_Internal = null;
        private static FMOD5_Channel_GetPaused_Delegate FMOD5_Channel_GetPaused => FMOD5_Channel_GetPaused_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_GetPaused_Delegate>(nameof(FMOD5_Channel_GetPaused));

        private delegate RESULT FMOD5_Channel_SetVolume_Delegate(IntPtr channel, float volume);
        private static FMOD5_Channel_SetVolume_Delegate FMOD5_Channel_SetVolume_Internal = null;
        private static FMOD5_Channel_SetVolume_Delegate FMOD5_Channel_SetVolume => FMOD5_Channel_SetVolume_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_SetVolume_Delegate>(nameof(FMOD5_Channel_SetVolume));

        private delegate RESULT FMOD5_Channel_GetVolume_Delegate(IntPtr channel, out float volume);
        private static FMOD5_Channel_GetVolume_Delegate FMOD5_Channel_GetVolume_Internal = null;
        private static FMOD5_Channel_GetVolume_Delegate FMOD5_Channel_GetVolume => FMOD5_Channel_GetVolume_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_GetVolume_Delegate>(nameof(FMOD5_Channel_GetVolume));

        private delegate RESULT FMOD5_Channel_SetVolumeRamp_Delegate(IntPtr channel, bool ramp);
        private static FMOD5_Channel_SetVolumeRamp_Delegate FMOD5_Channel_SetVolumeRamp_Internal = null;
        private static FMOD5_Channel_SetVolumeRamp_Delegate FMOD5_Channel_SetVolumeRamp => FMOD5_Channel_SetVolumeRamp_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_SetVolumeRamp_Delegate>(nameof(FMOD5_Channel_SetVolumeRamp));

        private delegate RESULT FMOD5_Channel_GetVolumeRamp_Delegate(IntPtr channel, out bool ramp);
        private static FMOD5_Channel_GetVolumeRamp_Delegate FMOD5_Channel_GetVolumeRamp_Internal = null;
        private static FMOD5_Channel_GetVolumeRamp_Delegate FMOD5_Channel_GetVolumeRamp => FMOD5_Channel_GetVolumeRamp_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_GetVolumeRamp_Delegate>(nameof(FMOD5_Channel_GetVolumeRamp));

        private delegate RESULT FMOD5_Channel_GetAudibility_Delegate(IntPtr channel, out float audibility);
        private static FMOD5_Channel_GetAudibility_Delegate FMOD5_Channel_GetAudibility_Internal = null;
        private static FMOD5_Channel_GetAudibility_Delegate FMOD5_Channel_GetAudibility => FMOD5_Channel_GetAudibility_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_GetAudibility_Delegate>(nameof(FMOD5_Channel_GetAudibility));

        private delegate RESULT FMOD5_Channel_SetPitch_Delegate(IntPtr channel, float pitch);
        private static FMOD5_Channel_SetPitch_Delegate FMOD5_Channel_SetPitch_Internal = null;
        private static FMOD5_Channel_SetPitch_Delegate FMOD5_Channel_SetPitch => FMOD5_Channel_SetPitch_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_SetPitch_Delegate>(nameof(FMOD5_Channel_SetPitch));

        private delegate RESULT FMOD5_Channel_GetPitch_Delegate(IntPtr channel, out float pitch);
        private static FMOD5_Channel_GetPitch_Delegate FMOD5_Channel_GetPitch_Internal = null;
        private static FMOD5_Channel_GetPitch_Delegate FMOD5_Channel_GetPitch => FMOD5_Channel_GetPitch_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_GetPitch_Delegate>(nameof(FMOD5_Channel_GetPitch));

        private delegate RESULT FMOD5_Channel_SetMute_Delegate(IntPtr channel, bool mute);
        private static FMOD5_Channel_SetMute_Delegate FMOD5_Channel_SetMute_Internal = null;
        private static FMOD5_Channel_SetMute_Delegate FMOD5_Channel_SetMute => FMOD5_Channel_SetMute_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_SetMute_Delegate>(nameof(FMOD5_Channel_SetMute));

        private delegate RESULT FMOD5_Channel_GetMute_Delegate(IntPtr channel, out bool mute);
        private static FMOD5_Channel_GetMute_Delegate FMOD5_Channel_GetMute_Internal = null;
        private static FMOD5_Channel_GetMute_Delegate FMOD5_Channel_GetMute => FMOD5_Channel_GetMute_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_GetMute_Delegate>(nameof(FMOD5_Channel_GetMute));

        private delegate RESULT FMOD5_Channel_SetReverbProperties_Delegate(IntPtr channel, int instance, float wet);
        private static FMOD5_Channel_SetReverbProperties_Delegate FMOD5_Channel_SetReverbProperties_Internal = null;
        private static FMOD5_Channel_SetReverbProperties_Delegate FMOD5_Channel_SetReverbProperties => FMOD5_Channel_SetReverbProperties_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_SetReverbProperties_Delegate>(nameof(FMOD5_Channel_SetReverbProperties));

        private delegate RESULT FMOD5_Channel_GetReverbProperties_Delegate(IntPtr channel, int instance, out float wet);
        private static FMOD5_Channel_GetReverbProperties_Delegate FMOD5_Channel_GetReverbProperties_Internal = null;
        private static FMOD5_Channel_GetReverbProperties_Delegate FMOD5_Channel_GetReverbProperties => FMOD5_Channel_GetReverbProperties_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_GetReverbProperties_Delegate>(nameof(FMOD5_Channel_GetReverbProperties));

        private delegate RESULT FMOD5_Channel_SetLowPassGain_Delegate(IntPtr channel, float gain);
        private static FMOD5_Channel_SetLowPassGain_Delegate FMOD5_Channel_SetLowPassGain_Internal = null;
        private static FMOD5_Channel_SetLowPassGain_Delegate FMOD5_Channel_SetLowPassGain => FMOD5_Channel_SetLowPassGain_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_SetLowPassGain_Delegate>(nameof(FMOD5_Channel_SetLowPassGain));

        private delegate RESULT FMOD5_Channel_GetLowPassGain_Delegate(IntPtr channel, out float gain);
        private static FMOD5_Channel_GetLowPassGain_Delegate FMOD5_Channel_GetLowPassGain_Internal = null;
        private static FMOD5_Channel_GetLowPassGain_Delegate FMOD5_Channel_GetLowPassGain => FMOD5_Channel_GetLowPassGain_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_GetLowPassGain_Delegate>(nameof(FMOD5_Channel_GetLowPassGain));

        private delegate RESULT FMOD5_Channel_SetMode_Delegate(IntPtr channel, MODE mode);
        private static FMOD5_Channel_SetMode_Delegate FMOD5_Channel_SetMode_Internal = null;
        private static FMOD5_Channel_SetMode_Delegate FMOD5_Channel_SetMode => FMOD5_Channel_SetMode_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_SetMode_Delegate>(nameof(FMOD5_Channel_SetMode));

        private delegate RESULT FMOD5_Channel_GetMode_Delegate(IntPtr channel, out MODE mode);
        private static FMOD5_Channel_GetMode_Delegate FMOD5_Channel_GetMode_Internal = null;
        private static FMOD5_Channel_GetMode_Delegate FMOD5_Channel_GetMode => FMOD5_Channel_GetMode_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_GetMode_Delegate>(nameof(FMOD5_Channel_GetMode));

        private delegate RESULT FMOD5_Channel_SetCallback_Delegate(IntPtr channel, CHANNELCONTROL_CALLBACK callback);
        private static FMOD5_Channel_SetCallback_Delegate FMOD5_Channel_SetCallback_Internal = null;
        private static FMOD5_Channel_SetCallback_Delegate FMOD5_Channel_SetCallback => FMOD5_Channel_SetCallback_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_SetCallback_Delegate>(nameof(FMOD5_Channel_SetCallback));

        private delegate RESULT FMOD5_Channel_IsPlaying_Delegate(IntPtr channel, out bool isplaying);
        private static FMOD5_Channel_IsPlaying_Delegate FMOD5_Channel_IsPlaying_Internal = null;
        private static FMOD5_Channel_IsPlaying_Delegate FMOD5_Channel_IsPlaying => FMOD5_Channel_IsPlaying_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_IsPlaying_Delegate>(nameof(FMOD5_Channel_IsPlaying));

        private delegate RESULT FMOD5_Channel_SetPan_Delegate(IntPtr channel, float pan);
        private static FMOD5_Channel_SetPan_Delegate FMOD5_Channel_SetPan_Internal = null;
        private static FMOD5_Channel_SetPan_Delegate FMOD5_Channel_SetPan => FMOD5_Channel_SetPan_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_SetPan_Delegate>(nameof(FMOD5_Channel_SetPan));

        private delegate RESULT FMOD5_Channel_SetMixLevelsOutput_Delegate(IntPtr channel, float frontleft, float frontright, float center, float lfe, float surroundleft, float surroundright, float backleft, float backright);
        private static FMOD5_Channel_SetMixLevelsOutput_Delegate FMOD5_Channel_SetMixLevelsOutput_Internal = null;
        private static FMOD5_Channel_SetMixLevelsOutput_Delegate FMOD5_Channel_SetMixLevelsOutput => FMOD5_Channel_SetMixLevelsOutput_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_SetMixLevelsOutput_Delegate>(nameof(FMOD5_Channel_SetMixLevelsOutput));

        private delegate RESULT FMOD5_Channel_SetMixLevelsInput_Delegate(IntPtr channel, float[] levels, int numlevels);
        private static FMOD5_Channel_SetMixLevelsInput_Delegate FMOD5_Channel_SetMixLevelsInput_Internal = null;
        private static FMOD5_Channel_SetMixLevelsInput_Delegate FMOD5_Channel_SetMixLevelsInput => FMOD5_Channel_SetMixLevelsInput_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_SetMixLevelsInput_Delegate>(nameof(FMOD5_Channel_SetMixLevelsInput));

        private delegate RESULT FMOD5_Channel_SetMixMatrix_Delegate(IntPtr channel, float[] matrix, int outchannels, int inchannels, int inchannel_hop);
        private static FMOD5_Channel_SetMixMatrix_Delegate FMOD5_Channel_SetMixMatrix_Internal = null;
        private static FMOD5_Channel_SetMixMatrix_Delegate FMOD5_Channel_SetMixMatrix => FMOD5_Channel_SetMixMatrix_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_SetMixMatrix_Delegate>(nameof(FMOD5_Channel_SetMixMatrix));

        private delegate RESULT FMOD5_Channel_GetMixMatrix_Delegate(IntPtr channel, float[] matrix, out int outchannels, out int inchannels, int inchannel_hop);
        private static FMOD5_Channel_GetMixMatrix_Delegate FMOD5_Channel_GetMixMatrix_Internal = null;
        private static FMOD5_Channel_GetMixMatrix_Delegate FMOD5_Channel_GetMixMatrix => FMOD5_Channel_GetMixMatrix_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_GetMixMatrix_Delegate>(nameof(FMOD5_Channel_GetMixMatrix));

        private delegate RESULT FMOD5_Channel_GetDSPClock_Delegate(IntPtr channel, out ulong dspclock, out ulong parentclock);
        private static FMOD5_Channel_GetDSPClock_Delegate FMOD5_Channel_GetDSPClock_Internal = null;
        private static FMOD5_Channel_GetDSPClock_Delegate FMOD5_Channel_GetDSPClock => FMOD5_Channel_GetDSPClock_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_GetDSPClock_Delegate>(nameof(FMOD5_Channel_GetDSPClock));

        private delegate RESULT FMOD5_Channel_SetDelay_Delegate(IntPtr channel, ulong dspclock_start, ulong dspclock_end, bool stopchannels);
        private static FMOD5_Channel_SetDelay_Delegate FMOD5_Channel_SetDelay_Internal = null;
        private static FMOD5_Channel_SetDelay_Delegate FMOD5_Channel_SetDelay => FMOD5_Channel_SetDelay_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_SetDelay_Delegate>(nameof(FMOD5_Channel_SetDelay));

        private delegate RESULT FMOD5_Channel_GetDelay_Delegate(IntPtr channel, out ulong dspclock_start, out ulong dspclock_end, IntPtr zero);
        private static FMOD5_Channel_GetDelay_Delegate FMOD5_Channel_GetDelay_Internal = null;
        private static FMOD5_Channel_GetDelay_Delegate FMOD5_Channel_GetDelay => FMOD5_Channel_GetDelay_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_GetDelay_Delegate>(nameof(FMOD5_Channel_GetDelay));

        private delegate RESULT FMOD5_Channel_GetDelay_Delegate2(IntPtr channel, out ulong dspclock_start, out ulong dspclock_end, out bool stopchannels);
        private static FMOD5_Channel_GetDelay_Delegate2 FMOD5_Channel_GetDelay_Internal2 = null;
        private static FMOD5_Channel_GetDelay_Delegate2 FMOD5_Channel_GetDelay2 => FMOD5_Channel_GetDelay_Internal2 ??= FModManager.GetProcInFModStudio<FMOD5_Channel_GetDelay_Delegate2>(nameof(FMOD5_Channel_GetDelay));

        private delegate RESULT FMOD5_Channel_AddFadePoint_Delegate(IntPtr channel, ulong dspclock, float volume);
        private static FMOD5_Channel_AddFadePoint_Delegate FMOD5_Channel_AddFadePoint_Internal = null;
        private static FMOD5_Channel_AddFadePoint_Delegate FMOD5_Channel_AddFadePoint => FMOD5_Channel_AddFadePoint_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_AddFadePoint_Delegate>(nameof(FMOD5_Channel_AddFadePoint));

        private delegate RESULT FMOD5_Channel_SetFadePointRamp_Delegate(IntPtr channel, ulong dspclock, float volume);
        private static FMOD5_Channel_SetFadePointRamp_Delegate FMOD5_Channel_SetFadePointRamp_Internal = null;
        private static FMOD5_Channel_SetFadePointRamp_Delegate FMOD5_Channel_SetFadePointRamp => FMOD5_Channel_SetFadePointRamp_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_SetFadePointRamp_Delegate>(nameof(FMOD5_Channel_SetFadePointRamp));

        private delegate RESULT FMOD5_Channel_RemoveFadePoints_Delegate(IntPtr channel, ulong dspclock_start, ulong dspclock_end);
        private static FMOD5_Channel_RemoveFadePoints_Delegate FMOD5_Channel_RemoveFadePoints_Internal = null;
        private static FMOD5_Channel_RemoveFadePoints_Delegate FMOD5_Channel_RemoveFadePoints => FMOD5_Channel_RemoveFadePoints_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_RemoveFadePoints_Delegate>(nameof(FMOD5_Channel_RemoveFadePoints));

        private delegate RESULT FMOD5_Channel_GetFadePoints_Delegate(IntPtr channel, ref uint numpoints, ulong[] point_dspclock, float[] point_volume);
        private static FMOD5_Channel_GetFadePoints_Delegate FMOD5_Channel_GetFadePoints_Internal = null;
        private static FMOD5_Channel_GetFadePoints_Delegate FMOD5_Channel_GetFadePoints => FMOD5_Channel_GetFadePoints_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_GetFadePoints_Delegate>(nameof(FMOD5_Channel_GetFadePoints));

        private delegate RESULT FMOD5_Channel_GetDSP_Delegate(IntPtr channel, int index, out IntPtr dsp);
        private static FMOD5_Channel_GetDSP_Delegate FMOD5_Channel_GetDSP_Internal = null;
        private static FMOD5_Channel_GetDSP_Delegate FMOD5_Channel_GetDSP => FMOD5_Channel_GetDSP_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_GetDSP_Delegate>(nameof(FMOD5_Channel_GetDSP));

        private delegate RESULT FMOD5_Channel_AddDSP_Delegate(IntPtr channel, int index, IntPtr dsp);
        private static FMOD5_Channel_AddDSP_Delegate FMOD5_Channel_AddDSP_Internal = null;
        private static FMOD5_Channel_AddDSP_Delegate FMOD5_Channel_AddDSP => FMOD5_Channel_AddDSP_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_AddDSP_Delegate>(nameof(FMOD5_Channel_AddDSP));

        private delegate RESULT FMOD5_Channel_RemoveDSP_Delegate(IntPtr channel, IntPtr dsp);
        private static FMOD5_Channel_RemoveDSP_Delegate FMOD5_Channel_RemoveDSP_Internal = null;
        private static FMOD5_Channel_RemoveDSP_Delegate FMOD5_Channel_RemoveDSP => FMOD5_Channel_RemoveDSP_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_RemoveDSP_Delegate>(nameof(FMOD5_Channel_RemoveDSP));

        private delegate RESULT FMOD5_Channel_GetNumDSPs_Delegate(IntPtr channel, out int numdsps);
        private static FMOD5_Channel_GetNumDSPs_Delegate FMOD5_Channel_GetNumDSPs_Internal = null;
        private static FMOD5_Channel_GetNumDSPs_Delegate FMOD5_Channel_GetNumDSPs => FMOD5_Channel_GetNumDSPs_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_GetNumDSPs_Delegate>(nameof(FMOD5_Channel_GetNumDSPs));

        private delegate RESULT FMOD5_Channel_SetDSPIndex_Delegate(IntPtr channel, IntPtr dsp, int index);
        private static FMOD5_Channel_SetDSPIndex_Delegate FMOD5_Channel_SetDSPIndex_Internal = null;
        private static FMOD5_Channel_SetDSPIndex_Delegate FMOD5_Channel_SetDSPIndex => FMOD5_Channel_SetDSPIndex_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_SetDSPIndex_Delegate>(nameof(FMOD5_Channel_SetDSPIndex));

        private delegate RESULT FMOD5_Channel_GetDSPIndex_Delegate(IntPtr channel, IntPtr dsp, out int index);
        private static FMOD5_Channel_GetDSPIndex_Delegate FMOD5_Channel_GetDSPIndex_Internal = null;
        private static FMOD5_Channel_GetDSPIndex_Delegate FMOD5_Channel_GetDSPIndex => FMOD5_Channel_GetDSPIndex_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_GetDSPIndex_Delegate>(nameof(FMOD5_Channel_GetDSPIndex));

        private delegate RESULT FMOD5_Channel_Set3DAttributes_Delegate(IntPtr channel, ref VECTOR pos, ref VECTOR vel);
        private static FMOD5_Channel_Set3DAttributes_Delegate FMOD5_Channel_Set3DAttributes_Internal = null;
        private static FMOD5_Channel_Set3DAttributes_Delegate FMOD5_Channel_Set3DAttributes => FMOD5_Channel_Set3DAttributes_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_Set3DAttributes_Delegate>(nameof(FMOD5_Channel_Set3DAttributes));

        private delegate RESULT FMOD5_Channel_Get3DAttributes_Delegate(IntPtr channel, out VECTOR pos, out VECTOR vel);
        private static FMOD5_Channel_Get3DAttributes_Delegate FMOD5_Channel_Get3DAttributes_Internal = null;
        private static FMOD5_Channel_Get3DAttributes_Delegate FMOD5_Channel_Get3DAttributes => FMOD5_Channel_Get3DAttributes_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_Get3DAttributes_Delegate>(nameof(FMOD5_Channel_Get3DAttributes));

        private delegate RESULT FMOD5_Channel_Set3DMinMaxDistance_Delegate(IntPtr channel, float mindistance, float maxdistance);
        private static FMOD5_Channel_Set3DMinMaxDistance_Delegate FMOD5_Channel_Set3DMinMaxDistance_Internal = null;
        private static FMOD5_Channel_Set3DMinMaxDistance_Delegate FMOD5_Channel_Set3DMinMaxDistance => FMOD5_Channel_Set3DMinMaxDistance_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_Set3DMinMaxDistance_Delegate>(nameof(FMOD5_Channel_Set3DMinMaxDistance));

        private delegate RESULT FMOD5_Channel_Get3DMinMaxDistance_Delegate(IntPtr channel, out float mindistance, out float maxdistance);
        private static FMOD5_Channel_Get3DMinMaxDistance_Delegate FMOD5_Channel_Get3DMinMaxDistance_Internal = null;
        private static FMOD5_Channel_Get3DMinMaxDistance_Delegate FMOD5_Channel_Get3DMinMaxDistance => FMOD5_Channel_Get3DMinMaxDistance_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_Get3DMinMaxDistance_Delegate>(nameof(FMOD5_Channel_Get3DMinMaxDistance));

        private delegate RESULT FMOD5_Channel_Set3DConeSettings_Delegate(IntPtr channel, float insideconeangle, float outsideconeangle, float outsidevolume);
        private static FMOD5_Channel_Set3DConeSettings_Delegate FMOD5_Channel_Set3DConeSettings_Internal = null;
        private static FMOD5_Channel_Set3DConeSettings_Delegate FMOD5_Channel_Set3DConeSettings => FMOD5_Channel_Set3DConeSettings_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_Set3DConeSettings_Delegate>(nameof(FMOD5_Channel_Set3DConeSettings));

        private delegate RESULT FMOD5_Channel_Get3DConeSettings_Delegate(IntPtr channel, out float insideconeangle, out float outsideconeangle, out float outsidevolume);
        private static FMOD5_Channel_Get3DConeSettings_Delegate FMOD5_Channel_Get3DConeSettings_Internal = null;
        private static FMOD5_Channel_Get3DConeSettings_Delegate FMOD5_Channel_Get3DConeSettings => FMOD5_Channel_Get3DConeSettings_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_Get3DConeSettings_Delegate>(nameof(FMOD5_Channel_Get3DConeSettings));

        private delegate RESULT FMOD5_Channel_Set3DConeOrientation_Delegate(IntPtr channel, ref VECTOR orientation);
        private static FMOD5_Channel_Set3DConeOrientation_Delegate FMOD5_Channel_Set3DConeOrientation_Internal = null;
        private static FMOD5_Channel_Set3DConeOrientation_Delegate FMOD5_Channel_Set3DConeOrientation => FMOD5_Channel_Set3DConeOrientation_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_Set3DConeOrientation_Delegate>(nameof(FMOD5_Channel_Set3DConeOrientation));

        private delegate RESULT FMOD5_Channel_Get3DConeOrientation_Delegate(IntPtr channel, out VECTOR orientation);
        private static FMOD5_Channel_Get3DConeOrientation_Delegate FMOD5_Channel_Get3DConeOrientation_Internal = null;
        private static FMOD5_Channel_Get3DConeOrientation_Delegate FMOD5_Channel_Get3DConeOrientation => FMOD5_Channel_Get3DConeOrientation_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_Get3DConeOrientation_Delegate>(nameof(FMOD5_Channel_Get3DConeOrientation));

        private delegate RESULT FMOD5_Channel_Set3DCustomRolloff_Delegate(IntPtr channel, ref VECTOR points, int numpoints);
        private static FMOD5_Channel_Set3DCustomRolloff_Delegate FMOD5_Channel_Set3DCustomRolloff_Internal = null;
        private static FMOD5_Channel_Set3DCustomRolloff_Delegate FMOD5_Channel_Set3DCustomRolloff => FMOD5_Channel_Set3DCustomRolloff_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_Set3DCustomRolloff_Delegate>(nameof(FMOD5_Channel_Set3DCustomRolloff));

        private delegate RESULT FMOD5_Channel_Get3DCustomRolloff_Delegate(IntPtr channel, out IntPtr points, out int numpoints);
        private static FMOD5_Channel_Get3DCustomRolloff_Delegate FMOD5_Channel_Get3DCustomRolloff_Internal = null;
        private static FMOD5_Channel_Get3DCustomRolloff_Delegate FMOD5_Channel_Get3DCustomRolloff => FMOD5_Channel_Get3DCustomRolloff_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_Get3DCustomRolloff_Delegate>(nameof(FMOD5_Channel_Get3DCustomRolloff));

        private delegate RESULT FMOD5_Channel_Set3DOcclusion_Delegate(IntPtr channel, float directocclusion, float reverbocclusion);
        private static FMOD5_Channel_Set3DOcclusion_Delegate FMOD5_Channel_Set3DOcclusion_Internal = null;
        private static FMOD5_Channel_Set3DOcclusion_Delegate FMOD5_Channel_Set3DOcclusion => FMOD5_Channel_Set3DOcclusion_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_Set3DOcclusion_Delegate>(nameof(FMOD5_Channel_Set3DOcclusion));

        private delegate RESULT FMOD5_Channel_Get3DOcclusion_Delegate(IntPtr channel, out float directocclusion, out float reverbocclusion);
        private static FMOD5_Channel_Get3DOcclusion_Delegate FMOD5_Channel_Get3DOcclusion_Internal = null;
        private static FMOD5_Channel_Get3DOcclusion_Delegate FMOD5_Channel_Get3DOcclusion => FMOD5_Channel_Get3DOcclusion_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_Get3DOcclusion_Delegate>(nameof(FMOD5_Channel_Get3DOcclusion));

        private delegate RESULT FMOD5_Channel_Set3DSpread_Delegate(IntPtr channel, float angle);
        private static FMOD5_Channel_Set3DSpread_Delegate FMOD5_Channel_Set3DSpread_Internal = null;
        private static FMOD5_Channel_Set3DSpread_Delegate FMOD5_Channel_Set3DSpread => FMOD5_Channel_Set3DSpread_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_Set3DSpread_Delegate>(nameof(FMOD5_Channel_Set3DSpread));

        private delegate RESULT FMOD5_Channel_Get3DSpread_Delegate(IntPtr channel, out float angle);
        private static FMOD5_Channel_Get3DSpread_Delegate FMOD5_Channel_Get3DSpread_Internal = null;
        private static FMOD5_Channel_Get3DSpread_Delegate FMOD5_Channel_Get3DSpread => FMOD5_Channel_Get3DSpread_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_Get3DSpread_Delegate>(nameof(FMOD5_Channel_Get3DSpread));

        private delegate RESULT FMOD5_Channel_Set3DLevel_Delegate(IntPtr channel, float level);
        private static FMOD5_Channel_Set3DLevel_Delegate FMOD5_Channel_Set3DLevel_Internal = null;
        private static FMOD5_Channel_Set3DLevel_Delegate FMOD5_Channel_Set3DLevel => FMOD5_Channel_Set3DLevel_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_Set3DLevel_Delegate>(nameof(FMOD5_Channel_Set3DLevel));

        private delegate RESULT FMOD5_Channel_Get3DLevel_Delegate(IntPtr channel, out float level);
        private static FMOD5_Channel_Get3DLevel_Delegate FMOD5_Channel_Get3DLevel_Internal = null;
        private static FMOD5_Channel_Get3DLevel_Delegate FMOD5_Channel_Get3DLevel => FMOD5_Channel_Get3DLevel_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_Get3DLevel_Delegate>(nameof(FMOD5_Channel_Get3DLevel));

        private delegate RESULT FMOD5_Channel_Set3DDopplerLevel_Delegate(IntPtr channel, float level);
        private static FMOD5_Channel_Set3DDopplerLevel_Delegate FMOD5_Channel_Set3DDopplerLevel_Internal = null;
        private static FMOD5_Channel_Set3DDopplerLevel_Delegate FMOD5_Channel_Set3DDopplerLevel => FMOD5_Channel_Set3DDopplerLevel_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_Set3DDopplerLevel_Delegate>(nameof(FMOD5_Channel_Set3DDopplerLevel));

        private delegate RESULT FMOD5_Channel_Get3DDopplerLevel_Delegate(IntPtr channel, out float level);
        private static FMOD5_Channel_Get3DDopplerLevel_Delegate FMOD5_Channel_Get3DDopplerLevel_Internal = null;
        private static FMOD5_Channel_Get3DDopplerLevel_Delegate FMOD5_Channel_Get3DDopplerLevel => FMOD5_Channel_Get3DDopplerLevel_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_Get3DDopplerLevel_Delegate>(nameof(FMOD5_Channel_Get3DDopplerLevel));

        private delegate RESULT FMOD5_Channel_Set3DDistanceFilter_Delegate(IntPtr channel, bool custom, float customLevel, float centerFreq);
        private static FMOD5_Channel_Set3DDistanceFilter_Delegate FMOD5_Channel_Set3DDistanceFilter_Internal = null;
        private static FMOD5_Channel_Set3DDistanceFilter_Delegate FMOD5_Channel_Set3DDistanceFilter => FMOD5_Channel_Set3DDistanceFilter_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_Set3DDistanceFilter_Delegate>(nameof(FMOD5_Channel_Set3DDistanceFilter));

        private delegate RESULT FMOD5_Channel_Get3DDistanceFilter_Delegate(IntPtr channel, out bool custom, out float customLevel, out float centerFreq);
        private static FMOD5_Channel_Get3DDistanceFilter_Delegate FMOD5_Channel_Get3DDistanceFilter_Internal = null;
        private static FMOD5_Channel_Get3DDistanceFilter_Delegate FMOD5_Channel_Get3DDistanceFilter => FMOD5_Channel_Get3DDistanceFilter_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_Get3DDistanceFilter_Delegate>(nameof(FMOD5_Channel_Get3DDistanceFilter));

        private delegate RESULT FMOD5_Channel_SetUserData_Delegate(IntPtr channel, IntPtr userdata);
        private static FMOD5_Channel_SetUserData_Delegate FMOD5_Channel_SetUserData_Internal = null;
        private static FMOD5_Channel_SetUserData_Delegate FMOD5_Channel_SetUserData => FMOD5_Channel_SetUserData_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_SetUserData_Delegate>(nameof(FMOD5_Channel_SetUserData));

        private delegate RESULT FMOD5_Channel_GetUserData_Delegate(IntPtr channel, out IntPtr userdata);
        private static FMOD5_Channel_GetUserData_Delegate FMOD5_Channel_GetUserData_Internal = null;
        private static FMOD5_Channel_GetUserData_Delegate FMOD5_Channel_GetUserData => FMOD5_Channel_GetUserData_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Channel_GetUserData_Delegate>(nameof(FMOD5_Channel_GetUserData));

        #endregion

        #region wrapperinternal

        public IntPtr handle;

        public Channel(IntPtr ptr)  { this.handle = ptr; }
        public bool hasHandle()     { return this.handle != IntPtr.Zero; }
        public void clearHandle()   { this.handle = IntPtr.Zero; }

        #endregion
    }

    /*
        'ChannelGroup' API
    */
    public struct ChannelGroup : IChannelControl
    {
        public RESULT release()
        {
            return FMOD5_ChannelGroup_Release(this.handle);
        }

        // Nested channel groups.
        public RESULT addGroup(ChannelGroup group, bool propagatedspclock = true)
        {
            return FMOD5_ChannelGroup_AddGroup(this.handle, group.handle, propagatedspclock, IntPtr.Zero);
        }
        public RESULT addGroup(ChannelGroup group, bool propagatedspclock, out DSPConnection connection)
        {
            return FMOD5_ChannelGroup_AddGroup2(this.handle, group.handle, propagatedspclock, out connection.handle);
        }
        public RESULT getNumGroups(out int numgroups)
        {
            return FMOD5_ChannelGroup_GetNumGroups(this.handle, out numgroups);
        }
        public RESULT getGroup(int index, out ChannelGroup group)
        {
            return FMOD5_ChannelGroup_GetGroup(this.handle, index, out group.handle);
        }
        public RESULT getParentGroup(out ChannelGroup group)
        {
            return FMOD5_ChannelGroup_GetParentGroup(this.handle, out group.handle);
        }

        // Information only functions.
        public RESULT getName(out string name, int namelen)
        {
            IntPtr stringMem = Marshal.AllocHGlobal(namelen);

            RESULT result = FMOD5_ChannelGroup_GetName(this.handle, stringMem, namelen);
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                name = encoder.stringFromNative(stringMem);
            }
            Marshal.FreeHGlobal(stringMem);

            return result;
        }
        public RESULT getNumChannels(out int numchannels)
        {
            return FMOD5_ChannelGroup_GetNumChannels(this.handle, out numchannels);
        }
        public RESULT getChannel(int index, out Channel channel)
        {
            return FMOD5_ChannelGroup_GetChannel(this.handle, index, out channel.handle);
        }

        public RESULT getSystemObject(out System system)
        {
            return FMOD5_ChannelGroup_GetSystemObject(this.handle, out system.handle);
        }

        // General control functionality for Channels and ChannelGroups.
        public RESULT stop()
        {
            return FMOD5_ChannelGroup_Stop(this.handle);
        }
        public RESULT setPaused(bool paused)
        {
            return FMOD5_ChannelGroup_SetPaused(this.handle, paused);
        }
        public RESULT getPaused(out bool paused)
        {
            return FMOD5_ChannelGroup_GetPaused(this.handle, out paused);
        }
        public RESULT setVolume(float volume)
        {
            return FMOD5_ChannelGroup_SetVolume(this.handle, volume);
        }
        public RESULT getVolume(out float volume)
        {
            return FMOD5_ChannelGroup_GetVolume(this.handle, out volume);
        }
        public RESULT setVolumeRamp(bool ramp)
        {
            return FMOD5_ChannelGroup_SetVolumeRamp(this.handle, ramp);
        }
        public RESULT getVolumeRamp(out bool ramp)
        {
            return FMOD5_ChannelGroup_GetVolumeRamp(this.handle, out ramp);
        }
        public RESULT getAudibility(out float audibility)
        {
            return FMOD5_ChannelGroup_GetAudibility(this.handle, out audibility);
        }
        public RESULT setPitch(float pitch)
        {
            return FMOD5_ChannelGroup_SetPitch(this.handle, pitch);
        }
        public RESULT getPitch(out float pitch)
        {
            return FMOD5_ChannelGroup_GetPitch(this.handle, out pitch);
        }
        public RESULT setMute(bool mute)
        {
            return FMOD5_ChannelGroup_SetMute(this.handle, mute);
        }
        public RESULT getMute(out bool mute)
        {
            return FMOD5_ChannelGroup_GetMute(this.handle, out mute);
        }
        public RESULT setReverbProperties(int instance, float wet)
        {
            return FMOD5_ChannelGroup_SetReverbProperties(this.handle, instance, wet);
        }
        public RESULT getReverbProperties(int instance, out float wet)
        {
            return FMOD5_ChannelGroup_GetReverbProperties(this.handle, instance, out wet);
        }
        public RESULT setLowPassGain(float gain)
        {
            return FMOD5_ChannelGroup_SetLowPassGain(this.handle, gain);
        }
        public RESULT getLowPassGain(out float gain)
        {
            return FMOD5_ChannelGroup_GetLowPassGain(this.handle, out gain);
        }
        public RESULT setMode(MODE mode)
        {
            return FMOD5_ChannelGroup_SetMode(this.handle, mode);
        }
        public RESULT getMode(out MODE mode)
        {
            return FMOD5_ChannelGroup_GetMode(this.handle, out mode);
        }
        public RESULT setCallback(CHANNELCONTROL_CALLBACK callback)
        {
            return FMOD5_ChannelGroup_SetCallback(this.handle, callback);
        }
        public RESULT isPlaying(out bool isplaying)
        {
            return FMOD5_ChannelGroup_IsPlaying(this.handle, out isplaying);
        }

        // Note all 'set' functions alter a final matrix, this is why the only get function is getMixMatrix, to avoid other get functions returning incorrect/obsolete values.
        public RESULT setPan(float pan)
        {
            return FMOD5_ChannelGroup_SetPan(this.handle, pan);
        }
        public RESULT setMixLevelsOutput(float frontleft, float frontright, float center, float lfe, float surroundleft, float surroundright, float backleft, float backright)
        {
            return FMOD5_ChannelGroup_SetMixLevelsOutput(this.handle, frontleft, frontright, center, lfe, surroundleft, surroundright, backleft, backright);
        }
        public RESULT setMixLevelsInput(float[] levels, int numlevels)
        {
            return FMOD5_ChannelGroup_SetMixLevelsInput(this.handle, levels, numlevels);
        }
        public RESULT setMixMatrix(float[] matrix, int outchannels, int inchannels, int inchannel_hop)
        {
            return FMOD5_ChannelGroup_SetMixMatrix(this.handle, matrix, outchannels, inchannels, inchannel_hop);
        }
        public RESULT getMixMatrix(float[] matrix, out int outchannels, out int inchannels, int inchannel_hop)
        {
            return FMOD5_ChannelGroup_GetMixMatrix(this.handle, matrix, out outchannels, out inchannels, inchannel_hop);
        }

        // Clock based functionality.
        public RESULT getDSPClock(out ulong dspclock, out ulong parentclock)
        {
            return FMOD5_ChannelGroup_GetDSPClock(this.handle, out dspclock, out parentclock);
        }
        public RESULT setDelay(ulong dspclock_start, ulong dspclock_end, bool stopchannels)
        {
            return FMOD5_ChannelGroup_SetDelay(this.handle, dspclock_start, dspclock_end, stopchannels);
        }
        public RESULT getDelay(out ulong dspclock_start, out ulong dspclock_end)
        {
            return FMOD5_ChannelGroup_GetDelay(this.handle, out dspclock_start, out dspclock_end, IntPtr.Zero);
        }
        public RESULT getDelay(out ulong dspclock_start, out ulong dspclock_end, out bool stopchannels)
        {
            return FMOD5_ChannelGroup_GetDelay2(this.handle, out dspclock_start, out dspclock_end, out stopchannels);
        }
        public RESULT addFadePoint(ulong dspclock, float volume)
        {
            return FMOD5_ChannelGroup_AddFadePoint(this.handle, dspclock, volume);
        }
        public RESULT setFadePointRamp(ulong dspclock, float volume)
        {
            return FMOD5_ChannelGroup_SetFadePointRamp(this.handle, dspclock, volume);
        }
        public RESULT removeFadePoints(ulong dspclock_start, ulong dspclock_end)
        {
            return FMOD5_ChannelGroup_RemoveFadePoints(this.handle, dspclock_start, dspclock_end);
        }
        public RESULT getFadePoints(ref uint numpoints, ulong[] point_dspclock, float[] point_volume)
        {
            return FMOD5_ChannelGroup_GetFadePoints(this.handle, ref numpoints, point_dspclock, point_volume);
        }

        // DSP effects.
        public RESULT getDSP(int index, out DSP dsp)
        {
            return FMOD5_ChannelGroup_GetDSP(this.handle, index, out dsp.handle);
        }
        public RESULT addDSP(int index, DSP dsp)
        {
            return FMOD5_ChannelGroup_AddDSP(this.handle, index, dsp.handle);
        }
        public RESULT removeDSP(DSP dsp)
        {
            return FMOD5_ChannelGroup_RemoveDSP(this.handle, dsp.handle);
        }
        public RESULT getNumDSPs(out int numdsps)
        {
            return FMOD5_ChannelGroup_GetNumDSPs(this.handle, out numdsps);
        }
        public RESULT setDSPIndex(DSP dsp, int index)
        {
            return FMOD5_ChannelGroup_SetDSPIndex(this.handle, dsp.handle, index);
        }
        public RESULT getDSPIndex(DSP dsp, out int index)
        {
            return FMOD5_ChannelGroup_GetDSPIndex(this.handle, dsp.handle, out index);
        }

        // 3D functionality.
        public RESULT set3DAttributes(ref VECTOR pos, ref VECTOR vel)
        {
            return FMOD5_ChannelGroup_Set3DAttributes(this.handle, ref pos, ref vel);
        }
        public RESULT get3DAttributes(out VECTOR pos, out VECTOR vel)
        {
            return FMOD5_ChannelGroup_Get3DAttributes(this.handle, out pos, out vel);
        }
        public RESULT set3DMinMaxDistance(float mindistance, float maxdistance)
        {
            return FMOD5_ChannelGroup_Set3DMinMaxDistance(this.handle, mindistance, maxdistance);
        }
        public RESULT get3DMinMaxDistance(out float mindistance, out float maxdistance)
        {
            return FMOD5_ChannelGroup_Get3DMinMaxDistance(this.handle, out mindistance, out maxdistance);
        }
        public RESULT set3DConeSettings(float insideconeangle, float outsideconeangle, float outsidevolume)
        {
            return FMOD5_ChannelGroup_Set3DConeSettings(this.handle, insideconeangle, outsideconeangle, outsidevolume);
        }
        public RESULT get3DConeSettings(out float insideconeangle, out float outsideconeangle, out float outsidevolume)
        {
            return FMOD5_ChannelGroup_Get3DConeSettings(this.handle, out insideconeangle, out outsideconeangle, out outsidevolume);
        }
        public RESULT set3DConeOrientation(ref VECTOR orientation)
        {
            return FMOD5_ChannelGroup_Set3DConeOrientation(this.handle, ref orientation);
        }
        public RESULT get3DConeOrientation(out VECTOR orientation)
        {
            return FMOD5_ChannelGroup_Get3DConeOrientation(this.handle, out orientation);
        }
        public RESULT set3DCustomRolloff(ref VECTOR points, int numpoints)
        {
            return FMOD5_ChannelGroup_Set3DCustomRolloff(this.handle, ref points, numpoints);
        }
        public RESULT get3DCustomRolloff(out IntPtr points, out int numpoints)
        {
            return FMOD5_ChannelGroup_Get3DCustomRolloff(this.handle, out points, out numpoints);
        }
        public RESULT set3DOcclusion(float directocclusion, float reverbocclusion)
        {
            return FMOD5_ChannelGroup_Set3DOcclusion(this.handle, directocclusion, reverbocclusion);
        }
        public RESULT get3DOcclusion(out float directocclusion, out float reverbocclusion)
        {
            return FMOD5_ChannelGroup_Get3DOcclusion(this.handle, out directocclusion, out reverbocclusion);
        }
        public RESULT set3DSpread(float angle)
        {
            return FMOD5_ChannelGroup_Set3DSpread(this.handle, angle);
        }
        public RESULT get3DSpread(out float angle)
        {
            return FMOD5_ChannelGroup_Get3DSpread(this.handle, out angle);
        }
        public RESULT set3DLevel(float level)
        {
            return FMOD5_ChannelGroup_Set3DLevel(this.handle, level);
        }
        public RESULT get3DLevel(out float level)
        {
            return FMOD5_ChannelGroup_Get3DLevel(this.handle, out level);
        }
        public RESULT set3DDopplerLevel(float level)
        {
            return FMOD5_ChannelGroup_Set3DDopplerLevel(this.handle, level);
        }
        public RESULT get3DDopplerLevel(out float level)
        {
            return FMOD5_ChannelGroup_Get3DDopplerLevel(this.handle, out level);
        }
        public RESULT set3DDistanceFilter(bool custom, float customLevel, float centerFreq)
        {
            return FMOD5_ChannelGroup_Set3DDistanceFilter(this.handle, custom, customLevel, centerFreq);
        }
        public RESULT get3DDistanceFilter(out bool custom, out float customLevel, out float centerFreq)
        {
            return FMOD5_ChannelGroup_Get3DDistanceFilter(this.handle, out custom, out customLevel, out centerFreq);
        }

        // Userdata set/get.
        public RESULT setUserData(IntPtr userdata)
        {
            return FMOD5_ChannelGroup_SetUserData(this.handle, userdata);
        }
        public RESULT getUserData(out IntPtr userdata)
        {
            return FMOD5_ChannelGroup_GetUserData(this.handle, out userdata);
        }

        #region importfunctions
        private delegate RESULT FMOD5_ChannelGroup_Release_Delegate(IntPtr channelgroup);
        private static FMOD5_ChannelGroup_Release_Delegate FMOD5_ChannelGroup_Release_Internal = null;
        private static FMOD5_ChannelGroup_Release_Delegate FMOD5_ChannelGroup_Release => FMOD5_ChannelGroup_Release_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_Release_Delegate>(nameof(FMOD5_ChannelGroup_Release));

        private delegate RESULT FMOD5_ChannelGroup_AddGroup_Delegate(IntPtr channelgroup, IntPtr group, bool propagatedspclock, IntPtr zero);
        private static FMOD5_ChannelGroup_AddGroup_Delegate FMOD5_ChannelGroup_AddGroup_Internal = null;
        private static FMOD5_ChannelGroup_AddGroup_Delegate FMOD5_ChannelGroup_AddGroup => FMOD5_ChannelGroup_AddGroup_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_AddGroup_Delegate>(nameof(FMOD5_ChannelGroup_AddGroup));

        private delegate RESULT FMOD5_ChannelGroup_AddGroup_Delegate2(IntPtr channelgroup, IntPtr group, bool propagatedspclock, out IntPtr connection);
        private static FMOD5_ChannelGroup_AddGroup_Delegate2 FMOD5_ChannelGroup_AddGroup_Internal2 = null;
        private static FMOD5_ChannelGroup_AddGroup_Delegate2 FMOD5_ChannelGroup_AddGroup2 => FMOD5_ChannelGroup_AddGroup_Internal2 ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_AddGroup_Delegate2>(nameof(FMOD5_ChannelGroup_AddGroup));

        private delegate RESULT FMOD5_ChannelGroup_GetNumGroups_Delegate(IntPtr channelgroup, out int numgroups);
        private static FMOD5_ChannelGroup_GetNumGroups_Delegate FMOD5_ChannelGroup_GetNumGroups_Internal = null;
        private static FMOD5_ChannelGroup_GetNumGroups_Delegate FMOD5_ChannelGroup_GetNumGroups => FMOD5_ChannelGroup_GetNumGroups_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_GetNumGroups_Delegate>(nameof(FMOD5_ChannelGroup_GetNumGroups));

        private delegate RESULT FMOD5_ChannelGroup_GetGroup_Delegate(IntPtr channelgroup, int index, out IntPtr group);
        private static FMOD5_ChannelGroup_GetGroup_Delegate FMOD5_ChannelGroup_GetGroup_Internal = null;
        private static FMOD5_ChannelGroup_GetGroup_Delegate FMOD5_ChannelGroup_GetGroup => FMOD5_ChannelGroup_GetGroup_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_GetGroup_Delegate>(nameof(FMOD5_ChannelGroup_GetGroup));

        private delegate RESULT FMOD5_ChannelGroup_GetParentGroup_Delegate(IntPtr channelgroup, out IntPtr group);
        private static FMOD5_ChannelGroup_GetParentGroup_Delegate FMOD5_ChannelGroup_GetParentGroup_Internal = null;
        private static FMOD5_ChannelGroup_GetParentGroup_Delegate FMOD5_ChannelGroup_GetParentGroup => FMOD5_ChannelGroup_GetParentGroup_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_GetParentGroup_Delegate>(nameof(FMOD5_ChannelGroup_GetParentGroup));

        private delegate RESULT FMOD5_ChannelGroup_GetName_Delegate(IntPtr channelgroup, IntPtr name, int namelen);
        private static FMOD5_ChannelGroup_GetName_Delegate FMOD5_ChannelGroup_GetName_Internal = null;
        private static FMOD5_ChannelGroup_GetName_Delegate FMOD5_ChannelGroup_GetName => FMOD5_ChannelGroup_GetName_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_GetName_Delegate>(nameof(FMOD5_ChannelGroup_GetName));

        private delegate RESULT FMOD5_ChannelGroup_GetNumChannels_Delegate(IntPtr channelgroup, out int numchannels);
        private static FMOD5_ChannelGroup_GetNumChannels_Delegate FMOD5_ChannelGroup_GetNumChannels_Internal = null;
        private static FMOD5_ChannelGroup_GetNumChannels_Delegate FMOD5_ChannelGroup_GetNumChannels => FMOD5_ChannelGroup_GetNumChannels_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_GetNumChannels_Delegate>(nameof(FMOD5_ChannelGroup_GetNumChannels));

        private delegate RESULT FMOD5_ChannelGroup_GetChannel_Delegate(IntPtr channelgroup, int index, out IntPtr channel);
        private static FMOD5_ChannelGroup_GetChannel_Delegate FMOD5_ChannelGroup_GetChannel_Internal = null;
        private static FMOD5_ChannelGroup_GetChannel_Delegate FMOD5_ChannelGroup_GetChannel => FMOD5_ChannelGroup_GetChannel_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_GetChannel_Delegate>(nameof(FMOD5_ChannelGroup_GetChannel));

        private delegate RESULT FMOD5_ChannelGroup_GetSystemObject_Delegate(IntPtr channelgroup, out IntPtr system);
        private static FMOD5_ChannelGroup_GetSystemObject_Delegate FMOD5_ChannelGroup_GetSystemObject_Internal = null;
        private static FMOD5_ChannelGroup_GetSystemObject_Delegate FMOD5_ChannelGroup_GetSystemObject => FMOD5_ChannelGroup_GetSystemObject_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_GetSystemObject_Delegate>(nameof(FMOD5_ChannelGroup_GetSystemObject));

        private delegate RESULT FMOD5_ChannelGroup_Stop_Delegate(IntPtr channelgroup);
        private static FMOD5_ChannelGroup_Stop_Delegate FMOD5_ChannelGroup_Stop_Internal = null;
        private static FMOD5_ChannelGroup_Stop_Delegate FMOD5_ChannelGroup_Stop => FMOD5_ChannelGroup_Stop_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_Stop_Delegate>(nameof(FMOD5_ChannelGroup_Stop));

        private delegate RESULT FMOD5_ChannelGroup_SetPaused_Delegate(IntPtr channelgroup, bool paused);
        private static FMOD5_ChannelGroup_SetPaused_Delegate FMOD5_ChannelGroup_SetPaused_Internal = null;
        private static FMOD5_ChannelGroup_SetPaused_Delegate FMOD5_ChannelGroup_SetPaused => FMOD5_ChannelGroup_SetPaused_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_SetPaused_Delegate>(nameof(FMOD5_ChannelGroup_SetPaused));

        private delegate RESULT FMOD5_ChannelGroup_GetPaused_Delegate(IntPtr channelgroup, out bool paused);
        private static FMOD5_ChannelGroup_GetPaused_Delegate FMOD5_ChannelGroup_GetPaused_Internal = null;
        private static FMOD5_ChannelGroup_GetPaused_Delegate FMOD5_ChannelGroup_GetPaused => FMOD5_ChannelGroup_GetPaused_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_GetPaused_Delegate>(nameof(FMOD5_ChannelGroup_GetPaused));

        private delegate RESULT FMOD5_ChannelGroup_SetVolume_Delegate(IntPtr channelgroup, float volume);
        private static FMOD5_ChannelGroup_SetVolume_Delegate FMOD5_ChannelGroup_SetVolume_Internal = null;
        private static FMOD5_ChannelGroup_SetVolume_Delegate FMOD5_ChannelGroup_SetVolume => FMOD5_ChannelGroup_SetVolume_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_SetVolume_Delegate>(nameof(FMOD5_ChannelGroup_SetVolume));

        private delegate RESULT FMOD5_ChannelGroup_GetVolume_Delegate(IntPtr channelgroup, out float volume);
        private static FMOD5_ChannelGroup_GetVolume_Delegate FMOD5_ChannelGroup_GetVolume_Internal = null;
        private static FMOD5_ChannelGroup_GetVolume_Delegate FMOD5_ChannelGroup_GetVolume => FMOD5_ChannelGroup_GetVolume_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_GetVolume_Delegate>(nameof(FMOD5_ChannelGroup_GetVolume));

        private delegate RESULT FMOD5_ChannelGroup_SetVolumeRamp_Delegate(IntPtr channelgroup, bool ramp);
        private static FMOD5_ChannelGroup_SetVolumeRamp_Delegate FMOD5_ChannelGroup_SetVolumeRamp_Internal = null;
        private static FMOD5_ChannelGroup_SetVolumeRamp_Delegate FMOD5_ChannelGroup_SetVolumeRamp => FMOD5_ChannelGroup_SetVolumeRamp_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_SetVolumeRamp_Delegate>(nameof(FMOD5_ChannelGroup_SetVolumeRamp));

        private delegate RESULT FMOD5_ChannelGroup_GetVolumeRamp_Delegate(IntPtr channelgroup, out bool ramp);
        private static FMOD5_ChannelGroup_GetVolumeRamp_Delegate FMOD5_ChannelGroup_GetVolumeRamp_Internal = null;
        private static FMOD5_ChannelGroup_GetVolumeRamp_Delegate FMOD5_ChannelGroup_GetVolumeRamp => FMOD5_ChannelGroup_GetVolumeRamp_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_GetVolumeRamp_Delegate>(nameof(FMOD5_ChannelGroup_GetVolumeRamp));

        private delegate RESULT FMOD5_ChannelGroup_GetAudibility_Delegate(IntPtr channelgroup, out float audibility);
        private static FMOD5_ChannelGroup_GetAudibility_Delegate FMOD5_ChannelGroup_GetAudibility_Internal = null;
        private static FMOD5_ChannelGroup_GetAudibility_Delegate FMOD5_ChannelGroup_GetAudibility => FMOD5_ChannelGroup_GetAudibility_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_GetAudibility_Delegate>(nameof(FMOD5_ChannelGroup_GetAudibility));

        private delegate RESULT FMOD5_ChannelGroup_SetPitch_Delegate(IntPtr channelgroup, float pitch);
        private static FMOD5_ChannelGroup_SetPitch_Delegate FMOD5_ChannelGroup_SetPitch_Internal = null;
        private static FMOD5_ChannelGroup_SetPitch_Delegate FMOD5_ChannelGroup_SetPitch => FMOD5_ChannelGroup_SetPitch_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_SetPitch_Delegate>(nameof(FMOD5_ChannelGroup_SetPitch));

        private delegate RESULT FMOD5_ChannelGroup_GetPitch_Delegate(IntPtr channelgroup, out float pitch);
        private static FMOD5_ChannelGroup_GetPitch_Delegate FMOD5_ChannelGroup_GetPitch_Internal = null;
        private static FMOD5_ChannelGroup_GetPitch_Delegate FMOD5_ChannelGroup_GetPitch => FMOD5_ChannelGroup_GetPitch_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_GetPitch_Delegate>(nameof(FMOD5_ChannelGroup_GetPitch));

        private delegate RESULT FMOD5_ChannelGroup_SetMute_Delegate(IntPtr channelgroup, bool mute);
        private static FMOD5_ChannelGroup_SetMute_Delegate FMOD5_ChannelGroup_SetMute_Internal = null;
        private static FMOD5_ChannelGroup_SetMute_Delegate FMOD5_ChannelGroup_SetMute => FMOD5_ChannelGroup_SetMute_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_SetMute_Delegate>(nameof(FMOD5_ChannelGroup_SetMute));

        private delegate RESULT FMOD5_ChannelGroup_GetMute_Delegate(IntPtr channelgroup, out bool mute);
        private static FMOD5_ChannelGroup_GetMute_Delegate FMOD5_ChannelGroup_GetMute_Internal = null;
        private static FMOD5_ChannelGroup_GetMute_Delegate FMOD5_ChannelGroup_GetMute => FMOD5_ChannelGroup_GetMute_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_GetMute_Delegate>(nameof(FMOD5_ChannelGroup_GetMute));

        private delegate RESULT FMOD5_ChannelGroup_SetReverbProperties_Delegate(IntPtr channelgroup, int instance, float wet);
        private static FMOD5_ChannelGroup_SetReverbProperties_Delegate FMOD5_ChannelGroup_SetReverbProperties_Internal = null;
        private static FMOD5_ChannelGroup_SetReverbProperties_Delegate FMOD5_ChannelGroup_SetReverbProperties => FMOD5_ChannelGroup_SetReverbProperties_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_SetReverbProperties_Delegate>(nameof(FMOD5_ChannelGroup_SetReverbProperties));

        private delegate RESULT FMOD5_ChannelGroup_GetReverbProperties_Delegate(IntPtr channelgroup, int instance, out float wet);
        private static FMOD5_ChannelGroup_GetReverbProperties_Delegate FMOD5_ChannelGroup_GetReverbProperties_Internal = null;
        private static FMOD5_ChannelGroup_GetReverbProperties_Delegate FMOD5_ChannelGroup_GetReverbProperties => FMOD5_ChannelGroup_GetReverbProperties_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_GetReverbProperties_Delegate>(nameof(FMOD5_ChannelGroup_GetReverbProperties));

        private delegate RESULT FMOD5_ChannelGroup_SetLowPassGain_Delegate(IntPtr channelgroup, float gain);
        private static FMOD5_ChannelGroup_SetLowPassGain_Delegate FMOD5_ChannelGroup_SetLowPassGain_Internal = null;
        private static FMOD5_ChannelGroup_SetLowPassGain_Delegate FMOD5_ChannelGroup_SetLowPassGain => FMOD5_ChannelGroup_SetLowPassGain_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_SetLowPassGain_Delegate>(nameof(FMOD5_ChannelGroup_SetLowPassGain));

        private delegate RESULT FMOD5_ChannelGroup_GetLowPassGain_Delegate(IntPtr channelgroup, out float gain);
        private static FMOD5_ChannelGroup_GetLowPassGain_Delegate FMOD5_ChannelGroup_GetLowPassGain_Internal = null;
        private static FMOD5_ChannelGroup_GetLowPassGain_Delegate FMOD5_ChannelGroup_GetLowPassGain => FMOD5_ChannelGroup_GetLowPassGain_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_GetLowPassGain_Delegate>(nameof(FMOD5_ChannelGroup_GetLowPassGain));

        private delegate RESULT FMOD5_ChannelGroup_SetMode_Delegate(IntPtr channelgroup, MODE mode);
        private static FMOD5_ChannelGroup_SetMode_Delegate FMOD5_ChannelGroup_SetMode_Internal = null;
        private static FMOD5_ChannelGroup_SetMode_Delegate FMOD5_ChannelGroup_SetMode => FMOD5_ChannelGroup_SetMode_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_SetMode_Delegate>(nameof(FMOD5_ChannelGroup_SetMode));

        private delegate RESULT FMOD5_ChannelGroup_GetMode_Delegate(IntPtr channelgroup, out MODE mode);
        private static FMOD5_ChannelGroup_GetMode_Delegate FMOD5_ChannelGroup_GetMode_Internal = null;
        private static FMOD5_ChannelGroup_GetMode_Delegate FMOD5_ChannelGroup_GetMode => FMOD5_ChannelGroup_GetMode_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_GetMode_Delegate>(nameof(FMOD5_ChannelGroup_GetMode));

        private delegate RESULT FMOD5_ChannelGroup_SetCallback_Delegate(IntPtr channelgroup, CHANNELCONTROL_CALLBACK callback);
        private static FMOD5_ChannelGroup_SetCallback_Delegate FMOD5_ChannelGroup_SetCallback_Internal = null;
        private static FMOD5_ChannelGroup_SetCallback_Delegate FMOD5_ChannelGroup_SetCallback => FMOD5_ChannelGroup_SetCallback_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_SetCallback_Delegate>(nameof(FMOD5_ChannelGroup_SetCallback));

        private delegate RESULT FMOD5_ChannelGroup_IsPlaying_Delegate(IntPtr channelgroup, out bool isplaying);
        private static FMOD5_ChannelGroup_IsPlaying_Delegate FMOD5_ChannelGroup_IsPlaying_Internal = null;
        private static FMOD5_ChannelGroup_IsPlaying_Delegate FMOD5_ChannelGroup_IsPlaying => FMOD5_ChannelGroup_IsPlaying_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_IsPlaying_Delegate>(nameof(FMOD5_ChannelGroup_IsPlaying));

        private delegate RESULT FMOD5_ChannelGroup_SetPan_Delegate(IntPtr channelgroup, float pan);
        private static FMOD5_ChannelGroup_SetPan_Delegate FMOD5_ChannelGroup_SetPan_Internal = null;
        private static FMOD5_ChannelGroup_SetPan_Delegate FMOD5_ChannelGroup_SetPan => FMOD5_ChannelGroup_SetPan_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_SetPan_Delegate>(nameof(FMOD5_ChannelGroup_SetPan));

        private delegate RESULT FMOD5_ChannelGroup_SetMixLevelsOutput_Delegate(IntPtr channelgroup, float frontleft, float frontright, float center, float lfe, float surroundleft, float surroundright, float backleft, float backright);
        private static FMOD5_ChannelGroup_SetMixLevelsOutput_Delegate FMOD5_ChannelGroup_SetMixLevelsOutput_Internal = null;
        private static FMOD5_ChannelGroup_SetMixLevelsOutput_Delegate FMOD5_ChannelGroup_SetMixLevelsOutput => FMOD5_ChannelGroup_SetMixLevelsOutput_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_SetMixLevelsOutput_Delegate>(nameof(FMOD5_ChannelGroup_SetMixLevelsOutput));

        private delegate RESULT FMOD5_ChannelGroup_SetMixLevelsInput_Delegate(IntPtr channelgroup, float[] levels, int numlevels);
        private static FMOD5_ChannelGroup_SetMixLevelsInput_Delegate FMOD5_ChannelGroup_SetMixLevelsInput_Internal = null;
        private static FMOD5_ChannelGroup_SetMixLevelsInput_Delegate FMOD5_ChannelGroup_SetMixLevelsInput => FMOD5_ChannelGroup_SetMixLevelsInput_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_SetMixLevelsInput_Delegate>(nameof(FMOD5_ChannelGroup_SetMixLevelsInput));

        private delegate RESULT FMOD5_ChannelGroup_SetMixMatrix_Delegate(IntPtr channelgroup, float[] matrix, int outchannels, int inchannels, int inchannel_hop);
        private static FMOD5_ChannelGroup_SetMixMatrix_Delegate FMOD5_ChannelGroup_SetMixMatrix_Internal = null;
        private static FMOD5_ChannelGroup_SetMixMatrix_Delegate FMOD5_ChannelGroup_SetMixMatrix => FMOD5_ChannelGroup_SetMixMatrix_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_SetMixMatrix_Delegate>(nameof(FMOD5_ChannelGroup_SetMixMatrix));

        private delegate RESULT FMOD5_ChannelGroup_GetMixMatrix_Delegate(IntPtr channelgroup, float[] matrix, out int outchannels, out int inchannels, int inchannel_hop);
        private static FMOD5_ChannelGroup_GetMixMatrix_Delegate FMOD5_ChannelGroup_GetMixMatrix_Internal = null;
        private static FMOD5_ChannelGroup_GetMixMatrix_Delegate FMOD5_ChannelGroup_GetMixMatrix => FMOD5_ChannelGroup_GetMixMatrix_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_GetMixMatrix_Delegate>(nameof(FMOD5_ChannelGroup_GetMixMatrix));

        private delegate RESULT FMOD5_ChannelGroup_GetDSPClock_Delegate(IntPtr channelgroup, out ulong dspclock, out ulong parentclock);
        private static FMOD5_ChannelGroup_GetDSPClock_Delegate FMOD5_ChannelGroup_GetDSPClock_Internal = null;
        private static FMOD5_ChannelGroup_GetDSPClock_Delegate FMOD5_ChannelGroup_GetDSPClock => FMOD5_ChannelGroup_GetDSPClock_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_GetDSPClock_Delegate>(nameof(FMOD5_ChannelGroup_GetDSPClock));

        private delegate RESULT FMOD5_ChannelGroup_SetDelay_Delegate(IntPtr channelgroup, ulong dspclock_start, ulong dspclock_end, bool stopchannels);
        private static FMOD5_ChannelGroup_SetDelay_Delegate FMOD5_ChannelGroup_SetDelay_Internal = null;
        private static FMOD5_ChannelGroup_SetDelay_Delegate FMOD5_ChannelGroup_SetDelay => FMOD5_ChannelGroup_SetDelay_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_SetDelay_Delegate>(nameof(FMOD5_ChannelGroup_SetDelay));

        private delegate RESULT FMOD5_ChannelGroup_GetDelay_Delegate(IntPtr channelgroup, out ulong dspclock_start, out ulong dspclock_end, IntPtr zero);
        private static FMOD5_ChannelGroup_GetDelay_Delegate FMOD5_ChannelGroup_GetDelay_Internal = null;
        private static FMOD5_ChannelGroup_GetDelay_Delegate FMOD5_ChannelGroup_GetDelay => FMOD5_ChannelGroup_GetDelay_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_GetDelay_Delegate>(nameof(FMOD5_ChannelGroup_GetDelay));

        private delegate RESULT FMOD5_ChannelGroup_GetDelay_Delegate2(IntPtr channelgroup, out ulong dspclock_start, out ulong dspclock_end, out bool stopchannels);
        private static FMOD5_ChannelGroup_GetDelay_Delegate2 FMOD5_ChannelGroup_GetDelay_Internal2 = null;
        private static FMOD5_ChannelGroup_GetDelay_Delegate2 FMOD5_ChannelGroup_GetDelay2 => FMOD5_ChannelGroup_GetDelay_Internal2 ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_GetDelay_Delegate2>(nameof(FMOD5_ChannelGroup_GetDelay));

        private delegate RESULT FMOD5_ChannelGroup_AddFadePoint_Delegate(IntPtr channelgroup, ulong dspclock, float volume);
        private static FMOD5_ChannelGroup_AddFadePoint_Delegate FMOD5_ChannelGroup_AddFadePoint_Internal = null;
        private static FMOD5_ChannelGroup_AddFadePoint_Delegate FMOD5_ChannelGroup_AddFadePoint => FMOD5_ChannelGroup_AddFadePoint_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_AddFadePoint_Delegate>(nameof(FMOD5_ChannelGroup_AddFadePoint));

        private delegate RESULT FMOD5_ChannelGroup_SetFadePointRamp_Delegate(IntPtr channelgroup, ulong dspclock, float volume);
        private static FMOD5_ChannelGroup_SetFadePointRamp_Delegate FMOD5_ChannelGroup_SetFadePointRamp_Internal = null;
        private static FMOD5_ChannelGroup_SetFadePointRamp_Delegate FMOD5_ChannelGroup_SetFadePointRamp => FMOD5_ChannelGroup_SetFadePointRamp_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_SetFadePointRamp_Delegate>(nameof(FMOD5_ChannelGroup_SetFadePointRamp));

        private delegate RESULT FMOD5_ChannelGroup_RemoveFadePoints_Delegate(IntPtr channelgroup, ulong dspclock_start, ulong dspclock_end);
        private static FMOD5_ChannelGroup_RemoveFadePoints_Delegate FMOD5_ChannelGroup_RemoveFadePoints_Internal = null;
        private static FMOD5_ChannelGroup_RemoveFadePoints_Delegate FMOD5_ChannelGroup_RemoveFadePoints => FMOD5_ChannelGroup_RemoveFadePoints_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_RemoveFadePoints_Delegate>(nameof(FMOD5_ChannelGroup_RemoveFadePoints));

        private delegate RESULT FMOD5_ChannelGroup_GetFadePoints_Delegate(IntPtr channelgroup, ref uint numpoints, ulong[] point_dspclock, float[] point_volume);
        private static FMOD5_ChannelGroup_GetFadePoints_Delegate FMOD5_ChannelGroup_GetFadePoints_Internal = null;
        private static FMOD5_ChannelGroup_GetFadePoints_Delegate FMOD5_ChannelGroup_GetFadePoints => FMOD5_ChannelGroup_GetFadePoints_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_GetFadePoints_Delegate>(nameof(FMOD5_ChannelGroup_GetFadePoints));

        private delegate RESULT FMOD5_ChannelGroup_GetDSP_Delegate(IntPtr channelgroup, int index, out IntPtr dsp);
        private static FMOD5_ChannelGroup_GetDSP_Delegate FMOD5_ChannelGroup_GetDSP_Internal = null;
        private static FMOD5_ChannelGroup_GetDSP_Delegate FMOD5_ChannelGroup_GetDSP => FMOD5_ChannelGroup_GetDSP_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_GetDSP_Delegate>(nameof(FMOD5_ChannelGroup_GetDSP));

        private delegate RESULT FMOD5_ChannelGroup_AddDSP_Delegate(IntPtr channelgroup, int index, IntPtr dsp);
        private static FMOD5_ChannelGroup_AddDSP_Delegate FMOD5_ChannelGroup_AddDSP_Internal = null;
        private static FMOD5_ChannelGroup_AddDSP_Delegate FMOD5_ChannelGroup_AddDSP => FMOD5_ChannelGroup_AddDSP_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_AddDSP_Delegate>(nameof(FMOD5_ChannelGroup_AddDSP));

        private delegate RESULT FMOD5_ChannelGroup_RemoveDSP_Delegate(IntPtr channelgroup, IntPtr dsp);
        private static FMOD5_ChannelGroup_RemoveDSP_Delegate FMOD5_ChannelGroup_RemoveDSP_Internal = null;
        private static FMOD5_ChannelGroup_RemoveDSP_Delegate FMOD5_ChannelGroup_RemoveDSP => FMOD5_ChannelGroup_RemoveDSP_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_RemoveDSP_Delegate>(nameof(FMOD5_ChannelGroup_RemoveDSP));

        private delegate RESULT FMOD5_ChannelGroup_GetNumDSPs_Delegate(IntPtr channelgroup, out int numdsps);
        private static FMOD5_ChannelGroup_GetNumDSPs_Delegate FMOD5_ChannelGroup_GetNumDSPs_Internal = null;
        private static FMOD5_ChannelGroup_GetNumDSPs_Delegate FMOD5_ChannelGroup_GetNumDSPs => FMOD5_ChannelGroup_GetNumDSPs_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_GetNumDSPs_Delegate>(nameof(FMOD5_ChannelGroup_GetNumDSPs));

        private delegate RESULT FMOD5_ChannelGroup_SetDSPIndex_Delegate(IntPtr channelgroup, IntPtr dsp, int index);
        private static FMOD5_ChannelGroup_SetDSPIndex_Delegate FMOD5_ChannelGroup_SetDSPIndex_Internal = null;
        private static FMOD5_ChannelGroup_SetDSPIndex_Delegate FMOD5_ChannelGroup_SetDSPIndex => FMOD5_ChannelGroup_SetDSPIndex_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_SetDSPIndex_Delegate>(nameof(FMOD5_ChannelGroup_SetDSPIndex));

        private delegate RESULT FMOD5_ChannelGroup_GetDSPIndex_Delegate(IntPtr channelgroup, IntPtr dsp, out int index);
        private static FMOD5_ChannelGroup_GetDSPIndex_Delegate FMOD5_ChannelGroup_GetDSPIndex_Internal = null;
        private static FMOD5_ChannelGroup_GetDSPIndex_Delegate FMOD5_ChannelGroup_GetDSPIndex => FMOD5_ChannelGroup_GetDSPIndex_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_GetDSPIndex_Delegate>(nameof(FMOD5_ChannelGroup_GetDSPIndex));

        private delegate RESULT FMOD5_ChannelGroup_Set3DAttributes_Delegate(IntPtr channelgroup, ref VECTOR pos, ref VECTOR vel);
        private static FMOD5_ChannelGroup_Set3DAttributes_Delegate FMOD5_ChannelGroup_Set3DAttributes_Internal = null;
        private static FMOD5_ChannelGroup_Set3DAttributes_Delegate FMOD5_ChannelGroup_Set3DAttributes => FMOD5_ChannelGroup_Set3DAttributes_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_Set3DAttributes_Delegate>(nameof(FMOD5_ChannelGroup_Set3DAttributes));

        private delegate RESULT FMOD5_ChannelGroup_Get3DAttributes_Delegate(IntPtr channelgroup, out VECTOR pos, out VECTOR vel);
        private static FMOD5_ChannelGroup_Get3DAttributes_Delegate FMOD5_ChannelGroup_Get3DAttributes_Internal = null;
        private static FMOD5_ChannelGroup_Get3DAttributes_Delegate FMOD5_ChannelGroup_Get3DAttributes => FMOD5_ChannelGroup_Get3DAttributes_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_Get3DAttributes_Delegate>(nameof(FMOD5_ChannelGroup_Get3DAttributes));

        private delegate RESULT FMOD5_ChannelGroup_Set3DMinMaxDistance_Delegate(IntPtr channelgroup, float mindistance, float maxdistance);
        private static FMOD5_ChannelGroup_Set3DMinMaxDistance_Delegate FMOD5_ChannelGroup_Set3DMinMaxDistance_Internal = null;
        private static FMOD5_ChannelGroup_Set3DMinMaxDistance_Delegate FMOD5_ChannelGroup_Set3DMinMaxDistance => FMOD5_ChannelGroup_Set3DMinMaxDistance_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_Set3DMinMaxDistance_Delegate>(nameof(FMOD5_ChannelGroup_Set3DMinMaxDistance));

        private delegate RESULT FMOD5_ChannelGroup_Get3DMinMaxDistance_Delegate(IntPtr channelgroup, out float mindistance, out float maxdistance);
        private static FMOD5_ChannelGroup_Get3DMinMaxDistance_Delegate FMOD5_ChannelGroup_Get3DMinMaxDistance_Internal = null;
        private static FMOD5_ChannelGroup_Get3DMinMaxDistance_Delegate FMOD5_ChannelGroup_Get3DMinMaxDistance => FMOD5_ChannelGroup_Get3DMinMaxDistance_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_Get3DMinMaxDistance_Delegate>(nameof(FMOD5_ChannelGroup_Get3DMinMaxDistance));

        private delegate RESULT FMOD5_ChannelGroup_Set3DConeSettings_Delegate(IntPtr channelgroup, float insideconeangle, float outsideconeangle, float outsidevolume);
        private static FMOD5_ChannelGroup_Set3DConeSettings_Delegate FMOD5_ChannelGroup_Set3DConeSettings_Internal = null;
        private static FMOD5_ChannelGroup_Set3DConeSettings_Delegate FMOD5_ChannelGroup_Set3DConeSettings => FMOD5_ChannelGroup_Set3DConeSettings_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_Set3DConeSettings_Delegate>(nameof(FMOD5_ChannelGroup_Set3DConeSettings));

        private delegate RESULT FMOD5_ChannelGroup_Get3DConeSettings_Delegate(IntPtr channelgroup, out float insideconeangle, out float outsideconeangle, out float outsidevolume);
        private static FMOD5_ChannelGroup_Get3DConeSettings_Delegate FMOD5_ChannelGroup_Get3DConeSettings_Internal = null;
        private static FMOD5_ChannelGroup_Get3DConeSettings_Delegate FMOD5_ChannelGroup_Get3DConeSettings => FMOD5_ChannelGroup_Get3DConeSettings_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_Get3DConeSettings_Delegate>(nameof(FMOD5_ChannelGroup_Get3DConeSettings));

        private delegate RESULT FMOD5_ChannelGroup_Set3DConeOrientation_Delegate(IntPtr channelgroup, ref VECTOR orientation);
        private static FMOD5_ChannelGroup_Set3DConeOrientation_Delegate FMOD5_ChannelGroup_Set3DConeOrientation_Internal = null;
        private static FMOD5_ChannelGroup_Set3DConeOrientation_Delegate FMOD5_ChannelGroup_Set3DConeOrientation => FMOD5_ChannelGroup_Set3DConeOrientation_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_Set3DConeOrientation_Delegate>(nameof(FMOD5_ChannelGroup_Set3DConeOrientation));

        private delegate RESULT FMOD5_ChannelGroup_Get3DConeOrientation_Delegate(IntPtr channelgroup, out VECTOR orientation);
        private static FMOD5_ChannelGroup_Get3DConeOrientation_Delegate FMOD5_ChannelGroup_Get3DConeOrientation_Internal = null;
        private static FMOD5_ChannelGroup_Get3DConeOrientation_Delegate FMOD5_ChannelGroup_Get3DConeOrientation => FMOD5_ChannelGroup_Get3DConeOrientation_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_Get3DConeOrientation_Delegate>(nameof(FMOD5_ChannelGroup_Get3DConeOrientation));

        private delegate RESULT FMOD5_ChannelGroup_Set3DCustomRolloff_Delegate(IntPtr channelgroup, ref VECTOR points, int numpoints);
        private static FMOD5_ChannelGroup_Set3DCustomRolloff_Delegate FMOD5_ChannelGroup_Set3DCustomRolloff_Internal = null;
        private static FMOD5_ChannelGroup_Set3DCustomRolloff_Delegate FMOD5_ChannelGroup_Set3DCustomRolloff => FMOD5_ChannelGroup_Set3DCustomRolloff_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_Set3DCustomRolloff_Delegate>(nameof(FMOD5_ChannelGroup_Set3DCustomRolloff));

        private delegate RESULT FMOD5_ChannelGroup_Get3DCustomRolloff_Delegate(IntPtr channelgroup, out IntPtr points, out int numpoints);
        private static FMOD5_ChannelGroup_Get3DCustomRolloff_Delegate FMOD5_ChannelGroup_Get3DCustomRolloff_Internal = null;
        private static FMOD5_ChannelGroup_Get3DCustomRolloff_Delegate FMOD5_ChannelGroup_Get3DCustomRolloff => FMOD5_ChannelGroup_Get3DCustomRolloff_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_Get3DCustomRolloff_Delegate>(nameof(FMOD5_ChannelGroup_Get3DCustomRolloff));

        private delegate RESULT FMOD5_ChannelGroup_Set3DOcclusion_Delegate(IntPtr channelgroup, float directocclusion, float reverbocclusion);
        private static FMOD5_ChannelGroup_Set3DOcclusion_Delegate FMOD5_ChannelGroup_Set3DOcclusion_Internal = null;
        private static FMOD5_ChannelGroup_Set3DOcclusion_Delegate FMOD5_ChannelGroup_Set3DOcclusion => FMOD5_ChannelGroup_Set3DOcclusion_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_Set3DOcclusion_Delegate>(nameof(FMOD5_ChannelGroup_Set3DOcclusion));

        private delegate RESULT FMOD5_ChannelGroup_Get3DOcclusion_Delegate(IntPtr channelgroup, out float directocclusion, out float reverbocclusion);
        private static FMOD5_ChannelGroup_Get3DOcclusion_Delegate FMOD5_ChannelGroup_Get3DOcclusion_Internal = null;
        private static FMOD5_ChannelGroup_Get3DOcclusion_Delegate FMOD5_ChannelGroup_Get3DOcclusion => FMOD5_ChannelGroup_Get3DOcclusion_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_Get3DOcclusion_Delegate>(nameof(FMOD5_ChannelGroup_Get3DOcclusion));

        private delegate RESULT FMOD5_ChannelGroup_Set3DSpread_Delegate(IntPtr channelgroup, float angle);
        private static FMOD5_ChannelGroup_Set3DSpread_Delegate FMOD5_ChannelGroup_Set3DSpread_Internal = null;
        private static FMOD5_ChannelGroup_Set3DSpread_Delegate FMOD5_ChannelGroup_Set3DSpread => FMOD5_ChannelGroup_Set3DSpread_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_Set3DSpread_Delegate>(nameof(FMOD5_ChannelGroup_Set3DSpread));

        private delegate RESULT FMOD5_ChannelGroup_Get3DSpread_Delegate(IntPtr channelgroup, out float angle);
        private static FMOD5_ChannelGroup_Get3DSpread_Delegate FMOD5_ChannelGroup_Get3DSpread_Internal = null;
        private static FMOD5_ChannelGroup_Get3DSpread_Delegate FMOD5_ChannelGroup_Get3DSpread => FMOD5_ChannelGroup_Get3DSpread_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_Get3DSpread_Delegate>(nameof(FMOD5_ChannelGroup_Get3DSpread));

        private delegate RESULT FMOD5_ChannelGroup_Set3DLevel_Delegate(IntPtr channelgroup, float level);
        private static FMOD5_ChannelGroup_Set3DLevel_Delegate FMOD5_ChannelGroup_Set3DLevel_Internal = null;
        private static FMOD5_ChannelGroup_Set3DLevel_Delegate FMOD5_ChannelGroup_Set3DLevel => FMOD5_ChannelGroup_Set3DLevel_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_Set3DLevel_Delegate>(nameof(FMOD5_ChannelGroup_Set3DLevel));

        private delegate RESULT FMOD5_ChannelGroup_Get3DLevel_Delegate(IntPtr channelgroup, out float level);
        private static FMOD5_ChannelGroup_Get3DLevel_Delegate FMOD5_ChannelGroup_Get3DLevel_Internal = null;
        private static FMOD5_ChannelGroup_Get3DLevel_Delegate FMOD5_ChannelGroup_Get3DLevel => FMOD5_ChannelGroup_Get3DLevel_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_Get3DLevel_Delegate>(nameof(FMOD5_ChannelGroup_Get3DLevel));

        private delegate RESULT FMOD5_ChannelGroup_Set3DDopplerLevel_Delegate(IntPtr channelgroup, float level);
        private static FMOD5_ChannelGroup_Set3DDopplerLevel_Delegate FMOD5_ChannelGroup_Set3DDopplerLevel_Internal = null;
        private static FMOD5_ChannelGroup_Set3DDopplerLevel_Delegate FMOD5_ChannelGroup_Set3DDopplerLevel => FMOD5_ChannelGroup_Set3DDopplerLevel_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_Set3DDopplerLevel_Delegate>(nameof(FMOD5_ChannelGroup_Set3DDopplerLevel));

        private delegate RESULT FMOD5_ChannelGroup_Get3DDopplerLevel_Delegate(IntPtr channelgroup, out float level);
        private static FMOD5_ChannelGroup_Get3DDopplerLevel_Delegate FMOD5_ChannelGroup_Get3DDopplerLevel_Internal = null;
        private static FMOD5_ChannelGroup_Get3DDopplerLevel_Delegate FMOD5_ChannelGroup_Get3DDopplerLevel => FMOD5_ChannelGroup_Get3DDopplerLevel_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_Get3DDopplerLevel_Delegate>(nameof(FMOD5_ChannelGroup_Get3DDopplerLevel));

        private delegate RESULT FMOD5_ChannelGroup_Set3DDistanceFilter_Delegate(IntPtr channelgroup, bool custom, float customLevel, float centerFreq);
        private static FMOD5_ChannelGroup_Set3DDistanceFilter_Delegate FMOD5_ChannelGroup_Set3DDistanceFilter_Internal = null;
        private static FMOD5_ChannelGroup_Set3DDistanceFilter_Delegate FMOD5_ChannelGroup_Set3DDistanceFilter => FMOD5_ChannelGroup_Set3DDistanceFilter_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_Set3DDistanceFilter_Delegate>(nameof(FMOD5_ChannelGroup_Set3DDistanceFilter));

        private delegate RESULT FMOD5_ChannelGroup_Get3DDistanceFilter_Delegate(IntPtr channelgroup, out bool custom, out float customLevel, out float centerFreq);
        private static FMOD5_ChannelGroup_Get3DDistanceFilter_Delegate FMOD5_ChannelGroup_Get3DDistanceFilter_Internal = null;
        private static FMOD5_ChannelGroup_Get3DDistanceFilter_Delegate FMOD5_ChannelGroup_Get3DDistanceFilter => FMOD5_ChannelGroup_Get3DDistanceFilter_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_Get3DDistanceFilter_Delegate>(nameof(FMOD5_ChannelGroup_Get3DDistanceFilter));

        private delegate RESULT FMOD5_ChannelGroup_SetUserData_Delegate(IntPtr channelgroup, IntPtr userdata);
        private static FMOD5_ChannelGroup_SetUserData_Delegate FMOD5_ChannelGroup_SetUserData_Internal = null;
        private static FMOD5_ChannelGroup_SetUserData_Delegate FMOD5_ChannelGroup_SetUserData => FMOD5_ChannelGroup_SetUserData_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_SetUserData_Delegate>(nameof(FMOD5_ChannelGroup_SetUserData));

        private delegate RESULT FMOD5_ChannelGroup_GetUserData_Delegate(IntPtr channelgroup, out IntPtr userdata);
        private static FMOD5_ChannelGroup_GetUserData_Delegate FMOD5_ChannelGroup_GetUserData_Internal = null;
        private static FMOD5_ChannelGroup_GetUserData_Delegate FMOD5_ChannelGroup_GetUserData => FMOD5_ChannelGroup_GetUserData_Internal ??= FModManager.GetProcInFModStudio<FMOD5_ChannelGroup_GetUserData_Delegate>(nameof(FMOD5_ChannelGroup_GetUserData));

        #endregion

        #region wrapperinternal

        public IntPtr handle;

        public ChannelGroup(IntPtr ptr) { this.handle = ptr; }
        public bool hasHandle()         { return this.handle != IntPtr.Zero; }
        public void clearHandle()       { this.handle = IntPtr.Zero; }

        #endregion
    }

    /*
        'SoundGroup' API
    */
    public struct SoundGroup
    {
        public RESULT release()
        {
            return FMOD5_SoundGroup_Release(this.handle);
        }

        public RESULT getSystemObject(out System system)
        {
            return FMOD5_SoundGroup_GetSystemObject(this.handle, out system.handle);
        }

        // SoundGroup control functions.
        public RESULT setMaxAudible(int maxaudible)
        {
            return FMOD5_SoundGroup_SetMaxAudible(this.handle, maxaudible);
        }
        public RESULT getMaxAudible(out int maxaudible)
        {
            return FMOD5_SoundGroup_GetMaxAudible(this.handle, out maxaudible);
        }
        public RESULT setMaxAudibleBehavior(SOUNDGROUP_BEHAVIOR behavior)
        {
            return FMOD5_SoundGroup_SetMaxAudibleBehavior(this.handle, behavior);
        }
        public RESULT getMaxAudibleBehavior(out SOUNDGROUP_BEHAVIOR behavior)
        {
            return FMOD5_SoundGroup_GetMaxAudibleBehavior(this.handle, out behavior);
        }
        public RESULT setMuteFadeSpeed(float speed)
        {
            return FMOD5_SoundGroup_SetMuteFadeSpeed(this.handle, speed);
        }
        public RESULT getMuteFadeSpeed(out float speed)
        {
            return FMOD5_SoundGroup_GetMuteFadeSpeed(this.handle, out speed);
        }
        public RESULT setVolume(float volume)
        {
            return FMOD5_SoundGroup_SetVolume(this.handle, volume);
        }
        public RESULT getVolume(out float volume)
        {
            return FMOD5_SoundGroup_GetVolume(this.handle, out volume);
        }
        public RESULT stop()
        {
            return FMOD5_SoundGroup_Stop(this.handle);
        }

        // Information only functions.
        public RESULT getName(out string name, int namelen)
        {
            IntPtr stringMem = Marshal.AllocHGlobal(namelen);

            RESULT result = FMOD5_SoundGroup_GetName(this.handle, stringMem, namelen);
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                name = encoder.stringFromNative(stringMem);
            }
            Marshal.FreeHGlobal(stringMem);

            return result;
        }
        public RESULT getNumSounds(out int numsounds)
        {
            return FMOD5_SoundGroup_GetNumSounds(this.handle, out numsounds);
        }
        public RESULT getSound(int index, out Sound sound)
        {
            return FMOD5_SoundGroup_GetSound(this.handle, index, out sound.handle);
        }
        public RESULT getNumPlaying(out int numplaying)
        {
            return FMOD5_SoundGroup_GetNumPlaying(this.handle, out numplaying);
        }

        // Userdata set/get.
        public RESULT setUserData(IntPtr userdata)
        {
            return FMOD5_SoundGroup_SetUserData(this.handle, userdata);
        }
        public RESULT getUserData(out IntPtr userdata)
        {
            return FMOD5_SoundGroup_GetUserData(this.handle, out userdata);
        }

        #region importfunctions
        private delegate RESULT FMOD5_SoundGroup_Release_Delegate(IntPtr soundgroup);
        private static FMOD5_SoundGroup_Release_Delegate FMOD5_SoundGroup_Release_Internal = null;
        private static FMOD5_SoundGroup_Release_Delegate FMOD5_SoundGroup_Release => FMOD5_SoundGroup_Release_Internal ??= FModManager.GetProcInFModStudio<FMOD5_SoundGroup_Release_Delegate>(nameof(FMOD5_SoundGroup_Release));

        private delegate RESULT FMOD5_SoundGroup_GetSystemObject_Delegate(IntPtr soundgroup, out IntPtr system);
        private static FMOD5_SoundGroup_GetSystemObject_Delegate FMOD5_SoundGroup_GetSystemObject_Internal = null;
        private static FMOD5_SoundGroup_GetSystemObject_Delegate FMOD5_SoundGroup_GetSystemObject => FMOD5_SoundGroup_GetSystemObject_Internal ??= FModManager.GetProcInFModStudio<FMOD5_SoundGroup_GetSystemObject_Delegate>(nameof(FMOD5_SoundGroup_GetSystemObject));

        private delegate RESULT FMOD5_SoundGroup_SetMaxAudible_Delegate(IntPtr soundgroup, int maxaudible);
        private static FMOD5_SoundGroup_SetMaxAudible_Delegate FMOD5_SoundGroup_SetMaxAudible_Internal = null;
        private static FMOD5_SoundGroup_SetMaxAudible_Delegate FMOD5_SoundGroup_SetMaxAudible => FMOD5_SoundGroup_SetMaxAudible_Internal ??= FModManager.GetProcInFModStudio<FMOD5_SoundGroup_SetMaxAudible_Delegate>(nameof(FMOD5_SoundGroup_SetMaxAudible));

        private delegate RESULT FMOD5_SoundGroup_GetMaxAudible_Delegate(IntPtr soundgroup, out int maxaudible);
        private static FMOD5_SoundGroup_GetMaxAudible_Delegate FMOD5_SoundGroup_GetMaxAudible_Internal = null;
        private static FMOD5_SoundGroup_GetMaxAudible_Delegate FMOD5_SoundGroup_GetMaxAudible => FMOD5_SoundGroup_GetMaxAudible_Internal ??= FModManager.GetProcInFModStudio<FMOD5_SoundGroup_GetMaxAudible_Delegate>(nameof(FMOD5_SoundGroup_GetMaxAudible));

        private delegate RESULT FMOD5_SoundGroup_SetMaxAudibleBehavior_Delegate(IntPtr soundgroup, SOUNDGROUP_BEHAVIOR behavior);
        private static FMOD5_SoundGroup_SetMaxAudibleBehavior_Delegate FMOD5_SoundGroup_SetMaxAudibleBehavior_Internal = null;
        private static FMOD5_SoundGroup_SetMaxAudibleBehavior_Delegate FMOD5_SoundGroup_SetMaxAudibleBehavior => FMOD5_SoundGroup_SetMaxAudibleBehavior_Internal ??= FModManager.GetProcInFModStudio<FMOD5_SoundGroup_SetMaxAudibleBehavior_Delegate>(nameof(FMOD5_SoundGroup_SetMaxAudibleBehavior));

        private delegate RESULT FMOD5_SoundGroup_GetMaxAudibleBehavior_Delegate(IntPtr soundgroup, out SOUNDGROUP_BEHAVIOR behavior);
        private static FMOD5_SoundGroup_GetMaxAudibleBehavior_Delegate FMOD5_SoundGroup_GetMaxAudibleBehavior_Internal = null;
        private static FMOD5_SoundGroup_GetMaxAudibleBehavior_Delegate FMOD5_SoundGroup_GetMaxAudibleBehavior => FMOD5_SoundGroup_GetMaxAudibleBehavior_Internal ??= FModManager.GetProcInFModStudio<FMOD5_SoundGroup_GetMaxAudibleBehavior_Delegate>(nameof(FMOD5_SoundGroup_GetMaxAudibleBehavior));

        private delegate RESULT FMOD5_SoundGroup_SetMuteFadeSpeed_Delegate(IntPtr soundgroup, float speed);
        private static FMOD5_SoundGroup_SetMuteFadeSpeed_Delegate FMOD5_SoundGroup_SetMuteFadeSpeed_Internal = null;
        private static FMOD5_SoundGroup_SetMuteFadeSpeed_Delegate FMOD5_SoundGroup_SetMuteFadeSpeed => FMOD5_SoundGroup_SetMuteFadeSpeed_Internal ??= FModManager.GetProcInFModStudio<FMOD5_SoundGroup_SetMuteFadeSpeed_Delegate>(nameof(FMOD5_SoundGroup_SetMuteFadeSpeed));

        private delegate RESULT FMOD5_SoundGroup_GetMuteFadeSpeed_Delegate(IntPtr soundgroup, out float speed);
        private static FMOD5_SoundGroup_GetMuteFadeSpeed_Delegate FMOD5_SoundGroup_GetMuteFadeSpeed_Internal = null;
        private static FMOD5_SoundGroup_GetMuteFadeSpeed_Delegate FMOD5_SoundGroup_GetMuteFadeSpeed => FMOD5_SoundGroup_GetMuteFadeSpeed_Internal ??= FModManager.GetProcInFModStudio<FMOD5_SoundGroup_GetMuteFadeSpeed_Delegate>(nameof(FMOD5_SoundGroup_GetMuteFadeSpeed));

        private delegate RESULT FMOD5_SoundGroup_SetVolume_Delegate(IntPtr soundgroup, float volume);
        private static FMOD5_SoundGroup_SetVolume_Delegate FMOD5_SoundGroup_SetVolume_Internal = null;
        private static FMOD5_SoundGroup_SetVolume_Delegate FMOD5_SoundGroup_SetVolume => FMOD5_SoundGroup_SetVolume_Internal ??= FModManager.GetProcInFModStudio<FMOD5_SoundGroup_SetVolume_Delegate>(nameof(FMOD5_SoundGroup_SetVolume));

        private delegate RESULT FMOD5_SoundGroup_GetVolume_Delegate(IntPtr soundgroup, out float volume);
        private static FMOD5_SoundGroup_GetVolume_Delegate FMOD5_SoundGroup_GetVolume_Internal = null;
        private static FMOD5_SoundGroup_GetVolume_Delegate FMOD5_SoundGroup_GetVolume => FMOD5_SoundGroup_GetVolume_Internal ??= FModManager.GetProcInFModStudio<FMOD5_SoundGroup_GetVolume_Delegate>(nameof(FMOD5_SoundGroup_GetVolume));

        private delegate RESULT FMOD5_SoundGroup_Stop_Delegate(IntPtr soundgroup);
        private static FMOD5_SoundGroup_Stop_Delegate FMOD5_SoundGroup_Stop_Internal = null;
        private static FMOD5_SoundGroup_Stop_Delegate FMOD5_SoundGroup_Stop => FMOD5_SoundGroup_Stop_Internal ??= FModManager.GetProcInFModStudio<FMOD5_SoundGroup_Stop_Delegate>(nameof(FMOD5_SoundGroup_Stop));

        private delegate RESULT FMOD5_SoundGroup_GetName_Delegate(IntPtr soundgroup, IntPtr name, int namelen);
        private static FMOD5_SoundGroup_GetName_Delegate FMOD5_SoundGroup_GetName_Internal = null;
        private static FMOD5_SoundGroup_GetName_Delegate FMOD5_SoundGroup_GetName => FMOD5_SoundGroup_GetName_Internal ??= FModManager.GetProcInFModStudio<FMOD5_SoundGroup_GetName_Delegate>(nameof(FMOD5_SoundGroup_GetName));

        private delegate RESULT FMOD5_SoundGroup_GetNumSounds_Delegate(IntPtr soundgroup, out int numsounds);
        private static FMOD5_SoundGroup_GetNumSounds_Delegate FMOD5_SoundGroup_GetNumSounds_Internal = null;
        private static FMOD5_SoundGroup_GetNumSounds_Delegate FMOD5_SoundGroup_GetNumSounds => FMOD5_SoundGroup_GetNumSounds_Internal ??= FModManager.GetProcInFModStudio<FMOD5_SoundGroup_GetNumSounds_Delegate>(nameof(FMOD5_SoundGroup_GetNumSounds));

        private delegate RESULT FMOD5_SoundGroup_GetSound_Delegate(IntPtr soundgroup, int index, out IntPtr sound);
        private static FMOD5_SoundGroup_GetSound_Delegate FMOD5_SoundGroup_GetSound_Internal = null;
        private static FMOD5_SoundGroup_GetSound_Delegate FMOD5_SoundGroup_GetSound => FMOD5_SoundGroup_GetSound_Internal ??= FModManager.GetProcInFModStudio<FMOD5_SoundGroup_GetSound_Delegate>(nameof(FMOD5_SoundGroup_GetSound));

        private delegate RESULT FMOD5_SoundGroup_GetNumPlaying_Delegate(IntPtr soundgroup, out int numplaying);
        private static FMOD5_SoundGroup_GetNumPlaying_Delegate FMOD5_SoundGroup_GetNumPlaying_Internal = null;
        private static FMOD5_SoundGroup_GetNumPlaying_Delegate FMOD5_SoundGroup_GetNumPlaying => FMOD5_SoundGroup_GetNumPlaying_Internal ??= FModManager.GetProcInFModStudio<FMOD5_SoundGroup_GetNumPlaying_Delegate>(nameof(FMOD5_SoundGroup_GetNumPlaying));

        private delegate RESULT FMOD5_SoundGroup_SetUserData_Delegate(IntPtr soundgroup, IntPtr userdata);
        private static FMOD5_SoundGroup_SetUserData_Delegate FMOD5_SoundGroup_SetUserData_Internal = null;
        private static FMOD5_SoundGroup_SetUserData_Delegate FMOD5_SoundGroup_SetUserData => FMOD5_SoundGroup_SetUserData_Internal ??= FModManager.GetProcInFModStudio<FMOD5_SoundGroup_SetUserData_Delegate>(nameof(FMOD5_SoundGroup_SetUserData));

        private delegate RESULT FMOD5_SoundGroup_GetUserData_Delegate(IntPtr soundgroup, out IntPtr userdata);
        private static FMOD5_SoundGroup_GetUserData_Delegate FMOD5_SoundGroup_GetUserData_Internal = null;
        private static FMOD5_SoundGroup_GetUserData_Delegate FMOD5_SoundGroup_GetUserData => FMOD5_SoundGroup_GetUserData_Internal ??= FModManager.GetProcInFModStudio<FMOD5_SoundGroup_GetUserData_Delegate>(nameof(FMOD5_SoundGroup_GetUserData));

        #endregion

        #region wrapperinternal

        public IntPtr handle;

        public SoundGroup(IntPtr ptr) { this.handle = ptr; }
        public bool hasHandle()       { return this.handle != IntPtr.Zero; }
        public void clearHandle()     { this.handle = IntPtr.Zero; }

        #endregion
    }

    /*
        'DSP' API
    */
    public struct DSP
    {
        public RESULT release()
        {
            return FMOD5_DSP_Release(this.handle);
        }
        public RESULT getSystemObject(out System system)
        {
            return FMOD5_DSP_GetSystemObject(this.handle, out system.handle);
        }

        // Connection / disconnection / input and output enumeration.
        public RESULT addInput(DSP input)
        {
            return FMOD5_DSP_AddInput(this.handle, input.handle, IntPtr.Zero, DSPCONNECTION_TYPE.STANDARD);
        }
        public RESULT addInput(DSP input, out DSPConnection connection, DSPCONNECTION_TYPE type = DSPCONNECTION_TYPE.STANDARD)
        {
            return FMOD5_DSP_AddInput2(this.handle, input.handle, out connection.handle, type);
        }
        public RESULT disconnectFrom(DSP target, DSPConnection connection)
        {
            return FMOD5_DSP_DisconnectFrom(this.handle, target.handle, connection.handle);
        }
        public RESULT disconnectAll(bool inputs, bool outputs)
        {
            return FMOD5_DSP_DisconnectAll(this.handle, inputs, outputs);
        }
        public RESULT getNumInputs(out int numinputs)
        {
            return FMOD5_DSP_GetNumInputs(this.handle, out numinputs);
        }
        public RESULT getNumOutputs(out int numoutputs)
        {
            return FMOD5_DSP_GetNumOutputs(this.handle, out numoutputs);
        }
        public RESULT getInput(int index, out DSP input, out DSPConnection inputconnection)
        {
            return FMOD5_DSP_GetInput(this.handle, index, out input.handle, out inputconnection.handle);
        }
        public RESULT getOutput(int index, out DSP output, out DSPConnection outputconnection)
        {
            return FMOD5_DSP_GetOutput(this.handle, index, out output.handle, out outputconnection.handle);
        }

        // DSP unit control.
        public RESULT setActive(bool active)
        {
            return FMOD5_DSP_SetActive(this.handle, active);
        }
        public RESULT getActive(out bool active)
        {
            return FMOD5_DSP_GetActive(this.handle, out active);
        }
        public RESULT setBypass(bool bypass)
        {
            return FMOD5_DSP_SetBypass(this.handle, bypass);
        }
        public RESULT getBypass(out bool bypass)
        {
            return FMOD5_DSP_GetBypass(this.handle, out bypass);
        }
        public RESULT setWetDryMix(float prewet, float postwet, float dry)
        {
            return FMOD5_DSP_SetWetDryMix(this.handle, prewet, postwet, dry);
        }
        public RESULT getWetDryMix(out float prewet, out float postwet, out float dry)
        {
            return FMOD5_DSP_GetWetDryMix(this.handle, out prewet, out postwet, out dry);
        }
        public RESULT setChannelFormat(CHANNELMASK channelmask, int numchannels, SPEAKERMODE source_speakermode)
        {
            return FMOD5_DSP_SetChannelFormat(this.handle, channelmask, numchannels, source_speakermode);
        }
        public RESULT getChannelFormat(out CHANNELMASK channelmask, out int numchannels, out SPEAKERMODE source_speakermode)
        {
            return FMOD5_DSP_GetChannelFormat(this.handle, out channelmask, out numchannels, out source_speakermode);
        }
        public RESULT getOutputChannelFormat(CHANNELMASK inmask, int inchannels, SPEAKERMODE inspeakermode, out CHANNELMASK outmask, out int outchannels, out SPEAKERMODE outspeakermode)
        {
            return FMOD5_DSP_GetOutputChannelFormat(this.handle, inmask, inchannels, inspeakermode, out outmask, out outchannels, out outspeakermode);
        }
        public RESULT reset()
        {
            return FMOD5_DSP_Reset(this.handle);
        }
        public RESULT setCallback(DSP_CALLBACK callback)
        {
            return FMOD5_DSP_SetCallback(this.handle, callback);
        }

        // DSP parameter control.
        public RESULT setParameterFloat(int index, float value)
        {
            return FMOD5_DSP_SetParameterFloat(this.handle, index, value);
        }
        public RESULT setParameterInt(int index, int value)
        {
            return FMOD5_DSP_SetParameterInt(this.handle, index, value);
        }
        public RESULT setParameterBool(int index, bool value)
        {
            return FMOD5_DSP_SetParameterBool(this.handle, index, value);
        }
        public RESULT setParameterData(int index, byte[] data)
        {
            return FMOD5_DSP_SetParameterData(this.handle, index, Marshal.UnsafeAddrOfPinnedArrayElement(data, 0), (uint)data.Length);
        }
        public RESULT getParameterFloat(int index, out float value)
        {
            return FMOD5_DSP_GetParameterFloat(this.handle, index, out value, IntPtr.Zero, 0);
        }
        public RESULT getParameterInt(int index, out int value)
        {
            return FMOD5_DSP_GetParameterInt(this.handle, index, out value, IntPtr.Zero, 0);
        }
        public RESULT getParameterBool(int index, out bool value)
        {
            return FMOD5_DSP_GetParameterBool(this.handle, index, out value, IntPtr.Zero, 0);
        }
        public RESULT getParameterData(int index, out IntPtr data, out uint length)
        {
            return FMOD5_DSP_GetParameterData(this.handle, index, out data, out length, IntPtr.Zero, 0);
        }
        public RESULT getNumParameters(out int numparams)
        {
            return FMOD5_DSP_GetNumParameters(this.handle, out numparams);
        }
        public RESULT getParameterInfo(int index, out DSP_PARAMETER_DESC desc)
        {
            IntPtr descPtr;
            RESULT result = FMOD5_DSP_GetParameterInfo(this.handle, index, out descPtr);
            desc = (DSP_PARAMETER_DESC)MarshalHelper.PtrToStructure(descPtr, typeof(DSP_PARAMETER_DESC));
            return result;
        }
        public RESULT getDataParameterIndex(int datatype, out int index)
        {
            return FMOD5_DSP_GetDataParameterIndex(this.handle, datatype, out index);
        }
        public RESULT showConfigDialog(IntPtr hwnd, bool show)
        {
            return FMOD5_DSP_ShowConfigDialog(this.handle, hwnd, show);
        }

        //  DSP attributes.
        public RESULT getInfo(out string name, out uint version, out int channels, out int configwidth, out int configheight)
        {
            IntPtr nameMem = Marshal.AllocHGlobal(32);

            RESULT result = FMOD5_DSP_GetInfo(this.handle, nameMem, out version, out channels, out configwidth, out configheight);
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                name = encoder.stringFromNative(nameMem);
            }
            Marshal.FreeHGlobal(nameMem);
            return result;
        }
        public RESULT getInfo(out uint version, out int channels, out int configwidth, out int configheight)
        {
            return FMOD5_DSP_GetInfo(this.handle, IntPtr.Zero, out version, out channels, out configwidth, out configheight); ;
        }
        public RESULT getType(out DSP_TYPE type)
        {
            return FMOD5_DSP_GetType(this.handle, out type);
        }
        public RESULT getIdle(out bool idle)
        {
            return FMOD5_DSP_GetIdle(this.handle, out idle);
        }

        // Userdata set/get.
        public RESULT setUserData(IntPtr userdata)
        {
            return FMOD5_DSP_SetUserData(this.handle, userdata);
        }
        public RESULT getUserData(out IntPtr userdata)
        {
            return FMOD5_DSP_GetUserData(this.handle, out userdata);
        }

        // Metering.
        public RESULT setMeteringEnabled(bool inputEnabled, bool outputEnabled)
        {
            return FMOD5_DSP_SetMeteringEnabled(this.handle, inputEnabled, outputEnabled);
        }
        public RESULT getMeteringEnabled(out bool inputEnabled, out bool outputEnabled)
        {
            return FMOD5_DSP_GetMeteringEnabled(this.handle, out inputEnabled, out outputEnabled);
        }

        public RESULT getMeteringInfo(IntPtr zero, out DSP_METERING_INFO outputInfo)
        {
            return FMOD5_DSP_GetMeteringInfo(this.handle, zero, out outputInfo);
        }
        public RESULT getMeteringInfo(out DSP_METERING_INFO inputInfo, IntPtr zero)
        {
            return FMOD5_DSP_GetMeteringInfo2(this.handle, out inputInfo, zero);
        }
        public RESULT getMeteringInfo(out DSP_METERING_INFO inputInfo, out DSP_METERING_INFO outputInfo)
        {
            return FMOD5_DSP_GetMeteringInfo3(this.handle, out inputInfo, out outputInfo);
        }

        public RESULT getCPUUsage(out uint exclusive, out uint inclusive)
        {
            return FMOD5_DSP_GetCPUUsage(this.handle, out exclusive, out inclusive);
        }

        #region importfunctions
        private delegate RESULT FMOD5_DSP_Release_Delegate(IntPtr dsp);
        private static FMOD5_DSP_Release_Delegate FMOD5_DSP_Release_Internal = null;
        private static FMOD5_DSP_Release_Delegate FMOD5_DSP_Release => FMOD5_DSP_Release_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_Release_Delegate>(nameof(FMOD5_DSP_Release));

        private delegate RESULT FMOD5_DSP_GetSystemObject_Delegate(IntPtr dsp, out IntPtr system);
        private static FMOD5_DSP_GetSystemObject_Delegate FMOD5_DSP_GetSystemObject_Internal = null;
        private static FMOD5_DSP_GetSystemObject_Delegate FMOD5_DSP_GetSystemObject => FMOD5_DSP_GetSystemObject_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_GetSystemObject_Delegate>(nameof(FMOD5_DSP_GetSystemObject));

        private delegate RESULT FMOD5_DSP_AddInput_Delegate(IntPtr dsp, IntPtr input, IntPtr zero, DSPCONNECTION_TYPE type);
        private static FMOD5_DSP_AddInput_Delegate FMOD5_DSP_AddInput_Internal = null;
        private static FMOD5_DSP_AddInput_Delegate FMOD5_DSP_AddInput => FMOD5_DSP_AddInput_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_AddInput_Delegate>(nameof(FMOD5_DSP_AddInput));

        private delegate RESULT FMOD5_DSP_AddInput_Delegate2(IntPtr dsp, IntPtr input, out IntPtr connection, DSPCONNECTION_TYPE type);
        private static FMOD5_DSP_AddInput_Delegate2 FMOD5_DSP_AddInput_Internal2 = null;
        private static FMOD5_DSP_AddInput_Delegate2 FMOD5_DSP_AddInput2 => FMOD5_DSP_AddInput_Internal2 ??= FModManager.GetProcInFModStudio<FMOD5_DSP_AddInput_Delegate2>(nameof(FMOD5_DSP_AddInput));

        private delegate RESULT FMOD5_DSP_DisconnectFrom_Delegate(IntPtr dsp, IntPtr target, IntPtr connection);
        private static FMOD5_DSP_DisconnectFrom_Delegate FMOD5_DSP_DisconnectFrom_Internal = null;
        private static FMOD5_DSP_DisconnectFrom_Delegate FMOD5_DSP_DisconnectFrom => FMOD5_DSP_DisconnectFrom_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_DisconnectFrom_Delegate>(nameof(FMOD5_DSP_DisconnectFrom));

        private delegate RESULT FMOD5_DSP_DisconnectAll_Delegate(IntPtr dsp, bool inputs, bool outputs);
        private static FMOD5_DSP_DisconnectAll_Delegate FMOD5_DSP_DisconnectAll_Internal = null;
        private static FMOD5_DSP_DisconnectAll_Delegate FMOD5_DSP_DisconnectAll => FMOD5_DSP_DisconnectAll_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_DisconnectAll_Delegate>(nameof(FMOD5_DSP_DisconnectAll));

        private delegate RESULT FMOD5_DSP_GetNumInputs_Delegate(IntPtr dsp, out int numinputs);
        private static FMOD5_DSP_GetNumInputs_Delegate FMOD5_DSP_GetNumInputs_Internal = null;
        private static FMOD5_DSP_GetNumInputs_Delegate FMOD5_DSP_GetNumInputs => FMOD5_DSP_GetNumInputs_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_GetNumInputs_Delegate>(nameof(FMOD5_DSP_GetNumInputs));

        private delegate RESULT FMOD5_DSP_GetNumOutputs_Delegate(IntPtr dsp, out int numoutputs);
        private static FMOD5_DSP_GetNumOutputs_Delegate FMOD5_DSP_GetNumOutputs_Internal = null;
        private static FMOD5_DSP_GetNumOutputs_Delegate FMOD5_DSP_GetNumOutputs => FMOD5_DSP_GetNumOutputs_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_GetNumOutputs_Delegate>(nameof(FMOD5_DSP_GetNumOutputs));

        private delegate RESULT FMOD5_DSP_GetInput_Delegate(IntPtr dsp, int index, out IntPtr input, out IntPtr inputconnection);
        private static FMOD5_DSP_GetInput_Delegate FMOD5_DSP_GetInput_Internal = null;
        private static FMOD5_DSP_GetInput_Delegate FMOD5_DSP_GetInput => FMOD5_DSP_GetInput_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_GetInput_Delegate>(nameof(FMOD5_DSP_GetInput));

        private delegate RESULT FMOD5_DSP_GetOutput_Delegate(IntPtr dsp, int index, out IntPtr output, out IntPtr outputconnection);
        private static FMOD5_DSP_GetOutput_Delegate FMOD5_DSP_GetOutput_Internal = null;
        private static FMOD5_DSP_GetOutput_Delegate FMOD5_DSP_GetOutput => FMOD5_DSP_GetOutput_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_GetOutput_Delegate>(nameof(FMOD5_DSP_GetOutput));

        private delegate RESULT FMOD5_DSP_SetActive_Delegate(IntPtr dsp, bool active);
        private static FMOD5_DSP_SetActive_Delegate FMOD5_DSP_SetActive_Internal = null;
        private static FMOD5_DSP_SetActive_Delegate FMOD5_DSP_SetActive => FMOD5_DSP_SetActive_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_SetActive_Delegate>(nameof(FMOD5_DSP_SetActive));

        private delegate RESULT FMOD5_DSP_GetActive_Delegate(IntPtr dsp, out bool active);
        private static FMOD5_DSP_GetActive_Delegate FMOD5_DSP_GetActive_Internal = null;
        private static FMOD5_DSP_GetActive_Delegate FMOD5_DSP_GetActive => FMOD5_DSP_GetActive_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_GetActive_Delegate>(nameof(FMOD5_DSP_GetActive));

        private delegate RESULT FMOD5_DSP_SetBypass_Delegate(IntPtr dsp, bool bypass);
        private static FMOD5_DSP_SetBypass_Delegate FMOD5_DSP_SetBypass_Internal = null;
        private static FMOD5_DSP_SetBypass_Delegate FMOD5_DSP_SetBypass => FMOD5_DSP_SetBypass_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_SetBypass_Delegate>(nameof(FMOD5_DSP_SetBypass));

        private delegate RESULT FMOD5_DSP_GetBypass_Delegate(IntPtr dsp, out bool bypass);
        private static FMOD5_DSP_GetBypass_Delegate FMOD5_DSP_GetBypass_Internal = null;
        private static FMOD5_DSP_GetBypass_Delegate FMOD5_DSP_GetBypass => FMOD5_DSP_GetBypass_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_GetBypass_Delegate>(nameof(FMOD5_DSP_GetBypass));

        private delegate RESULT FMOD5_DSP_SetWetDryMix_Delegate(IntPtr dsp, float prewet, float postwet, float dry);
        private static FMOD5_DSP_SetWetDryMix_Delegate FMOD5_DSP_SetWetDryMix_Internal = null;
        private static FMOD5_DSP_SetWetDryMix_Delegate FMOD5_DSP_SetWetDryMix => FMOD5_DSP_SetWetDryMix_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_SetWetDryMix_Delegate>(nameof(FMOD5_DSP_SetWetDryMix));

        private delegate RESULT FMOD5_DSP_GetWetDryMix_Delegate(IntPtr dsp, out float prewet, out float postwet, out float dry);
        private static FMOD5_DSP_GetWetDryMix_Delegate FMOD5_DSP_GetWetDryMix_Internal = null;
        private static FMOD5_DSP_GetWetDryMix_Delegate FMOD5_DSP_GetWetDryMix => FMOD5_DSP_GetWetDryMix_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_GetWetDryMix_Delegate>(nameof(FMOD5_DSP_GetWetDryMix));

        private delegate RESULT FMOD5_DSP_SetChannelFormat_Delegate(IntPtr dsp, CHANNELMASK channelmask, int numchannels, SPEAKERMODE source_speakermode);
        private static FMOD5_DSP_SetChannelFormat_Delegate FMOD5_DSP_SetChannelFormat_Internal = null;
        private static FMOD5_DSP_SetChannelFormat_Delegate FMOD5_DSP_SetChannelFormat => FMOD5_DSP_SetChannelFormat_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_SetChannelFormat_Delegate>(nameof(FMOD5_DSP_SetChannelFormat));

        private delegate RESULT FMOD5_DSP_GetChannelFormat_Delegate(IntPtr dsp, out CHANNELMASK channelmask, out int numchannels, out SPEAKERMODE source_speakermode);
        private static FMOD5_DSP_GetChannelFormat_Delegate FMOD5_DSP_GetChannelFormat_Internal = null;
        private static FMOD5_DSP_GetChannelFormat_Delegate FMOD5_DSP_GetChannelFormat => FMOD5_DSP_GetChannelFormat_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_GetChannelFormat_Delegate>(nameof(FMOD5_DSP_GetChannelFormat));

        private delegate RESULT FMOD5_DSP_GetOutputChannelFormat_Delegate(IntPtr dsp, CHANNELMASK inmask, int inchannels, SPEAKERMODE inspeakermode, out CHANNELMASK outmask, out int outchannels, out SPEAKERMODE outspeakermode);
        private static FMOD5_DSP_GetOutputChannelFormat_Delegate FMOD5_DSP_GetOutputChannelFormat_Internal = null;
        private static FMOD5_DSP_GetOutputChannelFormat_Delegate FMOD5_DSP_GetOutputChannelFormat => FMOD5_DSP_GetOutputChannelFormat_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_GetOutputChannelFormat_Delegate>(nameof(FMOD5_DSP_GetOutputChannelFormat));

        private delegate RESULT FMOD5_DSP_Reset_Delegate(IntPtr dsp);
        private static FMOD5_DSP_Reset_Delegate FMOD5_DSP_Reset_Internal = null;
        private static FMOD5_DSP_Reset_Delegate FMOD5_DSP_Reset => FMOD5_DSP_Reset_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_Reset_Delegate>(nameof(FMOD5_DSP_Reset));

        private delegate RESULT FMOD5_DSP_SetCallback_Delegate(IntPtr dsp, DSP_CALLBACK callback);
        private static FMOD5_DSP_SetCallback_Delegate FMOD5_DSP_SetCallback_Internal = null;
        private static FMOD5_DSP_SetCallback_Delegate FMOD5_DSP_SetCallback => FMOD5_DSP_SetCallback_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_SetCallback_Delegate>(nameof(FMOD5_DSP_SetCallback));

        private delegate RESULT FMOD5_DSP_SetParameterFloat_Delegate(IntPtr dsp, int index, float value);
        private static FMOD5_DSP_SetParameterFloat_Delegate FMOD5_DSP_SetParameterFloat_Internal = null;
        private static FMOD5_DSP_SetParameterFloat_Delegate FMOD5_DSP_SetParameterFloat => FMOD5_DSP_SetParameterFloat_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_SetParameterFloat_Delegate>(nameof(FMOD5_DSP_SetParameterFloat));

        private delegate RESULT FMOD5_DSP_SetParameterInt_Delegate(IntPtr dsp, int index, int value);
        private static FMOD5_DSP_SetParameterInt_Delegate FMOD5_DSP_SetParameterInt_Internal = null;
        private static FMOD5_DSP_SetParameterInt_Delegate FMOD5_DSP_SetParameterInt => FMOD5_DSP_SetParameterInt_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_SetParameterInt_Delegate>(nameof(FMOD5_DSP_SetParameterInt));

        private delegate RESULT FMOD5_DSP_SetParameterBool_Delegate(IntPtr dsp, int index, bool value);
        private static FMOD5_DSP_SetParameterBool_Delegate FMOD5_DSP_SetParameterBool_Internal = null;
        private static FMOD5_DSP_SetParameterBool_Delegate FMOD5_DSP_SetParameterBool => FMOD5_DSP_SetParameterBool_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_SetParameterBool_Delegate>(nameof(FMOD5_DSP_SetParameterBool));

        private delegate RESULT FMOD5_DSP_SetParameterData_Delegate(IntPtr dsp, int index, IntPtr data, uint length);
        private static FMOD5_DSP_SetParameterData_Delegate FMOD5_DSP_SetParameterData_Internal = null;
        private static FMOD5_DSP_SetParameterData_Delegate FMOD5_DSP_SetParameterData => FMOD5_DSP_SetParameterData_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_SetParameterData_Delegate>(nameof(FMOD5_DSP_SetParameterData));

        private delegate RESULT FMOD5_DSP_GetParameterFloat_Delegate(IntPtr dsp, int index, out float value, IntPtr valuestr, int valuestrlen);
        private static FMOD5_DSP_GetParameterFloat_Delegate FMOD5_DSP_GetParameterFloat_Internal = null;
        private static FMOD5_DSP_GetParameterFloat_Delegate FMOD5_DSP_GetParameterFloat => FMOD5_DSP_GetParameterFloat_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_GetParameterFloat_Delegate>(nameof(FMOD5_DSP_GetParameterFloat));

        private delegate RESULT FMOD5_DSP_GetParameterInt_Delegate(IntPtr dsp, int index, out int value, IntPtr valuestr, int valuestrlen);
        private static FMOD5_DSP_GetParameterInt_Delegate FMOD5_DSP_GetParameterInt_Internal = null;
        private static FMOD5_DSP_GetParameterInt_Delegate FMOD5_DSP_GetParameterInt => FMOD5_DSP_GetParameterInt_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_GetParameterInt_Delegate>(nameof(FMOD5_DSP_GetParameterInt));

        private delegate RESULT FMOD5_DSP_GetParameterBool_Delegate(IntPtr dsp, int index, out bool value, IntPtr valuestr, int valuestrlen);
        private static FMOD5_DSP_GetParameterBool_Delegate FMOD5_DSP_GetParameterBool_Internal = null;
        private static FMOD5_DSP_GetParameterBool_Delegate FMOD5_DSP_GetParameterBool => FMOD5_DSP_GetParameterBool_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_GetParameterBool_Delegate>(nameof(FMOD5_DSP_GetParameterBool));

        private delegate RESULT FMOD5_DSP_GetParameterData_Delegate(IntPtr dsp, int index, out IntPtr data, out uint length, IntPtr valuestr, int valuestrlen);
        private static FMOD5_DSP_GetParameterData_Delegate FMOD5_DSP_GetParameterData_Internal = null;
        private static FMOD5_DSP_GetParameterData_Delegate FMOD5_DSP_GetParameterData => FMOD5_DSP_GetParameterData_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_GetParameterData_Delegate>(nameof(FMOD5_DSP_GetParameterData));

        private delegate RESULT FMOD5_DSP_GetNumParameters_Delegate(IntPtr dsp, out int numparams);
        private static FMOD5_DSP_GetNumParameters_Delegate FMOD5_DSP_GetNumParameters_Internal = null;
        private static FMOD5_DSP_GetNumParameters_Delegate FMOD5_DSP_GetNumParameters => FMOD5_DSP_GetNumParameters_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_GetNumParameters_Delegate>(nameof(FMOD5_DSP_GetNumParameters));

        private delegate RESULT FMOD5_DSP_GetParameterInfo_Delegate(IntPtr dsp, int index, out IntPtr desc);
        private static FMOD5_DSP_GetParameterInfo_Delegate FMOD5_DSP_GetParameterInfo_Internal = null;
        private static FMOD5_DSP_GetParameterInfo_Delegate FMOD5_DSP_GetParameterInfo => FMOD5_DSP_GetParameterInfo_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_GetParameterInfo_Delegate>(nameof(FMOD5_DSP_GetParameterInfo));

        private delegate RESULT FMOD5_DSP_GetDataParameterIndex_Delegate(IntPtr dsp, int datatype, out int index);
        private static FMOD5_DSP_GetDataParameterIndex_Delegate FMOD5_DSP_GetDataParameterIndex_Internal = null;
        private static FMOD5_DSP_GetDataParameterIndex_Delegate FMOD5_DSP_GetDataParameterIndex => FMOD5_DSP_GetDataParameterIndex_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_GetDataParameterIndex_Delegate>(nameof(FMOD5_DSP_GetDataParameterIndex));

        private delegate RESULT FMOD5_DSP_ShowConfigDialog_Delegate(IntPtr dsp, IntPtr hwnd, bool show);
        private static FMOD5_DSP_ShowConfigDialog_Delegate FMOD5_DSP_ShowConfigDialog_Internal = null;
        private static FMOD5_DSP_ShowConfigDialog_Delegate FMOD5_DSP_ShowConfigDialog => FMOD5_DSP_ShowConfigDialog_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_ShowConfigDialog_Delegate>(nameof(FMOD5_DSP_ShowConfigDialog));

        private delegate RESULT FMOD5_DSP_GetInfo_Delegate(IntPtr dsp, IntPtr name, out uint version, out int channels, out int configwidth, out int configheight);
        private static FMOD5_DSP_GetInfo_Delegate FMOD5_DSP_GetInfo_Internal = null;
        private static FMOD5_DSP_GetInfo_Delegate FMOD5_DSP_GetInfo => FMOD5_DSP_GetInfo_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_GetInfo_Delegate>(nameof(FMOD5_DSP_GetInfo));

        private delegate RESULT FMOD5_DSP_GetType_Delegate(IntPtr dsp, out DSP_TYPE type);
        private static FMOD5_DSP_GetType_Delegate FMOD5_DSP_GetType_Internal = null;
        private static FMOD5_DSP_GetType_Delegate FMOD5_DSP_GetType => FMOD5_DSP_GetType_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_GetType_Delegate>(nameof(FMOD5_DSP_GetType));

        private delegate RESULT FMOD5_DSP_GetIdle_Delegate(IntPtr dsp, out bool idle);
        private static FMOD5_DSP_GetIdle_Delegate FMOD5_DSP_GetIdle_Internal = null;
        private static FMOD5_DSP_GetIdle_Delegate FMOD5_DSP_GetIdle => FMOD5_DSP_GetIdle_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_GetIdle_Delegate>(nameof(FMOD5_DSP_GetIdle));

        private delegate RESULT FMOD5_DSP_SetUserData_Delegate(IntPtr dsp, IntPtr userdata);
        private static FMOD5_DSP_SetUserData_Delegate FMOD5_DSP_SetUserData_Internal = null;
        private static FMOD5_DSP_SetUserData_Delegate FMOD5_DSP_SetUserData => FMOD5_DSP_SetUserData_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_SetUserData_Delegate>(nameof(FMOD5_DSP_SetUserData));

        private delegate RESULT FMOD5_DSP_GetUserData_Delegate(IntPtr dsp, out IntPtr userdata);
        private static FMOD5_DSP_GetUserData_Delegate FMOD5_DSP_GetUserData_Internal = null;
        private static FMOD5_DSP_GetUserData_Delegate FMOD5_DSP_GetUserData => FMOD5_DSP_GetUserData_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_GetUserData_Delegate>(nameof(FMOD5_DSP_GetUserData));

        public delegate RESULT FMOD5_DSP_SetMeteringEnabled_Delegate(IntPtr dsp, bool inputEnabled, bool outputEnabled);
        private static FMOD5_DSP_SetMeteringEnabled_Delegate FMOD5_DSP_SetMeteringEnabled_Internal = null;
        public static FMOD5_DSP_SetMeteringEnabled_Delegate FMOD5_DSP_SetMeteringEnabled => FMOD5_DSP_SetMeteringEnabled_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_SetMeteringEnabled_Delegate>(nameof(FMOD5_DSP_SetMeteringEnabled));

        public delegate RESULT FMOD5_DSP_GetMeteringEnabled_Delegate(IntPtr dsp, out bool inputEnabled, out bool outputEnabled);
        private static FMOD5_DSP_GetMeteringEnabled_Delegate FMOD5_DSP_GetMeteringEnabled_Internal = null;
        public static FMOD5_DSP_GetMeteringEnabled_Delegate FMOD5_DSP_GetMeteringEnabled => FMOD5_DSP_GetMeteringEnabled_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_GetMeteringEnabled_Delegate>(nameof(FMOD5_DSP_GetMeteringEnabled));

        public delegate RESULT FMOD5_DSP_GetMeteringInfo_Delegate(IntPtr dsp, IntPtr zero, out DSP_METERING_INFO outputInfo);
        private static FMOD5_DSP_GetMeteringInfo_Delegate FMOD5_DSP_GetMeteringInfo_Internal = null;
        public static FMOD5_DSP_GetMeteringInfo_Delegate FMOD5_DSP_GetMeteringInfo => FMOD5_DSP_GetMeteringInfo_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_GetMeteringInfo_Delegate>(nameof(FMOD5_DSP_GetMeteringInfo));

        public delegate RESULT FMOD5_DSP_GetMeteringInfo_Delegate2(IntPtr dsp, out DSP_METERING_INFO inputInfo, IntPtr zero);
        private static FMOD5_DSP_GetMeteringInfo_Delegate2 FMOD5_DSP_GetMeteringInfo_Internal2 = null;
        public static FMOD5_DSP_GetMeteringInfo_Delegate2 FMOD5_DSP_GetMeteringInfo2 => FMOD5_DSP_GetMeteringInfo_Internal2 ??= FModManager.GetProcInFModStudio<FMOD5_DSP_GetMeteringInfo_Delegate2>(nameof(FMOD5_DSP_GetMeteringInfo));

        public delegate RESULT FMOD5_DSP_GetMeteringInfo_Delegate3(IntPtr dsp, out DSP_METERING_INFO inputInfo, out DSP_METERING_INFO outputInfo);
        private static FMOD5_DSP_GetMeteringInfo_Delegate3 FMOD5_DSP_GetMeteringInfo_Internal3 = null;
        public static FMOD5_DSP_GetMeteringInfo_Delegate3 FMOD5_DSP_GetMeteringInfo3 => FMOD5_DSP_GetMeteringInfo_Internal3 ??= FModManager.GetProcInFModStudio<FMOD5_DSP_GetMeteringInfo_Delegate3>(nameof(FMOD5_DSP_GetMeteringInfo));

        public delegate RESULT FMOD5_DSP_GetCPUUsage_Delegate(IntPtr dsp, out uint exclusive, out uint inclusive);
        private static FMOD5_DSP_GetCPUUsage_Delegate FMOD5_DSP_GetCPUUsage_Internal = null;
        public static FMOD5_DSP_GetCPUUsage_Delegate FMOD5_DSP_GetCPUUsage => FMOD5_DSP_GetCPUUsage_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSP_GetCPUUsage_Delegate>(nameof(FMOD5_DSP_GetCPUUsage));

        #endregion

        #region wrapperinternal

        public IntPtr handle;

        public DSP(IntPtr ptr)      { this.handle = ptr; }
        public bool hasHandle()     { return this.handle != IntPtr.Zero; }
        public void clearHandle()   { this.handle = IntPtr.Zero; }

        #endregion
    }

    /*
        'DSPConnection' API
    */
    public struct DSPConnection
    {
        public RESULT getInput(out DSP input)
        {
            return FMOD5_DSPConnection_GetInput(this.handle, out input.handle);
        }
        public RESULT getOutput(out DSP output)
        {
            return FMOD5_DSPConnection_GetOutput(this.handle, out output.handle);
        }
        public RESULT setMix(float volume)
        {
            return FMOD5_DSPConnection_SetMix(this.handle, volume);
        }
        public RESULT getMix(out float volume)
        {
            return FMOD5_DSPConnection_GetMix(this.handle, out volume);
        }
        public RESULT setMixMatrix(float[] matrix, int outchannels, int inchannels, int inchannel_hop = 0)
        {
            return FMOD5_DSPConnection_SetMixMatrix(this.handle, matrix, outchannels, inchannels, inchannel_hop);
        }
        public RESULT getMixMatrix(float[] matrix, out int outchannels, out int inchannels, int inchannel_hop = 0)
        {
            return FMOD5_DSPConnection_GetMixMatrix(this.handle, matrix, out outchannels, out inchannels, inchannel_hop);
        }
        public RESULT getType(out DSPCONNECTION_TYPE type)
        {
            return FMOD5_DSPConnection_GetType(this.handle, out type);
        }

        // Userdata set/get.
        public RESULT setUserData(IntPtr userdata)
        {
            return FMOD5_DSPConnection_SetUserData(this.handle, userdata);
        }
        public RESULT getUserData(out IntPtr userdata)
        {
            return FMOD5_DSPConnection_GetUserData(this.handle, out userdata);
        }

        #region importfunctions
        private delegate RESULT FMOD5_DSPConnection_GetInput_Delegate(IntPtr dspconnection, out IntPtr input);
        private static FMOD5_DSPConnection_GetInput_Delegate FMOD5_DSPConnection_GetInput_Internal = null;
        private static FMOD5_DSPConnection_GetInput_Delegate FMOD5_DSPConnection_GetInput => FMOD5_DSPConnection_GetInput_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSPConnection_GetInput_Delegate>(nameof(FMOD5_DSPConnection_GetInput));

        private delegate RESULT FMOD5_DSPConnection_GetOutput_Delegate(IntPtr dspconnection, out IntPtr output);
        private static FMOD5_DSPConnection_GetOutput_Delegate FMOD5_DSPConnection_GetOutput_Internal = null;
        private static FMOD5_DSPConnection_GetOutput_Delegate FMOD5_DSPConnection_GetOutput => FMOD5_DSPConnection_GetOutput_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSPConnection_GetOutput_Delegate>(nameof(FMOD5_DSPConnection_GetOutput));

        private delegate RESULT FMOD5_DSPConnection_SetMix_Delegate(IntPtr dspconnection, float volume);
        private static FMOD5_DSPConnection_SetMix_Delegate FMOD5_DSPConnection_SetMix_Internal = null;
        private static FMOD5_DSPConnection_SetMix_Delegate FMOD5_DSPConnection_SetMix => FMOD5_DSPConnection_SetMix_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSPConnection_SetMix_Delegate>(nameof(FMOD5_DSPConnection_SetMix));

        private delegate RESULT FMOD5_DSPConnection_GetMix_Delegate(IntPtr dspconnection, out float volume);
        private static FMOD5_DSPConnection_GetMix_Delegate FMOD5_DSPConnection_GetMix_Internal = null;
        private static FMOD5_DSPConnection_GetMix_Delegate FMOD5_DSPConnection_GetMix => FMOD5_DSPConnection_GetMix_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSPConnection_GetMix_Delegate>(nameof(FMOD5_DSPConnection_GetMix));

        private delegate RESULT FMOD5_DSPConnection_SetMixMatrix_Delegate(IntPtr dspconnection, float[] matrix, int outchannels, int inchannels, int inchannel_hop);
        private static FMOD5_DSPConnection_SetMixMatrix_Delegate FMOD5_DSPConnection_SetMixMatrix_Internal = null;
        private static FMOD5_DSPConnection_SetMixMatrix_Delegate FMOD5_DSPConnection_SetMixMatrix => FMOD5_DSPConnection_SetMixMatrix_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSPConnection_SetMixMatrix_Delegate>(nameof(FMOD5_DSPConnection_SetMixMatrix));

        private delegate RESULT FMOD5_DSPConnection_GetMixMatrix_Delegate(IntPtr dspconnection, float[] matrix, out int outchannels, out int inchannels, int inchannel_hop);
        private static FMOD5_DSPConnection_GetMixMatrix_Delegate FMOD5_DSPConnection_GetMixMatrix_Internal = null;
        private static FMOD5_DSPConnection_GetMixMatrix_Delegate FMOD5_DSPConnection_GetMixMatrix => FMOD5_DSPConnection_GetMixMatrix_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSPConnection_GetMixMatrix_Delegate>(nameof(FMOD5_DSPConnection_GetMixMatrix));

        private delegate RESULT FMOD5_DSPConnection_GetType_Delegate(IntPtr dspconnection, out DSPCONNECTION_TYPE type);
        private static FMOD5_DSPConnection_GetType_Delegate FMOD5_DSPConnection_GetType_Internal = null;
        private static FMOD5_DSPConnection_GetType_Delegate FMOD5_DSPConnection_GetType => FMOD5_DSPConnection_GetType_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSPConnection_GetType_Delegate>(nameof(FMOD5_DSPConnection_GetType));

        private delegate RESULT FMOD5_DSPConnection_SetUserData_Delegate(IntPtr dspconnection, IntPtr userdata);
        private static FMOD5_DSPConnection_SetUserData_Delegate FMOD5_DSPConnection_SetUserData_Internal = null;
        private static FMOD5_DSPConnection_SetUserData_Delegate FMOD5_DSPConnection_SetUserData => FMOD5_DSPConnection_SetUserData_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSPConnection_SetUserData_Delegate>(nameof(FMOD5_DSPConnection_SetUserData));

        private delegate RESULT FMOD5_DSPConnection_GetUserData_Delegate(IntPtr dspconnection, out IntPtr userdata);
        private static FMOD5_DSPConnection_GetUserData_Delegate FMOD5_DSPConnection_GetUserData_Internal = null;
        private static FMOD5_DSPConnection_GetUserData_Delegate FMOD5_DSPConnection_GetUserData => FMOD5_DSPConnection_GetUserData_Internal ??= FModManager.GetProcInFModStudio<FMOD5_DSPConnection_GetUserData_Delegate>(nameof(FMOD5_DSPConnection_GetUserData));

        #endregion

        #region wrapperinternal

        public IntPtr handle;

        public DSPConnection(IntPtr ptr) { this.handle = ptr; }
        public bool hasHandle()          { return this.handle != IntPtr.Zero; }
        public void clearHandle()        { this.handle = IntPtr.Zero; }

        #endregion
    }

    /*
        'Geometry' API
    */
    public struct Geometry
    {
        public RESULT release()
        {
            return FMOD5_Geometry_Release(this.handle);
        }

        // Polygon manipulation.
        public RESULT addPolygon(float directocclusion, float reverbocclusion, bool doublesided, int numvertices, VECTOR[] vertices, out int polygonindex)
        {
            return FMOD5_Geometry_AddPolygon(this.handle, directocclusion, reverbocclusion, doublesided, numvertices, vertices, out polygonindex);
        }
        public RESULT getNumPolygons(out int numpolygons)
        {
            return FMOD5_Geometry_GetNumPolygons(this.handle, out numpolygons);
        }
        public RESULT getMaxPolygons(out int maxpolygons, out int maxvertices)
        {
            return FMOD5_Geometry_GetMaxPolygons(this.handle, out maxpolygons, out maxvertices);
        }
        public RESULT getPolygonNumVertices(int index, out int numvertices)
        {
            return FMOD5_Geometry_GetPolygonNumVertices(this.handle, index, out numvertices);
        }
        public RESULT setPolygonVertex(int index, int vertexindex, ref VECTOR vertex)
        {
            return FMOD5_Geometry_SetPolygonVertex(this.handle, index, vertexindex, ref vertex);
        }
        public RESULT getPolygonVertex(int index, int vertexindex, out VECTOR vertex)
        {
            return FMOD5_Geometry_GetPolygonVertex(this.handle, index, vertexindex, out vertex);
        }
        public RESULT setPolygonAttributes(int index, float directocclusion, float reverbocclusion, bool doublesided)
        {
            return FMOD5_Geometry_SetPolygonAttributes(this.handle, index, directocclusion, reverbocclusion, doublesided);
        }
        public RESULT getPolygonAttributes(int index, out float directocclusion, out float reverbocclusion, out bool doublesided)
        {
            return FMOD5_Geometry_GetPolygonAttributes(this.handle, index, out directocclusion, out reverbocclusion, out doublesided);
        }

        // Object manipulation.
        public RESULT setActive(bool active)
        {
            return FMOD5_Geometry_SetActive(this.handle, active);
        }
        public RESULT getActive(out bool active)
        {
            return FMOD5_Geometry_GetActive(this.handle, out active);
        }
        public RESULT setRotation(ref VECTOR forward, ref VECTOR up)
        {
            return FMOD5_Geometry_SetRotation(this.handle, ref forward, ref up);
        }
        public RESULT getRotation(out VECTOR forward, out VECTOR up)
        {
            return FMOD5_Geometry_GetRotation(this.handle, out forward, out up);
        }
        public RESULT setPosition(ref VECTOR position)
        {
            return FMOD5_Geometry_SetPosition(this.handle, ref position);
        }
        public RESULT getPosition(out VECTOR position)
        {
            return FMOD5_Geometry_GetPosition(this.handle, out position);
        }
        public RESULT setScale(ref VECTOR scale)
        {
            return FMOD5_Geometry_SetScale(this.handle, ref scale);
        }
        public RESULT getScale(out VECTOR scale)
        {
            return FMOD5_Geometry_GetScale(this.handle, out scale);
        }
        public RESULT save(IntPtr data, out int datasize)
        {
            return FMOD5_Geometry_Save(this.handle, data, out datasize);
        }

        // Userdata set/get.
        public RESULT setUserData(IntPtr userdata)
        {
            return FMOD5_Geometry_SetUserData(this.handle, userdata);
        }
        public RESULT getUserData(out IntPtr userdata)
        {
            return FMOD5_Geometry_GetUserData(this.handle, out userdata);
        }

        #region importfunctions
        private delegate RESULT FMOD5_Geometry_Release_Delegate(IntPtr geometry);
        private static FMOD5_Geometry_Release_Delegate FMOD5_Geometry_Release_Internal = null;
        private static FMOD5_Geometry_Release_Delegate FMOD5_Geometry_Release => FMOD5_Geometry_Release_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Geometry_Release_Delegate>(nameof(FMOD5_Geometry_Release));

        private delegate RESULT FMOD5_Geometry_AddPolygon_Delegate(IntPtr geometry, float directocclusion, float reverbocclusion, bool doublesided, int numvertices, VECTOR[] vertices, out int polygonindex);
        private static FMOD5_Geometry_AddPolygon_Delegate FMOD5_Geometry_AddPolygon_Internal = null;
        private static FMOD5_Geometry_AddPolygon_Delegate FMOD5_Geometry_AddPolygon => FMOD5_Geometry_AddPolygon_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Geometry_AddPolygon_Delegate>(nameof(FMOD5_Geometry_AddPolygon));

        private delegate RESULT FMOD5_Geometry_GetNumPolygons_Delegate(IntPtr geometry, out int numpolygons);
        private static FMOD5_Geometry_GetNumPolygons_Delegate FMOD5_Geometry_GetNumPolygons_Internal = null;
        private static FMOD5_Geometry_GetNumPolygons_Delegate FMOD5_Geometry_GetNumPolygons => FMOD5_Geometry_GetNumPolygons_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Geometry_GetNumPolygons_Delegate>(nameof(FMOD5_Geometry_GetNumPolygons));

        private delegate RESULT FMOD5_Geometry_GetMaxPolygons_Delegate(IntPtr geometry, out int maxpolygons, out int maxvertices);
        private static FMOD5_Geometry_GetMaxPolygons_Delegate FMOD5_Geometry_GetMaxPolygons_Internal = null;
        private static FMOD5_Geometry_GetMaxPolygons_Delegate FMOD5_Geometry_GetMaxPolygons => FMOD5_Geometry_GetMaxPolygons_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Geometry_GetMaxPolygons_Delegate>(nameof(FMOD5_Geometry_GetMaxPolygons));

        private delegate RESULT FMOD5_Geometry_GetPolygonNumVertices_Delegate(IntPtr geometry, int index, out int numvertices);
        private static FMOD5_Geometry_GetPolygonNumVertices_Delegate FMOD5_Geometry_GetPolygonNumVertices_Internal = null;
        private static FMOD5_Geometry_GetPolygonNumVertices_Delegate FMOD5_Geometry_GetPolygonNumVertices => FMOD5_Geometry_GetPolygonNumVertices_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Geometry_GetPolygonNumVertices_Delegate>(nameof(FMOD5_Geometry_GetPolygonNumVertices));

        private delegate RESULT FMOD5_Geometry_SetPolygonVertex_Delegate(IntPtr geometry, int index, int vertexindex, ref VECTOR vertex);
        private static FMOD5_Geometry_SetPolygonVertex_Delegate FMOD5_Geometry_SetPolygonVertex_Internal = null;
        private static FMOD5_Geometry_SetPolygonVertex_Delegate FMOD5_Geometry_SetPolygonVertex => FMOD5_Geometry_SetPolygonVertex_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Geometry_SetPolygonVertex_Delegate>(nameof(FMOD5_Geometry_SetPolygonVertex));

        private delegate RESULT FMOD5_Geometry_GetPolygonVertex_Delegate(IntPtr geometry, int index, int vertexindex, out VECTOR vertex);
        private static FMOD5_Geometry_GetPolygonVertex_Delegate FMOD5_Geometry_GetPolygonVertex_Internal = null;
        private static FMOD5_Geometry_GetPolygonVertex_Delegate FMOD5_Geometry_GetPolygonVertex => FMOD5_Geometry_GetPolygonVertex_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Geometry_GetPolygonVertex_Delegate>(nameof(FMOD5_Geometry_GetPolygonVertex));

        private delegate RESULT FMOD5_Geometry_SetPolygonAttributes_Delegate(IntPtr geometry, int index, float directocclusion, float reverbocclusion, bool doublesided);
        private static FMOD5_Geometry_SetPolygonAttributes_Delegate FMOD5_Geometry_SetPolygonAttributes_Internal = null;
        private static FMOD5_Geometry_SetPolygonAttributes_Delegate FMOD5_Geometry_SetPolygonAttributes => FMOD5_Geometry_SetPolygonAttributes_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Geometry_SetPolygonAttributes_Delegate>(nameof(FMOD5_Geometry_SetPolygonAttributes));

        private delegate RESULT FMOD5_Geometry_GetPolygonAttributes_Delegate(IntPtr geometry, int index, out float directocclusion, out float reverbocclusion, out bool doublesided);
        private static FMOD5_Geometry_GetPolygonAttributes_Delegate FMOD5_Geometry_GetPolygonAttributes_Internal = null;
        private static FMOD5_Geometry_GetPolygonAttributes_Delegate FMOD5_Geometry_GetPolygonAttributes => FMOD5_Geometry_GetPolygonAttributes_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Geometry_GetPolygonAttributes_Delegate>(nameof(FMOD5_Geometry_GetPolygonAttributes));

        private delegate RESULT FMOD5_Geometry_SetActive_Delegate(IntPtr geometry, bool active);
        private static FMOD5_Geometry_SetActive_Delegate FMOD5_Geometry_SetActive_Internal = null;
        private static FMOD5_Geometry_SetActive_Delegate FMOD5_Geometry_SetActive => FMOD5_Geometry_SetActive_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Geometry_SetActive_Delegate>(nameof(FMOD5_Geometry_SetActive));

        private delegate RESULT FMOD5_Geometry_GetActive_Delegate(IntPtr geometry, out bool active);
        private static FMOD5_Geometry_GetActive_Delegate FMOD5_Geometry_GetActive_Internal = null;
        private static FMOD5_Geometry_GetActive_Delegate FMOD5_Geometry_GetActive => FMOD5_Geometry_GetActive_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Geometry_GetActive_Delegate>(nameof(FMOD5_Geometry_GetActive));

        private delegate RESULT FMOD5_Geometry_SetRotation_Delegate(IntPtr geometry, ref VECTOR forward, ref VECTOR up);
        private static FMOD5_Geometry_SetRotation_Delegate FMOD5_Geometry_SetRotation_Internal = null;
        private static FMOD5_Geometry_SetRotation_Delegate FMOD5_Geometry_SetRotation => FMOD5_Geometry_SetRotation_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Geometry_SetRotation_Delegate>(nameof(FMOD5_Geometry_SetRotation));

        private delegate RESULT FMOD5_Geometry_GetRotation_Delegate(IntPtr geometry, out VECTOR forward, out VECTOR up);
        private static FMOD5_Geometry_GetRotation_Delegate FMOD5_Geometry_GetRotation_Internal = null;
        private static FMOD5_Geometry_GetRotation_Delegate FMOD5_Geometry_GetRotation => FMOD5_Geometry_GetRotation_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Geometry_GetRotation_Delegate>(nameof(FMOD5_Geometry_GetRotation));

        private delegate RESULT FMOD5_Geometry_SetPosition_Delegate(IntPtr geometry, ref VECTOR position);
        private static FMOD5_Geometry_SetPosition_Delegate FMOD5_Geometry_SetPosition_Internal = null;
        private static FMOD5_Geometry_SetPosition_Delegate FMOD5_Geometry_SetPosition => FMOD5_Geometry_SetPosition_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Geometry_SetPosition_Delegate>(nameof(FMOD5_Geometry_SetPosition));

        private delegate RESULT FMOD5_Geometry_GetPosition_Delegate(IntPtr geometry, out VECTOR position);
        private static FMOD5_Geometry_GetPosition_Delegate FMOD5_Geometry_GetPosition_Internal = null;
        private static FMOD5_Geometry_GetPosition_Delegate FMOD5_Geometry_GetPosition => FMOD5_Geometry_GetPosition_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Geometry_GetPosition_Delegate>(nameof(FMOD5_Geometry_GetPosition));

        private delegate RESULT FMOD5_Geometry_SetScale_Delegate(IntPtr geometry, ref VECTOR scale);
        private static FMOD5_Geometry_SetScale_Delegate FMOD5_Geometry_SetScale_Internal = null;
        private static FMOD5_Geometry_SetScale_Delegate FMOD5_Geometry_SetScale => FMOD5_Geometry_SetScale_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Geometry_SetScale_Delegate>(nameof(FMOD5_Geometry_SetScale));

        private delegate RESULT FMOD5_Geometry_GetScale_Delegate(IntPtr geometry, out VECTOR scale);
        private static FMOD5_Geometry_GetScale_Delegate FMOD5_Geometry_GetScale_Internal = null;
        private static FMOD5_Geometry_GetScale_Delegate FMOD5_Geometry_GetScale => FMOD5_Geometry_GetScale_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Geometry_GetScale_Delegate>(nameof(FMOD5_Geometry_GetScale));

        private delegate RESULT FMOD5_Geometry_Save_Delegate(IntPtr geometry, IntPtr data, out int datasize);
        private static FMOD5_Geometry_Save_Delegate FMOD5_Geometry_Save_Internal = null;
        private static FMOD5_Geometry_Save_Delegate FMOD5_Geometry_Save => FMOD5_Geometry_Save_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Geometry_Save_Delegate>(nameof(FMOD5_Geometry_Save));

        private delegate RESULT FMOD5_Geometry_SetUserData_Delegate(IntPtr geometry, IntPtr userdata);
        private static FMOD5_Geometry_SetUserData_Delegate FMOD5_Geometry_SetUserData_Internal = null;
        private static FMOD5_Geometry_SetUserData_Delegate FMOD5_Geometry_SetUserData => FMOD5_Geometry_SetUserData_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Geometry_SetUserData_Delegate>(nameof(FMOD5_Geometry_SetUserData));

        private delegate RESULT FMOD5_Geometry_GetUserData_Delegate(IntPtr geometry, out IntPtr userdata);
        private static FMOD5_Geometry_GetUserData_Delegate FMOD5_Geometry_GetUserData_Internal = null;
        private static FMOD5_Geometry_GetUserData_Delegate FMOD5_Geometry_GetUserData => FMOD5_Geometry_GetUserData_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Geometry_GetUserData_Delegate>(nameof(FMOD5_Geometry_GetUserData));

        #endregion

        #region wrapperinternal

        public IntPtr handle;

        public Geometry(IntPtr ptr) { this.handle = ptr; }
        public bool hasHandle()     { return this.handle != IntPtr.Zero; }
        public void clearHandle()   { this.handle = IntPtr.Zero; }

        #endregion
    }

    /*
        'Reverb3D' API
    */
    public struct Reverb3D
    {
        public RESULT release()
        {
            return FMOD5_Reverb3D_Release(this.handle);
        }

        // Reverb manipulation.
        public RESULT set3DAttributes(ref VECTOR position, float mindistance, float maxdistance)
        {
            return FMOD5_Reverb3D_Set3DAttributes(this.handle, ref position, mindistance, maxdistance);
        }
        public RESULT get3DAttributes(ref VECTOR position, ref float mindistance, ref float maxdistance)
        {
            return FMOD5_Reverb3D_Get3DAttributes(this.handle, ref position, ref mindistance, ref maxdistance);
        }
        public RESULT setProperties(ref REVERB_PROPERTIES properties)
        {
            return FMOD5_Reverb3D_SetProperties(this.handle, ref properties);
        }
        public RESULT getProperties(ref REVERB_PROPERTIES properties)
        {
            return FMOD5_Reverb3D_GetProperties(this.handle, ref properties);
        }
        public RESULT setActive(bool active)
        {
            return FMOD5_Reverb3D_SetActive(this.handle, active);
        }
        public RESULT getActive(out bool active)
        {
            return FMOD5_Reverb3D_GetActive(this.handle, out active);
        }

        // Userdata set/get.
        public RESULT setUserData(IntPtr userdata)
        {
            return FMOD5_Reverb3D_SetUserData(this.handle, userdata);
        }
        public RESULT getUserData(out IntPtr userdata)
        {
            return FMOD5_Reverb3D_GetUserData(this.handle, out userdata);
        }

        #region importfunctions
        private delegate RESULT FMOD5_Reverb3D_Release_Delegate(IntPtr reverb3d);
        private static FMOD5_Reverb3D_Release_Delegate FMOD5_Reverb3D_Release_Internal = null;
        private static FMOD5_Reverb3D_Release_Delegate FMOD5_Reverb3D_Release => FMOD5_Reverb3D_Release_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Reverb3D_Release_Delegate>(nameof(FMOD5_Reverb3D_Release));

        private delegate RESULT FMOD5_Reverb3D_Set3DAttributes_Delegate(IntPtr reverb3d, ref VECTOR position, float mindistance, float maxdistance);
        private static FMOD5_Reverb3D_Set3DAttributes_Delegate FMOD5_Reverb3D_Set3DAttributes_Internal = null;
        private static FMOD5_Reverb3D_Set3DAttributes_Delegate FMOD5_Reverb3D_Set3DAttributes => FMOD5_Reverb3D_Set3DAttributes_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Reverb3D_Set3DAttributes_Delegate>(nameof(FMOD5_Reverb3D_Set3DAttributes));

        private delegate RESULT FMOD5_Reverb3D_Get3DAttributes_Delegate(IntPtr reverb3d, ref VECTOR position, ref float mindistance, ref float maxdistance);
        private static FMOD5_Reverb3D_Get3DAttributes_Delegate FMOD5_Reverb3D_Get3DAttributes_Internal = null;
        private static FMOD5_Reverb3D_Get3DAttributes_Delegate FMOD5_Reverb3D_Get3DAttributes => FMOD5_Reverb3D_Get3DAttributes_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Reverb3D_Get3DAttributes_Delegate>(nameof(FMOD5_Reverb3D_Get3DAttributes));

        private delegate RESULT FMOD5_Reverb3D_SetProperties_Delegate(IntPtr reverb3d, ref REVERB_PROPERTIES properties);
        private static FMOD5_Reverb3D_SetProperties_Delegate FMOD5_Reverb3D_SetProperties_Internal = null;
        private static FMOD5_Reverb3D_SetProperties_Delegate FMOD5_Reverb3D_SetProperties => FMOD5_Reverb3D_SetProperties_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Reverb3D_SetProperties_Delegate>(nameof(FMOD5_Reverb3D_SetProperties));

        private delegate RESULT FMOD5_Reverb3D_GetProperties_Delegate(IntPtr reverb3d, ref REVERB_PROPERTIES properties);
        private static FMOD5_Reverb3D_GetProperties_Delegate FMOD5_Reverb3D_GetProperties_Internal = null;
        private static FMOD5_Reverb3D_GetProperties_Delegate FMOD5_Reverb3D_GetProperties => FMOD5_Reverb3D_GetProperties_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Reverb3D_GetProperties_Delegate>(nameof(FMOD5_Reverb3D_GetProperties));

        private delegate RESULT FMOD5_Reverb3D_SetActive_Delegate(IntPtr reverb3d, bool active);
        private static FMOD5_Reverb3D_SetActive_Delegate FMOD5_Reverb3D_SetActive_Internal = null;
        private static FMOD5_Reverb3D_SetActive_Delegate FMOD5_Reverb3D_SetActive => FMOD5_Reverb3D_SetActive_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Reverb3D_SetActive_Delegate>(nameof(FMOD5_Reverb3D_SetActive));

        private delegate RESULT FMOD5_Reverb3D_GetActive_Delegate(IntPtr reverb3d, out bool active);
        private static FMOD5_Reverb3D_GetActive_Delegate FMOD5_Reverb3D_GetActive_Internal = null;
        private static FMOD5_Reverb3D_GetActive_Delegate FMOD5_Reverb3D_GetActive => FMOD5_Reverb3D_GetActive_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Reverb3D_GetActive_Delegate>(nameof(FMOD5_Reverb3D_GetActive));

        private delegate RESULT FMOD5_Reverb3D_SetUserData_Delegate(IntPtr reverb3d, IntPtr userdata);
        private static FMOD5_Reverb3D_SetUserData_Delegate FMOD5_Reverb3D_SetUserData_Internal = null;
        private static FMOD5_Reverb3D_SetUserData_Delegate FMOD5_Reverb3D_SetUserData => FMOD5_Reverb3D_SetUserData_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Reverb3D_SetUserData_Delegate>(nameof(FMOD5_Reverb3D_SetUserData));

        private delegate RESULT FMOD5_Reverb3D_GetUserData_Delegate(IntPtr reverb3d, out IntPtr userdata);
        private static FMOD5_Reverb3D_GetUserData_Delegate FMOD5_Reverb3D_GetUserData_Internal = null;
        private static FMOD5_Reverb3D_GetUserData_Delegate FMOD5_Reverb3D_GetUserData => FMOD5_Reverb3D_GetUserData_Internal ??= FModManager.GetProcInFModStudio<FMOD5_Reverb3D_GetUserData_Delegate>(nameof(FMOD5_Reverb3D_GetUserData));

        #endregion

        #region wrapperinternal

        public IntPtr handle;

        public Reverb3D(IntPtr ptr) { this.handle = ptr; }
        public bool hasHandle()     { return this.handle != IntPtr.Zero; }
        public void clearHandle()   { this.handle = IntPtr.Zero; }

        #endregion
    }

    #region Helper Functions
    [StructLayout(LayoutKind.Sequential)]
    public struct StringWrapper
    {
        IntPtr nativeUtf8Ptr;

        public StringWrapper(IntPtr ptr)
        {
            nativeUtf8Ptr = ptr;
        }

        public static implicit operator string(StringWrapper fstring)
        {
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                return encoder.stringFromNative(fstring.nativeUtf8Ptr);
            }
        }

        public bool StartsWith(byte[] prefix)
        {
            if (nativeUtf8Ptr == IntPtr.Zero)
            {
                return false;
            }

            for (int i = 0; i < prefix.Length; i++)
            {
                if (Marshal.ReadByte(nativeUtf8Ptr, i) != prefix[i])
                {
                    return false;
                }
            }

            return true;
        }

        public bool Equals(byte[] comparison)
        {
            if (nativeUtf8Ptr == IntPtr.Zero)
            {
                return false;
            }

            for (int i = 0; i < comparison.Length; i++)
            {
                if (Marshal.ReadByte(nativeUtf8Ptr, i) != comparison[i])
                {
                    return false;
                }
            }

            if (Marshal.ReadByte(nativeUtf8Ptr, comparison.Length) != 0)
            {
                return false;
            }

            return true;
        }
    }

    static class StringHelper
    {
        public class ThreadSafeEncoding : IDisposable
        {
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] encodedBuffer = new byte[128];
            char[] decodedBuffer = new char[128];
            bool inUse;
            GCHandle gcHandle;

            public bool InUse()    { return inUse; }
            public void SetInUse() { inUse = true; }

            private int roundUpPowerTwo(int number)
            {
                int newNumber = 1;
                while (newNumber <= number)
                {
                    newNumber *= 2;
                }

                return newNumber;
            }

            public byte[] byteFromStringUTF8(string s)
            {
                if (s == null)
                {
                    return null;
                }

                int maximumLength = encoding.GetMaxByteCount(s.Length) + 1; // +1 for null terminator
                if (maximumLength > encodedBuffer.Length)
                {
                    int encodedLength = encoding.GetByteCount(s) + 1; // +1 for null terminator
                    if (encodedLength > encodedBuffer.Length)
                    {
                        encodedBuffer = new byte[roundUpPowerTwo(encodedLength)];
                    }
                }

                int byteCount = encoding.GetBytes(s, 0, s.Length, encodedBuffer, 0);
                encodedBuffer[byteCount] = 0; // Apply null terminator

                return encodedBuffer;
            }

            public IntPtr intptrFromStringUTF8(string s)
            {
                if (s == null)
                {
                    return IntPtr.Zero;
                }

                gcHandle = GCHandle.Alloc(byteFromStringUTF8(s), GCHandleType.Pinned);
                return gcHandle.AddrOfPinnedObject();
            }

            public string stringFromNative(IntPtr nativePtr)
            {
                if (nativePtr == IntPtr.Zero)
                {
                    return "";
                }

                int nativeLen = 0;
                while (Marshal.ReadByte(nativePtr, nativeLen) != 0)
                {
                    nativeLen++;
                }

                if (nativeLen == 0)
                {
                    return "";
                }

                if (nativeLen > encodedBuffer.Length)
                {
                    encodedBuffer = new byte[roundUpPowerTwo(nativeLen)];
                }

                Marshal.Copy(nativePtr, encodedBuffer, 0, nativeLen);

                int maximumLength = encoding.GetMaxCharCount(nativeLen);
                if (maximumLength > decodedBuffer.Length)
                {
                    int decodedLength = encoding.GetCharCount(encodedBuffer, 0, nativeLen);
                    if (decodedLength > decodedBuffer.Length)
                    {
                        decodedBuffer = new char[roundUpPowerTwo(decodedLength)];
                    }
                }

                int charCount = encoding.GetChars(encodedBuffer, 0, nativeLen, decodedBuffer, 0);

                return new String(decodedBuffer, 0, charCount);
            }

            public void Dispose()
            {
                if (gcHandle.IsAllocated)
                {
                    gcHandle.Free();
                }
                lock (encoders)
                {
                    inUse = false;
                }
            }
        }

        static List<ThreadSafeEncoding> encoders = new List<ThreadSafeEncoding>(1);

        public static ThreadSafeEncoding GetFreeHelper()
        {
            lock (encoders)
            {
                ThreadSafeEncoding helper = null;
                // Search for not in use helper
                for (int i = 0; i < encoders.Count; i++)
                {
                    if (!encoders[i].InUse())
                    {
                        helper = encoders[i];
                        break;
                    }
                }
                // Otherwise create another helper
                if (helper == null)
                {
                    helper = new ThreadSafeEncoding();
                    encoders.Add(helper);
                }
                helper.SetInUse();
                return helper;
            }
        }
    }

    // Some of the Marshal functions were marked as deprecated / obsolete, however that decision was reversed: https://github.com/dotnet/corefx/pull/10541
    // Use the old syntax (non-generic) to ensure maximum compatibility (especially with Unity) ignoring the warnings
    public static class MarshalHelper
    {
#pragma warning disable 618
        public static int SizeOf(Type t)
        {
            return Marshal.SizeOf(t); // Always use Type version, never Object version as it boxes causes GC allocations
        }

        public static object PtrToStructure(IntPtr ptr, Type structureType)
        {
            return Marshal.PtrToStructure(ptr, structureType);
        }
#pragma warning restore 618
    }

    #endregion
}
