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

        var definitions = DefinitionHandler.LoadAllDefinitions();
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

        // RectTransform rectTransform = this.GetComponent<RectTransform>();
        // rectTransform.anchorMin = new Vector2(0.5f, 1);
        // rectTransform.anchorMax = new Vector2(0.5f, 1);
        // rectTransform.pivot = new Vector2(0.5f, 1);
        // rectTransform.offsetMin = new Vector2(0, rectTransform.offsetMin.y);
        // rectTransform.offsetMax = new Vector2(0, rectTransform.offsetMax.y);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
