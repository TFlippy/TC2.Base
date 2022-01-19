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
			public Vector2 head_offset;
			public float head_bob_speed;
			public float head_bob_amplitude;

			[Save.Ignore, Net.Ignore] public Vector2 offset;
			[Save.Ignore, Net.Ignore] public float lerp;

			[Save.Ignore] public Torso.Flags flags;
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
					joint.offset_a_modifier = 0.50f;
					joint.offset_b_modifier = 0.50f;
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
				if (runner.flags.HasAll(Runner.Flags.Crouching)) torso.flags |= Torso.Flags.Crouching;
				else torso.flags &= ~Torso.Flags.Crouching;
			}
		}

#if CLIENT
		[ISystem.Update(ISystem.Mode.Single)]
		public static void UpdateAnimation(ISystem.Info info, Entity entity, [Source.Owned] in Organic.Data organic, [Source.Owned] in Organic.State organic_state, [Source.Owned] ref Torso.Data torso, [Source.Owned] in Control.Data control, [Source.Owned] ref Animated.Renderer.Data renderer)
		{
			var walking = !torso.flags.HasAll(Torso.Flags.Crouching) && !control.keyboard.GetKey(Keyboard.Key.NoMove) && control.keyboard.GetKey(Keyboard.Key.MoveLeft | Keyboard.Key.MoveRight);

			if (organic_state.efficiency < 0.20f) goto dead;
			else if (walking) goto walking;
			else goto idle;

			walking:
			{
				var offset = new Vector2(0.00f, -torso.head_bob_amplitude * ((MathF.Sin(info.WorldTime * torso.head_bob_speed) + 1.00f) * 0.50f));

				torso.head_offset = offset;

				renderer.sprite.fps = (byte)Math.Round(12 * (0.30f + (0.70f * organic_state.efficiency)));
				renderer.sprite.frame.X = 1;
				renderer.sprite.count = 4;
				renderer.offset = offset;

				return;
			}

			idle:
			{
				//var pain_mult = 1.00f + (organic.pain_shared / 10000.00f);
				var offset = new Vector2(0, -torso.head_bob_amplitude * ((MathF.Sin((info.WorldTime + (float)entity.GetShortID()) * 2.50f) + 1.00f) * 0.20f));

				torso.head_offset = Vector2.Lerp(torso.head_offset, offset, 0.50f);

				renderer.sprite.fps = 0;
				renderer.sprite.frame.X = 0;
				renderer.sprite.count = 0;
				renderer.offset = Vector2.Lerp(renderer.offset, offset, 0.50f);

				return;
			}

			dead:
			{
				var offset = Vector2.Zero;

				torso.head_offset = offset;

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
