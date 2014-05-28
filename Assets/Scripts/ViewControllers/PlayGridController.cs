using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Shanghai.Model;

namespace Shanghai.ViewControllers {
    public class PlayGridController : MonoBehaviour {
        public static readonly string CELL_PATH = "prefabs/cell";
        public const string CELL_NAME_FORMAT = "y{0}_x{1}";

        private UITable _Table;

        public void Awake() {
            Messenger<Cell>.AddListener(Cell.EVENT_CELL_UPDATED, OnCellUpdated);
            Messenger<List<List<Cell>>>.AddListener(Grid.EVENT_GRID_UPDATED, OnGridUpdated);
        }

        public void OnDestroy() {
            Messenger<Cell>.RemoveListener(Cell.EVENT_CELL_UPDATED, OnCellUpdated);
            Messenger<List<List<Cell>>>.RemoveListener(Grid.EVENT_GRID_UPDATED, OnGridUpdated);
        }

        private void OnCellUpdated(Cell cell) {
            Transform cellTrans = transform.Find(string.Format(CELL_NAME_FORMAT, cell.Key.y, cell.Key.x));
            if (cell != null && cellTrans != null) {
                CellController cellCtr = cellTrans.GetComponent<CellController>();
                if (cellCtr != null) {
                    cellCtr.UpdateCell(cell);
                }
            }
        }

        private void OnGridUpdated(List<List<Cell>> cells) {
            foreach(List<Cell> row in cells) {
                foreach(Cell cell in row) {
                    OnCellUpdated(cell);
                }
            }
        }

        public void CreateTable() {
            _Table = gameObject.GetComponent<UITable>();
            if (_Table == null) {
                Debug.Log("UITable not initialized ");
                return;
            }

            _Table.columns = ShanghaiConfig.Instance.GridSize;

            GameObject cellPrefab = Resources.Load(CELL_PATH) as GameObject;
            if (cellPrefab == null) {
                Debug.Log("Could not load cell from path " + CELL_PATH);
            }

            ShanghaiUtils.RemoveAllChildren(transform);

            for (int y = 0; y < ShanghaiConfig.Instance.GridSize; y++) {
                for (int x = 0; x < ShanghaiConfig.Instance.GridSize; x++) {
                    GameObject cell = GameObject.Instantiate(cellPrefab) as GameObject;

                    CellController cellCtr = cell.GetComponent<CellController>();
                    cellCtr.Key = new IntVect2(x, y);

                    cell.name = string.Format(CELL_NAME_FORMAT, y, x);
                    cell.transform.parent = transform;
                    cell.transform.localPosition = Vector3.zero;
                    cell.transform.localScale = Vector3.one;
                }
            }
            _Table.repositionNow = true;


            StartCoroutine(LoadCellPositions());
        }

        public IEnumerator LoadCellPositions() {
            yield return new WaitForEndOfFrame();

            Dictionary<IntVect2, Vector2> cellPositions = new Dictionary<IntVect2, Vector2>();
            foreach (Transform cell in transform) {
                //NOTE: not the most optimised of code
                
                CellController cellController = cell.GetComponent<CellController>();
                if (cellController != null) {
                    cellPositions.Add(cellController.Key, Camera.main.WorldToScreenPoint(cell.position));
                }
            }

            GameModel.Instance.CellPositions = cellPositions;
        }
    }
}
