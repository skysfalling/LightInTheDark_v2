using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    GameManager gameManager;

    [Header("Fullscreen Panic Shader")]
    public Material panicShaderMaterial;
    public float panic_fullscreenIntensity = 0.05f; // The initial value for the _FullscreenIntensity property of the Panic Shader material
    public float panic_transitionSpeed;
    private Coroutine panicCoroutine;
    public bool flickerActive;

    private void Start()
    {
        gameManager = GetComponentInParent<GameManager>();

        DisablePanicShader();
    }

    private void Update()
    {
        
    }

    public void EnablePanicShader()
    {
        if (panicCoroutine == null)
        {
            panicCoroutine = StartCoroutine(LerpPanicFullscreenIntensity(panic_fullscreenIntensity, panic_transitionSpeed));

            StartCoroutine(PanicFlicker(1, 10));
        }
    }

    public void DisablePanicShader()
    {

        flickerActive = false;
        if (panicCoroutine == null)
        {
            panicCoroutine = StartCoroutine(LerpPanicFullscreenIntensity(0, 100));
        }

    }

    IEnumerator LerpPanicFullscreenIntensity(float targetIntensity, float speed)
    {
        float currentIntensity = panicShaderMaterial.GetFloat("_FullscreenIntensity");

        // update intensity
        while (Mathf.Abs(currentIntensity - targetIntensity) > 0.01f)
        {
            currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, speed * Time.deltaTime);
            panicShaderMaterial.SetFloat("_FullscreenIntensity", currentIntensity);
            yield return null;
        }

        panicShaderMaterial.SetFloat("_FullscreenIntensity", targetIntensity);

        panicCoroutine = null;
    }

    IEnumerator PanicFlicker(float maxIntensity, float speed)
    {
        flickerActive = true;
        while (flickerActive)
        {
            StartCoroutine(LerpPanicFullscreenIntensity(0, speed));

            yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));

            float targetIntensity = Random.Range(maxIntensity * 0.5f, maxIntensity);
            StartCoroutine(LerpPanicFullscreenIntensity(targetIntensity, speed));

            yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
        }
    }
}
