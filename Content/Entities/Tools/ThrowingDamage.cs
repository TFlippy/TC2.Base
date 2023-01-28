
namespace TC2.Base.Components
{
	public static partial class ThrowingDamage
	{
		[IComponent.Data(Net.SendType.Reliable, name: "Throwing Damage")]
		public partial struct Data: IComponent
		{
			[Statistics.Info("Added Damage", description: "Added damage when throwing the object, multiplied by velocity.", format: "{0:0}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.High)]
			public float damage = default;

			[Statistics.Info("Base Damage Type", description: "Type of damage dealt.", format: "{0}", comparison: Statistics.Comparison.None, priority: Statistics.Priority.Low)]
			public Damage.Type damage_type = Damage.Type.Blunt;

			[Statistics.Info("Includes Melee", description: "Whether or not throwing damage includes any pre existing melee damage and damage type.", format: "{0}", comparison: Statistics.Comparison.None, priority: Statistics.Priority.Low)]
			public bool overrideWithMelee = true; 

			[Save.Ignore, Net.Ignore] public float next_hit = default;
			[Save.Ignore, Net.Ignore] public float last_attach = default;

			public bool collision = false;

			public Data()
			{

			}
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single)]
		public static void OnUpdate(ISystem.Info info, Entity entity, [Source.Owned] ref ThrowingDamage.Data throwing, [Source.Owned] ref Body.Data body, 
		[Source.Owned] in Transform.Data transform)
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
				throwing.collision = false;
			}
			else
			{
				var time = info.WorldTime - throwing.last_attach;
				if(time < 0.20f)
				{

				}
				else if (time < 0.25f)
				{
					//App.WriteLine("drop");
					throwing.collision = true;
				}
				else if(throwing.collision)
				{
					var damagetype = throwing.damage_type;
					var basedamage = throwing.damage * 0.1f;

					ref var melee = ref entity.GetComponent<Melee.Data>(); //Copies melee damage type if the item has one
					if(!melee.IsNull() && throwing.overrideWithMelee)
					{
						damagetype = melee.damage_type;
						var random = XorRandom.New();
						basedamage = basedamage + melee.damage_base * 0.1f + random.NextFloatRange(0.00f, melee.damage_bonus) * 0.1f;
						//This is multiplied by velocity so the 0.1 multiplier mitigates this and encourages high speed throwing attacks

					}

					foreach (var arbiter in body.GetArbiters())
					{
						var ent_hit = arbiter.GetEntity();
						
						if (can_hit)
						{
							var material_type = arbiter.GetMaterial();
							var vel = arbiter.GetVelocity() - body.GetVelocity();
							var vel_sq = vel.Length();
							hit = true;
							//App.WriteLine("speed" + vel + " " + vel_sq);
							if (ent_hit.IsValid() && vel_sq > 1.00f)
							{
								entity.Hit(ent_hit, ent_hit, arbiter.GetPosition(), -arbiter.GetNormal(), arbiter.GetNormal(), 
								damage: basedamage * vel_sq, target_material_type: material_type, damage_type: damagetype);
							}
						}
					}
				}
			}

			if (hit)
			{
				//App.WriteLine("stop");
				throwing.next_hit = info.WorldTime + 0.40f;
				throwing.collision = false;
			}
#endif
		}


		[ISystem.LateUpdate(ISystem.Mode.Single)]
		public static void OnLateUpdateLine(ISystem.Info info, Entity entity, [Source.Owned] ref ThrowingDamage.Data throwing, [Source.Owned] ref Body.Data body, 
		[Source.Owned, Pair.Of<Body.Data>] ref Shape.Line shape)
		{
			bool mask = shape.mask.HasFlag(Physics.Layer.Solid);
			if(throwing.collision != mask)
			{
				shape.mask.SetFlag(Physics.Layer.Creature, throwing.collision);
				shape.mask.SetFlag(Physics.Layer.Solid, throwing.collision);
				body.Rebuild();
				//shape.Sync<Shape.Line, Body.Data>(entity);
			}
		}

		[ISystem.LateUpdate(ISystem.Mode.Single)]
		public static void OnLateUpdateBox(ISystem.Info info, Entity entity, [Source.Owned] ref ThrowingDamage.Data throwing, [Source.Owned] ref Body.Data body, 
		[Source.Owned, Pair.Of<Body.Data>] ref Shape.Box shape)
		{
			bool mask = shape.mask.HasFlag(Physics.Layer.Solid);
			if(throwing.collision != mask)
			{
				shape.mask.SetFlag(Physics.Layer.Creature, throwing.collision);
				shape.mask.SetFlag(Physics.Layer.Solid, throwing.collision);
				body.Rebuild();
				//shape.Sync<Shape.Box, Body.Data>(entity);
			}
		}

		[ISystem.LateUpdate(ISystem.Mode.Single)]
		public static void OnLateUpdateCircle(ISystem.Info info, Entity entity, [Source.Owned] ref ThrowingDamage.Data throwing, [Source.Owned] ref Body.Data body, 
		[Source.Owned, Pair.Of<Body.Data>] ref Shape.Circle shape)
		{
			bool mask = shape.mask.HasFlag(Physics.Layer.Solid);
			if(throwing.collision != mask)
			{
				shape.mask.SetFlag(Physics.Layer.Creature, throwing.collision);
				shape.mask.SetFlag(Physics.Layer.Solid, throwing.collision);
				body.Rebuild();
				//shape.Sync<Shape.Circle, Body.Data>(entity);
			}
		}
	}
}
