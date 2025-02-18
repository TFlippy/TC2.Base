
namespace TC2.Base.Components
{
	public static partial class RandomActivation
	{
		[Flags]
		public enum Flags: uint
		{
			None = 0,

			Active = 1 << 0
		}

		[IComponent.Data(Net.SendType.Reliable, name: "Random Activation", region_only: true)]
		public partial struct Data(): IComponent
		{
			[Statistics.Info("Duration", description: "Duration of a random activation.", format: "{0:0.##}s", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Low)]
			public float duration;

			[Statistics.Info("Chance", description: "Chance of a random activation being triggered.", format: "{0:P2}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Low)]
			public float chance = 0.005f;

			public RandomActivation.Flags flags;

			[Save.Ignore, Net.Ignore] public float time_stop;
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region, interval: 0.10f), HasRelation(Source.Modifier.Owned, Relation.Type.Stored, false)]
		public static void Update(ISystem.Info info, ref XorRandom random, Entity entity,
		[Source.Owned] ref Control.Data control, [Source.Owned] ref RandomActivation.Data random_activation)
		{
			if (random_activation.flags.HasAny(RandomActivation.Flags.Active))
			{
				control.mouse.SetKeyPressed(Mouse.Key.Left, true);
				control.keyboard.SetKeyPressed(Keyboard.Key.Spacebar, true);

#if SERVER
				if (info.WorldTime >= random_activation.time_stop)
				{
					random_activation.flags.RemoveFlag(RandomActivation.Flags.Active);
					random_activation.Sync(entity);
				}
#endif
			}
			else
			{
#if SERVER
				if (random.NextBool(random_activation.chance))
				{
					random_activation.time_stop = info.WorldTime + random_activation.duration;
					random_activation.flags.AddFlag(RandomActivation.Flags.Active);
					random_activation.Sync(entity);
				}
#endif
			}
		}
	}
}
