using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Shanghai.Model;
using Shanghai.ViewControllers;

namespace Shanghai.ModelControllers {
    public class ModelController : MonoBehaviour {
        public static readonly string EVENT_ACTIVE_MISSION_FINISHED = "EVENT_ACTIVE_MISSION_FINISHED";
        public static readonly string EVENT_POINTS_AWARDED = "EVENT_POINTS_AWARDED";

        private GameModel _Model;
        private GridModelController _GridModelController = null;

        private IntVect2 _FirstCell = null;
        private IntVect2 _CurrentCell = null;

        private ShanghaiConfig _Config;
        private EventGenerator _Generator;

        public UILabel DebugLabel;

        private float _CurrentTime;
        private float _SourceInterval = 0.0f;
        private float _TargetInterval = 0.0f;

        public void Awake() {
            _Config = ShanghaiConfig.Instance;
            _Generator = new EventGenerator();

            Messenger<IntVect2>.AddListener(CellController.EVENT_CELL_DRAGGED, OnCellDragged);
            Messenger<IntVect2>.AddListener(CellController.EVENT_CELL_CLICKED, OnCellClicked);
            Messenger.AddListener(CellController.EVENT_CELL_DRAG_END, OnCellDragEnd);
            Messenger.AddListener(Shanghai.EVENT_SKIP_GO, OnSkipGo);
        }

        public void Start() {
            _Model = GameModel.Instance;
            _GridModelController = new GridModelController();

            _Model.ChangeColour();
            NextTurn();
        }

        public void OnDestroy() {
            Messenger<IntVect2>.RemoveListener(CellController.EVENT_CELL_DRAGGED, OnCellDragged);
            Messenger<IntVect2>.RemoveListener(CellController.EVENT_CELL_CLICKED, OnCellClicked);
            Messenger.RemoveListener(CellController.EVENT_CELL_DRAG_END, OnCellDragEnd);
            Messenger.RemoveListener(Shanghai.EVENT_SKIP_GO, OnSkipGo);
        }

        private void OnCellClicked(IntVect2 cellKey) {
            //NOTE: Looks like this isn't used at all, trying without it
        }

        private void OnCellDragged(IntVect2 cellKey) {
            //NOTE: Ignore if we're still dragging on the same cell
            //      or all subsequent drags from the dragged cell after the first
            if (cellKey == _CurrentCell || cellKey == _FirstCell) return;

            _CurrentCell = cellKey;
            //Debug.Log("DRAG, first: " + _FirstCell + " current: " + _CurrentCell);

            if (_GridModelController.ValidateCellInput(cellKey, _Model.Path)) {
                _Model.Path.Add(cellKey);
                //FreezeTarget(cellKey); //to prevent target destruction?
                if (_Model.Path.Count == 1) {
                    _FirstCell = cellKey;
                }
            }
        }

        private void OnSkipGo() {
            NextTurn();
        }

        private void FreezeTarget(IntVect2 cellKey) {
            Cell cell = _GridModelController.GetCell(cellKey);
            if (cell.Target != null) {
                cell.Target.Freeze = true;
            }
        }

        private void OnCellDragEnd() {
            SetPath();
            _Model.ResetPath();

            _CurrentCell = null;
            _FirstCell = null;

            NextTurn();
        }

        private void SetPath() {
            if (_Model.Path.Count < 2) {
                return;
            }

            //List<IntVect2> path = PrunePath(_Model.Path);
            /*
            if (path.Count < 2) {
                Cell firstCell = _GridModelController.GetCell(_Model.Path[0]);
                KillCell(firstCell);
                return;
            }
            */

            Cell startCell = _GridModelController.GetCell(_Model.Path[0]);
            //Cell endCell = _GridModelController.GetCell(path[path.Count-1]);

            _Model.AddActiveMission(new ActiveMission(_Model.Path, startCell.Target.PaintColour));
        }

        /* Removes any unconnected path after the last target 
           (i.e. snaps to the last connected target)
        */

        private List<IntVect2> PrunePath(List<IntVect2> originalPath) {
            List<IntVect2> prunedPath = new List<IntVect2>();
            
            List<IntVect2> reversedPath = new List<IntVect2>(originalPath);
            reversedPath.Reverse();
            bool targetFound = false;
            foreach (IntVect2 cellKey in reversedPath) {
                if (!targetFound) {
                    Cell cell = _GridModelController.GetCell(cellKey);
                    if (cell.Target != null) {
                        targetFound = true;
                        prunedPath.Add(cellKey);
                    } else {
                        _GridModelController.ResetCell(cell);
                    }
                } else {
                    prunedPath.Add(cellKey);
                }
            }

            prunedPath.Reverse();
            return prunedPath;
        }

        public void NextTurn() {
            CellChange[,] changes = new CellChange[
                ShanghaiConfig.Instance.GridSize,
                ShanghaiConfig.Instance.GridSize];

            EnactTargetChanges(changes);
            UpdateActiveMissions(changes);
            TickTargets(changes);

            //PrintChanges(changes);

            PerformChanges(changes);

            /* spawn */
            //_Generator.GenerateSource();
            for (int i = 0; i < ShanghaiConfig.Instance.TargetsPerTurn; i++) {
                _Generator.GenerateTarget();
            }

            _Model.ChangeColour();
        }

        public void EnactTargetChanges(CellChange[,] changes) {
            foreach (Target target in _Model.Targets) {
                Debug.Log("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
                Debug.Log("no targets: " + _Model.Targets.Count);
                Debug.Log("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
                Cell cell = _Model.Grid.GetCell(target.CellKey);

                if (!_GridModelController.CellHasMission(cell.Key) &&
                        !_GridModelController.CellIsAtEndOfMission(cell.Key)) {
                    target.Lives -= ShanghaiConfig.Instance.LifeDecPerTurn;
                }
            }
        }

        public void TickTargets(CellChange[,] changes) {
            List<Target> garbage = new List<Target>();
            foreach (Target target in _Model.Targets) {
                Cell cell = _Model.Grid.GetCell(target.CellKey);

                if (target.Lives <= ShanghaiConfig.Instance.TargetMin) {
                    garbage.Add(target);
                    //NOTE: only destroy if not coloured
                    if (cell.Colour == ShanghaiUtils.PaintColour.NONE) {
                        ExplodeCell(cell, changes);
                    } else {
                        garbage.Add(target);
                        cell.Reset();
                    }
                } else if (target.Lives >= ShanghaiConfig.Instance.TargetMax) {
                    PaintCell(cell.Key, target.PaintColour);
                    garbage.Add(target);
                }

                Messenger<Cell>.Broadcast(Cell.EVENT_CELL_UPDATED, cell);
            }

            /* Garbage collection */
            foreach (Target target in garbage) {
                _Model.Targets.Remove(target);
            }
        }

        public void UpdateActiveMissions(CellChange[,] changes) {
            List<ActiveMission> garbage = new List<ActiveMission>();

            foreach (ActiveMission actMiss in _Model.ActiveMissions) {
                Cell cell = _GridModelController.GetCell(actMiss.Path[0]);
                Cell nextCell = _GridModelController.GetCell(actMiss.Path[1]);
                if (nextCell.Target != null) {
                    nextCell.Target.Lives += cell.Target.Lives;
                    _Model.Targets.Remove(cell.Target);
                } else {
                    nextCell.Target = cell.Target;
                    nextCell.Target.CellKey = nextCell.Key;
                }
                cell.Reset();
                Messenger<Cell>.Broadcast(Cell.EVENT_CELL_UPDATED, cell);
                Messenger<Cell>.Broadcast(Cell.EVENT_CELL_UPDATED, nextCell);
                if (actMiss.Progress()) {
                    garbage.Add(actMiss);
                    //_Model.Point += actMiss.Points * actMiss.PointsModifier;

                    changes[actMiss.Path[0].x, actMiss.Path[0].y] = CellChange.ACCOMPLISHED;

                    RegenerateSurroundingCells(_GridModelController.GetCell(actMiss.Path[0]), changes);
                    /*Disable bombs*/

                    /*
                    List<IntVect2> surroundingCells = ShanghaiUtils.GetLegitimateSurroundingCells(actMiss.Path[0]);
                    foreach (IntVect2 cellKey in surroundingCells) {
                        Cell adjCell = _GridModelController.GetCell(cellKey);
                        /* this should be the regenerate thing
                        if (adjCell.IsDead()) {
                            Debug.Log("reset dead tile " + adjCell.Key);
                            //adjCell.Reset();
                            //Messenger<Cell>.Broadcast(Cell.EVENT_CELL_UPDATED, adjCell);

                        if (adjCell.Target != null && !adjCell.Target.Freeze) {
                            Debug.Log("defuse bomb " + adjCell.Key);
                            changes[cellKey.x, cellKey.y] = CellChange.DEFUSED;
                            //_Model.Targets.Remove(adjCell.Target);
                            //adjCell.Reset();
                            //Messenger<Cell>.Broadcast(Cell.EVENT_CELL_UPDATED, adjCell);
                        } else if (adjCell.IsDead()) {
                            changes[cellKey.x, cellKey.y] = CellChange.RESTORED;
                        } else {
                            changes[cellKey.x, cellKey.y] = CellChange.PROTECTED;
                        }
                    }
                    */

                } else {
                    //accumulate
                    /*
                    actMiss.Points += 1;
                    if (_GridModelController.GetCell(actMiss.CurrentCell).Colour == actMiss.PaintColour) {
                        actMiss.PointsModifier += 1;
                    } else if (_GridModelController.GetCell(actMiss.CurrentCell).Target != null) {
                        actMiss.PointsModifier += 1;
                    }
                    Cell cell = _GridModelController.GetCell(actMiss.Path[0]);
                    cell.SetNodePoints(actMiss.Points, actMiss.PointsModifier);
                    Messenger<Cell>.Broadcast(Cell.EVENT_CELL_UPDATED, cell);
                    */
                }
            }

            /* Garbage collection */
            foreach (ActiveMission actMiss in garbage) {
                _Model.ActiveMissions.Remove(actMiss);
            }
        }

        private void PrintChanges(CellChange[,] changes) {
            string line = "";

            for (int i = 0; i < changes.GetLength(0); i++) {
                for (int j = 0; j < changes.GetLength(1); j++) {
                    line += GetCellChangeAbrv(changes[j, i]);
                }
                line += "\n";
            }
            Debug.Log(line);
        }

        private void PerformChanges(CellChange[,] changes) {
            for (int i = 0; i < changes.GetLength(0); i++) {
                for (int j = 0; j < changes.GetLength(1); j++) {
                    switch (changes[j,i]) {
                        case CellChange.NONE:
                            break;
                        case CellChange.BOMB:
                            KillCell(_GridModelController.GetCell(new IntVect2(j, i)));
                            break;
                        case CellChange.DEFUSED:
                            _GridModelController.ResetCell(_GridModelController.GetCell(new IntVect2(j, i)));
                            break;
                        case CellChange.COLLATERAL:
                            KillCell(_GridModelController.GetCell(new IntVect2(j, i)));
                            break;
                        case CellChange.RESTORED:
                            _GridModelController.ResetCell(_GridModelController.GetCell(new IntVect2(j, i)));
                            break;
                        case CellChange.PROTECTED:
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void PaintCell(IntVect2 cellKey, ShanghaiUtils.PaintColour colour) {
            Cell cell = _GridModelController.GetCell(cellKey);
            cell.Reset();
            cell.Colour = colour;
            Messenger<Cell>.Broadcast(Cell.EVENT_CELL_UPDATED, cell);
        }

        private IEnumerator PaintCell(float waitTime, Cell cell, ShanghaiUtils.PaintColour colour, int points) {
            yield return new WaitForSeconds(waitTime);
            cell.Reset();
            cell.Colour = colour;
            _Model.Point += points;
            Messenger<int, Cell>.Broadcast(ModelController.EVENT_POINTS_AWARDED, points, cell);
            //RegenerateSurroundingCells(cell);
            Messenger<Cell>.Broadcast(Cell.EVENT_CELL_UPDATED, cell);
        }


        private IEnumerator RemoveActiveMission(float waitTime, ActiveMission actMiss) {
            yield return new WaitForSeconds(waitTime);
            _Model.RemoveActiveMission(actMiss);
        }

        private void RegenerateSurroundingCells(IntVect2 cellKey) {
            //RegenerateSurroundingCells(_GridModelController.GetCell(cellKey));
        }

        private void RegenerateSurroundingCells(Cell cell, CellChange[,] changes) {
            List<IntVect2> surroundingCells = ShanghaiUtils.GetLegitimateSurroundingCells(cell.Key);
            Debug.Log("regenerate the cells around " + cell.Key);
            foreach (IntVect2 cellKey in surroundingCells) {
                Debug.Log("regenerating " + cellKey);
                Cell adjacentCell = _GridModelController.GetCell(cellKey);
                if (adjacentCell.State == Cell.CellState.DEAD) {
                    changes[cellKey.x, cellKey.y] = CellChange.RESTORED;
                }
            }
        }

        // Get two new targets and one new source each round
        // Game over when grid is full
        public enum CellChange {NONE=0,ACCOMPLISHED,BOMB,DEFUSED,COLLATERAL,RESTORED,PROTECTED};
        public static string GetCellChangeAbrv(CellChange change) {
            switch (change) {
                case CellChange.NONE:
                    return "N";
                case CellChange.ACCOMPLISHED:
                    return "A";
                case CellChange.BOMB:
                    return "B";
                case CellChange.DEFUSED:
                    return "D";
                case CellChange.COLLATERAL:
                    return "C";
                case CellChange.RESTORED:
                    return "R";
                case CellChange.PROTECTED:
                    return "P";
                default:
                    return "U";
            }
        }

        public void ExplodeCell(Cell cell, CellChange[,] changes) {
            if (changes[cell.Key.x, cell.Key.y] == CellChange.DEFUSED) return;

            //NOTE: no splash damage
            /*
            List<IntVect2> surroundingCells = ShanghaiUtils.GetLegitimateSurroundingCells(cell.Key);
            foreach (IntVect2 cellKey in surroundingCells) {
                //KillCell(_GridModelController.GetCell(cellKey));
                if (changes[cellKey.x, cellKey.y] == CellChange.NONE) {
                    changes[cellKey.x, cellKey.y] = CellChange.COLLATERAL;
                }
            }
            */
            changes[cell.Key.x, cell.Key.y] = CellChange.BOMB;
            //KillCell(cell);
        }

        private IEnumerator KillCellAfterWait(Cell cell) {
            yield return new WaitForSeconds(0.1f);
            KillCell(cell);
        }

        public void KillCell(Cell cell) {
            // Removing mission will reset the squares, so kill after
            CheckActiveMissionDestroyed(cell);
            cell.KillCell();

            Messenger<Cell>.Broadcast(Cell.EVENT_CELL_UPDATED, cell);
        }

        private void CheckActiveMissionDestroyed(Cell cell) {
            List<ActiveMission> toDestroy = new List<ActiveMission>();
            foreach (ActiveMission actMiss in _Model.ActiveMissions) {
                //Debug.Log("check any active missions destroyed, key: " + cell.Key);
                foreach (IntVect2 point in actMiss.Path) {
                    //Debug.Log("point: " + point);
                }
                if (ShanghaiUtils.PathContainsPoint(actMiss.Path, cell.Key)) {
                    toDestroy.Add(actMiss);
                }
            }

            foreach (ActiveMission actMiss in toDestroy) {
                _Model.RemoveActiveMission(actMiss);
            }
        }
    }
}
