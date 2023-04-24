using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using XIV;

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
        [SerializeField] Material[] mineClickedMaterials;
        [SerializeField] Material[] emptyFieldMaterials;
        [SerializeField] float materialChangeDuration = 0.75f;
        [SerializeField] AudioClip selectionChangedClip;
        [SerializeField] AudioClip selectionClickedClip;

        AudioSource audioSource;
        
        GameObject[] cellGameObjects;
        CellData[] cellDatas;
        int[] mineIndices;
        int acitveCell = -1;
        Material previousMaterial;
        Vector3 CellSize => new Vector3(areaSize.x / cellCount.x, areaSize.y / cellCount.y, 0f);

        bool inputDisabled;

#if UNITY_EDITOR
        public bool enableGizmos;
        Vector2 cachedAreaSize;
#endif

        void Awake()
        {
            CreateCells();
            audioSource = new GameObject("GridManager-AudioSource").AddComponent<AudioSource>();
        }

        [ContextMenu(nameof(CreateCells))]
        void CreateCells()
        {
            ClearCells();

            int length = cellCount.y * cellCount.x;
            if (length < 0) return;
            cellGameObjects = new GameObject[length];
            cellDatas = new CellData[length];
            if (length != cellGameObjects.Length)
            {
                Array.Resize(ref cellGameObjects, length);
                Array.Resize(ref cellDatas, length);
            }
            
            var start = transform.position - (Vector3)(areaSize * 0.5f) + (CellSize * 0.5f);
            
            for (int x = 0; x < cellCount.x; x++)
            {
                for (int y = 0; y < cellCount.y; y++)
                {
                    var pos = start + new Vector3(CellSize.x * x, CellSize.y * y, 0f);
                    var cell = CreateCell(x, y, pos, CellSize);
                    
                    int index = x * cellCount.y + y;
                    cellGameObjects[index] = cell;
                }
            }
            
            var coll = GetComponent<BoxCollider>();
            coll.size = new Vector3(areaSize.x, areaSize.y, coll.size.z);
            PlaceMines();

#if UNITY_EDITOR
            cachedAreaSize = areaSize;
#endif
        }

        [ContextMenu(nameof(ClearCells))]
        void ClearCells()
        {
            if (cellGameObjects != null && cellGameObjects.Length > 0)
            {
                for (int i = 0; i < cellGameObjects.Length; i++)
                {
                    if (Application.isPlaying) Destroy(cellGameObjects[i]);
                    else DestroyImmediate(cellGameObjects[i]);
                }

                Array.Clear(cellGameObjects, 0, cellGameObjects.Length);
            }

            for (int i = 0; i < transform.childCount; i++)
            {
                if (Application.isPlaying) Destroy(transform.GetChild(i));
                else DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }

#if UNITY_EDITOR
        void Update()
        {
            if (cachedAreaSize != areaSize)
            {
                ResizeCells();
                cachedAreaSize = areaSize;
            }
        }
#endif
        
        void ResizeCells()
        {
            var start = transform.position - (Vector3)(areaSize * 0.5f) + (CellSize * 0.5f);

            for (int x = 0; x < cellCount.x; x++)
            {
                for (int y = 0; y < cellCount.y; y++)
                {
                    int index = x * cellCount.y + y;
                    var pos = start + new Vector3(CellSize.x * x, CellSize.y * y, 0f);
                    var cell = cellGameObjects[index];
                    cell.transform.position = pos;
                    var size3D = CellSize - (Vector3.one * cellPadding);
                    size3D.z = cell.transform.localScale.z;
                    cell.transform.localScale = size3D;
                }
            }
        }

        void PlaceMines()
        {
            mineIndices = GetDenseArr(cellCount.x, cellCount.y, mineCount);

            for (int i = 0; i < mineIndices.Length; i++)
            {
                int index = mineIndices[i];
                var cell = cellGameObjects[index];
                cellDatas[index].hasMine = true;
                var neighbours = GetNeighboringIndices(cell.transform.position);
                for (int j = 0; j < neighbours.Count; j++)
                {
                    var neighbourIndex = neighbours[j];
                    cellDatas[neighbourIndex].mineNeigbourCount++;
                }
            }
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

        int GetIndexByWorldPos(Vector3 worldPos)
        {
            var cellSize = CellSize - (Vector3.one * cellPadding);
            var localPos = transform.InverseTransformPoint(worldPos);
            var xLength = areaSize.x;
            var yLength = areaSize.y;

            float normalizedX = (localPos.x + xLength / 2f - cellSize.x * 0.5f) / xLength;
            float normalizedY = (localPos.y + yLength / 2f - cellSize.y * 0.5f) / yLength;
            normalizedX = Mathf.Clamp(normalizedX, 0, normalizedX);
            normalizedY = Mathf.Clamp(normalizedY, 0, normalizedY);

            int x = (int)Math.Round(cellCount.x * normalizedX);
            int y = (int)Math.Round(cellCount.y * normalizedY); 
            int index = x * cellCount.y + y;
            index = Mathf.Clamp(index, 0, cellGameObjects.Length - 1);
            return index;
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
            var materialChanger = cellGameObjects[cellIndex].AddComponent<MaterialChanger>();
            materialChanger.duration = materialChangeDuration;
            materialChanger.materials = emptyFieldMaterials;
            var nearMines = celldata.mineNeigbourCount;
            materialChanger.OnDestroyed += (go) =>
            {
                var txt = go.GetComponentInChildren<TMP_Text>();
                txt.enabled = true;
                txt.text = nearMines.ToString();
            };
            
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

        IEnumerator Explore(int index)
        {
            ExploreCell(index);
            yield return new WaitForSeconds(materialChangeDuration * emptyFieldMaterials.Length);
            inputDisabled = false;
        }

        IEnumerator BlastAllMines()
        {
            for (int i = 0; i < mineIndices.Length; i++)
            {
                int index = mineIndices[i];
                if (cellGameObjects[index].GetComponent<MaterialChanger>() != null) continue;
                
                var materialChanger = cellGameObjects[index].AddComponent<MaterialChanger>();
                materialChanger.duration = materialChangeDuration;
                materialChanger.materials = mineClickedMaterials;
                yield return new WaitForSeconds(materialChangeDuration * 0.5f);
            }
            FindObjectOfType<GameOverUI>().ShowUI();
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            audioSource.PlayOneShot(selectionClickedClip);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (inputDisabled) return;
            inputDisabled = true;
            ResetActiveCell();
            acitveCell = -1;
            previousMaterial = null;
            
            var worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
            int index = GetIndexByWorldPos(worldPos);
            var celldata = cellDatas[index];
            if (celldata.hasMine)
            {
                var materialChanger = cellGameObjects[index].AddComponent<MaterialChanger>();
                materialChanger.duration = materialChangeDuration;
                materialChanger.materials = mineClickedMaterials;
                StartCoroutine(BlastAllMines());
                return;
            }

            StartCoroutine(Explore(index));
            int exploredCount = 0;
            for (int i = 0; i < cellDatas.Length; i++)
            {
                if (cellDatas[i].hasMine || cellDatas[i].explored == false) continue;
                exploredCount++;
            }

            var left = cellDatas.Length - mineIndices.Length - exploredCount;
            if (left == 0)
            {
                FindObjectOfType<GameOverUI>().ShowUI();
            }
        }

        void ResetActiveCell()
        {
            if (acitveCell == -1) return;
            
            var previousRenderer = cellGameObjects[acitveCell].GetComponent<Renderer>();
            previousRenderer.material = previousMaterial;
        }

        void IPointerMoveHandler.OnPointerMove(PointerEventData eventData)
        {
            if (inputDisabled) return;
            
            var worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
            int index = GetIndexByWorldPos(worldPos);

            if (acitveCell == index) return;
            ResetActiveCell();
            acitveCell = index;
            audioSource.PlayOneShot(selectionChangedClip);
            var currentRenderer = cellGameObjects[acitveCell].GetComponent<Renderer>();
            previousMaterial = currentRenderer.material;
            currentRenderer.material = emptyFieldMaterials[0];
#if UNITY_EDITOR
            XIVDebug.DrawBox(cellGameObjects[acitveCell].transform.position, CellSize * 0.5f, 0.25f);
#endif
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
        
#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (enableGizmos == false) return;
            XIVDebug.DrawBox(transform.position, areaSize * 0.5f);
            
            var start = transform.position - (Vector3)(areaSize * 0.5f) + (CellSize * 0.5f);
            
            for (int x = 0; x < cellCount.x; x++)
            {
                for (int y = 0; y < cellCount.y; y++)
                {
                    var pos = start + new Vector3(CellSize.x * x, CellSize.y * y, 0f);
                    XIVDebug.DrawBox(pos, CellSize * 0.5f, 10);
                }
            }
        }
#endif
        
    }
}