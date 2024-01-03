/*/

Script to execute and stop Shade Lord's attacks

/*/

using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Attacks : MonoBehaviour
{
	public GameObject target;

	// Helper variables
	private float xEdge, xCenter, yDef;
	private bool attacking, wait, forward, infiniteSpike, lastPhase;
	private GameObject parent;

	// Unity Components
	private BoxCollider2D col;
	private Animator anim;
	private Rigidbody2D rig;
	private AudioSource aud;

	// Dicts for easy access to sound clips and objects needed for attacks
	private Dictionary<string, GameObject> atts;
	public Dictionary<string, AudioClip> sounds;

	// set variables upon awake
	private void Awake()
	{
		// Helper variables
		xEdge = 18; xCenter = 100; yDef = 75.23f;
		attacking = false;
		wait = false;
		infiniteSpike = false;
		lastPhase = false;
		parent = GameObject.Find("HoldAttacks");

		// Unity Components
		col = GetComponent<BoxCollider2D>();
		anim = GetComponent<Animator>();
		rig = GetComponent<Rigidbody2D>();
		aud = GetComponent<AudioSource>();

		target = HeroController.instance.gameObject;
		aud.outputAudioMixerGroup = target.GetComponent<AudioSource>().outputAudioMixerGroup;

		// Load dicts
		atts = new Dictionary<string, GameObject>();
		sounds = new Dictionary<string, AudioClip>();

		// Get attacks
		List<string> names = new List<string> { "Dash", "CrossSlash", "Sweep", "BurstSpike", "Spike", "BeamOrigin" };
		foreach (string n in names)
		{
			atts.Add(n, GameObject.Find("ShadeLord/" + n));
			atts[n].SetActive(false);
		}

		// Get audio clips
		foreach (AudioSource s in GameObject.Find("ShadeLord/SFX").GetComponents<AudioSource>())
			sounds.Add(s.clip.name, s.clip);
	}

	// Attacks
	public void Dash()
	{
		attacking = true;
		forward = false;
		StartCoroutine(Dash());
		IEnumerator Dash()
		{
			//	pick direction
			bool goright = UnityEngine.Random.Range(0, 1f) > .5f;
			if (Math.Abs(target.transform.position.x - xCenter) > xEdge + 15)
			{
				goright = target.transform.position.x > xCenter;
			}
			if (goright)
			{
				transform.localScale = new Vector3(1, 1, 1);
				transform.SetPositionX(xCenter - xEdge);
			}
			else
			{

				transform.localScale = new Vector3(-1, 1, 1);
				transform.SetPositionX(xCenter + xEdge);
			}
			arrive();
			yield return new WaitWhile(() => wait);

			// windup
			anim.Play("DashWindup");
			transform.SetPositionY( 68.04f);
			col.offset = new Vector2(0, 5.87f);
			yield return new WaitForSeconds(1 / 12f);
			col.offset = new Vector2(5.98f, 4.21f);
			col.size = new Vector2(3.13f, 3.86f);
			yield return new WaitForSeconds(1 / 12f);
			col.offset = new Vector2(9.3f, 0);
			yield return new WaitForSeconds(12 / 12f);

			// go
			anim.Play("DashLoop");
			playSound("BeamBlast");
			aud.clip = sounds["DashLoop"];
			aud.loop = true;

			col.offset = new Vector2(6, -.07f);
			col.size = new Vector2(19, 2.36f);
			atts["Dash"].SetActive(true);
			if (goright)
				rig.velocity = new Vector2(25f, 0f);
			else
				rig.velocity = new Vector2(-25f, 0f);

			yield return new WaitUntil(() => Mathf.Abs(transform.position.x - xCenter) > (xEdge + 20f));

			// end
			atts["Dash"].SetActive(false);
			Hide();
			attacking = false;
		}
	}
	public void CrossSlash()
	{
		attacking = true;
		forward = true;
		StartCoroutine(CrossSlash());
		IEnumerator CrossSlash()
		{
			// go to player
			float x = target.transform.position.x, aDelay, sDelay;
			if (x > (xCenter + xEdge - 5f))
				x = xCenter + xEdge - 5f;
			else if (x < (xCenter - xEdge + 5f))
				x = xCenter - xEdge + 5f;
			transform.SetPositionX(x);
			arrive();

			yield return new WaitUntil(() => !wait);

			// cross slash windup
			anim.Play("CrossSlashCharge1");

			aDelay = 6; sDelay = 2.95f;
			yield return new WaitForSeconds(sDelay / 12f);

			// cross slash
			playSound("CrossSlashAttack1");

			yield return new WaitForSeconds((aDelay - sDelay) / 12f);

			anim.Play("CrossSlashAttack1");
			atts["CrossSlash"].SetActive(true);

			yield return new WaitForSeconds(2 / 12f);

			atts["CrossSlash"].SetActive(false);

			yield return new WaitForSeconds(1 / 12f);

			// sweep windup
			if (target.transform.position.x < transform.position.x)
				transform.localScale = new Vector3(-1, 1, 1);

			anim.Play("CrossSlashCharge2");
			aDelay = 6.5f; sDelay = 3.45f;
			yield return new WaitForSeconds(sDelay / 12f);

			// sweep
			playSound("CrossSlashAttack2");

			yield return new WaitForSeconds((aDelay - sDelay) / 12f);

			anim.Play("CrossSlashAttack2");
			atts["Sweep"].SetActive(true);

			yield return new WaitForSeconds(2 / 12f);

			atts["Sweep"].SetActive(false);

			yield return new WaitForSeconds(1 / 12f);

			// end
			anim.Play("CrossSlashEnd");
			yield return new WaitForSeconds(2 / 12f);

			leave();
			yield return new WaitUntil(() => !wait);

			attacking = false;
		}
	}
	public void Spikes()
	{
		attacking = true;
		forward = true;
		StartCoroutine(Spikes());

		IEnumerator Spikes()
		{
			// go to random location
			if (infiniteSpike)
				transform.SetPositionX(xCenter);
			else
				transform.SetPositionX(UnityEngine.Random.Range(xCenter - xEdge + 4, xCenter + xEdge - 4));
			arrive();
			yield return new WaitUntil(() => !wait);

			// fire 3 sets of spikes with random offset
			int i = 3;
			while (i > 0 || infiniteSpike)
			{
				anim.Play("NeutralSquint");
				// generate spikes
				float offset = xCenter - xEdge + UnityEngine.Random.Range(0, 2.4f);

				playSound("BeamCharge");
				while (offset < xCenter + xEdge)
				{
					GameObject s = Instantiate(atts["Spike"], parent.transform);
					s.SetActive(true);
					s.transform.SetPosition2D(offset, 66.42f);
					offset += 2f;
				}
				yield return new WaitForSeconds(.7f);
				// spikes go up
				playSound("SpikeUpLower");
				anim.Play("NeutralBlank");
				yield return new WaitForSeconds(3 / 12f);

				anim.Play("NeutralOpen");
				yield return new WaitForSeconds(1f);
				i--;
			}

			// end
			yield return new WaitForSeconds(.4f);
			leave();
			yield return new WaitUntil(() => !wait);
			attacking = false;
		}
	}
	public void SweepBeam()
	{
		attacking = true;
		forward = false;
		StartCoroutine(SweepBeam());

		IEnumerator SweepBeam()
		{
			// pick location
			bool goright = target.transform.position.x > transform.position.x;
			Transform beam = atts["BeamOrigin"].transform;
			SpriteRenderer head = transform.Find("BeamOrigin/Head").GetComponent<SpriteRenderer>();
			head.enabled = false;

			transform.SetPositionX(UnityEngine.Random.Range(xCenter - 14, xCenter + 14));
			if (goright)
			{
				transform.localScale = new Vector3(1, 1, 1);
			}
			else
			{
				transform.localScale = new Vector3(-1, 1, 1);
			}

			if (lastPhase)
			{
				if (UnityEngine.Random.Range(0, 3f) > 1)
					transform.SetPositionX(xCenter);
				else if (goright)
					transform.SetPositionX(xCenter - xEdge + 5);
				else
					transform.SetPositionX(xCenter + xEdge - 5);
			}
			arrive();
			yield return new WaitUntil(() => !wait);
			yield return new WaitForSeconds(.3f);

			// charge
			beam.rotation = transform.rotation;
			anim.Play("SweepBeamCharge");
			playSound("BeamCharge");
			atts["BeamOrigin"].SetActive(true);
			yield return new WaitForSeconds(1f);

			// fire
			playSound("BeamBlast");
			aud.clip = sounds["BeamLoop"];
			aud.loop = true;
			anim.Play("Body");
			head.enabled = true;

			// rotation
			int incr = 40;
			float end = -60f;
			yield return new WaitForSeconds(1 / 24f);

			for (int i = 0; i < incr; i++)
			{
				rotate(beam, end / incr);
				yield return new WaitForSeconds(1.5f / incr);
			}
			transform.localScale = new Vector3(transform.localScale.x * -1, 1, 1);
			for (int i = 0; i < incr; i++)
			{
				rotate(beam, -1 * end / incr);
				yield return new WaitForSeconds(1.5f / incr);
			}
			
			// end
			atts["BeamOrigin"].SetActive(false);
			aud.Stop();
			aud.loop = false;
			eyes(true);
			yield return new WaitUntil(() => !wait);

			yield return new WaitForSeconds(3 / 12f);
			leave();
			yield return new WaitUntil(() => !wait);
			attacking = false;
		}
	}
	public void FaceSpikes()
	{
		attacking = true;
		forward = true;
		StartCoroutine(FaceSpikes());

		IEnumerator FaceSpikes()
		{
			// go to random location
			int seg = 5;
			float offset = UnityEngine.Random.Range(0, 360f / seg);
			transform.SetPositionX(UnityEngine.Random.Range(xCenter - 14, xCenter + 14));
			arrive();
			yield return new WaitUntil(() => !wait);
			eyes(false);
			yield return new WaitUntil(() => !wait);

			// fire 4 sets of face spikes
			playSound("BeamCharge");
			for (int i = 0; i < 4; i++)
			{
				for (int k = 0; k < seg; k++)
				{
					GameObject s = Instantiate(atts["BurstSpike"], parent.transform);
					s.SetActive(true);
					s.transform.SetPosition3D(transform.position.x, transform.position.y - 2.22f, atts["BurstSpike"].transform.position.z);
					s.transform.Rotate(new Vector3(0, 0, offset + k * 360f / seg));
				}
				yield return new WaitForSeconds(.7f);
				playSound("SpikeUpLower");
				offset += UnityEngine.Random.Range(10, (360f / seg) - 10);

				yield return new WaitForSeconds(.3f);
			}

			// end
			yield return new WaitForSeconds(2f);
			eyes(true);
			yield return new WaitUntil(() => !wait);
			leave();//*/
			yield return new WaitUntil(() => !wait);
			attacking = false;
		}
	}
	public void AimBeam()
	{
		attacking = true;
		forward = false;
		StartCoroutine(AimBeam(4));

		IEnumerator AimBeam(int beams)
		{
			// go to side
			bool goright = UnityEngine.Random.Range(0, 1f) > .5f;
			if (Math.Abs(target.transform.position.x - xCenter) > xEdge + 15)
			{
				goright = target.transform.position.x > xCenter;
			}
			Transform beam = atts["BeamOrigin"].transform;
			SpriteRenderer head = transform.Find("BeamOrigin/Head").GetComponent<SpriteRenderer>();
			head.enabled = false;

			if (goright)
			{
				transform.localScale = new Vector3(1, 1, 1);
				transform.SetPositionX(xCenter - xEdge + 5);
			}
			else
			{
				transform.localScale = new Vector3(-1, 1, 1);
				transform.SetPositionX(xCenter + xEdge - 5);
			}
			arrive(70.98f);
			yield return new WaitUntil(() => !wait);
			yield return new WaitForSeconds(.3f);

			anim.Play("Body");
			head.enabled = true;

			//fire 4 beams
			for (int i = 0; i < beams; i++)
			{
				// too close
				if (Math.Abs(target.transform.position.x - transform.position.x) < 7f && target.transform.position.y - 15f < transform.position.y)
				{
					i = beams;
				}

				// targeting
				float x = (beam.transform.position.x + 0.91f) - (target.transform.position.x);
				float y = (beam.transform.position.y + .7f) - (target.transform.position.y);
				double deg = Math.Atan((y / x)) * 180.0 / Math.PI;

				deg = (Math.Min(Math.Max(deg, -35), 17));
				if (!goright)
				{
					//-35, 17
					deg *= -1;
				}

				// charge
				playSound("BeamCharge");
				atts["BeamOrigin"].SetActive(false);
				atts["BeamOrigin"].SetActive(true);
				beam.rotation = transform.rotation;
				beam.Rotate(new Vector3(0, 0, (float)deg));
				yield return new WaitForSeconds(1f);

				// fire
				playSound("BeamBlast");
				yield return new WaitForSeconds(.6f);
			}

			// end
			atts["BeamOrigin"].SetActive(false);
			eyes(true);
			yield return new WaitUntil(() => !wait);

			yield return new WaitForSeconds(3 / 12f);
			leave();
			yield return new WaitUntil(() => !wait);
			attacking = false;
		}
	}

	// Misc Public functions 
	public void Stop()
	{
		StopAllCoroutines();
		foreach (Transform c in parent.GetComponentsInChildren<Transform>())
		{
			if (!c.gameObject.name.Equals(parent.name))
				Destroy(c.gameObject);
		}
		foreach (GameObject g in atts.Values)
			g.SetActive(false);
		attacking = false;
		Hide();
	}
	public void Hide()
	{
		anim.Play("Nothing");
		col.enabled = false;
	}
	public void Phase(int phase)
	{
		switch (phase)
		{
			case 0:
				xEdge = 18; xCenter = 100; yDef = 75.23f;
				infiniteSpike = false;
				lastPhase = false;
				break;
			case 2:
				infiniteSpike = true;
				break;
			case 3:
				infiniteSpike = false;
				xEdge = 43.5f;
				break;
			case 4:
				xEdge = 17.71f; xCenter = 217.61f; yDef = 9.7f;
				lastPhase = true;
				break;
		}
	}

	public bool isAttacking()
	{
		return attacking;
	}

	// Common Coroutines
	private void eyes(bool open)
	{
		StartCoroutine(eyes(open));
		IEnumerator eyes(bool open)
		{
			wait = true;
			if (open)
			{
				if (forward)
					anim.Play("NeutralOpen");
				else
					anim.Play("SideOpen");
			}
			else
			{
				if (forward)
					anim.Play("NeutralClose");
				else
					anim.Play("SideClose");
			}
			yield return new WaitForSeconds(3 / 12f);
			wait = false;
		}
	}
	private void leave()
	{
		StartCoroutine(leave());
		IEnumerator leave()
		{
			wait = true;
			if (forward)
			{
				anim.Play("NeutralLeave");
			}
			else
			{
				anim.Play("SideLeave");
			}
			yield return new WaitForSeconds(2 / 12f);
			rig.velocity = new Vector2(0f, -30f);
			yield return new WaitUntil(() => transform.position.y < yDef - 20);
			rig.velocity = new Vector2(0f, 0f);
			Hide();
			transform.localScale = new Vector3(1, 1, 1);
			wait = false;
		}
	}
	private void arrive()
	{
		StartCoroutine(arriveRoutine(yDef));
	}
	private void arrive(float height)
	{
		StartCoroutine(arriveRoutine(height));
	}
	private IEnumerator arriveRoutine(float max)
	{
		wait = true;
		transform.SetPositionY(50f);
		resetHitBox();
		if (forward)
		{
			anim.Play("NeutralUp");
		}
		else
		{
			anim.Play("SideUp");
		}
		rig.velocity = new Vector2(0f, 30f);
		yield return new WaitUntil(() => transform.position.y > max - 5);
		if (forward)
		{
			anim.Play("NeutralArrive");
		}
		else
		{
			anim.Play("SideArrive");
		}
		yield return new WaitUntil(() => transform.position.y > max);
		rig.velocity = new Vector2(0f, 0f);
		transform.SetPositionY( max);

		wait = false;
	}

	// Simplify Unity actions
	private void rotate(Transform t, float r)
	{
		t.Rotate(new Vector3(0, 0, t.rotation.z + r));
	}
	private void playSound(string clip)
	{
		//aud.clip = sounds[clip];
		aud.GetComponent<AudioSource>().PlayOneShot(sounds[clip]);
	}
	private void resetHitBox()
	{
		col.size = new Vector2(3.86f, 4f);
		col.offset = new Vector2(0f, -1.96f);
		col.enabled = true;
	}
}
