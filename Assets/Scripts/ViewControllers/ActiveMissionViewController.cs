using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Vectrosity;

using Shanghai.Model;

namespace Shanghai.ViewControllers {
    public class ActiveMissionViewController {
        private GameModel _Model;

        public Material _ColourPathMaterial;
        public Material _OutlinePathMaterial;

        private VectorLine _ColourPath = null;
        private VectorLine _OutlinePath = null;

        private ActiveMission _ActMission = null;

        private List<Vector2> _ColourPathPoints;

        public ActiveMissionViewController(ActiveMission actMission, Material material) {
            _Model = GameModel.Instance;
            _ActMission = actMission;

            _ColourPathMaterial = new Material(material);
            _OutlinePathMaterial = new Material(material);

            _ColourPathPoints = ShanghaiUtils.GetScreenCoordsFromCellKeys(_ActMission.Path, _Model.CellPositions);
            _ColourPathMaterial.SetColor("_TintColor", ShanghaiUtils.GetColour(_ActMission.PaintColour));
            _ColourPath = new VectorLine("Colour Path",  _ColourPathPoints.ToArray(), _ColourPathMaterial, 5.0f, LineType.Continuous, Joins.Weld);
            _ColourPath.Draw();

            _OutlinePathMaterial.SetColor("_TintColor", Color.black);
            _OutlinePath = new VectorLine("Colour Path",  _ColourPathPoints.ToArray(), _OutlinePathMaterial, 10.0f, LineType.Continuous, Joins.Weld);
            _OutlinePath.Draw();
        }

        // manually updated by ActiveMissionsViewController
        public void Update() {
            // Stop drawing once the ID reaches the last point
            if (_ActMission.Path.Count >= 2) {
                _OutlinePath.active = true;
                _ColourPath.active = true;

                _OutlinePath.Resize(ShanghaiUtils.GetScreenCoordsFromCellKeys(_ActMission.Path, _Model.CellPositions).ToArray());
                _ColourPath.Resize(ShanghaiUtils.GetScreenCoordsFromCellKeys(_ActMission.Path, _Model.CellPositions).ToArray());

            } else {
                _OutlinePath.active = false;
                _ColourPath.active = false;
            }

            _ColourPath.Draw();
            _OutlinePath.Draw();
        }

        public void CleanUp() {
            VectorLine.Destroy(ref _ColourPath);
            VectorLine.Destroy(ref _OutlinePath);
        }

        //Builds path backwards
        /*
        private Vector2[] GetTrackPoints() {
            List<Vector2> reversePath = new List<Vector2>(_ColourPathPoints);
            reversePath.Reverse();

            List<Vector2> trackPoints = new List<Vector2>();
            // + 1 as we'll tween the last value
            for (int i = 0; i < reversePath.Count - (_ActMission.CurrentCellID + 1); i++) {
                trackPoints.Add(reversePath[i]);
            }

            Vector2 penultimatePoint = reversePath[reversePath.Count - (_ActMission.CurrentCellID + 2)];
            Vector2 finalPoint = reversePath[reversePath.Count - (_ActMission.CurrentCellID + 1)];

            Vector2 lastTrackPoint = finalPoint - (finalPoint - penultimatePoint) * _ActMission.CurrentCellProgress;
            trackPoints.Add(lastTrackPoint);
            return trackPoints.ToArray();
        }
        */
    }
}

