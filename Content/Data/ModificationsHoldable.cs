using TC2.Base.Components;

namespace TC2.Base
{
	public sealed partial class ModInstance
	{
		private static void RegisterSpriteModifications(ref List<Modification.Definition> definitions)
		{
			definitions.Add(Modification.Definition.New<Holdable.Data>
			(
				identifier: "holdable.sneaky",
				category: "Holdable",
				name: "Sneaky",
				description: "Hold this item BEHIND your body to make it less obvious",

				can_add: static (ref Modification.Context context, in Holdable.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var renderer = ref context.GetComponent<Sprite.Renderer.Data>();
					return !modifications.HasModification(handle) && !renderer.IsNull();
				},

				apply_1: static (ref Modification.Context context, ref Holdable.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.pain -= 200.00f;
					context.requirements_new.Add(Crafting.Requirement.Resource("red_sugar", 10));
				}
			));
		}
	}
}

