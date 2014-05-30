using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Shanghai.Model;
using Shanghai.ModelControllers;
using Shanghai.ViewControllers;

namespace Shanghai {
    public class Shanghai : MonoBehaviour {
        public static readonly string EVENT_GAME_START = "EVENT_GAME_START";
        public static readonly string EVENT_SKIP_GO = "EVENT_SKIP_GO";

        public enum GameState {START=0, PLAY, END_GAME};

        public PlayGridController GridController;
        public ModelController ModelController;

        private GameModel _Model;
        private GameState _State = GameState.PLAY;

        public void Awake() {
            _Model = GameModel.Instance;

            // Update GUI
            Messenger.AddListener(Grid.EVENT_MISSION_FAILED, OnMissionFailed);
            Messenger.AddListener(EVENT_GAME_START, OnGameStart);

            OnGameStart();
        }

        public void CreateAssets() {
            if (GridController != null) {
                GridController.CreateTable();
            } else {
                Debug.Log("GridController not set");
            }
        }

        public void OnDestroy() {
            Messenger.RemoveListener(Grid.EVENT_MISSION_FAILED, OnMissionFailed);
            Messenger.RemoveListener(EVENT_GAME_START, OnGameStart);
        }

        public void Update() {
            switch (_State) {
                case GameState.START:
                    break;
                case GameState.PLAY:
                    //ModelController.GameLoop();
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
            

        /*
        public void CheckForEndGame() {
            foreach (KeyValuePair<string, Client> client in _Model.Clients) {
                if (client.Value.Reputation <= _Config.MinReputation) {
                    EndGame();
                }
            }
        }
        */

        private void EndGame () {
            _Model.CanDraw = false;
            _State = GameState.END_GAME;
        }

        /*
        public void ReplinishTargets(float delta) {
            foreach (KeyValuePair<string, Target> target in _Model.Targets) {
                target.Value.ReplenishHealth(delta);
            
        }

        public void DrainClients(float delta) {
            foreach (KeyValuePair<string, Client> client in _Model.Clients) {
                client.Value.DrainReputation(delta);
            }
        }
        */

        private void OnMissionFailed() {
            Debug.Log("mission failed");
        }

    }
}
