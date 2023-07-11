/*using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WeaverCore.Utilities;

[ExecuteAlways]
public class GapFinder : MonoBehaviour
{
    [InitializeOnLoad]
    static class Initializer
    {
        static Initializer()
        {
            var mainFinder = GameObject.FindObjectOfType<GapFinder>();
            mainFinder.OnEnable();
        }
    }

    class SpriteHighlightInfo
    {
        public SpriteRenderer renderer;
        public SpriteRenderer outer;
        public SpriteRenderer inner;
    }

    [NonSerialized]
    UnboundCoroutine updateRoutine;

    [NonSerialized]
    Dictionary<GameObject, SpriteHighlightInfo> selectedObjects;

    public void OnEnable()
    {
        if (updateRoutine == null)
        {
            updateRoutine = UnboundCoroutine.Start(UpdateAlways());
            EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;  
        }
    }

    private void EditorApplication_playModeStateChanged(PlayModeStateChange obj)
    {
        switch (obj)
        {
            case PlayModeStateChange.EnteredEditMode:
                break;
            case PlayModeStateChange.EnteredPlayMode:
                break;
            case PlayModeStateChange.ExitingEditMode:
            case PlayModeStateChange.ExitingPlayMode:
                OnDestroy();
                break;
            default:
                break;
        }
    }

    void UpdateObject(GameObject obj, SpriteHighlightInfo info)
    {
        //Debug.Log("UPDATE OBJECT = " + obj);
        //return;
        if (info == null)
        {
            Debug.Log("ADDING OBJECT = " + obj);
            info = new SpriteHighlightInfo();
            if (obj.TryGetComponent<SpriteRenderer>(out var renderer))
            {
                info.renderer = renderer;

                //var bounds = info.renderer.bounds;

                info.outer = new GameObject().AddComponent<SpriteRenderer>();
                info.outer.gameObject.hideFlags = HideFlags.HideAndDontSave;
                info.outer.sprite = info.renderer.sprite;
                info.outer.color = new Color(1f,0,0,0.2f);
                info.outer.transform.position = obj.transform.position;
                info.outer.transform.rotation = obj.transform.rotation;
                info.outer.transform.localScale = obj.transform.localScale;

                info.inner = new GameObject().AddComponent<SpriteRenderer>();
                info.inner.gameObject.hideFlags = HideFlags.HideAndDontSave;
                info.inner.sprite = info.renderer.sprite;
                info.inner.color = new Color(0f, 1, 0, 0.3f);
                info.inner.transform.position = obj.transform.position;
                info.inner.transform.rotation = obj.transform.rotation;
                info.inner.transform.localScale = obj.transform.localScale + new Vector3(-0.37f * obj.transform.position.z, -0.37f * obj.transform.position.z);
            }

            selectedObjects.Add(obj, info);
        }
        else
        {
            if (info.outer != null && info.renderer != null)
            {
                info.outer.sprite = info.renderer.sprite;
                info.outer.color = new Color(1f, 0, 0, 1f);
                info.outer.flipX = info.renderer.flipX;
                info.outer.flipY = info.renderer.flipY;
                info.outer.transform.position = obj.transform.position + new Vector3(0f,0f,-0.00001f);
                info.outer.transform.rotation = obj.transform.rotation;
                info.outer.transform.localScale = obj.transform.localScale;
            }

            if (info.inner != null && info.renderer != null)
            {
                info.inner.sprite = info.renderer.sprite;
                info.inner.flipX = info.renderer.flipX;
                info.inner.flipY = info.renderer.flipY;
                info.inner.color = new Color(0f, 1, 0, 1f);
                info.inner.transform.position = obj.transform.position + new Vector3(0f, 0f, -0.00002f);
                info.inner.transform.rotation = obj.transform.rotation;
                info.inner.transform.localScale = obj.transform.localScale + new Vector3(-0.37f * obj.transform.position.z * 2f, -0.37f * obj.transform.position.z * 2f);

                if (info.inner.transform.GetXLocalScale() < 0)
                {
                    info.inner.transform.SetXLocalScale(0f);
                }

                if (info.inner.transform.GetYLocalScale() < 0)
                {
                    info.inner.transform.SetYLocalScale(0f);
                }
            }
        }
    }

    private void OnDisable()
    {
        OnDestroy();
    }


    static List<(GameObject, SpriteHighlightInfo)> removeCache = new List<(GameObject, SpriteHighlightInfo)>();

    IEnumerator UpdateAlways()
    {
        if (selectedObjects == null)
        {
            selectedObjects = new Dictionary<GameObject, SpriteHighlightInfo>();
        }
        while (true)
        {
            foreach (var obj in Selection.gameObjects)
            {
                SpriteHighlightInfo info;
                selectedObjects.TryGetValue(obj, out info);
                UpdateObject(obj, info);
            }

            foreach (var obj in selectedObjects)
            {
                if (!Selection.gameObjects.Contains(obj.Key))
                {
                    removeCache.Add((obj.Key, obj.Value));
                }
            }

            if (removeCache.Count > 0)
            {
                foreach (var (obj, info) in removeCache)
                {
                    if (info.outer != null)
                    {
                        if (!Application.isPlaying)
                        {
                            DestroyImmediate(info.outer.gameObject);
                        }
                        else
                        {
                            Destroy(info.outer.gameObject);
                        }
                    }

                    if (info.inner != null)
                    {
                        if (!Application.isPlaying)
                        {
                            DestroyImmediate(info.inner.gameObject);
                        }
                        else
                        {
                            Destroy(info.inner.gameObject);
                        }
                    }

                    Debug.Log("REMOVING OBJECT = " + obj);

                    selectedObjects.Remove(obj);
                }

                removeCache.Clear();
            }


            yield return null;
        }
    }

    private void OnDestroy()
    {
        if (updateRoutine != null)
        {
            UnboundCoroutine.Stop(updateRoutine);
            updateRoutine = null;
            EditorApplication.playModeStateChanged -= EditorApplication_playModeStateChanged;

            if (selectedObjects != null)
            {
                foreach (var obj in selectedObjects)
                {
                    Debug.Log("REMOVING OBJECT = " + obj.Key);

                    if (obj.Value.outer != null)
                    {
                        if (!Application.isPlaying)
                        {
                            DestroyImmediate(obj.Value.outer.gameObject);
                        }
                        else
                        {
                            Destroy(obj.Value.outer.gameObject);
                        }
                    }

                    if (obj.Value.inner != null)
                    {
                        if (!Application.isPlaying)
                        {
                            DestroyImmediate(obj.Value.inner.gameObject);
                        }
                        else
                        {
                            Destroy(obj.Value.inner.gameObject);
                        }
                    }
                }

                selectedObjects.Clear();
            }
        }
    }
}
*/