﻿using TC2.Base.Components;

namespace TC2.Base
{
	public sealed partial class ModInstance
	{
		private static void RegisterHoldableModifications(ref List<Modification.Definition> definitions)
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
					ref var renderer = ref context.GetComponent<Sprite.Renderer.Data>();
					if (!renderer.IsNull())
					{
						renderer.z = -140.00f;
					}

					ref var gun = ref context.GetComponent<Gun.Data>();
					if (!gun.IsNull())
					{
						gun.damage_multiplier *= 0.80f;
					}

					ref var melee = ref context.GetComponent<Melee.Data>();
					if (!melee.IsNull())
					{
						melee.damage_base *= 0.80f;
					}
				}
			));
		}
	}
}

