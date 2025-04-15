using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadDictionary : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.SetActive(false);
        SetDefinitions();

    }

    // Update is called once per frame
    void Update()
    {
        this.transform.Find("Scrollbar Vertical").GetComponent<Scrollbar>().size = 0.2f;
    }

    public void Reload()
    {
        var defContainer = this.transform.Find("Viewport").Find("Content").Find("DefinitionContainer");
        for (int i = 0; i < defContainer.childCount; i++)
        {
            Destroy(defContainer.GetChild(i).gameObject);
        }
        SetDefinitions();
    }

    private void SetDefinitions()
    {
        var definitions = DefinitionHandler.DefinitionKeys();
        var prefabRef = Resources.Load<Object>("Prefab/Definition");
        var defContainer = this.transform.Find("Viewport").Find("Content").Find("DefinitionContainer");

        for (int i = 0; i < definitions.Count; i++)
        {
            GameObject prefab = Instantiate(prefabRef) as GameObject;
            prefab.name = $"Definition{i + 1}";

            var handler = prefab.GetComponent<DefinitionHandler>();
            handler.DisplayDefinition(definitions[i]);

            RectTransform rectTransform = prefab.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 1);
            rectTransform.anchorMax = new Vector2(0.5f, 1);
            rectTransform.pivot = new Vector2(0.5f, 1);

            prefab.transform.SetParent(defContainer);
        }
    }
}
