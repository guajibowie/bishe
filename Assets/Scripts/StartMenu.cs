using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class StartMenu : MonoBehaviour
{
    // Start is called before the first frame update
    public Animator _danceAnimator;
    public Animator _soldierAnimator;
    public Animator _FadeAnimator;
    public GameObject _weapon;
    public GameObject _SettingPanel;
    private float _danceMultiplier;

    private void Start()
    {
        _FadeAnimator.SetBool("FadeIn", false);
        _FadeAnimator.SetBool("FadeOut", true);
        _danceMultiplier =  _danceAnimator.GetFloat("Multiplier");
    }
    // Update is called once per frame
    void Update()
    {
        //如果点击鼠标左键 
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray1 = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;
            if (Physics.Raycast(ray1, out hit))
            {
                switch (hit.transform.name)
                {
                    case "zombo":
                        if (!_danceAnimator.GetBool("Dance"))
                        {
                            _danceAnimator.SetBool("Dance", true);
                        }
                        else
                        {
                            _danceMultiplier += 0.1f;
                            Mathf.Clamp(_danceMultiplier, 1f, 2f);
                            _danceAnimator.SetFloat("Multiplier",_danceMultiplier);
                        }
                        break;
                    case "Models":
                        _soldierAnimator.SetBool("Dance", !_soldierAnimator.GetBool("Dance"));
                        break;
                    default:
                        break;
                }
            }
        }
    }


    public void StartGameButton()
    {
        StartCoroutine(AsyncLoad(1));
    }

    IEnumerator AsyncLoad(int Index)
    {
        _FadeAnimator.SetBool("FadeIn", true);
        _FadeAnimator.SetBool("FadeOut", false);
        yield return new WaitForSeconds(1);
        AsyncOperation load =  SceneManager.LoadSceneAsync(Index);
        load.completed += Load_completed;
    }

    private void Load_completed(AsyncOperation obj)
    {
        //_FadeAnimator.SetBool("FadeIn", false);
        //_FadeAnimator.SetBool("FadeOut", true);
    }

    public void SettingButton()
    {
        _SettingPanel.SetActive(true);
    }

    public void closeSetting()
    {
        _SettingPanel.SetActive(false);
    }
}
