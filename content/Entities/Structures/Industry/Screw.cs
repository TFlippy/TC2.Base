namespace TC2.Base.Components
{
	public static partial class Screw
	{
		[Flags]
		public enum Flags: ushort
		{
			None = 0,
		}

		public enum Status: byte
		{
			None = 0,
		}

		[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Region)]
		public partial struct Data(): IComponent
		{
			[Net.Segment.A, Save.Force, Editor.Picker.Position(relative: true)] public required Vec2f offset;
			[Net.Segment.A, Save.Force, Editor.Picker.Direction(normalize: true)] public required Vec2f direction = Vec2f.Down;

			[Net.Segment.A, Save.Force, Editor.Slider.Clamped(-16.00f, 0.00f, snap: 0.125f)] public required float length_outer = 1.00f;
			[Net.Segment.A, Save.Force, Editor.Slider.Clamped(0.00f, 16.00f, snap: 0.125f)] public required float length_inner = 1.00f;
			[Net.Segment.A, Save.Force, Editor.Slider.Clamped(0.001f, 8.00f, snap: 0.001f)] public required float ratio = 1.00f;
			[Net.Segment.A, Save.Force, Editor.Slider.Clamped(0.01f, 10000.00f, snap: 0.125f)] public required Mass mass = 1.00f;

			[Net.Segment.C] public Screw.Flags flags;
			[Net.Segment.C, Asset.Ignore] public Screw.Status status;
			[Net.Segment.C, Asset.Ignore, Editor.Slider.Clamped(0, 0, snap: 0.01f)] public float current_displacement;
		}

		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateAxle(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity ent_screw,
		[Source.Owned] ref Screw.Data screw, [Source.Owned] in Transform.Data transform,
		[Source.Owned] ref Crafter.Data crafter, [Source.Owned] ref Crafter.State crafter_state,
		[Source.Owned] ref Axle.Data axle, [Source.Owned] ref Axle.State axle_state)
		{
			var angular_distance = Axle.CalculateAngularDistance(axle.radius_outer, axle_state.rotation_delta) * screw.ratio;
			var current_displacement = angular_distance + screw.current_displacement;

			current_displacement = Maths.Clamp(current_displacement, screw.length_outer, screw.length_inner);
			screw.current_displacement = current_displacement;
		}

		[ISystem.PostUpdate.A(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdateRenderer(ISystem.Info info,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Screw.Data screw,
		[Source.Owned, Pair.Component<Screw.Data>] ref Animated.Renderer.Data renderer)
		{
			//renderer.offset = screw.offset.AddY(screw.current_displacement - (screw.length * 0.50f));
			renderer.offset = screw.offset.AddY(screw.current_displacement);
			//renderer_slider.offset = press.slider_offset + new Vector2(0.00f, MathF.Pow((MathF.Cos(axle_state.rotation) + 1.00f) * 0.50f, press.speed) * press.slider_length);
		}

		[ISystem.PostUpdate.D(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void Update(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity ent_screw,
		[Source.Owned] in Transform.Data transform,
		[Source.Owned] ref Screw.Data screw)
		{

		}
	}
}
