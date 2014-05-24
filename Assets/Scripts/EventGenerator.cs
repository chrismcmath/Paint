using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Shanghai.Entities;
using Shanghai.Grid;

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

            ShanghaiUtils.PaintColour targetColour = ShanghaiUtils.GetRandomColour(_Model.AvailableColours); 
            float TTL = _Config.TargetWaitTime;
            Target target = new Target(cellKey, targetColour, TTL);
            Messenger<Target>.Broadcast(EVENT_TARGET_CREATED, target);
            return true;
        }

        public bool GenerateSource() {
            IntVect2 cellKey = new IntVect2(0,0);
            if (!_Model.Grid.GetRandomCell(ref cellKey)) {
                return false;
            }

            float TTL = _Config.SourceWaitTime;

            Source source = new Source(cellKey, TTL);
            Messenger<Source>.Broadcast(EVENT_SOURCE_CREATED, source);
            return true;
        }
    }
}

