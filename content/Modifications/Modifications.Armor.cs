using TC2.Base.Components;

namespace TC2.Base
{
	public sealed partial class ModInstance
	{
		private static void RegisterArmorModifications(ref List<Modification.Definition> definitions)
		{
			definitions.Add(Modification.Definition.New<Health.Data>
			(
				identifier: "health.smirgl_structure",
				category: "Protection",
				name: "Smirgl-Reinforced Structure",
				description: "Replaces entire structure with smirgl, greatly increasing durability.",

				can_add: static (ref Modification.Context context, in Health.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					var has_ingot = false;
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetDefinition();
							if (material.flags.HasAll(Material.Flags.Ingot))
							{
								has_ingot = true;
								break;
							}
						}
					}

					return has_ingot && !modifications.HasModification(handle);
				},

				apply_1: static (ref Modification.Context context, ref Health.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					var ingot_amount = 0.00f;
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Work)
						{
							requirement.amount *= 1.50f;
							requirement.difficulty += 5.00f;
						}
						else if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetDefinition();
							if (material.flags.HasAll(Material.Flags.Ingot))
							{
								ingot_amount += requirement.amount;
								requirement = default;
							}
						}
					}

					var total_amount = 3.00f + (ingot_amount * 0.30f);
					context.requirements_new.Add(Crafting.Requirement.Resource("smirgl_ingot", total_amount));

					data.max += total_amount * 1000.00f;
					data.armor *= 1.35f;
					data.armor += 200.00f;

					ref var body = ref context.GetComponent<Body.Data>();
					if (!body.IsNull())
					{
						ref var material = ref Material.GetMaterial("smirgl_ingot");
						body.mass_extra += total_amount * material.mass_per_unit * 0.70f;
					}
				}
			));

			definitions.Add(Modification.Definition.New<Armor.Data>
			(
				identifier: "armor.chitin_lined",
				category: "Protection",
				name: "Chitin-Lined",
				description: "Incorporate chitin lining into the armor, greatly improving its blocking capacity, while making it slightly more brittle.",

				can_add: static (ref Modification.Context context, in Armor.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return !modifications.HasModification(handle);
				},

				apply_0: static (ref Modification.Context context, ref Armor.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.threshold = Maths.Clamp(data.threshold - 0.25f, 0.00f, 1.00f);

					ref var health = ref context.GetComponent<Health.Data>();
					if (!health.IsNull())
					{
						health.max *= 0.95f;
					}
				},

				apply_1: static (ref Modification.Context context, ref Armor.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					context.requirements_new.Add(Crafting.Requirement.Resource("chitin", 10.00f));
				}
			));
		}
	}
}

