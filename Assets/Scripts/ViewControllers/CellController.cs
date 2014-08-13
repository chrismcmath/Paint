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
        public const float MAX_OFFSET = 5.0f;

        public IntVect2 Key;
        private GameObject _CurrentObject = null;
        private float _Vibration = 0.0f;

        public UISprite PipeSprite;
        public UISprite SourceSprite;
        public UISprite TargetSprite;
        public UILabel TargetLabel;
        public UIWidget TargetWidget;
        public UISprite ProgressSprite;
        public UISprite BackgroundSprite;

        public UILabel ActMissionNodeLabel;
        public UIWidget ActMissionNodeWidget;

        public void UpdateCell(Cell cell) {
            //UpdateSprite(PipeSprite, PIPE_PREFIX, GetPipeString(cell.Pipe));
            //Debug.Log("source: " + cell.Source + " target: " + cell.Target);
            UpdateColour(SourceSprite, cell.Source);
            UpdateColour(TargetSprite, cell.Target);

            if (cell.Target != null) {
                TargetWidget.alpha = 1.0f;
                if (!cell.Target.Freeze) {
                    //TargetLabel.text = cell.Target.Lives.ToString();
                    TargetLabel.text = "";
                    switch (cell.Target.Lives) {
                        case 3:
                            TargetSprite.transform.localPosition = Vector3.zero;
                            TargetSprite.transform.localScale = Vector3.one;
                            _Vibration = 0.0f;
                            break;
                        case 2:
                            TargetSprite.transform.localScale = Vector3.one * 0.9f;
                            _Vibration = 0.1f;
                            break;
                        case 1:
                            TargetSprite.transform.localScale = Vector3.one * 0.7f;
                            _Vibration = 1f;
                            break;
                        default:
                            TargetSprite.transform.localPosition = Vector3.zero;
                            transform.localScale = Vector3.one;
                            _Vibration = 0.0f;
                            break; 
                    }
                    Debug.Log("local scale: " + TargetSprite.transform.localScale);
                } else {
                    TargetLabel.text = "";
                    _Vibration = 0.0f;
                    TargetSprite.transform.localPosition = Vector3.zero;
                }
            } else {
                TargetWidget.alpha = 0.0f;
            }

            if (cell.State == Cell.CellState.DEAD) {
                BackgroundSprite.color = Color.black;
            } else {
                if (cell.Colour == ShanghaiUtils.PaintColour.NONE) {
                    BackgroundSprite.color = Color.white;
                } else {
                    BackgroundSprite.color = ShanghaiUtils.GetColour(cell.Colour);
                }
            }

            if (cell.State == Cell.CellState.ACT_MISSION_NODE) {
                ActMissionNodeWidget.alpha = 1;
                //Debug.Log("set points to " + cell.Points + " * " + cell.PointsModifier);
                ActMissionNodeLabel.text = string.Format("{0}", cell.Points * cell.PointsModifier);
            } else {
                ActMissionNodeWidget.alpha = 0;
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
                TargetWidget.transform.localPosition = new Vector3(xOffset, yOffset);
            }

            TargetSprite.transform.Rotate(Vector3.forward * Time.deltaTime * 100.0f);
        }

        private float GetRandomVibrationOffset() {
            return _Vibration * (Random.Range(0.0f, MAX_OFFSET*2) - MAX_OFFSET);
        }

        public void OnClick() {
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
