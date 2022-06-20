
namespace TC2.Base.Components
{
	public static partial class Consumable
	{
		[Flags]
		public enum Flags: uint
		{
			None = 0,

			Enable_Use_On_Self = 1 << 0,
			Enable_Use_On_Others = 1 << 1
		}

		[IComponent.Data(Net.SendType.Reliable)]
		public partial struct Data: IComponent
		{
			public Consumable.Flags flags;

			public Sound.Handle sound_use;

			[Statistics.Info("Release Rate", description: "TODO: Desc", format: "{0:0.##} ml/s", comparison: Statistics.Comparison.None, priority: Statistics.Priority.Medium)]
			public float release_rate;

			[Statistics.Info("Release Step", description: "TODO: Desc", format: "{0:0.##} ml/s", comparison: Statistics.Comparison.None, priority: Statistics.Priority.Medium)]
			public float release_step;
		}

		[IEvent.Data]
		public partial struct ConsumeEvent: IEvent
		{
			public Entity ent_organic = default;
			public Entity ent_holder = default;
			public Entity ent_consumable = default;
			public Vector2 world_position = default;

			public ConsumeEvent()
			{

			}
		}

#if SERVER
		[ISystem.VeryLateUpdate(ISystem.Mode.Single), HasTag("dead", false, Source.Modifier.Parent)]
		public static void Update(ISystem.Info info, Entity entity,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Consumable.Data consumable, [Source.Parent] in Control.Data control, [Source.Parent, Override] in Organic.Data organic)
		{
			if (consumable.flags.HasAny(Consumable.Flags.Enable_Use_On_Others) && control.mouse.GetKeyDown(Mouse.Key.Left))
			{
				if (Vector2.DistanceSquared(transform.position, control.mouse.position) < (3 * 3))
				{
					ref var region = ref info.GetRegion();

					var ent_holder = entity.GetParent(Relation.Type.Child);
					if (ent_holder.IsAlive())
					{
						Span<OverlapResult> results = stackalloc OverlapResult[8];
						if (region.TryOverlapPointAll(control.mouse.position, 0.01f, ref results, mask: Physics.Layer.Organic | Physics.Layer.Creature))
						{
							ref var result_nearest = ref results.GetNearest();
							if (region.IsInLineOfSight(transform.position, result_nearest.world_position, 0.125f, mask: Physics.Layer.World | Physics.Layer.Solid, exclude: Physics.Layer.Ignore_Bullet | Physics.Layer.Ignore_Melee | Physics.Layer.Dynamic, query_flags: Physics.QueryFlag.Static))
							{
								var ent_target = result_nearest.entity;

								var oc_organic = ent_target.GetComponentWithOwner<Organic.Data>(Relation.Type.Instance);
								if (oc_organic.IsValid())
								{
									Sound.Play(ref region, consumable.sound_use, result_nearest.world_position);

									var data = new Consumable.ConsumeEvent();
									data.ent_organic = oc_organic.Entity;
									data.ent_holder = ent_holder;
									data.ent_consumable = entity;
									data.world_position = result_nearest.world_position;

									entity.Notify(ref data);

									entity.Delete();
								}
							}
						}
					}
				}
			}
			else if (consumable.flags.HasAny(Consumable.Flags.Enable_Use_On_Self) && control.mouse.GetKeyDown(Mouse.Key.Right))
			{
				ref var region = ref info.GetRegion();

				var ent_holder = entity.GetParent(Relation.Type.Child);
				if (ent_holder.IsAlive())
				{
					var oc_organic = ent_holder.GetComponentWithOwner<Organic.Data>(Relation.Type.Instance);
					if (oc_organic.IsValid())
					{
						Sound.Play(ref region, consumable.sound_use, transform.position);

						var data = new Consumable.ConsumeEvent();
						data.ent_organic = oc_organic.Entity;
						data.ent_holder = ent_holder;
						data.ent_consumable = entity;
						data.world_position = transform.position;

						entity.Notify(ref data);

						entity.Delete();
					}
				}
			}
		}
#endif
	}
}