
namespace TC2.Base.Components
{
	public static partial class Legs
	{
		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct Data: IComponent
		{
			public float multiplier;
			
			[Net.Ignore, Save.Ignore] public float next_step;
		}

		[ISystem.Update(ISystem.Mode.Single)]
		public static void UpdateNoRotate(ISystem.Info info, [Source.Owned] in Organic.Data organic, [Source.Owned] in Organic.State organic_state, [Source.Owned] ref NoRotate.Data no_rotate, [Source.Owned] in Legs.Data legs)
		{
			no_rotate.multiplier = MathF.Round(organic_state.consciousness_shared) * organic_state.efficiency;
		}

		public static readonly Sound.Handle[] walk_sounds = new Sound.Handle[]
		{
			"dirt_run_00",
			"dirt_run_01",
			"dirt_run_02",
			"dirt_run_03",
			"dirt_run_04"
		};

#if CLIENT
		[ISystem.Update(ISystem.Mode.Single)]
		public static void UpdateAnimation(ISystem.Info info, [Source.Owned] in Organic.Data organic, [Source.Owned] in Organic.State organic_state, [Source.Owned] ref Legs.Data legs, [Source.Owned] in Runner.Data runner, [Source.Owned] ref Animated.Renderer.Data renderer, [Source.Owned] in Control.Data control, [Source.Owned] in Transform.Data transform)
		{
			if (organic_state.efficiency < 0.20f) goto dead;
			else if (true) //runner.flags.HasAll(Runner.Flags.Grounded))
			{
				if (runner.flags.HasAll(Runner.Flags.Walking)) goto walking;
				else goto idle;
			}
			else
			{
				goto air;
			}

			walking:
			{
				renderer.sprite.fps = (byte)Math.Round(12 * (0.30f + ((0.70f * organic_state.efficiency) * (runner.flags.HasAll(Runner.Flags.Crouching) ? 0.50f : 1.00f))));
				renderer.sprite.frame.X = 1;
				renderer.sprite.count = 4;

				if (renderer.sprite.fps > 0 && runner.flags.HasAll(Runner.Flags.Grounded))
				{
					if (info.WorldTime >= legs.next_step)
					{
						var interval = ((1.00f / renderer.sprite.fps) * renderer.sprite.count);
						legs.next_step = info.WorldTime + interval;

						var random = XorRandom.New();
						Sound.Play(Legs.walk_sounds.GetRandom(), transform.position, volume: 0.25f, pitch: random.NextFloatRange(0.98f, 1.02f), priority: 0.10f);
					}
				}

				return;
			}

			dead:
			idle:
			{
				renderer.sprite.fps = 0;
				renderer.sprite.frame.X = 0;
				renderer.sprite.count = 0;

				return;
			}

			air:
			{
				renderer.sprite.fps = 0;
				renderer.sprite.frame.X = 2;
				renderer.sprite.count = 0;

				return;
			}
		}

#endif
	}
}
