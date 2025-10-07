using TMPro;
using UnityEngine;

namespace AlphaTetris {
  public class StatsView : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _linesText;
    [SerializeField] private string _scoreFormat = "Score\n{0}";
    [SerializeField] private string _levelFormat = "Level\n{0}";
    [SerializeField] private string _linesFormat = "Lines\n{0}";

    private void Awake() {
      if (_scoreText == null || _levelText == null || _linesText == null) {
        var texts = GetComponentsInChildren<TextMeshProUGUI>();
        if (_scoreText == null && texts.Length > 0) {
          _scoreText = texts[0];
        }
        if (_levelText == null && texts.Length > 1) {
          _levelText = texts[1];
        }
        if (_linesText == null && texts.Length > 2) {
          _linesText = texts[2];
        }
      }
    }

    public void SetValues(int score, int level, int lines) {
      if (_scoreText != null) {
        _scoreText.text = string.Format(_scoreFormat, score);
      }

      if (_levelText != null) {
        _levelText.text = string.Format(_levelFormat, level);
      }

      if (_linesText != null) {
        _linesText.text = string.Format(_linesFormat, lines);
      }
    }
  }
}
