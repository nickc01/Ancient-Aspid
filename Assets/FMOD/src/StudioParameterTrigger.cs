using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;

namespace FMODUnity
{
    [Serializable]
    public class EmitterRef
    {
        public StudioEventEmitter Target;
        public ParamRef[] Params;
    }

    [AddComponentMenu("FMOD Studio/FMOD Studio Parameter Trigger")]
    public class StudioParameterTrigger: EventHandler, ISerializationCallbackReceiver
    {
        public EmitterRef[] Emitters;
        public EmitterGameEvent TriggerEvent;

        private void Awake()
        {
            for (int i = 0; i < Emitters.Length; i++)
            {
                var emitterRef = Emitters[i];
                if (emitterRef.Target != null && !emitterRef.Target.EventReference.IsNull)
                {
                    FMOD.Studio.EventDescription eventDesc = RuntimeManager.GetEventDescription(emitterRef.Target.EventReference);
                    if (eventDesc.isValid())
                    {
                        for (int j = 0; j < Emitters[i].Params.Length; j++)
                        {
                            FMOD.Studio.PARAMETER_DESCRIPTION param;
                            eventDesc.getParameterDescriptionByName(emitterRef.Params[j].Name, out param);
                            emitterRef.Params[j].ID = param.id;
                        }
                    }
                }
            }
        }

        protected override void HandleGameEvent(EmitterGameEvent gameEvent)
        {
            if (TriggerEvent == gameEvent)
            {
                TriggerParameters();
            }
        }

        public void TriggerParameters()
        {
            for (int i = 0; i < Emitters.Length; i++)
            {
                var emitterRef = Emitters[i];
                if (emitterRef.Target != null && emitterRef.Target.EventInstance.isValid())
                {
                    for (int j = 0; j < Emitters[i].Params.Length; j++)
                    {
                        emitterRef.Target.EventInstance.setParameterByID(Emitters[i].Params[j].ID, Emitters[i].Params[j].Value);
                    }
                }
            }
        }

        [SerializeField]
        [JsonIgnore]
        string stored_data;

        [SerializeField]
        [JsonIgnore]
        List<UnityEngine.Object> stored_objects = new List<UnityEngine.Object>();

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            WeaverSerializer.Serialize(Emitters, out stored_data, out stored_objects);
#endif
        }

        public void OnAfterDeserialize()
        {
#if !UNITY_EDITOR
            Emitters = WeaverSerializer.Deserialize<EmitterRef[]>(stored_data, stored_objects);
#endif
        }
    }
}
