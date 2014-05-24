using UnityEngine;
using System.Collections;
using Shanghai.Grid;

namespace Shanghai.Controllers {
    public class CellController : MonoBehaviour {
        public static readonly string EVENT_CELL_CLICKED = "EVENT_CELL_CLICKED";
        public static readonly string EVENT_CELL_DRAGGED = "EVENT_CELL_DRAGGED";
        public static readonly string EVENT_CELL_DRAG_END = "EVENT_CELL_DRAG_END";

        public static readonly string PIPE_PREFIX = "default";
        public static readonly string CLIENT_PREFIX = "mission";
        public static readonly string TARGET_PREFIX = "target";
        public static readonly string OBSTACLE_PREFIX = "obstacle";

        public static readonly float FULL_ALPHA = 1.0f;

        public IntVect2 Key;
        private GameObject _CurrentObject = null;

        public UISprite PipeSprite;
        public UISprite SourceSprite;
        public UISprite TargetSprite;
        public UISprite ProgressSprite;
        public UISprite BackgroundSprite;

        public void UpdateCell(PlayableCell cell) {
            UpdateSprite(PipeSprite, PIPE_PREFIX, GetPipeString(cell.Pipe));
            Debug.Log("source: " + cell.Source + " target: " + cell.Target);
            UpdateColour(SourceSprite, cell.Source);
            UpdateColour(TargetSprite, cell.Target);

            if (cell.State == PlayableCell.CellState.DEAD) {
                BackgroundSprite.color = Color.black;
            }

            ProgressSprite.fillAmount = cell.Progress;
        }

        private void UpdateColour(UISprite sprite, ColouredCellAsset asset) {
            if (asset == null) { 
                sprite.alpha = 0;
            } else {
                sprite.alpha = FULL_ALPHA;
                Debug.Log("set color to " + ShanghaiUtils.GetColour(asset.PaintColour));
                sprite.color = ShanghaiUtils.GetColour(asset.PaintColour);
            }
        }

        private void UpdateSprite(UISprite sprite, string prefix, string state) {
            if (state == "") { 
                sprite.alpha = 0;
            } else {
                sprite.alpha = ShanghaiConfig.Instance.MissionFlagAlpha;
                sprite.spriteName = string.Format("{0}_{1}", prefix, state);
            }
        }

        public void Update() {
            //Debug.Log("UICamera hovered object: " + UICamera.hoveredObject);
            //Debug.Log("isDragging: " + UICamera.isDragging);
        }

        private string GetPipeString(PlayableCell.PipeType type) {
            switch (type) {
                case PlayableCell.PipeType.NONE:
                    return "";
                    break;
                case PlayableCell.PipeType.HORI:
                    return "horizontal";
                    break;
                case PlayableCell.PipeType.VERT:
                    return "vertical";
                    break;
                case PlayableCell.PipeType.NE:
                    return "northeast";
                    break;
                case PlayableCell.PipeType.NW:
                    return "northwest";
                    break;
                case PlayableCell.PipeType.SE:
                    return "southeast";
                    break;
                case PlayableCell.PipeType.SW:
                    return "southwest";
                    break;
                case PlayableCell.PipeType.LEFT:
                    return "endwest";
                    break;
                case PlayableCell.PipeType.RIGHT:
                    return "endeast";
                    break;
                case PlayableCell.PipeType.TOP:
                    return "endnorth";
                    break;
                case PlayableCell.PipeType.BOTTOM:
                    return "endsouth";
                    break;
                default:
                    Debug.Log("Could not GetPipeString " + type);
                    return "";
                    break;
            }
        }

        public void OnClick() {
            Debug.Log("clicked " + Key);
            Messenger<IntVect2>.Broadcast(EVENT_CELL_CLICKED, Key);
        }

        public void OnDrag(Vector2 delta) {
            Messenger<IntVect2>.Broadcast(EVENT_CELL_DRAGGED, Key);
        }

        public void OnDragOver(GameObject go) {
            Messenger<IntVect2>.Broadcast(EVENT_CELL_DRAGGED, Key);
        }

        public void OnDragEnd() {
            Messenger.Broadcast(EVENT_CELL_DRAG_END);
        }
    }
}
