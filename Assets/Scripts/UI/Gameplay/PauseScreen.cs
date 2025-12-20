using UnityEngine;
using UnityEngine.UI;
using TowerDefence.Core;
using TowerDefence.Game;

namespace TowerDefence.UI
{
    public class PauseScreen : BaseScreen
    {
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _menuButton;

        private IEventBus _eventBus;

        protected override void Awake()
        {
            base.Awake();
            _eventBus = Services.Get<IEventBus>();

            if (_resumeButton != null)
            {
                _resumeButton.onClick.AddListener(OnResumeClicked);
            }

            if (_menuButton != null)
            {
                _menuButton.onClick.AddListener(OnMenuClicked);
            }
        }

        private void OnResumeClicked()
        {
            _eventBus.Publish(new ResumeGameRequestedEvent());
        }

        private void OnMenuClicked()
        {
            _eventBus.Publish(new ReturnToMenuRequestedEvent());
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_resumeButton != null)
            {
                _resumeButton.onClick.RemoveListener(OnResumeClicked);
            }

            if (_menuButton != null)
            {
                _menuButton.onClick.RemoveListener(OnMenuClicked);
            }
        }
    }
}