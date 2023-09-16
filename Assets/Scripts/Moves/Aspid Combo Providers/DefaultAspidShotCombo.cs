using System.Collections.Generic;
using UnityEngine;
using static IAspidComboProvider;

public class DefaultAspidShotCombo : MonoBehaviour, IAspidComboProvider
{
    [SerializeField]
    float shotSpeed = 16f;

    [SerializeField]
    float shotScale = 1.5f;

    [SerializeField]
    float shotAngleSeparation = 30f;

    [SerializeField]
    float slowerShotSpeed = 8f;

    [SerializeField]
    float comboShotSpeed = 12.5f;

    [SerializeField]
    int shotAmount = 3;

    List<float> shotAngleOffsets = new List<float>
    {
        0f,
        15f,
        -15f
    };

    bool doingCombo = false;

    public IEnumerator<ShotInfo> DoShots(int comboIndex)
    {
        yield return new ShotInfo(shotAmount, doingCombo ? comboShotSpeed : shotSpeed, shotScale, shotAngleSeparation, shotAngleOffsets[comboIndex % 3]);

        if (!doingCombo)
        {
            yield return new ShotInfo(shotAmount - 1, slowerShotSpeed, shotScale, shotAngleSeparation, shotAngleOffsets[comboIndex % 3]);
        }
    }

    public void Init(out int comboCount)
    {
        comboCount = UnityEngine.Random.Range(0f, 1f) > 0.5f ? 1 : 3;
        doingCombo = comboCount > 1;
    }
}