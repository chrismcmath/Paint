using UnityEngine;
using System.Collections;
using Shanghai.Model;

namespace Shanghai.ViewControllers {
    public class CellController : MonoBehaviour {
        public static readonly string EVENT_CELL_CLICKED = "EVENT_CELL_CLICKED";
        public static readonly string EVENT_CELL_DRAGGED = "EVENT_CELL_DRAGGED";
        public static readonly string EVENT_CELL_DRAG_END = "EVENT_CELL_DRAG_END";

        public static readonly string PIPE_PREFIX = "default";
        public static readonly string CLIENT_PREFIX = "mission";
        public static readonly string TARGET_PREFIX = "target";
        public static readonly string OBSTACLE_PREFIX = "obstacle";

        public static readonly float FULL_ALPHA = 1.0f;

        public const float VIBRATE_START_THRESHOLD = 10.0f;
        public const float MAX_OFFSET = 17.0f;

        public IntVect2 Key;
        private GameObject _CurrentObject = null;
        private float _Vibration = 0.0f;

        public UISprite PipeSprite;
        public UISprite SourceSprite;
        public UISprite TargetSprite;
        public UISprite ProgressSprite;
        public UISprite BackgroundSprite;

        public void UpdateCell(Cell cell) {
            UpdateSprite(PipeSprite, PIPE_PREFIX, GetPipeString(cell.Pipe));
            //Debug.Log("source: " + cell.Source + " target: " + cell.Target);
            UpdateColour(SourceSprite, cell.Source);
            UpdateColour(TargetSprite, cell.Target);

            if (cell.Target != null) {
                if (!cell.Target.Freeze) {
                    // Linear graph (vibration, ttl) from (0,1) to (10,0)
                    _Vibration = Mathf.Clamp(-(cell.Target.TTL / VIBRATE_START_THRESHOLD) + 1.0f, 0.0f, 1.0f);
                } else {
                    _Vibration = 0.0f;
                    TargetSprite.transform.localPosition = Vector3.zero;
                }
            }

            if (cell.State == Cell.CellState.DEAD) {
                BackgroundSprite.color = Color.black;
            }

            ProgressSprite.fillAmount = cell.Progress;
        }

        private void UpdateColour(UISprite sprite, ColouredCellAsset asset) {
            if (asset == null) { 
                sprite.alpha = 0;
            } else {
                sprite.alpha = FULL_ALPHA;
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
            if (_Vibration > 0.0f) {
                float xOffset = GetRandomVibrationOffset();
                float yOffset= GetRandomVibrationOffset();
                TargetSprite.transform.localPosition = new Vector3(xOffset, yOffset);
            }

            //Debug.Log("UICamera hovered object: " + UICamera.hoveredObject);
            //Debug.Log("isDragging: " + UICamera.isDragging);
        }

        private float GetRandomVibrationOffset() {
            return _Vibration * (Random.Range(0.0f, MAX_OFFSET*2) - MAX_OFFSET);
        }

        private string GetPipeString(Cell.PipeType type) {
            switch (type) {
                case Cell.PipeType.NONE:
                    return "";
                    break;
                case Cell.PipeType.HORI:
                    return "horizontal";
                    break;
                case Cell.PipeType.VERT:
                    return "vertical";
                    break;
                case Cell.PipeType.NE:
                    return "northeast";
                    break;
                case Cell.PipeType.NW:
                    return "northwest";
                    break;
                case Cell.PipeType.SE:
                    return "southeast";
                    break;
                case Cell.PipeType.SW:
                    return "southwest";
                    break;
                case Cell.PipeType.LEFT:
                    return "endwest";
                    break;
                case Cell.PipeType.RIGHT:
                    return "endeast";
                    break;
                case Cell.PipeType.TOP:
                    return "endnorth";
                    break;
                case Cell.PipeType.BOTTOM:
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
