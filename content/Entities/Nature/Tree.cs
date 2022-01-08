
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

		[IComponent.Data(Net.SendType.Reliable)]
		public partial struct Data: IComponent
		{
			public Prefab.Handle prefab_stump;
			public float health_cut = 0.80f;

			public Texture.Handle sprite_cut;
			public Sound.Handle sound_cut = Tree.sound_tree_cut_default;

			public Tree.Flags flags;
		}

		public static readonly Sound.Handle sound_tree_cut_default = "tree_fall";

#if SERVER
		[ISystem.Modified<Split.Data>(ISystem.Mode.Single)]
		public static void OnSplit(ISystem.Info info, Entity entity, [Source.Owned] in Split.Data split, [Source.Owned] in Tree.Data tree, [Source.Owned, Trait.Of<Tree.Data>] in Foliage.Renderer.Data renderer)
		{
			if (split.rect_normalized.a.Y != 0.00f || split.rect_normalized.GetHeight() < 0.40f)
			{
				entity.RemoveTrait<Tree.Data, Foliage.Renderer.Data>();
			}
		}

		[ISystem.Modified<Health.Data>(ISystem.Mode.Single)]
		public static void OnModified(ISystem.Info info, Entity entity, [Source.Owned] in Health.Data health, [Source.Owned] ref Tree.Data tree, [Source.Owned] ref Body.Data body, [Source.Owned] in Transform.Data transform, [Source.Owned] ref Animated.Renderer.Data renderer)
		{
			if (!tree.flags.HasAll(Tree.Flags.Cut) && health.integrity < tree.health_cut)
			{
				tree.flags |= Tree.Flags.Cut;
				renderer.sprite = tree.sprite_cut;

				ref var region = ref info.GetRegion();

				Sound.Play(ref region, tree.sound_cut, transform.position);
				Shake.Emit(ref region, transform.position, 0.40f, 0.40f);

				var random = XorRandom.New();
				Experience.Add(health.last_damage_owner, Experience.Type.Woodcutting, (int)random.NextFloatRange(100, 150));

				body.type = Body.Type.Dynamic;
				body.flags &= ~Body.Flags.NonDirty;

				region.SpawnPrefab(tree.prefab_stump, transform.position, transform.rotation, transform.scale);

				//entity.MarkModified<Body.Data>();

				//entity.RemoveTrait<Tree.Data, Foliage.Renderer.Data>();

				entity.SyncComponent<Animated.Renderer.Data>(ref renderer);
				entity.SyncComponent<Body.Data>(ref body);
				entity.SyncComponent<Tree.Data>(ref tree);
			}
		}
#endif
	}
}
