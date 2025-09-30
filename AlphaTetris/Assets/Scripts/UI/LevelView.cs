using TMPro;
using UnityEngine;

namespace AlphaTetris {
  public class LevelView : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _linesText;
    [SerializeField] private string _levelFormat = "Level: {0}";
    [SerializeField] private string _linesFormat = "Lines: {0}";

    private void Awake() {
      if (_levelText == null || _linesText == null) {
        var texts = GetComponentsInChildren<TextMeshProUGUI>();
        if (_levelText == null && texts.Length > 0) {
          _levelText = texts[0];
        }
        if (_linesText == null && texts.Length > 1) {
          _linesText = texts[1];
        }
      }
    }

    public void SetValues(int level, int lines) {
      if (_levelText != null) {
        _levelText.text = string.Format(_levelFormat, level);
      }

      if (_linesText != null) {
        _linesText.text = string.Format(_linesFormat, lines);
      }
    }
  }
}
