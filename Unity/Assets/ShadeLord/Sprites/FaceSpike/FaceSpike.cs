using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceSpike : MonoBehaviour
{
	public Animator outline, anim;
	public BoxCollider2D col;
	public SpriteRenderer channel;
	// Start is called before the first frame update
	void OnEnable()
	{
		Transform[] t = transform.GetComponentsInChildren<Transform>();
		outline = t[1].GetComponent<Animator>();
		channel = t[2].GetComponent<SpriteRenderer>();
		anim = gameObject.GetComponent<Animator>();
		col = gameObject.GetComponent<BoxCollider2D>();

		StartCoroutine(go());
	}

	// Update is called once per frame
	IEnumerator go()
	{
		anim.Play("Nothing");
		outline.Play("Nothing");
		channel.enabled = true;
		col.enabled = false;
		yield return new WaitForSeconds(.7f);

		anim.Play("FaceSpike");
		outline.Play("FaceSpikeOutline");
		yield return new WaitForSeconds(3/12f);
		col.enabled = true;
		channel.enabled = false;
		yield return new WaitForSeconds(1f);

		col.enabled = false;
		anim.Play("FaceSpikeRetract");
		outline.Play("FaceSpikeOutlineRetract");
		yield return new WaitForSeconds(4/12f);

		Destroy(gameObject);
	}
}