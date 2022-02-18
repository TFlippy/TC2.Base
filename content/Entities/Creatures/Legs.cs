
namespace TC2.Base.Components
{
	public static partial class Legs
	{
		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct Data: IComponent
		{
			public float sound_interval_multiplier = 1.00f;

			public float sound_volume = 0.25f;
			public float sound_pitch = 1.00f;

			[Net.Ignore, Save.Ignore] public float next_step;
		
			public uint frame_count = 4;
			public uint fps = 12;
		}

		[ISystem.Update(ISystem.Mode.Single), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateNoRotateAlive(ISystem.Info info, [Source.Owned] in Organic.Data organic, [Source.Owned] in Organic.State organic_state, [Source.Owned] ref NoRotate.Data no_rotate, [Source.Owned] in Legs.Data legs)
		{
			no_rotate.multiplier = MathF.Round(organic_state.consciousness_shared) * organic_state.efficiency;
		}

		[ISystem.Update(ISystem.Mode.Single), HasTag("dead", true, Source.Modifier.Owned)]
		public static void UpdateNoRotateDead(ISystem.Info info, [Source.Owned] in Organic.Data organic, [Source.Owned] in Organic.State organic_state, [Source.Owned] ref NoRotate.Data no_rotate, [Source.Owned] in Legs.Data legs)
		{
			no_rotate.multiplier = 0.00f;
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
		public static void UpdateAnimation(ISystem.Info info, Entity entity,
		[Source.Owned] in Organic.Data organic, [Source.Owned] in Organic.State organic_state, 
		[Source.Owned] ref Legs.Data legs, [Source.Owned] in Runner.Data runner, [Source.Owned, Optional(true)] ref HeadBob.Data headbob,
		[Source.Owned] ref Animated.Renderer.Data renderer, [Source.Owned] in Control.Data control, [Source.Owned] in Transform.Data transform)
		{
			var bob_amplitude = Vector2.Zero;
			var bob_speed = 0.00f;

			if (!headbob.IsNull())
			{
				bob_amplitude = headbob.multiplier;
				bob_speed = headbob.speed;
			}

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
				renderer.sprite.fps = (byte)Math.Round(legs.fps * (0.30f + ((0.70f * organic_state.efficiency) * (runner.flags.HasAll(Runner.Flags.Crouching) ? runner.crouch_speed_modifier : 1.00f))));
				renderer.sprite.frame.X = 1;
				renderer.sprite.count = legs.frame_count;

				var offset = runner.flags.HasAll(Runner.Flags.Grounded) ? new Vector2(-bob_amplitude.X * (MathF.Sin(info.WorldTime * bob_speed)), -bob_amplitude.Y * ((MathF.Sin(info.WorldTime * bob_speed) + 1.00f) * 0.50f)) : Vector2.Zero;
				if (!headbob.IsNull())
				{
					headbob.offset = Vector2.Lerp(headbob.offset, offset, 0.50f);
				}

				if (renderer.sprite.fps > 0 && runner.flags.HasAll(Runner.Flags.Grounded))
				{
					if (info.WorldTime >= legs.next_step)
					{
						var interval = ((1.00f / renderer.sprite.fps) * renderer.sprite.count) * legs.sound_interval_multiplier;
						legs.next_step = info.WorldTime + interval;

						var random = XorRandom.New();
						Sound.Play(Legs.walk_sounds.GetRandom(), transform.position, volume: legs.sound_volume, pitch: random.NextFloatRange(0.98f, 1.02f) * legs.sound_pitch, priority: 0.10f);
					}
				}

				return;
			}

			idle:
			{
				var offset = Vector2.Zero;

				if (!headbob.IsNull())
				{
					headbob.offset = Vector2.Lerp(headbob.offset, offset, 0.50f);
				}

				renderer.sprite.fps = 0;
				renderer.sprite.frame.X = 0;
				renderer.sprite.count = 0;

				return;
			}

			dead:
			{
				var offset = Vector2.Zero;

				if (!headbob.IsNull())
				{
					headbob.offset = offset;
				}

				renderer.sprite.fps = 0;
				renderer.sprite.frame.X = 0;
				renderer.sprite.count = 0;

				return;
			}

			air:
			{
				renderer.sprite.fps = 0;
				renderer.sprite.frame.X = (uint)(5 + Maths.Clamp(runner.air_time * legs.fps, 0, 2));
				renderer.sprite.count = 0;

				return;
			}
		}

#endif
	}
}
