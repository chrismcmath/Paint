using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Shanghai.Model {
    public class Source : ColouredCellAsset {
        public float TTL;

        public bool IsActive = false;
        // locked is when a player starts drawing on it
        public bool Locked = false;

        public Source(IntVect2 cellKey, float ttl, ShanghaiUtils.PaintColour colour) {
            CellKey = cellKey;
            TTL = ttl;
            PaintColour = colour;
        }

        public bool IsTTD(float delta) {
            TTL -= delta;
            return TTL <= 0.0f;
        }
    }
}
