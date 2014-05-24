using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Shanghai.Entities;
using Shanghai.Grid;
using Shanghai.Path;
using Shanghai.Controllers;

namespace Shanghai {
    public class GameModel : MonoSingleton<GameModel> {
        public static readonly string EVENT_MONEY_CHANGED = "EVENT_MONEY_CHANGED";
        public static readonly string EVENT_COLOUR_CHANGED = "EVENT_COLOUR_CHANGED";
        public static readonly string EVENT_SOURCE_CHANGED = "EVENT_SOURCE_CHANGED";

        public static readonly string EVENT_CLIENT_UPDATED = "EVENT_CLIENT_UPDATED";
        public static readonly string EVENT_TARGET_UPDATED = "EVENT_TARGET_UPDATED";

        public static readonly int GRID_SIZE = 6;

        private Grid.Grid _Grid;
        public Grid.Grid Grid {
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

        private List<Mission> _Missions = new List<Mission>();
        public List<Mission> Missions {
            get { return _Missions; }
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
            /* Add clients (embassies) */
            /*
            _Clients = new Dictionary<string, Client>();
            AddEntityToCollection("uk", _Clients);
            AddEntityToCollection("france", _Clients);
            AddEntityToCollection("usa", _Clients);
            AddEntityToCollection("japan", _Clients);
            AddEntityToCollection("russia", _Clients);

            /* Add targets (minitries) */
            /*
            _Targets = new Dictionary<string, Target>();
            AddEntityToCollection("education", _Targets);
            AddEntityToCollection("environment", _Targets);
            AddEntityToCollection("health", _Targets);
            AddEntityToCollection("justice", _Targets);
            AddEntityToCollection("trade", _Targets);
            */

            _Grid = new Grid.Grid(GRID_SIZE);
            _Grid.ResetAllCells(true);

            _Missions = new List<Mission>();
            _ActiveMissions = new List<ActiveMission>();
            Money = 0;
            _CanDraw = true;
        }

        //NOTE: NOT WORKING Timing issue here, need to update after Controllers have initialized
        private IEnumerator UpdateGridController() {
            yield return new WaitForEndOfFrame();
            Messenger<List<List<PlayableCell>>>.Broadcast(Grid.EVENT_GRID_UPDATED, _Grid.Cells);
        }

        private void AddEntityToCollection<T>(string key, Dictionary<string, T> collection) where T : Entity, new() {
            T entity = new T();
            entity.Key = key;
            collection.Add(key, entity);
        }

        public Mission GetMissionFromCellKey(IntVect2 key) {
            foreach (Mission mission in _Missions) {
                if (ShanghaiUtils.KeysMatch(mission.CellKey, key)) {
                    return mission;
                }
            }
            Debug.Log("ERROR, couldn't get mission from key " + key);
            return null;
        }

        public void RemoveActiveMission(ActiveMission actMiss) {
            _Grid.ResetCellsInPath(actMiss.Path);
            // remove source and target
            _ActiveMissions.Remove(actMiss);
        }

        public void ChangeColour() {
            _PaintColour = ShanghaiUtils.GetRandomColour(_AvailableColours);
            Messenger<ShanghaiUtils.PaintColour>.Broadcast(EVENT_COLOUR_CHANGED, _PaintColour);
        }

        private void OnTargetCreated(Target target) {
            Debug.Log("OnTargetCreated");
            Targets.Add(target);
            PlayableCell cell = _Grid.GetCell(target.CellKey);
            cell.Target = target;
            Debug.Log("OnTargetCreated broadcast");
            Messenger<PlayableCell>.Broadcast(PlayableCell.EVENT_CELL_UPDATED, cell);
        } 

        private void OnSourceCreated(Source source) {
            Debug.Log("OnSourceCreated");
            Sources.Add(source);
            PlayableCell cell = _Grid.GetCell(source.CellKey);
            cell.Source = source;
            Debug.Log("OnSourceCreated broadcast");
            Messenger<PlayableCell>.Broadcast(PlayableCell.EVENT_CELL_UPDATED, cell);
        } 

        private void OnCellProgressed(IntVect2 cellKey, float progress) {
            _Grid.CellProgressed(cellKey, progress);
        }

        private void OnPackageDelivered(List<IntVect2> path, Source source) {
            _Grid.ResetCellsProgress(path);
        }
    }
}
