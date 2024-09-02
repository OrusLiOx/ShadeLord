/*/

Randomly spawns void particles between a given range of x values at elevation y
particles move upward and fade out of existence

particle density changed with setDensity(...)
can be disabled or enabled using setActive(...)
positions can be changed using set(...)

/*/

using System.Collections;
using UnityEngine;

public class VoidParticleSpawner : MonoBehaviour
{
	float minX, maxX, y, density, prev, yScale, xScale;
	float velMin=1, velMax=5;
	GameObject template = GameObject.Find("VoidParticle");
	Color scale = new Color(0, 0, 0, 1/20f);
	Coroutine co;

	IEnumerator spawn()
	{
		// wait until active
		// randomlly spawn particle between minX and maxX
		for (float k = minX; k < maxX; k+=5)
		{
			GameObject particle = Instantiate(template, transform);
			particle.transform.localPosition = new Vector3(Random.Range(k, k+5), y,-1f);
			if (minX == -2)
				Modding.Logger.Log(particle.transform.position);
			float scale = Random.Range(.05f, .2f);
			particle.transform.SetScaleX(scale);
			particle.transform.SetScaleY(scale);
			particle.GetComponent<Rigidbody2D>().velocity = new Vector2(xScale, yScale)* Random.Range(velMin, velMax);
			StartCoroutine(die(particle));
		}

		// wait
		yield return new WaitForSeconds(1f/density);
		co = StartCoroutine(spawn());
	}
	IEnumerator die(GameObject particle)
	{
		yield return new WaitForSeconds(Random.Range(2, 5f));

		SpriteRenderer sprite = particle.GetComponent<SpriteRenderer>();
		while (sprite.color.a > 0)
		{
			sprite.color -= scale;

			yield return new WaitForSeconds(.1f);
		}
		Destroy(particle);
	}

	public void setDensity(float d)
	{
		density = d;
	}
	public void set(float min, float max)
	{
		minX = min;
		maxX = max;
		updateVelocity();
	}
	public void set(float min, float max, float newy)
	{
		minX = min;
		maxX = max;
		y = newy;
		updateVelocity();
	}
	public void set(float min, float max, float newy, float d)
	{
		minX = min;
		maxX = max;
		y = newy;
		density = d;
		updateVelocity();
	}
	private void updateVelocity()
	{
		float converted = (transform.GetRotation2D() + 90) / 180 * Mathf.PI;
		yScale = Mathf.Sin(converted);
		xScale = Mathf.Cos(converted);
		Modding.Logger.Log(xScale + " " + yScale);
	}
	public void setVelocityRange(float min, float max)
	{
		if (min > max)
		{
			velMin = max;
			velMax = min;
		}
		else
		{
			velMin = min;
			velMax = max;
		}
	}
	public void setActive(bool a)
	{
		if (!a)
		{
			prev = density;
			StopCoroutine(co);
		}
		else
		{
			if (prev == 0)
				density = 3;
			else
				density = prev;
			co = StartCoroutine(spawn());
		}

	}
}
