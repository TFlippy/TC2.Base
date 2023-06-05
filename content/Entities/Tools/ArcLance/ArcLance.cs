namespace TC2.Base.Components
{
	public static partial class ArcLance
	{
		[IComponent.Data(Net.SendType.Reliable)]
		public partial struct Data: IComponent
		{
			public Sound.Handle sound_hit = ArcLance.sound_hit_default;

			public float damage_integrity = 400.00f;
			public float damage_durability = 4000.00f;
			public float damage_terrain = 200.00f;

			public Data()
			{

			}
		}

		public static readonly Sound.Handle sound_hit_default = "arcane_infuser.fizzle.00";

		public static readonly Texture.Handle tex_blank = "blank";
		public static readonly Texture.Handle tex_light = "light.circle.00";
		public static readonly Texture.Handle tex_smoke = "BiggerSmoke_Light";
		public static readonly Texture.Handle tex_spark = "metal_spark.01";

		[ISystem.Event<Melee.HitEvent>(ISystem.Mode.Single)]
		public static void OnHit(ISystem.Info info, Entity entity, ref Region.Data region, ref XorRandom random,
		ref Melee.HitEvent data, [Source.Owned] ref ArcLance.Data arc_lance)
		{
#if CLIENT
			var dir = data.direction;

			for (var i = 0; i < 40; i++)
			{
				Particle.Spawn(ref region, new Particle.Data()
				{
					texture = tex_blank,
					lifetime = random.NextFloatRange(0.10f, 1.00f),
					pos = data.world_position + random.NextUnitVector2Range(0.10f, 0.20f),
					vel = dir.RotateByRad(random.NextFloatRange(-2.50f, 2.50f)) * random.NextFloatRange(0, 80),
					fps = 0,
					scale = random.NextFloatRange(0.50f, 1.50f),
					growth = -random.NextFloatRange(1.60f, 4.00f),
					force = random.NextUnitVector2Range(0, 80),
					drag = random.NextFloatRange(0.00f, 0.40f),
					stretch = new Vector2(random.NextFloatRange(0.50f, 1.50f), 0.05f),
					color_a = random.NextColor32Range(0xffffffff, 0xffc89eff),
					color_b = random.NextColor32Range(0xffc89eff, 0xffffffff),
					lit = random.NextFloatRange(0.50f, 1.00f),
					face_dir_ratio = 1.00f
				});
			}

			for (var i = 0; i < 4; i++)
			{
				Particle.Spawn(ref region, new Particle.Data()
				{
					texture = tex_light,
					lifetime = random.NextFloatRange(0.01f, 0.10f),
					pos = data.world_position + random.NextVector2(0.10f),
					vel = dir.RotateByRad(random.NextFloatRange(-2.00f, 2.00f)) * random.NextFloatRange(10.00f, 40.00f),
					force = dir.RotateByRad(random.NextFloatRange(-0.20f, 0.20f)) * random.NextFloatRange(0, 200),
					scale = random.NextFloatRange(2.00f, 30.00f),
					rotation = random.NextFloatRange(-3.50f, 3.50f),
					angular_velocity = random.NextFloat(0.50f),
					growth = random.NextFloatRange(10.00f, 200.00f),
					drag = random.NextFloatRange(0.07f, 0.25f),
					color_a = random.NextColor32Range(0xffffffff, 0xffc89eff),
					color_b = random.NextColor32Range(0xffc89eff, 0xffffffff),
					glow = random.NextFloatRange(2.00f, 8.00f),
				});
			}

			for (var i = 0; i < 4; i++)
			{
				Particle.Spawn(ref region, new Particle.Data()
				{
					texture = tex_smoke,
					lifetime = random.NextFloatRange(3.00f, 5.00f),
					pos = data.world_position + random.NextVector2(1.00f),
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

			for (var i = 0; i < 20; i++)
			{
				Particle.Spawn(ref region, new Particle.Data()
				{
					texture = tex_spark,
					lifetime = random.NextFloatRange(1.00f, 2.00f),
					pos = data.world_position + random.NextVector2(0.20f),
					vel = (dir * random.NextFloatRange(0, 40)).RotateByRad(random.NextFloatRange(-1.15f, 1.15f)) + random.NextVector2(15),
					fps = 0,
					frame_count = 1,
					frame_offset = random.NextByteRange(0, 4),
					frame_count_total = 4,
					scale = random.NextFloatRange(0.20f, 0.60f),
					growth = -random.NextFloatRange(0.60f, 1.00f),
					force = new Vector2(0.00f, random.NextFloatRange(20, 80)),
					drag = random.NextFloatRange(0.01f, 0.10f),
					stretch = new Vector2(random.NextFloatRange(1.00f, 2.00f), 0.75f),
					color_a = random.NextColor32Range(0xffffffff, 0xffc89eff),
					color_b = random.NextColor32Range(0xffffc0b0, 0xffffa090),
					lit = 1.00f,
					face_dir_ratio = 1.00f,
				});
			}
#endif

#if SERVER
			Sound.Play(ref region, arc_lance.sound_hit, data.world_position, volume: 1.00f, pitch: random.NextFloatRange(0.70f, 0.85f), size: 1.00f);
			Shake.Emit(ref region, data.world_position, 0.50f, 0.80f, 20.00f);

			var multiplier = 0.50f + (MathF.Pow(random.NextFloatRange(0.00f, 1.00f), 2.00f) * 0.50f);

			Damage.Hit(ent_attacker: entity, ent_owner: data.ent_owner, ent_target: data.ent_target,
				position: data.world_position, velocity: data.direction * 8.00f, normal: -data.direction,
				damage_integrity: arc_lance.damage_integrity * multiplier, damage_durability: arc_lance.damage_durability * multiplier, damage_terrain: arc_lance.damage_terrain * multiplier,
				target_material_type: data.target_material_type, damage_type: Damage.Type.Electricity,
				yield: 0.95f, size: 1.50f, impulse: 0.00f);


			// Zap the holder too when hitting a metallic object
			if (data.target_material_type == Material.Type.Metal && data.ent_target.IsAlive())
			{
				var ent_holder_arm = data.ent_target.GetParent(Relation.Type.Child);
				if (ent_holder_arm.IsAlive())
				{
					App.WriteLine($"{ent_holder_arm.GetFullName()}");

					var oc_body = ent_holder_arm.GetComponentWithOwner<Body.Data>(Relation.Type.Instance, false);
					if (oc_body.IsValid())
					{
						ref var body_holder = ref oc_body.data;
						var result = body_holder.GetClosestPoint(data.world_position, true);

						var impulse_mult = 1.00f;
						if (result.material_type == Material.Type.Flesh || result.material_type == Material.Type.Insect)
						{
							impulse_mult *= 6.00f;

							if (random.NextBool(0.50f))
							{
								Arm.Drop(ent_holder_arm, target: data.ent_target, direction: -data.direction);
							}
						}

						Damage.Hit(ent_attacker: entity, ent_owner: data.ent_owner, ent_target: result.entity,
							position: result.world_position, velocity: data.direction * 4.00f, normal: -data.direction,
							damage_integrity: arc_lance.damage_integrity * multiplier * 0.25f, damage_durability: arc_lance.damage_durability * multiplier * 0.35f, damage_terrain: arc_lance.damage_terrain * multiplier * 0.25f,
							target_material_type: result.material_type, damage_type: Damage.Type.Electricity, pain: 1.10f,
							yield: 0.95f, size: 1.00f, impulse: 100.00f * impulse_mult);
					}
				}
			}


			//if (random.NextBool(0.10f))
			//{
			//	Fire.Ignite(data.ent_target, random.NextFloatRange(1.00f, 4.00f), Fire.Flags.No_Radius_Ignite);
			//}
#endif

		}
	}
}
