using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Vectrosity;

using Shanghai.Model;

namespace Shanghai.ViewControllers {
    public class ActiveMissionViewController {
        private GameModel _Model;

        public Material _ColourPathMaterial;
        public Material _TrackPathMaterial;

        private VectorLine _ColourPath = null;
        private VectorLine _TrackPath = null;

        private ActiveMission _ActMission = null;

        private List<Vector2> _ColourPathPoints;

        public ActiveMissionViewController(ActiveMission actMission, Material material) {
            _Model = GameModel.Instance;
            _ActMission = actMission;

            _TrackPathMaterial = new Material(material);
            _ColourPathMaterial = new Material(material);

            _ColourPathPoints = ShanghaiUtils.GetScreenCoordsFromCellKeys(_ActMission.Path, _Model.CellPositions);
            _ColourPathMaterial.SetColor("_TintColor", ShanghaiUtils.GetColour(_ActMission.Source.PaintColour));
            _ColourPath = new VectorLine("Colour Path",  _ColourPathPoints.ToArray(), _ColourPathMaterial, 10.0f, LineType.Continuous, Joins.Weld);
            _ColourPath.Draw();

            Vector2[] linePoints = new Vector2[2];
            linePoints[0] = new Vector2(0,0);
            linePoints[1] = new Vector2(Screen.width, Screen.height);
            _TrackPathMaterial.SetColor("_TintColor", Color.black);
            _TrackPath = new VectorLine("Active Path",  _ColourPathPoints.ToArray(), _TrackPathMaterial, 5.0f, LineType.Continuous, Joins.Weld);
            _TrackPath.Draw();
        }

        // manually updated by ActiveMissionsViewController
        public void Update() {
            _TrackPath.Resize(GetTrackPoints());
            _TrackPath.Draw();
        }

        public void CleanUp() {
            VectorLine.Destroy(ref _ColourPath);
            VectorLine.Destroy(ref _TrackPath);
        }

        //Builds path backwards
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
    }
}

