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
                    cell.Source != null &&
                    cell.Source.PaintColour != ShanghaiUtils.PaintColour.NONE) {
                GameModel.Instance.PathColour = cell.Source.PaintColour;
                cell.Source.Locked = true;
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

        public void KillCell (Cell cell) {
            cell.KillCell();
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
            Debug.Log("cell: " + cell + " origin: " + originCell);
            return cell.IsFree() ||
                (cell.Target != null &&
                 cell.Target.PaintColour == originCell.Source.PaintColour);
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

        /*
        private void UpdateCellPipeType(Cell cell, List<IntVect2> path) {
        //TODO: Left is coming from chance card
            if (path.Count < 1) {
                cell.Pipe = Cell.PipeType.TOP;
                return;
            }
            Cell prevCell = GetCell(path[path.Count - 1]);

            if (prevCell.Key.x < cell.Key.x) {
                cell.Pipe = Cell.PipeType.LEFT;
            } else if (prevCell.Key.x > cell.Key.x) {
                cell.Pipe = Cell.PipeType.RIGHT;
            } else if (prevCell.Key.y < cell.Key.y) {
                cell.Pipe = Cell.PipeType.TOP;
            } else if (prevCell.Key.y > cell.Key.y) {
                cell.Pipe = Cell.PipeType.BOTTOM;
            }

            UpdatePrevCell(prevCell, cell);
        }

        private void UpdatePrevCell(Cell prevCell, Cell cell) {
            if (prevCell.Pipe == cell.Pipe) {
                prevCell.Pipe = (prevCell.Pipe == Cell.PipeType.LEFT ||
                        prevCell.Pipe == Cell.PipeType.RIGHT) ?  
                    Cell.PipeType.HORI  : Cell.PipeType.VERT;
            }
            switch (prevCell.Pipe) {
                case Cell.PipeType.LEFT:
                    switch (cell.Pipe) {
                        case Cell.PipeType.TOP:
                            prevCell.Pipe = Cell.PipeType.SW;
                            break;
                        case Cell.PipeType.BOTTOM:
                            prevCell.Pipe = Cell.PipeType.NW;
                            break;
                    }
                    break;
                case Cell.PipeType.RIGHT:
                    switch (cell.Pipe) {
                        case Cell.PipeType.TOP:
                            prevCell.Pipe = Cell.PipeType.SE;
                            break;
                        case Cell.PipeType.BOTTOM:
                            prevCell.Pipe = Cell.PipeType.NE;
                            break;
                    }
                    break;
                case Cell.PipeType.TOP:
                    switch (cell.Pipe) {
                        case Cell.PipeType.LEFT:
                            prevCell.Pipe = Cell.PipeType.NE;
                            break;
                        case Cell.PipeType.RIGHT:
                            prevCell.Pipe = Cell.PipeType.NW;
                            break;
                    }
                    break;
                case Cell.PipeType.BOTTOM:
                    switch (cell.Pipe) {
                        case Cell.PipeType.LEFT:
                            prevCell.Pipe = Cell.PipeType.SE;
                            break;
                        case Cell.PipeType.RIGHT:
                            prevCell.Pipe = Cell.PipeType.SW;
                            break;
                    }
                    break;
                case Cell.PipeType.NONE:
                    Debug.Log("Prev cell cant be NONE");
                    break;
            }
            Messenger<Cell>.Broadcast(Cell.EVENT_CELL_UPDATED, prevCell);
        }
        */
    }
}
