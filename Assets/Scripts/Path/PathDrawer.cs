using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Shanghai.Grid;
using Shanghai.Controllers;

namespace Shanghai.Path {
    public class PathDrawer : MonoBehaviour {
        public static readonly string EVENT_RESET_COLOUR_INTERVAL = "EVENT_RESET_COLOUR_INTERVAL";

        private List<IntVect2> _Path = new List<IntVect2>();
        private GameModel _Model;
        private IntVect2 _CurrentCell = null;

        public void Awake() {
            Messenger<IntVect2>.AddListener(CellController.EVENT_CELL_DRAGGED, OnCellDragged);
            Messenger<IntVect2>.AddListener(CellController.EVENT_CELL_CLICKED, OnCellClicked);
            Messenger.AddListener(CellController.EVENT_CELL_DRAG_END, OnCellDragEnd);
            _Model = GameModel.Instance;
        }

        public void OnDestroy() {
            Messenger<IntVect2>.RemoveListener(CellController.EVENT_CELL_DRAGGED, OnCellDragged);
            Messenger<IntVect2>.RemoveListener(CellController.EVENT_CELL_CLICKED, OnCellClicked);
            Messenger.RemoveListener(CellController.EVENT_CELL_DRAG_END, OnCellDragEnd);
        }

        private void OnCellClicked(IntVect2 cellKey) {
            PlayableCell cell = _Model.Grid.GetCell(cellKey);
            if (cell != null &&
                cell.Source != null &&
                cell.Source.PaintColour == ShanghaiUtils.PaintColour.NONE) {

                cell.Source.PaintColour = _Model.PaintColour;
                Messenger<PlayableCell>.Broadcast(PlayableCell.EVENT_CELL_UPDATED, cell);
                Messenger.Broadcast(EVENT_RESET_COLOUR_INTERVAL);
            }
        }

        private void OnCellDragged(IntVect2 cellKey) {
            if (cellKey == _CurrentCell) return;
            _CurrentCell = cellKey;

            if (_Model.Grid.ValidateCellInput(cellKey, _Path)){
                _Path.Add(cellKey);
                if (ShanghaiUtils.IsEndPoint(_Model.Grid.GetCell(cellKey))) {
                    _Model.CanDraw = false;
                }
            }
        }

        private void OnCellDragEnd() {
            /*
            foreach(IntVect2 key in _Path) {
                Debug.Log("END: x " + key.x + " y " + key.y);
            }
            */
            _Model.Grid.SetPath(_Path);
            _Path = new List<IntVect2>();
        }
    }
}
