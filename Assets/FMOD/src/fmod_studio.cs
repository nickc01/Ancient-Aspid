/* ======================================================================================== */
/* FMOD Studio API - C# wrapper.                                                            */
/* Copyright (c), Firelight Technologies Pty, Ltd. 2004-2023.                               */
/*                                                                                          */
/* For more detail visit:                                                                   */
/* https://fmod.com/docs/2.02/api/studio-api.html                                           */
/* ======================================================================================== */

using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;

namespace FMOD.Studio
{
    public partial class STUDIO_VERSION
    {
#if !UNITY_2019_4_OR_NEWER
        public const string dll     = "fmodstudio";
#endif
    }

    public enum STOP_MODE : int
    {
        ALLOWFADEOUT,
        IMMEDIATE,
    }

    public enum LOADING_STATE : int
    {
        UNLOADING,
        UNLOADED,
        LOADING,
        LOADED,
        ERROR,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PROGRAMMER_SOUND_PROPERTIES
    {
        public StringWrapper name;
        public IntPtr sound;
        public int subsoundIndex;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TIMELINE_MARKER_PROPERTIES
    {
        public StringWrapper name;
        public int position;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TIMELINE_BEAT_PROPERTIES
    {
        public int bar;
        public int beat;
        public int position;
        public float tempo;
        public int timesignatureupper;
        public int timesignaturelower;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TIMELINE_NESTED_BEAT_PROPERTIES
    {
        public GUID eventid;
        public TIMELINE_BEAT_PROPERTIES properties;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ADVANCEDSETTINGS
    {
        public int cbsize;
        public int commandqueuesize;
        public int handleinitialsize;
        public int studioupdateperiod;
        public int idlesampledatapoolsize;
        public int streamingscheduledelay;
        public IntPtr encryptionkey;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CPU_USAGE
    {
        public float update;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BUFFER_INFO
    {
        public int currentusage;
        public int peakusage;
        public int capacity;
        public int stallcount;
        public float stalltime;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BUFFER_USAGE
    {
        public BUFFER_INFO studiocommandqueue;
        public BUFFER_INFO studiohandle;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BANK_INFO
    {
        public int size;
        public IntPtr userdata;
        public int userdatalength;
        public FILE_OPEN_CALLBACK opencallback;
        public FILE_CLOSE_CALLBACK closecallback;
        public FILE_READ_CALLBACK readcallback;
        public FILE_SEEK_CALLBACK seekcallback;
    }

    [Flags]
    public enum SYSTEM_CALLBACK_TYPE : uint
    {
        PREUPDATE = 0x00000001,
        POSTUPDATE = 0x00000002,
        BANK_UNLOAD = 0x00000004,
        LIVEUPDATE_CONNECTED = 0x00000008,
        LIVEUPDATE_DISCONNECTED = 0x00000010,
        ALL = 0xFFFFFFFF,
    }

    public delegate RESULT SYSTEM_CALLBACK(IntPtr system, SYSTEM_CALLBACK_TYPE type, IntPtr commanddata, IntPtr userdata);

    public enum PARAMETER_TYPE : int
    {
        GAME_CONTROLLED,
        AUTOMATIC_DISTANCE,
        AUTOMATIC_EVENT_CONE_ANGLE,
        AUTOMATIC_EVENT_ORIENTATION,
        AUTOMATIC_DIRECTION,
        AUTOMATIC_ELEVATION,
        AUTOMATIC_LISTENER_ORIENTATION,
        AUTOMATIC_SPEED,
        AUTOMATIC_SPEED_ABSOLUTE,
        AUTOMATIC_DISTANCE_NORMALIZED,
        MAX
    }

    [Flags]
    public enum PARAMETER_FLAGS : uint
    {
        READONLY      = 0x00000001,
        AUTOMATIC     = 0x00000002,
        GLOBAL        = 0x00000004,
        DISCRETE      = 0x00000008,
        LABELED       = 0x00000010,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PARAMETER_ID
    {
        public uint data1;
        public uint data2;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PARAMETER_DESCRIPTION
    {
        public StringWrapper name;
        public PARAMETER_ID id;
        public float minimum;
        public float maximum;
        public float defaultvalue;
        public PARAMETER_TYPE type;
        public PARAMETER_FLAGS flags;
        public GUID guid;
    }

    // This is only need for loading memory and given our C# wrapper LOAD_MEMORY_POINT isn't feasible anyway
    public enum LOAD_MEMORY_MODE : int
    {
        LOAD_MEMORY,
        LOAD_MEMORY_POINT,
    }

    enum LOAD_MEMORY_ALIGNMENT : int
    {
        VALUE = 32
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SOUND_INFO
    {
        public IntPtr name_or_data;
        public MODE mode;
        public CREATESOUNDEXINFO exinfo;
        public int subsoundindex;

        public string name
        {
            get
            {
                using (StringHelper.ThreadSafeEncoding encoding = StringHelper.GetFreeHelper())
                {
                    return ((mode & (MODE.OPENMEMORY | MODE.OPENMEMORY_POINT)) == 0) ? encoding.stringFromNative(name_or_data) : String.Empty;
                }
            }
        }
    }

    public enum USER_PROPERTY_TYPE : int
    {
        INTEGER,
        BOOLEAN,
        FLOAT,
        STRING,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct USER_PROPERTY
    {
        public StringWrapper name;
        public USER_PROPERTY_TYPE type;
        private Union_IntBoolFloatString value;

        public int intValue()       {   return (type == USER_PROPERTY_TYPE.INTEGER) ? value.intvalue : -1;      }
        public bool boolValue()     {   return (type == USER_PROPERTY_TYPE.BOOLEAN) ? value.boolvalue : false;  }
        public float floatValue()   {   return (type == USER_PROPERTY_TYPE.FLOAT)   ? value.floatvalue : -1;    }
        public string stringValue() {   return (type == USER_PROPERTY_TYPE.STRING)  ? value.stringvalue : "";   }
    };

    [StructLayout(LayoutKind.Explicit)]
    struct Union_IntBoolFloatString
    {
        [FieldOffset(0)]
        public int intvalue;
        [FieldOffset(0)]
        public bool boolvalue;
        [FieldOffset(0)]
        public float floatvalue;
        [FieldOffset(0)]
        public StringWrapper stringvalue;
    }

    [Flags]
    public enum INITFLAGS : uint
    {
        NORMAL                  = 0x00000000,
        LIVEUPDATE              = 0x00000001,
        ALLOW_MISSING_PLUGINS   = 0x00000002,
        SYNCHRONOUS_UPDATE      = 0x00000004,
        DEFERRED_CALLBACKS      = 0x00000008,
        LOAD_FROM_UPDATE        = 0x00000010,
        MEMORY_TRACKING         = 0x00000020,
    }

    [Flags]
    public enum LOAD_BANK_FLAGS : uint
    {
        NORMAL                  = 0x00000000,
        NONBLOCKING             = 0x00000001,
        DECOMPRESS_SAMPLES      = 0x00000002,
        UNENCRYPTED             = 0x00000004,
    }

    [Flags]
    public enum COMMANDCAPTURE_FLAGS : uint
    {
        NORMAL                  = 0x00000000,
        FILEFLUSH               = 0x00000001,
        SKIP_INITIAL_STATE      = 0x00000002,
    }

    [Flags]
    public enum COMMANDREPLAY_FLAGS : uint
    {
        NORMAL                  = 0x00000000,
        SKIP_CLEANUP            = 0x00000001,
        FAST_FORWARD            = 0x00000002,
        SKIP_BANK_LOAD          = 0x00000004,
    }

    public enum PLAYBACK_STATE : int
    {
        PLAYING,
        SUSTAINING,
        STOPPED,
        STARTING,
        STOPPING,
    }

    public enum EVENT_PROPERTY : int
    {
        CHANNELPRIORITY,
        SCHEDULE_DELAY,
        SCHEDULE_LOOKAHEAD,
        MINIMUM_DISTANCE,
        MAXIMUM_DISTANCE,
        COOLDOWN,
        MAX
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct PLUGIN_INSTANCE_PROPERTIES
    {
        public IntPtr name;
        public IntPtr dsp;
    }

    [Flags]
    public enum EVENT_CALLBACK_TYPE : uint
    {
        CREATED                  = 0x00000001,
        DESTROYED                = 0x00000002,
        STARTING                 = 0x00000004,
        STARTED                  = 0x00000008,
        RESTARTED                = 0x00000010,
        STOPPED                  = 0x00000020,
        START_FAILED             = 0x00000040,
        CREATE_PROGRAMMER_SOUND  = 0x00000080,
        DESTROY_PROGRAMMER_SOUND = 0x00000100,
        PLUGIN_CREATED           = 0x00000200,
        PLUGIN_DESTROYED         = 0x00000400,
        TIMELINE_MARKER          = 0x00000800,
        TIMELINE_BEAT            = 0x00001000,
        SOUND_PLAYED             = 0x00002000,
        SOUND_STOPPED            = 0x00004000,
        REAL_TO_VIRTUAL          = 0x00008000,
        VIRTUAL_TO_REAL          = 0x00010000,
        START_EVENT_COMMAND      = 0x00020000,
        NESTED_TIMELINE_BEAT     = 0x00040000,

        ALL                      = 0xFFFFFFFF,
    }

    public delegate RESULT EVENT_CALLBACK(EVENT_CALLBACK_TYPE type, IntPtr _event, IntPtr parameters);

    public delegate RESULT COMMANDREPLAY_FRAME_CALLBACK(IntPtr replay, int commandindex, float currenttime, IntPtr userdata);
    public delegate RESULT COMMANDREPLAY_LOAD_BANK_CALLBACK(IntPtr replay, int commandindex, GUID bankguid, IntPtr bankfilename, LOAD_BANK_FLAGS flags, out IntPtr bank, IntPtr userdata);
    public delegate RESULT COMMANDREPLAY_CREATE_INSTANCE_CALLBACK(IntPtr replay, int commandindex, IntPtr eventdescription, out IntPtr instance, IntPtr userdata);

    public enum INSTANCETYPE : int
    {
        NONE,
        SYSTEM,
        EVENTDESCRIPTION,
        EVENTINSTANCE,
        PARAMETERINSTANCE,
        BUS,
        VCA,
        BANK,
        COMMANDREPLAY,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct COMMAND_INFO
    {
        public StringWrapper commandname;
        public int parentcommandindex;
        public int framenumber;
        public float frametime;
        public INSTANCETYPE instancetype;
        public INSTANCETYPE outputtype;
        public UInt32 instancehandle;
        public UInt32 outputhandle;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MEMORY_USAGE
    {
        public int exclusive;
        public int inclusive;
        public int sampledata;
    }

    public struct Util
    {
        public static RESULT parseID(string idString, out GUID id)
        {
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                return FMOD_Studio_ParseID(encoder.byteFromStringUTF8(idString), out id);
            }
        }

        #region importfunctions
        private delegate RESULT FMOD_Studio_ParseID_Delegate(byte[] idString, out GUID id);
        private static FMOD_Studio_ParseID_Delegate FMOD_Studio_ParseID_Internal = null;
        private static FMOD_Studio_ParseID_Delegate FMOD_Studio_ParseID => FMOD_Studio_ParseID_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_ParseID_Delegate>(nameof(FMOD_Studio_ParseID));

        #endregion
    }

    public struct System
    {
        // Initialization / system functions.
        public static RESULT create(out System system)
        {
            return FMOD_Studio_System_Create(out system.handle, VERSION.number);
        }
        public RESULT setAdvancedSettings(ADVANCEDSETTINGS settings)
        {
            settings.cbsize = MarshalHelper.SizeOf(typeof(ADVANCEDSETTINGS));
            return FMOD_Studio_System_SetAdvancedSettings(this.handle, ref settings);
        }
        public RESULT setAdvancedSettings(ADVANCEDSETTINGS settings, string encryptionKey)
        {
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                IntPtr userKey = settings.encryptionkey;
                settings.encryptionkey = encoder.intptrFromStringUTF8(encryptionKey);
                FMOD.RESULT result = setAdvancedSettings(settings);
                settings.encryptionkey = userKey;
                return result;
            }
        }
        public RESULT getAdvancedSettings(out ADVANCEDSETTINGS settings)
        {
            settings.cbsize = MarshalHelper.SizeOf(typeof(ADVANCEDSETTINGS));
            return FMOD_Studio_System_GetAdvancedSettings(this.handle, out settings);
        }
        public RESULT initialize(int maxchannels, INITFLAGS studioflags, FMOD.INITFLAGS flags, IntPtr extradriverdata)
        {
            return FMOD_Studio_System_Initialize(this.handle, maxchannels, studioflags, flags, extradriverdata);
        }
        public RESULT release()
        {
            return FMOD_Studio_System_Release(this.handle);
        }
        public RESULT update()
        {
            return FMOD_Studio_System_Update(this.handle);
        }
        public RESULT getCoreSystem(out FMOD.System coresystem)
        {
            return FMOD_Studio_System_GetCoreSystem(this.handle, out coresystem.handle);
        }
        public RESULT getEvent(string path, out EventDescription _event)
        {
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                return FMOD_Studio_System_GetEvent(this.handle, encoder.byteFromStringUTF8(path), out _event.handle);
            }
        }
        public RESULT getBus(string path, out Bus bus)
        {
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                return FMOD_Studio_System_GetBus(this.handle, encoder.byteFromStringUTF8(path), out bus.handle);
            }
        }
        public RESULT getVCA(string path, out VCA vca)
        {
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                return FMOD_Studio_System_GetVCA(this.handle, encoder.byteFromStringUTF8(path), out vca.handle);
            }
        }
        public RESULT getBank(string path, out Bank bank)
        {
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                return FMOD_Studio_System_GetBank(this.handle, encoder.byteFromStringUTF8(path), out bank.handle);
            }
        }

        public RESULT getEventByID(GUID id, out EventDescription _event)
        {
            return FMOD_Studio_System_GetEventByID(this.handle, ref id, out _event.handle);
        }
        public RESULT getBusByID(GUID id, out Bus bus)
        {
            return FMOD_Studio_System_GetBusByID(this.handle, ref id, out bus.handle);
        }
        public RESULT getVCAByID(GUID id, out VCA vca)
        {
            return FMOD_Studio_System_GetVCAByID(this.handle, ref id, out vca.handle);
        }
        public RESULT getBankByID(GUID id, out Bank bank)
        {
            return FMOD_Studio_System_GetBankByID(this.handle, ref id, out bank.handle);
        }
        public RESULT getSoundInfo(string key, out SOUND_INFO info)
        {
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                return FMOD_Studio_System_GetSoundInfo(this.handle, encoder.byteFromStringUTF8(key), out info);
            }
        }
        public RESULT getParameterDescriptionByName(string name, out PARAMETER_DESCRIPTION parameter)
        {
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                return FMOD_Studio_System_GetParameterDescriptionByName(this.handle, encoder.byteFromStringUTF8(name), out parameter);
            }
        }
        public RESULT getParameterDescriptionByID(PARAMETER_ID id, out PARAMETER_DESCRIPTION parameter)
        {
            return FMOD_Studio_System_GetParameterDescriptionByID(this.handle, id, out parameter);
        }
        public RESULT getParameterLabelByName(string name, int labelindex, out string label)
        {
            label = null;

            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                IntPtr stringMem = Marshal.AllocHGlobal(256);
                int retrieved = 0;
                byte[] nameBytes = encoder.byteFromStringUTF8(name);
                RESULT result = FMOD_Studio_System_GetParameterLabelByName(this.handle, nameBytes, labelindex, stringMem, 256, out retrieved);

                if (result == RESULT.ERR_TRUNCATED)
                {
                    Marshal.FreeHGlobal(stringMem);
                    result = FMOD_Studio_System_GetParameterLabelByName(this.handle, nameBytes, labelindex, IntPtr.Zero, 0, out retrieved);
                    stringMem = Marshal.AllocHGlobal(retrieved);
                    result = FMOD_Studio_System_GetParameterLabelByName(this.handle, nameBytes, labelindex, stringMem, retrieved, out retrieved);
                }

                if (result == RESULT.OK)
                {
                    label = encoder.stringFromNative(stringMem);
                }
                Marshal.FreeHGlobal(stringMem);
                return result;
            }
        }
        public RESULT getParameterLabelByID(PARAMETER_ID id, int labelindex, out string label)
        {
            label = null;

            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                IntPtr stringMem = Marshal.AllocHGlobal(256);
                int retrieved = 0;
                RESULT result = FMOD_Studio_System_GetParameterLabelByID(this.handle, id, labelindex, stringMem, 256, out retrieved);

                if (result == RESULT.ERR_TRUNCATED)
                {
                    Marshal.FreeHGlobal(stringMem);
                    result = FMOD_Studio_System_GetParameterLabelByID(this.handle, id, labelindex, IntPtr.Zero, 0, out retrieved);
                    stringMem = Marshal.AllocHGlobal(retrieved);
                    result = FMOD_Studio_System_GetParameterLabelByID(this.handle, id, labelindex, stringMem, retrieved, out retrieved);
                }

                if (result == RESULT.OK)
                {
                    label = encoder.stringFromNative(stringMem);
                }
                Marshal.FreeHGlobal(stringMem);
                return result;
            }
        }
        public RESULT getParameterByID(PARAMETER_ID id, out float value)
        {
            float finalValue;
            return getParameterByID(id, out value, out finalValue);
        }
        public RESULT getParameterByID(PARAMETER_ID id, out float value, out float finalvalue)
        {
            return FMOD_Studio_System_GetParameterByID(this.handle, id, out value, out finalvalue);
        }
        public RESULT setParameterByID(PARAMETER_ID id, float value, bool ignoreseekspeed = false)
        {
            return FMOD_Studio_System_SetParameterByID(this.handle, id, value, ignoreseekspeed);
        }
        public RESULT setParameterByIDWithLabel(PARAMETER_ID id, string label, bool ignoreseekspeed = false)
        {
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                return FMOD_Studio_System_SetParameterByIDWithLabel(this.handle, id, encoder.byteFromStringUTF8(label), ignoreseekspeed);
            }
        }
        public RESULT setParametersByIDs(PARAMETER_ID[] ids, float[] values, int count, bool ignoreseekspeed = false)
        {
            return FMOD_Studio_System_SetParametersByIDs(this.handle, ids, values, count, ignoreseekspeed);
        }
        public RESULT getParameterByName(string name, out float value)
        {
            float finalValue;
            return getParameterByName(name, out value, out finalValue);
        }
        public RESULT getParameterByName(string name, out float value, out float finalvalue)
        {
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                return FMOD_Studio_System_GetParameterByName(this.handle, encoder.byteFromStringUTF8(name), out value, out finalvalue);
            }
        }
        public RESULT setParameterByName(string name, float value, bool ignoreseekspeed = false)
        {
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                return FMOD_Studio_System_SetParameterByName(this.handle, encoder.byteFromStringUTF8(name), value, ignoreseekspeed);
            }
        }
        public RESULT setParameterByNameWithLabel(string name, string label, bool ignoreseekspeed = false)
        {
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper(),
                                                   labelEncoder = StringHelper.GetFreeHelper())
            {
                return FMOD_Studio_System_SetParameterByNameWithLabel(this.handle, encoder.byteFromStringUTF8(name), labelEncoder.byteFromStringUTF8(label), ignoreseekspeed);
            }
        }
        public RESULT lookupID(string path, out GUID id)
        {
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                return FMOD_Studio_System_LookupID(this.handle, encoder.byteFromStringUTF8(path), out id);
            }
        }
        public RESULT lookupPath(GUID id, out string path)
        {
            path = null;

            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                IntPtr stringMem = Marshal.AllocHGlobal(256);
                int retrieved = 0;
                RESULT result = FMOD_Studio_System_LookupPath(this.handle, ref id, stringMem, 256, out retrieved);

                if (result == RESULT.ERR_TRUNCATED)
                {
                    Marshal.FreeHGlobal(stringMem);
                    stringMem = Marshal.AllocHGlobal(retrieved);
                    result = FMOD_Studio_System_LookupPath(this.handle, ref id, stringMem, retrieved, out retrieved);
                }

                if (result == RESULT.OK)
                {
                    path = encoder.stringFromNative(stringMem);
                }
                Marshal.FreeHGlobal(stringMem);
                return result;
            }
        }
        public RESULT getNumListeners(out int numlisteners)
        {
            return FMOD_Studio_System_GetNumListeners(this.handle, out numlisteners);
        }
        public RESULT setNumListeners(int numlisteners)
        {
            return FMOD_Studio_System_SetNumListeners(this.handle, numlisteners);
        }
        public RESULT getListenerAttributes(int listener, out ATTRIBUTES_3D attributes)
        {
            return FMOD_Studio_System_GetListenerAttributes(this.handle, listener, out attributes, IntPtr.Zero);
        }
        public RESULT getListenerAttributes(int listener, out ATTRIBUTES_3D attributes, out VECTOR attenuationposition)
        {
            return FMOD_Studio_System_GetListenerAttributes2(this.handle, listener, out attributes, out attenuationposition);
        }
        public RESULT setListenerAttributes(int listener, ATTRIBUTES_3D attributes)
        {
            return FMOD_Studio_System_SetListenerAttributes(this.handle, listener, ref attributes, IntPtr.Zero);
        }
        public RESULT setListenerAttributes(int listener, ATTRIBUTES_3D attributes, VECTOR attenuationposition)
        {
            return FMOD_Studio_System_SetListenerAttributes2(this.handle, listener, ref attributes, ref attenuationposition);
        }
        public RESULT getListenerWeight(int listener, out float weight)
        {
            return FMOD_Studio_System_GetListenerWeight(this.handle, listener, out weight);
        }
        public RESULT setListenerWeight(int listener, float weight)
        {
            return FMOD_Studio_System_SetListenerWeight(this.handle, listener, weight);
        }
        public RESULT loadBankFile(string filename, LOAD_BANK_FLAGS flags, out Bank bank)
        {
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                return FMOD_Studio_System_LoadBankFile(this.handle, encoder.byteFromStringUTF8(filename), flags, out bank.handle);
            }
        }

        /*public unsafe RESULT loadBankMemory(byte* buffer, LOAD_BANK_FLAGS flags, out Bank bank)
        {
            // Manually pin the byte array. It's what the marshaller should do anyway but don't leave it to chance.
            GCHandle pinnedArray = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr pointer = pinnedArray.AddrOfPinnedObject();
            RESULT result = FMOD_Studio_System_LoadBankMemory(this.handle, pointer, buffer.Length, LOAD_MEMORY_MODE.LOAD_MEMORY, flags, out bank.handle);
            pinnedArray.Free();
            return result;
        }*/

        public RESULT loadBankMemory(byte[] buffer, LOAD_BANK_FLAGS flags, out Bank bank)
        {
            // Manually pin the byte array. It's what the marshaller should do anyway but don't leave it to chance.
            GCHandle pinnedArray = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr pointer = pinnedArray.AddrOfPinnedObject();
            RESULT result = FMOD_Studio_System_LoadBankMemory(this.handle, pointer, buffer.Length, LOAD_MEMORY_MODE.LOAD_MEMORY, flags, out bank.handle);
            pinnedArray.Free();
            return result;
        }
        public RESULT loadBankCustom(BANK_INFO info, LOAD_BANK_FLAGS flags, out Bank bank)
        {
            info.size = MarshalHelper.SizeOf(typeof(BANK_INFO));
            return FMOD_Studio_System_LoadBankCustom(this.handle, ref info, flags, out bank.handle);
        }
        public RESULT unloadAll()
        {
            return FMOD_Studio_System_UnloadAll(this.handle);
        }
        public RESULT flushCommands()
        {
            return FMOD_Studio_System_FlushCommands(this.handle);
        }
        public RESULT flushSampleLoading()
        {
            return FMOD_Studio_System_FlushSampleLoading(this.handle);
        }
        public RESULT startCommandCapture(string filename, COMMANDCAPTURE_FLAGS flags)
        {
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                return FMOD_Studio_System_StartCommandCapture(this.handle, encoder.byteFromStringUTF8(filename), flags);
            }
        }
        public RESULT stopCommandCapture()
        {
            return FMOD_Studio_System_StopCommandCapture(this.handle);
        }
        public RESULT loadCommandReplay(string filename, COMMANDREPLAY_FLAGS flags, out CommandReplay replay)
        {
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                return FMOD_Studio_System_LoadCommandReplay(this.handle, encoder.byteFromStringUTF8(filename), flags, out replay.handle);
            }
        }
        public RESULT getBankCount(out int count)
        {
            return FMOD_Studio_System_GetBankCount(this.handle, out count);
        }
        public RESULT getBankList(out Bank[] array)
        {
            array = null;

            RESULT result;
            int capacity;
            result = FMOD_Studio_System_GetBankCount(this.handle, out capacity);
            if (result != RESULT.OK)
            {
                return result;
            }
            if (capacity == 0)
            {
                array = new Bank[0];
                return result;
            }

            IntPtr[] rawArray = new IntPtr[capacity];
            int actualCount;
            result = FMOD_Studio_System_GetBankList(this.handle, rawArray, capacity, out actualCount);
            if (result != RESULT.OK)
            {
                return result;
            }
            if (actualCount > capacity) // More items added since we queried just now?
            {
                actualCount = capacity;
            }
            array = new Bank[actualCount];
            for (int i = 0; i < actualCount; ++i)
            {
                array[i].handle = rawArray[i];
            }
            return RESULT.OK;
        }
        public RESULT getParameterDescriptionCount(out int count)
        {
            return FMOD_Studio_System_GetParameterDescriptionCount(this.handle, out count);
        }
        public RESULT getParameterDescriptionList(out PARAMETER_DESCRIPTION[] array)
        {
            array = null;

            int capacity;
            RESULT result = FMOD_Studio_System_GetParameterDescriptionCount(this.handle, out capacity);
            if (result != RESULT.OK)
            {
                return result;
            }
            if (capacity == 0)
            {
                array = new PARAMETER_DESCRIPTION[0];
                return RESULT.OK;
            }

            PARAMETER_DESCRIPTION[] tempArray = new PARAMETER_DESCRIPTION[capacity];
            int actualCount;
            result = FMOD_Studio_System_GetParameterDescriptionList(this.handle, tempArray, capacity, out actualCount);
            if (result != RESULT.OK)
            {
                return result;
            }

            if (actualCount != capacity)
            {
                Array.Resize(ref tempArray, actualCount);
            }

            array = tempArray;

            return RESULT.OK;
        }
        public RESULT getCPUUsage(out CPU_USAGE usage, out FMOD.CPU_USAGE usage_core)
        {
            return FMOD_Studio_System_GetCPUUsage(this.handle, out usage, out usage_core);
        }
        public RESULT getBufferUsage(out BUFFER_USAGE usage)
        {
            return FMOD_Studio_System_GetBufferUsage(this.handle, out usage);
        }
        public RESULT resetBufferUsage()
        {
            return FMOD_Studio_System_ResetBufferUsage(this.handle);
        }

        public RESULT setCallback(SYSTEM_CALLBACK callback, SYSTEM_CALLBACK_TYPE callbackmask = SYSTEM_CALLBACK_TYPE.ALL)
        {
            return FMOD_Studio_System_SetCallback(this.handle, callback, callbackmask);
        }

        public RESULT getUserData(out IntPtr userdata)
        {
            return FMOD_Studio_System_GetUserData(this.handle, out userdata);
        }

        public RESULT setUserData(IntPtr userdata)
        {
            return FMOD_Studio_System_SetUserData(this.handle, userdata);
        }

        public RESULT getMemoryUsage(out MEMORY_USAGE memoryusage)
        {
            return FMOD_Studio_System_GetMemoryUsage(this.handle, out memoryusage);
        }

        #region importfunctions
        private delegate RESULT FMOD_Studio_System_Create_Delegate(out IntPtr system, uint headerversion);
        private static FMOD_Studio_System_Create_Delegate FMOD_Studio_System_Create_Internal = null;
        private static FMOD_Studio_System_Create_Delegate FMOD_Studio_System_Create => FMOD_Studio_System_Create_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_Create_Delegate>(nameof(FMOD_Studio_System_Create));

        private delegate bool FMOD_Studio_System_IsValid_Delegate(IntPtr system);
        private static FMOD_Studio_System_IsValid_Delegate FMOD_Studio_System_IsValid_Internal = null;
        private static FMOD_Studio_System_IsValid_Delegate FMOD_Studio_System_IsValid => FMOD_Studio_System_IsValid_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_IsValid_Delegate>(nameof(FMOD_Studio_System_IsValid));

        private delegate RESULT FMOD_Studio_System_SetAdvancedSettings_Delegate(IntPtr system, ref ADVANCEDSETTINGS settings);
        private static FMOD_Studio_System_SetAdvancedSettings_Delegate FMOD_Studio_System_SetAdvancedSettings_Internal = null;
        private static FMOD_Studio_System_SetAdvancedSettings_Delegate FMOD_Studio_System_SetAdvancedSettings => FMOD_Studio_System_SetAdvancedSettings_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_SetAdvancedSettings_Delegate>(nameof(FMOD_Studio_System_SetAdvancedSettings));

        private delegate RESULT FMOD_Studio_System_GetAdvancedSettings_Delegate(IntPtr system, out ADVANCEDSETTINGS settings);
        private static FMOD_Studio_System_GetAdvancedSettings_Delegate FMOD_Studio_System_GetAdvancedSettings_Internal = null;
        private static FMOD_Studio_System_GetAdvancedSettings_Delegate FMOD_Studio_System_GetAdvancedSettings => FMOD_Studio_System_GetAdvancedSettings_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_GetAdvancedSettings_Delegate>(nameof(FMOD_Studio_System_GetAdvancedSettings));

        private delegate RESULT FMOD_Studio_System_Initialize_Delegate(IntPtr system, int maxchannels, INITFLAGS studioflags, FMOD.INITFLAGS flags, IntPtr extradriverdata);
        private static FMOD_Studio_System_Initialize_Delegate FMOD_Studio_System_Initialize_Internal = null;
        private static FMOD_Studio_System_Initialize_Delegate FMOD_Studio_System_Initialize => FMOD_Studio_System_Initialize_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_Initialize_Delegate>(nameof(FMOD_Studio_System_Initialize));

        private delegate RESULT FMOD_Studio_System_Release_Delegate(IntPtr system);
        private static FMOD_Studio_System_Release_Delegate FMOD_Studio_System_Release_Internal = null;
        private static FMOD_Studio_System_Release_Delegate FMOD_Studio_System_Release => FMOD_Studio_System_Release_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_Release_Delegate>(nameof(FMOD_Studio_System_Release));

        private delegate RESULT FMOD_Studio_System_Update_Delegate(IntPtr system);
        private static FMOD_Studio_System_Update_Delegate FMOD_Studio_System_Update_Internal = null;
        private static FMOD_Studio_System_Update_Delegate FMOD_Studio_System_Update => FMOD_Studio_System_Update_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_Update_Delegate>(nameof(FMOD_Studio_System_Update));

        private delegate RESULT FMOD_Studio_System_GetCoreSystem_Delegate(IntPtr system, out IntPtr coresystem);
        private static FMOD_Studio_System_GetCoreSystem_Delegate FMOD_Studio_System_GetCoreSystem_Internal = null;
        private static FMOD_Studio_System_GetCoreSystem_Delegate FMOD_Studio_System_GetCoreSystem => FMOD_Studio_System_GetCoreSystem_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_GetCoreSystem_Delegate>(nameof(FMOD_Studio_System_GetCoreSystem));

        private delegate RESULT FMOD_Studio_System_GetEvent_Delegate(IntPtr system, byte[] path, out IntPtr _event);
        private static FMOD_Studio_System_GetEvent_Delegate FMOD_Studio_System_GetEvent_Internal = null;
        private static FMOD_Studio_System_GetEvent_Delegate FMOD_Studio_System_GetEvent => FMOD_Studio_System_GetEvent_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_GetEvent_Delegate>(nameof(FMOD_Studio_System_GetEvent));

        private delegate RESULT FMOD_Studio_System_GetBus_Delegate(IntPtr system, byte[] path, out IntPtr bus);
        private static FMOD_Studio_System_GetBus_Delegate FMOD_Studio_System_GetBus_Internal = null;
        private static FMOD_Studio_System_GetBus_Delegate FMOD_Studio_System_GetBus => FMOD_Studio_System_GetBus_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_GetBus_Delegate>(nameof(FMOD_Studio_System_GetBus));

        private delegate RESULT FMOD_Studio_System_GetVCA_Delegate(IntPtr system, byte[] path, out IntPtr vca);
        private static FMOD_Studio_System_GetVCA_Delegate FMOD_Studio_System_GetVCA_Internal = null;
        private static FMOD_Studio_System_GetVCA_Delegate FMOD_Studio_System_GetVCA => FMOD_Studio_System_GetVCA_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_GetVCA_Delegate>(nameof(FMOD_Studio_System_GetVCA));

        private delegate RESULT FMOD_Studio_System_GetBank_Delegate(IntPtr system, byte[] path, out IntPtr bank);
        private static FMOD_Studio_System_GetBank_Delegate FMOD_Studio_System_GetBank_Internal = null;
        private static FMOD_Studio_System_GetBank_Delegate FMOD_Studio_System_GetBank => FMOD_Studio_System_GetBank_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_GetBank_Delegate>(nameof(FMOD_Studio_System_GetBank));

        private delegate RESULT FMOD_Studio_System_GetEventByID_Delegate(IntPtr system, ref GUID id, out IntPtr _event);
        private static FMOD_Studio_System_GetEventByID_Delegate FMOD_Studio_System_GetEventByID_Internal = null;
        private static FMOD_Studio_System_GetEventByID_Delegate FMOD_Studio_System_GetEventByID => FMOD_Studio_System_GetEventByID_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_GetEventByID_Delegate>(nameof(FMOD_Studio_System_GetEventByID));

        private delegate RESULT FMOD_Studio_System_GetBusByID_Delegate(IntPtr system, ref GUID id, out IntPtr bus);
        private static FMOD_Studio_System_GetBusByID_Delegate FMOD_Studio_System_GetBusByID_Internal = null;
        private static FMOD_Studio_System_GetBusByID_Delegate FMOD_Studio_System_GetBusByID => FMOD_Studio_System_GetBusByID_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_GetBusByID_Delegate>(nameof(FMOD_Studio_System_GetBusByID));

        private delegate RESULT FMOD_Studio_System_GetVCAByID_Delegate(IntPtr system, ref GUID id, out IntPtr vca);
        private static FMOD_Studio_System_GetVCAByID_Delegate FMOD_Studio_System_GetVCAByID_Internal = null;
        private static FMOD_Studio_System_GetVCAByID_Delegate FMOD_Studio_System_GetVCAByID => FMOD_Studio_System_GetVCAByID_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_GetVCAByID_Delegate>(nameof(FMOD_Studio_System_GetVCAByID));

        private delegate RESULT FMOD_Studio_System_GetBankByID_Delegate(IntPtr system, ref GUID id, out IntPtr bank);
        private static FMOD_Studio_System_GetBankByID_Delegate FMOD_Studio_System_GetBankByID_Internal = null;
        private static FMOD_Studio_System_GetBankByID_Delegate FMOD_Studio_System_GetBankByID => FMOD_Studio_System_GetBankByID_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_GetBankByID_Delegate>(nameof(FMOD_Studio_System_GetBankByID));

        private delegate RESULT FMOD_Studio_System_GetSoundInfo_Delegate(IntPtr system, byte[] key, out SOUND_INFO info);
        private static FMOD_Studio_System_GetSoundInfo_Delegate FMOD_Studio_System_GetSoundInfo_Internal = null;
        private static FMOD_Studio_System_GetSoundInfo_Delegate FMOD_Studio_System_GetSoundInfo => FMOD_Studio_System_GetSoundInfo_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_GetSoundInfo_Delegate>(nameof(FMOD_Studio_System_GetSoundInfo));

        private delegate RESULT FMOD_Studio_System_GetParameterDescriptionByName_Delegate(IntPtr system, byte[] name, out PARAMETER_DESCRIPTION parameter);
        private static FMOD_Studio_System_GetParameterDescriptionByName_Delegate FMOD_Studio_System_GetParameterDescriptionByName_Internal = null;
        private static FMOD_Studio_System_GetParameterDescriptionByName_Delegate FMOD_Studio_System_GetParameterDescriptionByName => FMOD_Studio_System_GetParameterDescriptionByName_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_GetParameterDescriptionByName_Delegate>(nameof(FMOD_Studio_System_GetParameterDescriptionByName));

        private delegate RESULT FMOD_Studio_System_GetParameterDescriptionByID_Delegate(IntPtr system, PARAMETER_ID id, out PARAMETER_DESCRIPTION parameter);
        private static FMOD_Studio_System_GetParameterDescriptionByID_Delegate FMOD_Studio_System_GetParameterDescriptionByID_Internal = null;
        private static FMOD_Studio_System_GetParameterDescriptionByID_Delegate FMOD_Studio_System_GetParameterDescriptionByID => FMOD_Studio_System_GetParameterDescriptionByID_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_GetParameterDescriptionByID_Delegate>(nameof(FMOD_Studio_System_GetParameterDescriptionByID));

        private delegate RESULT FMOD_Studio_System_GetParameterLabelByName_Delegate(IntPtr system, byte[] name, int labelindex, IntPtr label, int size, out int retrieved);
        private static FMOD_Studio_System_GetParameterLabelByName_Delegate FMOD_Studio_System_GetParameterLabelByName_Internal = null;
        private static FMOD_Studio_System_GetParameterLabelByName_Delegate FMOD_Studio_System_GetParameterLabelByName => FMOD_Studio_System_GetParameterLabelByName_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_GetParameterLabelByName_Delegate>(nameof(FMOD_Studio_System_GetParameterLabelByName));

        private delegate RESULT FMOD_Studio_System_GetParameterLabelByID_Delegate(IntPtr system, PARAMETER_ID id, int labelindex, IntPtr label, int size, out int retrieved);
        private static FMOD_Studio_System_GetParameterLabelByID_Delegate FMOD_Studio_System_GetParameterLabelByID_Internal = null;
        private static FMOD_Studio_System_GetParameterLabelByID_Delegate FMOD_Studio_System_GetParameterLabelByID => FMOD_Studio_System_GetParameterLabelByID_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_GetParameterLabelByID_Delegate>(nameof(FMOD_Studio_System_GetParameterLabelByID));

        private delegate RESULT FMOD_Studio_System_GetParameterByID_Delegate(IntPtr system, PARAMETER_ID id, out float value, out float finalvalue);
        private static FMOD_Studio_System_GetParameterByID_Delegate FMOD_Studio_System_GetParameterByID_Internal = null;
        private static FMOD_Studio_System_GetParameterByID_Delegate FMOD_Studio_System_GetParameterByID => FMOD_Studio_System_GetParameterByID_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_GetParameterByID_Delegate>(nameof(FMOD_Studio_System_GetParameterByID));

        private delegate RESULT FMOD_Studio_System_SetParameterByID_Delegate(IntPtr system, PARAMETER_ID id, float value, bool ignoreseekspeed);
        private static FMOD_Studio_System_SetParameterByID_Delegate FMOD_Studio_System_SetParameterByID_Internal = null;
        private static FMOD_Studio_System_SetParameterByID_Delegate FMOD_Studio_System_SetParameterByID => FMOD_Studio_System_SetParameterByID_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_SetParameterByID_Delegate>(nameof(FMOD_Studio_System_SetParameterByID));

        private delegate RESULT FMOD_Studio_System_SetParameterByIDWithLabel_Delegate(IntPtr system, PARAMETER_ID id, byte[] label, bool ignoreseekspeed);
        private static FMOD_Studio_System_SetParameterByIDWithLabel_Delegate FMOD_Studio_System_SetParameterByIDWithLabel_Internal = null;
        private static FMOD_Studio_System_SetParameterByIDWithLabel_Delegate FMOD_Studio_System_SetParameterByIDWithLabel => FMOD_Studio_System_SetParameterByIDWithLabel_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_SetParameterByIDWithLabel_Delegate>(nameof(FMOD_Studio_System_SetParameterByIDWithLabel));

        private delegate RESULT FMOD_Studio_System_SetParametersByIDs_Delegate(IntPtr system, PARAMETER_ID[] ids, float[] values, int count, bool ignoreseekspeed);
        private static FMOD_Studio_System_SetParametersByIDs_Delegate FMOD_Studio_System_SetParametersByIDs_Internal = null;
        private static FMOD_Studio_System_SetParametersByIDs_Delegate FMOD_Studio_System_SetParametersByIDs => FMOD_Studio_System_SetParametersByIDs_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_SetParametersByIDs_Delegate>(nameof(FMOD_Studio_System_SetParametersByIDs));

        private delegate RESULT FMOD_Studio_System_GetParameterByName_Delegate(IntPtr system, byte[] name, out float value, out float finalvalue);
        private static FMOD_Studio_System_GetParameterByName_Delegate FMOD_Studio_System_GetParameterByName_Internal = null;
        private static FMOD_Studio_System_GetParameterByName_Delegate FMOD_Studio_System_GetParameterByName => FMOD_Studio_System_GetParameterByName_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_GetParameterByName_Delegate>(nameof(FMOD_Studio_System_GetParameterByName));

        private delegate RESULT FMOD_Studio_System_SetParameterByName_Delegate(IntPtr system, byte[] name, float value, bool ignoreseekspeed);
        private static FMOD_Studio_System_SetParameterByName_Delegate FMOD_Studio_System_SetParameterByName_Internal = null;
        private static FMOD_Studio_System_SetParameterByName_Delegate FMOD_Studio_System_SetParameterByName => FMOD_Studio_System_SetParameterByName_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_SetParameterByName_Delegate>(nameof(FMOD_Studio_System_SetParameterByName));

        private delegate RESULT FMOD_Studio_System_SetParameterByNameWithLabel_Delegate(IntPtr system, byte[] name, byte[] label, bool ignoreseekspeed);
        private static FMOD_Studio_System_SetParameterByNameWithLabel_Delegate FMOD_Studio_System_SetParameterByNameWithLabel_Internal = null;
        private static FMOD_Studio_System_SetParameterByNameWithLabel_Delegate FMOD_Studio_System_SetParameterByNameWithLabel => FMOD_Studio_System_SetParameterByNameWithLabel_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_SetParameterByNameWithLabel_Delegate>(nameof(FMOD_Studio_System_SetParameterByNameWithLabel));

        private delegate RESULT FMOD_Studio_System_LookupID_Delegate(IntPtr system, byte[] path, out GUID id);
        private static FMOD_Studio_System_LookupID_Delegate FMOD_Studio_System_LookupID_Internal = null;
        private static FMOD_Studio_System_LookupID_Delegate FMOD_Studio_System_LookupID => FMOD_Studio_System_LookupID_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_LookupID_Delegate>(nameof(FMOD_Studio_System_LookupID));

        private delegate RESULT FMOD_Studio_System_LookupPath_Delegate(IntPtr system, ref GUID id, IntPtr path, int size, out int retrieved);
        private static FMOD_Studio_System_LookupPath_Delegate FMOD_Studio_System_LookupPath_Internal = null;
        private static FMOD_Studio_System_LookupPath_Delegate FMOD_Studio_System_LookupPath => FMOD_Studio_System_LookupPath_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_LookupPath_Delegate>(nameof(FMOD_Studio_System_LookupPath));

        private delegate RESULT FMOD_Studio_System_GetNumListeners_Delegate(IntPtr system, out int numlisteners);
        private static FMOD_Studio_System_GetNumListeners_Delegate FMOD_Studio_System_GetNumListeners_Internal = null;
        private static FMOD_Studio_System_GetNumListeners_Delegate FMOD_Studio_System_GetNumListeners => FMOD_Studio_System_GetNumListeners_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_GetNumListeners_Delegate>(nameof(FMOD_Studio_System_GetNumListeners));

        private delegate RESULT FMOD_Studio_System_SetNumListeners_Delegate(IntPtr system, int numlisteners);
        private static FMOD_Studio_System_SetNumListeners_Delegate FMOD_Studio_System_SetNumListeners_Internal = null;
        private static FMOD_Studio_System_SetNumListeners_Delegate FMOD_Studio_System_SetNumListeners => FMOD_Studio_System_SetNumListeners_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_SetNumListeners_Delegate>(nameof(FMOD_Studio_System_SetNumListeners));

        private delegate RESULT FMOD_Studio_System_GetListenerAttributes_Delegate(IntPtr system, int listener, out ATTRIBUTES_3D attributes, IntPtr zero);
        private static FMOD_Studio_System_GetListenerAttributes_Delegate FMOD_Studio_System_GetListenerAttributes_Internal = null;
        private static FMOD_Studio_System_GetListenerAttributes_Delegate FMOD_Studio_System_GetListenerAttributes => FMOD_Studio_System_GetListenerAttributes_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_GetListenerAttributes_Delegate>(nameof(FMOD_Studio_System_GetListenerAttributes));

        private delegate RESULT FMOD_Studio_System_GetListenerAttributes_Delegate2(IntPtr system, int listener, out ATTRIBUTES_3D attributes, out VECTOR attenuationposition);
        private static FMOD_Studio_System_GetListenerAttributes_Delegate2 FMOD_Studio_System_GetListenerAttributes_Internal2 = null;
        private static FMOD_Studio_System_GetListenerAttributes_Delegate2 FMOD_Studio_System_GetListenerAttributes2 => FMOD_Studio_System_GetListenerAttributes_Internal2 ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_GetListenerAttributes_Delegate2>(nameof(FMOD_Studio_System_GetListenerAttributes));

        private delegate RESULT FMOD_Studio_System_SetListenerAttributes_Delegate(IntPtr system, int listener, ref ATTRIBUTES_3D attributes, IntPtr zero);
        private static FMOD_Studio_System_SetListenerAttributes_Delegate FMOD_Studio_System_SetListenerAttributes_Internal = null;
        private static FMOD_Studio_System_SetListenerAttributes_Delegate FMOD_Studio_System_SetListenerAttributes => FMOD_Studio_System_SetListenerAttributes_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_SetListenerAttributes_Delegate>(nameof(FMOD_Studio_System_SetListenerAttributes));

        private delegate RESULT FMOD_Studio_System_SetListenerAttributes_Delegate2(IntPtr system, int listener, ref ATTRIBUTES_3D attributes, ref VECTOR attenuationposition);
        private static FMOD_Studio_System_SetListenerAttributes_Delegate2 FMOD_Studio_System_SetListenerAttributes_Internal2 = null;
        private static FMOD_Studio_System_SetListenerAttributes_Delegate2 FMOD_Studio_System_SetListenerAttributes2 => FMOD_Studio_System_SetListenerAttributes_Internal2 ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_SetListenerAttributes_Delegate2>(nameof(FMOD_Studio_System_SetListenerAttributes));

        private delegate RESULT FMOD_Studio_System_GetListenerWeight_Delegate(IntPtr system, int listener, out float weight);
        private static FMOD_Studio_System_GetListenerWeight_Delegate FMOD_Studio_System_GetListenerWeight_Internal = null;
        private static FMOD_Studio_System_GetListenerWeight_Delegate FMOD_Studio_System_GetListenerWeight => FMOD_Studio_System_GetListenerWeight_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_GetListenerWeight_Delegate>(nameof(FMOD_Studio_System_GetListenerWeight));

        private delegate RESULT FMOD_Studio_System_SetListenerWeight_Delegate(IntPtr system, int listener, float weight);
        private static FMOD_Studio_System_SetListenerWeight_Delegate FMOD_Studio_System_SetListenerWeight_Internal = null;
        private static FMOD_Studio_System_SetListenerWeight_Delegate FMOD_Studio_System_SetListenerWeight => FMOD_Studio_System_SetListenerWeight_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_SetListenerWeight_Delegate>(nameof(FMOD_Studio_System_SetListenerWeight));

        private delegate RESULT FMOD_Studio_System_LoadBankFile_Delegate(IntPtr system, byte[] filename, LOAD_BANK_FLAGS flags, out IntPtr bank);
        private static FMOD_Studio_System_LoadBankFile_Delegate FMOD_Studio_System_LoadBankFile_Internal = null;
        private static FMOD_Studio_System_LoadBankFile_Delegate FMOD_Studio_System_LoadBankFile => FMOD_Studio_System_LoadBankFile_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_LoadBankFile_Delegate>(nameof(FMOD_Studio_System_LoadBankFile));

        public delegate RESULT FMOD_Studio_System_LoadBankMemory_Delegate(IntPtr system, IntPtr buffer, int length, LOAD_MEMORY_MODE mode, LOAD_BANK_FLAGS flags, out IntPtr bank);
        private static FMOD_Studio_System_LoadBankMemory_Delegate FMOD_Studio_System_LoadBankMemory_Internal = null;
        public static FMOD_Studio_System_LoadBankMemory_Delegate FMOD_Studio_System_LoadBankMemory => FMOD_Studio_System_LoadBankMemory_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_LoadBankMemory_Delegate>(nameof(FMOD_Studio_System_LoadBankMemory));

        private delegate RESULT FMOD_Studio_System_LoadBankCustom_Delegate(IntPtr system, ref BANK_INFO info, LOAD_BANK_FLAGS flags, out IntPtr bank);
        private static FMOD_Studio_System_LoadBankCustom_Delegate FMOD_Studio_System_LoadBankCustom_Internal = null;
        private static FMOD_Studio_System_LoadBankCustom_Delegate FMOD_Studio_System_LoadBankCustom => FMOD_Studio_System_LoadBankCustom_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_LoadBankCustom_Delegate>(nameof(FMOD_Studio_System_LoadBankCustom));

        private delegate RESULT FMOD_Studio_System_UnloadAll_Delegate(IntPtr system);
        private static FMOD_Studio_System_UnloadAll_Delegate FMOD_Studio_System_UnloadAll_Internal = null;
        private static FMOD_Studio_System_UnloadAll_Delegate FMOD_Studio_System_UnloadAll => FMOD_Studio_System_UnloadAll_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_UnloadAll_Delegate>(nameof(FMOD_Studio_System_UnloadAll));

        private delegate RESULT FMOD_Studio_System_FlushCommands_Delegate(IntPtr system);
        private static FMOD_Studio_System_FlushCommands_Delegate FMOD_Studio_System_FlushCommands_Internal = null;
        private static FMOD_Studio_System_FlushCommands_Delegate FMOD_Studio_System_FlushCommands => FMOD_Studio_System_FlushCommands_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_FlushCommands_Delegate>(nameof(FMOD_Studio_System_FlushCommands));

        private delegate RESULT FMOD_Studio_System_FlushSampleLoading_Delegate(IntPtr system);
        private static FMOD_Studio_System_FlushSampleLoading_Delegate FMOD_Studio_System_FlushSampleLoading_Internal = null;
        private static FMOD_Studio_System_FlushSampleLoading_Delegate FMOD_Studio_System_FlushSampleLoading => FMOD_Studio_System_FlushSampleLoading_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_FlushSampleLoading_Delegate>(nameof(FMOD_Studio_System_FlushSampleLoading));

        private delegate RESULT FMOD_Studio_System_StartCommandCapture_Delegate(IntPtr system, byte[] filename, COMMANDCAPTURE_FLAGS flags);
        private static FMOD_Studio_System_StartCommandCapture_Delegate FMOD_Studio_System_StartCommandCapture_Internal = null;
        private static FMOD_Studio_System_StartCommandCapture_Delegate FMOD_Studio_System_StartCommandCapture => FMOD_Studio_System_StartCommandCapture_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_StartCommandCapture_Delegate>(nameof(FMOD_Studio_System_StartCommandCapture));

        private delegate RESULT FMOD_Studio_System_StopCommandCapture_Delegate(IntPtr system);
        private static FMOD_Studio_System_StopCommandCapture_Delegate FMOD_Studio_System_StopCommandCapture_Internal = null;
        private static FMOD_Studio_System_StopCommandCapture_Delegate FMOD_Studio_System_StopCommandCapture => FMOD_Studio_System_StopCommandCapture_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_StopCommandCapture_Delegate>(nameof(FMOD_Studio_System_StopCommandCapture));

        private delegate RESULT FMOD_Studio_System_LoadCommandReplay_Delegate(IntPtr system, byte[] filename, COMMANDREPLAY_FLAGS flags, out IntPtr replay);
        private static FMOD_Studio_System_LoadCommandReplay_Delegate FMOD_Studio_System_LoadCommandReplay_Internal = null;
        private static FMOD_Studio_System_LoadCommandReplay_Delegate FMOD_Studio_System_LoadCommandReplay => FMOD_Studio_System_LoadCommandReplay_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_LoadCommandReplay_Delegate>(nameof(FMOD_Studio_System_LoadCommandReplay));

        private delegate RESULT FMOD_Studio_System_GetBankCount_Delegate(IntPtr system, out int count);
        private static FMOD_Studio_System_GetBankCount_Delegate FMOD_Studio_System_GetBankCount_Internal = null;
        private static FMOD_Studio_System_GetBankCount_Delegate FMOD_Studio_System_GetBankCount => FMOD_Studio_System_GetBankCount_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_GetBankCount_Delegate>(nameof(FMOD_Studio_System_GetBankCount));

        private delegate RESULT FMOD_Studio_System_GetBankList_Delegate(IntPtr system, IntPtr[] array, int capacity, out int count);
        private static FMOD_Studio_System_GetBankList_Delegate FMOD_Studio_System_GetBankList_Internal = null;
        private static FMOD_Studio_System_GetBankList_Delegate FMOD_Studio_System_GetBankList => FMOD_Studio_System_GetBankList_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_GetBankList_Delegate>(nameof(FMOD_Studio_System_GetBankList));

        private delegate RESULT FMOD_Studio_System_GetParameterDescriptionCount_Delegate(IntPtr system, out int count);
        private static FMOD_Studio_System_GetParameterDescriptionCount_Delegate FMOD_Studio_System_GetParameterDescriptionCount_Internal = null;
        private static FMOD_Studio_System_GetParameterDescriptionCount_Delegate FMOD_Studio_System_GetParameterDescriptionCount => FMOD_Studio_System_GetParameterDescriptionCount_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_GetParameterDescriptionCount_Delegate>(nameof(FMOD_Studio_System_GetParameterDescriptionCount));

        private delegate RESULT FMOD_Studio_System_GetParameterDescriptionList_Delegate(IntPtr system, [Out] PARAMETER_DESCRIPTION[] array, int capacity, out int count);
        private static FMOD_Studio_System_GetParameterDescriptionList_Delegate FMOD_Studio_System_GetParameterDescriptionList_Internal = null;
        private static FMOD_Studio_System_GetParameterDescriptionList_Delegate FMOD_Studio_System_GetParameterDescriptionList => FMOD_Studio_System_GetParameterDescriptionList_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_GetParameterDescriptionList_Delegate>(nameof(FMOD_Studio_System_GetParameterDescriptionList));

        private delegate RESULT FMOD_Studio_System_GetCPUUsage_Delegate(IntPtr system, out CPU_USAGE usage, out FMOD.CPU_USAGE usage_core);
        private static FMOD_Studio_System_GetCPUUsage_Delegate FMOD_Studio_System_GetCPUUsage_Internal = null;
        private static FMOD_Studio_System_GetCPUUsage_Delegate FMOD_Studio_System_GetCPUUsage => FMOD_Studio_System_GetCPUUsage_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_GetCPUUsage_Delegate>(nameof(FMOD_Studio_System_GetCPUUsage));

        private delegate RESULT FMOD_Studio_System_GetBufferUsage_Delegate(IntPtr system, out BUFFER_USAGE usage);
        private static FMOD_Studio_System_GetBufferUsage_Delegate FMOD_Studio_System_GetBufferUsage_Internal = null;
        private static FMOD_Studio_System_GetBufferUsage_Delegate FMOD_Studio_System_GetBufferUsage => FMOD_Studio_System_GetBufferUsage_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_GetBufferUsage_Delegate>(nameof(FMOD_Studio_System_GetBufferUsage));

        private delegate RESULT FMOD_Studio_System_ResetBufferUsage_Delegate(IntPtr system);
        private static FMOD_Studio_System_ResetBufferUsage_Delegate FMOD_Studio_System_ResetBufferUsage_Internal = null;
        private static FMOD_Studio_System_ResetBufferUsage_Delegate FMOD_Studio_System_ResetBufferUsage => FMOD_Studio_System_ResetBufferUsage_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_ResetBufferUsage_Delegate>(nameof(FMOD_Studio_System_ResetBufferUsage));

        private delegate RESULT FMOD_Studio_System_SetCallback_Delegate(IntPtr system, SYSTEM_CALLBACK callback, SYSTEM_CALLBACK_TYPE callbackmask);
        private static FMOD_Studio_System_SetCallback_Delegate FMOD_Studio_System_SetCallback_Internal = null;
        private static FMOD_Studio_System_SetCallback_Delegate FMOD_Studio_System_SetCallback => FMOD_Studio_System_SetCallback_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_SetCallback_Delegate>(nameof(FMOD_Studio_System_SetCallback));

        private delegate RESULT FMOD_Studio_System_GetUserData_Delegate(IntPtr system, out IntPtr userdata);
        private static FMOD_Studio_System_GetUserData_Delegate FMOD_Studio_System_GetUserData_Internal = null;
        private static FMOD_Studio_System_GetUserData_Delegate FMOD_Studio_System_GetUserData => FMOD_Studio_System_GetUserData_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_GetUserData_Delegate>(nameof(FMOD_Studio_System_GetUserData));

        private delegate RESULT FMOD_Studio_System_SetUserData_Delegate(IntPtr system, IntPtr userdata);
        private static FMOD_Studio_System_SetUserData_Delegate FMOD_Studio_System_SetUserData_Internal = null;
        private static FMOD_Studio_System_SetUserData_Delegate FMOD_Studio_System_SetUserData => FMOD_Studio_System_SetUserData_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_SetUserData_Delegate>(nameof(FMOD_Studio_System_SetUserData));

        private delegate RESULT FMOD_Studio_System_GetMemoryUsage_Delegate(IntPtr system, out MEMORY_USAGE memoryusage);
        private static FMOD_Studio_System_GetMemoryUsage_Delegate FMOD_Studio_System_GetMemoryUsage_Internal = null;
        private static FMOD_Studio_System_GetMemoryUsage_Delegate FMOD_Studio_System_GetMemoryUsage => FMOD_Studio_System_GetMemoryUsage_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_System_GetMemoryUsage_Delegate>(nameof(FMOD_Studio_System_GetMemoryUsage));

        #endregion

        #region wrapperinternal

        public IntPtr handle;

        public System(IntPtr ptr)   { this.handle = ptr; }
        public bool hasHandle()     { return this.handle != IntPtr.Zero; }
        public void clearHandle()   { this.handle = IntPtr.Zero; }

        public bool isValid()
        {
            return hasHandle() && FMOD_Studio_System_IsValid(this.handle);
        }

        #endregion
    }

    public struct EventDescription
    {
        public RESULT getID(out GUID id)
        {
            return FMOD_Studio_EventDescription_GetID(this.handle, out id);
        }
        public RESULT getPath(out string path)
        {
            path = null;

            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                IntPtr stringMem = Marshal.AllocHGlobal(256);
                int retrieved = 0;
                RESULT result = FMOD_Studio_EventDescription_GetPath(this.handle, stringMem, 256, out retrieved);

                if (result == RESULT.ERR_TRUNCATED)
                {
                    Marshal.FreeHGlobal(stringMem);
                    stringMem = Marshal.AllocHGlobal(retrieved);
                    result = FMOD_Studio_EventDescription_GetPath(this.handle, stringMem, retrieved, out retrieved);
                }

                if (result == RESULT.OK)
                {
                    path = encoder.stringFromNative(stringMem);
                }
                Marshal.FreeHGlobal(stringMem);
                return result;
            }
        }
        public RESULT getParameterDescriptionCount(out int count)
        {
            return FMOD_Studio_EventDescription_GetParameterDescriptionCount(this.handle, out count);
        }
        public RESULT getParameterDescriptionByIndex(int index, out PARAMETER_DESCRIPTION parameter)
        {
            return FMOD_Studio_EventDescription_GetParameterDescriptionByIndex(this.handle, index, out parameter);
        }
        public RESULT getParameterDescriptionByName(string name, out PARAMETER_DESCRIPTION parameter)
        {
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                return FMOD_Studio_EventDescription_GetParameterDescriptionByName(this.handle, encoder.byteFromStringUTF8(name), out parameter);
            }
        }
        public RESULT getParameterDescriptionByID(PARAMETER_ID id, out PARAMETER_DESCRIPTION parameter)
        {
            return FMOD_Studio_EventDescription_GetParameterDescriptionByID(this.handle, id, out parameter);
        }
        public RESULT getParameterLabelByIndex(int index, int labelindex, out string label)
        {
            label = null;

            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                IntPtr stringMem = Marshal.AllocHGlobal(256);
                int retrieved = 0;
                RESULT result = FMOD_Studio_EventDescription_GetParameterLabelByIndex(this.handle, index, labelindex, stringMem, 256, out retrieved);

                if (result == RESULT.ERR_TRUNCATED)
                {
                    Marshal.FreeHGlobal(stringMem);
                    result = FMOD_Studio_EventDescription_GetParameterLabelByIndex(this.handle, index, labelindex, IntPtr.Zero, 0, out retrieved);
                    stringMem = Marshal.AllocHGlobal(retrieved);
                    result = FMOD_Studio_EventDescription_GetParameterLabelByIndex(this.handle, index, labelindex, stringMem, retrieved, out retrieved);
                }

                if (result == RESULT.OK)
                {
                    label = encoder.stringFromNative(stringMem);
                }
                Marshal.FreeHGlobal(stringMem);
                return result;
            }
        }
        public RESULT getParameterLabelByName(string name, int labelindex, out string label)
        {
            label = null;

            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                IntPtr stringMem = Marshal.AllocHGlobal(256);
                int retrieved = 0;
                byte[] nameBytes = encoder.byteFromStringUTF8(name);
                RESULT result = FMOD_Studio_EventDescription_GetParameterLabelByName(this.handle, nameBytes, labelindex, stringMem, 256, out retrieved);

                if (result == RESULT.ERR_TRUNCATED)
                {
                    Marshal.FreeHGlobal(stringMem);
                    result = FMOD_Studio_EventDescription_GetParameterLabelByName(this.handle, nameBytes, labelindex, IntPtr.Zero, 0, out retrieved);
                    stringMem = Marshal.AllocHGlobal(retrieved);
                    result = FMOD_Studio_EventDescription_GetParameterLabelByName(this.handle, nameBytes, labelindex, stringMem, retrieved, out retrieved);
                }

                if (result == RESULT.OK)
                {
                    label = encoder.stringFromNative(stringMem);
                }
                Marshal.FreeHGlobal(stringMem);
                return result;
            }
        }
        public RESULT getParameterLabelByID(PARAMETER_ID id, int labelindex, out string label)
        {
            label = null;

            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                IntPtr stringMem = Marshal.AllocHGlobal(256);
                int retrieved = 0;
                RESULT result = FMOD_Studio_EventDescription_GetParameterLabelByID(this.handle, id, labelindex, stringMem, 256, out retrieved);

                if (result == RESULT.ERR_TRUNCATED)
                {
                    Marshal.FreeHGlobal(stringMem);
                    result = FMOD_Studio_EventDescription_GetParameterLabelByID(this.handle, id, labelindex, IntPtr.Zero, 0, out retrieved);
                    stringMem = Marshal.AllocHGlobal(retrieved);
                    result = FMOD_Studio_EventDescription_GetParameterLabelByID(this.handle, id, labelindex, stringMem, retrieved, out retrieved);
                }

                if (result == RESULT.OK)
                {
                    label = encoder.stringFromNative(stringMem);
                }
                Marshal.FreeHGlobal(stringMem);
                return result;
            }
        }
        public RESULT getUserPropertyCount(out int count)
        {
            return FMOD_Studio_EventDescription_GetUserPropertyCount(this.handle, out count);
        }
        public RESULT getUserPropertyByIndex(int index, out USER_PROPERTY property)
        {
            return FMOD_Studio_EventDescription_GetUserPropertyByIndex(this.handle, index, out property);
        }
        public RESULT getUserProperty(string name, out USER_PROPERTY property)
        {
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                return FMOD_Studio_EventDescription_GetUserProperty(this.handle, encoder.byteFromStringUTF8(name), out property);
            }
        }
        public RESULT getLength(out int length)
        {
            return FMOD_Studio_EventDescription_GetLength(this.handle, out length);
        }
        public RESULT getMinMaxDistance(out float min, out float max)
        {
            return FMOD_Studio_EventDescription_GetMinMaxDistance(this.handle, out min, out max);
        }
        public RESULT getSoundSize(out float size)
        {
            return FMOD_Studio_EventDescription_GetSoundSize(this.handle, out size);
        }
        public RESULT isSnapshot(out bool snapshot)
        {
            return FMOD_Studio_EventDescription_IsSnapshot(this.handle, out snapshot);
        }
        public RESULT isOneshot(out bool oneshot)
        {
            return FMOD_Studio_EventDescription_IsOneshot(this.handle, out oneshot);
        }
        public RESULT isStream(out bool isStream)
        {
            return FMOD_Studio_EventDescription_IsStream(this.handle, out isStream);
        }
        public RESULT is3D(out bool is3D)
        {
            return FMOD_Studio_EventDescription_Is3D(this.handle, out is3D);
        }
        public RESULT isDopplerEnabled(out bool doppler)
        {
            return FMOD_Studio_EventDescription_IsDopplerEnabled(this.handle, out doppler);
        }
        public RESULT hasSustainPoint(out bool sustainPoint)
        {
            return FMOD_Studio_EventDescription_HasSustainPoint(this.handle, out sustainPoint);
        }

        public RESULT createInstance(out EventInstance instance)
        {
            return FMOD_Studio_EventDescription_CreateInstance(this.handle, out instance.handle);
        }

        public RESULT getInstanceCount(out int count)
        {
            return FMOD_Studio_EventDescription_GetInstanceCount(this.handle, out count);
        }
        public RESULT getInstanceList(out EventInstance[] array)
        {
            array = null;

            RESULT result;
            int capacity;
            result = FMOD_Studio_EventDescription_GetInstanceCount(this.handle, out capacity);
            if (result != RESULT.OK)
            {
                return result;
            }
            if (capacity == 0)
            {
                array = new EventInstance[0];
                return result;
            }

            IntPtr[] rawArray = new IntPtr[capacity];
            int actualCount;
            result = FMOD_Studio_EventDescription_GetInstanceList(this.handle, rawArray, capacity, out actualCount);
            if (result != RESULT.OK)
            {
                return result;
            }
            if (actualCount > capacity) // More items added since we queried just now?
            {
                actualCount = capacity;
            }
            array = new EventInstance[actualCount];
            for (int i = 0; i < actualCount; ++i)
            {
                array[i].handle = rawArray[i];
            }
            return RESULT.OK;
        }

        public RESULT loadSampleData()
        {
            return FMOD_Studio_EventDescription_LoadSampleData(this.handle);
        }

        public RESULT unloadSampleData()
        {
            return FMOD_Studio_EventDescription_UnloadSampleData(this.handle);
        }

        public RESULT getSampleLoadingState(out LOADING_STATE state)
        {
            return FMOD_Studio_EventDescription_GetSampleLoadingState(this.handle, out state);
        }

        public RESULT releaseAllInstances()
        {
            return FMOD_Studio_EventDescription_ReleaseAllInstances(this.handle);
        }
        public RESULT setCallback(EVENT_CALLBACK callback, EVENT_CALLBACK_TYPE callbackmask = EVENT_CALLBACK_TYPE.ALL)
        {
            return FMOD_Studio_EventDescription_SetCallback(this.handle, callback, callbackmask);
        }

        public RESULT getUserData(out IntPtr userdata)
        {
            return FMOD_Studio_EventDescription_GetUserData(this.handle, out userdata);
        }

        public RESULT setUserData(IntPtr userdata)
        {
            return FMOD_Studio_EventDescription_SetUserData(this.handle, userdata);
        }

        #region importfunctions
        private delegate bool FMOD_Studio_EventDescription_IsValid_Delegate(IntPtr eventdescription);
        private static FMOD_Studio_EventDescription_IsValid_Delegate FMOD_Studio_EventDescription_IsValid_Internal = null;
        private static FMOD_Studio_EventDescription_IsValid_Delegate FMOD_Studio_EventDescription_IsValid => FMOD_Studio_EventDescription_IsValid_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventDescription_IsValid_Delegate>(nameof(FMOD_Studio_EventDescription_IsValid));

        private delegate RESULT FMOD_Studio_EventDescription_GetID_Delegate(IntPtr eventdescription, out GUID id);
        private static FMOD_Studio_EventDescription_GetID_Delegate FMOD_Studio_EventDescription_GetID_Internal = null;
        private static FMOD_Studio_EventDescription_GetID_Delegate FMOD_Studio_EventDescription_GetID => FMOD_Studio_EventDescription_GetID_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventDescription_GetID_Delegate>(nameof(FMOD_Studio_EventDescription_GetID));

        private delegate RESULT FMOD_Studio_EventDescription_GetPath_Delegate(IntPtr eventdescription, IntPtr path, int size, out int retrieved);
        private static FMOD_Studio_EventDescription_GetPath_Delegate FMOD_Studio_EventDescription_GetPath_Internal = null;
        private static FMOD_Studio_EventDescription_GetPath_Delegate FMOD_Studio_EventDescription_GetPath => FMOD_Studio_EventDescription_GetPath_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventDescription_GetPath_Delegate>(nameof(FMOD_Studio_EventDescription_GetPath));

        private delegate RESULT FMOD_Studio_EventDescription_GetParameterDescriptionCount_Delegate(IntPtr eventdescription, out int count);
        private static FMOD_Studio_EventDescription_GetParameterDescriptionCount_Delegate FMOD_Studio_EventDescription_GetParameterDescriptionCount_Internal = null;
        private static FMOD_Studio_EventDescription_GetParameterDescriptionCount_Delegate FMOD_Studio_EventDescription_GetParameterDescriptionCount => FMOD_Studio_EventDescription_GetParameterDescriptionCount_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventDescription_GetParameterDescriptionCount_Delegate>(nameof(FMOD_Studio_EventDescription_GetParameterDescriptionCount));

        private delegate RESULT FMOD_Studio_EventDescription_GetParameterDescriptionByIndex_Delegate(IntPtr eventdescription, int index, out PARAMETER_DESCRIPTION parameter);
        private static FMOD_Studio_EventDescription_GetParameterDescriptionByIndex_Delegate FMOD_Studio_EventDescription_GetParameterDescriptionByIndex_Internal = null;
        private static FMOD_Studio_EventDescription_GetParameterDescriptionByIndex_Delegate FMOD_Studio_EventDescription_GetParameterDescriptionByIndex => FMOD_Studio_EventDescription_GetParameterDescriptionByIndex_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventDescription_GetParameterDescriptionByIndex_Delegate>(nameof(FMOD_Studio_EventDescription_GetParameterDescriptionByIndex));

        private delegate RESULT FMOD_Studio_EventDescription_GetParameterDescriptionByName_Delegate(IntPtr eventdescription, byte[] name, out PARAMETER_DESCRIPTION parameter);
        private static FMOD_Studio_EventDescription_GetParameterDescriptionByName_Delegate FMOD_Studio_EventDescription_GetParameterDescriptionByName_Internal = null;
        private static FMOD_Studio_EventDescription_GetParameterDescriptionByName_Delegate FMOD_Studio_EventDescription_GetParameterDescriptionByName => FMOD_Studio_EventDescription_GetParameterDescriptionByName_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventDescription_GetParameterDescriptionByName_Delegate>(nameof(FMOD_Studio_EventDescription_GetParameterDescriptionByName));

        private delegate RESULT FMOD_Studio_EventDescription_GetParameterDescriptionByID_Delegate(IntPtr eventdescription, PARAMETER_ID id, out PARAMETER_DESCRIPTION parameter);
        private static FMOD_Studio_EventDescription_GetParameterDescriptionByID_Delegate FMOD_Studio_EventDescription_GetParameterDescriptionByID_Internal = null;
        private static FMOD_Studio_EventDescription_GetParameterDescriptionByID_Delegate FMOD_Studio_EventDescription_GetParameterDescriptionByID => FMOD_Studio_EventDescription_GetParameterDescriptionByID_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventDescription_GetParameterDescriptionByID_Delegate>(nameof(FMOD_Studio_EventDescription_GetParameterDescriptionByID));

        private delegate RESULT FMOD_Studio_EventDescription_GetParameterLabelByIndex_Delegate(IntPtr eventdescription, int index, int labelindex, IntPtr label, int size, out int retrieved);
        private static FMOD_Studio_EventDescription_GetParameterLabelByIndex_Delegate FMOD_Studio_EventDescription_GetParameterLabelByIndex_Internal = null;
        private static FMOD_Studio_EventDescription_GetParameterLabelByIndex_Delegate FMOD_Studio_EventDescription_GetParameterLabelByIndex => FMOD_Studio_EventDescription_GetParameterLabelByIndex_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventDescription_GetParameterLabelByIndex_Delegate>(nameof(FMOD_Studio_EventDescription_GetParameterLabelByIndex));

        private delegate RESULT FMOD_Studio_EventDescription_GetParameterLabelByName_Delegate(IntPtr eventdescription, byte[] name, int labelindex, IntPtr label, int size, out int retrieved);
        private static FMOD_Studio_EventDescription_GetParameterLabelByName_Delegate FMOD_Studio_EventDescription_GetParameterLabelByName_Internal = null;
        private static FMOD_Studio_EventDescription_GetParameterLabelByName_Delegate FMOD_Studio_EventDescription_GetParameterLabelByName => FMOD_Studio_EventDescription_GetParameterLabelByName_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventDescription_GetParameterLabelByName_Delegate>(nameof(FMOD_Studio_EventDescription_GetParameterLabelByName));

        private delegate RESULT FMOD_Studio_EventDescription_GetParameterLabelByID_Delegate(IntPtr eventdescription, PARAMETER_ID id, int labelindex, IntPtr label, int size, out int retrieved);
        private static FMOD_Studio_EventDescription_GetParameterLabelByID_Delegate FMOD_Studio_EventDescription_GetParameterLabelByID_Internal = null;
        private static FMOD_Studio_EventDescription_GetParameterLabelByID_Delegate FMOD_Studio_EventDescription_GetParameterLabelByID => FMOD_Studio_EventDescription_GetParameterLabelByID_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventDescription_GetParameterLabelByID_Delegate>(nameof(FMOD_Studio_EventDescription_GetParameterLabelByID));

        private delegate RESULT FMOD_Studio_EventDescription_GetUserPropertyCount_Delegate(IntPtr eventdescription, out int count);
        private static FMOD_Studio_EventDescription_GetUserPropertyCount_Delegate FMOD_Studio_EventDescription_GetUserPropertyCount_Internal = null;
        private static FMOD_Studio_EventDescription_GetUserPropertyCount_Delegate FMOD_Studio_EventDescription_GetUserPropertyCount => FMOD_Studio_EventDescription_GetUserPropertyCount_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventDescription_GetUserPropertyCount_Delegate>(nameof(FMOD_Studio_EventDescription_GetUserPropertyCount));

        private delegate RESULT FMOD_Studio_EventDescription_GetUserPropertyByIndex_Delegate(IntPtr eventdescription, int index, out USER_PROPERTY property);
        private static FMOD_Studio_EventDescription_GetUserPropertyByIndex_Delegate FMOD_Studio_EventDescription_GetUserPropertyByIndex_Internal = null;
        private static FMOD_Studio_EventDescription_GetUserPropertyByIndex_Delegate FMOD_Studio_EventDescription_GetUserPropertyByIndex => FMOD_Studio_EventDescription_GetUserPropertyByIndex_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventDescription_GetUserPropertyByIndex_Delegate>(nameof(FMOD_Studio_EventDescription_GetUserPropertyByIndex));

        private delegate RESULT FMOD_Studio_EventDescription_GetUserProperty_Delegate(IntPtr eventdescription, byte[] name, out USER_PROPERTY property);
        private static FMOD_Studio_EventDescription_GetUserProperty_Delegate FMOD_Studio_EventDescription_GetUserProperty_Internal = null;
        private static FMOD_Studio_EventDescription_GetUserProperty_Delegate FMOD_Studio_EventDescription_GetUserProperty => FMOD_Studio_EventDescription_GetUserProperty_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventDescription_GetUserProperty_Delegate>(nameof(FMOD_Studio_EventDescription_GetUserProperty));

        private delegate RESULT FMOD_Studio_EventDescription_GetLength_Delegate(IntPtr eventdescription, out int length);
        private static FMOD_Studio_EventDescription_GetLength_Delegate FMOD_Studio_EventDescription_GetLength_Internal = null;
        private static FMOD_Studio_EventDescription_GetLength_Delegate FMOD_Studio_EventDescription_GetLength => FMOD_Studio_EventDescription_GetLength_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventDescription_GetLength_Delegate>(nameof(FMOD_Studio_EventDescription_GetLength));

        private delegate RESULT FMOD_Studio_EventDescription_GetMinMaxDistance_Delegate(IntPtr eventdescription, out float min, out float max);
        private static FMOD_Studio_EventDescription_GetMinMaxDistance_Delegate FMOD_Studio_EventDescription_GetMinMaxDistance_Internal = null;
        private static FMOD_Studio_EventDescription_GetMinMaxDistance_Delegate FMOD_Studio_EventDescription_GetMinMaxDistance => FMOD_Studio_EventDescription_GetMinMaxDistance_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventDescription_GetMinMaxDistance_Delegate>(nameof(FMOD_Studio_EventDescription_GetMinMaxDistance));

        private delegate RESULT FMOD_Studio_EventDescription_GetSoundSize_Delegate(IntPtr eventdescription, out float size);
        private static FMOD_Studio_EventDescription_GetSoundSize_Delegate FMOD_Studio_EventDescription_GetSoundSize_Internal = null;
        private static FMOD_Studio_EventDescription_GetSoundSize_Delegate FMOD_Studio_EventDescription_GetSoundSize => FMOD_Studio_EventDescription_GetSoundSize_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventDescription_GetSoundSize_Delegate>(nameof(FMOD_Studio_EventDescription_GetSoundSize));

        private delegate RESULT FMOD_Studio_EventDescription_IsSnapshot_Delegate(IntPtr eventdescription, out bool snapshot);
        private static FMOD_Studio_EventDescription_IsSnapshot_Delegate FMOD_Studio_EventDescription_IsSnapshot_Internal = null;
        private static FMOD_Studio_EventDescription_IsSnapshot_Delegate FMOD_Studio_EventDescription_IsSnapshot => FMOD_Studio_EventDescription_IsSnapshot_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventDescription_IsSnapshot_Delegate>(nameof(FMOD_Studio_EventDescription_IsSnapshot));

        private delegate RESULT FMOD_Studio_EventDescription_IsOneshot_Delegate(IntPtr eventdescription, out bool oneshot);
        private static FMOD_Studio_EventDescription_IsOneshot_Delegate FMOD_Studio_EventDescription_IsOneshot_Internal = null;
        private static FMOD_Studio_EventDescription_IsOneshot_Delegate FMOD_Studio_EventDescription_IsOneshot => FMOD_Studio_EventDescription_IsOneshot_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventDescription_IsOneshot_Delegate>(nameof(FMOD_Studio_EventDescription_IsOneshot));

        private delegate RESULT FMOD_Studio_EventDescription_IsStream_Delegate(IntPtr eventdescription, out bool isStream);
        private static FMOD_Studio_EventDescription_IsStream_Delegate FMOD_Studio_EventDescription_IsStream_Internal = null;
        private static FMOD_Studio_EventDescription_IsStream_Delegate FMOD_Studio_EventDescription_IsStream => FMOD_Studio_EventDescription_IsStream_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventDescription_IsStream_Delegate>(nameof(FMOD_Studio_EventDescription_IsStream));

        private delegate RESULT FMOD_Studio_EventDescription_Is3D_Delegate(IntPtr eventdescription, out bool is3D);
        private static FMOD_Studio_EventDescription_Is3D_Delegate FMOD_Studio_EventDescription_Is3D_Internal = null;
        private static FMOD_Studio_EventDescription_Is3D_Delegate FMOD_Studio_EventDescription_Is3D => FMOD_Studio_EventDescription_Is3D_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventDescription_Is3D_Delegate>(nameof(FMOD_Studio_EventDescription_Is3D));

        private delegate RESULT FMOD_Studio_EventDescription_IsDopplerEnabled_Delegate(IntPtr eventdescription, out bool doppler);
        private static FMOD_Studio_EventDescription_IsDopplerEnabled_Delegate FMOD_Studio_EventDescription_IsDopplerEnabled_Internal = null;
        private static FMOD_Studio_EventDescription_IsDopplerEnabled_Delegate FMOD_Studio_EventDescription_IsDopplerEnabled => FMOD_Studio_EventDescription_IsDopplerEnabled_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventDescription_IsDopplerEnabled_Delegate>(nameof(FMOD_Studio_EventDescription_IsDopplerEnabled));

        private delegate RESULT FMOD_Studio_EventDescription_HasSustainPoint_Delegate(IntPtr eventdescription, out bool sustainPoint);
        private static FMOD_Studio_EventDescription_HasSustainPoint_Delegate FMOD_Studio_EventDescription_HasSustainPoint_Internal = null;
        private static FMOD_Studio_EventDescription_HasSustainPoint_Delegate FMOD_Studio_EventDescription_HasSustainPoint => FMOD_Studio_EventDescription_HasSustainPoint_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventDescription_HasSustainPoint_Delegate>(nameof(FMOD_Studio_EventDescription_HasSustainPoint));

        private delegate RESULT FMOD_Studio_EventDescription_CreateInstance_Delegate(IntPtr eventdescription, out IntPtr instance);
        private static FMOD_Studio_EventDescription_CreateInstance_Delegate FMOD_Studio_EventDescription_CreateInstance_Internal = null;
        private static FMOD_Studio_EventDescription_CreateInstance_Delegate FMOD_Studio_EventDescription_CreateInstance => FMOD_Studio_EventDescription_CreateInstance_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventDescription_CreateInstance_Delegate>(nameof(FMOD_Studio_EventDescription_CreateInstance));

        private delegate RESULT FMOD_Studio_EventDescription_GetInstanceCount_Delegate(IntPtr eventdescription, out int count);
        private static FMOD_Studio_EventDescription_GetInstanceCount_Delegate FMOD_Studio_EventDescription_GetInstanceCount_Internal = null;
        private static FMOD_Studio_EventDescription_GetInstanceCount_Delegate FMOD_Studio_EventDescription_GetInstanceCount => FMOD_Studio_EventDescription_GetInstanceCount_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventDescription_GetInstanceCount_Delegate>(nameof(FMOD_Studio_EventDescription_GetInstanceCount));

        private delegate RESULT FMOD_Studio_EventDescription_GetInstanceList_Delegate(IntPtr eventdescription, IntPtr[] array, int capacity, out int count);
        private static FMOD_Studio_EventDescription_GetInstanceList_Delegate FMOD_Studio_EventDescription_GetInstanceList_Internal = null;
        private static FMOD_Studio_EventDescription_GetInstanceList_Delegate FMOD_Studio_EventDescription_GetInstanceList => FMOD_Studio_EventDescription_GetInstanceList_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventDescription_GetInstanceList_Delegate>(nameof(FMOD_Studio_EventDescription_GetInstanceList));

        private delegate RESULT FMOD_Studio_EventDescription_LoadSampleData_Delegate(IntPtr eventdescription);
        private static FMOD_Studio_EventDescription_LoadSampleData_Delegate FMOD_Studio_EventDescription_LoadSampleData_Internal = null;
        private static FMOD_Studio_EventDescription_LoadSampleData_Delegate FMOD_Studio_EventDescription_LoadSampleData => FMOD_Studio_EventDescription_LoadSampleData_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventDescription_LoadSampleData_Delegate>(nameof(FMOD_Studio_EventDescription_LoadSampleData));

        private delegate RESULT FMOD_Studio_EventDescription_UnloadSampleData_Delegate(IntPtr eventdescription);
        private static FMOD_Studio_EventDescription_UnloadSampleData_Delegate FMOD_Studio_EventDescription_UnloadSampleData_Internal = null;
        private static FMOD_Studio_EventDescription_UnloadSampleData_Delegate FMOD_Studio_EventDescription_UnloadSampleData => FMOD_Studio_EventDescription_UnloadSampleData_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventDescription_UnloadSampleData_Delegate>(nameof(FMOD_Studio_EventDescription_UnloadSampleData));

        private delegate RESULT FMOD_Studio_EventDescription_GetSampleLoadingState_Delegate(IntPtr eventdescription, out LOADING_STATE state);
        private static FMOD_Studio_EventDescription_GetSampleLoadingState_Delegate FMOD_Studio_EventDescription_GetSampleLoadingState_Internal = null;
        private static FMOD_Studio_EventDescription_GetSampleLoadingState_Delegate FMOD_Studio_EventDescription_GetSampleLoadingState => FMOD_Studio_EventDescription_GetSampleLoadingState_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventDescription_GetSampleLoadingState_Delegate>(nameof(FMOD_Studio_EventDescription_GetSampleLoadingState));

        private delegate RESULT FMOD_Studio_EventDescription_ReleaseAllInstances_Delegate(IntPtr eventdescription);
        private static FMOD_Studio_EventDescription_ReleaseAllInstances_Delegate FMOD_Studio_EventDescription_ReleaseAllInstances_Internal = null;
        private static FMOD_Studio_EventDescription_ReleaseAllInstances_Delegate FMOD_Studio_EventDescription_ReleaseAllInstances => FMOD_Studio_EventDescription_ReleaseAllInstances_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventDescription_ReleaseAllInstances_Delegate>(nameof(FMOD_Studio_EventDescription_ReleaseAllInstances));

        private delegate RESULT FMOD_Studio_EventDescription_SetCallback_Delegate(IntPtr eventdescription, EVENT_CALLBACK callback, EVENT_CALLBACK_TYPE callbackmask);
        private static FMOD_Studio_EventDescription_SetCallback_Delegate FMOD_Studio_EventDescription_SetCallback_Internal = null;
        private static FMOD_Studio_EventDescription_SetCallback_Delegate FMOD_Studio_EventDescription_SetCallback => FMOD_Studio_EventDescription_SetCallback_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventDescription_SetCallback_Delegate>(nameof(FMOD_Studio_EventDescription_SetCallback));

        private delegate RESULT FMOD_Studio_EventDescription_GetUserData_Delegate(IntPtr eventdescription, out IntPtr userdata);
        private static FMOD_Studio_EventDescription_GetUserData_Delegate FMOD_Studio_EventDescription_GetUserData_Internal = null;
        private static FMOD_Studio_EventDescription_GetUserData_Delegate FMOD_Studio_EventDescription_GetUserData => FMOD_Studio_EventDescription_GetUserData_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventDescription_GetUserData_Delegate>(nameof(FMOD_Studio_EventDescription_GetUserData));

        private delegate RESULT FMOD_Studio_EventDescription_SetUserData_Delegate(IntPtr eventdescription, IntPtr userdata);
        private static FMOD_Studio_EventDescription_SetUserData_Delegate FMOD_Studio_EventDescription_SetUserData_Internal = null;
        private static FMOD_Studio_EventDescription_SetUserData_Delegate FMOD_Studio_EventDescription_SetUserData => FMOD_Studio_EventDescription_SetUserData_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventDescription_SetUserData_Delegate>(nameof(FMOD_Studio_EventDescription_SetUserData));

        #endregion
        #region wrapperinternal

        public IntPtr handle;

        public EventDescription(IntPtr ptr) { this.handle = ptr; }
        public bool hasHandle()             { return this.handle != IntPtr.Zero; }
        public void clearHandle()           { this.handle = IntPtr.Zero; }

        public bool isValid()
        {
            return hasHandle() && FMOD_Studio_EventDescription_IsValid(this.handle);
        }

        #endregion
    }

    public struct EventInstance
    {
        public RESULT getDescription(out EventDescription description)
        {
            return FMOD_Studio_EventInstance_GetDescription(this.handle, out description.handle);
        }
        public RESULT getVolume(out float volume)
        {
            return FMOD_Studio_EventInstance_GetVolume(this.handle, out volume, IntPtr.Zero);
        }
        public RESULT getVolume(out float volume, out float finalvolume)
        {
            return FMOD_Studio_EventInstance_GetVolume2(this.handle, out volume, out finalvolume);
        }
        public RESULT setVolume(float volume)
        {
            return FMOD_Studio_EventInstance_SetVolume(this.handle, volume);
        }
        public RESULT getPitch(out float pitch)
        {
            return FMOD_Studio_EventInstance_GetPitch(this.handle, out pitch, IntPtr.Zero);
        }
        public RESULT getPitch(out float pitch, out float finalpitch)
        {
            return FMOD_Studio_EventInstance_GetPitch2(this.handle, out pitch, out finalpitch);
        }
        public RESULT setPitch(float pitch)
        {
            return FMOD_Studio_EventInstance_SetPitch(this.handle, pitch);
        }
        public RESULT get3DAttributes(out ATTRIBUTES_3D attributes)
        {
            return FMOD_Studio_EventInstance_Get3DAttributes(this.handle, out attributes);
        }
        public RESULT set3DAttributes(ATTRIBUTES_3D attributes)
        {
            return FMOD_Studio_EventInstance_Set3DAttributes(this.handle, ref attributes);
        }
        public RESULT getListenerMask(out uint mask)
        {
            return FMOD_Studio_EventInstance_GetListenerMask(this.handle, out mask);
        }
        public RESULT setListenerMask(uint mask)
        {
            return FMOD_Studio_EventInstance_SetListenerMask(this.handle, mask);
        }
        public RESULT getProperty(EVENT_PROPERTY index, out float value)
        {
            return FMOD_Studio_EventInstance_GetProperty(this.handle, index, out value);
        }
        public RESULT setProperty(EVENT_PROPERTY index, float value)
        {
            return FMOD_Studio_EventInstance_SetProperty(this.handle, index, value);
        }
        public RESULT getReverbLevel(int index, out float level)
        {
            return FMOD_Studio_EventInstance_GetReverbLevel(this.handle, index, out level);
        }
        public RESULT setReverbLevel(int index, float level)
        {
            return FMOD_Studio_EventInstance_SetReverbLevel(this.handle, index, level);
        }
        public RESULT getPaused(out bool paused)
        {
            return FMOD_Studio_EventInstance_GetPaused(this.handle, out paused);
        }
        public RESULT setPaused(bool paused)
        {
            return FMOD_Studio_EventInstance_SetPaused(this.handle, paused);
        }
        public RESULT start()
        {
            return FMOD_Studio_EventInstance_Start(this.handle);
        }
        public RESULT stop(STOP_MODE mode)
        {
            return FMOD_Studio_EventInstance_Stop(this.handle, mode);
        }
        public RESULT getTimelinePosition(out int position)
        {
            return FMOD_Studio_EventInstance_GetTimelinePosition(this.handle, out position);
        }
        public RESULT setTimelinePosition(int position)
        {
            return FMOD_Studio_EventInstance_SetTimelinePosition(this.handle, position);
        }
        public RESULT getPlaybackState(out PLAYBACK_STATE state)
        {
            return FMOD_Studio_EventInstance_GetPlaybackState(this.handle, out state);
        }
        public RESULT getChannelGroup(out FMOD.ChannelGroup group)
        {
            return FMOD_Studio_EventInstance_GetChannelGroup(this.handle, out group.handle);
        }
        public RESULT getMinMaxDistance(out float min, out float max)
        {
            return FMOD_Studio_EventInstance_GetMinMaxDistance(this.handle, out min, out max);
        }
        public RESULT release()
        {
            return FMOD_Studio_EventInstance_Release(this.handle);
        }
        public RESULT isVirtual(out bool virtualstate)
        {
            return FMOD_Studio_EventInstance_IsVirtual(this.handle, out virtualstate);
        }
        public RESULT getParameterByID(PARAMETER_ID id, out float value)
        {
            float finalvalue;
            return getParameterByID(id, out value, out finalvalue);
        }
        public RESULT getParameterByID(PARAMETER_ID id, out float value, out float finalvalue)
        {
            return FMOD_Studio_EventInstance_GetParameterByID(this.handle, id, out value, out finalvalue);
        }
        public RESULT setParameterByID(PARAMETER_ID id, float value, bool ignoreseekspeed = false)
        {
            return FMOD_Studio_EventInstance_SetParameterByID(this.handle, id, value, ignoreseekspeed);
        }
        public RESULT setParameterByIDWithLabel(PARAMETER_ID id, string label, bool ignoreseekspeed = false)
        {
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                return FMOD_Studio_EventInstance_SetParameterByIDWithLabel(this.handle, id, encoder.byteFromStringUTF8(label), ignoreseekspeed);
            }
        }
        public RESULT setParametersByIDs(PARAMETER_ID[] ids, float[] values, int count, bool ignoreseekspeed = false)
        {
            return FMOD_Studio_EventInstance_SetParametersByIDs(this.handle, ids, values, count, ignoreseekspeed);
        }
        public RESULT getParameterByName(string name, out float value)
        {
            float finalValue;
            return getParameterByName(name, out value, out finalValue);
        }
        public RESULT getParameterByName(string name, out float value, out float finalvalue)
        {
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                return FMOD_Studio_EventInstance_GetParameterByName(this.handle, encoder.byteFromStringUTF8(name), out value, out finalvalue);
            }
        }
        public RESULT setParameterByName(string name, float value, bool ignoreseekspeed = false)
        {
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                return FMOD_Studio_EventInstance_SetParameterByName(this.handle, encoder.byteFromStringUTF8(name), value, ignoreseekspeed);
            }
        }
        public RESULT setParameterByNameWithLabel(string name, string label, bool ignoreseekspeed = false)
        {
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper(),
                                                   labelEncoder = StringHelper.GetFreeHelper())
            {
                return FMOD_Studio_EventInstance_SetParameterByNameWithLabel(this.handle, encoder.byteFromStringUTF8(name), labelEncoder.byteFromStringUTF8(label), ignoreseekspeed);
            }
        }
        public RESULT keyOff()
        {
            return FMOD_Studio_EventInstance_KeyOff(this.handle);
        }
        public RESULT setCallback(EVENT_CALLBACK callback, EVENT_CALLBACK_TYPE callbackmask = EVENT_CALLBACK_TYPE.ALL)
        {
            return FMOD_Studio_EventInstance_SetCallback(this.handle, callback, callbackmask);
        }
        public RESULT getUserData(out IntPtr userdata)
        {
            return FMOD_Studio_EventInstance_GetUserData(this.handle, out userdata);
        }
        public RESULT setUserData(IntPtr userdata)
        {
            return FMOD_Studio_EventInstance_SetUserData(this.handle, userdata);
        }
        public RESULT getCPUUsage(out uint exclusive, out uint inclusive)
        {
            return FMOD_Studio_EventInstance_GetCPUUsage(this.handle, out exclusive, out inclusive);
        }
        public RESULT getMemoryUsage(out MEMORY_USAGE memoryusage)
        {
            return FMOD_Studio_EventInstance_GetMemoryUsage(this.handle, out memoryusage);
        }
        #region importfunctions
        private delegate bool FMOD_Studio_EventInstance_IsValid_Delegate(IntPtr _event);
        private static FMOD_Studio_EventInstance_IsValid_Delegate FMOD_Studio_EventInstance_IsValid_Internal = null;
        private static FMOD_Studio_EventInstance_IsValid_Delegate FMOD_Studio_EventInstance_IsValid => FMOD_Studio_EventInstance_IsValid_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_IsValid_Delegate>(nameof(FMOD_Studio_EventInstance_IsValid));

        private delegate RESULT FMOD_Studio_EventInstance_GetDescription_Delegate(IntPtr _event, out IntPtr description);
        private static FMOD_Studio_EventInstance_GetDescription_Delegate FMOD_Studio_EventInstance_GetDescription_Internal = null;
        private static FMOD_Studio_EventInstance_GetDescription_Delegate FMOD_Studio_EventInstance_GetDescription => FMOD_Studio_EventInstance_GetDescription_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_GetDescription_Delegate>(nameof(FMOD_Studio_EventInstance_GetDescription));

        private delegate RESULT FMOD_Studio_EventInstance_GetVolume_Delegate(IntPtr _event, out float volume, IntPtr zero);
        private static FMOD_Studio_EventInstance_GetVolume_Delegate FMOD_Studio_EventInstance_GetVolume_Internal = null;
        private static FMOD_Studio_EventInstance_GetVolume_Delegate FMOD_Studio_EventInstance_GetVolume => FMOD_Studio_EventInstance_GetVolume_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_GetVolume_Delegate>(nameof(FMOD_Studio_EventInstance_GetVolume));

        private delegate RESULT FMOD_Studio_EventInstance_GetVolume_Delegate2(IntPtr _event, out float volume, out float finalvolume);
        private static FMOD_Studio_EventInstance_GetVolume_Delegate2 FMOD_Studio_EventInstance_GetVolume_Internal2 = null;
        private static FMOD_Studio_EventInstance_GetVolume_Delegate2 FMOD_Studio_EventInstance_GetVolume2 => FMOD_Studio_EventInstance_GetVolume_Internal2 ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_GetVolume_Delegate2>(nameof(FMOD_Studio_EventInstance_GetVolume));

        private delegate RESULT FMOD_Studio_EventInstance_SetVolume_Delegate(IntPtr _event, float volume);
        private static FMOD_Studio_EventInstance_SetVolume_Delegate FMOD_Studio_EventInstance_SetVolume_Internal = null;
        private static FMOD_Studio_EventInstance_SetVolume_Delegate FMOD_Studio_EventInstance_SetVolume => FMOD_Studio_EventInstance_SetVolume_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_SetVolume_Delegate>(nameof(FMOD_Studio_EventInstance_SetVolume));

        private delegate RESULT FMOD_Studio_EventInstance_GetPitch_Delegate(IntPtr _event, out float pitch, IntPtr zero);
        private static FMOD_Studio_EventInstance_GetPitch_Delegate FMOD_Studio_EventInstance_GetPitch_Internal = null;
        private static FMOD_Studio_EventInstance_GetPitch_Delegate FMOD_Studio_EventInstance_GetPitch => FMOD_Studio_EventInstance_GetPitch_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_GetPitch_Delegate>(nameof(FMOD_Studio_EventInstance_GetPitch));

        private delegate RESULT FMOD_Studio_EventInstance_GetPitch_Delegate2(IntPtr _event, out float pitch, out float finalpitch);
        private static FMOD_Studio_EventInstance_GetPitch_Delegate2 FMOD_Studio_EventInstance_GetPitch_Internal2 = null;
        private static FMOD_Studio_EventInstance_GetPitch_Delegate2 FMOD_Studio_EventInstance_GetPitch2 => FMOD_Studio_EventInstance_GetPitch_Internal2 ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_GetPitch_Delegate2>(nameof(FMOD_Studio_EventInstance_GetPitch));

        private delegate RESULT FMOD_Studio_EventInstance_SetPitch_Delegate(IntPtr _event, float pitch);
        private static FMOD_Studio_EventInstance_SetPitch_Delegate FMOD_Studio_EventInstance_SetPitch_Internal = null;
        private static FMOD_Studio_EventInstance_SetPitch_Delegate FMOD_Studio_EventInstance_SetPitch => FMOD_Studio_EventInstance_SetPitch_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_SetPitch_Delegate>(nameof(FMOD_Studio_EventInstance_SetPitch));

        private delegate RESULT FMOD_Studio_EventInstance_Get3DAttributes_Delegate(IntPtr _event, out ATTRIBUTES_3D attributes);
        private static FMOD_Studio_EventInstance_Get3DAttributes_Delegate FMOD_Studio_EventInstance_Get3DAttributes_Internal = null;
        private static FMOD_Studio_EventInstance_Get3DAttributes_Delegate FMOD_Studio_EventInstance_Get3DAttributes => FMOD_Studio_EventInstance_Get3DAttributes_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_Get3DAttributes_Delegate>(nameof(FMOD_Studio_EventInstance_Get3DAttributes));

        private delegate RESULT FMOD_Studio_EventInstance_Set3DAttributes_Delegate(IntPtr _event, ref ATTRIBUTES_3D attributes);
        private static FMOD_Studio_EventInstance_Set3DAttributes_Delegate FMOD_Studio_EventInstance_Set3DAttributes_Internal = null;
        private static FMOD_Studio_EventInstance_Set3DAttributes_Delegate FMOD_Studio_EventInstance_Set3DAttributes => FMOD_Studio_EventInstance_Set3DAttributes_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_Set3DAttributes_Delegate>(nameof(FMOD_Studio_EventInstance_Set3DAttributes));

        private delegate RESULT FMOD_Studio_EventInstance_GetListenerMask_Delegate(IntPtr _event, out uint mask);
        private static FMOD_Studio_EventInstance_GetListenerMask_Delegate FMOD_Studio_EventInstance_GetListenerMask_Internal = null;
        private static FMOD_Studio_EventInstance_GetListenerMask_Delegate FMOD_Studio_EventInstance_GetListenerMask => FMOD_Studio_EventInstance_GetListenerMask_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_GetListenerMask_Delegate>(nameof(FMOD_Studio_EventInstance_GetListenerMask));

        private delegate RESULT FMOD_Studio_EventInstance_SetListenerMask_Delegate(IntPtr _event, uint mask);
        private static FMOD_Studio_EventInstance_SetListenerMask_Delegate FMOD_Studio_EventInstance_SetListenerMask_Internal = null;
        private static FMOD_Studio_EventInstance_SetListenerMask_Delegate FMOD_Studio_EventInstance_SetListenerMask => FMOD_Studio_EventInstance_SetListenerMask_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_SetListenerMask_Delegate>(nameof(FMOD_Studio_EventInstance_SetListenerMask));

        private delegate RESULT FMOD_Studio_EventInstance_GetProperty_Delegate(IntPtr _event, EVENT_PROPERTY index, out float value);
        private static FMOD_Studio_EventInstance_GetProperty_Delegate FMOD_Studio_EventInstance_GetProperty_Internal = null;
        private static FMOD_Studio_EventInstance_GetProperty_Delegate FMOD_Studio_EventInstance_GetProperty => FMOD_Studio_EventInstance_GetProperty_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_GetProperty_Delegate>(nameof(FMOD_Studio_EventInstance_GetProperty));

        private delegate RESULT FMOD_Studio_EventInstance_SetProperty_Delegate(IntPtr _event, EVENT_PROPERTY index, float value);
        private static FMOD_Studio_EventInstance_SetProperty_Delegate FMOD_Studio_EventInstance_SetProperty_Internal = null;
        private static FMOD_Studio_EventInstance_SetProperty_Delegate FMOD_Studio_EventInstance_SetProperty => FMOD_Studio_EventInstance_SetProperty_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_SetProperty_Delegate>(nameof(FMOD_Studio_EventInstance_SetProperty));

        private delegate RESULT FMOD_Studio_EventInstance_GetReverbLevel_Delegate(IntPtr _event, int index, out float level);
        private static FMOD_Studio_EventInstance_GetReverbLevel_Delegate FMOD_Studio_EventInstance_GetReverbLevel_Internal = null;
        private static FMOD_Studio_EventInstance_GetReverbLevel_Delegate FMOD_Studio_EventInstance_GetReverbLevel => FMOD_Studio_EventInstance_GetReverbLevel_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_GetReverbLevel_Delegate>(nameof(FMOD_Studio_EventInstance_GetReverbLevel));

        private delegate RESULT FMOD_Studio_EventInstance_SetReverbLevel_Delegate(IntPtr _event, int index, float level);
        private static FMOD_Studio_EventInstance_SetReverbLevel_Delegate FMOD_Studio_EventInstance_SetReverbLevel_Internal = null;
        private static FMOD_Studio_EventInstance_SetReverbLevel_Delegate FMOD_Studio_EventInstance_SetReverbLevel => FMOD_Studio_EventInstance_SetReverbLevel_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_SetReverbLevel_Delegate>(nameof(FMOD_Studio_EventInstance_SetReverbLevel));

        private delegate RESULT FMOD_Studio_EventInstance_GetPaused_Delegate(IntPtr _event, out bool paused);
        private static FMOD_Studio_EventInstance_GetPaused_Delegate FMOD_Studio_EventInstance_GetPaused_Internal = null;
        private static FMOD_Studio_EventInstance_GetPaused_Delegate FMOD_Studio_EventInstance_GetPaused => FMOD_Studio_EventInstance_GetPaused_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_GetPaused_Delegate>(nameof(FMOD_Studio_EventInstance_GetPaused));

        private delegate RESULT FMOD_Studio_EventInstance_SetPaused_Delegate(IntPtr _event, bool paused);
        private static FMOD_Studio_EventInstance_SetPaused_Delegate FMOD_Studio_EventInstance_SetPaused_Internal = null;
        private static FMOD_Studio_EventInstance_SetPaused_Delegate FMOD_Studio_EventInstance_SetPaused => FMOD_Studio_EventInstance_SetPaused_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_SetPaused_Delegate>(nameof(FMOD_Studio_EventInstance_SetPaused));

        private delegate RESULT FMOD_Studio_EventInstance_Start_Delegate(IntPtr _event);
        private static FMOD_Studio_EventInstance_Start_Delegate FMOD_Studio_EventInstance_Start_Internal = null;
        private static FMOD_Studio_EventInstance_Start_Delegate FMOD_Studio_EventInstance_Start => FMOD_Studio_EventInstance_Start_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_Start_Delegate>(nameof(FMOD_Studio_EventInstance_Start));

        private delegate RESULT FMOD_Studio_EventInstance_Stop_Delegate(IntPtr _event, STOP_MODE mode);
        private static FMOD_Studio_EventInstance_Stop_Delegate FMOD_Studio_EventInstance_Stop_Internal = null;
        private static FMOD_Studio_EventInstance_Stop_Delegate FMOD_Studio_EventInstance_Stop => FMOD_Studio_EventInstance_Stop_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_Stop_Delegate>(nameof(FMOD_Studio_EventInstance_Stop));

        private delegate RESULT FMOD_Studio_EventInstance_GetTimelinePosition_Delegate(IntPtr _event, out int position);
        private static FMOD_Studio_EventInstance_GetTimelinePosition_Delegate FMOD_Studio_EventInstance_GetTimelinePosition_Internal = null;
        private static FMOD_Studio_EventInstance_GetTimelinePosition_Delegate FMOD_Studio_EventInstance_GetTimelinePosition => FMOD_Studio_EventInstance_GetTimelinePosition_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_GetTimelinePosition_Delegate>(nameof(FMOD_Studio_EventInstance_GetTimelinePosition));

        private delegate RESULT FMOD_Studio_EventInstance_SetTimelinePosition_Delegate(IntPtr _event, int position);
        private static FMOD_Studio_EventInstance_SetTimelinePosition_Delegate FMOD_Studio_EventInstance_SetTimelinePosition_Internal = null;
        private static FMOD_Studio_EventInstance_SetTimelinePosition_Delegate FMOD_Studio_EventInstance_SetTimelinePosition => FMOD_Studio_EventInstance_SetTimelinePosition_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_SetTimelinePosition_Delegate>(nameof(FMOD_Studio_EventInstance_SetTimelinePosition));

        private delegate RESULT FMOD_Studio_EventInstance_GetPlaybackState_Delegate(IntPtr _event, out PLAYBACK_STATE state);
        private static FMOD_Studio_EventInstance_GetPlaybackState_Delegate FMOD_Studio_EventInstance_GetPlaybackState_Internal = null;
        private static FMOD_Studio_EventInstance_GetPlaybackState_Delegate FMOD_Studio_EventInstance_GetPlaybackState => FMOD_Studio_EventInstance_GetPlaybackState_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_GetPlaybackState_Delegate>(nameof(FMOD_Studio_EventInstance_GetPlaybackState));

        private delegate RESULT FMOD_Studio_EventInstance_GetChannelGroup_Delegate(IntPtr _event, out IntPtr group);
        private static FMOD_Studio_EventInstance_GetChannelGroup_Delegate FMOD_Studio_EventInstance_GetChannelGroup_Internal = null;
        private static FMOD_Studio_EventInstance_GetChannelGroup_Delegate FMOD_Studio_EventInstance_GetChannelGroup => FMOD_Studio_EventInstance_GetChannelGroup_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_GetChannelGroup_Delegate>(nameof(FMOD_Studio_EventInstance_GetChannelGroup));

        private delegate RESULT FMOD_Studio_EventInstance_GetMinMaxDistance_Delegate(IntPtr _event, out float min, out float max);
        private static FMOD_Studio_EventInstance_GetMinMaxDistance_Delegate FMOD_Studio_EventInstance_GetMinMaxDistance_Internal = null;
        private static FMOD_Studio_EventInstance_GetMinMaxDistance_Delegate FMOD_Studio_EventInstance_GetMinMaxDistance => FMOD_Studio_EventInstance_GetMinMaxDistance_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_GetMinMaxDistance_Delegate>(nameof(FMOD_Studio_EventInstance_GetMinMaxDistance));

        private delegate RESULT FMOD_Studio_EventInstance_Release_Delegate(IntPtr _event);
        private static FMOD_Studio_EventInstance_Release_Delegate FMOD_Studio_EventInstance_Release_Internal = null;
        private static FMOD_Studio_EventInstance_Release_Delegate FMOD_Studio_EventInstance_Release => FMOD_Studio_EventInstance_Release_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_Release_Delegate>(nameof(FMOD_Studio_EventInstance_Release));

        private delegate RESULT FMOD_Studio_EventInstance_IsVirtual_Delegate(IntPtr _event, out bool virtualstate);
        private static FMOD_Studio_EventInstance_IsVirtual_Delegate FMOD_Studio_EventInstance_IsVirtual_Internal = null;
        private static FMOD_Studio_EventInstance_IsVirtual_Delegate FMOD_Studio_EventInstance_IsVirtual => FMOD_Studio_EventInstance_IsVirtual_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_IsVirtual_Delegate>(nameof(FMOD_Studio_EventInstance_IsVirtual));

        private delegate RESULT FMOD_Studio_EventInstance_GetParameterByName_Delegate(IntPtr _event, byte[] name, out float value, out float finalvalue);
        private static FMOD_Studio_EventInstance_GetParameterByName_Delegate FMOD_Studio_EventInstance_GetParameterByName_Internal = null;
        private static FMOD_Studio_EventInstance_GetParameterByName_Delegate FMOD_Studio_EventInstance_GetParameterByName => FMOD_Studio_EventInstance_GetParameterByName_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_GetParameterByName_Delegate>(nameof(FMOD_Studio_EventInstance_GetParameterByName));

        private delegate RESULT FMOD_Studio_EventInstance_SetParameterByName_Delegate(IntPtr _event, byte[] name, float value, bool ignoreseekspeed);
        private static FMOD_Studio_EventInstance_SetParameterByName_Delegate FMOD_Studio_EventInstance_SetParameterByName_Internal = null;
        private static FMOD_Studio_EventInstance_SetParameterByName_Delegate FMOD_Studio_EventInstance_SetParameterByName => FMOD_Studio_EventInstance_SetParameterByName_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_SetParameterByName_Delegate>(nameof(FMOD_Studio_EventInstance_SetParameterByName));

        private delegate RESULT FMOD_Studio_EventInstance_SetParameterByNameWithLabel_Delegate(IntPtr _event, byte[] name, byte[] label, bool ignoreseekspeed);
        private static FMOD_Studio_EventInstance_SetParameterByNameWithLabel_Delegate FMOD_Studio_EventInstance_SetParameterByNameWithLabel_Internal = null;
        private static FMOD_Studio_EventInstance_SetParameterByNameWithLabel_Delegate FMOD_Studio_EventInstance_SetParameterByNameWithLabel => FMOD_Studio_EventInstance_SetParameterByNameWithLabel_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_SetParameterByNameWithLabel_Delegate>(nameof(FMOD_Studio_EventInstance_SetParameterByNameWithLabel));

        private delegate RESULT FMOD_Studio_EventInstance_GetParameterByID_Delegate(IntPtr _event, PARAMETER_ID id, out float value, out float finalvalue);
        private static FMOD_Studio_EventInstance_GetParameterByID_Delegate FMOD_Studio_EventInstance_GetParameterByID_Internal = null;
        private static FMOD_Studio_EventInstance_GetParameterByID_Delegate FMOD_Studio_EventInstance_GetParameterByID => FMOD_Studio_EventInstance_GetParameterByID_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_GetParameterByID_Delegate>(nameof(FMOD_Studio_EventInstance_GetParameterByID));

        private delegate RESULT FMOD_Studio_EventInstance_SetParameterByID_Delegate(IntPtr _event, PARAMETER_ID id, float value, bool ignoreseekspeed);
        private static FMOD_Studio_EventInstance_SetParameterByID_Delegate FMOD_Studio_EventInstance_SetParameterByID_Internal = null;
        private static FMOD_Studio_EventInstance_SetParameterByID_Delegate FMOD_Studio_EventInstance_SetParameterByID => FMOD_Studio_EventInstance_SetParameterByID_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_SetParameterByID_Delegate>(nameof(FMOD_Studio_EventInstance_SetParameterByID));

        private delegate RESULT FMOD_Studio_EventInstance_SetParameterByIDWithLabel_Delegate(IntPtr _event, PARAMETER_ID id, byte[] label, bool ignoreseekspeed);
        private static FMOD_Studio_EventInstance_SetParameterByIDWithLabel_Delegate FMOD_Studio_EventInstance_SetParameterByIDWithLabel_Internal = null;
        private static FMOD_Studio_EventInstance_SetParameterByIDWithLabel_Delegate FMOD_Studio_EventInstance_SetParameterByIDWithLabel => FMOD_Studio_EventInstance_SetParameterByIDWithLabel_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_SetParameterByIDWithLabel_Delegate>(nameof(FMOD_Studio_EventInstance_SetParameterByIDWithLabel));

        private delegate RESULT FMOD_Studio_EventInstance_SetParametersByIDs_Delegate(IntPtr _event, PARAMETER_ID[] ids, float[] values, int count, bool ignoreseekspeed);
        private static FMOD_Studio_EventInstance_SetParametersByIDs_Delegate FMOD_Studio_EventInstance_SetParametersByIDs_Internal = null;
        private static FMOD_Studio_EventInstance_SetParametersByIDs_Delegate FMOD_Studio_EventInstance_SetParametersByIDs => FMOD_Studio_EventInstance_SetParametersByIDs_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_SetParametersByIDs_Delegate>(nameof(FMOD_Studio_EventInstance_SetParametersByIDs));

        private delegate RESULT FMOD_Studio_EventInstance_KeyOff_Delegate(IntPtr _event);
        private static FMOD_Studio_EventInstance_KeyOff_Delegate FMOD_Studio_EventInstance_KeyOff_Internal = null;
        private static FMOD_Studio_EventInstance_KeyOff_Delegate FMOD_Studio_EventInstance_KeyOff => FMOD_Studio_EventInstance_KeyOff_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_KeyOff_Delegate>(nameof(FMOD_Studio_EventInstance_KeyOff));

        private delegate RESULT FMOD_Studio_EventInstance_SetCallback_Delegate(IntPtr _event, EVENT_CALLBACK callback, EVENT_CALLBACK_TYPE callbackmask);
        private static FMOD_Studio_EventInstance_SetCallback_Delegate FMOD_Studio_EventInstance_SetCallback_Internal = null;
        private static FMOD_Studio_EventInstance_SetCallback_Delegate FMOD_Studio_EventInstance_SetCallback => FMOD_Studio_EventInstance_SetCallback_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_SetCallback_Delegate>(nameof(FMOD_Studio_EventInstance_SetCallback));

        private delegate RESULT FMOD_Studio_EventInstance_GetUserData_Delegate(IntPtr _event, out IntPtr userdata);
        private static FMOD_Studio_EventInstance_GetUserData_Delegate FMOD_Studio_EventInstance_GetUserData_Internal = null;
        private static FMOD_Studio_EventInstance_GetUserData_Delegate FMOD_Studio_EventInstance_GetUserData => FMOD_Studio_EventInstance_GetUserData_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_GetUserData_Delegate>(nameof(FMOD_Studio_EventInstance_GetUserData));

        private delegate RESULT FMOD_Studio_EventInstance_SetUserData_Delegate(IntPtr _event, IntPtr userdata);
        private static FMOD_Studio_EventInstance_SetUserData_Delegate FMOD_Studio_EventInstance_SetUserData_Internal = null;
        private static FMOD_Studio_EventInstance_SetUserData_Delegate FMOD_Studio_EventInstance_SetUserData => FMOD_Studio_EventInstance_SetUserData_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_SetUserData_Delegate>(nameof(FMOD_Studio_EventInstance_SetUserData));

        private delegate RESULT FMOD_Studio_EventInstance_GetCPUUsage_Delegate(IntPtr _event, out uint exclusive, out uint inclusive);
        private static FMOD_Studio_EventInstance_GetCPUUsage_Delegate FMOD_Studio_EventInstance_GetCPUUsage_Internal = null;
        private static FMOD_Studio_EventInstance_GetCPUUsage_Delegate FMOD_Studio_EventInstance_GetCPUUsage => FMOD_Studio_EventInstance_GetCPUUsage_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_GetCPUUsage_Delegate>(nameof(FMOD_Studio_EventInstance_GetCPUUsage));

        private delegate RESULT FMOD_Studio_EventInstance_GetMemoryUsage_Delegate(IntPtr _event, out MEMORY_USAGE memoryusage);
        private static FMOD_Studio_EventInstance_GetMemoryUsage_Delegate FMOD_Studio_EventInstance_GetMemoryUsage_Internal = null;
        private static FMOD_Studio_EventInstance_GetMemoryUsage_Delegate FMOD_Studio_EventInstance_GetMemoryUsage => FMOD_Studio_EventInstance_GetMemoryUsage_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_EventInstance_GetMemoryUsage_Delegate>(nameof(FMOD_Studio_EventInstance_GetMemoryUsage));

        #endregion

        #region wrapperinternal

        public IntPtr handle;

        public EventInstance(IntPtr ptr) { this.handle = ptr; }
        public bool hasHandle()          { return this.handle != IntPtr.Zero; }
        public void clearHandle()        { this.handle = IntPtr.Zero; }

        public bool isValid()
        {
            return hasHandle() && FMOD_Studio_EventInstance_IsValid(this.handle);
        }

        #endregion
    }

    public struct Bus
    {
        public RESULT getID(out GUID id)
        {
            return FMOD_Studio_Bus_GetID(this.handle, out id);
        }
        public RESULT getPath(out string path)
        {
            path = null;

            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                IntPtr stringMem = Marshal.AllocHGlobal(256);
                int retrieved = 0;
                RESULT result = FMOD_Studio_Bus_GetPath(this.handle, stringMem, 256, out retrieved);

                if (result == RESULT.ERR_TRUNCATED)
                {
                    Marshal.FreeHGlobal(stringMem);
                    stringMem = Marshal.AllocHGlobal(retrieved);
                    result = FMOD_Studio_Bus_GetPath(this.handle, stringMem, retrieved, out retrieved);
                }

                if (result == RESULT.OK)
                {
                    path = encoder.stringFromNative(stringMem);
                }
                Marshal.FreeHGlobal(stringMem);
                return result;
            }

        }
        public RESULT getVolume(out float volume)
        {
            float finalVolume;
            return getVolume(out volume, out finalVolume);
        }
        public RESULT getVolume(out float volume, out float finalvolume)
        {
            return FMOD_Studio_Bus_GetVolume(this.handle, out volume, out finalvolume);
        }
        public RESULT setVolume(float volume)
        {
            return FMOD_Studio_Bus_SetVolume(this.handle, volume);
        }
        public RESULT getPaused(out bool paused)
        {
            return FMOD_Studio_Bus_GetPaused(this.handle, out paused);
        }
        public RESULT setPaused(bool paused)
        {
            return FMOD_Studio_Bus_SetPaused(this.handle, paused);
        }
        public RESULT getMute(out bool mute)
        {
            return FMOD_Studio_Bus_GetMute(this.handle, out mute);
        }
        public RESULT setMute(bool mute)
        {
            return FMOD_Studio_Bus_SetMute(this.handle, mute);
        }
        public RESULT stopAllEvents(STOP_MODE mode)
        {
            return FMOD_Studio_Bus_StopAllEvents(this.handle, mode);
        }
        public RESULT lockChannelGroup()
        {
            return FMOD_Studio_Bus_LockChannelGroup(this.handle);
        }
        public RESULT unlockChannelGroup()
        {
            return FMOD_Studio_Bus_UnlockChannelGroup(this.handle);
        }
        public RESULT getChannelGroup(out FMOD.ChannelGroup group)
        {
            return FMOD_Studio_Bus_GetChannelGroup(this.handle, out group.handle);
        }
        public RESULT getCPUUsage(out uint exclusive, out uint inclusive)
        {
            return FMOD_Studio_Bus_GetCPUUsage(this.handle, out exclusive, out inclusive);
        }
        public RESULT getMemoryUsage(out MEMORY_USAGE memoryusage)
        {
            return FMOD_Studio_Bus_GetMemoryUsage(this.handle, out memoryusage);
        }
        public RESULT getPortIndex(out ulong index)
        {
            return FMOD_Studio_Bus_GetPortIndex(this.handle, out index);
        }
        public RESULT setPortIndex(ulong index)
        {
            return FMOD_Studio_Bus_SetPortIndex(this.handle, index);
        }

        #region importfunctions
        private delegate bool FMOD_Studio_Bus_IsValid_Delegate(IntPtr bus);
        private static FMOD_Studio_Bus_IsValid_Delegate FMOD_Studio_Bus_IsValid_Internal = null;
        private static FMOD_Studio_Bus_IsValid_Delegate FMOD_Studio_Bus_IsValid => FMOD_Studio_Bus_IsValid_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_Bus_IsValid_Delegate>(nameof(FMOD_Studio_Bus_IsValid));

        private delegate RESULT FMOD_Studio_Bus_GetID_Delegate(IntPtr bus, out GUID id);
        private static FMOD_Studio_Bus_GetID_Delegate FMOD_Studio_Bus_GetID_Internal = null;
        private static FMOD_Studio_Bus_GetID_Delegate FMOD_Studio_Bus_GetID => FMOD_Studio_Bus_GetID_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_Bus_GetID_Delegate>(nameof(FMOD_Studio_Bus_GetID));

        private delegate RESULT FMOD_Studio_Bus_GetPath_Delegate(IntPtr bus, IntPtr path, int size, out int retrieved);
        private static FMOD_Studio_Bus_GetPath_Delegate FMOD_Studio_Bus_GetPath_Internal = null;
        private static FMOD_Studio_Bus_GetPath_Delegate FMOD_Studio_Bus_GetPath => FMOD_Studio_Bus_GetPath_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_Bus_GetPath_Delegate>(nameof(FMOD_Studio_Bus_GetPath));

        private delegate RESULT FMOD_Studio_Bus_GetVolume_Delegate(IntPtr bus, out float volume, out float finalvolume);
        private static FMOD_Studio_Bus_GetVolume_Delegate FMOD_Studio_Bus_GetVolume_Internal = null;
        private static FMOD_Studio_Bus_GetVolume_Delegate FMOD_Studio_Bus_GetVolume => FMOD_Studio_Bus_GetVolume_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_Bus_GetVolume_Delegate>(nameof(FMOD_Studio_Bus_GetVolume));

        private delegate RESULT FMOD_Studio_Bus_SetVolume_Delegate(IntPtr bus, float volume);
        private static FMOD_Studio_Bus_SetVolume_Delegate FMOD_Studio_Bus_SetVolume_Internal = null;
        private static FMOD_Studio_Bus_SetVolume_Delegate FMOD_Studio_Bus_SetVolume => FMOD_Studio_Bus_SetVolume_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_Bus_SetVolume_Delegate>(nameof(FMOD_Studio_Bus_SetVolume));

        private delegate RESULT FMOD_Studio_Bus_GetPaused_Delegate(IntPtr bus, out bool paused);
        private static FMOD_Studio_Bus_GetPaused_Delegate FMOD_Studio_Bus_GetPaused_Internal = null;
        private static FMOD_Studio_Bus_GetPaused_Delegate FMOD_Studio_Bus_GetPaused => FMOD_Studio_Bus_GetPaused_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_Bus_GetPaused_Delegate>(nameof(FMOD_Studio_Bus_GetPaused));

        private delegate RESULT FMOD_Studio_Bus_SetPaused_Delegate(IntPtr bus, bool paused);
        private static FMOD_Studio_Bus_SetPaused_Delegate FMOD_Studio_Bus_SetPaused_Internal = null;
        private static FMOD_Studio_Bus_SetPaused_Delegate FMOD_Studio_Bus_SetPaused => FMOD_Studio_Bus_SetPaused_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_Bus_SetPaused_Delegate>(nameof(FMOD_Studio_Bus_SetPaused));

        private delegate RESULT FMOD_Studio_Bus_GetMute_Delegate(IntPtr bus, out bool mute);
        private static FMOD_Studio_Bus_GetMute_Delegate FMOD_Studio_Bus_GetMute_Internal = null;
        private static FMOD_Studio_Bus_GetMute_Delegate FMOD_Studio_Bus_GetMute => FMOD_Studio_Bus_GetMute_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_Bus_GetMute_Delegate>(nameof(FMOD_Studio_Bus_GetMute));

        private delegate RESULT FMOD_Studio_Bus_SetMute_Delegate(IntPtr bus, bool mute);
        private static FMOD_Studio_Bus_SetMute_Delegate FMOD_Studio_Bus_SetMute_Internal = null;
        private static FMOD_Studio_Bus_SetMute_Delegate FMOD_Studio_Bus_SetMute => FMOD_Studio_Bus_SetMute_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_Bus_SetMute_Delegate>(nameof(FMOD_Studio_Bus_SetMute));

        private delegate RESULT FMOD_Studio_Bus_StopAllEvents_Delegate(IntPtr bus, STOP_MODE mode);
        private static FMOD_Studio_Bus_StopAllEvents_Delegate FMOD_Studio_Bus_StopAllEvents_Internal = null;
        private static FMOD_Studio_Bus_StopAllEvents_Delegate FMOD_Studio_Bus_StopAllEvents => FMOD_Studio_Bus_StopAllEvents_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_Bus_StopAllEvents_Delegate>(nameof(FMOD_Studio_Bus_StopAllEvents));

        private delegate RESULT FMOD_Studio_Bus_LockChannelGroup_Delegate(IntPtr bus);
        private static FMOD_Studio_Bus_LockChannelGroup_Delegate FMOD_Studio_Bus_LockChannelGroup_Internal = null;
        private static FMOD_Studio_Bus_LockChannelGroup_Delegate FMOD_Studio_Bus_LockChannelGroup => FMOD_Studio_Bus_LockChannelGroup_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_Bus_LockChannelGroup_Delegate>(nameof(FMOD_Studio_Bus_LockChannelGroup));

        private delegate RESULT FMOD_Studio_Bus_UnlockChannelGroup_Delegate(IntPtr bus);
        private static FMOD_Studio_Bus_UnlockChannelGroup_Delegate FMOD_Studio_Bus_UnlockChannelGroup_Internal = null;
        private static FMOD_Studio_Bus_UnlockChannelGroup_Delegate FMOD_Studio_Bus_UnlockChannelGroup => FMOD_Studio_Bus_UnlockChannelGroup_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_Bus_UnlockChannelGroup_Delegate>(nameof(FMOD_Studio_Bus_UnlockChannelGroup));

        private delegate RESULT FMOD_Studio_Bus_GetChannelGroup_Delegate(IntPtr bus, out IntPtr group);
        private static FMOD_Studio_Bus_GetChannelGroup_Delegate FMOD_Studio_Bus_GetChannelGroup_Internal = null;
        private static FMOD_Studio_Bus_GetChannelGroup_Delegate FMOD_Studio_Bus_GetChannelGroup => FMOD_Studio_Bus_GetChannelGroup_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_Bus_GetChannelGroup_Delegate>(nameof(FMOD_Studio_Bus_GetChannelGroup));

        private delegate RESULT FMOD_Studio_Bus_GetCPUUsage_Delegate(IntPtr bus, out uint exclusive, out uint inclusive);
        private static FMOD_Studio_Bus_GetCPUUsage_Delegate FMOD_Studio_Bus_GetCPUUsage_Internal = null;
        private static FMOD_Studio_Bus_GetCPUUsage_Delegate FMOD_Studio_Bus_GetCPUUsage => FMOD_Studio_Bus_GetCPUUsage_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_Bus_GetCPUUsage_Delegate>(nameof(FMOD_Studio_Bus_GetCPUUsage));

        private delegate RESULT FMOD_Studio_Bus_GetMemoryUsage_Delegate(IntPtr bus, out MEMORY_USAGE memoryusage);
        private static FMOD_Studio_Bus_GetMemoryUsage_Delegate FMOD_Studio_Bus_GetMemoryUsage_Internal = null;
        private static FMOD_Studio_Bus_GetMemoryUsage_Delegate FMOD_Studio_Bus_GetMemoryUsage => FMOD_Studio_Bus_GetMemoryUsage_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_Bus_GetMemoryUsage_Delegate>(nameof(FMOD_Studio_Bus_GetMemoryUsage));

        private delegate RESULT FMOD_Studio_Bus_GetPortIndex_Delegate(IntPtr bus, out ulong index);
        private static FMOD_Studio_Bus_GetPortIndex_Delegate FMOD_Studio_Bus_GetPortIndex_Internal = null;
        private static FMOD_Studio_Bus_GetPortIndex_Delegate FMOD_Studio_Bus_GetPortIndex => FMOD_Studio_Bus_GetPortIndex_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_Bus_GetPortIndex_Delegate>(nameof(FMOD_Studio_Bus_GetPortIndex));

        private delegate RESULT FMOD_Studio_Bus_SetPortIndex_Delegate(IntPtr bus, ulong index);
        private static FMOD_Studio_Bus_SetPortIndex_Delegate FMOD_Studio_Bus_SetPortIndex_Internal = null;
        private static FMOD_Studio_Bus_SetPortIndex_Delegate FMOD_Studio_Bus_SetPortIndex => FMOD_Studio_Bus_SetPortIndex_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_Bus_SetPortIndex_Delegate>(nameof(FMOD_Studio_Bus_SetPortIndex));

        #endregion

        #region wrapperinternal

        public IntPtr handle;

        public Bus(IntPtr ptr)      { this.handle = ptr; }
        public bool hasHandle()     { return this.handle != IntPtr.Zero; }
        public void clearHandle()   { this.handle = IntPtr.Zero; }

        public bool isValid()
        {
            return hasHandle() && FMOD_Studio_Bus_IsValid(this.handle);
        }

        #endregion
    }

    public struct VCA
    {
        public RESULT getID(out GUID id)
        {
            return FMOD_Studio_VCA_GetID(this.handle, out id);
        }
        public RESULT getPath(out string path)
        {
            path = null;

            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                IntPtr stringMem = Marshal.AllocHGlobal(256);
                int retrieved = 0;
                RESULT result = FMOD_Studio_VCA_GetPath(this.handle, stringMem, 256, out retrieved);

                if (result == RESULT.ERR_TRUNCATED)
                {
                    Marshal.FreeHGlobal(stringMem);
                    stringMem = Marshal.AllocHGlobal(retrieved);
                    result = FMOD_Studio_VCA_GetPath(this.handle, stringMem, retrieved, out retrieved);
                }

                if (result == RESULT.OK)
                {
                    path = encoder.stringFromNative(stringMem);
                }
                Marshal.FreeHGlobal(stringMem);
                return result;
            }
        }
        public RESULT getVolume(out float volume)
        {
            float finalVolume;
            return getVolume(out volume, out finalVolume);
        }
        public RESULT getVolume(out float volume, out float finalvolume)
        {
            return FMOD_Studio_VCA_GetVolume(this.handle, out volume, out finalvolume);
        }
        public RESULT setVolume(float volume)
        {
            return FMOD_Studio_VCA_SetVolume(this.handle, volume);
        }

        #region importfunctions
        private delegate bool FMOD_Studio_VCA_IsValid_Delegate(IntPtr vca);
        private static FMOD_Studio_VCA_IsValid_Delegate FMOD_Studio_VCA_IsValid_Internal = null;
        private static FMOD_Studio_VCA_IsValid_Delegate FMOD_Studio_VCA_IsValid => FMOD_Studio_VCA_IsValid_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_VCA_IsValid_Delegate>(nameof(FMOD_Studio_VCA_IsValid));

        private delegate RESULT FMOD_Studio_VCA_GetID_Delegate(IntPtr vca, out GUID id);
        private static FMOD_Studio_VCA_GetID_Delegate FMOD_Studio_VCA_GetID_Internal = null;
        private static FMOD_Studio_VCA_GetID_Delegate FMOD_Studio_VCA_GetID => FMOD_Studio_VCA_GetID_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_VCA_GetID_Delegate>(nameof(FMOD_Studio_VCA_GetID));

        private delegate RESULT FMOD_Studio_VCA_GetPath_Delegate(IntPtr vca, IntPtr path, int size, out int retrieved);
        private static FMOD_Studio_VCA_GetPath_Delegate FMOD_Studio_VCA_GetPath_Internal = null;
        private static FMOD_Studio_VCA_GetPath_Delegate FMOD_Studio_VCA_GetPath => FMOD_Studio_VCA_GetPath_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_VCA_GetPath_Delegate>(nameof(FMOD_Studio_VCA_GetPath));

        private delegate RESULT FMOD_Studio_VCA_GetVolume_Delegate(IntPtr vca, out float volume, out float finalvolume);
        private static FMOD_Studio_VCA_GetVolume_Delegate FMOD_Studio_VCA_GetVolume_Internal = null;
        private static FMOD_Studio_VCA_GetVolume_Delegate FMOD_Studio_VCA_GetVolume => FMOD_Studio_VCA_GetVolume_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_VCA_GetVolume_Delegate>(nameof(FMOD_Studio_VCA_GetVolume));

        private delegate RESULT FMOD_Studio_VCA_SetVolume_Delegate(IntPtr vca, float volume);
        private static FMOD_Studio_VCA_SetVolume_Delegate FMOD_Studio_VCA_SetVolume_Internal = null;
        private static FMOD_Studio_VCA_SetVolume_Delegate FMOD_Studio_VCA_SetVolume => FMOD_Studio_VCA_SetVolume_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_VCA_SetVolume_Delegate>(nameof(FMOD_Studio_VCA_SetVolume));

        #endregion

        #region wrapperinternal

        public IntPtr handle;

        public VCA(IntPtr ptr)      { this.handle = ptr; }
        public bool hasHandle()     { return this.handle != IntPtr.Zero; }
        public void clearHandle()   { this.handle = IntPtr.Zero; }

        public bool isValid()
        {
            return hasHandle() && FMOD_Studio_VCA_IsValid(this.handle);
        }

        #endregion
    }

    public struct Bank
    {
        // Property access

        public RESULT getID(out GUID id)
        {
            return FMOD_Studio_Bank_GetID(this.handle, out id);
        }
        public RESULT getPath(out string path)
        {
            path = null;

            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                IntPtr stringMem = Marshal.AllocHGlobal(256);
                int retrieved = 0;
                RESULT result = FMOD_Studio_Bank_GetPath(this.handle, stringMem, 256, out retrieved);

                if (result == RESULT.ERR_TRUNCATED)
                {
                    Marshal.FreeHGlobal(stringMem);
                    stringMem = Marshal.AllocHGlobal(retrieved);
                    result = FMOD_Studio_Bank_GetPath(this.handle, stringMem, retrieved, out retrieved);
                }

                if (result == RESULT.OK)
                {
                    path = encoder.stringFromNative(stringMem);
                }
                Marshal.FreeHGlobal(stringMem);
                return result;
            }
        }
        public RESULT unload()
        {
            return FMOD_Studio_Bank_Unload(this.handle);
        }
        public RESULT loadSampleData()
        {
            return FMOD_Studio_Bank_LoadSampleData(this.handle);
        }
        public RESULT unloadSampleData()
        {
            return FMOD_Studio_Bank_UnloadSampleData(this.handle);
        }
        public RESULT getLoadingState(out LOADING_STATE state)
        {
            return FMOD_Studio_Bank_GetLoadingState(this.handle, out state);
        }
        public RESULT getSampleLoadingState(out LOADING_STATE state)
        {
            return FMOD_Studio_Bank_GetSampleLoadingState(this.handle, out state);
        }

        // Enumeration
        public RESULT getStringCount(out int count)
        {
            return FMOD_Studio_Bank_GetStringCount(this.handle, out count);
        }
        public RESULT getStringInfo(int index, out GUID id, out string path)
        {
            path = null;
            id = new GUID();

            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                IntPtr stringMem = Marshal.AllocHGlobal(256);
                int retrieved = 0;
                RESULT result = FMOD_Studio_Bank_GetStringInfo(this.handle, index, out id, stringMem, 256, out retrieved);

                if (result == RESULT.ERR_TRUNCATED)
                {
                    Marshal.FreeHGlobal(stringMem);
                    stringMem = Marshal.AllocHGlobal(retrieved);
                    result = FMOD_Studio_Bank_GetStringInfo(this.handle, index, out id, stringMem, retrieved, out retrieved);
                }

                if (result == RESULT.OK)
                {
                    path = encoder.stringFromNative(stringMem);
                }
                Marshal.FreeHGlobal(stringMem);
                return result;
            }
        }

        public RESULT getEventCount(out int count)
        {
            return FMOD_Studio_Bank_GetEventCount(this.handle, out count);
        }
        public RESULT getEventList(out EventDescription[] array)
        {
            array = null;

            RESULT result;
            int capacity;
            result = FMOD_Studio_Bank_GetEventCount(this.handle, out capacity);
            if (result != RESULT.OK)
            {
                return result;
            }
            if (capacity == 0)
            {
                array = new EventDescription[0];
                return result;
            }

            IntPtr[] rawArray = new IntPtr[capacity];
            int actualCount;
            result = FMOD_Studio_Bank_GetEventList(this.handle, rawArray, capacity, out actualCount);
            if (result != RESULT.OK)
            {
                return result;
            }
            if (actualCount > capacity) // More items added since we queried just now?
            {
                actualCount = capacity;
            }
            array = new EventDescription[actualCount];
            for (int i = 0; i < actualCount; ++i)
            {
                array[i].handle = rawArray[i];
            }
            return RESULT.OK;
        }
        public RESULT getBusCount(out int count)
        {
            return FMOD_Studio_Bank_GetBusCount(this.handle, out count);
        }
        public RESULT getBusList(out Bus[] array)
        {
            array = null;

            RESULT result;
            int capacity;
            result = FMOD_Studio_Bank_GetBusCount(this.handle, out capacity);
            if (result != RESULT.OK)
            {
                return result;
            }
            if (capacity == 0)
            {
                array = new Bus[0];
                return result;
            }

            IntPtr[] rawArray = new IntPtr[capacity];
            int actualCount;
            result = FMOD_Studio_Bank_GetBusList(this.handle, rawArray, capacity, out actualCount);
            if (result != RESULT.OK)
            {
                return result;
            }
            if (actualCount > capacity) // More items added since we queried just now?
            {
                actualCount = capacity;
            }
            array = new Bus[actualCount];
            for (int i = 0; i < actualCount; ++i)
            {
                array[i].handle = rawArray[i];
            }
            return RESULT.OK;
        }
        public RESULT getVCACount(out int count)
        {
            return FMOD_Studio_Bank_GetVCACount(this.handle, out count);
        }
        public RESULT getVCAList(out VCA[] array)
        {
            array = null;

            RESULT result;
            int capacity;
            result = FMOD_Studio_Bank_GetVCACount(this.handle, out capacity);
            if (result != RESULT.OK)
            {
                return result;
            }
            if (capacity == 0)
            {
                array = new VCA[0];
                return result;
            }

            IntPtr[] rawArray = new IntPtr[capacity];
            int actualCount;
            result = FMOD_Studio_Bank_GetVCAList(this.handle, rawArray, capacity, out actualCount);
            if (result != RESULT.OK)
            {
                return result;
            }
            if (actualCount > capacity) // More items added since we queried just now?
            {
                actualCount = capacity;
            }
            array = new VCA[actualCount];
            for (int i = 0; i < actualCount; ++i)
            {
                array[i].handle = rawArray[i];
            }
            return RESULT.OK;
        }

        public RESULT getUserData(out IntPtr userdata)
        {
            return FMOD_Studio_Bank_GetUserData(this.handle, out userdata);
        }

        public RESULT setUserData(IntPtr userdata)
        {
            return FMOD_Studio_Bank_SetUserData(this.handle, userdata);
        }

        #region importfunctions
        private delegate bool FMOD_Studio_Bank_IsValid_Delegate(IntPtr bank);
        private static FMOD_Studio_Bank_IsValid_Delegate FMOD_Studio_Bank_IsValid_Internal = null;
        private static FMOD_Studio_Bank_IsValid_Delegate FMOD_Studio_Bank_IsValid => FMOD_Studio_Bank_IsValid_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_Bank_IsValid_Delegate>(nameof(FMOD_Studio_Bank_IsValid));

        private delegate RESULT FMOD_Studio_Bank_GetID_Delegate(IntPtr bank, out GUID id);
        private static FMOD_Studio_Bank_GetID_Delegate FMOD_Studio_Bank_GetID_Internal = null;
        private static FMOD_Studio_Bank_GetID_Delegate FMOD_Studio_Bank_GetID => FMOD_Studio_Bank_GetID_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_Bank_GetID_Delegate>(nameof(FMOD_Studio_Bank_GetID));

        private delegate RESULT FMOD_Studio_Bank_GetPath_Delegate(IntPtr bank, IntPtr path, int size, out int retrieved);
        private static FMOD_Studio_Bank_GetPath_Delegate FMOD_Studio_Bank_GetPath_Internal = null;
        private static FMOD_Studio_Bank_GetPath_Delegate FMOD_Studio_Bank_GetPath => FMOD_Studio_Bank_GetPath_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_Bank_GetPath_Delegate>(nameof(FMOD_Studio_Bank_GetPath));

        private delegate RESULT FMOD_Studio_Bank_Unload_Delegate(IntPtr bank);
        private static FMOD_Studio_Bank_Unload_Delegate FMOD_Studio_Bank_Unload_Internal = null;
        private static FMOD_Studio_Bank_Unload_Delegate FMOD_Studio_Bank_Unload => FMOD_Studio_Bank_Unload_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_Bank_Unload_Delegate>(nameof(FMOD_Studio_Bank_Unload));

        private delegate RESULT FMOD_Studio_Bank_LoadSampleData_Delegate(IntPtr bank);
        private static FMOD_Studio_Bank_LoadSampleData_Delegate FMOD_Studio_Bank_LoadSampleData_Internal = null;
        private static FMOD_Studio_Bank_LoadSampleData_Delegate FMOD_Studio_Bank_LoadSampleData => FMOD_Studio_Bank_LoadSampleData_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_Bank_LoadSampleData_Delegate>(nameof(FMOD_Studio_Bank_LoadSampleData));

        private delegate RESULT FMOD_Studio_Bank_UnloadSampleData_Delegate(IntPtr bank);
        private static FMOD_Studio_Bank_UnloadSampleData_Delegate FMOD_Studio_Bank_UnloadSampleData_Internal = null;
        private static FMOD_Studio_Bank_UnloadSampleData_Delegate FMOD_Studio_Bank_UnloadSampleData => FMOD_Studio_Bank_UnloadSampleData_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_Bank_UnloadSampleData_Delegate>(nameof(FMOD_Studio_Bank_UnloadSampleData));

        private delegate RESULT FMOD_Studio_Bank_GetLoadingState_Delegate(IntPtr bank, out LOADING_STATE state);
        private static FMOD_Studio_Bank_GetLoadingState_Delegate FMOD_Studio_Bank_GetLoadingState_Internal = null;
        private static FMOD_Studio_Bank_GetLoadingState_Delegate FMOD_Studio_Bank_GetLoadingState => FMOD_Studio_Bank_GetLoadingState_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_Bank_GetLoadingState_Delegate>(nameof(FMOD_Studio_Bank_GetLoadingState));

        private delegate RESULT FMOD_Studio_Bank_GetSampleLoadingState_Delegate(IntPtr bank, out LOADING_STATE state);
        private static FMOD_Studio_Bank_GetSampleLoadingState_Delegate FMOD_Studio_Bank_GetSampleLoadingState_Internal = null;
        private static FMOD_Studio_Bank_GetSampleLoadingState_Delegate FMOD_Studio_Bank_GetSampleLoadingState => FMOD_Studio_Bank_GetSampleLoadingState_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_Bank_GetSampleLoadingState_Delegate>(nameof(FMOD_Studio_Bank_GetSampleLoadingState));

        private delegate RESULT FMOD_Studio_Bank_GetStringCount_Delegate(IntPtr bank, out int count);
        private static FMOD_Studio_Bank_GetStringCount_Delegate FMOD_Studio_Bank_GetStringCount_Internal = null;
        private static FMOD_Studio_Bank_GetStringCount_Delegate FMOD_Studio_Bank_GetStringCount => FMOD_Studio_Bank_GetStringCount_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_Bank_GetStringCount_Delegate>(nameof(FMOD_Studio_Bank_GetStringCount));

        private delegate RESULT FMOD_Studio_Bank_GetStringInfo_Delegate(IntPtr bank, int index, out GUID id, IntPtr path, int size, out int retrieved);
        private static FMOD_Studio_Bank_GetStringInfo_Delegate FMOD_Studio_Bank_GetStringInfo_Internal = null;
        private static FMOD_Studio_Bank_GetStringInfo_Delegate FMOD_Studio_Bank_GetStringInfo => FMOD_Studio_Bank_GetStringInfo_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_Bank_GetStringInfo_Delegate>(nameof(FMOD_Studio_Bank_GetStringInfo));

        private delegate RESULT FMOD_Studio_Bank_GetEventCount_Delegate(IntPtr bank, out int count);
        private static FMOD_Studio_Bank_GetEventCount_Delegate FMOD_Studio_Bank_GetEventCount_Internal = null;
        private static FMOD_Studio_Bank_GetEventCount_Delegate FMOD_Studio_Bank_GetEventCount => FMOD_Studio_Bank_GetEventCount_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_Bank_GetEventCount_Delegate>(nameof(FMOD_Studio_Bank_GetEventCount));

        private delegate RESULT FMOD_Studio_Bank_GetEventList_Delegate(IntPtr bank, IntPtr[] array, int capacity, out int count);
        private static FMOD_Studio_Bank_GetEventList_Delegate FMOD_Studio_Bank_GetEventList_Internal = null;
        private static FMOD_Studio_Bank_GetEventList_Delegate FMOD_Studio_Bank_GetEventList => FMOD_Studio_Bank_GetEventList_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_Bank_GetEventList_Delegate>(nameof(FMOD_Studio_Bank_GetEventList));

        private delegate RESULT FMOD_Studio_Bank_GetBusCount_Delegate(IntPtr bank, out int count);
        private static FMOD_Studio_Bank_GetBusCount_Delegate FMOD_Studio_Bank_GetBusCount_Internal = null;
        private static FMOD_Studio_Bank_GetBusCount_Delegate FMOD_Studio_Bank_GetBusCount => FMOD_Studio_Bank_GetBusCount_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_Bank_GetBusCount_Delegate>(nameof(FMOD_Studio_Bank_GetBusCount));

        private delegate RESULT FMOD_Studio_Bank_GetBusList_Delegate(IntPtr bank, IntPtr[] array, int capacity, out int count);
        private static FMOD_Studio_Bank_GetBusList_Delegate FMOD_Studio_Bank_GetBusList_Internal = null;
        private static FMOD_Studio_Bank_GetBusList_Delegate FMOD_Studio_Bank_GetBusList => FMOD_Studio_Bank_GetBusList_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_Bank_GetBusList_Delegate>(nameof(FMOD_Studio_Bank_GetBusList));

        private delegate RESULT FMOD_Studio_Bank_GetVCACount_Delegate(IntPtr bank, out int count);
        private static FMOD_Studio_Bank_GetVCACount_Delegate FMOD_Studio_Bank_GetVCACount_Internal = null;
        private static FMOD_Studio_Bank_GetVCACount_Delegate FMOD_Studio_Bank_GetVCACount => FMOD_Studio_Bank_GetVCACount_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_Bank_GetVCACount_Delegate>(nameof(FMOD_Studio_Bank_GetVCACount));

        private delegate RESULT FMOD_Studio_Bank_GetVCAList_Delegate(IntPtr bank, IntPtr[] array, int capacity, out int count);
        private static FMOD_Studio_Bank_GetVCAList_Delegate FMOD_Studio_Bank_GetVCAList_Internal = null;
        private static FMOD_Studio_Bank_GetVCAList_Delegate FMOD_Studio_Bank_GetVCAList => FMOD_Studio_Bank_GetVCAList_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_Bank_GetVCAList_Delegate>(nameof(FMOD_Studio_Bank_GetVCAList));

        private delegate RESULT FMOD_Studio_Bank_GetUserData_Delegate(IntPtr bank, out IntPtr userdata);
        private static FMOD_Studio_Bank_GetUserData_Delegate FMOD_Studio_Bank_GetUserData_Internal = null;
        private static FMOD_Studio_Bank_GetUserData_Delegate FMOD_Studio_Bank_GetUserData => FMOD_Studio_Bank_GetUserData_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_Bank_GetUserData_Delegate>(nameof(FMOD_Studio_Bank_GetUserData));

        private delegate RESULT FMOD_Studio_Bank_SetUserData_Delegate(IntPtr bank, IntPtr userdata);
        private static FMOD_Studio_Bank_SetUserData_Delegate FMOD_Studio_Bank_SetUserData_Internal = null;
        private static FMOD_Studio_Bank_SetUserData_Delegate FMOD_Studio_Bank_SetUserData => FMOD_Studio_Bank_SetUserData_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_Bank_SetUserData_Delegate>(nameof(FMOD_Studio_Bank_SetUserData));

        #endregion

        #region wrapperinternal

        public IntPtr handle;

        public Bank(IntPtr ptr)     { this.handle = ptr; }
        public bool hasHandle()     { return this.handle != IntPtr.Zero; }
        public void clearHandle()   { this.handle = IntPtr.Zero; }

        public bool isValid()
        {
            return hasHandle() && FMOD_Studio_Bank_IsValid(this.handle);
        }

        #endregion
    }

    public struct CommandReplay
    {
        // Information query
        public RESULT getSystem(out System system)
        {
            return FMOD_Studio_CommandReplay_GetSystem(this.handle, out system.handle);
        }

        public RESULT getLength(out float length)
        {
            return FMOD_Studio_CommandReplay_GetLength(this.handle, out length);
        }
        public RESULT getCommandCount(out int count)
        {
            return FMOD_Studio_CommandReplay_GetCommandCount(this.handle, out count);
        }
        public RESULT getCommandInfo(int commandIndex, out COMMAND_INFO info)
        {
            return FMOD_Studio_CommandReplay_GetCommandInfo(this.handle, commandIndex, out info);
        }

        public RESULT getCommandString(int commandIndex, out string buffer)
        {
            buffer = null;
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                int stringLength = 256;
                IntPtr stringMem = Marshal.AllocHGlobal(256);
                RESULT result = FMOD_Studio_CommandReplay_GetCommandString(this.handle, commandIndex, stringMem, stringLength);

                while (result == RESULT.ERR_TRUNCATED)
                {
                    Marshal.FreeHGlobal(stringMem);
                    stringLength *= 2;
                    stringMem = Marshal.AllocHGlobal(stringLength);
                    result = FMOD_Studio_CommandReplay_GetCommandString(this.handle, commandIndex, stringMem, stringLength);
                }

                if (result == RESULT.OK)
                {
                    buffer = encoder.stringFromNative(stringMem);
                }
                Marshal.FreeHGlobal(stringMem);
                return result;
            }
        }
        public RESULT getCommandAtTime(float time, out int commandIndex)
        {
            return FMOD_Studio_CommandReplay_GetCommandAtTime(this.handle, time, out commandIndex);
        }
        // Playback
        public RESULT setBankPath(string bankPath)
        {
            using (StringHelper.ThreadSafeEncoding encoder = StringHelper.GetFreeHelper())
            {
                return FMOD_Studio_CommandReplay_SetBankPath(this.handle, encoder.byteFromStringUTF8(bankPath));
            }
        }
        public RESULT start()
        {
            return FMOD_Studio_CommandReplay_Start(this.handle);
        }
        public RESULT stop()
        {
            return FMOD_Studio_CommandReplay_Stop(this.handle);
        }
        public RESULT seekToTime(float time)
        {
            return FMOD_Studio_CommandReplay_SeekToTime(this.handle, time);
        }
        public RESULT seekToCommand(int commandIndex)
        {
            return FMOD_Studio_CommandReplay_SeekToCommand(this.handle, commandIndex);
        }
        public RESULT getPaused(out bool paused)
        {
            return FMOD_Studio_CommandReplay_GetPaused(this.handle, out paused);
        }
        public RESULT setPaused(bool paused)
        {
            return FMOD_Studio_CommandReplay_SetPaused(this.handle, paused);
        }
        public RESULT getPlaybackState(out PLAYBACK_STATE state)
        {
            return FMOD_Studio_CommandReplay_GetPlaybackState(this.handle, out state);
        }
        public RESULT getCurrentCommand(out int commandIndex, out float currentTime)
        {
            return FMOD_Studio_CommandReplay_GetCurrentCommand(this.handle, out commandIndex, out currentTime);
        }
        // Release
        public RESULT release()
        {
            return FMOD_Studio_CommandReplay_Release(this.handle);
        }
        // Callbacks
        public RESULT setFrameCallback(COMMANDREPLAY_FRAME_CALLBACK callback)
        {
            return FMOD_Studio_CommandReplay_SetFrameCallback(this.handle, callback);
        }
        public RESULT setLoadBankCallback(COMMANDREPLAY_LOAD_BANK_CALLBACK callback)
        {
            return FMOD_Studio_CommandReplay_SetLoadBankCallback(this.handle, callback);
        }
        public RESULT setCreateInstanceCallback(COMMANDREPLAY_CREATE_INSTANCE_CALLBACK callback)
        {
            return FMOD_Studio_CommandReplay_SetCreateInstanceCallback(this.handle, callback);
        }
        public RESULT getUserData(out IntPtr userdata)
        {
            return FMOD_Studio_CommandReplay_GetUserData(this.handle, out userdata);
        }
        public RESULT setUserData(IntPtr userdata)
        {
            return FMOD_Studio_CommandReplay_SetUserData(this.handle, userdata);
        }

        #region importfunctions
        private delegate bool FMOD_Studio_CommandReplay_IsValid_Delegate(IntPtr replay);
        private static FMOD_Studio_CommandReplay_IsValid_Delegate FMOD_Studio_CommandReplay_IsValid_Internal = null;
        private static FMOD_Studio_CommandReplay_IsValid_Delegate FMOD_Studio_CommandReplay_IsValid => FMOD_Studio_CommandReplay_IsValid_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_CommandReplay_IsValid_Delegate>(nameof(FMOD_Studio_CommandReplay_IsValid));

        private delegate RESULT FMOD_Studio_CommandReplay_GetSystem_Delegate(IntPtr replay, out IntPtr system);
        private static FMOD_Studio_CommandReplay_GetSystem_Delegate FMOD_Studio_CommandReplay_GetSystem_Internal = null;
        private static FMOD_Studio_CommandReplay_GetSystem_Delegate FMOD_Studio_CommandReplay_GetSystem => FMOD_Studio_CommandReplay_GetSystem_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_CommandReplay_GetSystem_Delegate>(nameof(FMOD_Studio_CommandReplay_GetSystem));

        private delegate RESULT FMOD_Studio_CommandReplay_GetLength_Delegate(IntPtr replay, out float length);
        private static FMOD_Studio_CommandReplay_GetLength_Delegate FMOD_Studio_CommandReplay_GetLength_Internal = null;
        private static FMOD_Studio_CommandReplay_GetLength_Delegate FMOD_Studio_CommandReplay_GetLength => FMOD_Studio_CommandReplay_GetLength_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_CommandReplay_GetLength_Delegate>(nameof(FMOD_Studio_CommandReplay_GetLength));

        private delegate RESULT FMOD_Studio_CommandReplay_GetCommandCount_Delegate(IntPtr replay, out int count);
        private static FMOD_Studio_CommandReplay_GetCommandCount_Delegate FMOD_Studio_CommandReplay_GetCommandCount_Internal = null;
        private static FMOD_Studio_CommandReplay_GetCommandCount_Delegate FMOD_Studio_CommandReplay_GetCommandCount => FMOD_Studio_CommandReplay_GetCommandCount_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_CommandReplay_GetCommandCount_Delegate>(nameof(FMOD_Studio_CommandReplay_GetCommandCount));

        private delegate RESULT FMOD_Studio_CommandReplay_GetCommandInfo_Delegate(IntPtr replay, int commandindex, out COMMAND_INFO info);
        private static FMOD_Studio_CommandReplay_GetCommandInfo_Delegate FMOD_Studio_CommandReplay_GetCommandInfo_Internal = null;
        private static FMOD_Studio_CommandReplay_GetCommandInfo_Delegate FMOD_Studio_CommandReplay_GetCommandInfo => FMOD_Studio_CommandReplay_GetCommandInfo_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_CommandReplay_GetCommandInfo_Delegate>(nameof(FMOD_Studio_CommandReplay_GetCommandInfo));

        private delegate RESULT FMOD_Studio_CommandReplay_GetCommandString_Delegate(IntPtr replay, int commandIndex, IntPtr buffer, int length);
        private static FMOD_Studio_CommandReplay_GetCommandString_Delegate FMOD_Studio_CommandReplay_GetCommandString_Internal = null;
        private static FMOD_Studio_CommandReplay_GetCommandString_Delegate FMOD_Studio_CommandReplay_GetCommandString => FMOD_Studio_CommandReplay_GetCommandString_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_CommandReplay_GetCommandString_Delegate>(nameof(FMOD_Studio_CommandReplay_GetCommandString));

        private delegate RESULT FMOD_Studio_CommandReplay_GetCommandAtTime_Delegate(IntPtr replay, float time, out int commandIndex);
        private static FMOD_Studio_CommandReplay_GetCommandAtTime_Delegate FMOD_Studio_CommandReplay_GetCommandAtTime_Internal = null;
        private static FMOD_Studio_CommandReplay_GetCommandAtTime_Delegate FMOD_Studio_CommandReplay_GetCommandAtTime => FMOD_Studio_CommandReplay_GetCommandAtTime_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_CommandReplay_GetCommandAtTime_Delegate>(nameof(FMOD_Studio_CommandReplay_GetCommandAtTime));

        private delegate RESULT FMOD_Studio_CommandReplay_SetBankPath_Delegate(IntPtr replay, byte[] bankPath);
        private static FMOD_Studio_CommandReplay_SetBankPath_Delegate FMOD_Studio_CommandReplay_SetBankPath_Internal = null;
        private static FMOD_Studio_CommandReplay_SetBankPath_Delegate FMOD_Studio_CommandReplay_SetBankPath => FMOD_Studio_CommandReplay_SetBankPath_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_CommandReplay_SetBankPath_Delegate>(nameof(FMOD_Studio_CommandReplay_SetBankPath));

        private delegate RESULT FMOD_Studio_CommandReplay_Start_Delegate(IntPtr replay);
        private static FMOD_Studio_CommandReplay_Start_Delegate FMOD_Studio_CommandReplay_Start_Internal = null;
        private static FMOD_Studio_CommandReplay_Start_Delegate FMOD_Studio_CommandReplay_Start => FMOD_Studio_CommandReplay_Start_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_CommandReplay_Start_Delegate>(nameof(FMOD_Studio_CommandReplay_Start));

        private delegate RESULT FMOD_Studio_CommandReplay_Stop_Delegate(IntPtr replay);
        private static FMOD_Studio_CommandReplay_Stop_Delegate FMOD_Studio_CommandReplay_Stop_Internal = null;
        private static FMOD_Studio_CommandReplay_Stop_Delegate FMOD_Studio_CommandReplay_Stop => FMOD_Studio_CommandReplay_Stop_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_CommandReplay_Stop_Delegate>(nameof(FMOD_Studio_CommandReplay_Stop));

        private delegate RESULT FMOD_Studio_CommandReplay_SeekToTime_Delegate(IntPtr replay, float time);
        private static FMOD_Studio_CommandReplay_SeekToTime_Delegate FMOD_Studio_CommandReplay_SeekToTime_Internal = null;
        private static FMOD_Studio_CommandReplay_SeekToTime_Delegate FMOD_Studio_CommandReplay_SeekToTime => FMOD_Studio_CommandReplay_SeekToTime_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_CommandReplay_SeekToTime_Delegate>(nameof(FMOD_Studio_CommandReplay_SeekToTime));

        private delegate RESULT FMOD_Studio_CommandReplay_SeekToCommand_Delegate(IntPtr replay, int commandIndex);
        private static FMOD_Studio_CommandReplay_SeekToCommand_Delegate FMOD_Studio_CommandReplay_SeekToCommand_Internal = null;
        private static FMOD_Studio_CommandReplay_SeekToCommand_Delegate FMOD_Studio_CommandReplay_SeekToCommand => FMOD_Studio_CommandReplay_SeekToCommand_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_CommandReplay_SeekToCommand_Delegate>(nameof(FMOD_Studio_CommandReplay_SeekToCommand));

        private delegate RESULT FMOD_Studio_CommandReplay_GetPaused_Delegate(IntPtr replay, out bool paused);
        private static FMOD_Studio_CommandReplay_GetPaused_Delegate FMOD_Studio_CommandReplay_GetPaused_Internal = null;
        private static FMOD_Studio_CommandReplay_GetPaused_Delegate FMOD_Studio_CommandReplay_GetPaused => FMOD_Studio_CommandReplay_GetPaused_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_CommandReplay_GetPaused_Delegate>(nameof(FMOD_Studio_CommandReplay_GetPaused));

        private delegate RESULT FMOD_Studio_CommandReplay_SetPaused_Delegate(IntPtr replay, bool paused);
        private static FMOD_Studio_CommandReplay_SetPaused_Delegate FMOD_Studio_CommandReplay_SetPaused_Internal = null;
        private static FMOD_Studio_CommandReplay_SetPaused_Delegate FMOD_Studio_CommandReplay_SetPaused => FMOD_Studio_CommandReplay_SetPaused_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_CommandReplay_SetPaused_Delegate>(nameof(FMOD_Studio_CommandReplay_SetPaused));

        private delegate RESULT FMOD_Studio_CommandReplay_GetPlaybackState_Delegate(IntPtr replay, out PLAYBACK_STATE state);
        private static FMOD_Studio_CommandReplay_GetPlaybackState_Delegate FMOD_Studio_CommandReplay_GetPlaybackState_Internal = null;
        private static FMOD_Studio_CommandReplay_GetPlaybackState_Delegate FMOD_Studio_CommandReplay_GetPlaybackState => FMOD_Studio_CommandReplay_GetPlaybackState_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_CommandReplay_GetPlaybackState_Delegate>(nameof(FMOD_Studio_CommandReplay_GetPlaybackState));

        private delegate RESULT FMOD_Studio_CommandReplay_GetCurrentCommand_Delegate(IntPtr replay, out int commandIndex, out float currentTime);
        private static FMOD_Studio_CommandReplay_GetCurrentCommand_Delegate FMOD_Studio_CommandReplay_GetCurrentCommand_Internal = null;
        private static FMOD_Studio_CommandReplay_GetCurrentCommand_Delegate FMOD_Studio_CommandReplay_GetCurrentCommand => FMOD_Studio_CommandReplay_GetCurrentCommand_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_CommandReplay_GetCurrentCommand_Delegate>(nameof(FMOD_Studio_CommandReplay_GetCurrentCommand));

        private delegate RESULT FMOD_Studio_CommandReplay_Release_Delegate(IntPtr replay);
        private static FMOD_Studio_CommandReplay_Release_Delegate FMOD_Studio_CommandReplay_Release_Internal = null;
        private static FMOD_Studio_CommandReplay_Release_Delegate FMOD_Studio_CommandReplay_Release => FMOD_Studio_CommandReplay_Release_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_CommandReplay_Release_Delegate>(nameof(FMOD_Studio_CommandReplay_Release));

        private delegate RESULT FMOD_Studio_CommandReplay_SetFrameCallback_Delegate(IntPtr replay, COMMANDREPLAY_FRAME_CALLBACK callback);
        private static FMOD_Studio_CommandReplay_SetFrameCallback_Delegate FMOD_Studio_CommandReplay_SetFrameCallback_Internal = null;
        private static FMOD_Studio_CommandReplay_SetFrameCallback_Delegate FMOD_Studio_CommandReplay_SetFrameCallback => FMOD_Studio_CommandReplay_SetFrameCallback_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_CommandReplay_SetFrameCallback_Delegate>(nameof(FMOD_Studio_CommandReplay_SetFrameCallback));

        private delegate RESULT FMOD_Studio_CommandReplay_SetLoadBankCallback_Delegate(IntPtr replay, COMMANDREPLAY_LOAD_BANK_CALLBACK callback);
        private static FMOD_Studio_CommandReplay_SetLoadBankCallback_Delegate FMOD_Studio_CommandReplay_SetLoadBankCallback_Internal = null;
        private static FMOD_Studio_CommandReplay_SetLoadBankCallback_Delegate FMOD_Studio_CommandReplay_SetLoadBankCallback => FMOD_Studio_CommandReplay_SetLoadBankCallback_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_CommandReplay_SetLoadBankCallback_Delegate>(nameof(FMOD_Studio_CommandReplay_SetLoadBankCallback));

        private delegate RESULT FMOD_Studio_CommandReplay_SetCreateInstanceCallback_Delegate(IntPtr replay, COMMANDREPLAY_CREATE_INSTANCE_CALLBACK callback);
        private static FMOD_Studio_CommandReplay_SetCreateInstanceCallback_Delegate FMOD_Studio_CommandReplay_SetCreateInstanceCallback_Internal = null;
        private static FMOD_Studio_CommandReplay_SetCreateInstanceCallback_Delegate FMOD_Studio_CommandReplay_SetCreateInstanceCallback => FMOD_Studio_CommandReplay_SetCreateInstanceCallback_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_CommandReplay_SetCreateInstanceCallback_Delegate>(nameof(FMOD_Studio_CommandReplay_SetCreateInstanceCallback));

        private delegate RESULT FMOD_Studio_CommandReplay_GetUserData_Delegate(IntPtr replay, out IntPtr userdata);
        private static FMOD_Studio_CommandReplay_GetUserData_Delegate FMOD_Studio_CommandReplay_GetUserData_Internal = null;
        private static FMOD_Studio_CommandReplay_GetUserData_Delegate FMOD_Studio_CommandReplay_GetUserData => FMOD_Studio_CommandReplay_GetUserData_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_CommandReplay_GetUserData_Delegate>(nameof(FMOD_Studio_CommandReplay_GetUserData));

        private delegate RESULT FMOD_Studio_CommandReplay_SetUserData_Delegate(IntPtr replay, IntPtr userdata);
        private static FMOD_Studio_CommandReplay_SetUserData_Delegate FMOD_Studio_CommandReplay_SetUserData_Internal = null;
        private static FMOD_Studio_CommandReplay_SetUserData_Delegate FMOD_Studio_CommandReplay_SetUserData => FMOD_Studio_CommandReplay_SetUserData_Internal ??= FModManager.GetProcInFModStudio<FMOD_Studio_CommandReplay_SetUserData_Delegate>(nameof(FMOD_Studio_CommandReplay_SetUserData));

        #endregion

        #region wrapperinternal

        public IntPtr handle;

        public CommandReplay(IntPtr ptr) { this.handle = ptr; }
        public bool hasHandle()          { return this.handle != IntPtr.Zero; }
        public void clearHandle()        { this.handle = IntPtr.Zero; }

        public bool isValid()
        {
            return hasHandle() && FMOD_Studio_CommandReplay_IsValid(this.handle);
        }

        #endregion
    }
} // FMOD
