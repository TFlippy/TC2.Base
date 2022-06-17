using TC2.Base.Components;

namespace TC2.Base
{
	public sealed partial class ModInstance
	{
		private static void RegisterHoldableAugments(ref List<Augment.Definition> definitions)
		{
			definitions.Add(Augment.Definition.New<Holdable.Data>
			(
				identifier: "holdable.sneaky",
				category: "Utility",
				name: "Sneaky",
				description: "Hold this item BEHIND your body to make it less obvious",

				can_add: static (ref Augment.Context context, in Holdable.Data data, ref Augment.Handle handle, Span<Augment.Handle> Augments) =>
				{
					ref var renderer = ref context.GetComponent<Animated.Renderer.Data>();
					return !Augments.HasAugment(handle) && !renderer.IsNull();
				},

				apply_1: static (ref Augment.Context context, ref Holdable.Data data, ref Augment.Handle handle, Span<Augment.Handle> Augments) =>
				{
					ref var renderer = ref context.GetComponent<Animated.Renderer.Data>();
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

					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Work)
						{
							requirement.difficulty += 1.00f;
							requirement.amount *= 1.20f;
						}
					}
				}
			));

			definitions.Add(Augment.Definition.New<Holdable.Data>
			(
				identifier: "holdable.rubber_grip",
				category: "Utility",
				name: "Rubber Grip",
				description: "Get a better grip on the item by adding a rubber grip",

				can_add: static (ref Augment.Context context, in Holdable.Data data, ref Augment.Handle handle, Span<Augment.Handle> Augments) =>
				{
					return !Augments.HasAugment(handle);
				},

				apply_1: static (ref Augment.Context context, ref Holdable.Data data, ref Augment.Handle handle, Span<Augment.Handle> Augments) =>
				{
					data.force_multiplier *= 1.5f;
					data.torque_multiplier *= 1.5f;

					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Work)
						{
							requirement.amount *= 1.10f;
						}
					}

					context.requirements_new.Add(Crafting.Requirement.Resource("rubber", 5.00f));
				}
			));

			definitions.Add(Augment.Definition.New<Holdable.Data>
			(
				identifier: "holdable.lubricated_grip",
				category: "Utility",
				name: "Slippery Grip",
				description: "Lubricate any grippable places of the object making it nearly unholdable",

				can_add: static (ref Augment.Context context, in Holdable.Data data, ref Augment.Handle handle, Span<Augment.Handle> Augments) =>
				{
					return !Augments.HasAugment(handle);
				},

				apply_1: static (ref Augment.Context context, ref Holdable.Data data, ref Augment.Handle handle, Span<Augment.Handle> Augments) =>
				{
					data.force_multiplier *= 0.01f;
					data.torque_multiplier *= 0.01f;

					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Work)
						{
							requirement.amount *= 1.10f;
						}
					}

					context.requirements_new.Add(Crafting.Requirement.Resource("lubricant", 5.00f));
				}
			));
		}
	}
}

