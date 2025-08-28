
namespace TC2.Base.Components
{
	public static partial class Explosive
	{
		[Flags]
		public enum Flags: byte
		{
			None = 0,

			Primed = 1 << 0,
			Any_Damage = 1 << 1,
			Explode_When_Primed = 1 << 2,
			No_Self_Damage = 1 << 3,
			Repeatable = 1 << 4,
			Primed_When_Destroyed = 1 << 5,
		}

		[IEvent.Data]
		public struct ExplodeEvent(): IEvent, IEvent<Crafting.ConfiguredRecipe>, IDrawable<Crafting.ConfiguredRecipe>
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0,

			}

			public Explosive.Data explosive;

			void IEvent<Crafting.ConfiguredRecipe>.Bind(ref Crafting.ConfiguredRecipe arg)
			{

			}

#if CLIENT
			void IDrawable<Crafting.ConfiguredRecipe>.OnDraw(ref Crafting.ConfiguredRecipe arg, GUI.Group group)
			{

			}
#endif
		}

		[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Region)]
		public partial struct Data(): IComponent
		{
			public static readonly BitField<Damage.Type> detonate_damage_filter_default = new BitField<Damage.Type>
			(
				Damage.Type.Bullet_LC,
				Damage.Type.Bullet_HC,
				Damage.Type.Bullet_SG,
				Damage.Type.Bullet_MG,
				Damage.Type.Bullet_AC,
				Damage.Type.Bullet_KN,
				Damage.Type.Bullet_HW,
				Damage.Type.Bullet_Musket,
				Damage.Type.Rivet,
				Damage.Type.Club,
				Damage.Type.Crush,
				Damage.Type.Impact,
				Damage.Type.Motion_Impulse,
				Damage.Type.Axe,
				Damage.Type.Saw,
				Damage.Type.Shrapnel,
				Damage.Type.Stress,
				Damage.Type.Fracture,
				Damage.Type.Electricity,
				Damage.Type.Pickaxe,
				Damage.Type.Explosion,
				Damage.Type.Fire,
				Damage.Type.Phlogiston,
				Damage.Type.Deflagration,
				Damage.Type.Sledgehammer,
				Damage.Type.Shockwave
			);

			[Statistics.Info("Radius", description: "Size of the explosion.", format: "{0:0.##}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.High)]
			[Save.Force] public float radius = 2.00f;

			[Statistics.Info("Power", description: "Strength of the explosion.", format: "{0:0.##}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.High)]
			[Save.Force] public float power = 1.00f;

			[Statistics.Info("Entity Damage", format: "{0:0.##}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Low)]
			[Save.Force] public float damage_entity;

			[Statistics.Info("Terrain Damage", format: "{0:0.##}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Low)]
			[Save.Force] public float damage_terrain;

			[Statistics.Info("Primed Threshold", description: "Explosive becomes primed when its health drops under this value.", format: "{0:P2}", comparison: Statistics.Comparison.None, priority: Statistics.Priority.Low)]
			[Save.Force] public float health_threshold = 0.20f;

			[Save.NewLine]
			[Save.Force] public Damage.Type damage_type = Damage.Type.Explosion;
			[Save.Force] public Damage.Type damage_type_secondary = Damage.Type.Shockwave;
			[Save.Force] public Explosive.Flags flags;

			[Save.NewLine]
			public Chance detonate_chance;
			public BitField<Damage.Type> detonate_damage_filter = Explosive.Data.detonate_damage_filter_default;

			[Save.NewLine]
			public float shake_multiplier = 1.00f;
			public float force_multiplier = 1.00f;
			public float stun_multiplier = 1.00f;
			public float terrain_radius_multiplier = 1.00f;

			[Save.NewLine]
			public float smoke_amount = 1.00f;
			public float smoke_lifetime_multiplier = 1.00f;
			public float smoke_velocity_multiplier = 1.00f;
			public Color32BGRA smoke_color = new Color32BGRA(220, 180, 180, 180);

			[Save.NewLine]
			public float flash_duration_multiplier = 1.00f;
			public float flash_intensity_multiplier = 1.00f;

			[Save.NewLine]
			public float fire_amount = 1.00f;
			public float sparks_amount = 0.00f;

			[Save.NewLine]
			public float volume = 1.00f;
			public float pitch = 1.00f;

			[Save.NewLine]
			[Asset.Ignore] public float modifier = 1.00f;

			[Net.Ignore, Asset.Ignore, Obsolete] public Entity ent_owner;
		}

		[ISystem.Add(ISystem.Mode.Single, ISystem.Scope.Region)]
		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region, interval: 0.50f)]
		public static void UpdateHoldable([Source.Owned] in Explosive.Data explosive, [Source.Owned] ref Holdable.Data holdable)
		{
			holdable.hints.AddFlag(NPC.ItemHints.Dangerous | NPC.ItemHints.Explosive | NPC.ItemHints.Destructive);
		}

#if SERVER
		[ISystem.Event<EssenceNode.FailureEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnFailure(ISystem.Info info, Entity entity, ref XorRandom random, ref EssenceNode.FailureEvent data, [Source.Owned] ref Explosive.Data explosive)
		{
			if (random.NextBool(data.power * 0.20f))
			{
				explosive.flags |= Explosive.Flags.Primed | Explosive.Flags.Any_Damage;
				explosive.health_threshold = 0.99f;
				
				if (random.NextBool(0.50f))
				{
					explosive.flags |= Explosive.Flags.Explode_When_Primed;
				}

				explosive.Sync(entity, true);
			}
		}

		[ISystem.Event<Health.PostDamageEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnPostDamage(Entity entity, ref XorRandom random, ref Health.PostDamageEvent data, 
		[Source.Owned] ref Health.Data health, [Source.Owned] ref Explosive.Data explosive)
		{
			//App.WriteLine(health.integrity);
			if (health.integrity <= explosive.health_threshold && (health.integrity <= 0.00f || explosive.detonate_chance.Evaluate(random: ref random, ignore_if_zero: true)))
			{
				if (explosive.flags.HasAny(Explosive.Flags.Any_Damage) || (explosive.flags.HasAny(Explosive.Flags.Primed_When_Destroyed) && health.integrity <= 0.00f))
				{
					explosive.flags |= Explosive.Flags.Primed;
				}
				else
				{
					if (explosive.detonate_damage_filter.Has(data.damage.damage_type)) explosive.flags |= Explosive.Flags.Primed;
				}

				if (explosive.flags.HasAny(Explosive.Flags.Primed))
				{
					//if (!explosive.ent_owner.IsValid()) explosive.ent_owner = data.damage.ent_attacker;

					if (explosive.flags.TryRemoveFlag(Explosive.Flags.Explode_When_Primed))
					{
						entity.RemoveComponent<Explosive.Data>();
						health.integrity = 0.00f;
					}
				}
			}
		}

		[ISystem.Modified.Component<Resource.Data>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnResourceModified([Source.Owned] in Resource.Data resource, [Source.Owned] ref Explosive.Data explosive)
		{
			ref var material = ref resource.material.GetData();
			if (material.IsNotNull())
			{
				//explosive.modifier = Maths.Sqrt(resource.GetQuantityNormalized()).Clamp01();
				explosive.modifier = Maths.Cbrt(resource.GetQuantityNormalized().Clamp01());
			}
			//explosive.modifier = Maths.NormalizeClamp(resource.quantity, resource.material.GetDefinition().quantity_max);
			//explosive.modifier = MathF.Log2(resource.quantity / Maths.Max(resource.material.GetDefinition().quantity_max, 1.00f));
			//App.WriteLine(explosive.modifier);
		}

		[ISystem.RemoveLast(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnRemove(ref Region.Data region, Entity entity, 
		[Source.Owned] in Transform.Data transform, [Source.Owned] in Explosive.Data explosive)
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

				region.SpawnPrefab("explosion", transform.position).ContinueWith(ent =>
				{
					ref var explosion = ref ent.GetComponent<Explosion.Data>();
					if (explosion.IsNotNull())
					{
						var random = XorRandom.New(true);

						App.WriteValue(explosive_tmp.modifier);

						//explosion.radius = Maths.Max(3.00f, Maths.Lerp01(explosive_tmp.radius * 0.25f, explosive_tmp.radius, explosive_tmp.modifier) * random.NextFloatExtra(0.90f, 0.15f));
						//explosion.radius = Maths.Max(3.00f, (explosive_tmp.radius * explosive_tmp.modifier) * random.NextFloatExtra(0.90f, 0.15f));
						explosion.radius = Maths.Max(2.00f, (explosive_tmp.radius * explosive_tmp.modifier) * random.NextFloatExtra(0.90f, 0.15f));
						//explosion.power = Maths.Max(2.50f, Maths.Lerp01(explosive_tmp.power * 0.50f, explosive_tmp.power, explosive_tmp.modifier) * random.NextFloatExtra(0.90f, 0.15f));
						explosion.power = Maths.Max(1.50f, Maths.Lerp01(explosive_tmp.power * 0.50f, explosive_tmp.power, explosive_tmp.modifier) * random.NextFloatExtra(0.90f, 0.15f));

						explosion.damage_entity = explosive_tmp.damage_entity * explosive_tmp.modifier * random.NextFloatExtra(0.90f, 0.18f);
						//explosion.damage_terrain = explosive_tmp.damage_terrain * explosive_tmp.modifier * random.NextFloatExtra(0.90f, 0.17f);
						explosion.damage_terrain = Maths.Lerp01(explosive_tmp.damage_terrain * 0.50f, explosive_tmp.damage_terrain, explosive_tmp.modifier) * random.NextFloatExtra(0.90f, 0.17f);

						explosion.damage_type = explosive_tmp.damage_type;
						explosion.damage_type_secondary = explosive_tmp.damage_type_secondary;

						explosion.shake_multiplier = explosive_tmp.shake_multiplier;
						explosion.force_multiplier = explosive_tmp.force_multiplier;
						explosion.stun_multiplier = explosive_tmp.stun_multiplier;
						explosion.terrain_radius_multiplier = explosive_tmp.terrain_radius_multiplier;

						explosion.smoke_amount = explosive_tmp.smoke_amount;
						explosion.smoke_lifetime_multiplier = explosive_tmp.smoke_lifetime_multiplier;
						explosion.smoke_velocity_multiplier = explosive_tmp.smoke_velocity_multiplier;
						explosion.smoke_color = explosive_tmp.smoke_color;

						explosion.flash_duration_multiplier = explosive_tmp.flash_duration_multiplier;
						explosion.flash_intensity_multiplier = explosive_tmp.flash_intensity_multiplier;

						explosion.fire_amount = explosive_tmp.fire_amount;
						explosion.sparks_amount = explosive_tmp.sparks_amount;

						explosion.volume = explosive_tmp.volume;
						explosion.pitch = explosive_tmp.pitch;
						explosion.delay = random.NextFloatExtra(0.01f, 0.05f);

						explosion.ent_owner = explosive_tmp.ent_owner;
						if (explosive_tmp.flags.HasAny(Explosive.Flags.No_Self_Damage)) explosion.ent_ignored = entity;

						explosion.Sync(ent, true);
					}
				});
			}
		}
#endif
	}
}
