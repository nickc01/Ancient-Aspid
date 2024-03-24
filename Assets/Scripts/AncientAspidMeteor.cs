using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using WeaverCore.Utilities;
using static UnityEngine.GraphicsBuffer;

public class AncientAspidMeteor : MonoBehaviour
{
    [SerializeField]
    float travelTime = 6f;

    [SerializeField]
    Vector3 travelDestination;

    [SerializeField]
    AnimationCurve cameraMovementCurve;

    [SerializeField]
    Vector3 cameraStartPos;

    [SerializeField]
    Vector3 cameraEndPos;

    [SerializeField]
    Vector2 cameraShakeIntensityStart = new Vector2(-0.5f,-0.5f);

    [SerializeField]
    Vector2 cameraShakeIntensityEnd = new Vector2(2f, 2f);

    [SerializeField]
    CameraController camController;

    [SerializeField]
    List<Transform> zeroZObjects;

    [SerializeField]
    Vector3 startScale = new Vector3(-0.3f, -0.3f, 1f);

    [SerializeField]
    Vector2 playerPos;

    [SerializeField]
    Vector3 playerScale = new Vector3(1f, 1f, 1f);

    Vector3 currentCamPos = default;

    [SerializeField]
    bool updateCamera = true;

    [SerializeField]
    bool updatePlayer = true;

    [SerializeField]
    float preWaitTime = 6f;

    private void Start()
    {
        currentCamPos = cameraStartPos;
        /*if (camController == null)
        {
            camController = GameManager.instance.cameraCtrl;
        }*/
        StartCoroutine(MainRoutine());
    }

    IEnumerator MainRoutine()
    {
        yield return new WaitForSeconds(preWaitTime);
        Vector2 sourcePos = transform.position;
        var hero = HeroController.instance;

        if (updatePlayer)
        {
            hero.RelinquishControl();
            hero.StopAnimationControl();

            hero.transform.position = playerPos;
            hero.transform.localScale = playerScale;
        }

        HeroUtilities.PlayPlayerClip("Challenge Start");

        void Interp(float t)
        {
            transform.position = Vector3.Lerp(sourcePos, travelDestination, t);
            if (camController != null && updateCamera)
            {
                UpdateShaking(camController.transform, Vector2.Lerp(cameraShakeIntensityStart, cameraShakeIntensityEnd, t));
                currentCamPos = Vector3.Lerp(cameraStartPos, cameraEndPos, cameraMovementCurve.Evaluate(t));
                camController.SetMode(CameraController.CameraMode.FROZEN);
                camController.transform.position = currentCamPos + shakeVector;
            }

            var newScale = Vector3.Lerp(startScale, new Vector3(1f, 1f, 1f), t);

            newScale.x = Mathf.Clamp(newScale.x, 0f, 9999f);
            newScale.y = Mathf.Clamp(newScale.y, 0f, 9999f);
            transform.localScale = newScale;

            foreach (var obj in zeroZObjects)
            {
                obj.transform.SetZPosition(-10f);
            }
        }

        for (float t = 0; t < travelTime; t += Time.deltaTime)
        {
            Interp(t / travelTime);
            yield return null;
        }

        while (true)
        {
            Interp(1f);
            yield return null;
        }
    }

    const float FpsLimit = 60f;
    private float nextUpdateTime;
    Vector3 shakeVector = default;

    private void UpdateShaking(Transform target, Vector2 extents)
    {
        extents.x = Mathf.Clamp(extents.x, 0f, 99999f);
        extents.y = Mathf.Clamp(extents.y, 0f, 99999f);
        if (target != null && !(extents.x == 0f && extents.y == 0f))
        {
            if (FpsLimit > 0f)
            {
                if (Time.unscaledTime < nextUpdateTime)
                {
                    return;
                }
                nextUpdateTime = Time.unscaledTime + 1f / FpsLimit;
            }
            shakeVector = Vector3.Scale(extents, new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)));
        }
    }

    //Vector3 originalPos;

    /*private void OnPreRender()
    {
        originalPos = cam.transform.position;
        cam.transform.position = currentCamPos + shakeVector;
    }

    private void OnPostRender()
    {
        cam.transform.position = originalPos;
    }*/
}
