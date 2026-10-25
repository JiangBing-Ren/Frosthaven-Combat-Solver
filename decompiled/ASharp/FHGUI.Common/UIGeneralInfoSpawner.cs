using System.Linq;
using TMPro;
using UnityEngine;

namespace FHGUI.Common;

public class UIGeneralInfoSpawner : UIGeneralInfoBase, IUIGeneralInfo<UIGeneralInfoSpawner.SpawnerGeneralInfoData>, IUIGeneralInfo
{
	public struct SpawnerGeneralInfoData : IGeneralInfoData
	{
		public PropImage Image;

		public SpawnerDTO[] Spawners;
	}

	[SerializeField]
	private TMP_Text _name;

	[SerializeField]
	private UIGeneralInfoSection _monster;

	[SerializeField]
	private UIAsyncImage _image;

	public bool Show(SpawnerGeneralInfoData data, params object[] parameters)
	{
		gameObject.SetActive(value: true);
		SpawnerDTO spawner = data.Spawners.First();
		_name.text = spawner.Name;
		_monster.Label.Text.text = spawner.MonsterData?.Name;
		_monster.Fill(spawner.MonsterData.HasValue, (UIBaseStatsView view) =>
		{
			view.Init(spawner.MonsterData.Value.Stats);
		});
		_image.enabled = spawner.MonsterData.HasValue;
		_image.Init(data.Image?.Sprite, data.Image?.FallbackSprite);
		return false;
	}
}
