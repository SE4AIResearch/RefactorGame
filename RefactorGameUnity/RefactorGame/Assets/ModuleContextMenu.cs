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

    private GameObject HowTo;

    private int StationIndex;
    private int ModuleIndex;

    // Start is called before the first frame update
    void Start()
    {
        CloseMenu(true);
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
        Overlay.SetActive(true);
        Kitchen.ContextMenuUp = true;
    }

    public void CloseMenu(bool first = false)
    {
        gameObject.SetActive(false);
        if (!first) Overlay.SetActive(false);
        Kitchen.ContextMenuUp = false;
    }

    public void DisplayModule(ModuleSignature module)
    {
        DisplayingModule = module;

        if (HowTo != null)
        {
            HowTo.SetActive(false);
            HowTo = this.transform.Find("ModuleButtons").Find(module.Module).Find("HowTo").gameObject;
            HowTo.SetActive(true);
        }

        this.transform.Find("ModuleName").GetComponent<TextMeshProUGUI>().text = module.Module;
    }

    public void Confirm()
    {
        Kitchen.KitchenState.Stations[StationIndex].Modules[ModuleIndex] = DisplayingModule.ConvertToModule();
        Kitchen.KitchenState = Kitchen.KitchenState;

        CloseMenu();
    }
}
