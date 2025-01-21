
namespace TC2.Base.Components
{
	public static partial class Wettable
	{
		[Flags]
		public enum Flags: uint
		{
			None = 0,

			Wet = 1u << 0,
			In_Water = 1u << 1,
			Damage_If_Wet = 1u << 2,
		}

		[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Region)]
		public partial struct Data(): IComponent
		{
			public Wettable.Flags flags;

			public float absorption;
			public float wetness;

			public float damage_multiplier = 1.00f;
			public float damage_interval = 0.50f;
			
			[Net.Ignore, Save.Ignore] public float t_last_water;
			[Net.Ignore, Save.Ignore] public float t_next_damage;
		}

		[ISystem.Monitor(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("initialized", true, Source.Modifier.Owned)]
		public static void OnAddRemove(ISystem.Info info, [Source.Owned] ref Body.Data body, [Source.Owned] ref Wettable.Data wettable)
		{
			switch (info.EventType)
			{
				case ISystem.EventType.Add:
				{
					body.override_shape_layer.AddFlag(Physics.Layer.Wettable);
					body.MarkDirty();
				}
				break;

				case ISystem.EventType.Remove:
				{
					body.override_shape_layer.RemoveFlag(Physics.Layer.Wettable);
					body.MarkDirty();
				}
				break;
			}
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity,
		[Source.Owned] ref Body.Data body, [Source.Owned] ref Wettable.Data wettable)
		{
			var is_in_water = false;
			var dt = info.DeltaTime;
			var time = info.WorldTime;

			var ent_water = Entity.None;

			if (body.HasArbiters())
			{
				foreach (var arbiter in body.GetArbiters())
				{
					var layer = arbiter.GetLayer();
					if (layer.HasAny(Physics.Layer.Water))
					{
						ent_water = arbiter.GetEntity();
						is_in_water = true;
						wettable.t_last_water = time;

#if SERVER
						if (time >= wettable.t_next_damage)
						{
							wettable.t_next_damage = time + random.NextFloatExtra(wettable.damage_interval, wettable.damage_interval * wettable.wetness.Inv());

							var damage = wettable.wetness * wettable.damage_multiplier;
							if (damage >= 10.00f)
							{
								Damage.Hit(ent_attacker: ent_water,
									ent_owner: default,
									ent_target: entity,
									position: body.GetPosition(),
									velocity: arbiter.GetRelativeVelocity(),
									normal: arbiter.GetNormal(),
									damage_integrity: damage,
									damage_durability: damage,
									damage_terrain: damage,
									target_material_type: body.GetMaterial(),
									damage_type: Damage.Type.Wet);

								//App.WriteLine($"wet damage; {damage}");
							}
						}
#endif

						break;
					}
				}
			}

			if (is_in_water) // wettable.flags.HasAny(Wettable.Flags.Wet))
			{
				wettable.wetness.AddTowards(1.00f, dt * wettable.absorption);

			}
			else
			{
				wettable.wetness *= 0.99f;
			}
		}
	}
}