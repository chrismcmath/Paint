using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Vectrosity;

using Shanghai.Model;

namespace Shanghai.ViewControllers {
    public class ActiveMissionsViewController : MonoBehaviour {

        public Material LineMaterial;

        private Dictionary<ActiveMission, ActiveMissionViewController> _Views = new Dictionary<ActiveMission, ActiveMissionViewController>();

        public void Awake() {
            Messenger<ActiveMission>.AddListener(GameModel.EVENT_ACTIVE_MISSION_ADDED, OnActiveMissionAdded);
            Messenger<ActiveMission>.AddListener(GameModel.EVENT_ACTIVE_MISSION_REMOVED, OnActiveMissionRemoved);
        }

        public void OnDestroy() {
            Messenger<ActiveMission>.RemoveListener(GameModel.EVENT_ACTIVE_MISSION_ADDED, OnActiveMissionAdded);
            Messenger<ActiveMission>.AddListener(GameModel.EVENT_ACTIVE_MISSION_REMOVED, OnActiveMissionRemoved);
        }

        public void Update() {
            foreach (KeyValuePair<ActiveMission, ActiveMissionViewController> view in _Views) {
                view.Value.Update();
            }
        }

        private void OnActiveMissionAdded(ActiveMission actMiss) {
            ActiveMissionViewController view = new ActiveMissionViewController(actMiss, LineMaterial);
            _Views.Add(actMiss, view);
        }

        private void OnActiveMissionRemoved(ActiveMission actMiss) {
            if (!_Views.ContainsKey(actMiss)){
                Debug.Log("Could not find active mission in _Views");
            } else {
                ActiveMissionViewController view = _Views[actMiss];
                view.CleanUp();
                _Views.Remove(actMiss);
            }
        }
    }
}
