using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Shanghai.Model {
    public class Source : ColouredCellAsset {
        public float TTL;

        public bool IsActive = false;

        public Source(IntVect2 cellKey, float ttl) {
            CellKey = cellKey;
            TTL = ttl;
        }

        public bool IsTTD(float delta) {
            TTL -= delta;
            return TTL <= 0.0f;
        }
    }
}
