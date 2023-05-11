using Minesweeper.GridSystems;
using Minesweeper.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using XIV.Core.Extensions;

namespace Minesweeper
{
    [RequireComponent(typeof(BoxCollider))]
    public class GameManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
    {
        [SerializeField] AudioClip selectionChangedClip;
        [SerializeField] AudioClip selectionClickedClip;
        [SerializeField] AudioClip mineExplosionClip;
        
        GridManager gridManager;
        public bool GameOver { get; private set; }
        bool gameOverUIEnabled;

        void Awake()
        {
            gridManager = FindObjectOfType<GridManager>();
            GetComponent<BoxCollider>().size = ((Vector3)gridManager.AreaSize).SetZ(transform.localScale.z + 0.1f) * 2f;
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (GameOver)
            {
                ShowGameOverUI();
                return;
            }

            var worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
            var cellData = gridManager.GetCellData(worldPos);
            // TODO : if exploring return
            if (cellData.isRevealed) return;
            
            gridManager.ChangeSelection(worldPos);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (GameOver) return;
            
            var worldPos = Camera.main.ScreenToWorldPoint(eventData.position);

            gridManager.PlaceMinesIfNotPlaced(worldPos);
            var cellData = gridManager.GetCellData(worldPos);
            
            if (cellData.isRevealed) return;

            if (cellData.hasMine)
            {
                GameOver = true;
                gridManager.BlastMines(cellData.index, () =>
                {
                    PlayClip(mineExplosionClip);
                }, ShowGameOverUI);
                return;
            }

            gridManager.ExploreCell(cellData.index, true, (_) => PlayClip(selectionClickedClip));
        }

        void IPointerMoveHandler.OnPointerMove(PointerEventData eventData)
        {
            if (GameOver) return;
            
            var worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
            if (gridManager.ChangeSelection(worldPos))
            {
                PlayClip(selectionChangedClip);
            }
        }

        void ShowGameOverUI()
        {
            if (gameOverUIEnabled) return;
            gameOverUIEnabled = true;
            FindObjectOfType<GameOverUI>().ShowUI();
        }

        void PlayClip(AudioClip clip)
        {
            AudioSource.PlayClipAtPoint(clip, transform.position);
        }
    }
}