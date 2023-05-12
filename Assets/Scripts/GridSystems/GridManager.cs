using System;
using System.Collections;
using Minesweeper.Utils;
using TMPro;
using UnityEngine;
using XIV.Core;
using XIV.Core.Collections;
using XIV.Core.Extensions;
using XIV.GridSystems;
using Random = UnityEngine.Random;

namespace Minesweeper.GridSystems
{
    public struct CellData
    {
        public int index;
        public bool isRevealed;
        public bool hasMine;
        public int mineNeigbourCount;
    }

    public class GridManager : MonoBehaviour, IGridListener
    {
        public Vector2 AreaSize = new Vector2(15f, 20f);
        [Tooltip("Beginner: 9x9 grid with 10 mines, Intermediate: 16x16 grid with 40 mines, Expert: 16x30 grid with 99 mines")]
        public Vector2Int cellCount;
        public int mineCount;
        
        [SerializeField] float cellPadding;
        [SerializeField] GameObject cellPrefab;
        [SerializeField] Material[] mineClickedMaterials;
        [SerializeField] Material[] emptyFieldMaterials;

        CellData[] cellDatas;
        Cell[] cellGameobjects;
        AudioSource audioSource;
        GridXY gridXY;
        int selectedIndex = -1;
        bool minePlaced;
        bool exploreLock;
        public bool drawGizmos;
        int[] mineIndices;

        void Awake()
        {
            gridXY = new GridXY(transform.position, AreaSize, cellCount);
            var cellDatas = gridXY.GetCells();
            int count = cellDatas.Count;
            this.cellDatas = new CellData[count];
            cellGameobjects = new Cell[count];
            for (int i = 0; i < count; i++)
            {
                ref var cellData = ref cellDatas[i];
                var cell = Instantiate(cellPrefab, cellData.worldPos, Quaternion.identity, transform).GetComponent<Cell>();
                cell.transform.localScale = cellData.cellSize.SetZ(cell.transform.localScale.z) - Vector3.one * cellPadding;
                cell.Initialize();
                cellGameobjects[cellData.index] = cell;
                this.cellDatas[cellData.index].index = cellData.index;
            }

            audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

            GetComponent<BoxCollider>().size = ((Vector3)AreaSize).SetZ(transform.localScale.z + 0.1f) * 2f;
        }

        void OnEnable()
        {
            gridXY.AddListener(this);
        }

        void OnDisable()
        {
            gridXY.RemoveListener(this);
        }
        
#if UNITY_EDITOR
        
        void OnDrawGizmos()
        {
            if (drawGizmos == false) return;
            GridXY.DisplayGrid(transform.position, AreaSize, cellCount);
        }
        
#endif

        void PlaceMines(int clickedIndex)
        {
            DynamicArray<int> clickedNeighbourIndices = new DynamicArray<int>(gridXY.GetNeighbourIndices(clickedIndex));
            // Remove some of the neighbours to leave some space for poisson disc sampling and mine placement
            for (int i = 0; i < 4 && i < clickedNeighbourIndices.Count; i++)
            {
                clickedNeighbourIndices.RemoveAt(Random.Range(0, clickedNeighbourIndices.Count));
            }
            clickedNeighbourIndices.Add() = clickedIndex;
            
            mineIndices = new int[mineCount];
            var points = GetPoints(clickedNeighbourIndices);
            SetMineIndices(points, clickedNeighbourIndices);

            for (int i = 0; i < 30 && mineIndices.Contains(0); i++)
            {
                var newPoints = GetPoints(clickedNeighbourIndices);
                SetMineIndices(newPoints, clickedNeighbourIndices);
            }
            
            for (int i = 0; i < mineCount; i++)
            {
                int index = mineIndices[i] - 1; // 1 based
                if (index < 0)
                {
                    Debug.Log("Not placed");
                    continue;
                }

#if UNITY_EDITOR
                cellGameobjects[index].GetComponent<Renderer>().material.color = Color.red;
#endif

                cellDatas[index].hasMine = true;
            }

            minePlaced = true;
        }

        void SetMineIndices(Vector3[] points, DynamicArray<int> clickedNeighbourIndices)
        {
            int pointsLength = points.Length;
            for (int i = 0; i < pointsLength && mineIndices.Contains(0, out int emptyPlaceIndex); i++)
            {
                int index = gridXY.GetIndexByWorldPos(points[i]);
                ref var cell = ref cellDatas[index];
                if (cell.hasMine || clickedNeighbourIndices.Contains(ref index)) continue;

                cell.hasMine = true;
                var neighbourIndices = gridXY.GetNeighbourIndices(index);
                int neighhbourCount = neighbourIndices.Count;
                for (int j = 0; j < neighhbourCount; j++)
                {
                    cellDatas[neighbourIndices[j]].mineNeigbourCount++;
                }

                mineIndices[emptyPlaceIndex] = index + 1; // 1 based
            }
        }

        Vector3[] GetPoints(DynamicArray<int> clickedNeighbourIndices)
        {
            Vector2[] ignoreList = new Vector2[clickedNeighbourIndices.Count];
            var gridCells = gridXY.GetCells();
            var cellSize = gridXY.CellSize;
            for (int i = 0; i < clickedNeighbourIndices.Count; i++)
            {
                var cell = gridCells[clickedNeighbourIndices[i]];
                var localPos = cell.worldPos - (gridXY.GridCenter - (Vector3)(AreaSize * 0.5f) + (cellSize * 0.5f));
                ignoreList[i] = localPos;
            }

            var r = Mathf.Sqrt(cellSize.x * cellSize.x + cellSize.y * cellSize.y) * 1.5f;
            var points = PoissonDiscSampling.GeneratePoints(r, AreaSize, ignore: ignoreList).ToVector3();
            for (int i = 0; i < points.Length; i++)
            {
                points[i] += gridXY.GridCenter - (Vector3)(AreaSize * 0.5f) + cellSize * 0.5f;
            }

            return points;
        }

        public void ExploreCell(int cellIndex, bool exploreNeighbours, Action<CellData> onCellExplored)
        {
            ref var celldata = ref cellDatas[cellIndex];
            if (celldata.isRevealed) return;
            celldata.isRevealed = true;

            var materials = cellDatas[cellIndex].hasMine ? mineClickedMaterials : emptyFieldMaterials;
            cellGameobjects[cellIndex].Explore(materials, () =>
            {
                selectedIndex = -1;
                exploreLock = false;
                var worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var index = gridXY.GetIndexByWorldPos(worldPos);
                if (index == selectedIndex || cellGameobjects[index].Select() == false) return;
                selectedIndex = index;
                onCellExplored?.Invoke(cellDatas[cellIndex]);
            });
            cellGameobjects[cellIndex].GetComponentInChildren<TMP_Text>().text = celldata.mineNeigbourCount.ToString();
            
            if (celldata.mineNeigbourCount > 0 || exploreNeighbours == false) return;
            var neighboringIndices = new DynamicArray<int>(gridXY.GetNeighbourIndices(cellIndex));
            for (int i = 0; i < neighboringIndices.Count; i++)
            {
                var neighbourIndex = neighboringIndices[i];
                ref var neighbourCellData = ref cellDatas[neighbourIndex];
                if (neighbourCellData.hasMine == false)
                {
                    ExploreCell(neighbourIndex, true, onCellExplored);
                }
            }
        }

        void IGridListener.OnGridChanged(IGrid grid)
        {
            var cellDatas = gridXY.GetCells();
            int count = cellDatas.Count;
            for (int i = 0; i < count; i++)
            {
                ref var cellData = ref cellDatas[i];
                Cell cell = cellGameobjects[cellData.index];
                Transform cellTransform = cell.transform;
                cellTransform.position = cellData.worldPos;
                cellTransform.localScale = cellData.cellSize - Vector3.one * cellPadding;
            }
        }

        public bool ChangeSelection(Vector3 worldPos)
        {
            var index = gridXY.GetIndexByWorldPos(worldPos);
            if (selectedIndex != -1 && (index == selectedIndex || cellGameobjects[selectedIndex].Deselect() == false || cellDatas[index].isRevealed || cellGameobjects[index].Select() == false)) return false;
            selectedIndex = index;
            return true;
        }

        public void PlaceMinesIfNotPlaced(Vector3 worldPos)
        {
            if (minePlaced) return;
            var index = gridXY.GetIndexByWorldPos(worldPos);
            PlaceMines(index);
        }

        public CellData GetCellData(Vector3 worldPos)
        {
            return cellDatas[gridXY.GetIndexByWorldPos(worldPos)];
        }

        public void BlastMines(int cellDataIndex, Action onCellBlasted, Action onCompleted = null)
        {
            StopAllCoroutines();
            cellDataIndex = cellDataIndex < 0 ? mineIndices[0] : cellDataIndex;
            StartCoroutine(BlastAll(cellDataIndex, onCellBlasted, onCompleted));
        }

        IEnumerator BlastAll(int cellDataIndex, Action onCellBlasted, Action onCompleted)
        {
            cellGameobjects[cellDataIndex].Explore(mineClickedMaterials, onCellBlasted);
            var duration = 8f / mineIndices.Length;
            for (int i = 0; i < mineIndices.Length; i++)
            {
                var waitTime = Random.Range(0.2f, duration);
                yield return new WaitForSeconds(waitTime);
                var index = mineIndices[i] - 1;
                if (index == cellDataIndex) continue;
                
                cellGameobjects[index].Explore(mineClickedMaterials, onCellBlasted);
            }

            onCompleted?.Invoke();
        }

        public bool IsClearedAllCells()
        {
            for (int i = 0; i < cellDatas.Length; i++)
            {
                ref var cellData = ref cellDatas[i];
                if (cellData.hasMine) continue;
                if (cellData.isRevealed == false) return false;
            }

            return true;
        }
    }
}