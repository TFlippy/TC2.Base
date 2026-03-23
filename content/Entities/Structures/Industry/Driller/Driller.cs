namespace TC2.Base.Components
{
	[WIP]
	public static partial class Driller
	{
		[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Region)]
		public struct Data(): IComponent
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0,

				Active = 1 << 0,
				Stuck = 1 << 1,
			}

			[Save.Force] public Driller.Data.Flags flags;
			[Save.Force] public IMaterial.Handle h_material_rail;
			[Save.Force] public ISubstance.Handle h_substance_drill;

			[Save.Force] public required float interval = 0.50f;
			[Save.Force] public float drill_offset;

			[Save.NewLine]
			[Save.Force, Editor.Picker.Position(relative: true)] public required Vec2f offset;
			[Save.Force, Editor.Picker.Direction(normalize: true)] public required Vec2f direction = Vec2f.Down;

			[Save.NewLine]
			[Save.Force] public required float power;
			[Save.Force] public required float yield;
			[Save.Force] public required float thickness;
			[Save.Force] public required float size;

			[Save.NewLine]
			[Save.Force] public required float speed_max;
			[Save.Force] public required float distance_max;

			public ISoundMix.Handle h_soundmix_dig;
			public ISoundMix.Handle h_soundmix_mode;
			public ISoundMix.Handle h_soundmix_move;

			public float distance_current;
			[Save.Ignore, Net.Ignore] public float t_next_update;
		}

		public struct EditRPC: Net.IRPC<Driller.Data>
		{
			public float? speed_ratio;
			public float? depth_ratio;

#if SERVER
			public void Invoke(Net.IRPC.Context rpc, ref Driller.Data data)
			{
				var sync = false;

				if (this.depth_ratio.HasValue)
				{
					sync |= data.drill_offset.TrySetClamped(this.depth_ratio.Value * data.distance_max, 0.00f, data.distance_max);
				}

				if (sync)
				{
					rpc.Sync(ref data, true);
				}
			}
#endif
		}

		[ISystem.Update.B(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Body.Data body, [Source.Owned] ref Health.Data health, [Source.Owned] ref Control.Data control,
		[Source.Owned] ref Driller.Data driller, [Source.Owned, Pair.Component<Driller.Data>, Optional(true)] ref Animated.Renderer.Data renderer_drill)
		{
			const bool debug_draw = false;

			driller.distance_current.MoveTowards(driller.drill_offset, driller.speed_max * info.DeltaTime);

			var pos_drill_local_base = (driller.direction * driller.distance_current);
			var pos_drill_world = transform.LocalToWorld(pos_drill_local_base);
			var dir_drill_world = transform.LocalToWorldDirection(driller.direction);
			//var pos_drill_world_start = pos_drill_world - (dir_drill_world * driller.distance_current);
			//var pos_drill_world_start = pos_drill_world - (dir_drill_world * driller.thickness);
			//var pos_drill_world_end = pos_drill_world;

			var pos_drill_world_start = pos_drill_world - (dir_drill_world * driller.distance_current);
			var pos_drill_world_end = pos_drill_world;


#if SERVER
			if (debug_draw) region.DrawDebugLine(pos_drill_world_start, pos_drill_world_end, color: Color32BGRA.Magenta.WithAlpha(32), thickness: driller.thickness * App.pixels_per_unit_f32 * 8);
#endif



			var time = info.WorldTime;
			if (time >= driller.t_next_update)
			{
				ref var terrain = ref region.GetTerrain();

				//WorldNotification.Push(ref region, "drill", color: Color32BGRA.Yellow, position: pos_drill_world, velocity: dir_drill_world);

				Span<LinecastResult> results = stackalloc LinecastResult[16];
				if (region.TryLinecastAll(world_position_start: pos_drill_world_start, world_position_end: pos_drill_world_end,
				radius: driller.thickness, hits: ref results, mask: Physics.Layer.World, exclude: Physics.Layer.Dynamic))
				{
					results.SortByDistance();

					var parent = body.GetParent();

					var damage_base = driller.power;
					var damage_bonus = 0.00f; // random.NextFloatRange(0.00f, melee.damage_bonus);
					var damage = damage_base + damage_bonus;

					var flags = Damage.Flags.No_Loot_Notification | Damage.Flags.No_Loot_Discard;

					var modifier = 1.00f;
					var hit_terrain = false;

					//#if SERVER
					//					region.DrawDebugLine(pos_drill_world_start, pos_drill_world_end, color: Color32BGRA.Magenta, thickness: driller.thickness * App.pixels_per_unit_f32 * 8);
					//#endif

					var dist_terrain = driller.distance_current;

					for (var i = 0; i < results.Length; i++)
					{
						ref var result = ref results[i];
						if (result.entity == entity) continue;

						var h_faction_result = result.GetFactionHandle();
						//if (hit_faction_id != 0 && hit_faction_id == faction.id) continue;

						var is_terrain = !result.entity.IsValid();
						if (is_terrain)
						{
							if (hit_terrain) continue;

							dist_terrain = Vec2f.Distance(pos_drill_world_start, result.world_position);
							hit_terrain = true;
						}

						var material_type = result.material_type;

#if SERVER
						//WorldNotification.Push(ref region, "drill", color: Color32BGRA.Yellow, position: hit.world_position, velocity: dir_drill_world);

						var damage_final = damage * modifier;
						//if (is_terrain) damage_final *= 1.00f; // drill.damage_terrain_multiplier;

						//Damage.Hit(entity, parent, hit.entity, hit.world_position, dir, -dir, damage_final, hit.material_type, Damage.Type.Drill, knockback: 0.25f, size: drill.radius * 1.50f, xp_modifier: 0.80f, flags: flags, yield: 0.90f, primary_damage_multiplier: 1.00f, secondary_damage_multiplier: 1.00f, terrain_damage_multiplier: 1.00f, faction_id: faction.id, speed: 4.00f);

						if (debug_draw) region.DrawDebugLine(pos_drill_world_start, result.world_position, color: Color32BGRA.Yellow, thickness: 8);


						Damage.Hit(ent_attacker: entity, ent_owner: entity, ent_target: result.entity,
							position: result.world_position, velocity: dir_drill_world * 4.00f, normal: result.normal, // -dir_drill_world,
							damage_integrity: damage_final, damage_durability: damage_final, damage_terrain: damage_final,
							target_material_type: result.material_type, damage_type: Damage.Type.Drill,
							yield: driller.yield, size: driller.size * 1.50f, impulse: 100.00f, flags: flags);
#endif

						//flags |= Damage.Flags.No_Sound;
						//modifier *= 0.70f;
						//penetration--;

						//if (heat_state.IsNotNull())
						//{
						//	heat_state.AddEnergy(heat_amount * 0.01f);
						//}
					}

					if (dist_terrain < driller.distance_current) driller.distance_current.LerpFMARef(dist_terrain, 0.30f);
				}
				driller.t_next_update = time + driller.interval;
			}

			//region.DrawDebugLine(pos_drill_world_start, pos_drill_world_end, color: Color32BGRA.Magenta, thickness: driller.thickness * App.pixels_per_unit_f32 * 8);

			if (renderer_drill.IsNotNull())
			{
				renderer_drill.offset = driller.offset.SubY((renderer_drill.sprite.size.y / 16)) + pos_drill_local_base;
			}
		}

		[ISystem.Update.C(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate_Heat(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Body.Data body, [Source.Owned] ref Health.Data health,
		[Source.Owned] ref Driller.Data driller, [Source.Owned] ref Heat.Data heat, [Source.Owned] ref Heat.State heat_state)
		{

		}

		[ISystem.PostUpdate.C(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate_FX(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Body.Data body,
		[Source.Owned] ref Driller.Data driller, [Source.Owned, Pair.Component<Driller.Data>] ref Sound.Emitter sound_emitter)
		{

		}

#if CLIENT
		public struct DrillerGUI: IGUICommand
		{
			public Entity ent_driller;

			public Transform.Data transform;

			public Driller.Data driller;

			public void Draw()
			{
				using (var window = GUI.Window.Interaction("Driller"u8, this.ent_driller))
				{
					this.StoreCurrentWindowTypeID(order: -100);
					if (window.show)
					{
						using (GUI.Group.New(size: GUI.Rm))
						{
							if (GUI.SliderFloat("Depth"u8, value: ref this.driller.drill_offset, min: 0.00f, max: this.driller.distance_max, size: new(128, 32)))
							{
								var depth_ratio = Maths.InvLerp01(0.00f, this.driller.distance_max, this.driller.drill_offset);
								var rpc = new Driller.EditRPC
								{
									depth_ratio = depth_ratio,
								};
								rpc.Send(this.ent_driller);
							}

							//GUI.TextShaded("Derpo"u8);
						}
					}
				}
			}
		}

		[ISystem.GUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI(Entity entity,
		[Source.Owned] in Interactable.Data interactable, [Source.Owned] in Transform.Data transform,
		[Source.Owned] in Driller.Data driller)
		{
			if (interactable.IsActive())
			{
				var gui = new DrillerGUI()
				{
					ent_driller = entity,
					transform = transform,
					driller = driller,
				};
				gui.Submit();
			}
		}
#endif

		//#if CLIENT
		//		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		//		public static void UpdateEffects(ISystem.Info info, ref Region.Data region, ref XorRandom random,
		//		[Source.Owned] in Transform.Data transform,
		//		[Source.Owned] in Driller.Data driller, [Source.Owned] ref Driller.State driller_state,
		//		[Source.Owned, Pair.Component<Driller.State>, Optional(true)] ref Sound.Emitter sound_emitter,
		//		[Source.Owned, Pair.Component<Driller.State>, Optional(true)] ref Light.Data light)
		//		{

		//		}
		//#endif
	}
}
