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
            if (cellKey == _CurrentCell) return;
            _CurrentCell = cellKey;

            if (_GridModelController.ValidateCellInput(cellKey, _Model.Path)){
                _Model.Path.Add(cellKey);
                FreezeTarget(cellKey);
                if (ShanghaiUtils.IsEndPoint(_Model.Grid.GetCell(cellKey))) {
                    _Model.CanDraw = false;
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

            NextTurn();
        }

        private void SetPath() {
            if (_Model.Path.Count < 1) {
                return;
            }

            List<IntVect2> path = PrunePath(_Model.Path);
            if (path.Count < 2) {
                Cell firstCell = _GridModelController.GetCell(_Model.Path[0]);
                _GridModelController.KillCell(firstCell);
                return;
            }

            Cell startCell = _GridModelController.GetCell(path[0]);
            Cell endCell = _GridModelController.GetCell(path[path.Count-1]);

            if (startCell.Source.PaintColour != endCell.Target.PaintColour) {
                Debug.LogError("This can't be happening!!!");
            } else {
                _Model.AddActiveMission(new ActiveMission(path, startCell.Source, endCell.Target));
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

                    cell.KillCell();
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
            foreach (ActiveMission actMiss in _Model.ActiveMissions) {
                if (actMiss.Progress()) {
                    actMiss.Path.Reverse();
                    float interval = 0.05f;
                    int cumulativePoints = 1;
                    foreach (IntVect2 cellKey in actMiss.Path) {
                        Cell cell = _GridModelController.GetCell(cellKey);
                        if (cell.Target != null) {
                            cumulativePoints *= 2;
                        }
                        int cellPoints = cell.Colour == actMiss.Source.PaintColour ? cumulativePoints * 2 : cumulativePoints;
                        StartCoroutine(PaintCell(interval, cell, actMiss.Source.PaintColour, cellPoints));
                        interval += 0.1f;
                    }
                    StartCoroutine(RemoveActiveMission(interval, actMiss));
                }
            }
        }

        public void UpdateActiveMissions(float delta) {
            foreach (ActiveMission actMiss in _Model.ActiveMissions) {
                if (actMiss.Progress(delta * _Config.CellFillPerSecond)) {
                    actMiss.Path.Reverse();
                    float interval = 0.1f;
                    int cumulativePoints = 1;
                    foreach (IntVect2 cellKey in actMiss.Path) {
                        Cell cell = _GridModelController.GetCell(cellKey);
                        if (cell.Target != null) {
                            cumulativePoints *= 2;
                        }
                        int cellPoints = cell.Colour == actMiss.Source.PaintColour ? cumulativePoints * 2 : cumulativePoints;
                        StartCoroutine(PaintCell(interval, cell, actMiss.Source.PaintColour, cellPoints));
                        interval += 0.1f;
                    }
                    StartCoroutine(RemoveActiveMission(interval, actMiss));
                }
            }
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
            Debug.Log("RegenerateSurroundingCells");
            IntVect2 cellKey = cell.Key;
            AddCellByDeviation(cell.Key, new IntVect2(-1, 0));
            AddCellByDeviation(cell.Key, new IntVect2(1, 0));
            AddCellByDeviation(cell.Key, new IntVect2(0, -1));
            AddCellByDeviation(cell.Key, new IntVect2(0, 1));
        }

        private void AddCellByDeviation(IntVect2 key, IntVect2 deviation) {
            IntVect2 newKey = new IntVect2(key.x + deviation.x, key.y + deviation.y);
            if (newKey.x >= 0 && newKey.x < ShanghaiConfig.Instance.GridSize &&
                    newKey.y >= 0 && newKey.y < ShanghaiConfig.Instance.GridSize) {
                Cell adjacentCell = _GridModelController.GetCell(newKey);
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
            _Generator.GenerateSource();
            _Generator.GenerateTarget();
            _Generator.GenerateTarget();

            _Model.ChangeColour();
        }

        public void GameLoop() {
        }

        public void RealTimeGameLoop() {
            _CurrentTime += Time.deltaTime;
            _ColourInterval -= Time.deltaTime;
            _SourceInterval -= Time.deltaTime;
            _TargetInterval -= Time.deltaTime;
            DebugLabel.text = string.Format("Clock:{0:00}\nColour:{1:00}\nSource:{2:00}\nTarget:{3:00}", _CurrentTime, _ColourInterval, _SourceInterval, _TargetInterval);

            /*
            if (_ColourInterval <= 0.0f) {
                _ColourInterval = _Config.ColourInterval;
                UpdateSources();
                _Model.ChangeColour();
            }
            if (_SourceInterval <= 0.0f) {
                _SourceInterval = _Config.SourceInterval;
                _Generator.GenerateSource();
            }
            if (_TargetInterval <= 0.0f) {
                _TargetInterval = _Config.TargetInterval;
                _Generator.GenerateTarget();
            }
            */

            UpdateActiveMissions(Time.deltaTime);

            TickTargets(Time.deltaTime);
            /*
            ReplinishTargets(Time.deltaTime);
            DrainClients(Time.deltaTime);
            TickMissions(Time.deltaTime);

            CheckForEndGame();
            */
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
    }
}
