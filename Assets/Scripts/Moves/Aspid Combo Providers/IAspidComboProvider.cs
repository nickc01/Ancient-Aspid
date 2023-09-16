using System.Collections;
using System.Collections.Generic;

public interface IAspidComboProvider
{
    public struct ShotInfo
    {
        public int Shots;
        public float ShotSpeed;
        public float ShotScale;
        public float ShotAngleSeparation;
        public float ShotAngleOffset;

        public ShotInfo(int shots)
        {
            Shots = shots;
            ShotSpeed = 16f;
            ShotScale = 1.5f;
            ShotAngleSeparation = 30f;
            ShotAngleOffset = 0f;
        }

        public ShotInfo(int shots, float shotSpeed, float shotScale, float shotAngleSeperation, float shotAngleOffset)
        {
            Shots = shots;
            ShotSpeed = shotSpeed;
            ShotScale = shotScale;
            ShotAngleSeparation = shotAngleSeperation;
            ShotAngleOffset = shotAngleOffset;
        }
    }


    void Init(out int comboCount);

    IEnumerator<ShotInfo> DoShots(int comboIndex);
}