using UnityEngine;
using UnityEngine.UI;
using TowerDefence.Core;
using TowerDefence.Game;

namespace TowerDefence.UI
{
    public class GameOverScreen : BaseScreen
    {
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _menuButton;

        private IEventBus _eventBus;

        protected override void Awake()
        {
            base.Awake();
            _eventBus = Services.Get<IEventBus>();

            if (_restartButton != null)
            {
                _restartButton.onClick.AddListener(OnRestartClicked);
            }

            if (_menuButton != null)
            {
                _menuButton.onClick.AddListener(OnMenuClicked);
            }
        }

        private void OnRestartClicked()
        {
            // Restart game by loading gameplay scene again
            _eventBus.Publish(new StartGameRequestedEvent());
        }

        private void OnMenuClicked()
        {
            _eventBus.Publish(new ReturnToMenuRequestedEvent());
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_restartButton != null)
            {
                _restartButton.onClick.RemoveListener(OnRestartClicked);
            }

            if (_menuButton != null)
            {
                _menuButton.onClick.RemoveListener(OnMenuClicked);
            }
        }
    }
}