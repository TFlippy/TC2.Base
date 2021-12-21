
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

		[IComponent.Data(Net.SendType.Reliable)]
		public partial struct Data: IComponent
		{
			[Statistics.Info("Random Activation Duration", description: "Duration of a random activation.", format: "{0:0.##}s", comparison: Statistics.Comparison.Higher)]
			public float duration;

			[Statistics.Info("Random Activation Chance", description: "Chance of a random activation being triggered.", format: "{0:P2}", comparison: Statistics.Comparison.Higher)]
			public float chance = 0.005f;

			public RandomActivation.Flags flags;

			[Save.Ignore, Net.Ignore] public float time_stop;
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single), Exclude<Toolbelt.Data>(Source.Modifier.Parent)]
		public static void Update(ISystem.Info info, Entity entity,
		[Source.Owned] ref Control.Data control, [Source.Owned] ref RandomActivation.Data random_activation)
		{
			if (random_activation.flags.HasAll(RandomActivation.Flags.Active))
			{
				control.mouse.SetKeyPressed(Mouse.Key.Left, true);
				control.keyboard.SetKeyPressed(Keyboard.Key.Spacebar, true);

#if SERVER
				if (info.WorldTime >= random_activation.time_stop)
				{
					random_activation.flags.SetFlag(RandomActivation.Flags.Active, false);
					random_activation.Sync(entity);
				}
#endif
			}
			else
			{
#if SERVER
				var random = XorRandom.New();
				if (random.NextBool(random_activation.chance))
				{
					random_activation.time_stop = info.WorldTime + random_activation.duration;
					random_activation.flags.SetFlag(RandomActivation.Flags.Active, true);
					random_activation.Sync(entity);
				}
#endif
			}
		}
	}
}
