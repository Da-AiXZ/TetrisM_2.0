using UnityEngine;

public class SnowMan : MonoBehaviour
{
	public SpriteRenderer sprite;

	private void Start()
	{
		sprite.sprite = Resources.LoadAll<Sprite>("SnowMan")[Random.Range(0, 18)];
		MySystem.sound.PlayOneShot(Resources.Load("SnowMan_voice") as AudioClip);
	}

	private void FixedUpdate()
	{
		if (base.transform.position.y < -10f)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
