namespace TC2.Base.Components
{
	public static class Torso
	{
		[Flags]
		public enum Flags: uint
		{
			None = 0,

			Crouching = 1 << 0
		}

		[IComponent.Data(Net.SendType.Unreliable)]
		public struct Data: IComponent
		{
			public float crouch_offset_modifier = 0.50f;

			[Save.Ignore, Net.Ignore] public Vector2 offset;
			[Save.Ignore, Net.Ignore] public float lerp;

			[Save.Ignore] public Torso.Flags flags;

			public uint frame_count = 4;
			public uint fps = 12;

			[Save.Ignore, Net.Ignore] public float air_time;
		}

		[ISystem.Update(ISystem.Mode.Single)]
		public static void UpdateNoRotate(ISystem.Info info, [Source.Owned] in Organic.Data organic, [Source.Owned] in Organic.State organic_state, [Source.Owned] ref NoRotate.Data no_rotate, [Source.Owned] in Torso.Data torso)
		{
			no_rotate.multiplier = MathF.Round(organic_state.consciousness_shared) * organic_state.efficiency;
		}

		[ISystem.VeryEarlyUpdate(ISystem.Mode.Single)]
		public static void UpdateJoints([Source.Shared] in Torso.Data torso, [Source.Owned] ref Joint.Base joint)
		{
			if (joint.flags.HasAll(Joint.Flags.Organic))
			{
				if (torso.flags.HasAll(Torso.Flags.Crouching))
				{
					joint.offset_a_modifier = torso.crouch_offset_modifier;
					joint.offset_b_modifier = torso.crouch_offset_modifier;
				}
				else
				{
					joint.offset_a_modifier = 1.00f;
					joint.offset_b_modifier = 1.00f;
				}
			}
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single)]
		public static void UpdateJoints([Source.Owned] in Runner.Data runner, [Source.Parent] ref Torso.Data torso, [Source.Parent] in Joint.Base joint)
		{
			if (joint.flags.HasAll(Joint.Flags.Organic))
			{
				torso.air_time = runner.air_time;

				if (runner.flags.HasAll(Runner.Flags.Crouching)) torso.flags |= Torso.Flags.Crouching;
				else torso.flags &= ~Torso.Flags.Crouching;
			}
		}

#if CLIENT
		[ISystem.Update(ISystem.Mode.Single)]
		public static void UpdateAnimation(ISystem.Info info, Entity entity, 
		[Source.Owned] in Organic.Data organic, [Source.Owned] in Organic.State organic_state,
		[Source.Owned] ref Torso.Data torso, [Source.Owned, Optional(true)] ref HeadBob.Data headbob,
		[Source.Owned] ref Animated.Renderer.Data renderer, [Source.Owned] in Control.Data control)
		{
			var walking = !torso.flags.HasAll(Torso.Flags.Crouching) && !control.keyboard.GetKey(Keyboard.Key.NoMove) && control.keyboard.GetKey(Keyboard.Key.MoveLeft | Keyboard.Key.MoveRight);

			var bob_amplitude = Vector2.Zero;
			var bob_speed = 0.00f;

			if (!headbob.IsNull())
			{
				bob_amplitude = headbob.multiplier;
				bob_speed = headbob.speed;
			}

			if (organic_state.efficiency < 0.20f) goto dead;
			//else if (torso.air_time > 0.10f) goto air;
			else if (walking) goto walking;
			else goto idle;

			walking:
			{
				var offset = new Vector2(-bob_amplitude.X * ((MathF.Cos(info.WorldTime * bob_speed) + 1.00f) * 0.50f), -bob_amplitude.Y * ((MathF.Sin(info.WorldTime * bob_speed) + 1.00f) * 0.50f));

				if (!headbob.IsNull())
				{
					headbob.offset = offset;
				}

				renderer.sprite.fps = (byte)Math.Round(torso.fps * (0.30f + (0.70f * organic_state.efficiency)));
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
				var offset = Vector2.Zero;

				if (!headbob.IsNull())
				{
					headbob.offset = offset;
				}

				renderer.sprite.fps = 0;
				renderer.sprite.frame.X = (uint)(5 + Maths.Clamp(torso.air_time * torso.fps, 0, 2));
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
