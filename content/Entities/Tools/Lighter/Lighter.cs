namespace TC2.Base.Components
{
	public static class Lighter
	{
		[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Region)]
		public struct Data(): IComponent
		{
			[Flags]
			public enum Flags: ushort
			{
				None = 0,
			}

			public Lighter.Data.Flags flags;
			public Sound.Handle h_sound_use;

			[Save.Force] public required float cooldown;
			[Save.Force] public required float distance_max;
			[Save.Force] public required float damage;
			[Save.Force] public required float intensity;

			[Editor.Picker.Position(relative: true)] public Vec2f offset;

			[Net.Ignore, Save.Ignore] public float t_next_use;
		}

		[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasRelation(Source.Modifier.Owned, Relation.Type.Stored, false)]
		public static void Update(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity ent_lighter,
		[Source.Owned] ref Lighter.Data lighter, [Source.Owned] in Transform.Data transform,
		[Source.Owned] in Control.Data control, [Source.Owned] ref Body.Data body)
		{
			if (control.mouse.GetKeyDown(Mouse.Key.Left) && info.WorldTime >= lighter.t_next_use)
			{
				var pos_a = transform.LocalToWorld(lighter.offset);
				var pos_b = control.mouse.position;

				if (pos_b.IsInDistance(pos_a, lighter.distance_max)
				&& region.IsInLineOfSight(pos_a: pos_a, pos_b: pos_b, radius: 0.000f, mask: Physics.Layer.World, layer: Physics.Layer.Solid, require: Physics.Layer.Static, query_flags: Physics.QueryFlag.Static))
				{
#if CLIENT
					if (lighter.h_sound_use)
					{
						for (var i = 0; i < 4; i++)
						{
							Particle.Spawn(ref region, new Particle.Data()
							{
								texture = Geyser.texture_metal_spark,
								lifetime = random.NextFloatRange(0.20f, 0.50f),
								pos = pos_a,
								vel = new Vector2(random.NextFloat(4), random.NextFloatRange(-4, 5)),
								fps = 16,
								frame_count = 4,
								frame_offset = random.NextByteRange(0, 4),
								frame_count_total = 4,
								scale = random.NextFloatRange(0.60f, 0.90f),
								growth = -random.NextFloatRange(1.60f, 2.00f),
								force = new Vector2(0, random.NextFloatRange(5, 10)),
								drag = random.NextFloatRange(0.01f, 0.03f),
								stretch = new Vector2(random.NextFloatRange(1.00f, 1.50f), 0.75f),
								color_a = random.NextColor32Range(0xffffffff, 0xffffc0b0),
								color_b = random.NextColor32Range(0xffffc0b0, 0xffffa090),
								lit = 1.00f,
								face_dir_ratio = 1.00f,
							});
						}

						//if (random.NextBool(0.90f))
						{
							Particle.Spawn(ref region, new Particle.Data()
							{
								texture = Light.tex_light_fire_00,
								lifetime = random.NextFloatRange(0.10f, 0.25f),
								pos = pos_a + random.NextVector2(0.125f),
								vel = random.NextVector2(0.03f),
								scale = random.NextFloatRange(1.00f, 1.50f),
								angular_velocity = random.NextFloat(0.05f),
								growth = random.NextFloatRange(1.50f, 3.50f),
								rotation = random.NextFloatRange(-10, 10),
								stretch = new Vector2(random.NextFloatRange(0.90f, 1.10f), random.NextFloatRange(0.90f, 1.10f)),
								color_a = ColorBGRA.RGBA(1.00f, 0.70f, 0.40f, random.NextFloatRange(10.00f, 30.00f)),
								color_b = ColorBGRA.RGBA(0.20f, 0.00f, 0.00f, 0.00f),
								glow = 1.00f
							});
						}

						Sound.Play(region: ref region, sound: lighter.h_sound_use, world_position: pos_a, volume: 0.60f, pitch: random.NextFloatExtra(0.90f, 0.30f));
					}
#endif

#if SERVER
					if (random.NextBool(0.33f) && region.TryOverlapPoint(world_position: pos_b, radius: 0.125f, result: out var result, mask: Physics.Layer.Flammable))
					{
						Fire.Ignite(ent_target: result.entity, intensity: lighter.intensity, flags: Fire.Flags.No_Radius_Ignite, damage: lighter.damage, step: lighter.intensity * random.NextFloatExtra(0.20f, 0.40f));
					}
#endif
				}

				lighter.t_next_use = info.WorldTime + lighter.cooldown;
			}
		}
	}
}
