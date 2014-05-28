using UnityEngine;
using System.Collections;

using Shanghai.Model;

namespace Shanghai.ViewControllers {
    public class TitleController : MonoBehaviour {
        public void OnClick() {
            Messenger.Broadcast(Shanghai.EVENT_GAME_START);
        }
    }
}
