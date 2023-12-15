/*/

Upon spawning, launch in a random horizontal direction and fade away upon colliding with ground

/*/

using System.Collections;
using UnityEngine;


class RockParticle : MonoBehaviour
{
	// SETTING STUFF UP
	void Start()
	{
		gameObject.GetComponent<Rigidbody2D>().gravityScale = 1.5f;
		gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(Random.Range(0, 30f)-15f, Random.Range(0, 10f)-5f);
		StartCoroutine(die());
	}

	IEnumerator die()
	{
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