using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Shanghai;

namespace Shanghai.ConfigControllers {
    public class ColourNumberController : ConfigController {
        protected override float GetInterval() {
            return (float) ShanghaiConfig.Instance.InitialAvailableColours;
        }

        protected override void SetInterval(float value) {
            ShanghaiConfig.Instance.InitialAvailableColours = (int) value;
        }
    }
}
