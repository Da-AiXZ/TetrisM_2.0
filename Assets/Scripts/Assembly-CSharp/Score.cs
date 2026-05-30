using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
	private Text scoreT;

	private Text bestScoreT;

	private int score;

	public static int bestScore;

	private string scoreS;

	private string bestScoreS;

	public static float beLing;

	public void _Reset()
	{
		score = 0;
	}

	private void Start()
	{
		scoreT = GameObject.Find("Score").GetComponent<Text>();
		bestScoreT = GameObject.Find("Best").GetComponent<Text>();
	}

	private void FixedUpdate()
	{
		if (score < MySystem.score)
		{
			score++;
		}
		if (score > MySystem.score)
		{
			score = MySystem.score;
		}
		bestScore = MySystem.bestScore;
		scoreS = score.ToString();
		if (scoreS.Length < 8)
		{
			int num = 8 - scoreS.Length;
			for (int i = 1; i <= num; i++)
			{
				scoreS = "0" + scoreS;
			}
		}
		if (beLing <= 0f)
		{
			scoreT.text = scoreS;
		}
		else if ((double)beLing > 0.6)
		{
			scoreT.text = "";
			beLing -= Time.deltaTime;
		}
		else if ((double)beLing > 0.3)
		{
			scoreT.text = scoreS;
			beLing -= Time.deltaTime;
		}
		else if (beLing > 0f)
		{
			scoreT.text = "";
			score = MySystem.score;
			beLing -= Time.deltaTime;
		}
		bestScoreS = bestScore.ToString();
		if (bestScoreS.Length < 8)
		{
			int num2 = 8 - bestScoreS.Length;
			for (int j = 1; j <= num2; j++)
			{
				bestScoreS = "0" + bestScoreS;
			}
		}
		bestScoreT.text = bestScoreS;
	}
}
