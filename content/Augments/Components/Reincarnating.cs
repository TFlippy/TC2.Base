
namespace TC2.Base.Components
{
	// This component simply spawns 1 or more copies of the prefab when it dies, except the copies have reincarnating count set to 0 so this doesnt repeat
	public static partial class Reincarnating
	{
		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct Data: IComponent
		{
			[Statistics.Info("Quantity", description: "How many copies of this are created on death", format: "{0:0}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.High)]
			public int count;

			public Data()
			{

			}
		}

#if SERVER
		[ISystem.Event<Health.PostDamageEvent>(ISystem.Mode.Single)]
		public static void OnPostDamage(ISystem.Info info, Entity entity, ref Health.PostDamageEvent data, [Source.Owned] ref Health.Data health, [Source.Owned] ref Data reincarnating)
		{
			if (health.integrity <= 0.00f)
			{
				entity.RemoveComponent<Reincarnating.Data>();
			}
		}

		[ISystem.RemoveLast(ISystem.Mode.Single)]
		public static void OnRemove(ISystem.Info info, Entity entity, [Source.Owned] in Transform.Data transform, [Source.Owned] in Data reincarnating)
		{
			if (reincarnating.count > 0)
			{
				ref var region = ref info.GetRegion();
				var random = XorRandom.New();

				if (entity.TryGetPrefabHandle(out var prefab_handle))
				{
					for (var i = 0; i < reincarnating.count; i++)
					{
						region.SpawnPrefab(prefab_handle, transform.position + random.NextUnitVector2Range(0.00f, 1.00f)).ContinueWith(static (ent) =>
						{
							ref var component = ref ent.GetComponent<Reincarnating.Data>();
							if (!component.IsNull())
							{
								component.count = 0;
								component.Sync(ent);
							}
							ref var fuse = ref ent.GetComponent<Fuse.Data>();
							if (!fuse.IsNull())
							{
								ent.SetTag("lit", enabled: true);
								ref var expl = ref ent.GetComponent<Explosive.Data>();
								expl.flags |= Explosive.Flags.Primed;
							}
							ref var dis = ref ent.GetComponent<Dismantlable.Data>(); //No double dismantelling
							if (!dis.IsNull())
							{
								dis.yield = -1;
							}
							//ref var body = ref ent.GetComponent<Body.Data>();
							//if (!body.IsNull())
							//{
							//	var random = XorRandom.New();
							//	Vector2 force = new Vector2(random.NextFloatRange(10, 10000), random.NextFloatRange(10, 10000));
							//	body.AddForce(force);
							//}
						});
					}
				}
			}
		}
#endif
	}
}
