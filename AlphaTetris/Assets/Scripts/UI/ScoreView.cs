using TMPro;
using UnityEngine;

namespace AlphaTetris {
  public class ScoreView : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private string _format = "Score: {0}";

    private void Awake() {
      if (_text == null) {
        _text = GetComponentInChildren<TextMeshProUGUI>();
      }
    }

    public void SetScore(int score) {
      if (_text == null) {
        return;
      }

      _text.text = string.Format(_format, score);
    }
  }
}
