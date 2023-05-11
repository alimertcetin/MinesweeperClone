using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using XIV.Core.Utils;
using XIV.TweenSystem;

namespace Minesweeper.UI
{
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] GameObject uiGameObject;
        [SerializeField] Button btn_Restart;
        [SerializeField] float tweenDuration;

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
            uiGameObject.SetActive(true);
            uiGameObject.transform.XIVTween()
                .Scale(Vector3.zero, Vector3.one, tweenDuration, EasingFunction.EaseInOutElastic)
                .Start();
        }

        public void CloseUI(Action onClosed = null)
        {
            uiGameObject.transform.XIVTween()
                .Scale(uiGameObject.transform.localScale, Vector3.zero, tweenDuration, EasingFunction.EaseInOutElastic)
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