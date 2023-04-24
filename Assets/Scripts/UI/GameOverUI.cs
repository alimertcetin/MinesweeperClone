using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
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
            uiGameObject.transform.localScale = Vector3.zero;
            uiGameObject.SetActive(true);
            uiGameObject.AddScaleTween(Vector3.one, tweenDuration);
        }

        public void CloseUI()
        {
            uiGameObject.AddScaleTween(Vector3.zero, tweenDuration, (_) =>
            {
                uiGameObject.SetActive(false);
            });
        }

        void OnRestartClicked()
        {
            uiGameObject.AddScaleTween(Vector3.zero, tweenDuration, (_) =>
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            });
        }
    }
    
    public static class TweenExtensions
    {
        public static void AddScaleTween(this GameObject go, Vector3 to, float duration)
        {
            var scaleTween = go.GetComponent<ScaleTween>();
            if (scaleTween == null) scaleTween = go.AddComponent<ScaleTween>();
            
            scaleTween.to = to;
            scaleTween.moveSpeed = (to - go.transform.localScale).magnitude / duration;
        }
        
        public static void AddScaleTween(this GameObject go, Vector3 to, float duration, Action<GameObject> callback)
        {
            var scaleTween = go.GetComponent<ScaleTween>();
            if (scaleTween == null) scaleTween = go.AddComponent<ScaleTween>();
            
            scaleTween.to = to;
            scaleTween.moveSpeed = (to - go.transform.localScale).magnitude / duration;
            scaleTween.OnTweenEnd += callback;
        }
    }
    
    public class ScaleTween : MonoBehaviour
    {
        public Vector3 to;
        public float moveSpeed;
        public event Action<GameObject> OnTweenEnd;

        void Update()
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, to, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.localScale, to) < Mathf.Epsilon)
            {
                OnTweenEnd?.Invoke(this.gameObject);
                Destroy(this);
            }
        }
    }

}