using RefactorLang;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayStation : MonoBehaviour
{
    public CurrentKitchenState kitchen;

    // Start is called before the first frame update
    void Start()
    {
        kitchen.OnStateChanged += UpdateAppearance;

        this.transform.Find("Module 1").gameObject.SetActive(false);
        this.transform.Find("Module 2").gameObject.SetActive(false);
        this.transform.Find("Module 3").gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateAppearance(KitchenState newState)
    {
        Station station = newState.Stations.Find(x => x.Name == name);

        int moduleIndex = 1;
        foreach (Module module in station.Modules)
        {
            var moduleObject = this.transform.Find($"Module {moduleIndex}").gameObject;

            moduleObject.SetActive(true);

            Sprite newSprite = Resources.Load<Sprite>($"Graphics/Modules/{module.GetType().Name}");

            SpriteRenderer renderer = moduleObject.transform.Find("Image").GetComponent<SpriteRenderer>();

            renderer.sprite = newSprite;

            moduleIndex++;
        }
    }
}
