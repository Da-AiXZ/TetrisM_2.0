using UnityEngine;

public class BGMM : MonoBehaviour
{
	public static AudioClip C418_1;

	public static AudioClip C418_2;

	public static AudioClip C418_3;

	public static AudioClip C418_4;

	public static AudioClip C418_5;

	public static AudioClip C418_6;

	public static AudioClip C418_7;

	public static AudioSource BGM;

	public static int bgm;

	private void Start()
	{
		BGM = base.gameObject.AddComponent<AudioSource>();
		C418_1 = Resources.Load("C418_1") as AudioClip;
		C418_2 = Resources.Load("C418_2") as AudioClip;
		C418_3 = Resources.Load("C418_3") as AudioClip;
		C418_4 = Resources.Load("C418_4") as AudioClip;
		C418_5 = Resources.Load("C418_5") as AudioClip;
		C418_6 = Resources.Load("C418_6") as AudioClip;
		C418_7 = Resources.Load("C418_7") as AudioClip;
	}

	public static void FakeUpdate()
	{
		if (!BGM.isPlaying)
		{
			int num = bgm;
			while (num == bgm)
			{
				bgm = Random.Range(1, 8);
			}
			switch (bgm)
			{
			case 1:
				BGM.clip = C418_1;
				break;
			case 2:
				BGM.clip = C418_2;
				break;
			case 3:
				BGM.clip = C418_3;
				break;
			case 4:
				BGM.clip = C418_4;
				break;
			case 5:
				BGM.clip = C418_5;
				break;
			case 6:
				BGM.clip = C418_6;
				break;
			case 7:
				BGM.clip = C418_7;
				break;
			}
			BGM.Play();
		}
	}
}
