/*/

Things i didnt feel like making a whole class for

/*/

using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;


class SLHelper : MonoBehaviour
{
// randomize animation times
	public void randomizeAnimStart(List<GameObject> objs, string anim, int frames)
	{
		IEnumerator waitThenPlay(GameObject obj, float wait)
		{
			yield return new WaitForSeconds(wait);
			obj.GetComponent<Animator>().Play(anim);
		}
		foreach (GameObject obj in objs)
		{
			StartCoroutine(waitThenPlay(obj, UnityEngine.Random.Range(0, frames) / 12f));
		}
	}

	
	
// rock particles
	public void launchRocks()
	{
		foreach (Rigidbody2D rb in GameObject.Find("Terrain/RocksLeft").GetComponentsInChildren<Rigidbody2D>())
		{
			StartCoroutine(launchRock(rb));
		}
		foreach (Rigidbody2D rb in GameObject.Find("Terrain/RocksRight").GetComponentsInChildren<Rigidbody2D>())
		{
			StartCoroutine(launchRock(rb));
		}
		foreach (Rigidbody2D rb in GameObject.Find("Terrain/RocksFloor").GetComponentsInChildren<Rigidbody2D>())
		{
			StartCoroutine(launchRock(rb));
		}
	}

	IEnumerator launchRock(Rigidbody2D rig)
	{
		rig.gravityScale = .7f;
		rig.velocity = new Vector2(UnityEngine.Random.Range(0, 30f) - 15f, UnityEngine.Random.Range(0, 10f) - 5f);
		yield return new WaitForSeconds(10f);
		Destroy(rig.gameObject);
	}


	public void fadeTo(SpriteRenderer sprite, Color target, float time)
	{
		StartCoroutine(fadeTo());
		IEnumerator fadeTo()
		{
			float frames = time * 30f;
			float
				r = (target.r - sprite.color.r) / frames,
				g = (target.g - sprite.color.g) / frames,
				b = (target.b - sprite.color.b) / frames,
				a = (target.a - sprite.color.a) / frames;
			float[] addVals = { 0, 0, 0, 0 };
			float[] subVals = { 0, 0, 0, 0 };

			if (r < 0)
				subVals[0] = r * -1;
			else
				addVals[0] = r * 1;

			if (g < 0)
				subVals[1] = g * -1;
			else
				addVals[1] = g * 1;

			if (b < 0)
				subVals[2] = b * -1;
			else
				addVals[2] = b * 1;

			if (a < 0)
				subVals[3] = a * -1;
			else
				addVals[3] = a * 1;//*/

			Color add = new Color(addVals[0], addVals[1], addVals[2], addVals[3]),
				sub = new Color(subVals[0], subVals[1], subVals[2], subVals[3]);

			for (int i = 0; i < frames; i++)
			{
				sprite.color += add;
				sprite.color -= sub;

				yield return new WaitForSeconds(1 / 30f);
			}
			sprite.color = target;
		}
	}
}