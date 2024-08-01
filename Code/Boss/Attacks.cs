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
	private GameObject halo;

	// Helper variables
	private float xEdge, xCenter, yDef;
	private int sweepPos;
	private bool attacking, wait, forward, infiniteSpike, lastPhase, fireOnce, platPhase;
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
		sweepPos = 0;
		attacking = false;
		wait = false;
		infiniteSpike = false;
		lastPhase = false;
		platPhase = false;
		fireOnce = false;
		parent = GameObject.Find("HoldAttacks");
		halo = GameObject.Find("ShadeLord/Halo");

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
		atts.Add("Beam", GameObject.Find("ShadeLord/BeamOrigin/Offset"));
		List<string> names = new List<string> { "Dash", "CrossSlash", "Sweep", "BurstSpike", "Spike", "BeamOrigin", "VoidBurst", "VoidCircle", "TendrilWindup" };
		foreach (string n in names)
		{
			atts.Add(n, GameObject.Find("ShadeLord/" + n));
			atts[n].SetActive(false);
		}
		atts.Add("TendrilBurst", GameObject.Find("ShadeLord/Tendrils"));
		atts["TendrilBurst"].GetComponent<BoxCollider2D>().enabled = false;

		// Get audio clips
		foreach (AudioSource s in GameObject.Find("ShadeLord/SFX").GetComponents<AudioSource>())
		{
			sounds.Add(s.clip.name, s.clip);
		}
	}
	/*
	 * GameObject obj;
		atts.Add("Beam", GameObject.Find("BeamOrigin/Offset"));
		Dictionary<string, Vector3> dict = new Dictionary<string, Vector3>();
		dict.Add("Dash", new Vector3(0f,-.3f,0));
		dict.Add("CrossSlash", new Vector3(0,0, -0.01f));
		dict.Add("Sweep", new Vector3(0,0, -0.01f));
		dict.Add("BurstSpike", new Vector3(0,-2.22f, -.2f));
		dict.Add("Spike", new Vector3(.31f, -9.38f, -.002f));
		dict.Add("BeamOrigin", new Vector3(-2, -2.05f,0));
		dict.Add("VoidBurst", new Vector3(-14.37f, -0.1300049f,-.01f));
		dict.Add("VoidCircle", new Vector3(-1.9f,17.9f,-.3f));
		dict.Add("TendrilWindup", new Vector3(0,0,0));
		foreach (string n in dict.Keys)
		{
			obj = GameObject.Find(n);
			obj.transform.SetParent(transform);
			obj.transform.position = dict[n];
			atts.Add(n, obj);
			atts[n].SetActive(false);
		}
		obj = GameObject.Find("Tendrils");
		obj.transform.SetParent(transform);
		obj.transform.position = new Vector3(0, -5.349998f, 0);
		atts.Add("TendrilBurst", obj);
		atts["TendrilBurst"].GetComponent<BoxCollider2D>().enabled = false;
	//*/
	// Attacks
	// sharp shadow from one side of the stage to the other
	public void Dash()
	{
		attacking = true;
		forward = false;
		StartCoroutine(DashV2());

		// dash across stage three times starting at the bottom and acending each dash
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
			halo.transform.localPosition = new Vector2(.15f, 4.98f);
			transform.SetPositionY( 68.04f);
			col.offset = new Vector2(0, 5.87f);
			yield return new WaitForSeconds(1 / 12f);
			halo.transform.localPosition = new Vector2(6.1f, 3.53f);
			col.offset = new Vector2(5.98f, 4.21f);
			col.size = new Vector2(3.13f, 3.86f);
			yield return new WaitForSeconds(1 / 12f);
			halo.transform.localPosition = new Vector2(9.5f, 0.08f);
			col.offset = new Vector2(9.3f, 0);
			yield return new WaitForSeconds(12 / 12f);

			// go
			anim.Play("DashLoop");
			for (int i = 0; i < 3; i++)
			{
				playSound("DashStart");
				aud.clip = sounds["DashLoop"];
				aud.loop = true;

				halo.transform.localPosition = new Vector2(11.23f, 0.04f);
				col.offset = new Vector2(6, -.07f);
				col.size = new Vector2(19, 2.36f);
				atts["Dash"].SetActive(true);
				if (goright)
					rig.velocity = new Vector2(50f, 0f);
				else
					rig.velocity = new Vector2(-50f, 0f);

				yield return new WaitUntil(() => Mathf.Abs(transform.position.x - xCenter) > (xEdge + 20f));

				goright = !goright;
				if (goright)
				{
					transform.localScale = new Vector3(1, 1, 1);
					transform.SetPositionX(xCenter - xEdge - 10f);
				}
				else
				{

					transform.localScale = new Vector3(-1, 1, 1);
					transform.SetPositionX(xCenter + xEdge + 10f);
				}
				transform.SetPositionY(transform.GetPositionY() + 5f);
			}

			// end
			halo.transform.localPosition = new Vector2(0f, -2.62f);
			atts["Dash"].SetActive(false);
			Hide();
			attacking = false;
		}
		
		// dash towards player location 3 times
		IEnumerator DashV2()
		{
			float dir = 0, x, y = 0;
			/*
			anim.Play("DashLoop");
			transform.position = new Vector2(xCenter, yDef);
			while (true)
			{
				x = transform.position.x - target.transform.position.x;
				y = transform.position.y - target.transform.position.y;
				if (x == 0)
					x = .001f;
				dir = (float)Math.Atan((y / x)) * 180.0f / (float)Math.PI;
				Modding.Logger.Log(dir);
				if (x > 0)
				{
					transform.SetScaleX(-1);
				}
				else
					transform.SetScaleX(1);
				transform.SetRotationZ(dir);
				Vector2 force = new Vector2(x, y).normalized * -10f;
				rig.velocity = force;
				yield return new WaitForSeconds(.1f);
			}//*/
			// pick direction
			bool goright = UnityEngine.Random.Range(0, 1f) > .5f;
			if (Math.Abs(target.transform.position.x - xCenter) > xEdge + 15)
			{
				goright = target.transform.position.x > xCenter;
			}
			if (goright)
			{
				transform.localScale = new Vector3(1, 1, 1);
				transform.SetPositionX(xCenter - xEdge - 30);
				x = -1;
			}
			else
			{

				transform.localScale = new Vector3(-1, 1, 1);
				transform.SetPositionX(xCenter + xEdge + 30);
				dir = (float)Math.PI;
				x = 1;
			}
			arrive();
			yield return new WaitWhile(() => wait);

			// setup
			anim.Play("DashLoop");
			transform.SetPositionY(68.04f);
			halo.transform.localPosition = new Vector2(11.23f, 0.04f);
			col.offset = new Vector2(6, -.07f);
			col.size = new Vector2(19, 2.36f);

			Modding.Logger.Log("Dash");
			// go
			for (int i = 0; i < 3; i++)
			{
				x = transform.position.x - target.transform.position.x;
				y = transform.position.y - target.transform.position.y;
				if (x == 0)
					x = .001f;
				dir = (float)Math.Atan((y / x)) * 180.0f / (float)Math.PI;//*
				if (x > 0)
				{
					transform.SetScaleX(-1);
				}
				else
					transform.SetScaleX(1);
				transform.SetRotationZ(dir);//*/
				Modding.Logger.Log(transform.position + " " + target.transform.position + " " + dir);

				// dash
				playSound("DashStart");
				atts["Dash"].SetActive(true);
				rig.velocity = new Vector2(x, y).normalized * -50f;

				yield return new WaitForSeconds(3f);
				//yield return new WaitUntil(() => (Mathf.Abs(transform.position.x - xCenter) > (xEdge + 20f) || Mathf.Abs(transform.position.y - 76.1f) > 20f));

				// dash finish
				atts["Dash"].SetActive(false);
				rig.velocity = new Vector2(0f, 0f);
			}

			// end
			halo.transform.localPosition = new Vector2(0f, -2.62f);
			transform.SetRotationZ(0);

			transform.SetScaleX(1);
			Hide();
			attacking = false;
		}
	}
	// Facing the camera, slash in an x in front of self, then sweeping slash across the floor
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
	// Spikes shoot out of the floor
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

			// first set
			anim.Play("RoarWait");
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
			//playSound("Scream");
			playSound("SpikeUpLower");
			anim.Play("Roar");
			yield return new WaitForSeconds(1f + 3/12f);

			if(!infiniteSpike)
				leave();

			// fire 3 sets of spikes with random offset
			int i = 2;
			while (i > 0 || infiniteSpike)
			{
				// generate spikes
				offset = xCenter - xEdge + UnityEngine.Random.Range(0, 2.4f);

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
				yield return new WaitForSeconds(1f + 3 / 12f);
				i--;
			}

			// end
			yield return new WaitForSeconds(2f);
			attacking = false;
		}
	}
	// fire beam that sweeps across the screen
	public void SweepBeam()
	{
		attacking = true;
		forward = false;
		StartCoroutine(SweepBeam());

		IEnumerator SweepBeam()
		{
			// pick location
			bool goright=true;
			if (lastPhase)
			{
				int i = UnityEngine.Random.Range(0, 2);
				if (i >= sweepPos)
					i++;
				sweepPos = i;
				switch (i)
				{
					case 0: // center
						transform.SetPositionX(xCenter);
						goright = UnityEngine.Random.Range(0, 2) == 0;
						break;
					case 1: // on left
						transform.SetPositionX(xCenter - xEdge + 9);
						goright = true;
						break;
					case 2: // on right
						transform.SetPositionX(xCenter + xEdge - 9);
						goright = false;
						break;
				}
			}
			else if (platPhase)
			{
				transform.SetPositionX(xCenter);
				goright = target.transform.position.x > xCenter;
			}
			else
			{
				goright = UnityEngine.Random.Range(0, 1f) > .5f;
				if (Math.Abs(target.transform.position.x - xCenter) > xEdge + 15)
				{
					goright = target.transform.position.x > xCenter;
				}


				if (goright)
				{
					transform.SetPositionX(xCenter - xEdge + 5);
				}
				else
				{
					transform.SetPositionX(xCenter + xEdge - 5);
				}
			}
			
			if (!goright)
				transform.localScale = new Vector3(-1, 1, 1);
			else
				transform.localScale = new Vector3(1, 1, 1);

			Transform beam = atts["BeamOrigin"].transform;
			SpriteRenderer head = transform.Find("BeamOrigin/Head").GetComponent<SpriteRenderer>();
			head.enabled = false;

			arrive();
			yield return new WaitUntil(() => !wait);
			yield return new WaitForSeconds(.3f);

			// charge
			beam.SetRotationZ(0f);
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
			float speed = -120f/90f;

			for (float f = 0f; f >= -60f; f += speed)
			{
				beam.SetRotationZ(f);
				yield return new WaitForSeconds(1 / 60f);
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
	// fire several spikes out of its face
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
	// tendrils emerge from the sides of the boss
	public void TendrilBurst()
	{
		attacking = true;
		forward = true;
		StartCoroutine(TendrilBurst());
		IEnumerator TendrilBurst()
		{
			// setup
			transform.SetPositionX(xCenter);
			if(platPhase)
				transform.SetPositionX(target.transform.GetPositionX());
			arrive();

			yield return new WaitUntil(() => !wait);

			// windup
			GameObject tendrils = atts["TendrilBurst"];
			tendrils.transform.SetScaleX(.1f);
			tendrils.transform.SetScaleY(.1f);


			//anim.Play("NeutralClose");
			atts["TendrilWindup"].SetActive(true);
			Transform windup = atts["TendrilWindup"].transform.GetChild(1);
			for (float f = .5f; f < 1.5f; f += .2f)
			{
				windup.SetScaleX(f);

				yield return new WaitForSeconds(1 / 12f);
			}
			windup.SetScaleX(1.5f);
			yield return new WaitForSeconds(1f);

			// attack
			float increment = .9f/(12*.3f);
			tendrils.SetActive(true);
			anim.Play("NeutralSquint");
			//playSound("Scream");
			playSound("TendrilsEmerge");
			playSound("TendrilWhip");
			for (float s = .1f; s < 1; s += increment)
			{
				tendrils.transform.SetScaleX(s);
				tendrils.transform.SetScaleY(s);
				yield return new WaitForSeconds(1/12f);
			}

			tendrils.GetComponent<PolygonCollider2D>().enabled = true;
			yield return new WaitForSeconds(2f);
			tendrils.GetComponent<PolygonCollider2D>().enabled = false;

			anim.Play("NeutralIdle");
			for (float s = 1f; s > 0; s -= increment)
			{
				tendrils.transform.SetScaleX(s);
				tendrils.transform.SetScaleY(s);
				windup.SetScaleX(1.5f*s);
				yield return new WaitForSeconds(1 / 12f);
			}
			yield return new WaitForSeconds(.3f);
			atts["TendrilWindup"].SetActive(false);
			tendrils.SetActive(false);
			// end
			leave();
			yield return new WaitUntil(() => !wait);
			attacking = false;
		}
	}
	// pure vessel focus
	public void VoidCircles()
	{
		attacking = true;
		forward = true;
		if(lastPhase)
			StartCoroutine(SpamCircles());
		else
			StartCoroutine(VoidCircles());

		IEnumerator VoidCircles()
		{
			// setup
			GameObject.Find("ShadeLord/Halo").GetComponent<SpriteRenderer>().enabled = false;
			transform.SetPositionX(UnityEngine.Random.Range(xCenter - xEdge + 4, xCenter + xEdge - 4));
			if (lastPhase)
			{
				switch (UnityEngine.Random.Range(0,3))
				{
					case 0:
						transform.SetPositionX(xCenter);
						break;
					case 1:
						transform.SetPositionX(xCenter - xEdge + 4);
						break;
					case 2:
						transform.SetPositionX(xCenter + xEdge - 4);
						break;
				}
			}
			arrive();
			yield return new WaitWhile(() => wait);
			yield return new WaitForSeconds(.5f);
			playSound("BeamCharge");
			anim.Play("RoarWait");
			// spawn circle on self
			GameObject obj = Instantiate(atts["VoidCircle"], parent.transform);
			obj.GetComponent<VoidCircle>().size = 1f;
			obj.transform.SetPosition3D(transform.GetPositionX(), transform.GetPositionY() - 2.37f, transform.GetPositionZ()+.001f);
			obj.SetActive(true);
			obj.GetComponent<VoidCircle>().Appear();

			yield return new WaitForSeconds(1.5f);

			anim.Play("Roar");
			playSound("Scream");
			playSound("BeamBlast");
			obj.GetComponent<VoidCircle>().Fire();
			//yield return new WaitForSeconds(.5f);

			// pick random points to spawn
			float curX = xCenter - xEdge-2.5f + UnityEngine.Random.Range(5f,9f);
			float wait2 = 1.5f;
			while (curX< xCenter + xEdge)
			{
				StartCoroutine(MakeCircle(curX, UnityEngine.Random.Range(66f, 76f), 1.5f));
				yield return new WaitForSeconds(.15f);
				wait2 -= .15f;
				curX += UnityEngine.Random.Range(5f, 9f);
			}
			if(wait2>0)
				yield return new WaitForSeconds(wait2);
			anim.Play("NeutralIdle");
			yield return new WaitForSeconds(.5f);

			// end
			leave();
			yield return new WaitForSeconds(.5f);
			yield return new WaitUntil(() => !wait);
			GameObject.Find("ShadeLord/Halo").GetComponent<SpriteRenderer>().enabled = true;
			yield return new WaitForSeconds(1f);
			attacking = false;
		}
		IEnumerator SpamCircles()
		{
			float tileSize = 10;
			int xMax = 5;
			int yMax = 5;
			for (int n = 0; n < xMax; n++)
			{
				for (int m = 0; m < yMax; m++)
				{
					float x = target.transform.position.x + n*tileSize - xMax/2 * tileSize + UnityEngine.Random.Range(-5f, 5f);
					float y = target.transform.position.y + m*tileSize - yMax/2 * tileSize + UnityEngine.Random.Range(-5f, 5f);
					StartCoroutine(MakeCircle(x, y, 1f));
				}
			}

			yield return new WaitForSeconds(1f);
			StartCoroutine(SpamCircles());
		}
		IEnumerator MakeCircle(float x, float y, float wait)
		{
			GameObject obj = Instantiate(atts["VoidCircle"], parent.transform);
			obj.transform.SetPosition2D(x, y);
			obj.SetActive(true);
			obj.GetComponent<VoidCircle>().size = .3f;
			obj.GetComponent<VoidCircle>().Appear();

			yield return new WaitForSeconds(wait);

			obj.GetComponent<VoidCircle>().Fire();
			//playSound("BeamBlast");
		}
	}
	// void beams
	public void VoidBeams()
	{
		attacking = true;
		forward = true;
		StartCoroutine(VoidBeams());
		IEnumerator VoidBeams()
		{
			yield return new WaitForSeconds(1f);
		}
	}

	// UNUSED
	// spawn several orbs that explode into crosses
	public void VoidBurst()
	{
		attacking = true;
		forward = true;
		StartCoroutine(VoidBurst());

		IEnumerator VoidBurst()
		{
			// pick rand location
			transform.SetPositionX(UnityEngine.Random.Range(xCenter - xEdge + 4, xCenter + xEdge - 4));
			arrive();
			yield return new WaitUntil(() => !wait);

			// spawn orbs
			//float rotation = UnityEngine.Random.Range(0f,359f);
			for (int k = (int)(xEdge / 6); k > 0; k--)
			{
				// pick random location
				float x = UnityEngine.Random.Range(xCenter - xEdge + 4, xCenter + xEdge - 4),
					  y = UnityEngine.Random.Range(yDef - 7, yDef + 3),
					  rotation = UnityEngine.Random.Range(0f, 359f);
				StartCoroutine(SpawnVoidBurst(x, y, 0, rotation, 1.5f));
				StartCoroutine(SpawnVoidBurst(x, y, .005f, rotation + 45, 1.5f));
			}


			yield return new WaitForSeconds(4f);
			leave();
			yield return new WaitUntil(() => !wait);
			attacking = false;

		}
		IEnumerator SpawnVoidBurst(float x, float y, float z, float r, float wait)
		{
			List<GameObject> burst = new List<GameObject>();
			for (int i = 0; i < 4; i++)
				burst.Add(Instantiate(atts["VoidBurst"], parent.transform));

			for (int i = 0; i < 4; i++)
			{
				burst[i].transform.SetPosition3D(x, y, z);
				burst[i].transform.SetRotationZ(r + 90 * i);
				burst[i].SetActive(true);
			}
			yield return new WaitForSeconds(wait);

			for (int i = 0; i < 4; i++)
			{
				burst[i].GetComponent<VoidBurst>().Fire();
			}
			playSound("BeamBlast");
		}
	}
	// fire beam at player that remains in place while several vertical beams sequentially fire at the player 
	public void AimBeam()
	{
		attacking = true;
		forward = false;
		StartCoroutine(AimBeam());

		IEnumerator AimBeam()
		{
			List<GameObject> beams = new List<GameObject>();
			// go to side further away from player
			bool goright = target.transform.position.x > xCenter;

			Transform beam = atts["BeamOrigin"].transform;
			SpriteRenderer head = transform.Find("BeamOrigin/Head").GetComponent<SpriteRenderer>();
			head.enabled = false;

			if (goright)
			{
				transform.localScale = new Vector3(1, 1, 1);
				transform.SetPositionX(xCenter - xEdge + 2.5f);
			}
			else
			{
				transform.localScale = new Vector3(-1, 1, 1);
				transform.SetPositionX(xCenter + xEdge - 2.5f);
			}
			arrive();
			yield return new WaitUntil(() => !wait);
			yield return new WaitForSeconds(.3f);

			anim.Play("Body");
			head.enabled = true;

			// FIRE MAIN BEAM
			// targeting
			float deg = getAngle(beam, target);

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
			beam.SetRotationZ(deg);
			/*
			while (true)
			{
				yield return new WaitForSeconds(.1f);
				deg = getAngle(beam, target);

				deg = (Math.Min(Math.Max(deg, -35), 17));
				if (!goright)
				{
					//-35, 17
					deg *= -1;
				}

				// charge
				beam.SetRotationZ(deg);
			}//*/
			//*
			yield return new WaitForSeconds(1f);

			// fire
			playSound("BeamBlast");
			yield return new WaitForSeconds(1f);

			// fire vertical beams
			for (int i = 0; i < 4; i++)
			{
				Modding.Logger.Log(i);

				// fire
				GameObject b = spawnVerticalBeam(yDef - 10f);
				beams.Add(b);
				b.transform.SetPositionZ(b.transform.GetPositionZ()+i*.001f);
				yield return new WaitForSeconds(1f);
				playSound("BeamBlast");
			}
			//*/
			yield return new WaitForSeconds(3f);
			foreach (GameObject obj in beams)
				Destroy(obj);

			// end
			atts["BeamOrigin"].SetActive(false);
			eyes(true);
			yield return new WaitUntil(() => !wait);
			leave();
			yield return new WaitUntil(() => !wait);
			attacking = false;
		}
	}
	private GameObject spawnVerticalBeam(float y)
	{
		GameObject beam = Instantiate(atts["Beam"], parent.transform);
		/*
		beam.transform.SetPositionY(60f);
		beam.transform.SetPositionX(target.transform.GetPositionX() + UnityEngine.Random.Range(-1f, 1f));
		beam.transform.SetRotationZ(getAngle(beam, target));//*/

		/*
		float angle = UnityEngine.Random.Range(-10f,10f);
		beam.transform.SetRotationZ(angle + 90);// UnityEngine.Random.Range(100f, 80f));
		beam.transform.SetPositionY(60f);
		beam.transform.SetPositionX(target.transform.GetPositionX()+Mathf.Tan(angle/180f*(float)Math.PI)*(target.transform.position.y - 60f));//*/

		beam.transform.SetRotationZ(90f);// UnityEngine.Random.Range(100f, 80f));
		beam.transform.SetPositionY(60f);
		beam.transform.SetPositionX(target.transform.GetPositionX());

		beam.SetActive(true);
		return beam;
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
		//Hide();
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
				platPhase = true;
				xEdge = 43.5f;
				break;
			case 4:
				xEdge = 17.71f; xCenter = 217.61f; yDef = 14f;
				lastPhase = true;
				platPhase = false;
				fireOnce = true;
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
			if (fireOnce)
			{
				Hide();
				transform.localScale = new Vector3(1, 1, 1);
				fireOnce = false;
			}
			else
			{
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
			}
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
		transform.SetPositionY(max-25f);
		halo.SetActive(true);
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

	// helpers
	private void rotate(Transform t, float r)
	{
		t.Rotate(new Vector3(0, 0, t.rotation.z + r));
	}
	public void playSound(string clip)
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

	private float getAngle(Transform from, Transform to)
	{
		float x = (transform.position.x) - (to.transform.position.x);
		float y = (transform.position.y) - (to.transform.position.y+.5f);
		Modding.Logger.Log((float)(Math.Atan((y / x)) * 180.0 / Math.PI));
		return (float)(Math.Atan((y / x)) * 180.0 / Math.PI);
	}
	private float getAngle(GameObject from, GameObject to)
	{
		return getAngle(from.transform, to.transform);
	}
	private float getAngle(Transform from, GameObject to)
	{
		return getAngle(from, to.transform);
	}
	private float getAngle(GameObject from, Transform to)
	{
		return getAngle(from.transform, to);
	}
}
