using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using WeaverCore;
using WeaverCore.Features;

public abstract class Phase : MonoBehaviour
{
    [field: SerializeField]
    public string PhaseName { get; private set; }

    [field: SerializeField]
    public bool ClimbingPhase { get; private set; } = false;

    /*[SerializeField]
    Rect _phaseBoundaries;

    public Rect PhaseBoundaries => new Rect(_phaseBoundaries.position + (Vector2)transform.position, _phaseBoundaries.size);*/
    [NonSerialized]
    BoxCollider2D _boundary;

    public BoxCollider2D Boundary => _boundary ??= GetComponent<BoxCollider2D>();

    public Rect PhaseBoundaries
    {
        get
        {
            var bounds = Boundary.bounds;

            var rect = new Rect
            {
                min = bounds.min,
                max = bounds.max
            };
            return rect;
        }
    }

    //[field: SerializeField]
    //public Rect PhaseBoundaries { get; private set; }

    [field: SerializeField]
    public Phase NextPhase { get; private set; }

    [SerializeField]
    bool onlyOnce = true;

    [SerializeField]
    [FormerlySerializedAs("doHeightCheck")]
    bool doPlayerHeightCheck = false;

    [SerializeField]
    [Tooltip("If \"Do Player Height Check\" is true, the height of the player will be checked against this value before going to the next phase")]
    [FormerlySerializedAs("nextPhaseStartHeight")]
    float nextPhasePlayerStartHeight;

    [SerializeField]
    bool doHealthCheck = false;

    [SerializeField]
    [Tooltip("If \"Do Health Check\" is true, the health of the boss will be checked against this value before going to the next phase")]
    float nextPhaseHealthValue;

    [SerializeField]
    bool doRegionCheck = false;

    [SerializeField]
    [Tooltip("If \"Do Region Check\" is true, then the next phase will only be activated if the player is within the specified region")]
    PhaseContactRegion phaseRegion;

    public bool ActivatedOnce { get; private set; }

    public bool PhaseActive { get; private set; }

    public virtual bool CanGoToNextPhase(AncientAspid boss)
    {
        bool canTransition = false;

        if (NextPhase != null && NextPhase.onlyOnce && NextPhase.ActivatedOnce)
        {
            return false;
        }

        if (doPlayerHeightCheck)
        {
            canTransition = canTransition || Player.Player1.transform.position.y >= nextPhasePlayerStartHeight;
        }

        if (doHealthCheck)
        {
            canTransition = canTransition || (boss.HealthManager.Health / (float)boss.StartingHealth) <= nextPhaseHealthValue;
        }

        if (doRegionCheck)
        {
            canTransition = canTransition || phaseRegion.EnteredPhaseRegion;
        }

        //Debug.Log($"{PhaseName} next = {canTransition && NextPhase != null}");

        return canTransition && NextPhase != null;
    }

    static void drawString(string text, Vector3 worldPos, Color? colour = null)
    {
#if UNITY_EDITOR
        UnityEditor.Handles.BeginGUI();
        if (colour.HasValue) GUI.color = colour.Value;
        var view = UnityEditor.SceneView.currentDrawingSceneView;
        Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);
        Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
        GUI.Label(new Rect(screenPos.x - (size.x / 2), -screenPos.y + view.position.height + 4, size.x, size.y), text);
        UnityEditor.Handles.EndGUI();
#endif
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1,1,0,0.4f);
        Gizmos.DrawCube(PhaseBoundaries.center, PhaseBoundaries.size);

        drawString(PhaseName, (Vector3)PhaseBoundaries.center);

        if (doPlayerHeightCheck)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawCube(new Vector3(transform.position.x, nextPhasePlayerStartHeight, transform.position.z), new Vector3(150, 1, 1));
        }
    }

    public IEnumerator PhaseStart(AncientAspid boss, Phase prevPhase)
    {
        if (!ActivatedOnce)
        {
            ActivatedOnce = true;
        }

        PhaseActive = true;

        Debug.Log("Entering " + PhaseName);

        yield return OnPhaseStart(boss, prevPhase);
    }

    public IEnumerator PhaseEnd(AncientAspid boss, Phase nextPhase)
    {
        Debug.Log("Exiting " + PhaseName);
        yield return OnPhaseEnd(boss, nextPhase);

        PhaseActive = false;
    }

    protected abstract IEnumerator OnPhaseStart(AncientAspid boss, Phase prevPhase);
    protected abstract IEnumerator OnPhaseEnd(AncientAspid boss, Phase nextPhase);
}