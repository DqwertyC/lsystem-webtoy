using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Slider))]
public class MediaController : MonoBehaviour
{
    Slider slider;
    float baseSpeed = 0.0001f;
    int speedMultiplier = 1;

    public bool forcePause = false;
    bool playing = false;
    bool looping;

    public Button playButton;
    public Button pauseButton;
    public Button loopButton;
    public Button fasterButton;
    public Button slowerButton;

    public TMP_Text speedDisplay;


    // Start is called before the first frame update
    void Start()
    {
        slider = GetComponent<Slider>();

        playButton.onClick.AddListener(ButtonPlay);
        pauseButton.onClick.AddListener(ButtonPause);
        loopButton.onClick.AddListener(ButtonLoop);
        fasterButton.onClick.AddListener(ButtonFaster);
        slowerButton.onClick.AddListener(ButtonSlower);

        UpdateSpeed();
    }

    // Update is called once per frame
    void Update()
    {
        if (playing && !forcePause)
        {
            float newValue = slider.value + (baseSpeed * speedMultiplier);

            if (newValue > slider.maxValue)
                if (looping)
                    newValue = slider.minValue;
                else
                    newValue = slider.maxValue;

            if (newValue < slider.minValue)
                if (looping)
                    newValue = slider.maxValue;
                else
                    newValue = slider.minValue;

            slider.value = newValue;
        }
    }

    void ButtonPlay()
    {
        playing = true;
        playButton.gameObject.SetActive(false);
        pauseButton.gameObject.SetActive(true);
    }

    void ButtonPause()
    {
        playing = false;
        playButton.gameObject.SetActive(true);
        pauseButton.gameObject.SetActive(false);
    }
    void ButtonLoop()
    {
        looping = !looping;
        loopButton.GetComponent<Image>().color = looping ? Color.white : Color.gray;
    }

    void ButtonFaster()
    {
        if (speedMultiplier < -1)
        {
            speedMultiplier /= 2;
        }
        else if (speedMultiplier == -1)
        {
            speedMultiplier = 1;
        }
        else if (speedMultiplier < 32)
        {
            speedMultiplier *= 2;
        }

        UpdateSpeed();
    }

    void ButtonSlower()
    {
        if (speedMultiplier > 1)
        {
            speedMultiplier /= 2;
        }
        else if (speedMultiplier == 1)
        {
            speedMultiplier = -1;
        }
        else if (speedMultiplier > -32)
        {
            speedMultiplier *= 2;
        }

        UpdateSpeed();
    }

    void UpdateSpeed()
    {
        speedDisplay.text = (speedMultiplier >= 0 ? "+" : "") + speedMultiplier.ToString() + "x";
    }
}
