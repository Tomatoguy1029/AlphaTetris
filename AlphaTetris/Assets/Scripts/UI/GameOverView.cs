using TMPro;
using UnityEngine;

namespace AlphaTetris {
  public class GameOverView : MonoBehaviour {
    [SerializeField] private GameObject _root;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _linesText;

    private void Awake() {
      if (_root == null) {
        _root = gameObject;
      }

      Hide();
    }

    public void Show(int score, int level, int lines) {
      if (_scoreText != null) {
        _scoreText.text = $"Score: {score}";
      }

      if (_levelText != null) {
        _levelText.text = $"Level: {level}";
      }

      if (_linesText != null) {
        _linesText.text = $"Lines: {lines}";
      }

      if (_root != null) {
        _root.SetActive(true);
      }
    }

    public void Hide() {
      if (_root != null) {
        _root.SetActive(false);
      }
    }
  }
}
