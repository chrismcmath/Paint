using UnityEngine;
using System.Collections;

using Shanghai.Model;

namespace Shanghai.Model {
    public class Cell {
        public static readonly string EVENT_CELL_UPDATED = "EVENT_CELL_UPDATED";

        public IntVect2 Key;
        public bool Selected = false;

        public enum PipeType {NONE=0, HORI, VERT, NE, NW, SE, SW, LEFT, RIGHT, TOP, BOTTOM};
        public enum CellState {EMPTY=0, SOURCE, TARGET, DEAD};

        private PipeType _Pipe = PipeType.NONE;
        public PipeType Pipe {
            get { return _Pipe; }
            set {
                _Pipe = value;
            }
        }

        private Target _Target = null;
        public Target Target {
            get { return _Target; }
            set {
                if (_Target != value) {
                    _Target = value;
                }
                State = CellState.TARGET;
            }
        }

        private Source _Source = null;
        public Source Source {
            get { return _Source; }
            set {
                if (_Source != value) {
                    _Source = value;
                }
                State = CellState.SOURCE;
            }
        }

        public float Progress = 0.0f;
        public float TotalProgress = 0.0f;

        public CellState State = CellState.EMPTY;

        public Cell(IntVect2 key) {
            Key = key;
        }

        public void Reset() {
            Target = null;
            Source = null;
            Progress = 0.0f;
            TotalProgress = 0.0f;
            State = CellState.EMPTY;
            Pipe = PipeType.NONE;
        }

        public void KillCell() {
            Reset();
            State = Cell.CellState.DEAD;
        }

        public bool IsFree() {
            return Pipe == PipeType.NONE && State == CellState.EMPTY;
        }

        public bool HasMission() {
            return Target != null || Source != null;
        }
    }
}
