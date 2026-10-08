namespace TC2.Base.Components
{
	//public static partial class Airdrop
	//{
	//	[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Region | IComponent.Scope.Global)]
	//	public partial struct Data(): IComponent
	//	{
	//		[Editor.Picker.Position(relative: false)] public Vec2f pos_target;
	//	}
	//}

	public static partial class Zeppelin
	{
		[Flags]
		public enum Flags: uint
		{
			None = 0u,

			Control_Auto = 1u << 1,
			//Control_Auto = 1u << 2,

			[Asset.Ignore] Sync_Pending = 1u << 31
		}

		[Flags]
		public enum CombatFlags: uint
		{
			None = 0u,

			Point_Defense = 1u << 0,
			Close_Range = 1u << 1,
			Artillery = 1u << 2,
			Bombard = 1u << 3,
			Missile = 1u << 4,

			Ignore_Collateral_Damage = 1u << 16,


		}

		[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Region | IComponent.Scope.Global)]
		public partial struct Data(): IComponent
		{
			public float unused_00;
			public float altitude_preferred;
			public float throttle;

			public Zeppelin.Flags flags;
			public Zeppelin.CombatFlags combat_flags;
			//public Zeppelin.CombatFlags combat_flags;

			public Vec2f speed_step = new(1.00f, 0.25f);
			public Vec2f speed_max = new(10, 2);

			[Editor.Picker.Position(true)] public Vec2f offset_bottom;
			[Editor.Picker.Position(true)] public Vec2f offset_bay;
			[Editor.Picker.Position(true)] public Vec2f offset_cabin;
			[Editor.Picker.Position(true)] public Vec2f offset_tail;

			[Asset.Ignore] public Entity ent_target_dock;
			[Asset.Ignore] public Entity ent_target_attack;
			[Asset.Ignore, Editor.Picker.Position(false)] public Vec2f pos_move;
			[Asset.Ignore, Editor.Picker.Position(false)] public Vec2f pos_defend;
			[Asset.Ignore, Editor.Picker.Position(false)] public Vec2f pos_aim;
			[Asset.Ignore, Editor.Picker.Position(false)] public Vec2f pos_skyhook;

			//[Asset.Ignore] public Vec2f vel_current;
			[Asset.Ignore] public Vec2f vel_target;
		}

		public struct DEV_SendRequestRPC: Net.IRPC<Zeppelin.Data>
		{
			public Entity ent_target;
			public Vec2f pos_target;

#if SERVER
			public void Invoke(Net.IRPC.Context rpc, ref Zeppelin.Data data)
			{
			}
#endif
		}

		public struct DEV_DockRPC: Net.IRPC<Zeppelin.Data>
		{
			public Entity ent_dock;
#if SERVER
			public void Invoke(Net.IRPC.Context rpc, ref Zeppelin.Data data)
			{
				ref var region_common = ref rpc.GetRegionCommon();
				Assert.Alive(this.ent_dock);

				ref var transform_dock = ref this.ent_dock.GetTransform();
				Assert.IsNotNull(ref transform_dock);

				ref var transform = ref rpc.record.GetTransform();
				Assert.IsNotNull(ref transform);

				var pos_dock = transform_dock.position;
				App.WriteValue(pos_dock);

				var pos_air = pos_dock.WithY(0);
				var pos_move = new Vec2f(pos_dock.X, -data.unused_00);

				region_common.DrawDebugLine(pos_air, pos_dock, color: Color32BGRA.Green, thickness: 4);
				region_common.DrawDebugLine(transform.position, pos_dock, color: Color32BGRA.Green, thickness: 4);

				data.pos_aim = pos_dock;
				data.pos_move = pos_move;
				data.Sync(rpc.record);
			}
#endif
		}

#if CLIENT
		public partial struct ZeppelinGUI: IGUICommand
		{
			public Entity ent_zeppelin;
			public Zeppelin.Data zeppelin;
			public Transform.Data transform;

			public void Draw()
			{
				using (var window = GUI.Window.Interaction("Zeppelin"u8, this.ent_zeppelin))
				{
					this.StoreCurrentWindowTypeID(order: 6);
					if (window.show)
					{

					}
				}
			}
		}

		[ISystem.GUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI([Source.Owned] in Interactable.Data interactable,
		Entity ent_zeppelin, [Source.Owned] in Zeppelin.Data zeppelin, [Source.Owned] in Transform.Data transform)
		{
			if (interactable.IsActive())
			{
				var gui = new ZeppelinGUI()
				{
					ent_zeppelin = ent_zeppelin,
					zeppelin = zeppelin,
					transform = transform,
				};
				gui.Submit();
			}
		}
#endif

		[ISystem.Update.B(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate(ISystem.Info info, ref Region.Data region, Entity entity,
		[Source.Owned] ref Zeppelin.Data zeppelin, [Source.Owned] ref Control.Data control,
		[Source.Owned] ref Transform.Data transform, [Source.Owned] ref Body.Data body,
		[Source.Owned] in Faction.Data faction)
		{
#if SERVER
			region.DrawDebugDir(a: transform.position, dir: Vec2f.Down * zeppelin.unused_00, color: Color32BGRA.Magenta);

			if (zeppelin.flags.HasAny(Flags.Control_Auto))
			{
				var pos_aim = zeppelin.pos_aim;
				if (pos_aim)
				{
					control.mouse.position = pos_aim;
				}

				var pos_target = zeppelin.pos_move;
				if (pos_target)
				{
					var pos_current = transform.position;
					var delta = pos_target - pos_current;

					var threshold = 0.10f;
					var vel = zeppelin.vel_target * 10; // * 50;
														//vel.x *= zeppelin.vel_target.x.Sign();

					//Maths.ShouldDecelerate()

					ref var kb = ref control.keyboard;
					kb.SetKeyPressed(Keyboard.Key.MoveRight, (delta.x - vel.x) > threshold);
					kb.SetKeyPressed(Keyboard.Key.MoveLeft, (delta.x - vel.x) < threshold);
					kb.SetKeyPressed(Keyboard.Key.MoveDown, (delta.y - vel.y) > threshold);
					kb.SetKeyPressed(Keyboard.Key.MoveUp, (delta.y - vel.y) < threshold);


				}
			}
			//Maths.CalculateStoppingDistance
#endif
		}

		// TODO: this could be entirely serverside actually, since the body's velocity gets automatically synced by the physics component anyway
		[Shitcode]
		[ISystem.Update.C(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate_Control(ISystem.Info info, ref Region.Data region, Entity entity,
		[Source.Owned] ref Zeppelin.Data zeppelin, [Source.Owned] ref Control.Data control,
		[Source.Owned] ref Transform.Data transform, [Source.Owned] ref Body.Data body)
		{
			// TODO: optimize with SIMD instructions for adjacent add/sub operations
			var vel_x = control.keyboard.GetKeyAxis(Keyboard.Key.MoveRight, Keyboard.Key.MoveLeft, zeppelin.speed_step.x);
			var vel_y = control.keyboard.GetKeyAxis(Keyboard.Key.MoveDown, Keyboard.Key.MoveUp, zeppelin.speed_step.y);

			vel_x = Maths.FMA(vel_x, App.fixed_update_interval_s_f32, zeppelin.vel_target.x);
			vel_y = Maths.FMA(vel_y, App.fixed_update_interval_s_f32, zeppelin.vel_target.y);

			vel_x.ClampMagnitude(zeppelin.speed_max.x);
			vel_y.ClampMagnitude(zeppelin.speed_max.y);

			if (zeppelin.vel_target.TrySet(new(vel_x, vel_y)))
			{
#if SERVER
				zeppelin.flags.AddFlag(Zeppelin.Flags.Sync_Pending);
#endif
			}

			//var acc = new Vec2f(acc_x, acc_y);
			//if (acc.IsNotNil())
			//{
			//	// TODO: use FMA
			//	var vel_target_new = zeppelin.vel_target + (acc * App.fixed_update_interval_s_f32);

			//	// TODO: bleh
			//	var vel_target_new_clamped = new Vec2f(Maths.ClampMagnitude(vel_target_new.x, zeppelin.speed_max.x), Maths.ClampMagnitude(vel_target_new.y, zeppelin.speed_max.y));

			//}

			body.SetVelocity(zeppelin.vel_target);

#if SERVER
			if (info.Tickstamp.CheckInterval(Tickstamp.Interval.T008))
			{
				if (zeppelin.flags.TryRemoveFlag(Zeppelin.Flags.Sync_Pending))
				{
					zeppelin.Sync(entity);
				}
			}
#endif
		}
	}
}
