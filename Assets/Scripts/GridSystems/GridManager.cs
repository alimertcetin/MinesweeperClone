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
        [SerializeField] Vector2 areaSize;
        [Tooltip("Beginner: 9x9 grid with 10 mines, Intermediate: 16x16 grid with 40 mines, Expert: 16x30 grid with 99 mines")]
        [SerializeField] Vector2Int cellCount;
        [SerializeField] int mineCount;
        [SerializeField] float cellPadding;
        [SerializeField] GameObject cellPrefab;
        [SerializeField] GameObject[] cellGameObjects;
        CellData[] cellDatas;
        int[] mineIndices;
        Vector2Int cachedGridSize;
        Vector2 cachedAreaSize;
        float cachedPadding;
        public bool enableGizmos;
        
        #if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (enableGizmos == false) return;
            DrawBox(transform.position, areaSize * 0.5f);
            
            var cellSize = new Vector3(areaSize.x / cellCount.x, areaSize.y / cellCount.y, 0f);
            var start = transform.position - (Vector3)(areaSize * 0.5f) + (cellSize * 0.5f);
            
            for (int x = 0; x < cellCount.x; x++)
            {
                for (int y = 0; y < cellCount.y; y++)
                {
                    var pos = start + new Vector3(cellSize.x * x, cellSize.y * y, 0f);
                    DrawBox(pos, cellSize * 0.5f, 10);
                }
            }
        }
        #endif

        void Awake()
        {
            int length = cellCount.y * cellCount.x;
            cellGameObjects = new GameObject[length];
            cellDatas = new CellData[length];
        }

        void Init()
        {
            var cellSize = new Vector3(areaSize.x / cellCount.x, areaSize.y / cellCount.y, 0f);
            var start = transform.position - (Vector3)(areaSize * 0.5f) + (cellSize * 0.5f);
            
            for (int x = 0; x < cellCount.x; x++)
            {
                for (int y = 0; y < cellCount.y; y++)
                {
                    var pos = start + new Vector3(cellSize.x * x, cellSize.y * y, 0f);
                    var cell = CreateCell(x, y, pos, cellSize);
                    
                    int index = x * cellCount.y + y;
                    cellGameObjects[index] = cell;
                }
            }

            SetCollSize();
            PlaceMines();
        }

        void Update()
        {
            if (cachedGridSize != cellCount || cachedAreaSize != areaSize || Math.Abs(cachedPadding - cellPadding) > 0.01f)
            {
                for (int i = 0; i < cellGameObjects.Length; i++)
                {
                    Destroy(cellGameObjects[i]);
                }

                Init();
                cachedGridSize = cellCount;
                cachedAreaSize = areaSize;
                cachedPadding = cellPadding;
            }
        }

        int GetIndexByWorldPos(Vector3 worldPos)
        {
            var cellSize = new Vector3(areaSize.x / cellCount.x, areaSize.y / cellCount.y, 0f) - (Vector3.one * cellPadding);
            var localPos = transform.InverseTransformPoint(worldPos);
            var xLength = areaSize.x;
            var yLength = areaSize.y;

            float normalizedX = (localPos.x + xLength / 2f - cellSize.x * 0.5f) / xLength;
            float normalizedY = (localPos.y + yLength / 2f - cellSize.y * 0.5f) / yLength;
            normalizedX = Mathf.Clamp(normalizedX, 0, normalizedX);
            normalizedY = Mathf.Clamp(normalizedY, 0, normalizedY);
            Debug.Log(nameof(normalizedX) + " : " + normalizedX);
            Debug.Log(nameof(normalizedY) + " : " + normalizedY);

            int x = (int)Math.Round(cellCount.x * normalizedX);
            int y = (int)Math.Round(cellCount.y * normalizedY); 
            int index = x * cellCount.y + y;
            index = Mathf.Clamp(index, 0, cellGameObjects.Length - 1);
            return index;
        }

        void SetCollSize()
        {
            var coll = GetComponent<BoxCollider>();
            coll.size = new Vector3(areaSize.x, areaSize.y, coll.size.z);
        }

        static void DrawBox(Vector3 pos, Vector3 halfExtend, float duration = 0)
        {
            var bottomLeft = pos - halfExtend;
            var topRight = pos + halfExtend;
            Debug.DrawLine(bottomLeft, topRight, Color.black, 5f);
                    
            Debug.DrawLine(bottomLeft, new Vector3(topRight.x, bottomLeft.y), Color.red, duration);
            Debug.DrawLine(new Vector3(topRight.x, bottomLeft.y), topRight, Color.green, duration);
            Debug.DrawLine(topRight, new Vector3(bottomLeft.x, topRight.y), Color.red, duration);
            Debug.DrawLine(new Vector3(bottomLeft.x, topRight.y), bottomLeft, Color.green, duration);
        }

        GameObject CreateCell(int x, int y, Vector3 pos, Vector3 cellSize)
        {
            GameObject cell = Instantiate(cellPrefab, this.transform, true);
            cell.gameObject.name = $"Cell_({x},{y})";
            cell.transform.position = pos;
            var size3D = cellSize - (Vector3.one * cellPadding);
            size3D.z = cell.transform.localScale.z;
            cell.transform.localScale = size3D;
            
            cell.GetComponentInChildren<TMP_Text>().enabled = false;
            return cell;
        }
        
        List<int> GetNeighboringIndices(Vector3 worldPos)
        {
            List<int> neighborIndices = new List<int>();

            int centerIndex = GetIndexByWorldPos(worldPos);
            int x = centerIndex / cellCount.y;
            int y = centerIndex % cellCount.y;

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0) continue;

                    int neighborX = x + i;
                    int neighborY = y + j;

                    if (neighborX < 0 || neighborX >= cellCount.x || neighborY < 0 || neighborY >= cellCount.y) continue;

                    int neighborIndex = neighborX * cellCount.y + neighborY;
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
                Debug.LogWarning("Game Over");
                return;
            }
            
            ExploreCell(index);
            int exploredCount = 0;
            for (int i = 0; i < cellDatas.Length; i++)
            {
                if (cellDatas[i].hasMine || cellDatas[i].explored == false) continue;
                exploredCount++;
            }

            var left = cellDatas.Length - mineIndices.Length - exploredCount;
            if (left == 0)
            {
                Debug.LogWarning("You Win");
            }
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
            mineIndices = GetDenseArr(cellCount.x, cellCount.y, mineCount);

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
