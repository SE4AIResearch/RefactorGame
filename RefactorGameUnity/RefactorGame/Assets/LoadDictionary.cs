using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadDictionary : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var definitions = DefinitionHandler.LoadAllDefinitions();
        var prefabRef = Resources.Load("Graphics/Prefab/Definition.prefab");

        for (int i = 0; i < definitions.Count; i++)
        {
            GameObject prefab = Instantiate(prefabRef) as GameObject;
            prefab.name = $"Definition${i + 1}";
            
            var handler = prefab.GetComponent<DefinitionHandler>();
            handler.DisplayDefinition(definitions[i].Name);

            prefab.transform.SetParent(this.transform);            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
