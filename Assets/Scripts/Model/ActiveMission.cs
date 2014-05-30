using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Shanghai.Model {
    public class ActiveMission {
        public static readonly string EVENT_CELL_PROGRESSED = "EVENT_CELL_PROGRESSED";
        public static readonly string EVENT_PACKAGE_DELIVERED = "EVENT_PACKAGE_DELIVERED";

        public List<IntVect2> Path;

        public ShanghaiUtils.PaintColour PaintColour;
        public int Points = 0;
        public int PointsModifier = 1;

        public IntVect2 CurrentCell {
            get { return Path[0]; }
        }

        private ShanghaiConfig _Config;

        public ActiveMission(List<IntVect2> path, ShanghaiUtils.PaintColour colour) {
            Path = path;
            PaintColour = colour;
            _Config = ShanghaiConfig.Instance;
        }

        public bool Progress() {
            Debug.Log("path was " + Path.Count);
            Path.Remove(Path[0]);
            Debug.Log("path now " + Path.Count);

            if (Path.Count <= 1) {
                return true;
            }
            return false;
        }
    }
}
