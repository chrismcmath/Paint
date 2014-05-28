using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Shanghai;

namespace Shanghai.ConfigControllers {
    public class TargetIntervalController : ConfigController {
        protected override float GetInterval() {
            return ShanghaiConfig.Instance.TargetInterval;
        }

        protected override void SetInterval(float value) {
            ShanghaiConfig.Instance.TargetInterval = value;
        }
    }
}
