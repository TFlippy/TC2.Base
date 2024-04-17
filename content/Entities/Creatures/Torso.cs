namespace TC2.Base.Components
{
	public static class Torso
	{
		[Flags]
		public enum Flags: uint
		{
			None = 0,

			[Save.Ignore] Crouching = 1 << 0
		}

		[IComponent.Data(Net.SendType.Unreliable, region_only: true)]
		public struct Data: IComponent
		{
			public float crouch_offset_modifier = 0.50f;

			public Torso.Flags flags;

			public uint frame_count = 4;
			public uint fps = 12;

			public uint2 frames_air;

			[Save.Ignore, Net.Ignore] public Vector2 offset;
			[Save.Ignore, Net.Ignore] public float lerp;
			[Save.Ignore, Net.Ignore] public float air_time;

			public Data()
			{

			}
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", false, Source.Modifier.Owned), HasComponent<Arm.Data>(Source.Modifier.Owned, true)]
		public static void UpdateArmDrag(ISystem.Info info, [Source.Shared] in Torso.Data torso, [Source.Owned, Override] ref Drag.Data drag_override)
		{
			drag_override.max_force *= Maths.Lerp01(1.00f, 0.00f, (torso.air_time * 1.50f) - 1.00f).Pow2();
		}

		[ISystem.Update(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateNoRotate(ISystem.Info info, [Source.Owned, Override] in Organic.Data organic, [Source.Owned] in Organic.State organic_state, [Source.Owned, Override] ref NoRotate.Data no_rotate, [Source.Owned] in Torso.Data torso)
		{
			var mult = (organic_state.consciousness_shared * organic_state.efficiency * Maths.Lerp(0.20f, 1.00f, organic.motorics * organic.motorics));

			//no_rotate.multiplier = MathF.Round(organic_state.consciousness_shared * organic_state.efficiency * Maths.Lerp(0.20f, 1.00f, organic.motorics * organic.motorics)) * organic.coordination * organic.motorics;
			no_rotate.multiplier *= Maths.Clamp01(Maths.Clamp01(mult + 0.40f) * organic.coordination * organic.motorics);
			no_rotate.speed *= Maths.Lerp(0.20f, 1.00f, organic.motorics);
			no_rotate.bias += (1.00f - organic.motorics.Clamp01()) * 0.15f;
		}

		[ISystem.VeryEarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region, order: 90)]
		public static void UpdateSitting([Source.Seat] in Seat.Data seat, [Source.Owned] ref Torso.Data torso)
		{
			torso.flags.SetFlag(Flags.Crouching, seat.flags.HasAny(Seat.Flags.Crouch));
		}

		[ISystem.VeryEarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region, order: 100)]
		public static void UpdateJoints([Source.Shared] in Torso.Data torso, [Source.Owned] ref Joint.Base joint)
		{
			if (joint.flags.HasAny(Joint.Flags.Organic))
			{
				if (torso.flags.HasAny(Torso.Flags.Crouching))
				{
					joint.offset_a_modifier = torso.crouch_offset_modifier;
					joint.offset_b_modifier = torso.crouch_offset_modifier;
				}
			}
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateJoints([Source.Owned, Override] in Runner.Data runner, [Source.Owned] in Runner.State runner_state, [Source.Parent] ref Torso.Data torso, [Source.Parent] in Joint.Base joint)
		{
			if (joint.flags.HasAny(Joint.Flags.Organic))
			{
				torso.air_time = runner_state.air_time;

				if (runner_state.flags.HasAny(Runner.State.Flags.Crouching)) torso.flags |= Torso.Flags.Crouching;
				else torso.flags &= ~Torso.Flags.Crouching;
			}
		}

#if SERVER
		[ISystem.Event<EssenceNode.FailureEvent>(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", false, Source.Modifier.Owned)]
		public static void OnFailure(ISystem.Info info, Entity entity, ref XorRandom random, ref Region.Data region, ref EssenceNode.FailureEvent data, [Source.Owned] ref Transform.Data transform, [Source.Owned, Override] ref Organic.Data organic, [Source.Owned] ref Organic.State organic_state, [Source.Owned] ref Torso.Data torso)
		{
			if (random.NextBool(data.power * 0.20f))
			{
				WorldNotification.Push(ref region, $"* Sudden Heart Failure! *", Color32BGRA.Red, transform.position, lifetime: 3.00f, velocity: Vector2.Zero);

				organic_state.pain += 10000.00f;
				organic_state.Sync(entity);
			}
		}
#endif

#if CLIENT
		[ISystem.Update(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateAnimation(ISystem.Info info, Entity entity,
		[Source.Owned, Override] in Organic.Data organic, [Source.Owned] in Organic.State organic_state,
		[Source.Owned] ref Torso.Data torso, [Source.Owned, Optional(true)] ref HeadBob.Data headbob,
		[Source.Owned] ref Animated.Renderer.Data renderer, [Source.Owned] in Control.Data control)
		{
			var walking = !torso.flags.HasAll(Torso.Flags.Crouching) && !control.keyboard.GetKey(Keyboard.Key.NoMove | Keyboard.Key.X) && control.keyboard.GetKey(Keyboard.Key.MoveLeft | Keyboard.Key.MoveRight);

			var bob_amplitude = Vector2.Zero;
			var bob_speed = 0.00f;

			if (!headbob.IsNull())
			{
				bob_amplitude = headbob.multiplier;
				bob_speed = headbob.speed;
			}

			if (organic_state.efficiency < 0.20f) goto dead;
			else if (torso.air_time > 0.10f) goto air;
			else if (walking) goto walking;
			else goto idle;

			walking:
			{
				//var offset = new Vector2(-bob_amplitude.X * ((MathF.Cos(info.WorldTime * bob_speed) + 1.00f) * 0.50f), -bob_amplitude.Y * ((MathF.Sin(info.WorldTime * bob_speed) + 1.00f) * 0.50f));
				var offset = new Vector2(-bob_amplitude.X * Maths.HvCos(info.WorldTime * bob_speed), -bob_amplitude.Y * Maths.HvSin(info.WorldTime * bob_speed));

				if (!headbob.IsNull())
				{
					headbob.offset = Vector2.Lerp(headbob.offset, offset, 0.75f);
				}

				renderer.sprite.fps = (byte)MathF.Round(torso.fps * (0.30f + (0.70f * organic_state.efficiency)));
				renderer.sprite.frame.X = 1;
				renderer.sprite.count = torso.frame_count;
				renderer.offset = offset;

				return;
			}

			idle:
			{
				//var pain_mult = 1.00f + (organic.pain_shared / 10000.00f);
				var offset = new Vector2(-bob_amplitude.X * ((MathF.Cos((info.WorldTime + (float)entity.GetShortID()) * 2.50f) + 1.00f) * 0.20f), -bob_amplitude.Y * ((MathF.Sin((info.WorldTime + (float)entity.GetShortID()) * 2.50f) + 1.00f) * 0.20f));

				if (!headbob.IsNull())
				{
					headbob.offset = Vector2.Lerp(headbob.offset, offset, 0.50f);
				}

				renderer.sprite.fps = 0;
				renderer.sprite.frame.X = 0;
				renderer.sprite.count = 0;
				renderer.offset = Vector2.Lerp(renderer.offset, offset, 0.50f);

				return;
			}

			air:
			{
				var t = Maths.Clamp01((torso.air_time) * 6.00f);

				var offset = Vector2.Zero;

				if (!headbob.IsNull())
				{
					headbob.offset = Vector2.Lerp(headbob.offset, new Vector2(0, 0.10f * Maths.Clamp01((torso.air_time) * 3.00f)), 0.50f);
				}

				renderer.sprite.fps = 0;
				renderer.sprite.frame.X = (uint)MathF.Floor(Maths.Lerp(torso.frames_air.X, torso.frames_air.Y, t));
				renderer.sprite.count = 0;
				renderer.offset = Vector2.Lerp(renderer.offset, offset, 0.50f);

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
				renderer.sprite.frame.X = 2;
				renderer.sprite.count = 0;
				renderer.offset = offset;

				return;
			}
		}
#endif
	}
}
