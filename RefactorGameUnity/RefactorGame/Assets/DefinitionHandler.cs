using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RefactorLang;
using System.Text.Json;
using Image = UnityEngine.UI.Image;
using TMPro;
using System;

public class DefinitionHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private DefinitionSignature LoadDefinition(string definition)
    {
        var json = Resources.Load<TextAsset>("Dictionary/dictionary");
        var dictionary = JsonSerializer.Deserialize<Dictionary<string, DefinitionSignature>>(json.text);
        return dictionary[definition];
    }

    public void DisplayDefinition(string key)
    {
        var definition = LoadDefinition(key);
        SetIcon(definition);
        SetText(definition);
    }

    private void SetIcon(DefinitionSignature definition)
    {
        var image = this.transform.Find("Icon").GetComponent<Image>();

        string path = "Graphics/Dictionary/";
        switch (definition.Type)
        {
            case "smell":
                path += "codeSmellIcon";
                break;

            case "refactoring":
                path += "refactorTechniqueIcon";
                break;

            default:
                path += "lockedIcon";
                break;
        }

        Sprite icon = Resources.Load<Sprite>(path);
        image.sprite = icon;
    }

    private void SetText(DefinitionSignature definition)
    {
        var description = this.transform.Find("Description");
        var name = description.Find("Name").GetComponent<TextMeshProUGUI>();
        var details = description.Find("Details").GetComponent<TextMeshProUGUI>();

        name.text = definition.Name;
        details.text = string.Join(" ", definition.Details);
    }
}
