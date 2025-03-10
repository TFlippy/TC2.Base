namespace TC2.Base.Components
{
	public static partial class Pump
	{
		[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Region), IComponent.With<Pump.State>]
		public struct Data(): IComponent
		{
			[Editor.Picker.Position(true)] public Vector2 slider_offset;
			public float volume_speed_mult = 0.30f;
			public Volume volume = 0.70f;
		}

		[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Region)]
		public struct State(): IComponent
		{
			public Pressure pressure;
			public float interval = 1.00f;
			[Save.Ignore, Net.Ignore] public float next_update;
		}

#if CLIENT
		[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdateFX(ISystem.Info info, 
		[Source.Owned] in Transform.Data transform, [Source.Owned] in Pump.Data pump,
		[Source.Owned] ref Sound.Emitter sound_emitter, [Source.Owned] in Axle.Data axle, [Source.Owned] in Axle.State axle_state, 
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
		[Source.Owned] in Pump.Data pump, [Source.Owned] ref Pump.State pump_state,
		[Source.Owned] in Axle.Data axle, [Source.Owned] ref Axle.State axle_state, 
		[Source.Owned, Pair.Component<Pump.Data>] ref Inventory1.Data inventory)
		{

		}
#endif
	}
}
