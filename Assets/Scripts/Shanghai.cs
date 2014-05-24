using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Shanghai.Entities;
using Shanghai.Grid;
using Shanghai.Controllers;
using Shanghai.Path;

namespace Shanghai {
    public class Shanghai : MonoBehaviour {
        public static readonly string EVENT_GAME_START = "EVENT_GAME_START";

        public enum GameState {START=0, PLAY, END_GAME};

        public PlayGridController GridController;

        public UILabel DebugLabel;

        private GameState _State = GameState.PLAY;
        private float _CurrentTime;
        private float _ColourInterval = 0.0f;
        private float _SourceInterval = 0.0f;
        private float _TargetInterval = 0.0f;
        private GameModel _Model;
        private ShanghaiConfig _Config;
        private EventGenerator _Generator;

        public void Awake() {
            _Model = GameModel.Instance;
            _Config = ShanghaiConfig.Instance;
            _Generator = new EventGenerator();


            // Update GUI
            Messenger<List<IntVect2>>.AddListener(Grid.Grid.EVENT_SET_PATH, OnSetPath);
            Messenger.AddListener(Grid.Grid.EVENT_MISSION_FAILED, OnMissionFailed);
            Messenger.AddListener(EVENT_GAME_START, OnGameStart);
            Messenger.AddListener(PathDrawer.EVENT_RESET_COLOUR_INTERVAL, OnResetColourInterval);

            OnGameStart();
        }

        public void CreateAssets() {
            if (GridController != null) {
                GridController.CreateTable(GameModel.GRID_SIZE);
            } else {
                Debug.Log("GridController not set");
            }
        }

        public void OnDestroy() {
            Messenger<List<IntVect2>>.RemoveListener(Grid.Grid.EVENT_SET_PATH, OnSetPath);
            Messenger.RemoveListener(Grid.Grid.EVENT_MISSION_FAILED, OnMissionFailed);
            Messenger.RemoveListener(EVENT_GAME_START, OnGameStart);
            Messenger.RemoveListener(PathDrawer.EVENT_RESET_COLOUR_INTERVAL, OnResetColourInterval);
        }

        public void Update() {
            switch (_State) {
                case GameState.START:
                    break;
                case GameState.PLAY:
                    GameLoop();
                    break;
                case GameState.END_GAME:
                    break;
            }   
        }

        public void OnGameStart() {
            _Model.Reset();
            CreateAssets();
            _State = GameState.PLAY;
        }
            

        private void GameLoop() {
            _CurrentTime += Time.deltaTime;
            _ColourInterval -= Time.deltaTime;
            _SourceInterval -= Time.deltaTime;
            _TargetInterval -= Time.deltaTime;
            //DebugLabel.text = string.Format("{0:00}\nMission:{1:00}\nSource:{1:00}", _CurrentTime, _ColourInterval, _SourceInterval);

            if (_ColourInterval < 0.0f) {
                _ColourInterval = _Config.ColourInterval;
                _Model.ChangeColour();
            }
            if (_SourceInterval < 0.0f) {
                _SourceInterval = _Config.SourceInterval;
                _Generator.GenerateSource();
            }
            if (_TargetInterval < 0.0f) {
                _TargetInterval = _Config.TargetInterval;
                _Generator.GenerateTarget();
            }

            UpdateActiveMissions(Time.deltaTime);
            /*
            ReplinishTargets(Time.deltaTime);
            DrainClients(Time.deltaTime);
            TickMissions(Time.deltaTime);

            CheckForEndGame();
            */
        }

        /*
        public void CheckForEndGame() {
            foreach (KeyValuePair<string, Client> client in _Model.Clients) {
                if (client.Value.Reputation <= _Config.MinReputation) {
                    EndGame();
                }
            }
        }
        */

        private void EndGame() {
            _Model.CanDraw = false;
            _State = GameState.END_GAME;
        }

        /*
        public void ReplinishTargets(float delta) {
            foreach (KeyValuePair<string, Target> target in _Model.Targets) {
                target.Value.ReplenishHealth(delta);
            }
        }

        public void DrainClients(float delta) {
            foreach (KeyValuePair<string, Client> client in _Model.Clients) {
                client.Value.DrainReputation(delta);
            }
        }

        public void TickMissions(float delta) {
            List<Mission> garbage = new List<Mission>();
            foreach (Mission mission in _Model.Missions) {
                if (!mission.IsActive && mission.IsTTD(delta)) {
                    garbage.Add(mission);

                    PlayableCell cell = _Model.Grid.GetCell(mission.CellKey);
                    cell.TargetID = "";
                    cell.ClientID = "";
                    Messenger<PlayableCell>.Broadcast(PlayableCell.EVENT_CELL_UPDATED, cell);
                }
            }

            /* Garbage collection */ /*
            foreach (Mission mission in garbage) {
                _Model.Missions.Remove(mission);
            }
        }
*/
        public void UpdateActiveMissions(float delta) {
            List<ActiveMission> garbage = new List<ActiveMission>();
            foreach (ActiveMission actMiss in _Model.ActiveMissions) {
                if (actMiss.Progress(delta * _Config.CellFillPerSecond)) {
                    garbage.Add(actMiss);
                    //TODO: active mission finished logic here

                    //NOTE: success logic here
                }
            }

            /* Garbage collection */
            foreach (ActiveMission actMiss in garbage) {
                _Model.RemoveActiveMission(actMiss);
            }
        }

        private void OnSetPath(List<IntVect2> path) {
            if (_State == GameState.PLAY) {
                _Model.CanDraw = true;
            }

            PlayableCell startCell = _Model.Grid.GetCell(path[0]);
            PlayableCell endCell = _Model.Grid.GetCell(path[path.Count-1]);

            if (startCell.Source.PaintColour != endCell.Target.PaintColour) {
                _Model.Grid.ResetCellsInPath(path);
                _Model.Grid.KillCell(endCell);
            } else {
                _Model.ActiveMissions.Add(new ActiveMission(path, startCell.Source, endCell.Target));
            }
        }

        private void OnMissionFailed() {
            Debug.Log("mission failed");
        }

        private void OnResetColourInterval() {
            _ColourInterval = 0.0f;
        }
    }
}
