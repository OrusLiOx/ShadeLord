using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attacks : MonoBehaviour
{
	public float xEdge, xCenter, yDef;
	public GameObject target, parent;
	public int phase;
	public bool attacking;
	private bool wait;
	private bool forward;

	private BoxCollider2D col;
	private Animator anim;
	private Rigidbody2D rig;
	private AudioSource aud;

	public List<GameObject> listGO;
	public List<AudioClip> listAudio;
	private Dictionary<string, GameObject> atts;
	private Dictionary<string, AudioClip> sounds;

	private void Start()
	{
		xEdge = 18; xCenter = 100; yDef = 75.23f;
		attacking = false;
		col = GetComponent<BoxCollider2D>();
		anim = GetComponent<Animator>();
		rig = GetComponent<Rigidbody2D>();
		aud = GetComponent<AudioSource>();

		atts = new Dictionary<string, GameObject>();
		sounds = new Dictionary<string, AudioClip>();

		foreach (GameObject g in listGO )
			atts.Add(g.name, g);
		foreach (AudioClip g in listAudio)
			sounds.Add(g.name, g);
	}

	public void Dash()
	{
		attacking = true;
		forward = false;
		StartCoroutine(Dash());
		IEnumerator Dash()
		{
			//	pick direction
			bool goright = Random.Range(0, 1f) > .5f;

			if (goright)
			{
				transform.localScale = new Vector3(1, 1, 1);
				setX(transform, xCenter - xEdge + 4);
			}
			else
			{

				transform.localScale = new Vector3(-1, 1, 1);
				setX(transform, xCenter + xEdge - 4);
			}
			arrive();
			yield return new WaitUntil(() => !wait);

			// windup
			anim.Play("DashWindup");
			setY(transform, 68.04f);
			col.offset = new Vector2(0, 5.87f);
			yield return new WaitForSeconds(1/12f);
			col.offset = new Vector2(5.98f, 4.21f);
			col.size = new Vector2(3.13f, 3.86f);
			yield return new WaitForSeconds(1/12f);
			col.offset = new Vector2(9.3f,0);
			yield return new WaitForSeconds(4 / 12f);

			// go
			anim.Play("DashLoop");
			playSound("DashStart");
			playSound("DashLoop");
			aud.loop = true;

			col.offset = new Vector2(6, -.07f);
			col.size = new Vector2(19,2.36f);
			atts["Dash"].SetActive(true);
			if (goright)
				rig.velocity = new Vector2(50f, 0f);
			else
				rig.velocity = new Vector2(-50f, 0f);
			
			yield return new WaitUntil(() => Mathf.Abs(transform.position.x - xCenter) > (xEdge+20f));

			// end
			aud.loop = false;
			aud.Stop();
			atts["Dash"].SetActive(false);
			hide();
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
			float x = target.transform.position.x;
			if (x > (xCenter + xEdge - 5f))
				x = xCenter + xEdge - 5f;
			else if (x < (xCenter - xEdge + 5f))
				x = xCenter - xEdge + 5f;
			setX(transform, x);
			arrive();

			yield return new WaitUntil(() => !wait);

			// cross slash windup
			anim.Play("CrossSlashCharge1");

			yield return new WaitForSeconds(2f/12f);

			// cross slash
			playSound("CrossSlashAttack1");

			yield return new WaitForSeconds(3f / 12f);

			anim.Play("CrossSlashAttack1");
			atts["CrossSlash"]. SetActive(true);

			yield return new WaitForSeconds(2/12f);

			atts["CrossSlash"].SetActive(false);

			yield return new WaitForSeconds(1 / 12f);

			// sweep windup
			if (target.transform.position.x < transform.position.x)
				transform.localScale = new Vector3(-1, 1, 1);

			anim.Play("CrossSlashCharge2");

			yield return new WaitForSeconds(3f/12f);

			// sweep
			playSound("CrossSlashAttack2");

			yield return new WaitForSeconds(3f / 12f);

			anim.Play("CrossSlashAttack2");
			atts["Sweep"].SetActive(true);

			yield return new WaitForSeconds(2/12f);

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
			setX(transform, Random.Range(xCenter - xEdge + 4, xCenter + xEdge - 4));
			arrive();
			yield return new WaitUntil(() => !wait);

			// fire 3 sets of spikes with random offset
			for (int i = 3; i > 0; i--)
			{
				anim.Play("NeutralSquint");
				// generate spikes
				int seg = 15;
				float offset = xCenter - xEdge + Random.Range(0, xEdge * 2 / seg);

				playSound("BeamCharge");

				for (int k = 0; k < seg; k++)
				{
					GameObject s = Instantiate(atts["Spike"], parent.transform);
					s.SetActive(true);
					setPos(s.transform, k * (xEdge * 2 / seg) + offset, 66.42f);
				}
				yield return new WaitForSeconds(.7f);
				// spikes go up
				playSound("SpikeUp");
				anim.Play("NeutralBlank");
				yield return new WaitForSeconds(3 / 12f);

				anim.Play("NeutralOpen");
				yield return new WaitForSeconds(.5f);
			}

			// end
			yield return new WaitForSeconds(.4f);
			leave();
			new WaitUntil(() => !wait);
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
			// go to side
			bool goright = Random.Range(0, 1f) > .5f;
			Transform beam = atts["BeamOrigin"].transform;
			SpriteRenderer head = transform.Find("BeamOrigin/head").GetComponent<SpriteRenderer>();
			head.enabled = false;

			if (goright)
			{
				transform.localScale = new Vector3(1, 1, 1);
				setX(transform, xCenter - xEdge + 5);
			}
			else
			{
				transform.localScale = new Vector3(-1, 1, 1);
				setX(transform, xCenter + xEdge - 5);
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
			playSound("BeamLoop");
			aud.loop = true;
			anim.Play("Body");
			head.enabled = true;

			// for each frame, rotate and reposition
			int incr = 40;
			float end = -60f;
			yield return new WaitForSeconds(1 / 24f);

			for (int i = 0; i< incr; i++)
			{
				rotate(beam, end/incr);
				yield return new WaitForSeconds(2f/incr);
			}

			// end
			atts["BeamOrigin"].SetActive(false);
			aud.Stop();
			aud.loop = false;
			eyes(true);
			new WaitUntil(() => !wait);

			yield return new WaitForSeconds(3 / 12f);
			leave();
			new WaitUntil(() => !wait);
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
			float offset = Random.Range(0,360f/seg);
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
					setPos(s.transform, transform.position.x, transform.position.y-2.22f);
					s.transform.Rotate(new Vector3(0,0, offset+k* 360f / seg));
				}
				yield return new WaitForSeconds(.7f);
				playSound("SpikeUp");
				offset += Random.Range(10, (360f / seg)-10);
			}

			// end
			yield return new WaitForSeconds(2f);
			eyes(true);
			yield return new WaitUntil(() => !wait);
			leave();//*/
			yield return new WaitUntil(()=>!wait);
			attacking = false;
		}
	}

	public void Stop()
	{
		StopAllCoroutines();
		foreach (Transform c in parent.GetComponentsInChildren <Transform> ())
		{
			if(!c.gameObject.name.Equals(parent.name))
				Destroy(c.gameObject);
		}
		foreach (GameObject g in atts.Values)
			g.SetActive(false);
		attacking = false;
	}

	private void resetHitBox()
	{
		col.size = new Vector2(3.86f, 4f);
		col.offset = new Vector2(0f, -1.96f);
		col.enabled = true;
	}

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
			hide();
			transform.localScale = new Vector3(1, 1, 1);
			wait = false;
		}
	}
	private void arrive()
	{
		StartCoroutine(arrive());
		IEnumerator arrive()
		{
			wait = true;
			setY(transform, 50f);
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
			yield return new WaitUntil(() => transform.position.y > yDef - 5);
			if (forward)
			{
				anim.Play("NeutralArrive");
			}
			else
			{
				anim.Play("SideArrive");
			}
			yield return new WaitUntil(() => transform.position.y > yDef);
			rig.velocity = new Vector2(0f, 0f);
			setY(transform, yDef);

			wait = false;
		}
	}
	private void hide()
	{
		anim.Play("Nothing");
		col.enabled = false;
	}

	private void playSound(string clip)
	{
		//aud.clip = sounds[clip];
		aud.PlayOneShot(sounds[clip]);
	}

	private void setX(Transform t, float x)
	{
		t.localPosition = new Vector3(x, t.localPosition.y, t.localPosition.z);
	}
	private void setY(Transform t, float y)
	{
		t.localPosition = new Vector3(t.localPosition.x, y, t.localPosition.z);
	}
	private void setPos(Transform t, float x, float y)
	{
		t.localPosition = new Vector3(x, y, t.localPosition.z);
	}
	private void rotate(Transform t, float r)
	{
		t.Rotate(new Vector3(0,0,t.rotation.z + r));
	}
}
