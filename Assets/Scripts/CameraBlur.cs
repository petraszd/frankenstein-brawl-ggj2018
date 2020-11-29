using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Curves
{
  public AnimationCurve BlurX;
  public AnimationCurve BlurY;
  public AnimationCurve Desaturate;
}

enum BlurStates
{
  NO_BLUR,
  TURNING_ON,
  TURNING_OFF,
  LOOPING,
};

public class CameraBlur : MonoBehaviour
{
  const string SHADER_PARAM_GREY_FACTOR = "_GreyFactor";
  const string SHADER_PARAM_BLURX = "_BlurX";
  const string SHADER_PARAM_BLURY = "_BlurY";

  const float TURN_ON_TIMER = 1.0f;
  const float TURN_OFF_TIMER = 0.75f;
  const float LOOP_TIMER = 1.3f;

  const float MAX_BLUR = 0.002f;
  const float MAX_DESATURATE = 0.7f;

  public Shader BlurShader;
  public Curves turnOn;
  public Curves turnOff;
  public Curves loop;

  Material blurMaterial;
  BlurStates state;
  float loopT;

  void OnEnable()
  {
    UIManager.OnPrepareForMatch += OnPrepareForMatch;
    UIManager.OnStartFighting += OnStartFighting;
  }

  void OnDisable()
  {
    UIManager.OnPrepareForMatch -= OnPrepareForMatch;
    UIManager.OnStartFighting -= OnStartFighting;
  }

  void Start()
  {
    loopT = 0.0f;
    state = BlurStates.NO_BLUR;

    blurMaterial = new Material(BlurShader);
    blurMaterial.SetFloat(SHADER_PARAM_BLURX, 0.0f);
    blurMaterial.SetFloat(SHADER_PARAM_BLURY, 0.0f);
    blurMaterial.SetFloat(SHADER_PARAM_GREY_FACTOR, 0.0f);
  }

  void OnRenderImage(RenderTexture source, RenderTexture destination)
  {
    if (state != BlurStates.NO_BLUR) {
      if (state == BlurStates.LOOPING) {
        UpdateForLooping();
      }
      Graphics.Blit(source, destination, blurMaterial);
    } else {
      Graphics.Blit(source, destination);
    }
  }

  void OnPrepareForMatch()
  {
    state = BlurStates.TURNING_ON;
    StartCoroutine(AnimateShaderParams(turnOn, TURN_ON_TIMER, BlurStates.LOOPING));
  }

  void OnStartFighting()
  {
    state = BlurStates.TURNING_OFF;
    StartCoroutine(AnimateShaderParams(turnOff, TURN_OFF_TIMER, BlurStates.NO_BLUR));
  }

  IEnumerator AnimateShaderParams(Curves curves, float timer, BlurStates newState)
  {
    float t = 0.0f;
    while (t < timer) {
      yield return null;
      UpdateBlur(curves, timer, t);
      t += Time.deltaTime;
    }

    state = newState;
  }

  void UpdateBlur(Curves curves, float timer, float t)
  {
    float grey = curves.Desaturate.Evaluate(t / timer) * MAX_DESATURATE;
    float blurX = curves.BlurX.Evaluate(t / timer) * MAX_BLUR;
    float blurY = curves.BlurY.Evaluate(t / timer) * MAX_BLUR;

    blurMaterial.SetFloat(SHADER_PARAM_GREY_FACTOR, grey);
    blurMaterial.SetFloat(SHADER_PARAM_BLURX, blurX);
    blurMaterial.SetFloat(SHADER_PARAM_BLURY, blurY);
  }

  void UpdateForLooping()
  {
    loopT += Time.deltaTime;
    UpdateBlur(loop, LOOP_TIMER, loopT);
  }
}
