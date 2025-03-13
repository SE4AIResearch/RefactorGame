using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RefactorLang;
using System.Linq;

public class ActivateStationContextMenu : MonoBehaviour
{
    public ModuleContextMenu ModuleContextMenu;
    public CurrentKitchenState Kitchen;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        if (Kitchen.ContextMenuUp) return;

        int stationIndex = (int)char.GetNumericValue(this.transform.parent.name.Last()) - 1;
        int moduleIndex = (int)char.GetNumericValue(this.name.Last()) - 1;
        Module thisModule = Kitchen.KitchenState.Stations[stationIndex].Modules[moduleIndex];

        ModuleSignature signature = new(thisModule.GetType().Name, thisModule.Name, thisModule.IsLocked);

        ModuleContextMenu.OpenMenu(signature, stationIndex, moduleIndex);
    }
}
