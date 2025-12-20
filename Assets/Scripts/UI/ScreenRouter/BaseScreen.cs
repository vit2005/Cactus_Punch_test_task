using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using TowerDefence.Core;
using UnityEngine;

namespace TowerDefence.UI
{
    /// <summary>
    /// Base class for UI screens. Handles auto-registration.
    /// Override OnShow/OnHide for custom logic.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class BaseScreen : MonoBehaviour, IScreen
    {
        [SerializeField] private string _screenId;

        private CanvasGroup _canvasGroup;
        private Tween _fadeTween;
        private bool _isDestroyed;
        private bool _isRegistered;

        public string ScreenId => _screenId;

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();

            // Immediately register on Awake
            RegisterSelf();

            // Initially hide the screen
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }

            gameObject.SetActive(false);
        }

        protected virtual void OnEnable()
        {
            // Ensure registration even if object was disabled/enabled
            if (!_isRegistered)
            {
                RegisterSelf();
            }
        }

        protected virtual void OnDisable()
        {
            // Don't unregister on disable, only on destroy
        }

        protected virtual void OnDestroy()
        {
            _isDestroyed = true;
            _fadeTween?.Kill();
            UnregisterSelf();
        }

        private void RegisterSelf()
        {
            if (_isDestroyed || _isRegistered || string.IsNullOrEmpty(_screenId))
            {
                return;
            }

            if (Services.TryGet<IUIRegistry>(out var registry))
            {
                registry.RegisterScreen(this);
                _isRegistered = true;
                Debug.Log($"[BaseScreen] Registered: {_screenId}");
            }
            else
            {
                Debug.LogWarning($"[BaseScreen] UIRegistry not available yet for {_screenId}");
            }
        }

        private void UnregisterSelf()
        {
            if (string.IsNullOrEmpty(_screenId) || !_isRegistered)
            {
                return;
            }

            if (Services.TryGet<IUIRegistry>(out var registry))
            {
                registry.UnregisterScreen(this);
                _isRegistered = false;
                Debug.Log($"[BaseScreen] Unregistered: {_screenId}");
            }
        }

        public virtual async Task ShowAsync(CancellationToken cancellationToken = default)
        {
            if (_isDestroyed || _canvasGroup == null)
            {
                return;
            }

            gameObject.SetActive(true);

            _fadeTween?.Kill();

            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            OnShow();

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            _fadeTween = _canvasGroup.DOFade(1f, 0.3f)
                .OnComplete(() =>
                {
                    if (_canvasGroup != null && !_isDestroyed)
                    {
                        _canvasGroup.interactable = true;
                        _canvasGroup.blocksRaycasts = true;
                    }
                });

            try
            {
                await _fadeTween.AsyncWaitForCompletion();
            }
            catch (System.Exception)
            {
                // Tween was killed or object was destroyed
            }
        }

        public virtual async Task HideAsync(CancellationToken cancellationToken = default)
        {
            if (_isDestroyed || _canvasGroup == null)
            {
                return;
            }

            _fadeTween?.Kill();

            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            OnHide();

            if (cancellationToken.IsCancellationRequested)
            {
                gameObject.SetActive(false);
                return;
            }

            _fadeTween = _canvasGroup.DOFade(0f, 0.3f)
                .OnComplete(() =>
                {
                    if (!_isDestroyed && gameObject != null)
                    {
                        gameObject.SetActive(false);
                    }
                });

            try
            {
                await _fadeTween.AsyncWaitForCompletion();
            }
            catch (System.Exception)
            {
                // Tween was killed or object was destroyed
            }
        }

        protected virtual void OnShow() { }

        protected virtual void OnHide() { }
    }
}