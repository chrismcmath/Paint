using UnityEngine;
using System.Collections;

using Shanghai.Grid;

namespace Shanghai.Grid {
    public class PlayableCell : GridCell {
        public static readonly string EVENT_CELL_UPDATED = "EVENT_CELL_UPDATED";

        public enum PipeType {NONE=0, HORI, VERT, NE, NW, SE, SW, LEFT, RIGHT, TOP, BOTTOM};
        public enum CellState {LIVE=0, DEAD};

        private PipeType _Pipe = PipeType.NONE;
        public PipeType Pipe {
            get { return _Pipe; }
            set {
                _Pipe = value;
            }
        }

        public Target Target = null;
        public Source Source = null;

        public float Progress = 0.0f;
        public float TotalProgress = 0.0f;

        public CellState State = CellState.LIVE;

        public PlayableCell(IntVect2 key) : base(key) {
        }

        public void Reset() {
            Target = null;
            Source = null;
            Progress = 0.0f;
            TotalProgress = 0.0f;
            Pipe = PipeType.NONE;
        }

        public bool IsFree() {
            return Pipe == PipeType.NONE && State != CellState.DEAD;
        }

        public bool HasMission() {
            return Target != null || Source != null;
        }
    }
}
