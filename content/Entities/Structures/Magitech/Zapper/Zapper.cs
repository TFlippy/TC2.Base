
namespace TC2.Base.Components
{
	public static partial class Zapper
	{
		[IComponent.Data(Net.SendType.Reliable, region_only: true), IComponent.With<Zapper.State>]
		public partial struct Data(): IComponent
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0u,
			}

			[Editor.Picker.Position(true)] public Vector2 offset_a;
			[Editor.Picker.Position(true)] public Vector2 offset_b;

			public Control.Bind bind_activate;

			public Zapper.Data.Flags flags;
			public float damage = 1000.00f;
			public float charge_rate = 1.00f;

			public Sound.Handle sound_on = Zapper.sound_on_default;
			public Sound.Handle sound_off = Zapper.sound_off_default;
		}

		[IComponent.Data(Net.SendType.Unreliable, region_only: true)]
		public partial struct State(): IComponent
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0u,

				Powered = 1u << 0
			}

			public float charge_current;
			public Zapper.State.Flags flags;
			[Save.Ignore, Net.Ignore] public float next_hit;
			[Net.Ignore, Asset.Ignore, Save.Ignore] private uint unused_00;
		}

		[IEvent.Data]
		public partial struct HitEvent(): IEvent
		{
			public Entity ent_attacker;
			public Entity ent_target;
			public Entity ent_owner;
			public Vector2 pos_a;
			public Vector2 pos_b;
		}

		public static readonly Sound.Handle sound_on_default = "arcane_infuser.on.00";
		public static readonly Sound.Handle sound_off_default = "arcane_infuser.off.00";

		public static Texture.Handle tex_blank = "blank";
		public static Texture.Handle tex_light = "light.circle.04";
		public static Texture.Handle tex_light_box = "light.box.00";
		public static Texture.Handle tex_smoke = "BiggerSmoke_Light";
		public static Texture.Handle tex_spark = "metal_spark.01";

		public static Sound.Handle[] sounds_hit =
		{
			"zapper.zap.00",
			"zapper.zap.01",
			"zapper.zap.02"
		};

		public struct EditRPC: Net.IRPC<Zapper.Data>
		{
			public Control.Bind? bind_activate;

#if SERVER
			public void Invoke(Net.IRPC.Context rpc, ref Zapper.Data data)
			{
				var sync = false;

				sync |= this.bind_activate.TryCopyTo(ref data.bind_activate);

				if (sync)
				{
					data.Sync(rpc.entity, true);
				}
			}
#endif
		}

		[ISystem.Event<Zapper.HitEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnHit(ref XorRandom random, ref Region.Data region, ISystem.Info info, Entity entity, ref Zapper.HitEvent data,
		[Source.Owned] ref Zapper.Data zapper, [Source.Owned] ref Zapper.State zapper_state, [Source.Owned] ref Transform.Data transform)
		{
			var damage = zapper.damage * random.NextFloatRange(0.90f, 1.40f);

			Span<OverlapResult> results = stackalloc OverlapResult[16];
			if (region.TryOverlapPointAll(data.pos_b, 2.00f, ref results, mask: Physics.Layer.Flammable | Physics.Layer.Creature | Physics.Layer.Organic, exclude: Physics.Layer.Static | Physics.Layer.Resource, require: Physics.Layer.Dynamic))
			{
#if CLIENT
				var spark_count = 10;

				Sound.Play(ref region, Zapper.sounds_hit.GetRandom(ref random), data.pos_a, volume: 1.50f, pitch: random.NextFloatRange(0.80f, 1.15f), size: 1.00f);
				Sound.Play(ref region, Arcer.sound_hit_default, data.pos_b, volume: 1.20f, pitch: random.NextFloatRange(0.90f, 1.15f), size: 1.50f);
				Shake.Emit(ref region, data.pos_b, 0.75f, 0.80f, 100.00f);
#endif

				foreach (ref var result in results)
				{
					var dir = (result.world_position - data.pos_a).GetNormalized(out var dist);

#if CLIENT
					//var dir = data.direction;
					var step = dist / spark_count;

					if (Camera.IsVisible(Camera.CullType.Rect4x, result.world_position))
					{
						for (var i = 0; i < spark_count; i++)
						{
							Particle.Spawn(ref region, new Particle.Data()
							{
								texture = tex_light_box,
								lifetime = random.NextFloatRange(0.05f, 0.20f),
								pos = data.pos_a + (dir * step * i) + random.NextUnitVector2Range(0.00f, 0.10f),
								vel = (dir + random.NextUnitVector2Range(0.00f, 0.50f)) * (i * step * 4),
								fps = 0,
								scale = random.NextFloatRange(0.50f, 2.00f),
								growth = random.NextFloatRange(0.50f, 6.00f),
								force = random.NextUnitVector2Range(0, 20) + (dir * random.NextFloatRange(10, 40)),
								drag = 0.05f,
								stretch = new Vector2(random.NextFloatRange(0.50f, 2.00f), random.NextFloatRange(0.10f, 0.30f)),
								color_a = random.NextColor32Range(0xffffffff, 0xffc89eff),
								color_b = random.NextColor32Range(0xffc89eff, 0xff2080ff),
								glow = random.NextFloatRange(30.00f, 60.00f),
								//lit = random.NextFloatRange(0.00f, 1.00f),
								face_dir_ratio = random.NextFloatRange(0.50f, 1.00f)
							});
						}

						for (var i = 0; i < 40; i++)
						{
							Particle.Spawn(ref region, new Particle.Data()
							{
								texture = tex_blank,
								lifetime = random.NextFloatRange(0.10f, 0.50f),
								pos = result.world_position,
								vel = dir.RotateByRad(random.NextFloatRange(-0.50f, 0.50f)) * random.NextFloatRange(-5, 20),
								fps = 0,
								scale = random.NextFloatRange(0.80f, 1.90f),
								growth = -random.NextFloatRange(0.50f, 1.00f),
								force = random.NextUnitVector2Range(0, 40),
								drag = random.NextFloatRange(0.00f, 0.40f),
								angular_velocity = random.NextFloatRange(-0.50f, 0.50f),
								stretch = new Vector2(random.NextFloatRange(0.50f, 1.50f), 0.05f),
								color_a = random.NextColor32Range(0xffffffff, 0xffc89eff),
								color_b = random.NextColor32Range(0xffc89eff, 0xffffffff),
								lit = 1.00f,
								face_dir_ratio = 1.00f
							});
						}

						for (var i = 0; i < 4; i++)
						{
							Particle.Spawn(ref region, new Particle.Data()
							{
								texture = tex_light,
								lifetime = random.NextFloatRange(0.05f, 0.10f),
								pos = Vector2.Lerp(data.pos_a, result.world_position, i * 0.25f),
								vel = dir.RotateByRad(random.NextFloatRange(-2.00f, 2.00f)) * random.NextFloatRange(10.00f, 40.00f),
								force = dir.RotateByRad(random.NextFloatRange(-0.20f, 0.20f)) * random.NextFloatRange(0, 200),
								scale = random.NextFloatRange(15.00f, 30.00f),
								rotation = random.NextFloatRange(-3.50f, 3.50f),
								angular_velocity = random.NextFloat(0.50f),
								growth = random.NextFloatRange(100.00f, 200.00f),
								drag = random.NextFloatRange(0.07f, 0.25f),
								color_a = random.NextColor32Range(0xffffffff, 0xffc89eff),
								color_b = random.NextColor32Range(0xffc89eff, 0xffffffff),
								glow = random.NextFloatRange(4.00f, 8.00f),
							});
						}

						for (var i = 0; i < 4; i++)
						{
							Particle.Spawn(ref region, new Particle.Data()
							{
								texture = tex_smoke,
								lifetime = random.NextFloatRange(3.00f, 5.00f),
								pos = data.pos_b + random.NextVector2(1.00f),
								vel = random.NextUnitVector2Range(0.50f * i, 1.50f * i) + random.NextVector2(5.00f),
								fps = random.NextByteRange(5, 10),
								frame_count = 64,
								frame_count_total = 64,
								frame_offset = random.NextByteRange(0, 64),
								//scale = random.NextFloatRange(1.50f, 2.50f),
								scale = random.NextFloatRange(0.50f, 0.80f),
								rotation = random.NextFloat(10.00f),
								angular_velocity = random.NextFloat(1.00f),
								growth = random.NextFloatRange(0.15f, 0.20f),
								drag = random.NextFloatRange(0.02f, 0.04f),
								force = new Vector2(0.00f, random.NextFloatRange(0, -2)),
								color_a = random.NextColor32Range(0x80ffffff, 0xa0ffffff),
								color_b = random.NextColor32Range(0x00ffffff, 0x00808080)
							});
						}

						for (var i = 0; i < 5; i++)
						{
							Particle.Spawn(ref region, new Particle.Data()
							{
								texture = tex_spark,
								lifetime = random.NextFloatRange(1.00f, 2.00f),
								pos = data.pos_b,
								vel = (dir * random.NextFloatRange(0, 20)).RotateByRad(random.NextFloatRange(-2.00f, 2.00f)) + random.NextVector2(15),
								fps = 0,
								frame_count = 1,
								frame_offset = random.NextByteRange(0, 4),
								frame_count_total = 4,
								scale = random.NextFloatRange(0.20f, 0.60f),
								growth = -random.NextFloatRange(0.60f, 1.00f),
								force = new Vector2(0.00f, random.NextFloatRange(10, 40)),
								drag = random.NextFloatRange(0.01f, 0.10f),
								stretch = new Vector2(random.NextFloatRange(1.00f, 2.00f), 0.75f),
								color_a = random.NextColor32Range(0xffffffff, 0xffc89eff),
								color_b = random.NextColor32Range(0xffffc0b0, 0xffffa090),
								lit = 1.00f,
								face_dir_ratio = 1.00f,
							});
						}
					}
#endif

#if SERVER
					Damage.Hit(ent_attacker: entity, ent_owner: default, ent_target: result.entity,
						position: result.world_position, velocity: random.NextUnitVector2Range(2.00f, 8.00f) + (dir * random.NextFloatRange(4.00f, 6.00f)), normal: -dir,
						damage_integrity: damage, damage_durability: damage, damage_terrain: 0.00f,
						target_material_type: result.material_type, damage_type: Damage.Type.Electricity,
						yield: 0.50f, size: 1.50f, impulse: random.NextFloatRange(1000.00f, 1800.00f), flags: Damage.Flags.No_Loot_Pickup | Damage.Flags.No_Loot_Notification);


					if (random.NextBool(0.10f))
					{
						Fire.Ignite(ent_target: result.entity, intensity: random.NextFloatExtra(1.00f, 4.00f), flags: Fire.Flags.No_Radius_Ignite);
					}
#endif
				}
			}

			//if (random.NextBool(0.10f))
			//{
			//	Fire.Ignite(data.ent_target, random.NextFloatRange(1.00f, 4.00f), Fire.Flags.No_Radius_Ignite);
			//}
			//#endif

		}

#if SERVER
		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void Update(ISystem.Info info, Entity entity, ref Region.Data region, ref XorRandom random,
		[Source.Owned] ref Zapper.Data zapper, [Source.Owned] ref Zapper.State zapper_state,
		[Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control)
		{
			var sync = false;

			if (zapper.bind_activate.GetKeyDown(in control))
			{
				var is_powered = zapper_state.flags.HasAll(Zapper.State.Flags.Powered);
				zapper_state.flags.SetFlag(Zapper.State.Flags.Powered, !is_powered);

				if (is_powered)
				{
					Sound.Play(ref region, zapper.sound_off, transform.position);
				}
				else
				{
					Sound.Play(ref region, zapper.sound_on, transform.position);
				}

				sync = true;
			}

			if (sync)
			{
				zapper_state.Sync(entity, true);
			}
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate(ISystem.Info info, Entity entity, ref XorRandom random,
		[Source.Owned] ref Zapper.Data zapper, [Source.Owned] ref Zapper.State zapper_state,
		[Source.Owned] ref Body.Data body, [Source.Owned] in Transform.Data transform)
		{
			//var ts = Timestamp.Now();
			if (zapper_state.flags.HasAny(Zapper.State.Flags.Powered))
			{
				if (info.WorldTime >= zapper_state.next_hit)
				{
					if (body.HasArbiters())
					{
						var hit = false;

						foreach (var arbiter in body.GetArbiters())
						{
							//if (hit)
							//{
							//	break;
							//}
							//else

							if (arbiter.GetState() == Body.Arbiter.State.Normal)
							{
								var material_type = arbiter.GetMaterial();

								var ent_target = arbiter.GetEntity();
								//App.WriteLine($"{ent_target} {ent_target.GetFullName()}; {material_type}");

								switch (material_type)
								{
									case Material.Type.Flesh:
									case Material.Type.Insect:
									case Material.Type.Metal:
									{
										if (ent_target.IsValid())
										{
											var ev = new Zapper.HitEvent();
											ev.ent_attacker = entity;
											ev.ent_owner = entity;
											ev.ent_target = ent_target;
											ev.pos_a = transform.LocalToWorld(Vector2.Lerp(zapper.offset_a, zapper.offset_b, random.NextFloat01()));
											ev.pos_b = arbiter.GetContactPosition(0);
											entity.TriggerEventDeferred(ev, sync: true);

											hit = true;
										}
									}
									break;

									//default:
									//{
									//	arbiter.SetIgnored();
									//}
									//break;
								}
							}
						}

						if (hit)
						{
							zapper_state.next_hit = info.WorldTime + random.NextFloatRange(0.75f, 1.50f);
						}
					}
					//App.WriteLine($"{ts.GetMilliseconds():0.0000} ms");
				}
			}
		}
#endif

#if CLIENT
		[ISystem.VeryEarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateEffects(ref Region.Data region, ref XorRandom random, ISystem.Info info, Entity entity,
		[Source.Owned] ref Zapper.Data zapper, [Source.Owned] ref Zapper.State zapper_state,
		[Source.Owned] ref Transform.Data transform,
		[Source.Owned, Pair.Component<Zapper.Data>] ref Light.Data light, [Source.Owned, Pair.Component<Zapper.Data>, Optional(true)] ref Sound.Emitter sound_emitter)
		{
			if (!zapper_state.flags.HasAny(Zapper.State.Flags.Powered))
			{
				light.intensity = 0.00f;

				if (sound_emitter.IsNotNull())
				{
					sound_emitter.volume_mult = 0.00f;
				}
			}
			else
			{
				light.intensity = random.NextFloatRange(1.00f, 1.50f);
				light.rotation = random.NextFloatRange(-0.10f, 0.10f);

				if (sound_emitter.IsNotNull())
				{
					sound_emitter.volume_mult = 1.00f;
				}

				if (random.NextBool(0.004f))
				{
					var spark_count = 5;
					var dir = transform.GetDirection();
					var step = 1.00f;
					var pos = transform.LocalToWorld(Vector2.Lerp(zapper.offset_a, zapper.offset_b, random.NextFloatRange(0.00f, 1.00f)));

					//for (var i = 0; i < spark_count; i++)
					//{
					//	Particle.Spawn(ref region, new Particle.Data()
					//	{
					//		texture = tex_light_box,
					//		lifetime = random.NextFloatRange(0.05f, 0.20f),
					//		pos = pos + (dir * step * i) + random.NextUnitVector2Range(0.00f, 0.10f),
					//		vel = (dir + random.NextUnitVector2Range(0.00f, 0.50f)) * (i * step * 4),
					//		fps = 0,
					//		scale = random.NextFloatRange(0.50f, 2.00f),
					//		growth = random.NextFloatRange(0.50f, 6.00f),
					//		force = random.NextUnitVector2Range(0, 20) + (dir * random.NextFloatRange(10, 40)),
					//		drag = 0.05f,
					//		stretch = new Vector2(random.NextFloatRange(0.50f, 2.00f), random.NextFloatRange(0.10f, 0.30f)),
					//		color_a = random.NextColor32Range(0xffffffff, 0xffc89eff),
					//		color_b = random.NextColor32Range(0xffc89eff, 0xff2080ff),
					//		glow = random.NextFloatRange(30.00f, 60.00f),
					//		//lit = random.NextFloatRange(0.00f, 1.00f),
					//		face_dir_ratio = random.NextFloatRange(0.50f, 1.00f)
					//	});
					//}

					Sound.Play("essence.discharge.03", pos, volume: random.NextFloatRange(0.80f, 1.10f), pitch: random.NextFloatRange(2.50f, 3.50f));

					Shake.Emit(ref region, pos, 0.50f, 0.65f, 40.00f);

					for (var i = 0; i < 15; i++)
					{
						Particle.Spawn(ref region, new Particle.Data()
						{
							texture = tex_blank,
							lifetime = random.NextFloatRange(0.10f, 0.80f),
							pos = pos + random.NextUnitVector2Range(0.00f, 0.50f),
							vel = random.NextUnitVector2Range(2.00f, 10.00f) + (region.GetGravity() * random.NextFloatRange(0, 1)),
							fps = 0,
							scale = random.NextFloatRange(0.40f, 1.00f),
							growth = -random.NextFloatRange(0.50f, 1.00f),
							force = random.NextUnitVector2Range(0, 10) + region.GetGravity(),
							drag = random.NextFloatRange(0.00f, 0.20f),
							angular_velocity = random.NextFloatRange(-0.10f, 0.10f),
							stretch = new Vector2(random.NextFloatRange(0.50f, 1.50f), 0.05f),
							color_a = random.NextColor32Range(0xffffffff, 0xffc89eff),
							color_b = random.NextColor32Range(0xffc89eff, 0xffffffff),
							lit = 1.00f,
							face_dir_ratio = 1.00f
						});
					}

					for (var i = 0; i < 1; i++)
					{
						Particle.Spawn(ref region, new Particle.Data()
						{
							texture = tex_light,
							lifetime = random.NextFloatRange(0.05f, 0.10f),
							pos = pos,
							vel = dir.RotateByRad(random.NextFloatRange(-2.00f, 2.00f)) * random.NextFloatRange(10.00f, 40.00f),
							force = dir.RotateByRad(random.NextFloatRange(-0.20f, 0.20f)) * random.NextFloatRange(0, 200),
							scale = random.NextFloatRange(15.00f, 30.00f),
							rotation = random.NextFloatRange(-3.50f, 3.50f),
							angular_velocity = random.NextFloat(0.50f),
							growth = random.NextFloatRange(100.00f, 200.00f),
							drag = random.NextFloatRange(0.07f, 0.25f),
							color_a = random.NextColor32Range(0xffffffff, 0xffc89eff),
							color_b = random.NextColor32Range(0xffc89eff, 0xffffffff),
							glow = random.NextFloatRange(4.00f, 8.00f),
						});
					}
				}
			}
		}

		public partial struct ZapperGUI: IGUICommand
		{
			public Entity ent_zapper;
			public Zapper.Data zapper;

			public void Draw()
			{
				using (var window = GUI.Window.Interaction("Zapper"u8, this.ent_zapper))
				{
					this.StoreCurrentWindowTypeID();
					if (window.show)
					{
						var rpc = new Zapper.EditRPC();
						var sync = false;

						var bind_activate_tmp = this.zapper.bind_activate;

						using (var group = GUI.Group.New(size: new Vector2(GUI.RmX, 48), padding: new Vector2(4, 4)))
						{
							GUI.DrawBackground(GUI.tex_panel, group.GetOuterRect(), new(4));

							if (GUI.EnumInput("bind.key", ref bind_activate_tmp.keyboard, new Vector2(GUI.RmX, 40), show_label: false))
							{
								rpc.bind_activate = bind_activate_tmp;
								sync = true;
							}
							GUI.DrawHoverTooltip("Keyboard");
						}

						using (var group = GUI.Group.New(size: new Vector2(GUI.RmX, 48), padding: new Vector2(4, 4)))
						{
							GUI.DrawBackground(GUI.tex_panel, group.GetOuterRect(), new(4));

							if (GUI.EnumInput("bind.mouse", ref bind_activate_tmp.mouse, new Vector2(GUI.RmX, 40), show_label: false))
							{
								rpc.bind_activate = bind_activate_tmp;
								sync = true;
							}
							GUI.DrawHoverTooltip("Mouse");
						}

						if (sync)
						{
							rpc.Send(this.ent_zapper);
						}
					}
				}
			}
		}

		[ISystem.EarlyGUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI(Entity entity, [Source.Owned] in Zapper.Data zapper,
		[Source.Owned] in Interactable.Data interactable)
		{
			if (interactable.IsActive())
			{
				var gui = new ZapperGUI()
				{
					ent_zapper = entity,
					zapper = zapper
				};
				gui.Submit();
			}
		}
#endif
	}
}
