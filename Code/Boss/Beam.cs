using System;
using System.Collections;
using UnityEngine;

public class Beam : MonoBehaviour
{
	public AudioClip charge;
	public AudioClip blast;
	private AudioSource audio;

	public void SetSounds(AudioClip c, AudioClip b)
	{
		charge = c;
		blast = b;
	}
	private void Start()
	{
		foreach (Transform beam in transform)
		{
			if (!gameObject.name.Contains("Blast"))
			{
				beam.gameObject.GetComponent<BoxCollider2D>().enabled = false;
			}
		}
	}
	public void go(float duration, bool blastEffect)
    {
        foreach (Transform beam in transform)
		{
			if (beam.name.Contains("Blast"))
			{
				if (blastEffect)
				{
					StartCoroutine(go(beam.gameObject, "Start", "Nothing", "BeamBlast", "Nothing", true));
				}
				else
				{
                    StartCoroutine(go(beam.gameObject, "Nothing", "Nothing", "Nothing", "Nothing", true));
                }
			}
			else
			{
                beam.gameObject.GetComponent<BoxCollider2D>().enabled = false;
                StartCoroutine(go(beam.gameObject, "Nothing", "Windup","Beam","BeamEnd", false));
            }
		}
        IEnumerator go(GameObject obj, String start, String windup, String active, String end, bool playsAudio)
		{
            // windup
            obj.GetComponent<Animator>().Play(start);
			Animator animator = obj.GetComponent<Animator>();

			if (playsAudio)
			{
                audio = obj.GetComponent<AudioSource>();
                audio.outputAudioMixerGroup = HeroController.instance.gameObject.GetComponent<AudioSource>().outputAudioMixerGroup;
            }

			yield return new WaitForSeconds(1 / 12f);
			if (playsAudio)
			{
				audio.PlayOneShot(charge);
			}
            animator.Play(windup);

			yield return new WaitForSeconds(.5f);

			// activate
			if (playsAudio)
			{
                audio.PlayOneShot(blast);
			}
			else
            {
                obj.GetComponent<BoxCollider2D>().enabled = true;
			}

            animator.Play(active);

			// end
			yield return new WaitForSeconds(duration);

			if (!playsAudio)
            {
                obj.GetComponent<BoxCollider2D>().enabled = false;
			}
            animator.Play(end);

			yield return new WaitForSeconds(2/12f);
			GetComponent<SpriteRenderer>().enabled = false;

			yield return new WaitWhile(() => audio.isPlaying);

            Destroy(obj);
		}
    }
}
