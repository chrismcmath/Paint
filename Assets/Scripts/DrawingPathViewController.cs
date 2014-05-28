using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Vectrosity;

using Shanghai.Model;

namespace Shanghai.ViewControllers {
    public class DrawingPathViewController : MonoBehaviour {
        
        private VectorLine _Path;
        private GameModel _Model;

        public Material lineMaterial;

        public void Awake() {
            Vector2[] linePoints = new Vector2[2];
            linePoints[0] = new Vector2(0,0);
            linePoints[1] = new Vector2(Screen.width, Screen.height);
            _Path = new VectorLine("active path",  linePoints, lineMaterial, 2.0f, LineType.Continuous, Joins.Weld);
            _Model = GameModel.Instance;
        }

        public void OnDestroy() {
        }

        public void Update() {
            if (_Model.Path != null && _Model.Path.Count > 0) {
                Debug.Log("update line");
                List<Vector2> pathPoints = GetScreenCoordsFromCellKeys(_Model.Path);
                int i = 0;
                foreach (Vector2 point in pathPoints) {
                    Debug.Log(i++ + ": " + point);
                }
                if (pathPoints.Count >= 2) {
                    _Path.Resize(pathPoints.ToArray());
                }
                _Path.Draw();
            }
        }

        private List<Vector2> GetScreenCoordsFromCellKeys(List<IntVect2> path) {
            List<Vector2> pathPoints = new List<Vector2>();
            foreach (IntVect2 cellKey in path) {
                if (_Model.CellPositions == null || !_Model.CellPositions.ContainsKey(cellKey)) {
                    Debug.LogError("Could not find key " + cellKey + " in CellPositions dictionary, or CellPositions not instantiated");
                    return new List<Vector2>();
                } else {
                    pathPoints.Add(_Model.CellPositions[cellKey]);
                }
            }
            return pathPoints;
        }
    }
}
