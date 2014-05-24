using UnityEngine;
using System.Collections;

namespace Shanghai.Controllers {
    public class PaintColourController : MonoBehaviour {

        public UISprite ColourSprite;
        public TweenAlpha Flash;

        public ShanghaiUtils.PaintColour CurrentColor;

        public void Awake() {
            Messenger<ShanghaiUtils.PaintColour>.AddListener(GameModel.EVENT_COLOUR_CHANGED, OnColourChanged);
        }

        public void OnDestroy() {
            Messenger<ShanghaiUtils.PaintColour>.RemoveListener(GameModel.EVENT_COLOUR_CHANGED, OnColourChanged);
        }

        public void OnColourChanged(ShanghaiUtils.PaintColour colour) {
            Flash.ResetToBeginning();
            Flash.PlayForward();
            CurrentColor = colour;
            ColourSprite.color = ShanghaiUtils.GetColour(colour);
        }
    }
}
