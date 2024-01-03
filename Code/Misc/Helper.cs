/*/

Things i didnt feel like making a whole class for

/*/

using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;


class Helper : MonoBehaviour
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
		SpriteRenderer sprite = gameObject.GetComponent<SpriteRenderer>();
		Color c = new Color(0, 0, 0, 1 / 20f);
		while (sprite.color.a > 0)
		{
			sprite.color -= c;
			yield return new WaitForSeconds(1 / 30f);
		}
		Destroy(gameObject);
	}
}