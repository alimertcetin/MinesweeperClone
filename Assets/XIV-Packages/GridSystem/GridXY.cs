using System;
using UnityEngine;
using XIV.Core;
using XIV.Core.Collections;

namespace XIV.GridSystems
{
    public class GridXY : IGrid
    {
        public Vector3 GridCenter
        {
            get => gridCenter;
            set
            {
                isDirty = gridCenter != value || isDirty;
                gridCenter = value;
            }
        }
        
        public Vector2 AreaSize
        {
            get => areaSize;
            set
            {
                isDirty = areaSize != value || isDirty;
                areaSize = value;
            }
        }
        
        public Vector2Int CellCount
        {
            get => cellCount;
            set
            {
                isDirty = cellCount != value || isDirty;
                cellCount = value;
            }
        }
        
        public Vector3 CellSize => new Vector3(areaSize.x / cellCount.x, areaSize.y / cellCount.y, 0f);

        public bool IsDirty => isDirty;

        Vector3 gridCenter;
        Vector2 areaSize;
        Vector2Int cellCount;
        bool isDirty;

        readonly DynamicArray<CellData> cellDatas;
        readonly DynamicArray<IGridListener> gridListeners;
        static readonly DynamicArray<int> neighbourIndicesBuffer = new DynamicArray<int>(8);
        static readonly DynamicArray<CellData> cellDataBuffer = new DynamicArray<CellData>(8);

        public GridXY(Vector3 gridCenter, Vector2 areaSize, Vector2Int cellCount)
        {
            this.gridCenter = gridCenter;
            this.areaSize = areaSize;
            this.cellCount = cellCount;
            int length = cellCount.y * cellCount.x;
            this.cellDatas = new DynamicArray<CellData>(length);
            this.gridListeners = new DynamicArray<IGridListener>(2);
            
            CreateCells();
        }

        void CreateCells()
        {
            cellDatas.Clear();
            int length = cellCount.y * cellCount.x;
            for (int i = 0; i < length; i++)
            {
                this.cellDatas.Add();
            }
            var cellSize = CellSize;
            var start = gridCenter - (Vector3)(areaSize * 0.5f) + (cellSize * 0.5f);
            for (int x = 0; x < cellCount.x; x++)
            {
                for (int y = 0; y < cellCount.y; y++)
                {
                    var pos = start + new Vector3(cellSize.x * x, cellSize.y * y, 0f);
                    int index = x * cellCount.y + y;
                    cellDatas[index] = new CellData(index, x, y, pos, cellSize);
                }
            }
        }

        public void RebuildIfDirty()
        {
            if (isDirty)
            {
                CreateCells();
                InformListeners();
            }

            isDirty = false;
        }

        void InformListeners()
        {
            for (int i = 0; i < gridListeners.Count; i++)
            {
                gridListeners[i].OnGridChanged(this);
            }
        }

        public void AddListener(IGridListener listener)
        {
            if (gridListeners.Contains(ref listener)) return;
            gridListeners.Add() = listener;
        }

        public void RemoveListener(IGridListener listener)
        {
            int index = gridListeners.IndexOf(ref listener);
            if (index >= 0) gridListeners.RemoveAt(index);
        }

        public int GetIndexByWorldPos(Vector3 worldPos)
        {
            var localPos = worldPos - gridCenter;
            var halfAreaSize = areaSize * 0.5f;
            var xLength = areaSize.x;
            var yLength = areaSize.y;
            var cellSize = CellSize;
            
            const float ERROR = 0.1f;
            halfAreaSize.x -= ERROR;
            halfAreaSize.y -= ERROR;
            
            localPos.x = Mathf.Clamp(localPos.x, -halfAreaSize.x, halfAreaSize.x);
            localPos.y = Mathf.Clamp(localPos.y, -halfAreaSize.y, halfAreaSize.y);

            float normalizedX = (localPos.x + xLength / 2f - cellSize.x * 0.5f) / xLength;
            float normalizedY = (localPos.y + yLength / 2f - cellSize.y * 0.5f) / yLength;

            int x = (int)Math.Round(cellCount.x * normalizedX);
            int y = (int)Math.Round(cellCount.y * normalizedY);
            int index = x * cellCount.y + y;
            index = Mathf.Clamp(index, 0, cellDatas.Count - 1);
            return index;
        }

        public DynamicArray<int> GetNeighbourIndices(int centerIndex)
        {
            neighbourIndicesBuffer.Clear();
            int x = centerIndex / cellCount.y;
            int y = centerIndex % cellCount.y;

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0) continue;

                    int neighbourX = x + i;
                    int neighbourY = y + j;

                    if (neighbourX < 0 || neighbourX >= cellCount.x || neighbourY < 0 || neighbourY >= cellCount.y) continue;

                    int neighborIndex = neighbourX * cellCount.y + neighbourY;
                    neighbourIndicesBuffer.Add() = neighborIndex;
                }
            }

            return neighbourIndicesBuffer;
        }

        public DynamicArray<int> GetNeighbourIndices(Vector3 worldPos)
        {
            int centerIndex = GetIndexByWorldPos(worldPos);
            return GetNeighbourIndices(centerIndex);
        }

        public DynamicArray<CellData> GetCells()
        {
            return GetCells(gridCenter, areaSize, cellCount);
        }

        public static DynamicArray<CellData> GetCells(Vector3 gridCenter, Vector2 areaSize, Vector2Int cellCount)
        {
            int length = cellCount.y * cellCount.x;
            cellDataBuffer.Clear();
            for (int i = 0; i < length; i++)
            {
                cellDataBuffer.Add();
            }
            Vector3 cellSize = new Vector3(areaSize.x / cellCount.x, areaSize.y / cellCount.y, 0f);
            var start = gridCenter - (Vector3)(areaSize * 0.5f) + (cellSize * 0.5f);
            for (int x = 0; x < cellCount.x; x++)
            {
                for (int y = 0; y < cellCount.y; y++)
                {
                    var pos = start + new Vector3(cellSize.x * x, cellSize.y * y, 0f);
                    int index = x * cellCount.y + y;
                    cellDataBuffer[index] = new CellData(index, x, y, pos, cellSize);
                }
            }

            return cellDataBuffer;
        }
        
        public DynamicArray<GridXY> DivideGridIntoSubGrids(Vector2Int subGridCellCount)
        {
            DynamicArray<GridXY> subgrids = new DynamicArray<GridXY>(2);

            for (int i = 0; i < cellCount.x; i+= subGridCellCount.x)
            {
                for (int j = 0; j < cellCount.y; j+= subGridCellCount.y)
                {
                    subgrids.Add() = CreateSubGrid(i, j, subGridCellCount);
                }
            }
            return subgrids;
        }

        GridXY CreateSubGrid(int startX, int startY, Vector2Int subGridCellCount)
        {
            Vector3 subGridCenter = Vector3.zero;
            int validTotalCellCount = 0;

            int loopX = startX + subGridCellCount.x;
            int loopY = startY + subGridCellCount.y;
            for (int i = startX; i < loopX && i < cellCount.x; i++)
            {
                for (int j = startY; j < loopY && j < cellCount.y; j++)
                {
                    int index = i * cellCount.y + j;
                    validTotalCellCount++;
                    subGridCenter += cellDatas[index].worldPos;
                }
            }

            Vector2Int validSubGridCellCount = new Vector2Int(
                Mathf.Min(subGridCellCount.x, this.cellCount.x - startX), 
                Mathf.Min(subGridCellCount.y, this.cellCount.y - startY));

            Vector2 size = CellSize;
            size.x *= validSubGridCellCount.x;
            size.y *= validSubGridCellCount.y;

            return new GridXY(subGridCenter / validTotalCellCount, size, validSubGridCellCount);
        }
#if UNITY_EDITOR
        
        
        public static void DisplayGrid(Vector3 gridCenter, Vector2 areaSize, Vector2Int cellCount, float duration = 0f)
        {
            var cells = GetCells(gridCenter, areaSize, cellCount);
            for (var i = 0; i < cells.Count; i++)
            {
                ref CellData cellData = ref cells[i];
                XIVDebug.DrawRectangle(cellData.worldPos, cellData.cellSize * 0.5f, duration);
            }
        }
        
#endif
    }
}