using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Shanghai.Model;

namespace Shanghai.ViewControllers {
    public class GameOverController : MonoBehaviour {
        public static readonly string HEADLINE_PREFIX = "headline";

        public UISprite Headline;
        public UILabel FinalPoint;

        public void Populate(Dictionary<string, Target> targets, int finalPoint) {
        /*
            FinalPoint.text = string.Format("{0}", finalMoney);

            Target lowestTarget = targets["education"];
            foreach(KeyValuePair<string, Target> targetPair in targets) {
                Target target = targetPair.Value;
                if (target.Health < lowestTarget.Health) {
                    lowestTarget = target;
                }
            }
            Headline.spriteName = string.Format("{0}_{1}", HEADLINE_PREFIX, lowestTarget.Key);
        */
        }

        public void OnClick() {
            Messenger.Broadcast(Shanghai.EVENT_GAME_START);
        }
    }
}
