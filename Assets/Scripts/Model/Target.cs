using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Shanghai.Model {
    public class Target : ColouredCellAsset {
        public float TTL;

        public int Lives = ShanghaiConfig.Instance.TargetLives;

        public bool IsActive = false;

        private bool _Freeze = false;
        public bool Freeze {
            get { return _Freeze; }
            set {
                Debug.Log("setting _Freeze to " + value);
                _Freeze = value;
            }
        }

        public Target(IntVect2 cellKey, ShanghaiUtils.PaintColour colour, float ttl) {
            CellKey = cellKey;
            PaintColour = colour;
            TTL = ttl;
        }

        public bool IsTTD() {
            Lives -= 1;
            return Lives == 0;
        }

        public bool IsTTD(float delta) {
            TTL -= delta;
            return TTL <= 0.0f;
        }
    }
}
