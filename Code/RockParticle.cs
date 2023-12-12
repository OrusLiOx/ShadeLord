/*/ 20-11-2023

Upon spawning, launch in a random horizontal direction and fade away upon colliding with ground

/*/

using System.Collections;
using UnityEngine;


class RockParticle : MonoBehaviour
{

	// SETTING STUFF UP
	void Start()
	{
		System.Random rand = new System.Random();
		gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2((float)rand.NextDouble()*20f-10f, 0f);
	}
	void OnCollisionEnter2D(Collision2D col)
	{
		if (col.gameObject.layer == 8)
			StartCoroutine(die());
	}

	IEnumerator die()
	{
		yield return new WaitForSeconds(1f);
		SpriteRenderer sprite = gameObject.GetComponent<SpriteRenderer>();
		Color c = new Color(0, 0, 0, 1 / 20f);
		while (sprite.color.a > 0)
		{
			sprite.color -= c;
			yield return new WaitForSeconds(1 / 30f);
		}
	}
}