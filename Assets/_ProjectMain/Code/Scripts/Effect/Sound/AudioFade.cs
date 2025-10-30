using System.Collections;
using UnityEngine;

public static class AudioFade
{   //Fade form A to B
    public static IEnumerator FadeTo(AudioSource source, float target, float duration)
    {
        if (source == null) yield break;

        float start = source.volume;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }
        source.volume = target;
    }
    //Fade Volume Out
    public static IEnumerator FadeOut(AudioSource s, float d) => FadeTo(s, 0f, d);
    public static IEnumerator FadeIn(AudioSource s, float d, float t = 1f) => FadeTo(s, t, d);
}
