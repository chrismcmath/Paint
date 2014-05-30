using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Vectrosity;

using Shanghai.Model;

namespace Shanghai.ViewControllers {
    public class DrawingPathViewController : MonoBehaviour {
        
        private VectorLine _ColourPath;
        private VectorLine _OutlinePath;
        private GameModel _Model;

        Material lineMaterial;
        private Material _ColourPathMaterial;
        private Material _OutlinePathMaterial;

        public void Awake() {
            _ColourPathMaterial = new Material(ShanghaiConfig.Instance.LineMaterial);
            _OutlinePathMaterial = new Material(ShanghaiConfig.Instance.LineMaterial);
            _OutlinePathMaterial.SetColor("_TintColor", Color.black);

            Vector2[] linePoints = new Vector2[2];
            linePoints[0] = new Vector2(0,0);
            linePoints[1] = new Vector2(Screen.width, Screen.height);
            _ColourPath = new VectorLine("active path",  linePoints, _ColourPathMaterial, 5.0f, LineType.Continuous, Joins.Weld);
            _OutlinePath = new VectorLine("outline path",  linePoints, _OutlinePathMaterial, 10.0f, LineType.Continuous, Joins.Weld);
            _Model = GameModel.Instance;
        }

        public void OnDestroy() {
        }

        public void Update() {
            //NOTE: needs optimization - check for change
            if (_Model.Path != null && _Model.Path.Count > 0) {
                List<Vector2> pathPoints = ShanghaiUtils.GetScreenCoordsFromCellKeys(_Model.Path, _Model.CellPositions);
                if (pathPoints.Count >= 2) {
                    _ColourPath.Resize(pathPoints.ToArray());
                    _OutlinePath.Resize(pathPoints.ToArray());

                    _ColourPathMaterial.SetColor("_TintColor", ShanghaiUtils.GetColour(_Model.PathColour));
                    _ColourPath.active = true;
                    _ColourPath.Draw();
                    _OutlinePath.active = true;
                    _OutlinePath.Draw();
                }
            } else {
                _ColourPath.active = false;
                _ColourPath.Draw();
                _OutlinePath.active = false;
                _OutlinePath.Draw();
            }
        }

    }
}
