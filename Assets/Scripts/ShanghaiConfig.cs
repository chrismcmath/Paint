using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Shanghai {
    public class ShanghaiConfig : MonoSingleton<ShanghaiConfig> {
        public float ColourInterval = 8.0f;
        public float SourceInterval = 3.0f;
        public float TargetInterval = 3.0f;
        public float CellFillPerSecond = 0.5f;

        public int BountyMin = 50;
        public int BountyMax = 2000;
        public float BountyDeviancePower = 3.0f;

        public float SourceWaitTime = 10.0f;
        public float TargetWaitTime = 10.0f;

        public int PacketSize = 100;

        /* Ministries */
        public float HealthIncPerSecond = 1f;
        public float MaxHealth = 100f;
        public float MinHealth = 10f;

        /* Embassies */
        public float ReputationIncOnMissionComplete = 20f;
        public float ReputationDecOnMissionFailed = 20f;
        public float ReputationDecPerSecond = 1f;
        public float MaxReputation = 100f;
        public float MinReputation = 10f;

        public float MissionFlagAlpha = 0.7f;
        public float MissionTargetAlpha = 0.5f;

        public Color RED = new Color(0.0f, 0.0f, 0.0f);
        public Color BLUE = new Color(0.0f, 0.0f, 0.0f);
        public Color YELLOW = new Color(0.0f, 0.0f, 0.0f);
        public Color GREEN = new Color(0.0f, 0.0f, 0.0f);
        public Color PURPLE = new Color(0.0f, 0.0f, 0.0f);
        public Color ORANGE = new Color(0.0f, 0.0f, 0.0f);
        public Color GREY = new Color(0.0f, 0.0f, 0.0f);
    }
}
