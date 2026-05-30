using UnityEngine;

public class ironMan : MonoBehaviour
{
	public SpriteRenderer sprite;

	private void Start()
	{
		sprite.sprite = Resources.LoadAll<Sprite>("IronMan")[Random.Range(0, 6)];
		MySystem.sound.PlayOneShot(Resources.Load("IronMan_voice") as AudioClip);
	}

	private void FixedUpdate()
	{
		if (base.transform.position.y < -10f)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
