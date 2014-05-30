using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Shanghai.Model;
using Shanghai.ViewControllers;

namespace Shanghai.ModelControllers {
    public class ModelController : MonoBehaviour {
        public static readonly string EVENT_RESET_COLOUR_INTERVAL = "EVENT_RESET_COLOUR_INTERVAL";
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
        private float _ColourInterval = 0.0f;
        private float _SourceInterval = 0.0f;
        private float _TargetInterval = 0.0f;

        public void Awake() {
            _Config = ShanghaiConfig.Instance;
            _Generator = new EventGenerator();

            Messenger<IntVect2>.AddListener(CellController.EVENT_CELL_DRAGGED, OnCellDragged);
            Messenger<IntVect2>.AddListener(CellController.EVENT_CELL_CLICKED, OnCellClicked);
            Messenger.AddListener(CellController.EVENT_CELL_DRAG_END, OnCellDragEnd);
            Messenger.AddListener(ModelController.EVENT_RESET_COLOUR_INTERVAL, OnResetColourInterval);
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
            Messenger.RemoveListener(ModelController.EVENT_RESET_COLOUR_INTERVAL, OnResetColourInterval);
            Messenger.RemoveListener(Shanghai.EVENT_SKIP_GO, OnSkipGo);
        }

        private void OnCellClicked(IntVect2 cellKey) {
            Cell cell = _Model.Grid.GetCell(cellKey);
            if (cell != null &&
                cell.Source != null &&
                cell.Source.PaintColour == ShanghaiUtils.PaintColour.NONE) {

                cell.Source.PaintColour = _Model.PaintColour;
                Messenger<Cell>.Broadcast(Cell.EVENT_CELL_UPDATED, cell);
                Messenger.Broadcast(EVENT_RESET_COLOUR_INTERVAL);
            }
        }

        private void OnCellDragged(IntVect2 cellKey) {
            //NOTE: Ignore if we're still dragging on the same cell
            //      or all subsequent drags from the dragged cell after the first
            if (cellKey == _CurrentCell || cellKey == _FirstCell) return;
            _CurrentCell = cellKey;
                Debug.Log("DRAG, first: " + _FirstCell + " current: " + _CurrentCell);

            if (_GridModelController.ValidateCellInput(cellKey, _Model.Path)){
                _Model.Path.Add(cellKey);
                FreezeTarget(cellKey);
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
            if (_Model.Path.Count < 1) {
                return;
            }

            List<IntVect2> path = PrunePath(_Model.Path);
            if (path.Count < 2) {
                Cell firstCell = _GridModelController.GetCell(_Model.Path[0]);
                KillCell(firstCell);
                return;
            }

            Cell startCell = _GridModelController.GetCell(path[0]);
            Cell endCell = _GridModelController.GetCell(path[path.Count-1]);

            if (startCell.Target.PaintColour != endCell.Target.PaintColour) {
                Debug.LogError("This can't be happening!!!");
            } else {
                _Model.AddActiveMission(new ActiveMission(path, startCell.Target.PaintColour));
            }
        }

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

        public void TickTargets() {
            List<Target> garbage = new List<Target>();
            foreach (Target target in _Model.Targets) {
                Cell cell = _Model.Grid.GetCell(target.CellKey);
                if (!target.Freeze && target.IsTTD()) {
                    garbage.Add(target);

                    ExplodeCell(cell);
                }
                Messenger<Cell>.Broadcast(Cell.EVENT_CELL_UPDATED, cell);
            }

            /* Garbage collection */
            foreach (Target target in garbage) {
                _Model.Targets.Remove(target);
            }
        }

        public void TickTargets (float delta) {
            List<Target> garbage = new List<Target>();
            foreach (Target target in _Model.Targets) {
                Cell cell = _Model.Grid.GetCell(target.CellKey);
                if (!target.Freeze && target.IsTTD(delta)) {
                    garbage.Add(target);

                    cell.KillCell();
                }
                Messenger<Cell>.Broadcast(Cell.EVENT_CELL_UPDATED, cell);
            }

            /* Garbage collection */
            foreach (Target target in garbage) {
                _Model.Targets.Remove(target);
            }
        }

        public void UpdateActiveMissions() {
            List<ActiveMission> garbage = new List<ActiveMission>();

            foreach (ActiveMission actMiss in _Model.ActiveMissions) {
                PaintCell(actMiss.Path[0], actMiss.PaintColour);
                if (actMiss.Progress()) {
                    garbage.Add(actMiss);
                    PaintCell(actMiss.Path[0], actMiss.PaintColour);
                    _Model.Point += actMiss.Points * actMiss.PointsModifier;

                    /*Disable bombs*/
                    List<IntVect2> surroundingCells = ShanghaiUtils.GetLegitimateSurroundingCells(actMiss.Path[0]);
                    foreach (IntVect2 cellKey in surroundingCells) {
                        Cell adjCell = _GridModelController.GetCell(cellKey);
                        if (adjCell.IsDead()) {
                            adjCell.Reset();
                            Messenger<Cell>.Broadcast(Cell.EVENT_CELL_UPDATED, adjCell);
                        } else if (adjCell.Target != null && !adjCell.Target.Freeze) {
                            adjCell.Reset();
                            Messenger<Cell>.Broadcast(Cell.EVENT_CELL_UPDATED, adjCell);
                        }
                    }

                    /*
                    actMiss.Path.Reverse();
                    int cumulativePoints = 1;
                    foreach (IntVect2 cellKey in actMiss.Path) {
                        Cell cell = _GridModelController.GetCell(cellKey);
                        if (cell.Target != null) {
                            cumulativePoints *= 2;
                        }
                        int cellPoints = cell.Colour == actMiss.PaintColour ? cumulativePoints * 2 : cumulativePoints;
                        StartCoroutine(PaintCell(interval, cell, actMiss.PaintColour, cellPoints));
                        interval += 0.1f;
                    }
                    StartCoroutine(RemoveActiveMission(interval, actMiss));
                    */
                } else {
                    //accumulate
                    if (_GridModelController.GetCell(actMiss.CurrentCell).Colour == actMiss.PaintColour) {
                        actMiss.Points += 1;
                    } else if (_GridModelController.GetCell(actMiss.CurrentCell).Target != null) {
                        actMiss.PointsModifier += 1;
                    }
                    Cell cell = _GridModelController.GetCell(actMiss.Path[0]);
                    cell.SetNodePoints(actMiss.Points, actMiss.PointsModifier);
                    Messenger<Cell>.Broadcast(Cell.EVENT_CELL_UPDATED, cell);
                }
            }

            /* Garbage collection */
            foreach (ActiveMission actMiss in garbage) {
                _Model.ActiveMissions.Remove(actMiss);
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
            RegenerateSurroundingCells(cell);
            Messenger<Cell>.Broadcast(Cell.EVENT_CELL_UPDATED, cell);
        }


        private IEnumerator RemoveActiveMission(float waitTime, ActiveMission actMiss) {
            yield return new WaitForSeconds(waitTime);
            _Model.RemoveActiveMission(actMiss);
        }

        private void RegenerateSurroundingCells(Cell cell) {
            List<IntVect2> surroundingCells = ShanghaiUtils.GetLegitimateSurroundingCells(cell.Key);
            foreach (IntVect2 cellKey in surroundingCells) {
                Cell adjacentCell = _GridModelController.GetCell(cellKey);
                if (adjacentCell.State == Cell.CellState.DEAD) {
                    _GridModelController.ResetCell(adjacentCell);
                }
            }
        }

        // Get two new targets and one new source each round
        // Game over when grid is full
        public void NextTurn() {
            UpdateActiveMissions();
            TickTargets();

            /* spawn */
            //_Generator.GenerateSource();
            for (int i = 0; i < ShanghaiConfig.Instance.TargetsPerTurn; i++) {
                _Generator.GenerateTarget();
            }

            _Model.ChangeColour();
        }

        private void UpdateSources() {
            foreach (Source source in _Model.Sources) {
                if (!source.Locked) {
                    source.PaintColour = _Model.PaintColour;
                    Cell cell = _GridModelController.GetCell(source.CellKey);
                    Messenger<Cell>.Broadcast(Cell.EVENT_CELL_UPDATED, cell);
                }
            }
        }

        private void OnResetColourInterval() {
            _ColourInterval = 0.0f;
        }

        public void ExplodeCell(Cell cell) {
            List<IntVect2> surroundingCells = ShanghaiUtils.GetLegitimateSurroundingCells(cell.Key);
            foreach (IntVect2 cellKey in surroundingCells) {
                StartCoroutine(KillCellAfterWait(_GridModelController.GetCell(cellKey)));
            }
            KillCell(cell);
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
