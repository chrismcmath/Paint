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
        }

        public void Start() {
            _Model = GameModel.Instance;
            _GridModelController = new GridModelController();
        }

        public void OnDestroy() {
            Messenger<IntVect2>.RemoveListener(CellController.EVENT_CELL_DRAGGED, OnCellDragged);
            Messenger<IntVect2>.RemoveListener(CellController.EVENT_CELL_CLICKED, OnCellClicked);
            Messenger.RemoveListener(CellController.EVENT_CELL_DRAG_END, OnCellDragEnd);
            Messenger.RemoveListener(ModelController.EVENT_RESET_COLOUR_INTERVAL, OnResetColourInterval);
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

        private void FreezeTarget(IntVect2 cellKey) {
            Cell cell = _GridModelController.GetCell(cellKey);
            if (cell.Target != null) {
                cell.Target.Freeze = true;
            }
        }

        private void OnCellDragEnd() {
            SetPath();
            _Model.ResetPath();
        }

        private void SetPath() {
            if (_Model.Path.Count < 1) {
                return;
            }

            List<IntVect2> path = PrunePath(_Model.Path);
            if (path.Count < 2) {
                return;
            }

            Cell startCell = _Model.Grid.GetCell(path[0]);
            Cell endCell = _Model.Grid.GetCell(path[path.Count-1]);

            if (startCell.Source.PaintColour != endCell.Target.PaintColour) {
                Debug.LogError("This can't be happening!!!");
            } else {
                _Model.AddActiveMission(new ActiveMission(path, startCell.Source, endCell.Target));
            }
        }

        private List<IntVect2> PrunePath(List<IntVect2> originalPath) {
            List<IntVect2> prunedPath = new List<IntVect2>();
            
            originalPath.Reverse();
            bool targetFound = false;
            foreach (IntVect2 cellKey in originalPath) {
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
            _Model.Money += points;
            Messenger<Cell>.Broadcast(Cell.EVENT_CELL_UPDATED, cell);
        }


        private IEnumerator RemoveActiveMission(float waitTime, ActiveMission actMiss) {
            yield return new WaitForSeconds(waitTime);
            _Model.RemoveActiveMission(actMiss);
        }

        public void GameLoop() {
            _CurrentTime += Time.deltaTime;
            _ColourInterval -= Time.deltaTime;
            _SourceInterval -= Time.deltaTime;
            _TargetInterval -= Time.deltaTime;
            DebugLabel.text = string.Format("Clock:{0:00}\nColour:{1:00}\nSource:{2:00}\nTarget:{3:00}", _CurrentTime, _ColourInterval, _SourceInterval, _TargetInterval);

            if (_ColourInterval <= 0.0f) {
                _ColourInterval = _Config.ColourInterval;
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

            UpdateActiveMissions(Time.deltaTime);

            TickTargets(Time.deltaTime);
            /*
            ReplinishTargets(Time.deltaTime);
            DrainClients(Time.deltaTime);
            TickMissions(Time.deltaTime);

            CheckForEndGame();
            */
        }

        private void OnResetColourInterval() {
            _ColourInterval = 0.0f;
        }
    }
}
