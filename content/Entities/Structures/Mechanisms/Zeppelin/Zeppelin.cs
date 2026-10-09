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

			Docked = 1u << 8,
			Landing = 1u << 9,

			Skyhook_Deployed = 1u << 16,
			Airstrike_Pending = 1u << 17,

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
			public float unused_01;

			[Save.NewLine]
			public Zeppelin.Flags flags;
			public Zeppelin.CombatFlags combat_flags;
			//public Zeppelin.CombatFlags combat_flags;

			[Save.NewLine]
			public Vec2f speed_step = new(1.00f, 0.25f);
			public Vec2f speed_max = new(10, 2);

			[Save.NewLine]
			[Editor.Picker.Position(true)] public Vec2f offset_bottom;
			[Editor.Picker.Position(true)] public Vec2f offset_bay;
			[Editor.Picker.Position(true)] public Vec2f offset_cabin;
			[Editor.Picker.Position(true)] public Vec2f offset_tail;

			[Save.NewLine]
			public float speed_skyhook_crane;
			[Editor.Picker.Position(true)] public Vec2f offset_skyhook_crane_a;
			[Editor.Picker.Position(true)] public Vec2f offset_skyhook_crane_b;

			[Save.NewLine]
			[Asset.Ignore] public Entity ent_target_dock;
			[Asset.Ignore] public Entity ent_target_attack;
			[Asset.Ignore] public Entity ent_target_cargo;

			[Asset.Ignore, Editor.Picker.Position(false)] public Vec2f pos_move;
			[Asset.Ignore, Editor.Picker.Position(false)] public Vec2f pos_defend;
			[Asset.Ignore, Editor.Picker.Position(false)] public Vec2f pos_aim;
			[Asset.Ignore, Editor.Picker.Position(false)] public Vec2f pos_airstrike;
			[Asset.Ignore, Editor.Picker.Position(false)] public Vec2f pos_skyhook;
			[Asset.Ignore, Editor.Picker.Position(false)] public Vec2f pos_skyhook_target;

			//[Asset.Ignore] public Vec2f vel_current;
			[Asset.Ignore] public Vec2f vel_target;
			[Asset.Ignore, Net.Ignore] public float dist_target;
			[Asset.Ignore, Net.Ignore] public float t_airstrike_start;
			[Asset.Ignore, Net.Ignore] public float t_airstrike_end;
		}

		public struct DEV_SendRequestRPC: Net.IRPC<Zeppelin.Data>
		{
			public Entity ent_target;
			public Vec2f? pos_target;

			public Zeppelin.Flags? flags; // TODO: probably make it a separate field on Zeppelin.Data

#if SERVER
			public void Invoke(Net.IRPC.Context rpc, ref Zeppelin.Data data)
			{
				ref var region_common = ref rpc.GetRegionCommon();

				var sync = false;
				sync |= data.flags.TrySetFlagMasked(this.flags, mask: Flags.Skyhook_Deployed | Flags.Airstrike_Pending);
				
				if (data.pos_airstrike.TrySet(this.pos_target).RelayTo(ref sync))
				{
					data.t_airstrike_start = region_common.GetWorldTime() + 2;
					data.t_airstrike_end = data.t_airstrike_start + 1.50f;
				}

				if (sync)
				{
					rpc.Sync(ref data);
				}
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
				data.ent_target_dock = this.ent_dock;
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
			//region.DrawDebugDir(a: transform.position, dir: Vec2f.Down * zeppelin.unused_00, color: Color32BGRA.Magenta);

			var sync = false;

			if (zeppelin.flags.HasAny(Flags.Control_Auto))
			{
				var time = region.GetWorldTime();

#if SERVER
				var pos_aim = zeppelin.pos_aim;

				if (zeppelin.flags.HasAny(Flags.Airstrike_Pending))
				{
					pos_aim = zeppelin.pos_airstrike;

					if (time > zeppelin.t_airstrike_end)
					{
						zeppelin.flags.RemoveFlag(Flags.Airstrike_Pending);
						control.keyboard.SetKeyPressed(Keyboard.Key.Reload, true);
						sync = true;
					}
					else if (time >= zeppelin.t_airstrike_start)
					{
						control.mouse.SetKeyPressed(Mouse.Key.Left, true);
					}
				}

				if (pos_aim)
				{
					control.mouse.SetPosition(pos_aim);
					//control.Sync(entity);
				}
#endif

				var pos_target = zeppelin.pos_move;
				if (pos_target)
				{
					var pos_current = transform.position;
					var delta = (pos_target - pos_current);
					var dist = delta.vect.Length();
					zeppelin.dist_target = dist;

					// TODO: properly calculate when it should start decelerating
					var threshold = 3.50f;
					var vel = body.GetVelocity().ToVec2f(); // * 50;
					var vel_len = vel.vect.Length();

					var vel_overshoot = vel * 2.00f;
					//zeppelin.throttle = dist;

					if (Maths.ShouldDecelerate(dist, vel_len * 2.00f, zeppelin.speed_step.vect.Length()))
					{
						vel_overshoot *= 20f;
					}

					if (dist > threshold * 1.50f) // || Maths.ShouldDecelerate(dist, vel_len, zeppelin.speed_step.vect.Length()))
					{
						ref var kb = ref control.keyboard;
						kb.SetKeyPressed(Keyboard.Key.MoveRight, (delta.x - vel_overshoot.x) > threshold);
						kb.SetKeyPressed(Keyboard.Key.MoveLeft, (delta.x - vel_overshoot.x) < threshold);
						kb.SetKeyPressed(Keyboard.Key.MoveDown, (delta.y - vel_overshoot.y) > threshold);
						kb.SetKeyPressed(Keyboard.Key.MoveUp, (delta.y - vel_overshoot.y) < threshold);
					}
					else
					{
						var vel_brake = zeppelin.vel_target * 0.99f;
						if (zeppelin.vel_target.TrySet(vel_brake))
						{
#if SERVER
							if (info.Tickstamp.CheckInterval(Tickstamp.Interval.T008))
							{
								sync = true;
							}
#endif
						}
					}
				}
			}

#if SERVER
			if (sync)
			{
				zeppelin.Sync(entity);
			}
#endif
		}

		[ISystem.Update.D(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate_Skyhook(ISystem.Info info, ref Region.Data region, 
		Entity ent_zeppelin, Entity ent_skyhook,
		[Source.Parent] ref Zeppelin.Data zeppelin, [Source.Owned] ref Control.Data control,
		[Source.Owned] ref Skyhook.Data skyhook,
		[Source.Parent, Original] ref Joint.Rope joint_rope, [Source.Parent] ref Joint.Base joint_base,
		[Source.Parent] ref Transform.Data transform_zeppelin, [Source.Owned] ref Transform.Data transform,
		[Source.Owned] ref Body.Data body)
		{
			//region.DrawDebugBody(ref body, color: Color32BGRA.Green);

			var pos_body = body.GetPosition();
			zeppelin.pos_skyhook = pos_body;

			if (zeppelin.flags.HasAny(Flags.Skyhook_Deployed))
			{
				var crane_dist_target = 0.00f;

				var pos_target = zeppelin.ent_target_dock.GetPosition();
				if (pos_target)
				{
					zeppelin.pos_skyhook_target = pos_target;

					var delta = pos_target - transform_zeppelin.LocalToWorld(zeppelin.offset_bay); //.AddY(-2);
					skyhook.length_target = delta.y.Abs();
				}

				var crane_wpos_a = transform_zeppelin.LocalToWorld(zeppelin.offset_skyhook_crane_a);
				var crane_wpos_b = transform_zeppelin.LocalToWorld(zeppelin.offset_skyhook_crane_b);



				var crane_dist_lerp = Maths.InvLerp01(crane_wpos_a.X, crane_wpos_b.X, pos_target.x);

				joint_base.offset_a = Maths.MoveTowards(joint_base.offset_a, Maths.Lerp(zeppelin.offset_skyhook_crane_a, zeppelin.offset_skyhook_crane_b, crane_dist_lerp), zeppelin.speed_skyhook_crane * App.fixed_update_interval_s);
				zeppelin.unused_01 = crane_dist_lerp;
			}
			else
			{
				skyhook.length_target = 0.00f;
			}
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

			vel_x = Maths.FMA(vel_x, info.DeltaTime, zeppelin.vel_target.x);
			vel_y = Maths.FMA(vel_y, info.DeltaTime, zeppelin.vel_target.y);

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

			//zeppelin.vel_target *= 0.50f;
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
