using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Shanghai.Model {
    public class ActiveMission {
        public static readonly string EVENT_CELL_PROGRESSED = "EVENT_CELL_PROGRESSED";
        public static readonly string EVENT_PACKAGE_DELIVERED = "EVENT_PACKAGE_DELIVERED";

        public List<IntVect2> Path;
        public Source Source;
        public Target Target;

        public float CurrentCellProgress = 0.0f;
        public int CurrentCellID = 0;
        private ShanghaiConfig _Config;

        public ActiveMission(List<IntVect2> path, Source source, Target target) {
            Path = path;
            Source = source;
            Target = target;
            _Config = ShanghaiConfig.Instance;
        }

        public bool Progress(float progress) {
            CurrentCellProgress += progress;
            Messenger<IntVect2, float>.Broadcast(EVENT_CELL_PROGRESSED, Path[CurrentCellID], CurrentCellProgress);

            if (CurrentCellProgress >= 1.0f) {
                CurrentCellProgress = 0.0f;
                CurrentCellID++;

                if (CurrentCellID >= (Path.Count - 1)) {
                    return true;
                }
            }
            return false;
        }
    }
}
