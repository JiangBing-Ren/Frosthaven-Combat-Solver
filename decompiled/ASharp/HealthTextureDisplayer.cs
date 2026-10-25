using UnityEngine;
using UnityEngine.UI;

public class HealthTextureDisplayer : MonoBehaviour
{
	[SerializeField]
	private RawImage image;

	[SerializeField]
	private int totalDivisions = 5;

	[Header("Size")]
	[SerializeField]
	private int width = 256;

	[SerializeField]
	private int height = 64;

	[Header("Borders")]
	[SerializeField]
	private int divisionBorder = 2;

	[SerializeField]
	private int topBorder = 2;

	[Header("Main Division")]
	[SerializeField]
	private int mainDivisionGroup = 5;

	[SerializeField]
	private Color mainDivisionColor = Color.black;

	[Header("Subdivisions")]
	[SerializeField]
	private int subdivisionHeight = 8;

	[SerializeField]
	private Color subdivisionColor = Color.black;

	[Space]
	public int algorithmDebug = 1;

	private Texture2D texture;

	private static readonly Color TRANSPARENT = new Color(0f, 0f, 0f, 0f);

	[ContextMenu("Resize")]
	private void Awake()
	{
		texture = new Texture2D(width, height, TextureFormat.ARGB32, mipChain: false);
		image.texture = texture;
	}

	public void SetTotalDivisions(int divisions, float percentSubdivisions)
	{
		totalDivisions = divisions;
		subdivisionHeight = ((percentSubdivisions == 1f) ? height : Mathf.CeilToInt((float)height * percentSubdivisions));
		UpdateTexture();
	}

	[ContextMenu("Update Texture")]
	private void UpdateTexture()
	{
		if (algorithmDebug == 1)
		{
			UpdateTexture1();
		}
		else
		{
			UpdateTexture2();
		}
	}

	private void Clear()
	{
		for (int i = 0; i < texture.width; i++)
		{
			for (int j = 0; j < texture.height; j++)
			{
				texture.SetPixel(i, j, (texture.height - j <= topBorder) ? mainDivisionColor : TRANSPARENT);
			}
		}
	}

	private void UpdateTexture2()
	{
		Clear();
		float num = (float)texture.width / (float)totalDivisions;
		float num2 = (float)divisionBorder / 2f;
		for (int i = 0; i < totalDivisions; i++)
		{
			for (int j = (int)Mathf.Max(0f, num * (float)i - num2); j < (int)Mathf.Min(texture.width, num * (float)i + num2); j++)
			{
				if (i % mainDivisionGroup == 0)
				{
					for (int k = 0; k < texture.height - topBorder; k++)
					{
						texture.SetPixel(j, k, mainDivisionColor);
					}
					continue;
				}
				for (int num3 = texture.height - topBorder; num3 >= texture.height - subdivisionHeight; num3--)
				{
					texture.SetPixel(j, num3, subdivisionColor);
				}
			}
		}
		texture.filterMode = FilterMode.Bilinear;
		texture.Apply();
	}

	private void UpdateTexture1()
	{
		float num = (float)texture.width / (float)totalDivisions;
		float num2 = (float)divisionBorder / 2f;
		int num3 = 0;
		float num4 = 0f;
		float num5 = num2;
		bool flag = false;
		for (int i = 0; i < texture.width; i++)
		{
			if ((float)i <= num5 && (float)i >= num4)
			{
				flag = true;
				if (num3 % mainDivisionGroup == 0)
				{
					for (int j = 0; j < texture.height; j++)
					{
						texture.SetPixel(i, j, mainDivisionColor);
					}
					continue;
				}
				for (int k = 0; k < texture.height; k++)
				{
					if (texture.height - k <= topBorder)
					{
						texture.SetPixel(i, k, mainDivisionColor);
					}
					else
					{
						texture.SetPixel(i, k, (texture.height - k <= subdivisionHeight) ? subdivisionColor : TRANSPARENT);
					}
				}
			}
			else
			{
				if (flag)
				{
					flag = false;
					num3++;
					num4 = (float)num3 * num - num2;
					num5 = (float)num3 * num + num2;
				}
				for (int l = 0; l < texture.height; l++)
				{
					texture.SetPixel(i, l, (texture.height - l <= topBorder) ? mainDivisionColor : TRANSPARENT);
				}
			}
		}
		texture.filterMode = FilterMode.Bilinear;
		texture.Apply();
	}

	private void OnDestroy()
	{
		Object.Destroy(texture);
	}
}
