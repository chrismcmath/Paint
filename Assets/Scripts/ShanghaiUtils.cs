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

        public static List<Vector2> GetScreenCoordsFromCellKeys(List<IntVect2> path, Dictionary<IntVect2, Vector2> cellPositions) {
            List<Vector2> pathPoints = new List<Vector2>();
            foreach (IntVect2 cellKey in path) {
                if (cellPositions == null || !cellPositions.ContainsKey(cellKey)) {
                    Debug.LogError("Could not find key " + cellKey + " in CellPositions dictionary, or CellPositions not instantiated");
                    return new List<Vector2>();
                } else {
                    pathPoints.Add(cellPositions[cellKey]);
                }
            }
            return pathPoints;
        }

        public static List<IntVect2> GetLegitimateSurroundingCells(IntVect2 center) {
            List<IntVect2> surroundingCells = new List<IntVect2>();

            TryAddCellOffset(new IntVect2(-1, 0), center, surroundingCells);
            TryAddCellOffset(new IntVect2(1, 0), center, surroundingCells);
            TryAddCellOffset(new IntVect2(0, -1), center, surroundingCells);
            TryAddCellOffset(new IntVect2(0, 1), center, surroundingCells);

            return surroundingCells;
        }

        private static void TryAddCellOffset(IntVect2 deviation, IntVect2 center, List<IntVect2> surroundingCells) {
            IntVect2 newKey = new IntVect2(center.x + deviation.x, center.y + deviation.y);
            if (newKey.x >= 0 && newKey.x < ShanghaiConfig.Instance.GridSize &&
                    newKey.y >= 0 && newKey.y < ShanghaiConfig.Instance.GridSize) {
                surroundingCells.Add(newKey);
            }
        }

        public static bool PathContainsPoint(List<IntVect2> path, IntVect2 point) {
            foreach (IntVect2 p in path) {
                if (p == point) {
                    return true;
                }
            }
            return false;
        }
    }
}
