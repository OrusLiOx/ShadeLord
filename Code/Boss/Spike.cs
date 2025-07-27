using System.Collections;
using UnityEngine;

public class Spike : MonoBehaviour
{
	private Animator anim;
	private BoxCollider2D col;
	public static float spawnTime = .4f, upTime = 5 / 24f, activeTime = .5f, downTime = 5 / 24f;

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
		yield return new WaitForSeconds(spawnTime);

		anim.Play("TendrilUp");
		yield return new WaitForSeconds(upTime);
		col.enabled = true;
		yield return new WaitForSeconds(activeTime);

		col.enabled = false;
		anim.Play("TendrilDown");
		yield return new WaitForSeconds(downTime);

		Destroy(gameObject);
	}
}
