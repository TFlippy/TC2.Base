
using Keg.Engine;

namespace TC2.Base.Components
{
	public static partial class Shrapnel
	{
		[IComponent.Data(Net.SendType.Reliable)]
		public partial struct Data: IComponent
		{
			public Prefab.Handle prefab;
			public float spread = 0.00f;

			public float damage_modifier = 1.00f;

			public float speed_modifier_min = 0.30f;
			public float speed_modifier_max = 1.30f;

			public int count;
		}

#if SERVER
		[ISystem.Remove(ISystem.Mode.Single)]
		public static void OnRemove(ISystem.Info info, [Source.Owned] in Transform.Data transform, [Source.Owned] in Shrapnel.Data shrapnel, [Source.Owned] in Projectile.Data projectile)
		{
			if (shrapnel.count > 0 && shrapnel.prefab.id != 0)
			{
				ref var region = ref info.GetRegion();
				var random = XorRandom.New();

				for (var i = 0; i < shrapnel.count; i++)
				{
					var projectile_init =
					(
						damage_mult: shrapnel.damage_modifier,
						vel: projectile.velocity.RotateByDeg(random.NextFloat(shrapnel.spread)) * random.NextFloatRange(shrapnel.speed_modifier_min, shrapnel.speed_modifier_max),
						owner: projectile.ent_owner,
						faction_id: projectile.faction_id
					);

					region.SpawnPrefab(shrapnel.prefab, transform.position).ContinueWith(ent =>
					{
						ref var projectile = ref ent.GetComponent<Projectile.Data>();
						if (!projectile.IsNull())
						{
							projectile.damage_base *= projectile_init.damage_mult;
							projectile.damage_bonus *= projectile_init.damage_mult;
							projectile.velocity = projectile_init.vel;
							projectile.ent_owner = projectile_init.owner;
							projectile.faction_id = projectile_init.faction_id;

							ent.SyncComponent(ref projectile);
						}

						ref var explosive = ref ent.GetComponent<Explosive.Data>();
						if (!explosive.IsNull())
						{
							explosive.owner_entity = projectile_init.owner;
							ent.SyncComponent(ref explosive);
						}
					});
				}
			}
		}
#endif
	}
}
