using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Shanghai.Model {
    public abstract class ColouredCellAsset {
        public IntVect2 CellKey;
        public ShanghaiUtils.PaintColour PaintColour = ShanghaiUtils.PaintColour.NONE;
    }
}
