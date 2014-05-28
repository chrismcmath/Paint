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
            _Path = new VectorLine("active path",  linePoints, lineMaterial, 5.0f, LineType.Continuous, Joins.Weld);
            _Model = GameModel.Instance;
        }

        public void OnDestroy() {
        }

        public void Update() {
            //NOTE: needs optimization - check for change
            if (_Model.Path != null && _Model.Path.Count > 0) {
                List<Vector2> pathPoints = ShanghaiUtils.GetScreenCoordsFromCellKeys(_Model.Path, _Model.CellPositions);
                if (pathPoints.Count >= 2) {
                    _Path.Resize(pathPoints.ToArray());
                    lineMaterial.SetColor("_TintColor", ShanghaiUtils.GetColour(_Model.PathColour));
                    _Path.active = true;
                    _Path.Draw();
                }
            } else {
                _Path.active = false;
                _Path.Draw();
            }
        }

    }
}
