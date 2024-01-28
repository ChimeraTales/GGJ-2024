using System.Collections;
using UnityEngine;

public class IntroManager : MonoBehaviour
{
    public bool ragdoll;

    [SerializeField] float startDelay, panelDelay, panelDuration;
    [SerializeField] GameObject[] panels;
    [SerializeField] ParticleSystem particles;
    [SerializeField] CollisionSceneSwitcher switcher;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(NextPanel(0));
    }

    private IEnumerator NextPanel(int index)
    {
        yield return new WaitForSeconds(index == 0? startDelay : panelDelay);
        panels[index].SetActive(true);
        yield return new WaitForSeconds(panelDuration);
        if (index >= panels.Length - 1)
        {
            while (!ragdoll)
            {
                yield return null;
            }
            ParticleSystem.EmissionModule emissionModule = particles.emission;
            emissionModule.rateOverTime = 1;
        }
        panels[index].SetActive(false);
        if (index < panels.Length - 1)
        {
            StartCoroutine(NextPanel(index + 1));
        }
        else
        {
            switcher.on = true;
        }
    }
}