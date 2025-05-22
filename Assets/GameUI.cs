using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    public GameObject MiniMap;
    public GameObject FadeImage;
    public Animator _FadeAnimator;
    private void Awake()
    {
        if (MiniMap.activeSelf)
        {
            MiniMap.SetActive(false);
        }
        StartCoroutine(StartFadeOut());
    }

    IEnumerator StartFadeOut()
    {
        _FadeAnimator.SetBool("FadeIn", false);
        _FadeAnimator.SetBool("FadeOut", true);
        yield return new WaitForSeconds(1);
        if (!MiniMap.activeSelf)
        {
            MiniMap.SetActive(true);
        }
    }
}
