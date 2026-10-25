namespace FHGUI.DTO;

public struct RangedValueDTO
{
	public int Min;

	public int Value;

	public int Max;

	public float Progress
	{
		get
		{
			if ((float)(Max - Min) != 0f)
			{
				return (float)(Value - Min) / (float)(Max - Min);
			}
			return 1f;
		}
	}

	public string ToDisplayString()
	{
		return $"{Value}/{Max}";
	}

	public LocalizationParam[] ToLocalizedParams()
	{
		return new LocalizationParam[2]
		{
			new LocalizationParam("CURRENT", Value),
			new LocalizationParam("MAX", Max)
		};
	}

	public RangedValueDTO(int value, int max)
	{
		Min = 0;
		Value = value;
		Max = max;
	}

	public RangedValueDTO(int value, int min, int max)
	{
		Min = min;
		Value = value;
		Max = max;
	}
}
