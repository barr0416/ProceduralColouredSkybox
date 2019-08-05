using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Skybox control for the GradientSkybox shader. Allows for control of two colors on the shader.
/// </summary>
public class SkyboxControl : MonoBehaviour 
{
    //Reference for the skybox and material found on the main camera
    private Material skyboxMaterial;
    public Skybox skybox;
    private string m_SkyboxTopName = "_Color1";
    private string m_SkyboxBottomName = "_Color2";
    private string m_IntensityName = "_Intensity";
    private string m_DropoffName = "_Exponent";
    private string m_HorizonLineName = "_Horizon";
    private string m_HorizonEdgeName = "_HorizonSize";

    //Controls that are found in the script
    [Range(0.0f, 1.0f)]
    public float skyboxIntensity = 1.0f;
    [Range(0.0f, 1.0f)]
    public float skyboxExponent = 1.0f;
    [Range(0.0f, 1.0f)]
    public float skyboxHorizonLine = 1.0f;
    [Range(0.0f, 1.0f)]
    public float skyboxHorizonEdge = 0.1f;

    //Skybox color references for the top and bottom
    public Color skyboxColorTop;
    public Color skyboxColorBottom;
    public Color skyboxColorTop2;
    public Color skyboxColorBottom2;

    //If random colors is selected, will snap to a random color after set time
    public bool randomColors = false;
    //The time between switching the colors, used for all calls
    public float colorsSwitchTimer = 5.0f;
    //Tracking the switch timers and lerp progress (keep private)
    private float lastSwitch = 0.0f;
    private float lerpStep = 0.0f;

    /// <summary>
    /// Toggle to change the given light color when the skybox color changes
    /// </summary>
    public bool changeLightColor = true;
    /// <summary>
    /// The main or global light being used
    /// </summary>
    public Light globalLight;

    /// <summary>
    /// Toggle to lerp random light colors
    /// </summary>
    public bool lerpRandomColors = false;
    /// <summary>
    /// Toggle to lerp between the two given colors (ping-pong)
    /// </summary>
    public bool lerpBetweenGivenColors = false;

    /// <summary>
    /// Controls for the colours using Unity UI
    /// </summary>
    [Header("Top RGB Settings")]
    public bool useCustomTopValues = false;
    public Slider topValueR;
    public Slider topValueG;
    public Slider topValueB;
    public bool updateGlobalLight = false;

    [Header("Bottom RGB Settings")]
    public bool useCustomBottomValues = false;
    public Slider bottomValueR;
    public Slider bottomValueG;
    public Slider bottomValueB;

    [Header("Extra Settings")]
    public Slider intensitySlider;
    public Slider dropoffSlider;
    public Slider horizontalLineSlider;
    public Slider horizontalEdgeSlider;

    [Header("Show Controls")]
    public Toggle showColorToggle;
    public GameObject colorSliders;
    public GameObject settingsSliders;
    private bool m_ShowColorOptions = true;

//State tracking for the style of color change that will happen
private enum ColorChangeStyle
    {
        SnapToRandom,
        TransitionToRandom,
        TransitionToGiven,
        None
    }
    private ColorChangeStyle colorChangeStyle;

    //State tracking for the current color transition
    private enum CurrentColorState
    {
        Idle,
        Changing,
        Done
    }
    private CurrentColorState currentColorState;

    /// <summary>
    /// Start this instance.
    /// </summary>
    private void Start()
    {
        //Set the skybox material from reference
        if (skybox)
        {
            skyboxMaterial = skybox.material;
        }

        //Set the skybox settings as given
        this.SetSkyboxSettings();

        //Set the skybox colors to the given top and bottom as the starting colors
        this.SetSkyboxColors(skyboxColorBottom, skyboxColorTop);

        //Set the color change style state depending on what the parameter selected has been
        if(randomColors)
        {
            colorChangeStyle = ColorChangeStyle.SnapToRandom;
        }
        else if(lerpBetweenGivenColors)
        {
            colorChangeStyle = ColorChangeStyle.TransitionToGiven;
        }
        else if(lerpRandomColors)
        {
            colorChangeStyle = ColorChangeStyle.TransitionToRandom;
        }
        else
        {
            colorChangeStyle = ColorChangeStyle.None;
        }

        //The current color state change is idle
        currentColorState = CurrentColorState.Idle;

        //Check to see if the change light option has been selected but no light was given,
        //Then turn off the global light color change to prevent error
        if(changeLightColor && globalLight == null)
        {
            changeLightColor = false;
        }

        //Add a listener for the toggle button change to show the color sliders or not
        showColorToggle.onValueChanged.AddListener((value) =>
        {
            ToggleColorSliders(value);
        });
    }

    /// <summary>
    /// Update this instance.
    /// </summary>
    private void Update()
    {
        if (useCustomTopValues && useCustomBottomValues)
        {
            skyboxColorTop = new Color(topValueR.value / 255, topValueG.value / 255, topValueB.value / 255);
            skyboxMaterial.SetColor(m_SkyboxTopName, skyboxColorTop);

            if(updateGlobalLight)
            {
                globalLight.color = skyboxColorTop;
            }

            skyboxColorBottom = new Color(bottomValueR.value / 255, bottomValueG.value / 255, bottomValueB.value / 255);
            skyboxMaterial.SetColor(m_SkyboxBottomName, skyboxColorBottom);

            this.SetSkyboxSliderSettings();
        }
        else if(useCustomBottomValues)
        {
            skyboxColorBottom = new Color(bottomValueR.value / 255, bottomValueG.value / 255, bottomValueB.value / 255);
            skyboxMaterial.SetColor(m_SkyboxBottomName, skyboxColorBottom);
        }
        else if(useCustomTopValues)
        {
            skyboxColorTop = new Color(topValueR.value / 255, topValueG.value / 255, topValueB.value / 255);
            skyboxMaterial.SetColor(m_SkyboxTopName, skyboxColorTop);

            if (updateGlobalLight)
            {
                globalLight.color = skyboxColorTop;
            }
        }
        else
        {
            //Count towards the time to switch (in seconds)
            lastSwitch += Time.deltaTime;

            //Check to see what color change style has been selected and see if the switch timer is filled to start the color switch
            if (colorChangeStyle == ColorChangeStyle.TransitionToRandom && lastSwitch >= colorsSwitchTimer)
            {
                StartCoroutine("TransitionRandomColors");
                lastSwitch = 0.0f;
            }
            else if (colorChangeStyle == ColorChangeStyle.TransitionToGiven && lastSwitch >= colorsSwitchTimer)
            {
                StartCoroutine("TransitionGivenColors");
                lastSwitch = 0.0f;
            }
            else if (colorChangeStyle == ColorChangeStyle.SnapToRandom && lastSwitch >= colorsSwitchTimer)
            {
                this.SetSkyboxColors(UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f), UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f));
                lastSwitch = 0.0f;
            }
            else
            {
                //Do nothing or something
            }

            this.SetSkyboxSettings();
        }
    }

    /// <summary>
    /// Sets the skybox colors with given bottom and top colors.
    /// </summary>
    /// <param name="bottomColor">Bottom color.</param>
    /// <param name="topColor">Top color.</param>
    private void SetSkyboxColors(Color bottomColor, Color topColor)
    {
        skyboxMaterial.SetColor("_Color1", bottomColor);
        skyboxMaterial.SetColor("_Color2", topColor);

        //Check for change light color selected and perform it
        if(changeLightColor)
        {
            globalLight.color = topColor;
        }
    }

    /// <summary>
    /// Transitions to random colors generated in the coroutine from the old, current colors.
    /// </summary>
    /// <returns>The random colors.</returns>
    private IEnumerator TransitionRandomColors()
    {
        //The new and old top and bottom colors for reference inside the coroutine
        Color oldTopColor = skyboxMaterial.GetColor("_Color2");
        Color oldBottomColor = skyboxMaterial.GetColor("_Color1");

        Color newTopColor = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        Color newBottomColor = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);

        //Continually change the colors toward the new colors given until the given time is reached
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

    /// <summary>
    /// Transitions to the given colors from the inspector. 
    /// Performs in a ping-pong fashion.
    /// </summary>
    /// <returns>The given colors.</returns>
    private IEnumerator TransitionGivenColors()
    {
        //Check the current color change state and perform the color changing as needed
        if (currentColorState == CurrentColorState.Idle)
        {
            currentColorState = CurrentColorState.Changing;

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
        else if (currentColorState == CurrentColorState.Done)
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

        //Set the state to reflect what the current color state is for future changes
        if (currentColorState == CurrentColorState.Changing)
        {
            currentColorState = CurrentColorState.Done;
        }
        else
        {
            currentColorState = CurrentColorState.Idle;
        }
    }

    /// <summary>
    /// Snaps to jew skybox colors to two new random colors generated in the call.
    /// </summary>
    private void SnapNewSkyboxColors()
    {
        //The new colors
        Color newColor1 = UnityEngine.Random.ColorHSV(0.0f, 1.0f, 1.0f, 1.0f, 0.5f, 1.0f);
        Color newColor2 = UnityEngine.Random.ColorHSV(0.0f, 1.0f, 1.0f, 1.0f, 0.5f, 1.0f);

        //Set them to the skybox
        skyboxMaterial.SetColor("_Color1", newColor1);
        skyboxMaterial.SetColor("_Color2", newColor2);

        //Check if the light needs to be changed as well and do it
        if (changeLightColor)
        {
            globalLight.color = newColor2;
        }
        else
        {
            //Do something else to your light perhaps
        }
    }

    /// <summary>
    /// Sets the skybox settings.
    /// </summary>
    private void SetSkyboxSettings()
    {
        skyboxMaterial.SetFloat("_Intensity", skyboxIntensity);
        skyboxMaterial.SetFloat("_Exponent", skyboxExponent);
        skyboxMaterial.SetFloat("_Horizon", skyboxHorizonLine);
        skyboxMaterial.SetFloat("_HorizonSize", skyboxHorizonEdge);
    }

    /// <summary>
    /// Sets the skybox settings to the values provided using the sliders in scene.
    /// </summary>
    private void SetSkyboxSliderSettings()
    {
        skyboxMaterial.SetFloat(m_IntensityName, intensitySlider.value);
        skyboxMaterial.SetFloat(m_DropoffName, dropoffSlider.value);
        skyboxMaterial.SetFloat(m_HorizonLineName, horizontalLineSlider.value);
        skyboxMaterial.SetFloat(m_HorizonEdgeName, horizontalEdgeSlider.value);
    }

    public void ToggleColorSliders(bool toggle)
    {
        if(!toggle)
        {
            colorSliders.SetActive(false);
        }
        else
        {
            colorSliders.SetActive(true);
        }
    }
}
