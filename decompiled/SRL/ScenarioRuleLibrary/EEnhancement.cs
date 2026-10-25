using System;
using FH.Tools;

namespace ScenarioRuleLibrary;

[Serializable]
[KronosEnumPolice(1)]
public enum EEnhancement
{
	NoEnhancement,
	Poison,
	Wound,
	Muddle,
	Immobilize,
	Disarm,
	Curse,
	Strengthen,
	Bless,
	Jump,
	Fire,
	Ice,
	Air,
	Earth,
	Light,
	Dark,
	AnyElement,
	Area,
	SummonMove,
	SummonAttack,
	SummonRange,
	SummonHP,
	PlusMove,
	PlusAttack,
	PlusRange,
	PlusShield,
	PlusPush,
	PlusPull,
	PlusPierce,
	PlusRetaliate,
	PlusHeal,
	PlusTarget,
	PlusRetaliateRange,
	PlusDamage,
	Ward,
	Regenerate,
	PlusTeleport
}
