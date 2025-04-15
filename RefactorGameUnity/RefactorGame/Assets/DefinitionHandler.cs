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

    public static List<DefinitionSignature> LoadDefinitions()
    {
        var json = Resources.Load<TextAsset>("Dictionary/dictionary");
        var dictionary = JsonSerializer.Deserialize<Dictionary<string, DefinitionSignature>>(json.text);
        var definitions = new List<DefinitionSignature>();
        foreach (var definition in dictionary)
        {
            definitions.Add(definition.Value);
        }
        return definitions;
    }

    public static List<string> DefinitionKeys()
    {
        var json = Resources.Load<TextAsset>("Dictionary/dictionary");
        var dictionary = JsonSerializer.Deserialize<Dictionary<string, DefinitionSignature>>(json.text);
        var definitions = new List<string>();
        foreach (var definition in dictionary)
        {
            definitions.Add(definition.Key);
        }
        return definitions;
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
        SetIcon(key, definition.Type);
        SetText(key, definition);
    }

    private void SetIcon(string key, string type)
    {
        var image = this.transform.Find("Icon").GetComponent<Image>();

        string path = "Graphics/Dictionary/";

        if (DictionaryState.Current.IsLocked(key))
        {
            path += "lockedIcon";

        }
        else if (type.Equals("smell"))
        {
            path += "codeSmellIcon";

        }
        else if (type.Equals("refactoring"))
        {
            path += "refactorTechniqueIcon";

        }
        else
        {
            path += "lockedIcon";
        }

        Sprite icon = Resources.Load<Sprite>(path);
        image.sprite = icon;
    }

    private void SetText(string key, DefinitionSignature definition)
    {
        var description = this.transform.Find("Description");
        var name = description.Find("Name").GetComponent<TextMeshProUGUI>();
        var details = description.Find("Details").GetComponent<TextMeshProUGUI>();


        if (DictionaryState.Current.IsLocked(key))
        {
            name.text = "???";
            details.text = "Play more to discover this method.";
        }
        else
        {
            name.text = definition.Name;
            details.text = string.Join(" ", definition.Details);
        }
    }
}
