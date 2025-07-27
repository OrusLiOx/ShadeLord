using System.Collections;
using UnityEngine;

public class FaceSpike : MonoBehaviour
{
	private Animator outline, anim;
	private BoxCollider2D col;
	private SpriteRenderer channel;

	void OnEnable()
	{
		Transform[] t = transform.GetComponentsInChildren<Transform>();
		outline = t[1].GetComponent<Animator>();
		channel = t[2].GetComponent<SpriteRenderer>();
		anim = gameObject.GetComponent<Animator>();
		col = gameObject.GetComponent<BoxCollider2D>();

		StartCoroutine(go());
	}

	IEnumerator go()
	{
		anim.Play("Nothing");
		outline.Play("Nothing");
		channel.enabled = true;
		col.enabled = false;
		yield return new WaitForSeconds(.7f);

		// 1
		anim.Play("FaceSpike");
		outline.Play("FaceSpikeOutline");
		yield return new WaitForSeconds(1/12f);

		// 2
		col.offset = new Vector2(7.51f, 0);
		col.size = new Vector2(14.9f, .47f);
		col.enabled = true;
		yield return new WaitForSeconds(1 / 12f);

		// 3
		col.offset = new Vector2(11.01f, 0);
		col.size = new Vector2(21.9f, .47f);
		yield return new WaitForSeconds(1 / 12f);

		// 4
		col.offset = new Vector2(15.011f, 0);
		col.size = new Vector2(29.9f, .47f);

		channel.enabled = false;
		yield return new WaitForSeconds(1f);

		col.enabled = false;
		anim.Play("FaceSpikeRetract");
		outline.Play("FaceSpikeOutlineRetract");
		yield return new WaitForSeconds(1 / 12f);

		// 3
		col.offset = new Vector2(11.01f, 0);
		col.size = new Vector2(21.9f, .47f);
		
		yield return new WaitForSeconds(1 / 12f);

		// 2
		col.offset = new Vector2(7.51f, 0);
		col.size = new Vector2(14.9f, .47f);
		yield return new WaitForSeconds(1 / 12f);

		// 1
		col.enabled = false;
		yield return new WaitForSeconds(1 / 12f);

		Destroy(gameObject);
	}
}