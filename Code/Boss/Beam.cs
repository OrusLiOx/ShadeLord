/*/

Script to play a windup then beam loop animation and sound for beam upon enabling
Intended for user to only have to worry about spawing and moving the beam

Attach this script to all beam objects

/*/

using System.Collections;
using UnityEngine;

public class Beam : MonoBehaviour
{
	private string anim1,anim2;
	public AudioClip charge;
	public AudioClip blast;
	private AudioSource audio;
	public void Start()
	{
		if (gameObject.name.Contains("Blast"))
		{
			audio = GetComponent<AudioSource>();
			audio.outputAudioMixerGroup = HeroController.instance.gameObject.GetComponent<AudioSource>().outputAudioMixerGroup;
		}
	}
	private void OnEnable()
	{
		if (gameObject.name.Contains("Blast"))
		{
			anim1 = "Nothing";
			anim2 = "BeamBlast";
		}
		else
		{
			anim2 = "Beam";
			if (gameObject.name.Contains("("))
				anim1 = "Windup";
			else
				anim1 = "WindupStart";
		}
		if (!gameObject.name.Contains("Blast"))
			GetComponent<BoxCollider2D>().enabled = false;
		StartCoroutine(go());
	}

	IEnumerator go()
	{
		if (gameObject.name.Contains("Blast"))
			audio.PlayOneShot(charge);
		gameObject.GetComponent<Animator>().Play(anim1);

		yield return new WaitForSeconds(1f);
		if (gameObject.name.Contains("Blast"))
		{
			GetComponent<BoxCollider2D>().enabled = true;
			audio.PlayOneShot(blast);
		}
		gameObject.GetComponent<Animator>().Play(anim2);
	}
}
