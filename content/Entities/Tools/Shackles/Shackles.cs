namespace TC2.Base.Components
{
	public static partial class Shackles
	{
		public const Physics.Layer default_layer_require = Physics.Layer.Creature;
		public const Physics.Layer default_layer_exclude = Physics.Layer.Static | Physics.Layer.Building | Physics.Layer.Item | Physics.Layer.Gas | Physics.Layer.Resource | Physics.Layer.Dead;

		[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Region)]
		public struct Data(): IComponent
		{
			[Flags]
			public enum Flags: ushort
			{
				None = 0,

				[Asset.Ignore] Attached = 1 << 15,
			}

			public required float cooldown;
			public required float max_distance;

			[Save.NewLine]
			[Save.Force] public float move_speed_mult = 0.50f;
			[Save.Force] public float move_force_mult = 0.50f;

			[Save.NewLine]
			[Save.Force] public float jump_speed_mult = 0.50f;
			[Save.Force] public float jump_force_mult = 0.50f;

			[Save.NewLine]
			public Shackles.Data.Flags flags;
			public required Chance chance_capture_base;

			[Save.NewLine]
			public Sound.Handle h_sound_success;
			public Sound.Handle h_sound_fail;

			[Save.Ignore, Net.Ignore] public float t_next_use;
		}

		public struct UseRPC: Net.IRPC<Shackles.Data>
		{
			public Vec2f world_position;
			public Entity ent_target;

#if SERVER
			public void Invoke(Net.IRPC.Context rpc, ref Shackles.Data data)
			{
				Assert.Check(data.flags.HasNone(Shackles.Data.Flags.Attached));
				Assert.IsNotEqual(this.ent_target, rpc.entity);
				Assert.Alive(this.ent_target);

				ref var region = ref rpc.GetRegion();
				ref var terrain = ref region.GetTerrain();

				App.WriteValue(this.ent_target.GetFullName());

				//rpc.entity.ReplaceRelation(this.ent_target, Relation.Type.Child, check_recursion: true);
				//rpc.entity.ReplaceRelation(this.ent_target, Relation.Type.Rope, check_recursion: true);



				this.ent_target.ReplaceRelation(rpc.entity, Relation.Type.Rope, check_recursion: true);

				//Sound.Play(region: ref region, sound: data.h_sound_success, world_position: this.world_position);

				//rpc.entity.AddRel<Rope>()
				//rpc.GetSenderCharacter().flags.HasAny(Character.Flags.Captured)

			}
#endif
		}


#if CLIENT
		public struct ShacklesGUI: IGUICommand
		{
			public Transform.Data transform;
			public Shackles.Data shackles;
			public Entity entity;

			public void Draw()
			{
				using (var hud = GUI.Window.HUD("shackles.hud"u8))
				{
					if (hud.show)
					{
						ref var region = ref this.entity.GetRegion();
						ref var terrain = ref region.GetTerrain();

						ref readonly var kb = ref GUI.GetKeyboard();
						ref readonly var mouse = ref GUI.GetMouse();
						var canvas_scale = region.GetWorldToCanvasScale();

						Arm.HoverGUI.Hide();

						//var size = 0.50f; // new Vector2(0.50f);
						//var size_half = size * 0.50f;

						var w_pos_new = mouse.position;

						var color = Color32BGRA.Yellow;
						var color_ok = Color32BGRA.Green;
						//var c_pos_new_snapped = region.WorldToCanvas(w_pos_new_snapped);

						if (region.TryOverlapPointNearest(world_position: w_pos_new,
						radius: 0.250f,
						result: out var result,
						require: Shackles.default_layer_require,
						exclude: Shackles.default_layer_exclude))
						{
							var ent_target = result.entity;
							GUI.DrawEntity(ent_target, color: GUI.col_button_yellow.WithAlpha(128), layer: GUI.Layer.Background);

							if (!GUI.IsHovered)
							{
								if (mouse.GetKeyDown(Mouse.Key.Left))
								{
									var rpc = new Shackles.UseRPC()
									{
										world_position = w_pos_new,
										ent_target = ent_target
									};
									rpc.Send(this.entity);
								}
								//else if (mouse.GetKeyDown(Mouse.Key.Right))
								//{
								//	if (nearest_index >= 0)
								//	{
								//		var rpc = new Shackles.MarkRPC()
								//		{
								//			index = (byte)nearest_index,
								//			position = null,
								//		};
								//		rpc.Send(this.entity);
								//	}
								//}
							}
						}
					}
				}
				
			}
		}

		[ISystem.GUI(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("local", true, Source.Modifier.Parent)]
		public static void OnGUI(ISystem.Info info, Entity entity,
		[Source.Parent] in Interactor.Data interactor, [Source.Owned] ref Shackles.Data shackles,
		[Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control)
		{
			var gui = new ShacklesGUI()
			{
				entity = entity,
				transform = transform,
				shackles = shackles
			};
			gui.Submit();
		}
#endif
		[ISystem.Monitor(ISystem.Mode.Single, ISystem.Scope.Region), HasRelation(Source.Modifier.Owned, Relation.Type.Rope, true)]
		public static void OnAddRem(ISystem.Info info, ref Region.Data region, ref XorRandom random, 
		Entity ent_transform, Entity ent_shackles,
		[Source.Rope] ref Shackles.Data shackles,
		[Source.Owned] in Transform.Data transform, [Source.Rope] in Transform.Data transform_shackles)
		{
#if SERVER
			var sync = false;

			switch (info.EventType)
			{
				case ISystem.EventType.Add:
				{
					sync |= shackles.flags.TryAddFlag(Shackles.Data.Flags.Attached);
					Sound.Play(region: ref region, sound: shackles.h_sound_success, world_position: transform.position);
				}
				break;

				case ISystem.EventType.Remove:
				{

				}
				break;
			}

			if (sync)
			{
				shackles.Sync(ent_shackles);
			}
#endif
		}

		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region), HasRelation(Source.Modifier.Owned, Relation.Type.Rope, true)]
		public static void UpdateRunner(ISystem.Info info, Entity ent_runner, Entity ent_shackles, ref XorRandom random,
		[Source.Rope] ref Shackles.Data shackles,
		[Source.Owned] in Transform.Data transform, [Source.Rope] in Transform.Data transform_shackles,
		[Source.Owned, Override] ref Runner.Data runner, [Source.Owned] ref Runner.State runner_state)
		{
			runner.max_speed *= shackles.move_speed_mult;
			runner.max_jump_speed *= shackles.jump_speed_mult;

			runner.walk_force *= shackles.move_force_mult;
			runner.jump_force *= shackles.jump_force_mult;
		}

		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region), HasRelation(Source.Modifier.Owned, Relation.Type.Rope, true)]
		public static void UpdateFlyer(ISystem.Info info, Entity entity, ref XorRandom random,
		[Source.Rope] ref Shackles.Data shackles, [Source.Owned] in Transform.Data transform,
		[Source.Owned/*, Override*/] ref Flyer.Data flyer)
		{
			//runner.max_speed *= 0.50f;
			//runner.max_jump_speed *= 0.50f;
			//runner.walk_force *= 0.250f;
			//runner.jump_force *= 0.250f;
		}

#if SERVER
		[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasRelation(Source.Modifier.Owned, Relation.Type.Stored, false)]
		public static void Update(ISystem.Info info, Entity entity, ref XorRandom random,
		[Source.Owned] ref Shackles.Data shackles, [Source.Owned] in Transform.Data transform, 
		[Source.Owned] in Control.Data control, [Source.Owned] ref Body.Data body)
		{
			if (control.mouse.GetKeyDown(Mouse.Key.Left) && info.WorldTime >= shackles.t_next_use)
			{

			}
		}
#endif
	}
}
