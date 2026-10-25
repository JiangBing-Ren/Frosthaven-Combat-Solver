using System;
using System.Collections.Generic;
using System.Linq;

namespace FH.LevelComponents.ProcGen;

public class AmbienceDiscriminator
{
	public class Differentiation
	{
		public readonly RenderingLayer Layer;

		public readonly Ambience ReusedAmbience;

		public bool DisableAmbience
		{
			get
			{
				if (!ReusedAmbience || ReusedAmbience.Visibility != ProceduralMapTile.Visibility.Revealed)
				{
					return Layer == UnrevealedLayer;
				}
				return true;
			}
		}

		public Differentiation(int layer)
		{
			if (layer == 0)
			{
				layer = 3;
			}
			Layer = new RenderingLayer(layer);
		}

		public Differentiation(RenderingLayer layer, Ambience reusedAmbience)
		{
			if (layer.Equals(RenderingLayer.NOTHING))
			{
				layer = RenderingLayer.EVERYTHING;
			}
			Layer = layer;
			ReusedAmbience = reusedAmbience;
		}
	}

	private const int _startingLayerOffset = 1;

	private const int _sizeLimit = 7;

	public static readonly RenderingLayer UnrevealedLayer = new RenderingLayer(1);

	private List<(Ambience Ambience, RenderingLayer Layer)> _records = new List<(Ambience, RenderingLayer)>(7);

	public Differentiation GetLayerDifferentiationFor(Ambience ambience)
	{
		if (ambience == null)
		{
			throw new ArgumentNullException("ambience", "Ambience cannot be null.");
		}
		if (_records.Any(((Ambience Ambience, RenderingLayer Layer) tuple) => tuple.Ambience == null))
		{
			_records.RemoveAll(((Ambience Ambience, RenderingLayer Layer) tuple) => tuple.Ambience == null);
		}
		int num = _records.FindIndex(((Ambience Ambience, RenderingLayer Layer) tuple) => Ambience.Same(tuple.Ambience, ambience));
		if (num >= 0)
		{
			Ambience item = _records[num].Ambience;
			return new Differentiation(_records[num].Layer, (item == ambience) ? null : item);
		}
		for (int num2 = 0; num2 < 7; num2++)
		{
			if (num2 < _records.Count)
			{
				if (_records[num2].Ambience == null)
				{
					RenderingLayer renderingLayer = new RenderingLayer(1 << 1 + num2);
					_records[num2] = (ambience, renderingLayer);
					return new Differentiation(renderingLayer, null);
				}
			}
			else if (_records.Count < 7)
			{
				RenderingLayer renderingLayer2 = new RenderingLayer(1 << 1 + num2);
				_records.Add((ambience, renderingLayer2));
				return new Differentiation(renderingLayer2, null);
			}
		}
		throw new IndexOutOfRangeException($"Exceeded maximum allowed layer count of {7}");
	}

	public void Clear()
	{
		_records.Clear();
	}
}
