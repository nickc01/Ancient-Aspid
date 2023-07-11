using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Assets;
using WeaverCore.Utilities;

public class ColorManager : MonoBehaviour
{
    [SerializeField]
    ColorSlider RedColorSlider;

    [SerializeField]
    ColorSlider GreenColorSlider;

    [SerializeField]
    ColorSlider BlueColorSlider;




    [SerializeField]
    ColorSlider SaturationSlider;

    [SerializeField]
    ColorSlider AmbientLightIntensitySlider;


    [SerializeField]
    ColorMode RedColorMode;

    [SerializeField]
    ColorMode GreenColorMode;

    [SerializeField]
    ColorMode BlueColorMode;

    [SerializeField]
    ColorBox bloomBox;

    private void Start()
    {
        StartCoroutine(StartRoutine());
    }


    static AnimationCurve GetCurve(ColorMode mode, float maxValue)
    {
        return GetCurve(mode.GetValue(),maxValue);
    }

    static AnimationCurve GetCurve(CurveType type, float maxValue)
    {
        if (type == CurveType.Linear)
        {
            return AnimationCurve.Linear(0f, 0f, 1f, maxValue);
        }
        else
        {
            return AnimationCurve.EaseInOut(0f, 0f, 1f, maxValue);
        }
    }

    IEnumerator StartRoutine()
    {
        yield return new WaitForSecondsRealtime(0.5f);


        var correctionCurves = GameCameras.instance.ReflectGetField("colorCorrectionCurves");
        var sceneColorManager = GameCameras.instance.ReflectGetField("sceneColorManager");
        var bloomEffect = (MonoBehaviour)WeaverCamera.Instance.GetComponent("UnityStandardAssets.ImageEffects.BloomOptimized");

        RedColorSlider.SetSliderValue(((AnimationCurve)correctionCurves.ReflectGetField("redChannel")).Evaluate(1f));
        GreenColorSlider.SetSliderValue(((AnimationCurve)correctionCurves.ReflectGetField("greenChannel")).Evaluate(1f));
        BlueColorSlider.SetSliderValue(((AnimationCurve)correctionCurves.ReflectGetField("blueChannel")).Evaluate(1f));

        SaturationSlider.SetSliderValue((float)correctionCurves.ReflectGetField("saturation"));
        AmbientLightIntensitySlider.SetSliderValue((float)sceneColorManager.ReflectGetField("AmbientIntensityA"));

        RedColorMode.SetValue(CurveType.Linear);
        GreenColorMode.SetValue(CurveType.Linear);
        BlueColorMode.SetValue(CurveType.Linear);

        bloomBox.SetValue(bloomEffect.enabled);


        Action<float> updateRed = v =>
        {
            correctionCurves.ReflectSetField("redChannel", GetCurve(RedColorMode, v));
            correctionCurves.ReflectCallMethod("UpdateParameters");
            //GameCameras.instance.colorCorrectionCurves.redChannel = GetCurve(RedColorMode, v);
            correctionCurves.ReflectCallMethod("UpdateParameters");
        };

        Action<float> updateGreen = v =>
        {
            correctionCurves.ReflectSetField("greenChannel", GetCurve(GreenColorMode, v));
            correctionCurves.ReflectCallMethod("UpdateParameters");
            //GameCameras.instance.colorCorrectionCurves.greenChannel = GetCurve(GreenColorMode, v);
            //GameCameras.instance.colorCorrectionCurves.UpdateParameters();
        };

        Action<float> updateBlue = v =>
        {
            correctionCurves.ReflectSetField("blueChannel", GetCurve(BlueColorMode, v));
            correctionCurves.ReflectCallMethod("UpdateParameters");
            //GameCameras.instance.colorCorrectionCurves.blueChannel = GetCurve(BlueColorMode, v);
            //GameCameras.instance.colorCorrectionCurves.UpdateParameters();
        };


        RedColorSlider.onSliderUpdate.AddListener(v => updateRed(v));

        GreenColorSlider.onSliderUpdate.AddListener(v => updateGreen(v));

        BlueColorSlider.onSliderUpdate.AddListener(v => updateBlue(v));


        SaturationSlider.onSliderUpdate.AddListener(v =>
        {
            //GameCameras.instance.colorCorrectionCurves.saturation = v;
            correctionCurves.ReflectSetField("saturation", v);
            correctionCurves.ReflectCallMethod("UpdateParameters");
            //GameCameras.instance.colorCorrectionCurves.UpdateParameters();
        });

        RedColorMode.onCurveUpdate.AddListener(v =>
        {
            updateRed(RedColorSlider.GetSliderValue());
        });

        GreenColorMode.onCurveUpdate.AddListener(v =>
        {
            updateGreen(GreenColorSlider.GetSliderValue());
        });

        BlueColorMode.onCurveUpdate.AddListener(v =>
        {
            updateBlue(BlueColorSlider.GetSliderValue());
        });

        AmbientLightIntensitySlider.onSliderUpdate.AddListener(v =>
        {
            SceneManager.SetLighting((Color)sceneColorManager.ReflectGetField("AmbientColorA"), v);
        });

        bloomBox.onValueChanged.AddListener(v =>
        {
            bloomEffect.enabled = v;
        });
    }

    public void Close()
    {
        GameObject.Destroy(gameObject);
    }
}
