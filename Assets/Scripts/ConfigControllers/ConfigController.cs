using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Shanghai;

namespace Shanghai.ConfigControllers {
    public abstract class ConfigController : MonoBehaviour {
        public const float MIN = 0.0f;
        public const float MAX = 10.0f;

        public UISlider Slider;
        public UILabel Value;

        public void Awake() {
            EventDelegate.Add(Slider.onChange, OnSliderChange);

            float interval = GetInterval();
            Slider.sliderValue = (interval - MIN) / MAX;
            UpdateLabel();
        }

        public void OnSliderChange() {
            float percent = UISlider.current.value;
            SetInterval((MAX - MIN) * percent + MIN);

            UpdateLabel();
        }

        private void UpdateLabel() {
            Value.text = string.Format("{0}", GetInterval());
        }

        protected abstract float GetInterval();
        protected abstract void SetInterval(float value);

    }
}
