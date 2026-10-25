using Chronos;

namespace UnityEngine.UI;

public class AutoScroller : MonoBehaviour
{
	[SerializeField]
	private Scrollbar scrollbar;

	[SerializeField]
	private float speed = 1f;

	private void Start()
	{
		enabled = false;
	}

	private void Update()
	{
		if (scrollbar.value < 1f)
		{
			scrollbar.value += speed * Timekeeper.instance.m_GlobalClock.deltaTime;
		}
		else
		{
			StopScrolling();
		}
	}

	public void StartScrolling()
	{
		scrollbar.value = 0f;
		enabled = true;
	}

	private void StopScrolling()
	{
		enabled = false;
	}
}
