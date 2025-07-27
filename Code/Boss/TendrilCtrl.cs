using System.Collections;
using UnityEngine;
class TendrilCtrl : MonoBehaviour
{
	private SpriteRenderer spriteRender;
	private BoxCollider2D boxCol;
	public bool infinite;

	// SETTING STUFF UP
	void Awake()
	{
		spriteRender = gameObject.GetComponent<SpriteRenderer>();
		boxCol = gameObject.GetComponent<BoxCollider2D>();
		gameObject.AddComponent<DamageHero>();
	}

	void Start()
	{
		AssignValues();
		StartCoroutine(Tendrils());
	}

	private void AssignValues()
	{
		gameObject.layer = 11;
		Destroy(boxCol);
		transform.SetPositionY(67f);

		gameObject.transform.GetComponent<SpriteRenderer>().material = new Material(Shader.Find("Sprites/Default"));
	}

	private IEnumerator Tendrils()
	{
		// play animation
		yield return new WaitForSeconds(1f);
		boxCol.enabled = true;
	}

	public void Kill()
	{
		boxCol.enabled = false;
		//play anim
		Destroy(gameObject);
	}

	void OnCollisionEnter2D(Collision2D col)
	{
		boxCol.enabled = false;
		//play anim

		if (infinite)
			StartCoroutine(Tendrils());
		else
			Destroy(gameObject);
	}
}