using UnityEngine;
using UnityEngine.UI;

public class ButtonClickDebug : MonoBehaviour
{
    private void Start()
    {
        var button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
        }
    }

    private void OnButtonClicked()
    {
        Debug.Log("[ButtonClickDebug] Button clicked");
    }
}
