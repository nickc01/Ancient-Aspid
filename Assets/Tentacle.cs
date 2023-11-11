using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Utilities;

[ExecuteAlways]
public class Tentacle : MonoBehaviour
{
    [SerializeField]
    [Range(2, 100)]
    int meshQuality = 50;

    [SerializeField]
    float waveIntensity = 5f;

    [SerializeField]
    float waveDelay = 0f;

    [SerializeField]
    Vector2 waveTimeOffsetRange = new Vector2(0f,1f);

    [SerializeField]
    float waveSpeed = 1f;

    [SerializeField]
    AnimationCurve topCurve;

    [SerializeField]
    AnimationCurve bottomCurve;

    [SerializeField]
    Color _color;

    [NonSerialized]
    Mesh mesh;
    [NonSerialized]
    int meshQualityPrev = int.MinValue;
    [NonSerialized]
    List<Vector3> verticies;
    [NonSerialized]
    List<int> indicies;
    [NonSerialized]
    List<Vector3> uvs;

    [NonSerialized]
    MeshRenderer _mainRenderer;
    public MeshRenderer MainRenderer => _mainRenderer ??= GetComponent<MeshRenderer>();

    [NonSerialized]
    MeshFilter _mainFilter;
    public MeshFilter MainFilter => _mainFilter ??= GetComponent<MeshFilter>();

    public Color Color
    {
        get => _color;
        set
        {
            _color = value;
            UpdateColor();
        }
    }

    [SerializeField]
    Texture _texture;
    public Texture Texture
    {
        get => _texture;
        set
        {
            _texture = value;
            UpdateTexture();
        }
    }

    [NonSerialized]
    bool init = false;

    [NonSerialized]
    float currentWaveIterator = 0f;

    [NonSerialized]
    private MaterialPropertyBlock propBlock;

    private void Awake()
    {
        Init();
    }

    void Init()
    {
        if (init)
        {
            return;
        }
        init = true;

        mesh = new Mesh();

        mesh.MarkDynamic();
        UpdateMesh();
        UpdateColor();
        UpdateTexture();

        MainFilter.sharedMesh = mesh;

        if (Application.isPlaying)
        {
            currentWaveIterator = waveTimeOffsetRange.RandomInRange();
        }
    }

    private void OnValidate()
    {
        UpdateColor();
        UpdateTexture();
    }

    void UpdateColor()
    {
        if (propBlock == null)
        {
            propBlock = new MaterialPropertyBlock();
        }

        MainRenderer.GetPropertyBlock(propBlock);

        propBlock.SetColor("_Color", _color);

        MainRenderer.SetPropertyBlock(propBlock);
    }

    void UpdateTexture()
    {
        if (propBlock == null)
        {
            propBlock = new MaterialPropertyBlock();
        }

        MainRenderer.GetPropertyBlock(propBlock);

        if (_texture == null)
        {
            propBlock.SetTexture("_MainTex", Texture2D.whiteTexture);
        }
        else
        {
            propBlock.SetTexture("_MainTex", _texture);
        }

        MainRenderer.SetPropertyBlock(propBlock);
    }

    private void Update()
    {
        Init();

        if (Application.isPlaying)
        {
            currentWaveIterator = (currentWaveIterator + (Time.deltaTime * waveSpeed)) % 1f;
        }

        UpdateMesh();
    }

    private void Reset()
    {
        topCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        bottomCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    }

    void UpdateMesh()
    {
        Init();
        if (indicies == null)
        {
            indicies = new List<int>((meshQuality - 1) * 6);
        }

        if (verticies == null)
        {
            verticies = new List<Vector3>(meshQuality * 2);
        }

        if (uvs == null)
        {
            uvs = new List<Vector3>(meshQuality * 2);
        }

        if (meshQuality != meshQualityPrev)
        {
            meshQualityPrev = meshQuality;

            verticies.Clear();
            indicies.Clear();
            uvs.Clear();

            for (int i = 0; i < meshQuality; i++)
            {
                //verticies.Add(new Vector3(-0.5f + (i / (meshQuality - 1f)), -0.5f, topCurve.Evaluate(i / (meshQuality - 1f))));
                //verticies.Add(new Vector3(-0.5f + (i / (meshQuality - 1f)), 0.5f, -bottomCurve.Evaluate(i / (meshQuality - 1f))));

                //uvs.Add(new Vector3(i / (meshQuality - 1f), 0f, 0f));
                //uvs.Add(new Vector3(i / (meshQuality - 1f), 1f, 0f));

                verticies.Add(default);
                verticies.Add(default);

                uvs.Add(default);
                uvs.Add(default);
            }

            //Initialize the triangle list
            for (int i = 0; i < (meshQuality - 1); i++)
            {
                indicies.Add(0 + (i * 2));
                indicies.Add(1 + (i * 2));
                indicies.Add(2 + (i * 2));

                indicies.Add(1 + (i * 2));
                indicies.Add(3 + (i * 2));
                indicies.Add(2 + (i * 2));
            }
        }

        for (int i = 0; i < meshQuality * 2; i += 2)
        {
            var x = (i / (meshQuality - 1f));

            var waveSample = Mathf.Sin((currentWaveIterator - (waveDelay * x)) * 2f * Mathf.PI);

            var waveOffset = waveSample * waveIntensity * x;

            var topCurveSample = topCurve.Evaluate(x);
            var bottomCurveSample = bottomCurve.Evaluate(x);

            verticies[i] = (new Vector3(-0.5f + x, waveOffset + topCurveSample, 0f));
            verticies[i + 1] = (new Vector3(-0.5f + x, waveOffset - bottomCurveSample, 0f));

            uvs[i] = new Vector3(x, topCurveSample + bottomCurveSample, 0f);
            uvs[i + 1] = new Vector3(x, 0f, 0f);
        }

        //Set the mesh's vertex data
        mesh.SetVertices(verticies);
        //Set the mesh's triangle data
        mesh.SetTriangles(indicies, 0);
        //Set the mesh's UV data
        mesh.SetUVs(0, uvs);
        //Recalculate the mesh normals and bounds
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}
