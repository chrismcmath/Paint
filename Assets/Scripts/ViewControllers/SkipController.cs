using UnityEngine;
using System.Collections;

using Shanghai.Model;

namespace Shanghai.ViewControllers {
    public class SkipController : MonoBehaviour {
        public void OnClick() {
            Messenger.Broadcast(Shanghai.EVENT_SKIP_GO);
        }
    }
}
