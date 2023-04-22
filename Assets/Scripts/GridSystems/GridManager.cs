using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GridSystems
{
    struct CellData
    {
        public bool explored;
        public bool hasMine;
        public int mineNeigbourCount;
    }
    
    [RequireComponent(typeof(BoxCollider))]
    public class GridManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
    {
        [Tooltip("Beginner: 9x9 grid with 10 mines, Intermediate: 16x16 grid with 40 mines, Expert: 16x30 grid with 99 mines")]
        [SerializeField] Vector2Int gridSize;
        [SerializeField] int mineCount;
        [SerializeField] float offset = 1.25f;
        [SerializeField] GameObject cellPrefab;
        [SerializeField] GameObject[] cellGameObjects;
        CellData[] cellDatas;
        int[] mineIndices;
        Vector2Int cachedGridSize;
        float cachedOffset;
        
        void Awake()
        {
            int length = gridSize.y * gridSize.x;
            cellGameObjects = new GameObject[length];
            cellDatas = new CellData[length];
            var xSize = 10f / (gridSize.x * offset);
            var ySize = 10f / (gridSize.y * offset);

            var bottomLeft = transform.position - new Vector3((gridSize.x - 1) * 0.5f, (gridSize.y - 1) * 0.5f, 0f);
            for (int y = 0; y < gridSize.y; y++)
            {
                for (int x = 0; x < gridSize.x; x++)
                {
                    GameObject cell = Instantiate(cellPrefab, this.transform, true);
                    cell.gameObject.name = $"Cell_({x},{y})";
                    cell.transform.position = bottomLeft + new Vector3(x, y, 0f);
                    cell.transform.localScale = new Vector3(xSize, ySize, 1f);

                    int index = x * gridSize.y + y;
                    cellGameObjects[index] = cell;
                    cellDatas[index] = new CellData();
                    cell.GetComponentInChildren<TMP_Text>().enabled = false;
                }
            }

            var coll = GetComponent<BoxCollider>();
            coll.size = new Vector3((gridSize.x + 1), (gridSize.y + 1), coll.size.z);
            PlaceMines();
        }

        void Update()
        {
            if (cachedGridSize != gridSize || Math.Abs(cachedOffset - offset) > 0.1f)
            {
                for (int i = 0; i < cellGameObjects.Length; i++)
                {
                    Destroy(cellGameObjects[i]);
                }

                Awake();
                cachedGridSize = gridSize;
                cachedOffset = offset;
            }
        }

        int GetIndexByWorldPos(Vector3 worldPos)
        {
            var localPos = transform.InverseTransformPoint(worldPos);
            int xLength = gridSize.x - 1;
            int yLength = gridSize.y - 1;

            float normalizedX = (localPos.x + xLength / 2f) / xLength;
            float normalizedY = (localPos.y + yLength / 2f) / yLength;
            normalizedX = Mathf.Clamp(normalizedX, 0, 1f);
            normalizedY = Mathf.Clamp(normalizedY, 0, 1f);

            int x = (int)Math.Round((gridSize.x - 1) * normalizedX);
            int y = (int)Math.Round((gridSize.y - 1) * normalizedY);
            int index = x * gridSize.y + y;
            index = Mathf.Clamp(index, 0, cellGameObjects.Length - 1);
            return index;
        }
        
        List<int> GetNeighboringIndices(Vector3 worldPos)
        {
            List<int> neighborIndices = new List<int>();

            int centerIndex = GetIndexByWorldPos(worldPos);
            int x = centerIndex / gridSize.y;
            int y = centerIndex % gridSize.y;

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0) continue;

                    int neighborX = x + i;
                    int neighborY = y + j;

                    if (neighborX < 0 || neighborX >= gridSize.x || neighborY < 0 || neighborY >= gridSize.y) continue;

                    int neighborIndex = neighborX * gridSize.y + neighborY;
                    neighborIndices.Add(neighborIndex);
                }
            }

            return neighborIndices;
        }

        void ExploreCell(int cellIndex)
        {
            ref var celldata = ref cellDatas[cellIndex];
            if (celldata.explored) return;
            celldata.explored = true;
            cellGameObjects[cellIndex].GetComponent<Renderer>().material.color = Color.blue;
            
            var txt = cellGameObjects[cellIndex].GetComponentInChildren<TMP_Text>();
            txt.enabled = true;
            txt.text = celldata.mineNeigbourCount.ToString();
            txt.color = Color.Lerp(Color.green, Color.blue, cellDatas[cellIndex].mineNeigbourCount / 8f);

            if (cached != null)
            {
                cachedColor = Color.blue;
            }
            
            if (celldata.mineNeigbourCount > 0) return;
            
            var worldPos = cellGameObjects[cellIndex].transform.position;
            var neighboringIndices = GetNeighboringIndices(worldPos);
            for (int i = 0; i < neighboringIndices.Count; i++)
            {
                var neighbourIndex = neighboringIndices[i];
                ref var neighbourCellData = ref cellDatas[neighbourIndex];
                if (neighbourCellData.hasMine == false && neighbourCellData.explored == false)
                {
                    ExploreCell(neighbourIndex);
                }
            }
        }

        GameObject cached;
        Color cachedColor;
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            var worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
            int index = GetIndexByWorldPos(worldPos);
            var celldata = cellDatas[index];
            if (celldata.hasMine)
            {
                // GameOver
            }
            
            ExploreCell(index);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            
        }

        void IPointerMoveHandler.OnPointerMove(PointerEventData eventData)
        {
            var worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
            int index = GetIndexByWorldPos(worldPos);
            
            var go = cellGameObjects[index];
            if (cached != null) cached.GetComponent<Renderer>().material.color = cachedColor;
            var material = go.GetComponent<Renderer>().material;
            cached = go;
            cachedColor = material.color;
            material.color = Color.green;
        }
        
        static int[] GetDenseArr(int xSize, int ySize, int desiredTrueCellCount)
        {
            int totalCells = xSize * ySize;
            int[] result = new int[totalCells];
            
            // Calculate the number of "true" cells to put in each row
            int trueCellsPerRow = desiredTrueCellCount / ySize;
            int remainingTrueCells = desiredTrueCellCount % ySize;

            // Distribute "true" cells evenly across the rows
            System.Random rand = new System.Random();
            int[] rowIndices = new int[xSize];
            for (int row = 0; row < ySize; row++)
            {
                int trueCountForRow = trueCellsPerRow + (remainingTrueCells > 0 ? 1 : 0);
                remainingTrueCells--;

                for (int i = 0; i < xSize; i++)
                {
                    rowIndices[i] = 0;
                }

                // Randomize the position of "true" cells in the row
                for (int col = 0; col < xSize; col++)
                {
                    rowIndices[col] = row * xSize + col;
                }

                for (int j = 0; j < xSize; j++)
                {
                    int randIndex = rand.Next(j, rowIndices.Length);
                    (rowIndices[j], rowIndices[randIndex]) = (rowIndices[randIndex], rowIndices[j]);
                }

                for (int j = 0; j < trueCountForRow; j++)
                {
                    result[rowIndices[j]] = rowIndices[j];
                }
            }

            return Array.FindAll(result, (val) => val > 0);
        }

        void PlaceMines()
        {
            mineIndices = GetDenseArr(gridSize.x, gridSize.y, mineCount);

            for (int i = 0; i < mineIndices.Length; i++)
            {
                int index = mineIndices[i];
                var cell = cellGameObjects[index];
                cell.GetComponent<Renderer>().material.color = Color.red;
                cellDatas[index].hasMine = true;
                var neighbours = GetNeighboringIndices(cell.transform.position);
                for (int j = 0; j < neighbours.Count; j++)
                {
                    var neighbourIndex = neighbours[j];
                    cellDatas[neighbourIndex].mineNeigbourCount++;
                }
            }
        }
    }
}
