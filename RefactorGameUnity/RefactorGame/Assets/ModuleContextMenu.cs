using RefactorLang;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ModuleContextMenu : MonoBehaviour
{
    public ModuleSignature DisplayingModule = null;
    public CurrentKitchenState Kitchen;
    public GameObject Overlay;

    private int StationIndex;
    private int ModuleIndex;

    // Start is called before the first frame update
    void Start()
    {
        CloseMenu();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenMenu(ModuleSignature module, int stationIndex, int moduleIndex)
    {
        DisplayModule(module);

        StationIndex = stationIndex;
        ModuleIndex = moduleIndex;

        gameObject.SetActive(true);
        Overlay.gameObject.SetActive(true);
        Kitchen.ContextMenuUp = true;
    }

    public void CloseMenu()
    {
        gameObject.SetActive(false);
        Overlay.gameObject.SetActive(false);
        Kitchen.ContextMenuUp = false;
    }

    public void DisplayModule(ModuleSignature module)
    {
        DisplayingModule = module;

        this.transform.Find("ModuleName").GetComponent<TextMeshProUGUI>().text = module.Module;
    }

    public void Confirm()
    {
        Kitchen.KitchenState.Stations[StationIndex].Modules[ModuleIndex] = DisplayingModule.ConvertToModule();
        Kitchen.KitchenState = Kitchen.KitchenState;

        CloseMenu();
    }
}
