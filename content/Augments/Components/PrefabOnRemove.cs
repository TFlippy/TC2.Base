﻿
namespace TC2.Base.Components
{
	// This component simply spawns a prefab when the entity is reduced to 0 (or less) health
	public static partial class PrefabOnRemove
	{
		[IComponent.Data(Net.SendType.Reliable)]
		public partial struct Data: IComponent
		{
			public Prefab.Handle prefab;
			public int count;
		}

#if SERVER
		[ISystem.Modified.Component<Health.Data>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnHealthModified(ISystem.Info info, Entity entity, [Source.Owned] in Health.Data health, [Source.Owned] in Data prefab_on_remove)
		{
			if (health.integrity <= 0.00f)
			{
				entity.RemoveComponent<PrefabOnRemove.Data>(); // Can only trigger once
			}
		}

		[ISystem.Remove(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnRemove(ISystem.Info info, ref XorRandom random, ref Region.Data region, [Source.Owned] in Transform.Data transform, [Source.Owned] in Data prefab_on_remove)
		{
			if (prefab_on_remove.prefab.id != 0)
			{
				for (var i = 0; i < prefab_on_remove.count; i++)
				{
					region.SpawnPrefab(prefab_on_remove.prefab, transform.position + random.NextUnitVector2Range(0.00f, 1.00f));
				}
			}
		}
#endif
	}
}
