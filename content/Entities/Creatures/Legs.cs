
namespace TC2.Base.Components
{
	public static partial class Legs
	{
		[IComponent.Data(Net.SendType.Unreliable, region_only: true)]
		public partial struct Data: IComponent
		{
			public float sound_interval_multiplier = 1.00f;
			public float sound_volume = 0.25f;
			public float sound_pitch = 1.00f;

			public uint frame_count = 4;
			public uint fps = 12;

			public uint2 frame_sitting;
			public uint2 frames_jump;

			public byte frame_air = 0;

			[Net.Ignore, Save.Ignore] public float next_step;

			public Data()
			{

			}
		}

		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateNoRotate(ISystem.Info info, [Source.Owned, Override] in Organic.Data organic, [Source.Owned] in Organic.State organic_state,
		[Source.Owned, Override] ref NoRotate.Data no_rotate, [Source.Owned] in Legs.Data legs)
		{
			var mult = (organic_state.consciousness_shared * organic_state.efficiency * Maths.Lerp(0.20f, 1.00f, organic.motorics * organic.motorics));

			//no_rotate.multiplier = MathF.Round(organic_state.consciousness_shared * Maths.Lerp(0.20f, 1.00f, organic.motorics * organic.motorics)) * organic.coordination;
			no_rotate.multiplier *= Maths.Clamp01(Maths.Clamp01(mult + 0.40f) * organic.coordination * organic.motorics) * organic_state.consciousness_shared * organic_state.efficiency * (1.00f - organic_state.stun_norm);
			no_rotate.mass_multiplier *= organic_state.consciousness_shared * organic_state.efficiency * (1.00f - organic_state.stun_norm);
			no_rotate.speed *= Maths.Lerp(0.20f, 1.00f, organic.motorics);
			no_rotate.bias += (1.00f - organic.motorics.Clamp01()) * 0.15f;
		}

		[ISystem.Update.C(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateNoRotateParent(ISystem.Info info, Entity entity, [Source.Owned, Override] ref NoRotate.Data no_rotate, [Source.Parent, Override] ref NoRotate.Data no_rotate_parent)
		{
			if (no_rotate_parent.flags.HasAny(NoRotate.Flags.No_Share)) return;

			no_rotate_parent.multiplier = Maths.Min(no_rotate_parent.multiplier, no_rotate.multiplier);
			no_rotate_parent.mass_multiplier = Maths.Min(no_rotate_parent.mass_multiplier, no_rotate.mass_multiplier);
		}

		//[ISystem.Update(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", true, Source.Modifier.Owned)]
		//public static void UpdateNoRotateDead(ISystem.Info info, [Source.Owned, Override] in Organic.Data organic, [Source.Owned] in Organic.State organic_state, [Source.Owned, Override] ref NoRotate.Data no_rotate, [Source.Owned] in Legs.Data legs)
		//{
		//	no_rotate.multiplier = 0.00f;
		//}

		public static readonly Sound.Handle[] walk_sounds = new Sound.Handle[]
		{
			"dirt_run_00",
			"dirt_run_01",
			"dirt_run_02",
			"dirt_run_03",
			"dirt_run_04"
		};

#if CLIENT
		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateAnimation(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity,
		[Source.Owned, Override] in Organic.Data organic, [Source.Owned] in Organic.State organic_state,
		[Source.Owned] ref Legs.Data legs, [Source.Owned, Override] in Runner.Data runner, [Source.Owned] in Runner.State runner_state,
		[Source.Owned] ref Animated.Renderer.Data renderer, [Source.Owned] in Control.Data control, [Source.Owned] in Transform.Data transform, [Source.Owned, Optional(true)] ref HeadBob.Data headbob)
		{
			//App.WriteLine($"{Unsafe.AsRef(in headbob).IsNull()}");

			//if (headbob.IsNull()) return;
			//return;

			var bob_amplitude = Vector2.Zero;
			var bob_speed = 0.00f;

			if (headbob.IsNotNull())
			{
				bob_amplitude = headbob.multiplier;
				bob_speed = headbob.speed;
			}

			if (organic_state.efficiency < 0.20f) goto dead;
			else if (runner_state.flags.HasAny(Runner.State.Flags.Sitting)) goto sitting;
			else if (runner_state.flags.HasAny(Runner.State.Flags.Jumping)) goto jumping;
			else if (runner_state.flags.HasAny(Runner.State.Flags.Falling | Runner.State.Flags.Swimming | Runner.State.Flags.Climbing | Runner.State.Flags.WallClimbing)) goto air;
			else if (runner_state.flags.HasAny(Runner.State.Flags.Walking)) goto walking;
			else goto idle;

			walking:
			{
				renderer.sprite.fps = (byte)MathF.Round(legs.fps * (0.30f + ((0.70f * organic_state.efficiency) * (runner_state.flags.HasAll(Runner.State.Flags.Crouching) ? runner.crouch_speed_modifier : 1.00f))));
				renderer.sprite.frame.X = 1;
				renderer.sprite.count = legs.frame_count;

				var offset = runner_state.flags.HasAll(Runner.State.Flags.Grounded) ? new Vector2(-bob_amplitude.X * (MathF.Sin(info.WorldTime * bob_speed)), -bob_amplitude.Y * ((MathF.Sin(info.WorldTime * bob_speed) + 1.00f) * 0.50f)) : Vector2.Zero;
				if (headbob.IsNotNull())
				{
					headbob.offset = Vector2.Lerp(headbob.offset, offset, 0.50f);
				}

				if (renderer.sprite.fps > 0 && runner_state.flags.HasAll(Runner.State.Flags.Grounded))
				{
					if (info.WorldTime >= legs.next_step)
					{
						var interval = ((1.00f / renderer.sprite.fps) * renderer.sprite.count) * legs.sound_interval_multiplier;
						legs.next_step = info.WorldTime + interval;

						Sound.Play(Legs.walk_sounds.GetRandom(), transform.position, volume: legs.sound_volume, pitch: random.NextFloatRange(0.98f, 1.02f) * legs.sound_pitch, priority: 0.10f);
					}
				}

				return;
			}

			jumping:
			{
				var t = 1.00f - Maths.NormalizeClamp(info.WorldTime - Maths.Max(runner_state.last_climb, Maths.Max(runner_state.last_ground, runner_state.last_jump + runner.max_jump_time)), runner.max_air_time);

				renderer.sprite.fps = 0;
				renderer.sprite.frame.X = (uint)MathF.Floor(Maths.Lerp(legs.frames_jump.X, legs.frames_jump.Y, t));
				renderer.sprite.count = 0;

				return;
			}

			sitting:
			{
				var offset = Vector2.Zero;

				if (headbob.IsNotNull())
				{
					headbob.offset = Vector2.Lerp(headbob.offset, offset, 0.50f);
				}

				renderer.sprite.fps = 0;
				renderer.sprite.frame = legs.frame_sitting;
				renderer.sprite.count = 0;

				return;
			}

			idle:
			{
				var offset = Vector2.Zero;

				if (headbob.IsNotNull())
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

				if (headbob.IsNotNull())
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
				//renderer.sprite.frame.X = (uint)(legs.frame_air + Maths.Clamp(runner_state.air_time * legs.fps, 0, 2));
				renderer.sprite.frame.X = (uint)(legs.frame_air);
				renderer.sprite.count = 0;

				return;
			}
		}

#endif
	}
}
