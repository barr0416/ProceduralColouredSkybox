using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxControl : MonoBehaviour 
{
    private Material skyboxMaterial;
    public Skybox skybox;

    public Color skyboxColorTop;
    public Color skyboxColorBottom;

    public bool randomColors = false;
    public float randomColorsSwitchTimer = 5.0f;
    private float lastSwitch = 0.0f;
    private float lerpStep = 0.0f;

    public bool changeLightColor = true;
    public Light globalLight;

    public bool lerpSkyboxColors = false;

    private void Start()
    {
        skyboxMaterial = skybox.material;

        this.SetSkyboxColors(skyboxColorBottom, skyboxColorTop);
    }

    private void Update()
    {
        lastSwitch += Time.deltaTime;

        if(randomColors && lastSwitch >= randomColorsSwitchTimer && lerpSkyboxColors)
        {
            StartCoroutine("TransitionColors");
            lastSwitch = 0.0f;
        }
        else if(randomColors && lastSwitch >= randomColorsSwitchTimer)
        {
            this.SetSkyboxColors(Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f), Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f));
            lastSwitch = 0.0f;
        }
    }

    private void LerpSkyColors()
    {

    }

    private void SetSkyboxColors(Color bottomColor, Color topColor)
    {
        skyboxMaterial.SetColor("_Color1", bottomColor);
        skyboxMaterial.SetColor("_Color2", topColor);

        if(changeLightColor)
        {
            globalLight.color = topColor;
        }
    }

    private IEnumerator TransitionColors()
    {
        Color oldTopColor = skyboxMaterial.GetColor("_Color2");
        Color oldBottomColor = skyboxMaterial.GetColor("_Color1");

        Color newTopColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        Color newBottomColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);

        while(lerpStep < 1)
        {
            skyboxMaterial.SetColor("_Color1", Color.Lerp(oldBottomColor, newBottomColor, lerpStep));
            skyboxMaterial.SetColor("_Color2", Color.Lerp(oldTopColor, newTopColor, lerpStep));

            globalLight.color = Color.Lerp(oldTopColor, newTopColor, lerpStep);
            
            lerpStep += Time.deltaTime * randomColorsSwitchTimer;

            yield return null;
        }

        lerpStep = 0;
    }

    private void TransitionSkyboxColors()
    {
        skyboxMaterial.SetColor("_Color1", Color.Lerp(skyboxMaterial.GetColor("_Color1"), Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f), randomColorsSwitchTimer));
        skyboxMaterial.SetColor("_Color2", Color.Lerp(skyboxMaterial.GetColor("_Color2"), Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f), randomColorsSwitchTimer));

        if (changeLightColor)
        {
            //globalLight.color = topColor;
        }
    }
}
