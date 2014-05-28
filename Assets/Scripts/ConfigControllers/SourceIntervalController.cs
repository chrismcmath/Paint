using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Shanghai;

namespace Shanghai.ConfigControllers {
    public class SourceIntervalController : ConfigController {
        protected override float GetInterval() {
            return ShanghaiConfig.Instance.SourceInterval;
        }

        protected override void SetInterval(float value) {
            ShanghaiConfig.Instance.SourceInterval = value;
        }
    }
}
