
namespace TC2.Base.Components
{
	public static partial class ThrowingDamage
	{
		[IComponent.Data(Net.SendType.Reliable)]
		public partial struct Data: IComponent
		{
			public float damage = default;

			[Save.Ignore, Net.Ignore] public float next_hit = default;
			[Save.Ignore, Net.Ignore] public float last_attach = default;

			public Data()
			{

			}
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single)]
		public static void OnUpdate(ISystem.Info info, Entity entity, [Source.Owned] ref ThrowingDamage.Data throwing, [Source.Owned] ref Body.Data body, 
		[Source.Owned] in Transform.Data transform, [Source.Owned, Pair.Of<Body.Data>] ref Shape.Line shape)
		{
			var ts = Timestamp.Now();

#if SERVER
			var hit = false;
			var can_hit = info.WorldTime >= throwing.next_hit;

			var ent_holder = entity.GetParent(Relation.Type.Child);

			if (ent_holder.IsAlive())
			{
				//App.WriteLine("attach");
				throwing.last_attach = info.WorldTime;

				shape.mask.SetFlag(Physics.Layer.Creature, false);
				shape.mask.SetFlag(Physics.Layer.Solid, false);
				body.Rebuild();
				shape.Sync<Shape.Line, Body.Data>(entity);
			}
			else
			{
				var time = info.WorldTime - throwing.last_attach;
				
				if(time < 0.20f)
				{

				}
				else if (time < 0.25f)
				{
					App.WriteLine("drop");

					shape.mask.SetFlag(Physics.Layer.Creature, true);
					shape.mask.SetFlag(Physics.Layer.Solid, true);
					body.Rebuild();
					shape.Sync<Shape.Line, Body.Data>(entity);
				}
				else if(shape.mask.HasFlag(Physics.Layer.Creature))
				{

					var damagetype = Damage.Type.Blunt;
					
					ref var melee = ref entity.GetComponent<Melee.Data>(); //Copies melee damage type if the item has one
					if(!melee.IsNull())
					{
						damagetype = melee.damage_type;
					}

					foreach (var arbiter in body.GetArbiters())
					{
						var ent_hit = arbiter.GetEntity();
						
						if (can_hit)
						{
							var material_type = arbiter.GetMaterial();
							var vel = arbiter.GetVelocity() - body.GetVelocity();
							var vel_sq = vel.LengthSquared();
							hit = true;
							App.WriteLine("speed" + vel + " " + vel_sq);
							if (ent_hit.IsValid() && vel_sq > 2.00f)
							{
								entity.Hit(ent_hit, ent_hit, arbiter.GetPosition(), -arbiter.GetNormal(), arbiter.GetNormal(), 
								damage: throwing.damage * vel_sq * 0.1f, target_material_type: material_type, damage_type: damagetype);
							}
						}
					}
				}
			}

			if (hit)
			{
				App.WriteLine("stop");
				throwing.next_hit = info.WorldTime + 0.40f;
				throwing.Sync<ThrowingDamage.Data>(entity);

				shape.mask.SetFlag(Physics.Layer.Creature, false);
				shape.mask.SetFlag(Physics.Layer.Solid, false);
				body.Rebuild();
				shape.Sync<Shape.Line, Body.Data>(entity);

				
			}
#endif
		}
	}
}
