using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fade : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
		Fade();
	}

    // Update is called once per frame
    void Update()
    {
        
    }

	private void Fade()
	{
		IEnumerator fade()
		{
			SpriteRenderer[] sprites = { GameObject.Find("Terrain/Area1/Floor/Sprite").GetComponent<SpriteRenderer>(), GameObject.Find("Terrain/Area1/LeftWall/Sprite").GetComponent<SpriteRenderer>(), GameObject.Find("Terrain/Area1/RightWall/Sprite").GetComponent<SpriteRenderer>() };
			float c = .04f;
			Color scale = new Color(c, c, c, 0);
			for (int k = 0; k < 6; k++)
			{
				// fade to black
				while (sprites[0].color.r > 0)
				{
					sprites[0].color -= scale;
					sprites[1].color -= scale;
					sprites[2].color -= scale;

					yield return new WaitForSeconds(.01f);
				}
				// fade back
				if (k != 5)
					while (sprites[0].color.r < 1)
				{
					sprites[0].color += scale;
					sprites[1].color += scale;
					sprites[2].color += scale;

					yield return new WaitForSeconds(.01f);
				}
				yield return new WaitForSeconds(.3f);
			}
		}
		StartCoroutine(fade());
	}
}
