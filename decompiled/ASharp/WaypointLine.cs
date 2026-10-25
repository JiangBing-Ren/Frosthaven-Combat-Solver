using System.Collections;
using System.Linq;
using Chronos;
using SETUtil.Materials;
using UnityEngine;

public class WaypointLine : MonoBehaviour
{
	public static WaypointLine s_Instance;

	public bool m_UpdateFlag;

	public bool m_UsePulse;

	public Transform m_Transform;

	private bool m_Pulse = true;

	private Vector3[] m_Waypoints;

	private LineRenderer m_LineRend;

	private Material m_LineMat;

	private void Awake()
	{
		s_Instance = this;
		m_Transform = transform;
	}

	private void Start()
	{
		GetDependencies();
		m_UpdateFlag = true;
		if (m_UsePulse)
		{
			StartCoroutine(PulseTimeline());
		}
	}

	private void GetDependencies()
	{
		if (m_LineRend == null)
		{
			m_LineRend = GetComponent<LineRenderer>();
			m_LineMat = m_LineRend.GetMaterialClonesSlow()[0];
		}
	}

	private void Update()
	{
		if (m_UpdateFlag)
		{
			Transform[] array = (from Transform c in transform
				where c.gameObject.tag == "Waypoint"
				select c).ToArray();
			m_Waypoints = new Vector3[array.Length];
			for (int num = 0; num < array.Length; num++)
			{
				m_Waypoints[num] = array[num].localPosition;
			}
			m_LineRend.positionCount = m_Waypoints.Length;
			m_LineRend.SetPositions(m_Waypoints);
			m_UpdateFlag = false;
		}
	}

	private IEnumerator PulseTimeline()
	{
		while (m_UsePulse)
		{
			yield return Timekeeper.instance.WaitForSeconds(Random.Range(3, 9));
			float pulseTime = Random.Range(1.2f, 1.9f);
			float dTime = 0f;
			float startTime = Timekeeper.instance.m_GlobalClock.time;
			while (Timekeeper.instance.m_GlobalClock.time - startTime < pulseTime)
			{
				if (dTime < pulseTime)
				{
					dTime += Timekeeper.instance.m_GlobalClock.deltaTime;
					m_LineMat.SetFloat("_Pulse", dTime / pulseTime);
				}
				yield return new WaitForEndOfFrame();
			}
		}
	}
}
