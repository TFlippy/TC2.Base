﻿
namespace TC2.Base.Components
{
	public static partial class Tree
	{
		[Flags]
		public enum Flags: uint
		{
			None = 0,

			Cut = 1 << 0
		}

		[IComponent.Data(Net.SendType.Reliable, region_only: true)]
		public partial struct Data(): IComponent
		{
			//public Prefab.Handle prefab_stump;
			//public float health_cut = 0.80f;

			//public Texture.Handle sprite_cut;
			public Sound.Handle sound_cut = Tree.sound_tree_cut_default;

			public Tree.Flags flags;
		}

		public static readonly Sound.Handle sound_tree_cut_default = "tree_fall";

#if SERVER
		[ISystem.Modified.Component<Split.Data>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnSplit(ISystem.Info info, Entity entity, 
		[Source.Owned] in Split.Data split, [Source.Owned] in Tree.Data tree, [Source.Owned, Pair.Component<Tree.Data>] in Foliage.Renderer.Data renderer)
		{
			if (split.rect_normalized.a.Y != 0.00f || split.rect_normalized.GetHeight() < 0.40f)
			{
				entity.RemoveTrait<Tree.Data, Foliage.Renderer.Data>();
			}
		}

		//[ISystem.Modified<Health.Data>(ISystem.Mode.Single, ISystem.Scope.Region)]
		//public static void OnModified(ISystem.Info info, Entity entity, ref XorRandom random, ref Region.Data region, 
		//[Source.Owned] in Health.Data health, [Source.Owned] ref Tree.Data tree, [Source.Owned] ref Body.Data body, [Source.Owned] in Transform.Data transform, [Source.Owned] ref Animated.Renderer.Data renderer)
		//{
		//	if (health.integrity < tree.health_cut && tree.flags.TryAddFlag(Tree.Flags.Cut))
		//	{
		//		renderer.sprite = tree.sprite_cut;

		//		Sound.Play(ref region, tree.sound_cut, transform.position);
		//		Shake.Emit(ref region, transform.position, 0.40f, 0.40f);

		//		//Experience.Add(health.last_damage_owner, Experience.Type.Woodcutting, (int)random.NextFloatRange(100, 150));

		//		body.type = Body.Type.Dynamic;
		//		body.MarkDirty();

		//		region.SpawnPrefab(tree.prefab_stump, transform.position, transform.rotation, transform.scale);

		//		//entity.MarkModified<Body.Data>();

		//		//entity.RemoveTrait<Tree.Data, Foliage.Renderer.Data>();

		//		renderer.Sync(entity, true);
		//		body.Sync(entity, true);
		//		tree.Sync(entity, true);
		//	}
		//}
#endif
	}
}
