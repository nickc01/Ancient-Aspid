using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using WeaverCore;
using Newtonsoft.Json.Linq;
using WeaverCore.Utilities;
using WeaverCore.Components;

public class ShotgunLaserManager : MonoBehaviour, ISerializationCallbackReceiver
{
    [NonSerialized]
    AncientAspid _boss;

    [SerializeField]
    List<Transform> laserOrigins;

    [Header("Other Settings")]
    public List<Sprite> head_Sprites;

    public List<bool> head_HorizFlip;

    public List<float> head_Degrees;

    [NonSerialized]
    List<LaserEmitter> _laserEmitters = null;

    public List<Transform> Lasers => laserOrigins;

    public List<LaserEmitter> LaserEmitters
    {
        get
        {
            if (_laserEmitters == null)
            {
                _laserEmitters = new List<LaserEmitter>(laserOrigins.Select(o => o.transform.parent.GetComponent<LaserEmitter>()));
            }
            return _laserEmitters;
        }
    }

    public int LaserCount => laserOrigins.Count;

    public Transform MiddleLaser => Lasers[2];
    public LaserEmitter MiddleEmitter => LaserEmitters[2];

    public AncientAspid Boss => _boss ??= GetComponentInParent<AncientAspid>();

    [Serializable]
    struct JsonContainer
    {
        public List<ShotgunLaserLocation> LaserLocations;
    }

    [Serializable]
    public struct LaserPosAndRot
    {
        public Vector3 Position;
        public Vector3 Rotation;
    }

    [Serializable]
    public class ShotgunLaserLocation
    {
        public string Name;
        public List<Sprite> correlatedHeadSprites;
        public LaserPosAndRot Left2Transform;
        public LaserPosAndRot Left1Transform;
        public LaserPosAndRot CenterTransform;
        public LaserPosAndRot Right1Transform;
        public LaserPosAndRot Right2Transform;
    }

    [SerializeField]
    public List<ShotgunLaserLocation> LaserLocations = new List<ShotgunLaserLocation>();

    [SerializeField]
    [HideInInspector]
    string laserLocations_data;

    [SerializeField]
    [HideInInspector]
    List<UnityEngine.Object> laserLocations_objects;

    //[NonSerialized]
    //uint continouslyUpdatingRoutine;

    float maxEmitterAngle;

    public float MaxHeadAngle => maxEmitterAngle;

    /*[SerializeField]
    List<string> spriteIDs = new List<string>();

    [SerializeField]
    List<Sprite> sprites = new List<Sprite>();*/

    //TODO - SWITCH THIS TO DECONTRUCTING EACH INDIVIDUAL FIELD, RATHER THAN USING JSON


    public bool ContinouslyUpdating => laserRotationGetter != null;


#if UNITY_EDITOR
    [SerializeField]
    bool ADD_NEW_ELEMENT = false;

    [SerializeField]
    string NAME_TO_RESTORE = "";

    [SerializeField]
    bool RESTORE_ELEMENT = false;
#endif

    private void Awake()
    {
        maxEmitterAngle = head_Degrees[head_Degrees.Count - 1];
    }

    public void SetHeadSpriteToRotation(float zDegrees, float divisor = 90f) => SetHeadSpriteToRotation(Quaternion.Euler(0f, 0f, zDegrees), divisor);

    public void SetHeadSpriteToRotation(Quaternion rotation, float divisor = 90f)
    {
        var (main, extra) = CalculateLaserRotation(rotation, divisor);

        //WeaverLog.Log("MAIN ANGLE = " + main);
        var spriteIndex = GetHeadIndexForAngle(main);

        //WeaverLog.Log("HEAD SPRITE INDEX = " + spriteIndex);

        Boss.Head.MainRenderer.sprite = head_Sprites[spriteIndex];
        Boss.Head.MainRenderer.flipX = head_HorizFlip[spriteIndex];
    }

    (float main, float extra) CalculateLaserRotation(Quaternion rotation, float divisor = 90f)
    {
        rotation *= Quaternion.Euler(0f, 0f, 90f);

        var angle = MathUtilities.ClampRotation(rotation.eulerAngles.z);

        float main = maxEmitterAngle * (angle / divisor);
        return (main, angle - main);
    }

    public bool AngleIsFacingRight(Quaternion rotation) => CalculateLaserRotation(rotation).main >= 0f;

    public bool UpdateLaserPositions(string name)
    {
        var index = LaserLocations.FindIndex(l => l.Name == name);

        if (index >= 0)
        {
            UpdateLaserPositions(LaserLocations[index]);
            return true;
        }

        return false;
    }

    public int GetHeadIndexForSprite(Sprite sprite, bool flipped)
    {
        for (int i = head_Degrees.Count - 1; i >= 0; i--)
        {
            if (head_Sprites[i] == sprite && head_HorizFlip[i] == flipped)
            {
                return i;
            }
        }
        return -1;
    }

    public int GetHeadIndexForAngle(float angle)
    {
        for (int i = head_Degrees.Count - 1; i >= 0; i--)
        {
            if (i == head_Degrees.Count - 1)
            {
                if (angle >= Mathf.Lerp(head_Degrees[i - 1], head_Degrees[i], 0.5f))
                {
                    return i;
                }
            }
            else if (i == 0)
            {
                if (angle < Mathf.Lerp(head_Degrees[i], head_Degrees[i + 1], 0.5f))
                {
                    return i;
                }
            }
            else
            {
                if (angle < Mathf.Lerp(head_Degrees[i], head_Degrees[i + 1], 0.5f) && angle >= Mathf.Lerp(head_Degrees[i - 1], head_Degrees[i], 0.5f))
                {
                    return i;
                }
            }
        }
        return -1;
    }

    public float GetCurrentHeadAngle() => GetHeadAngleForSprite(Boss.Head.MainRenderer.sprite);

    public float GetHeadAngleForSprite(Sprite sprite)
    {
        var index = GetHeadIndexForSprite(sprite, Boss.Head.MainRenderer.flipX);

        if (index != -1)
        {
            return head_Degrees[index];
        }
        else
        {
            switch (Boss.Orientation)
            {
                case AspidOrientation.Left:
                    return -MaxHeadAngle;
                case AspidOrientation.Right:
                    return MaxHeadAngle;
                default:
                    return 0f;
            }
        }
    }

    private void Reset()
    {
        laserOrigins.Clear();

        laserOrigins.Add(transform.Find("Left 2 Emitter").Find("Laser Rotate Origin"));
        laserOrigins.Add(transform.Find("Left 1 Emitter").Find("Laser Rotate Origin"));
        laserOrigins.Add(transform.Find("Central Emitter").Find("Laser Rotate Origin"));
        laserOrigins.Add(transform.Find("Right 1 Emitter").Find("Laser Rotate Origin"));
        laserOrigins.Add(transform.Find("Right 2 Emitter").Find("Laser Rotate Origin"));

        LaserLocations.Clear();

        /*var currentHeadSprite = Boss.GetComponentInChildren<HeadController>().MainRenderer.sprite;

        string currentSpriteID;

        if (!sprites.Contains(currentHeadSprite))
        {
            sprites.Add(currentHeadSprite);
            currentSpriteID = Guid.NewGuid().ToString();
            spriteIDs.Add(currentSpriteID);
        }
        else
        {
            var index = sprites.IndexOf(currentHeadSprite);
            currentSpriteID = spriteIDs[index];
        }*/

        LaserLocations.Add(new ShotgunLaserLocation
        {
            Name = "Default",
            correlatedHeadSprites = new List<Sprite>
            {
                Boss.GetComponentInChildren<HeadController>().MainRenderer.sprite
            },
            Left2Transform = CreateFromOrigin(laserOrigins[0]),
            Left1Transform = CreateFromOrigin(laserOrigins[1]),
            CenterTransform = CreateFromOrigin(laserOrigins[2]),
            Right1Transform = CreateFromOrigin(laserOrigins[3]),
            Right2Transform = CreateFromOrigin(laserOrigins[4]),
        });
    }

    static LaserPosAndRot CreateFromOrigin(Transform origin)
    {
        return new LaserPosAndRot
        {
            Position = origin.localPosition,
            Rotation = origin.localEulerAngles
        };
    }

    public void SetLaserRotation(int laserIndex, Quaternion rotation)
    {
        rotation = Quaternion.Euler(0, 0, 90f) * rotation;

        var direction = rotation * Vector3.right;

        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        laserOrigins[laserIndex].SetRotationZ(angle);
    }

    public void SetLaserRotation(int laserIndex, float zRotation)
    {
        SetLaserRotation(laserIndex, Quaternion.Euler(0f, 0f, zRotation));
    }

    public bool UpdateLaserPositions(Sprite correlatedHeadSprite = null)
    {
        if (correlatedHeadSprite == null)
        {
            correlatedHeadSprite = Boss.Head.MainRenderer.sprite;
        }

        var index = LaserLocations.FindIndex(l => l.correlatedHeadSprites.Contains(correlatedHeadSprite));

        if (index >= 0)
        {
            UpdateLaserPositions(LaserLocations[index]);
            return true;
        }

        return false;
    }

    public void UpdateLaserPositions(ShotgunLaserLocation laserLocation)
    {
        SetLaserPosAndRot(laserOrigins[0], laserLocation.Left2Transform);
        SetLaserPosAndRot(laserOrigins[1], laserLocation.Left1Transform);
        SetLaserPosAndRot(laserOrigins[2], laserLocation.CenterTransform);
        SetLaserPosAndRot(laserOrigins[3], laserLocation.Right1Transform);
        SetLaserPosAndRot(laserOrigins[4], laserLocation.Right2Transform);
    }

    void SetLaserPosAndRot(Transform laserOrigin, LaserPosAndRot posAndRot)
    {
        if (!Boss.Head.MainRenderer.flipX)
        {
            laserOrigin.localPosition = posAndRot.Position;
            laserOrigin.localRotation = Quaternion.Euler(posAndRot.Rotation);
        }
        else
        {
            laserOrigin.localPosition = new Vector3(-posAndRot.Position.x, posAndRot.Position.y, posAndRot.Position.z);
            laserOrigin.localRotation = Quaternion.Euler(new Vector3(-posAndRot.Rotation.x, posAndRot.Rotation.y, posAndRot.Rotation.z));
        }
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
#if UNITY_EDITOR
        /*if (laserLocations_objects == null)
        {
            laserLocations_objects = new List<UnityEngine.Object>();
        }
        laserLocations_data = WeaverSerializer.Serialize(LaserLocations, laserLocations_objects);*/

        //WeaverLog.Log(laserLocations_data);
#endif
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
#if !UNITY_EDITOR
        LaserLocations = WeaverSerializer.Deserialize<List<ShotgunLaserLocation>>(laserLocations_data, laserLocations_objects);
#endif
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (ADD_NEW_ELEMENT)
        {
            ADD_NEW_ELEMENT = false;

            var currentHeadSprite = Boss.GetComponentInChildren<HeadController>().MainRenderer.sprite;

            /*string currentSpriteID;

            if (!sprites.Contains(currentHeadSprite))
            {
                sprites.Add(currentHeadSprite);
                currentSpriteID = Guid.NewGuid().ToString();
                spriteIDs.Add(currentSpriteID);
            }
            else
            {
                var index = sprites.IndexOf(currentHeadSprite);
                currentSpriteID = spriteIDs[index];
            }*/

            LaserLocations.Add(new ShotgunLaserLocation
            {
                Name = string.IsNullOrEmpty(NAME_TO_RESTORE) ? "Default" : NAME_TO_RESTORE,
                correlatedHeadSprites = new List<Sprite>
                {
                    currentHeadSprite
                },
                Left2Transform = CreateFromOrigin(laserOrigins[0]),
                Left1Transform = CreateFromOrigin(laserOrigins[1]),
                CenterTransform = CreateFromOrigin(laserOrigins[2]),
                Right1Transform = CreateFromOrigin(laserOrigins[3]),
                Right2Transform = CreateFromOrigin(laserOrigins[4]),
            });
        }

        if (RESTORE_ELEMENT)
        {
            RESTORE_ELEMENT = false;

            UpdateLaserPositions(NAME_TO_RESTORE);
        }
        //laserLocations_data = WeaverSerializer.Serialize(LaserLocations, laserLocations_objects);
        WeaverSerializer.Serialize(LaserLocations, out laserLocations_data, out laserLocations_objects);

        /*List<UnityEngine.Object> objectReferences = new List<UnityEngine.Object>();

        var serializedResult = WeaverSerializer.Serialize(sprites, objectReferences);


        WeaverLog.Log("RESULT = " + serializedResult);

        foreach (var obj in objectReferences)
        {
            WeaverLog.Log("OBJ REF = " + obj);
        }*/

        /*var settings = new JsonSerializerSettings
        {
            ContractResolver = new IgnorePropertiesResolver()
        };

        Dictionary<string, UnityEngine.Object> objIDs = new Dictionary<string, UnityEngine.Object>();

        settings.Converters.Add(new UnityObjectToGUIDConverter(objIDs));

        var result = JsonConvert.SerializeObject(sprites, settings);

        WeaverLog.Log("TEST = " + result);

        foreach (var id in objIDs)
        {
            WeaverLog.Log("FOUND ID = " + id.Key + " - " + id.Value);
        }*/
    }
#endif

    Sprite lastSprite;
    bool oldFlipX;
    Func<int, Quaternion> laserRotationGetter = null;

    void LateUpdate()
    {
        if (laserRotationGetter != null)
        {
            if (Boss.Head.MainRenderer.sprite != lastSprite || oldFlipX != Boss.Head.MainRenderer.flipX)
            {
                oldFlipX = Boss.Head.MainRenderer.flipX;
                lastSprite = Boss.Head.MainRenderer.sprite;
                UpdateLaserPositions();
            }

            for (int i = 0; i < laserOrigins.Count; i++)
            {
                SetLaserRotation(i, laserRotationGetter(i));
            }
        }
    }

    void StartContinouslyUpdating(Func<int, Quaternion> laserRotationGetter)
    {
        lastSprite = null;
        oldFlipX = Boss.Head.MainRenderer.flipX;
        this.laserRotationGetter = laserRotationGetter;
    }

    /*IEnumerator ContinouslyUpdateRoutine(Func<int, Quaternion> laserRotationGetter)
    {
        lastSprite = null;
        oldFlipX = Boss.Head.MainRenderer.flipX;

        while (true)
        {
            if (Boss.Head.MainRenderer.sprite != lastSprite || oldFlipX != Boss.Head.MainRenderer.flipX)
            {
                oldFlipX = Boss.Head.MainRenderer.flipX;
                lastSprite = Boss.Head.MainRenderer.sprite;
                UpdateLaserPositions();
            }

            for (int i = 0; i < laserOrigins.Count; i++)
            {
                SetLaserRotation(i, laserRotationGetter(i));
            }

            yield return null;
        }
    }*/

    public void ContinouslyUpdateLasers(IList<float> laserRotations)
    {
        /*if (continouslyUpdatingRoutine != 0)
        {
            Boss.StopBoundRoutine(continouslyUpdatingRoutine);
            continouslyUpdatingRoutine = 0;
        }
        continouslyUpdatingRoutine = Boss.StartBoundRoutine(ContinouslyUpdateRoutine(index => Quaternion.Euler(0f, 0f, laserRotations[index])), () =>
        {
            continouslyUpdatingRoutine = 0;
        });*/
        StartContinouslyUpdating(index => Quaternion.Euler(0f, 0f, laserRotations[index]));
    }

    public void ContinouslyUpdateLasers(IList<Quaternion> laserRotations)
    {
        /*if (continouslyUpdatingRoutine != 0)
        {
            Boss.StopBoundRoutine(continouslyUpdatingRoutine);
            continouslyUpdatingRoutine = 0;
        }
        continouslyUpdatingRoutine = Boss.StartBoundRoutine(ContinouslyUpdateRoutine(index => laserRotations[index]), () =>
        {
            continouslyUpdatingRoutine = 0;
        });*/
        StartContinouslyUpdating(index => laserRotations[index]);
    }

    public void ContinouslyUpdateLasers(IList<Vector3> laserTargets)
    {
        /*continouslyUpdatingRoutine = Boss.StartBoundRoutine(ContinouslyUpdateRoutine(index =>
        {
            return Quaternion.Euler(0f, 0f, MathUtilities.CartesianToPolar(laserTargets[index] - laserOrigins[index].position).x);
        }), () =>
        {
            continouslyUpdatingRoutine = 0;
        });*/
        StartContinouslyUpdating(index =>
        {
            return Quaternion.Euler(0f, 0f, MathUtilities.CartesianToPolar(laserTargets[index] - laserOrigins[index].position).x);
        });
    }

    public void ContinouslyUpdateLasers(Func<int, Quaternion> laserRotationGetter)
    {
        /*if (continouslyUpdatingRoutine != 0)
        {
            Boss.StopBoundRoutine(continouslyUpdatingRoutine);
            continouslyUpdatingRoutine = 0;
        }
        continouslyUpdatingRoutine = Boss.StartBoundRoutine(ContinouslyUpdateRoutine(laserRotationGetter),() =>
        {
            continouslyUpdatingRoutine = 0;
        });*/
        StartContinouslyUpdating(laserRotationGetter);
    }

    public void ContinouslyUpdateLasers(Func<int, Vector3> laserTargetGetter)
    {
        /*continouslyUpdatingRoutine = Boss.StartBoundRoutine(ContinouslyUpdateRoutine(index =>
        {
            return Quaternion.Euler(0f, 0f, MathUtilities.CartesianToPolar(laserTargetGetter(index) - laserOrigins[index].position).x);
        }), () =>
        {
            continouslyUpdatingRoutine = 0;
        });*/

        StartContinouslyUpdating(index =>
        {
            return Quaternion.Euler(0f, 0f, MathUtilities.CartesianToPolar(laserTargetGetter(index) - laserOrigins[index].position).x);
        });
    }

    public void StopContinouslyUpdating()
    {
        /*if (continouslyUpdatingRoutine != 0)
        {
            Boss.StopBoundRoutine(continouslyUpdatingRoutine);
            continouslyUpdatingRoutine = 0;
        }*/
        laserRotationGetter = null;
    }

    public float GetAngleToTargetFromLaser(int laserIndex, Vector3 target)
    {
        return MathUtilities.CartesianToPolar(target - laserOrigins[laserIndex].position).x;
    }

    public Quaternion GetLaserAngle(int laserIndex)
    {
        return laserOrigins[laserIndex].rotation * Quaternion.Euler(0, 0, -90f);
    }

    public Vector3 GetFarAwayLaserTarget(int laserIndex)
    {
        return laserOrigins[laserIndex].position + (Vector3)MathUtilities.PolarToCartesian(GetLaserAngle(laserIndex).eulerAngles.z, 1000f);
    }

    public Vector3 GetFarAwayLaserTargetAtAngle(int laserIndex, float zDegrees)
    {
        return laserOrigins[laserIndex].position + (Vector3)MathUtilities.PolarToCartesian(zDegrees, 1000f);
    }
}
