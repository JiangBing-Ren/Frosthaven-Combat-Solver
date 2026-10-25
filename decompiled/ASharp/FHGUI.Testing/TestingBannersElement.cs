using FHGUI.DTO;
using FHGUI.Elements;
using TMPro;
using UnityEngine;

namespace FHGUI.Testing;

public class TestingBannersElement : MonoBehaviour
{
	public TMP_Text NormalName;

	public UIOutpostMonsterBanner Normal;

	public TMP_Text EliteName;

	public UIOutpostMonsterBanner Elite;

	public void Init(MonsterDataDto normalDto, MonsterDataDto eliteDto)
	{
		SetDto(Normal, NormalName, normalDto);
		SetDto(Elite, EliteName, eliteDto);
	}

	private static void SetDto(UIOutpostMonsterBanner banner, TMP_Text text, MonsterDataDto dto)
	{
		bool flag = dto != null;
		banner.gameObject.SetActive(flag);
		text.gameObject.SetActive(flag);
		if (flag)
		{
			banner.Set(dto);
			text.SetText(dto.NameLocalised + "\n(" + dto.Portrait.name + ")");
		}
	}
}
