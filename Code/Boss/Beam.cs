/*/

Script to play a windup then beam loop animation and sound for beam upon enabling
Intended for user to only have to worry about spawing and moving the beam

Attach this script to all beam objects

/*/

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
	private void OnEnable()
	{
		foreach (Transform beam in transform)
		{
			if (!gameObject.name.Contains("Blast"))
			{
				GetComponent<BoxCollider2D>().enabled = false;
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
					beam.gameObject.GetComponent<Animator>().Play("Nothing");

                }
			}
			else
			{
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

			Destroy(obj);
		}
    }
}
