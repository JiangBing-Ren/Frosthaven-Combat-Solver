using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "UI Config/Story Character")]
public class StoryCharacterConfigUI : ScriptableObject
{
	[Serializable]
	public class ExpressionConfig
	{
		public EExpression Expression;

		public Sprite SpriteForExpression;
	}

	public string CharacterGUID;

	public string CharacterDisplayNameKey;

	public Sprite shieldIcon;

	public List<ExpressionConfig> AvailableExpressions = new List<ExpressionConfig>();

	public Sprite GetExpression(EExpression expression)
	{
		return AvailableExpressions.FirstOrDefault((ExpressionConfig c) => expression == c.Expression)?.SpriteForExpression;
	}
}
