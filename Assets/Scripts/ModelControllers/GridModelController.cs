using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Shanghai.Model;

namespace Shanghai.ModelControllers {
    public class GridModelController {
        public static readonly string EVENT_SET_PATH = "EVENT_SET_PATH";
        public static readonly string EVENT_GRID_UPDATED = "EVENT_GRID_UPDATED";
        public static readonly string EVENT_MISSION_FAILED = "EVENT_MISSION_FAILED";

        private Grid _Grid = null;

        public GridModelController() {
            _Grid = GameModel.Instance.Grid;
            BuildGrid();
        }

        private void BuildGrid() {
            for (int y = 0; y < ShanghaiConfig.Instance.GridSize; y++) {
                List<Cell> row = new List<Cell>();
                for (int x = 0; x < ShanghaiConfig.Instance.GridSize; x++) {
                    row.Add(new Cell(new IntVect2(x,y)));
                }
                _Grid.Cells.Add(row);
            }
        }

        public void ResetAllCells(bool silent) {
            foreach (List<Cell> row in _Grid.Cells) {
                foreach (Cell cell in row) {
                    cell.Reset();
                }
            }
            if (!silent) {
                Messenger<List<List<Cell>>>.Broadcast(EVENT_GRID_UPDATED, _Grid.Cells, MessengerMode.DONT_REQUIRE_LISTENER);
            }
        }

        public void ResetCell(IntVect2 cellKey) {
            Cell cell = GetCell(cellKey);
            ResetCell(cell);
        }

        public void ResetCell(Cell cell) {
            cell.Reset();
            Messenger<Cell>.Broadcast(Cell.EVENT_CELL_UPDATED, cell);
        }


        public bool ValidateCellInput(IntVect2 key, List<IntVect2> path) {
            Cell cell = GetCell(key);
            /* First point, can only be a valid source */ 
            if (path.Count == 0 &&
                   cell.Target != null) {
                GameModel.Instance.PathColour = cell.Target.PaintColour;
                cell.Target.Freeze = true;
                return true;
            } else if (CellIsConnected(key, path) &&
                    CanDrawOnCell(GetCell(key), GetCell(path[0]))) {
                cell.HasPath = true;
                Messenger<Cell>.Broadcast(Cell.EVENT_CELL_UPDATED, cell);
                return true;
            }
            return false;
        }

        public Cell GetCell(IntVect2 key) {
            return _Grid.Cells[key.y][key.x];
        }

        public void CellProgressed(IntVect2 cellKey, float progress) {
            Cell cell = GetCell(cellKey);
            cell.Progress = progress;
            Messenger<Cell>.Broadcast(Cell.EVENT_CELL_UPDATED, cell);
        }

        public void ResetCellsProgress(List<IntVect2> path) {
            foreach (IntVect2 cellKey in path) {
                Cell cell = GetCell(cellKey);
                cell.Progress = 0.0f;
            }
            Messenger<List<List<Cell>>>.Broadcast(EVENT_GRID_UPDATED, _Grid.Cells);
        }

        private bool CanDrawOnCell(Cell cell, Cell originCell) {
            return cell.IsFree() ||
                (cell.Target != null &&
                 cell.Target.PaintColour == originCell.Target.PaintColour);
        }

        private bool CellInPath(IntVect2 key, List<IntVect2> path) {
            foreach (IntVect2 cellKey in path) {
                if (cellKey == key) {
                    return true;
                }
            }
            return false;
        }

        private bool CellIsConnected(IntVect2 key, List<IntVect2> path) {
            if (path.Count < 1) {
                // nothing yet
            } else {
                IntVect2 prevKey = GetCell(path[path.Count - 1]).Key;
                if ((Mathf.Abs(key.x - prevKey.x) == 1 && (key.y == prevKey.y)) ||
                        (Mathf.Abs(key.y - prevKey.y) == 1 && (key.x == prevKey.x))) {
                    return true;
                }
            }
            return false;
        }
    }
}
