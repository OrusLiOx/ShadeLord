using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spike : MonoBehaviour
{
	private Animator anim;
	private BoxCollider2D col;

	private void OnEnable()
	{
		anim = transform.GetComponent<Animator>();
		col = transform.GetComponent<BoxCollider2D>();
		col.enabled = false;
		StartCoroutine(go());

	}
	IEnumerator go()
	{
		anim.Play("TendrilSpawn");
		yield return new WaitForSeconds(.7f);

		anim.Play("TendrilUp");
		yield return new WaitForSeconds(3/12f);
		col.enabled = true;
		yield return new WaitForSeconds(1f);

		col.enabled = false;
		anim.Play("TendrilDown");
		yield return new WaitForSeconds(5/12f);

		Destroy(gameObject);
	}
}
