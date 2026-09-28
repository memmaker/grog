using System;
using Grog.Dressings;
using Grog.Dungeons;
using Grog.Dungeons.Generators.Rooms;
using Grog.Dungeons.Generators.Rooms.SpecialMessages;

namespace Grog.Systems.SpecialRooms.Modifiers;

public class FeatureRoomModifier : RoomModifierBase
{
	[Serializable]
	public class DisplaySpecialMessageIfFeatureTypeExists : ISpecialMessageProvider
	{
		private readonly Type _featureType;

		private readonly string _message;

		public DisplaySpecialMessageIfFeatureTypeExists(Type featureType, string message)
		{
			_featureType = featureType;
			_message = message;
		}

		public string GetMessageFor(DungeonLevel dungeonLevel, IRoom room)
		{
			foreach (Position insidePosition in room.GetInsidePositions())
			{
				Feature featureAt = dungeonLevel.GetFeatureAt(insidePosition.X, insidePosition.Y);
				if (featureAt != null && featureAt.GetType() == _featureType)
				{
					return _message;
				}
			}
			return null;
		}
	}

	private readonly Type _featureType;

	private readonly string _specialMessage;

	private readonly Func<Feature> _createFeature;

	public FeatureRoomModifier(int minimumLevel, int rarity, Type featureType, string specialMessage, Func<Feature> createFeature)
		: base(minimumLevel, rarity)
	{
		_featureType = featureType;
		_specialMessage = specialMessage;
		_createFeature = createFeature;
	}

	public override bool Modify(DungeonLevel dungeonLevel, IRoom room)
	{
		Position insidePositionForFeature = room.GetInsidePositionForFeature(dungeonLevel);
		if (!insidePositionForFeature.Equals(Position.Undefined))
		{
			room.SpecialMessage = new DisplaySpecialMessageIfFeatureTypeExists(_featureType, _specialMessage);
			dungeonLevel.SetFeatureAt(insidePositionForFeature, _createFeature());
			return true;
		}
		return false;
	}
}
