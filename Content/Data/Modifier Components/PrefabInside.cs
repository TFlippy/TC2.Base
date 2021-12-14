
namespace TC2.Base.Components
{
	//This component simply creates a prefab when the entity is reduced to 0 (or less) hp
	public static partial class PrefabInside
	{

		[IComponent.Data(Net.SendType.Reliable)]
		public partial struct Data: IComponent
		{
			public Prefab.Handle prefab_release;
		}

#if SERVER
		[ISystem.Modified(ISystem.Mode.Single, 0)]
		public static void OnHealthModified(ISystem.Info info, Entity entity, [Source.Owned] in Health.Data health, [Source.Owned] ref Data prefabInside)
		{
			if (health.primary <= 0.00f)
			{
				entity.RemoveComponent<Data>(); //Can only trigger once
			}
		}

		[ISystem.Remove(ISystem.Mode.Single, 0)]
		public static void OnRemove(ISystem.Info info, [Source.Owned] in Transform.Data transform, [Source.Owned] in Data prefabInside)
		{
			info.GetRegion().SpawnPrefab(prefabInside.prefab_release, transform.position);
		}
#endif
	}
}
