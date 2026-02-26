using UnityEngine;

/// <summary>
/// Скрывает главные кнопки (Patient, Super, List), когда открыта любая из менюшек;
/// показывает их снова, когда все меню закрыты.
/// Добавь на сцену (например на объект с PatientsListUI), укажи MainButtons и MenuPanels.
/// </summary>
public class MainMenuButtonsController : MonoBehaviour
{
    [Tooltip("Кнопки, которые скрываются при открытии любой менюшки (Patient, Super, List).")]
    public GameObject[] MainButtons;

    [Tooltip("Панели меню — кнопки показываются только когда все эти панели закрыты.")]
    public GameObject[] MenuPanels;

    public static MainMenuButtonsController Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void Update()
    {
        if (AnyMenuOpen())
            SetMainButtonsVisible(false);
    }

    /// <summary>Вызвать при открытии любой менюшки.</summary>
    public void OnMenuOpened()
    {
        SetMainButtonsVisible(false);
    }

    /// <summary>Вызвать при закрытии любой менюшки. Показывает кнопки только если все меню закрыты.</summary>
    public void OnMenuClosed()
    {
        if (AnyMenuOpen())
            return;
        SetMainButtonsVisible(true);
    }

    bool AnyMenuOpen()
    {
        if (MenuPanels == null) return false;
        foreach (var p in MenuPanels)
            if (p != null && p.activeInHierarchy)
                return true;
        return false;
    }

    void SetMainButtonsVisible(bool visible)
    {
        if (MainButtons == null) return;
        foreach (var go in MainButtons)
            if (go != null)
                go.SetActive(visible);
    }
}
