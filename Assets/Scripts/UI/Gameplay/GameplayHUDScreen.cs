using UnityEngine;
using UnityEngine.UI;
using TowerDefence.Core;
using TowerDefence.Game;
using TMPro;

namespace TowerDefence.UI
{
    public class GameplayHUDScreen : BaseScreen
    {
        [Header("UI Elements")]
        [SerializeField] private Button _pauseButton;
        [SerializeField] private TextMeshProUGUI _healthText;
        [SerializeField] private Slider _healthSlider;

        private IEventBus _eventBus;
        private HealthProvider _playerHealth;

        protected override void Awake()
        {
            base.Awake();
            _eventBus = Services.Get<IEventBus>();

            if (_pauseButton != null)
            {
                _pauseButton.onClick.AddListener(OnPauseClicked);
            }
        }

        protected override void OnShow()
        {
            base.OnShow();
            FindPlayerHealth();
        }

        private void Update()
        {
            UpdateHealthUI();
        }

        private void FindPlayerHealth()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && player.TryGetComponent<HealthProvider>(out var hp))
            {
                _playerHealth = hp;
            }
        }

        private void UpdateHealthUI()
        {
            if (_playerHealth == null) return;

            float healthPercent = _playerHealth.HP;

            if (_healthSlider != null)
            {
                _healthSlider.value = healthPercent / 100f;
            }

            if (_healthText != null)
            {
                _healthText.text = $"HP: {Mathf.RoundToInt(healthPercent)}%";
            }
        }

        private void OnPauseClicked()
        {
            _eventBus.Publish(new PauseGameRequestedEvent());
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_pauseButton != null)
            {
                _pauseButton.onClick.RemoveListener(OnPauseClicked);
            }
        }
    }
}