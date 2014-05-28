using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Shanghai;

namespace Shanghai.ConfigControllers {
    public class ColourChangeController : ConfigController {
        protected override float GetInterval() {
            return ShanghaiConfig.Instance.ColourInterval;
        }

        protected override void SetInterval(float value) {
            ShanghaiConfig.Instance.ColourInterval = value;
        }
    }
}
