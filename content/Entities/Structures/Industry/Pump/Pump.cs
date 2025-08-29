namespace TC2.Base.Components
{
	public static partial class Pump
	{
		[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Region)]
		public struct Data(): IComponent
		{
			[Editor.Picker.Position(true)] public Vec2f slider_offset;
			public float volume_speed_mult = 0.30f;
			public Volume volume = 0.70f;

			public IMaterial.Handle h_material_water;

			public Pressure pressure;
			public float interval = 1.00f;
			[Save.Ignore, Net.Ignore] public float next_update;
		}

#if CLIENT
		[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdateFX(ISystem.Info info, 
		[Source.Owned] in Transform.Data transform, [Source.Owned] in Pump.Data pump,
		[Source.Owned] in Axle.Data axle, [Source.Owned] in Axle.State axle_state,
		[Source.Owned, Pair.Component<Pump.Data>] ref Sound.Emitter sound_emitter,
		[Source.Owned, Pair.Component<Pump.Data>] ref Animated.Renderer.Data renderer_slider)
		{
			var speed = MathF.Abs(axle_state.angular_velocity);
			var sin = MathF.Sin(axle_state.rotation * 3);

			renderer_slider.offset = pump.slider_offset + new Vector2(0, sin * 0.25f);
			sound_emitter.pitch_mult = Maths.Lerp(sound_emitter.pitch_mult, Maths.Clamp((speed * 0.10f) + ((0.00f + MathF.Abs(sin)) * 0.50f), 0.50f, 2.50f), 0.25f);
			sound_emitter.volume_mult = Maths.Lerp(sound_emitter.volume_mult, Maths.Clamp(speed * pump.volume_speed_mult, 0.00f, pump.volume), 0.25f);
		}
#endif

#if SERVER
		[ISystem.Update.C(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate(ISystem.Info info, ref XorRandom random, 
		[Source.Owned] in Transform.Data transform, 
		[Source.Owned] in Pump.Data pump,
		[Source.Owned] in Axle.Data axle, [Source.Owned] ref Axle.State axle_state, 
		[Source.Owned, Pair.Component<Pump.Data>] ref Inventory1.Data inventory)
		{
			if (axle_state.flags.HasAny(Axle.State.Flags.Revolved))
			{
				var resource = new Resource.Data(pump.h_material_water, random.NextFloatExtra(0.50f, 1.00f));
				inventory.Deposit(ref resource);
			}
		}
#endif
	}
}
