using UnityEngine;

namespace AlphaTetris {
  public class PauseView : MonoBehaviour {
    [SerializeField] private GameObject _root;

    private void Awake() {
      if (_root == null) {
        _root = gameObject;
      }
    }

    public void Show() {
      if (_root != null && !_root.activeSelf) {
        _root.SetActive(true);
      }
    }

    public void Hide() {
      if (_root != null && _root.activeSelf) {
        _root.SetActive(false);
      }
    }
  }
}
