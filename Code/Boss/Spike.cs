/*/

Script to play animation and sound for spike and adjust hit box upon enabling
Intended for user to only have to worry about spawing the spikes

Attach this script to all spike objects (including outline)

/*/

using System.Collections;
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
		yield return new WaitForSeconds(.4f);

		anim.Play("TendrilUp");
		yield return new WaitForSeconds(5/24f);
		col.enabled = true;
		yield return new WaitForSeconds(.5f);

		col.enabled = false;
		anim.Play("TendrilDown");
		yield return new WaitForSeconds(5/24f);

		Destroy(gameObject);
	}
}
