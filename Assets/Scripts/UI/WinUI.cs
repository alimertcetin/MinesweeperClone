using System;
using Minesweeper.GridSystems;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using XIV.Core.Utils;
using XIV.TweenSystem;

namespace Minesweeper.UI
{
    public class WinUI : MonoBehaviour
    {
        [SerializeField] GameObject uiGameObject;
        [SerializeField] TMP_Text txt_Score;
        [SerializeField] Button btn_Restart;
        [SerializeField] float tweenDuration;
        float timePaseed;

        void Update()
        {
            timePaseed += Time.deltaTime;
        }

        void OnEnable()
        {
            btn_Restart.onClick.AddListener(OnRestartClicked);
        }

        void OnDisable()
        {
            btn_Restart.onClick.RemoveListener(OnRestartClicked);
        }

        public void ShowUI()
        {
            var gridManager = FindObjectOfType<GridManager>();

            var cellCount = gridManager.cellCount;
            var score = (1000 * (cellCount.x * cellCount.y)) / (timePaseed * (1 + gridManager.mineCount));
            txt_Score.text = ((int)score).ToString();
            txt_Score.enabled = true;
            uiGameObject.SetActive(true);
            uiGameObject.transform.XIVTween()
                .Scale(Vector3.zero, Vector3.one, tweenDuration, EasingFunction.EaseOutBounce)
                .OnComplete(() =>
                {
                    txt_Score.XIVTween()
                        .Scale(Vector3.zero, Vector3.one, 0.5f, EasingFunction.EaseOutExpo)
                        .Start();
                })
                .Start();
        }

        public void CloseUI(Action onClosed = null)
        {
            uiGameObject.transform.XIVTween()
                .Scale(uiGameObject.transform.localScale, Vector3.zero, tweenDuration, EasingFunction.EaseOutBounce)
                .OnComplete(() =>
                {
                    uiGameObject.SetActive(false);
                    onClosed?.Invoke();
                })
                .Start();
        }

        void OnRestartClicked()
        {
            CloseUI(() => SceneManager.LoadScene(SceneManager.GetActiveScene().name));
        }
    }
}