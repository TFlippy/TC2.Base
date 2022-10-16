
namespace TC2.Base.Components
{
	public static partial class Explosive
	{
		[Flags]
		public enum Flags: uint
		{
			None = 0,

			Primed = 1 << 0,
			Any_Damage = 1 << 1,
			Explode_When_Primed = 1 << 2,
			No_Self_Damage = 1 << 3,
		}

		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct Data: IComponent
		{
			public Entity ent_owner = default;

			[Statistics.Info("Radius", format: "{0:0.##}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.High)]
			public float radius = 2.00f;

			[Statistics.Info("Power", format: "{0:0.##}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.High)]
			public float power = 1.00f;

			[Statistics.Info("Entity Damage", format: "{0:0.##}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Low)]
			public float damage_entity = default;

			[Statistics.Info("Terrain Damage", format: "{0:0.##}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Low)]
			public float damage_terrain = default;

			public Damage.Type damage_type = Damage.Type.Explosion;

			public float fire_amount = 1.00f;

			public float smoke_amount = 1.00f;
			public float smoke_lifetime_multiplier = 1.00f;
			public float smoke_velocity_multiplier = 1.00f;
			public Color32BGRA smoke_color;

			public float flash_duration_multiplier = 1.00f;
			public float flash_intensity_multiplier = 1.00f;

			public float sparks_amount = 0.00f;
			public float volume = 1.00f;
			public float pitch = 1.00f;

			public float modifier = 1.00f;

			[Statistics.Info("Primed Threshold", description: "Explosive becomes primed when its health drops under this value.", format: "{0:P2}", comparison: Statistics.Comparison.None, priority: Statistics.Priority.Low)]
			public float health_threshold = 0.20f;

			public Explosive.Flags flags = default;

			public Data()
			{

			}
		}

#if SERVER
		[ISystem.Event<Health.PostDamageEvent>(ISystem.Mode.Single)]
		public static void OnPostDamage(ISystem.Info info, Entity entity, ref Health.PostDamageEvent data, [Source.Owned] ref Health.Data health, [Source.Owned] ref Explosive.Data explosive)
		{
			if (health.integrity <= explosive.health_threshold)
			{
				if (explosive.flags.HasAny(Explosive.Flags.Any_Damage))
				{
					explosive.flags |= Explosive.Flags.Primed;
				}
				else
				{
					switch (data.damage.damage_type)
					{
						case Damage.Type.Bullet_LC:
						case Damage.Type.Bullet_HC:
						case Damage.Type.Bullet_SG:
						case Damage.Type.Bullet_MG:
						case Damage.Type.Bullet_Musket:
						case Damage.Type.Explosion:
						case Damage.Type.Fire:
						{
							explosive.flags |= Explosive.Flags.Primed;
						}
						break;
					}
				}

				if (explosive.flags.HasAny(Explosive.Flags.Primed))
				{
					if (!explosive.ent_owner.IsValid()) explosive.ent_owner = data.damage.ent_attacker;

					if (explosive.flags.HasAny(Explosive.Flags.Explode_When_Primed))
					{
						entity.RemoveComponent<Explosive.Data>();
					}
				}
			}
		}

		[ISystem.Modified<Resource.Data>(ISystem.Mode.Single)]
		public static void OnResourceModified(ISystem.Info info, [Source.Owned] in Resource.Data resource, [Source.Owned] ref Explosive.Data explosive)
		{
			explosive.modifier = MathF.Sqrt(Maths.NormalizeClamp(resource.quantity, resource.material.GetDefinition().quantity_max));
			//explosive.modifier = Maths.NormalizeClamp(resource.quantity, resource.material.GetDefinition().quantity_max);
			//explosive.modifier = MathF.Log2(resource.quantity / MathF.Max(resource.material.GetDefinition().quantity_max, 1.00f));
			//App.WriteLine(explosive.modifier);
		}

		[ISystem.RemoveLast(ISystem.Mode.Single)]
		public static void OnRemove(ISystem.Info info, Entity entity, [Source.Owned] in Transform.Data transform, [Source.Owned] in Explosive.Data explosive)
		{
			if (explosive.flags.HasAny(Explosive.Flags.Primed))
			{
				//var explosion_tmp = new Explosion.Data()
				//{
				//	power = explosive.power * explosive.modifier,
				//	radius = explosive.radius * explosive.modifier,
				//	damage_entity = explosive.damage_entity * explosive.modifier,
				//	damage_terrain = explosive.damage_terrain * explosive.modifier,
				//	damage_type = explosive.damage_type,
				//	ent_owner = explosive.ent_owner,
				//	fire_amount = explosive.fire_amount,
				//	smoke_amount = explosive.smoke_amount,
				//	smoke_lifetime_multiplier = explosive.smoke_lifetime_multiplier,
				//	smoke_velocity_multiplier = explosive.smoke_velocity_multiplier,
				//	sparks_amount = explosive.sparks_amount,
				//	volume = explosive.volume,
				//	pitch = explosive.pitch,
				//};

				var explosive_tmp = explosive;

				//if (explosive.flags.HasAny(Explosive.Flags.No_Self_Damage)) explosion_tmp.ent_ignored = entity;

				info.GetRegion().SpawnPrefab("explosion", transform.position).ContinueWith(x =>
				{
					ref var explosion = ref x.GetComponent<Explosion.Data>();
					if (!explosion.IsNull())
					{
						explosion.power = Maths.Lerp01(explosive_tmp.power * 0.50f, explosive_tmp.power, explosive_tmp.modifier);
						explosion.radius = Maths.Lerp01(explosive_tmp.radius * 0.25f, explosive_tmp.radius, explosive_tmp.modifier);
						explosion.damage_entity = explosive_tmp.damage_entity * explosive_tmp.modifier;
						explosion.damage_terrain = explosive_tmp.damage_terrain * explosive_tmp.modifier;
						explosion.damage_type = explosive_tmp.damage_type;
						explosion.ent_owner = explosive_tmp.ent_owner;
						explosion.fire_amount = explosive_tmp.fire_amount;
						explosion.smoke_amount = explosive_tmp.smoke_amount;
						explosion.smoke_lifetime_multiplier = explosive_tmp.smoke_lifetime_multiplier;
						explosion.smoke_velocity_multiplier = explosive_tmp.smoke_velocity_multiplier;
						explosion.flash_duration_multiplier = explosive_tmp.flash_duration_multiplier;
						explosion.flash_intensity_multiplier = explosive_tmp.flash_intensity_multiplier;
						explosion.sparks_amount = explosive_tmp.sparks_amount;
						explosion.volume = explosive_tmp.volume;
						explosion.pitch = explosive_tmp.pitch;

						if (explosive_tmp.flags.HasAny(Explosive.Flags.No_Self_Damage)) explosion.ent_ignored = entity;

						explosion.Sync(x);
					}
				});
			}
		}
#endif
	}
}
