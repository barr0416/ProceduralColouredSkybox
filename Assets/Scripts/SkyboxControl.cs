using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxControl : MonoBehaviour 
{
    private Material skyboxMaterial;
    public Skybox skybox;

    public Color skyboxColorTop;
    public Color skyboxColorBottom;

    public Color skyboxColorTop2;
    public Color skyboxColorBottom2;

    public bool randomColors = false;
    public float colorsSwitchTimer = 5.0f;
    private float lastSwitch = 0.0f;
    private float lerpStep = 0.0f;

    public bool changeLightColor = true;
    public Light globalLight;

    public bool lerpRandomColors = false;
    public bool lerpBetweenGivenColors = false;
    
    private enum CurrentColor
    {
        Original,
        Given
    }
    private CurrentColor currentColor;

    private void Start()
    {
        skyboxMaterial = skybox.material;

        this.SetSkyboxColors(skyboxColorBottom, skyboxColorTop);

        if(lerpBetweenGivenColors)
        {
            currentColor = CurrentColor.Original;
        }
    }

    private void Update()
    {
        lastSwitch += Time.deltaTime;

        if(randomColors && lastSwitch >= colorsSwitchTimer && lerpRandomColors)
        {
            StartCoroutine("TransitionColors");
            lastSwitch = 0.0f;
        }
        else if (lastSwitch >= colorsSwitchTimer && lerpBetweenGivenColors)
        {
            StartCoroutine("TransitionGivenColors");
            lastSwitch = 0.0f;
        }
        else if (randomColors && lastSwitch >= colorsSwitchTimer)
        {
            this.SetSkyboxColors(Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f), Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f));
            lastSwitch = 0.0f;
        }
        else
        {
            //Do nothing
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

        while(lerpStep < 1.0f)
        {
            skyboxMaterial.SetColor("_Color1", Color.Lerp(oldBottomColor, newBottomColor, lerpStep));
            skyboxMaterial.SetColor("_Color2", Color.Lerp(oldTopColor, newTopColor, lerpStep));

            globalLight.color = Color.Lerp(oldTopColor, newTopColor, lerpStep);
            
            lerpStep += Time.deltaTime / colorsSwitchTimer;

            yield return null;
        }

        lerpStep = 0;
    }

    private IEnumerator TransitionGivenColors()
    {
        if (currentColor == CurrentColor.Original)
        {
            while (lerpStep < 1.0f)
            {
                skyboxMaterial.SetColor("_Color1", Color.Lerp(skyboxColorBottom, skyboxColorBottom2, lerpStep));
                skyboxMaterial.SetColor("_Color2", Color.Lerp(skyboxColorTop, skyboxColorTop2, lerpStep));

                globalLight.color = Color.Lerp(skyboxColorTop, skyboxColorTop2, lerpStep);

                lerpStep += (Time.deltaTime * 1.1f) / colorsSwitchTimer;

                yield return null;
            }

            lerpStep = 0;
        }
        else if (currentColor == CurrentColor.Given)
        {
            while (lerpStep < 1.0f)
            {
                skyboxMaterial.SetColor("_Color1", Color.Lerp(skyboxColorBottom2, skyboxColorBottom, lerpStep));
                skyboxMaterial.SetColor("_Color2", Color.Lerp(skyboxColorTop2, skyboxColorTop, lerpStep));

                globalLight.color = Color.Lerp(skyboxColorTop2, skyboxColorTop, lerpStep);

                lerpStep += (Time.deltaTime * 1.1f) / colorsSwitchTimer;

                yield return null;
            }

            lerpStep = 0;
        }



        if (currentColor == CurrentColor.Original)
        {
            currentColor = CurrentColor.Given;
        }
        else
        {
            currentColor = CurrentColor.Original;
        }
    }

    private void TransitionSkyboxColors()
    {
        skyboxMaterial.SetColor("_Color1", Color.Lerp(skyboxMaterial.GetColor("_Color1"), Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f), colorsSwitchTimer));
        skyboxMaterial.SetColor("_Color2", Color.Lerp(skyboxMaterial.GetColor("_Color2"), Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f), colorsSwitchTimer));

        if (changeLightColor)
        {
            //globalLight.color = topColor;
        }
    }
}
