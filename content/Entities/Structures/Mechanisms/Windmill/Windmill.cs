namespace TC2.Base.Components
{
	public static partial class Windmill
	{
		[Flags]
		public enum Flags: ushort
		{
			None = 0,


		}

		[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Region)]
		public partial struct Data(): IComponent
		{
			[Editor.Picker.Position(relative: true)] public Vec2f offset;
			public float efficiency;
			public Area sail_area;

			[Save.NewLine]
			public Windmill.Flags flags;
			public ushort unused_01;

			[Save.NewLine]
			public float speed_max;
			public float torque_max;


			[Asset.Ignore] public float current_altitude_cached;
			[Asset.Ignore] public float unused_04;
			[Asset.Ignore] public float unused_05;
			[Asset.Ignore] public float unused_06;
		}

		public struct DEV_EditRPC: Net.IRPC<Windmill.Data>
		{
			public float? edit_unused_00;

#if SERVER
			public void Invoke(Net.IRPC.Context rpc, ref Windmill.Data data)
			{
				var sync = false;
				//sync |= this.edit_length_target.TryCopyToClamped(ref data.length_target, min: 1.00f, max: data.length_max);

				if (sync)
				{
					rpc.Sync(ref data);
				}
			}
#endif
		}

#if CLIENT
		public partial struct WindmillGUI: IGUICommand
		{
			public Entity ent_windmill;
			public Windmill.Data windmill;
			public Transform.Data transform;

			public void Draw()
			{
				using (var window = GUI.Window.Interaction("Windmill"u8, this.ent_windmill))
				{
					this.StoreCurrentWindowTypeID(order: 7);
					if (window.show)
					{

					}
				}
			}
		}

		[ISystem.GUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI([Source.Owned] in Interactable.Data interactable,
		Entity ent_windmill, [Source.Owned] in Windmill.Data windmill, [Source.Owned] in Transform.Data transform)
		{
			if (interactable.IsActive())
			{
				var gui = new WindmillGUI()
				{
					ent_windmill = ent_windmill,
					windmill = windmill,
					transform = transform,
				};
				gui.Submit();
			}
		}
#endif

		[ISystem.Update.C(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate(ISystem.Info info, ref Region.Data region, Entity entity,
		[Source.Owned] ref Windmill.Data windmill, 
		[Source.Owned] ref Transform.Data transform, [Source.Owned] ref Body.Data body,
		[Source.Owned] ref Axle.Data axle, [Source.Owned] ref Axle.State axle_state)
		{

		}

		[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate_Effects(ISystem.Info info, ref Region.Data region, Entity entity,
		[Source.Owned] ref Windmill.Data windmill, 
		[Source.Owned] ref Transform.Data transform, [Source.Owned] ref Body.Data body,
		[Source.Owned] ref Axle.Data axle, [Source.Owned] ref Axle.State axle_state,
		[Source.Owned, Pair.Component<Windmill.Data>] ref Animated.Renderer.Data renderer,
		[Source.Owned, Pair.Component<Windmill.Data>, Optional(true)] ref Sound.Emitter sound_emitter)
		{


			if (sound_emitter.IsNotNull())
			{
				//sound_emitter.
			}
		}

//		[ISystem.Update.F(ISystem.Mode.Single, ISystem.Scope.Region)]
//		public static void OnUpdate_Joint(ISystem.Info info, ref Region.Data region, Entity entity,
//		[Source.Owned] ref Windmill.Data windmill, [Source.Owned] ref Transform.Data transform, [Source.Owned] ref Body.Data body,
//		[Source.Parent, Original] ref Joint.Rope joint_rope, [Source.Parent] ref Joint.Base joint_base)
//		{
//			//joint_rope.off

//			Maths.MoveTowards(ref windmill.length_current, windmill.length_target, windmill.speed * App.fixed_update_interval_s, out var delta);
//			joint_rope.distance = windmill.length_current;

////			var vel_add = body.Down * windmill.speed * App.fixed_update_interval_s * 100 * delta;
////			body.AddForce(vel_add * body.GetMass());

////#if SERVER
////			region.DrawDebugDir(body.GetPosition(), vel_add * 10, color: Color32BGRA.Red);
////#endif
//		}
	}
}
