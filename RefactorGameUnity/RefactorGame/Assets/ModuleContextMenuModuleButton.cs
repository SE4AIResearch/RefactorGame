using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RefactorLang;

public class ModuleContextMenuModuleButton : MonoBehaviour
{
    public CurrentKitchenState Kitchen;

    private ModuleContextMenu ModuleContextMenu;

    // Start is called before the first frame update
    void Start()
    {
        ModuleContextMenu = this.transform.parent.parent.parent.GetComponent<ModuleContextMenu>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SelectThisModule()
    {
        if (!Kitchen.ContextMenuUp) return;

        ModuleContextMenu.DisplayModule(new ModuleSignature(this.transform.parent.name, ModuleContextMenu.DisplayingModule.Name, ModuleContextMenu.DisplayingModule.Locked));
    }
}
