using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Shanghai.Model;

namespace Shanghai {
    public class EventGenerator {
        public static readonly string EVENT_COLOUR_CHANGED = "EVENT_COLOUR_CHANGED";
        public static readonly string EVENT_SOURCE_CREATED = "EVENT_SOURCE_CREATED";
        public static readonly string EVENT_TARGET_CREATED = "EVENT_TARGET_CREATED";

        private GameModel _Model;
        private ShanghaiConfig _Config;

        public EventGenerator() {
            _Config = ShanghaiConfig.Instance;
            _Model = GameModel.Instance;
        }

        public bool GenerateTarget() {
            IntVect2 cellKey = new IntVect2(0,0);
            if (!_Model.Grid.GetRandomCell(ref cellKey)) {
                return false;
            }

            Target target = new Target(cellKey, _Model.PaintColour, _Config.TargetWaitTime);
            Messenger<Target>.Broadcast(EVENT_TARGET_CREATED, target);
            return true;
        }
    }
}

