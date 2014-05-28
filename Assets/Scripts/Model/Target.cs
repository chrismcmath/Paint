using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Shanghai.Model {
    public class Target : ColouredCellAsset {
        public float TTL;

        public bool IsActive = false;
        public bool Freeze = false;

        public Target(IntVect2 cellKey, ShanghaiUtils.PaintColour colour, float ttl) {
            CellKey = cellKey;
            PaintColour = colour;
            TTL = ttl;
        }

        public bool IsTTD(float delta) {
            TTL -= delta;
            return TTL <= 0.0f;
        }
    }
}
