using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Shanghai.Model {
    public class Grid {
        public static readonly string EVENT_SET_PATH = "EVENT_SET_PATH";
        public static readonly string EVENT_GRID_UPDATED = "EVENT_GRID_UPDATED";
        public static readonly string EVENT_MISSION_FAILED = "EVENT_MISSION_FAILED";

        private List<List<Cell>> _Cells = new List<List<Cell>>();
        public List<List<Cell>> Cells {
            get { return _Cells; }
        }

        public Cell GetCell(IntVect2 key) {
            return _Cells[key.y][key.x];
        }

        public bool GetRandomCell(ref IntVect2 key) {
            List<Cell> availableCells = new List<Cell>();
            foreach (List<Cell> row in _Cells) {
                foreach (Cell cell in row) {
                    if (!cell.HasMission() && cell.IsFree()) {
                        availableCells.Add(cell);
                    }
                }
            }

            if (availableCells.Count < 1) {
                return false;
            } else {
                key = availableCells.ElementAt(Random.Range(0, availableCells.Count)).Key;
                return true;
            }
        }

        public void ResetCellsInPath (List<IntVect2> path) {
            foreach (IntVect2 cellKey in path) {
                Cell cell = GetCell(cellKey);
                cell.Reset();
            }
            Messenger<List<List<Cell>>>.Broadcast(EVENT_GRID_UPDATED, _Cells);
        }
    }
}
