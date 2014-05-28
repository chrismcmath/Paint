using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using Shanghai.Model;

namespace Shanghai {
    public class ShanghaiUtils {
        public enum PaintColour {RED=0, BLUE, YELLOW, GREEN, PURPLE, ORANGE, NONE};

        public static bool IsEndPoint(Cell cell) {
            return cell.Target != null;
        }

        public static bool KeysMatch(IntVect2 k1, IntVect2 k2) {
            return k1.x == k2.x && k1.y == k2.y;
        }

        public static string BeautifyString(string str) {
            switch (str) {
                case "education":
                    return "Ministry of Education";
                    break;
                case "environment":
                    return "Ministry of Environmental Affairs";
                    break;
                case "health":
                    return "Ministry of Health";
                    break;
                case "justice":
                    return "Ministry of Justice";
                    break;
                case "trade":
                    return "Ministry of Trade";
                    break;
                default:
                    return "none found";
                    break;
            }
        }

        public static void RemoveAllChildren(Transform transform) {
            List<GameObject> children = new List<GameObject>();
            foreach (Transform child in transform) children.Add(child.gameObject);
            children.ForEach(child => UnityEngine.Object.Destroy(child));
        }

        public static PaintColour GetRandomColour(int available) {
            Array values = Enum.GetValues(typeof(PaintColour));
            System.Random random = new System.Random();
            available = available >= values.Length ? values.Length - 1 : available;
            return (PaintColour) values.GetValue(random.Next(available));
        }

        public static Color GetColour(PaintColour colour) {
            switch (colour) {
                case PaintColour.RED:
                    return ShanghaiConfig.Instance.RED;
                    break;
                case PaintColour.BLUE:
                    return ShanghaiConfig.Instance.BLUE;
                    break;
                case PaintColour.YELLOW:
                    return ShanghaiConfig.Instance.YELLOW;
                    break;
                case PaintColour.GREEN:
                    return ShanghaiConfig.Instance.GREEN;
                    break;
                case PaintColour.PURPLE:
                    return ShanghaiConfig.Instance.PURPLE;
                    break;
                case PaintColour.ORANGE:
                    return ShanghaiConfig.Instance.ORANGE;
                    break;
                case PaintColour.NONE:
                    return ShanghaiConfig.Instance.GREY;
                    break;
                default:
                    return new UnityEngine.Color(0.0f, 0.0f, 0.0f);
                    break;
            }
        }
    }
}
