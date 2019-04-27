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

    public bool changeLightColor = true;
    public Light globalLight;

    private void Start()
    {
        skyboxMaterial = skybox.material;

        this.SetSkyboxColors(skyboxColorBottom, skyboxColorTop);
    }

    private void Update()
    {
        lastSwitch += Time.deltaTime;

        if(randomColors && lastSwitch >= randomColorsSwitchTimer)
        {
            this.SetSkyboxColors(Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f), Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f));
            lastSwitch = 0.0f;
        }
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
}
