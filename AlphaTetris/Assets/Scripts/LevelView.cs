using TMPro;
using UnityEngine;

namespace AlphaTetris {
  public class LevelView : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI _labelText;
    [SerializeField] private TextMeshProUGUI _valueText;
    [SerializeField] private string _labelFormat = "Level:";
    [SerializeField] private string _valueFormat = "{0}";

    public void UpdateText(int value) {
      if (_valueText == null) {
        Debug.LogWarning("LevelView: Value text reference is missing.");
        return;
      }

      if (_labelText != null) {
        _labelText.text = _labelFormat;
      }

      _valueText.text = string.Format(_valueFormat, value);
    }
  }
}
