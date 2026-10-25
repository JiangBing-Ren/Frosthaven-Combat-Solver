using System.Collections.Generic;
using System.Linq;

namespace ScenarioRuleLibrary;

public class CardLayoutGroup
{
	public enum GroupTypes
	{
		Row,
		Column,
		Consume,
		Element,
		Summon,
		Augment,
		Duration,
		XP,
		Song,
		Doom,
		Divider,
		MandatoryTrigger,
		Conditional,
		BlinkbladeState,
		AddedEffect
	}

	public enum DataTypes
	{
		None,
		RowCollection,
		ColumnCollection,
		RowData,
		Consume,
		Element,
		Summon,
		Augment,
		Duration,
		XP,
		Song,
		Doom,
		Divider,
		MandatoryTrigger,
		Conditional,
		BlinkbladeState,
		AddedEffect
	}

	public static class LayoutColorProperty
	{
		public const string None = "";

		public const string Inherit = "inherit";

		public const string Class = "class";

		public const string ClassAlt = "alt";
	}

	public class ColumnAttributes
	{
		public int[] ColumnWidths { get; private set; }

		public int ColumnSpacing { get; private set; }

		public ColumnAttributes()
		{
			ColumnSpacing = 0;
			ColumnWidths = new int[0];
		}

		public ColumnAttributes(int[] colWidths, int colSpacing)
		{
			ColumnWidths = colWidths;
			ColumnSpacing = colSpacing;
		}
	}

	public class RowAttributes
	{
		public bool AutoSize { get; set; }

		public int AutoSizeMin { get; set; }

		public int AutoSizeMax { get; set; }

		public int CharSpacing { get; set; }

		public int WordSpacing { get; set; }

		public int LineSpacing { get; set; }

		public int ParSpacing { get; set; }

		public string Alignment { get; set; }

		public bool Wrapping { get; set; }

		public string Overflow { get; set; }

		public RowAttributes(bool autosize = false, int autosizemin = 0, int autosizemax = 0, int charspacing = 0, int wordspacing = 0, int linespacing = 0, int parspacing = 0, string alignment = "Center", bool wrapping = true, string overflow = "Overflow")
		{
			AutoSize = autosize;
			AutoSizeMin = autosizemin;
			AutoSizeMax = autosizemax;
			CharSpacing = charspacing;
			WordSpacing = wordspacing;
			LineSpacing = linespacing;
			ParSpacing = parspacing;
			Alignment = alignment;
			Wrapping = wrapping;
			Overflow = overflow;
		}
	}

	public class SummonLayout
	{
		public CAbility SummonAbility { get; private set; }

		public string SummonName { get; private set; }

		public CardLayout SpecialText { get; private set; }

		public bool HideSummonBoxes { get; private set; }

		public SummonLayout(CAbility summonAbility, string summonName, CardLayout specialText, bool hideSummonBoxes)
		{
			SummonAbility = summonAbility;
			SummonName = summonName;
			SpecialText = specialText;
			HideSummonBoxes = hideSummonBoxes;
		}
	}

	public class AugmentLayout
	{
		public string AugmentIcon { get; private set; }

		public string DiscardText { get; private set; }

		public CardLayout AugmentAbilityLayout { get; private set; }

		public CardLayout NormalContentLayout { get; private set; }

		public AugmentLayout(string augmentIcon, string discardText, CardLayout augmentAbilityLayout, CardLayout normalContentLayout)
		{
			AugmentIcon = augmentIcon;
			DiscardText = discardText;
			AugmentAbilityLayout = augmentAbilityLayout;
			NormalContentLayout = normalContentLayout;
		}
	}

	public class DoomLayout
	{
		public CardLayout DoomAbilityLayout { get; private set; }

		public string ReminderTextOverride { get; private set; }

		public string DoomIcon { get; private set; }

		public string DiscardText { get; private set; }

		public DoomLayout(string doomIcon, string discardText, CardLayout doomAbilityLayout, string reminderTextOverride)
		{
			DoomAbilityLayout = doomAbilityLayout;
			ReminderTextOverride = reminderTextOverride;
			DoomIcon = doomIcon;
			DiscardText = discardText;
		}
	}

	public class SongLayout
	{
		public string SongIcon { get; private set; }

		public string DiscardText { get; private set; }

		public CardLayout SongAbilityLayout { get; private set; }

		public SongLayout(string songIcon, string discardText, CardLayout songAbilityLayout)
		{
			SongIcon = songIcon;
			DiscardText = discardText;
			SongAbilityLayout = songAbilityLayout;
		}
	}

	public class MandatoryTriggerLayout
	{
		public CardLayout Layout { get; private set; }

		public MandatoryTriggerLayout(CardLayout triggerLayout)
		{
			Layout = triggerLayout;
		}
	}

	public class ConditionalElementLayout
	{
		public CardLayout Layout { get; private set; }

		public bool HasTop { get; private set; }

		public bool Show { get; private set; }

		public ConditionalElementLayout(CardLayout triggerLayout, bool hasTop, bool show)
		{
			Layout = triggerLayout;
			HasTop = hasTop;
			Show = show;
		}
	}

	public class BlinkbladeStateLayout
	{
		public CardLayout Layout { get; private set; }

		public bool IsFast { get; private set; }

		public float Width { get; private set; }

		public bool IsPercent { get; private set; }

		public BlinkbladeStateLayout(CardLayout layout, bool isFast, float width, bool isPercent)
		{
			Layout = layout;
			IsFast = isFast;
			Width = width;
			IsPercent = isPercent;
		}
	}

	public struct CardLayoutGroupOverrides
	{
		public RowAttributes RowAtt;

		public ColumnAttributes ColAtt;
	}

	public GroupTypes GroupType { get; private set; }

	public DataTypes DataType { get; private set; }

	public CardLayoutGroup Parent { get; private set; }

	public List<CardLayoutGroup> Collection { get; private set; }

	public CardLayoutRow Data { get; private set; }

	public AbilityConsume Consume { get; private set; }

	public SummonLayout Summon { get; private set; }

	public AugmentLayout Augment { get; private set; }

	public DoomLayout Doom { get; private set; }

	public SongLayout Song { get; private set; }

	public MandatoryTriggerLayout MandatoryTrigger { get; private set; }

	public BlinkbladeStateLayout BlinkbladeState { get; private set; }

	public ConditionalElementLayout ConditionalElement { get; private set; }

	public List<ElementInfusionBoardManager.EElement> Elements { get; private set; }

	public List<int> XP { get; private set; }

	public string AddedEffect { get; private set; }

	public CActiveBonus.EActiveBonusDurationType Duration { get; private set; }

	public ColumnAttributes ColAtt { get; private set; }

	public RowAttributes RowAtt { get; private set; }

	public string Color { get; private set; }

	public void CreateRowCollection(List<CardLayoutGroup> collection)
	{
		DataType = DataTypes.RowCollection;
		Collection = collection;
	}

	public void CreateColumnCollection(List<CardLayoutGroup> collection, ColumnAttributes columnAttributes)
	{
		DataType = DataTypes.ColumnCollection;
		Collection = collection;
		ColAtt = columnAttributes;
	}

	public void CreateRowData(CardLayoutRow rowData, RowAttributes rowAttributes)
	{
		DataType = DataTypes.RowData;
		Data = rowData;
		RowAtt = rowAttributes;
	}

	public void CreateConsumeData(AbilityConsume consume)
	{
		DataType = DataTypes.Consume;
		Consume = consume;
	}

	public void CreateElementData(List<ElementInfusionBoardManager.EElement> elements)
	{
		DataType = DataTypes.Element;
		Elements = elements;
	}

	public void CreateSummonData(SummonLayout summonLayout)
	{
		DataType = DataTypes.Summon;
		Summon = summonLayout;
	}

	public void CreateAugmentData(AugmentLayout augmentLayout)
	{
		DataType = DataTypes.Augment;
		Augment = augmentLayout;
	}

	public void CreateDoomData(DoomLayout doomLayout)
	{
		DataType = DataTypes.Doom;
		Doom = doomLayout;
	}

	public void CreateSongData(SongLayout songLayout)
	{
		DataType = DataTypes.Song;
		Song = songLayout;
	}

	public void CreateDurationData(CActiveBonus.EActiveBonusDurationType duration)
	{
		DataType = DataTypes.Duration;
		Duration = duration;
	}

	public void CreateXPData(List<int> xp, bool showPersistentIcon)
	{
		DataType = DataTypes.XP;
		XP = xp;
		if (showPersistentIcon)
		{
			Duration = CActiveBonus.EActiveBonusDurationType.Persistent;
		}
	}

	public void CreateDivider()
	{
		DataType = DataTypes.Divider;
	}

	public void CreateMandatoryTrigger(MandatoryTriggerLayout mandatoryTriggerLayout)
	{
		DataType = DataTypes.MandatoryTrigger;
		MandatoryTrigger = mandatoryTriggerLayout;
	}

	public void CreateConditionalElement(ConditionalElementLayout conditionalElementLayout)
	{
		DataType = DataTypes.Conditional;
		ConditionalElement = conditionalElementLayout;
	}

	public void CreateBlinkbladeStateElement(BlinkbladeStateLayout blinkbladeStateLayout)
	{
		DataType = DataTypes.BlinkbladeState;
		BlinkbladeState = blinkbladeStateLayout;
	}

	public void CreateAddedEffect(string addedEffect)
	{
		DataType = DataTypes.AddedEffect;
		AddedEffect = addedEffect;
	}

	public void SetColor(string color)
	{
		Color = color;
	}

	public CardLayoutGroup(GroupTypes groupTypes, CardLayoutGroup parent = null, string color = "")
	{
		GroupType = groupTypes;
		Parent = parent;
		Color = color;
	}

	public CardLayoutGroup(CardLayoutGroup group)
	{
		GroupType = group.GroupType;
		DataType = group.DataType;
		Collection = group.Collection;
		RowAtt = group.RowAtt;
		ColAtt = group.ColAtt;
		Data = group.Data;
		Color = group.Color;
	}

	public CardLayoutGroup(CardLayoutGroup group, CardLayoutGroupOverrides groupOverrides)
	{
		GroupType = group.GroupType;
		DataType = group.DataType;
		Collection = group.Collection?.Select((CardLayoutGroup c) => new CardLayoutGroup(c, groupOverrides)).ToList();
		RowAtt = groupOverrides.RowAtt ?? group.RowAtt;
		ColAtt = groupOverrides.ColAtt ?? group.ColAtt;
		Data = group.Data;
		Color = group.Color;
		Augment = group.Augment;
		Consume = group.Consume;
		Doom = group.Doom;
		Duration = group.Duration;
		Elements = group.Elements;
		Song = group.Song;
		Summon = group.Summon;
		AddedEffect = group.AddedEffect;
		BlinkbladeState = group.BlinkbladeState;
		ConditionalElement = group.ConditionalElement;
		MandatoryTrigger = group.MandatoryTrigger;
		XP = group.XP;
	}
}
