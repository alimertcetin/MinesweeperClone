using UnityEngine;
using XIV.Core.Collections;

namespace XIV.GridSystems
{
    public interface IGrid
    {
        Vector3 GridCenter { get; }
        Vector2 AreaSize { get; }
        Vector2Int CellCount { get; }

        DynamicArray<CellData> GetCells();
    }
}