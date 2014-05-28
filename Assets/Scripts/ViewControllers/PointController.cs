using UnityEngine;
using System.Collections;

namespace Shanghai.ViewControllers {
    public class PointController : MonoBehaviour {

        public UILabel PointLabel;
        public UIPlaySound CashSound;

        public void Awake() {
            Messenger<int>.AddListener(GameModel.EVENT_POINT_CHANGED, OnPointChanged);
        }

        public void OnDestroy() {
            Messenger<int>.RemoveListener(GameModel.EVENT_POINT_CHANGED, OnPointChanged);
        }

        public void OnPointChanged(int point) {
            //CashSound.Play();
            PointLabel.text = string.Format("{0}", point);
        }
    }
}
