using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Shanghai.Model;
using Shanghai.ViewControllers;

namespace Shanghai {
    public class GameModel : MonoSingleton<GameModel> {
        public static readonly string EVENT_MONEY_CHANGED = "EVENT_MONEY_CHANGED";
        public static readonly string EVENT_COLOUR_CHANGED = "EVENT_COLOUR_CHANGED";
        public static readonly string EVENT_SOURCE_CHANGED = "EVENT_SOURCE_CHANGED";

        public static readonly string EVENT_CLIENT_UPDATED = "EVENT_CLIENT_UPDATED";
        public static readonly string EVENT_TARGET_UPDATED = "EVENT_TARGET_UPDATED";

        public static readonly string EVENT_ACTIVE_MISSION_ADDED = "EVENT_ACTIVE_MISSION_ADDED";
        public static readonly string EVENT_ACTIVE_MISSION_REMOVED = "EVENT_ACTIVE_MISSION_REMOVED";

        public static readonly int GRID_SIZE = 6;

        private Grid _Grid;
        public Grid Grid {
            get { return _Grid; }
        }

        private ShanghaiUtils.PaintColour _PaintColour = ShanghaiUtils.PaintColour.NONE;
        public ShanghaiUtils.PaintColour PaintColour {
            get { return _PaintColour; }
        }

        private List<Source> _Sources = new List<Source>();
        public List<Source> Sources {
            get { return _Sources; }
        }

        private List<Target> _Targets = new List<Target>();
        public List<Target> Targets {
            get { return _Targets; }
        }

        private int _AvailableColours = 3;
        public int AvailableColours {
            get { return _AvailableColours; }
        }

        private List<ActiveMission> _ActiveMissions = new List<ActiveMission>();
        public List<ActiveMission> ActiveMissions {
            get { return _ActiveMissions; }
        }

        private bool _CanDraw = true;
        public bool CanDraw {
            get { return _CanDraw; }
            set { _CanDraw = value; }
        }

        private int _Money = 0;
        public int Money {
            get { return _Money; }
            set {
                if (value != _Money) {
                    _Money = value;
                    Messenger<int>.Broadcast(EVENT_MONEY_CHANGED, _Money);
                }
            }
        }

        private List<IntVect2> _Path = new List<IntVect2>();
        public List<IntVect2> Path {
            get { return _Path; }
        }

        public ShanghaiUtils.PaintColour PathColour = ShanghaiUtils.PaintColour.NONE;

        // Use to get position on screen given a particular key
        public Dictionary<IntVect2, Vector2> CellPositions = null;

        public void Awake() {
            Messenger<Source>.AddListener(EventGenerator.EVENT_SOURCE_CREATED, OnSourceCreated);
            Messenger<Target>.AddListener(EventGenerator.EVENT_TARGET_CREATED, OnTargetCreated);

            Messenger<IntVect2, float>.AddListener(ActiveMission.EVENT_CELL_PROGRESSED, OnCellProgressed);
            Messenger<List<IntVect2>, Source>.AddListener(ActiveMission.EVENT_PACKAGE_DELIVERED, OnPackageDelivered);
            Reset();
        }

        public void OnDestroy() {
            Messenger<Source>.RemoveListener(EventGenerator.EVENT_SOURCE_CREATED, OnSourceCreated);
            Messenger<Target>.RemoveListener(EventGenerator.EVENT_TARGET_CREATED, OnTargetCreated);

            Messenger<IntVect2, float>.RemoveListener(ActiveMission.EVENT_CELL_PROGRESSED, OnCellProgressed);
            Messenger<List<IntVect2>, Source>.RemoveListener(ActiveMission.EVENT_PACKAGE_DELIVERED, OnPackageDelivered);
        }

        public void Reset() {
            _Grid = new Grid();
            //_Grid.ResetAllCells(true);

            _ActiveMissions = new List<ActiveMission>();
            Money = 0;
            _CanDraw = true;
            
        }

        public void ResetPath() {
            _Path = new List<IntVect2>();
        }


        //NOTE: NOT WORKING Timing issue here, need to update after Controllers have initialized
        private IEnumerator UpdateGridController() {
            yield return new WaitForEndOfFrame();
            Messenger<List<List<Cell>>>.Broadcast(Grid.EVENT_GRID_UPDATED, _Grid.Cells);
        }

        public void AddActiveMission(ActiveMission actMiss) {
            ActiveMissions.Add(actMiss);
            Messenger<ActiveMission>.Broadcast(EVENT_ACTIVE_MISSION_ADDED, actMiss);
        }

        public void RemoveActiveMission(ActiveMission actMiss) {
            _Grid.ResetCellsInPath(actMiss.Path);
            // remove source and target
            _ActiveMissions.Remove(actMiss);
            Messenger<ActiveMission>.Broadcast(EVENT_ACTIVE_MISSION_REMOVED, actMiss);
        }

        //TODO: (CM) Should this be here? It's more like behaviour
        public void ChangeColour() {
            _PaintColour = ShanghaiUtils.GetRandomColour(_AvailableColours);
            Messenger<ShanghaiUtils.PaintColour>.Broadcast(EVENT_COLOUR_CHANGED, _PaintColour);
        }

        private void OnTargetCreated(Target target) {
            Targets.Add(target);
            Cell cell = _Grid.GetCell(target.CellKey);
            cell.Target = target;
            cell.Colour = ShanghaiUtils.PaintColour.NONE;
            Messenger<Cell>.Broadcast(Cell.EVENT_CELL_UPDATED, cell);
        } 

        private void OnSourceCreated(Source source) {
            Sources.Add(source);
            Cell cell = _Grid.GetCell(source.CellKey);
            cell.Source = source;
            cell.Colour = ShanghaiUtils.PaintColour.NONE;
            Messenger<Cell>.Broadcast(Cell.EVENT_CELL_UPDATED, cell);
        } 

        //NOTE: Move to Controllers
        private void OnCellProgressed(IntVect2 cellKey, float progress) {
            //_Grid.CellProgressed(cellKey, progress);
        }

        private void OnPackageDelivered(List<IntVect2> path, Source source) {
            //_Grid.ResetCellsProgress(path);
        }
    }
}
