using UnityEngine;
using UnityEngine.UI;

public class ConfigurationPanelUI : MonoBehaviour
{
    public Button RetourButton;
    public GameObject ConfigurationPanel;
    public GameObject LoginPanel;

    void Start()
    {
        if (RetourButton != null)
            RetourButton.onClick.AddListener(OnRetour);
    }

    void OnRetour()
    {
        if (ConfigurationPanel != null) ConfigurationPanel.SetActive(false);
        if (LoginPanel != null) LoginPanel.SetActive(true);
    }
}
