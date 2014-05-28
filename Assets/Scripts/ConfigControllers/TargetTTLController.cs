using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Shanghai;

namespace Shanghai.ConfigControllers {
    public class TargetTTLController : ConfigController {
        protected override float GetInterval() {
            return ShanghaiConfig.Instance.TargetWaitTime;
        }

        protected override void SetInterval(float value) {
            ShanghaiConfig.Instance.TargetWaitTime = value;
        }
    }
}
