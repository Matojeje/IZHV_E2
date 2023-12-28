using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlsText : MonoBehaviour
{
    [Header("Game object references")]
    public Text jumpText;
    public Text swapText;
    public Player player;

    [Header("Settings")]
    [Range(0,1)]
    public float inactiveOpacity = 0.6f;
    [Range(0,1)]
    public float activeOpacity = 1.0f;
    public float fadeTime = 0.1f;

    [Header("Help strings")]
    public string jumpKey = "<SPACE>";
    public string swapKeyUp = "<UP> <W>";
    public string swapKeyDown = "<DOWN> <S>";

    // Start is called before the first frame update
    void Start()
    {
        // Todo: Set `jumpKey`, `swapKey1` and `swapKey2` to their actual keybinds
        
        jumpText.text = "Jump: " + jumpKey;

        jumpText.CrossFadeAlpha(inactiveOpacity, 0, true);
        swapText.CrossFadeAlpha(inactiveOpacity, 0, true);
    }

    // Update is called once per frame
    void Update()
    {
        bool isFlipped = player.mCurrentGravity > 0.0f;
        swapText.text = "Swap: " + (isFlipped ? swapKeyDown : swapKeyUp);

        if (Input.GetButtonDown("Jump")) jumpText.CrossFadeAlpha(activeOpacity, fadeTime, false);
        if (Input.GetButtonUp("Jump")) jumpText.CrossFadeAlpha(inactiveOpacity, fadeTime, false);

        bool isSwapping = Math.Abs(Input.GetAxisRaw("Vertical")) != 0.0f;
        swapText.CrossFadeAlpha(isSwapping ? activeOpacity : inactiveOpacity, fadeTime, false);
    }
}
